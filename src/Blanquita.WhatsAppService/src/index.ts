import express from 'express';
import makeWASocket, {
    DisconnectReason,
    useMultiFileAuthState,
    WASocket,
    fetchLatestBaileysVersion
} from '@whiskeysockets/baileys';
import { Boom } from '@hapi/boom';
import qrcode from 'qrcode';
import path from 'path';
import fs from 'fs';
import pino from 'pino';

import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const serviceRoot = path.join(__dirname, '..');
const authFolder = path.join(serviceRoot, 'auth_info');

// Cargar variables desde .env (sin dependencias externas). Las variables ya definidas en el entorno tienen prioridad.
function loadEnvFile(filePath: string) {
    if (!fs.existsSync(filePath)) return;
    const lines = fs.readFileSync(filePath, 'utf8').split(/\r?\n/);
    for (const rawLine of lines) {
        const line = rawLine.trim();
        if (!line || line.startsWith('#')) continue;
        const separator = line.indexOf('=');
        if (separator <= 0) continue;
        const key = line.substring(0, separator).trim();
        let value = line.substring(separator + 1).trim();
        if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) {
            value = value.substring(1, value.length - 1);
        }
        if (process.env[key] === undefined) {
            process.env[key] = value;
        }
    }
}

loadEnvFile(path.join(serviceRoot, '.env'));

const app = express();
const PORT = process.env.PORT || 3001;
const API_KEY = process.env.WHATSAPP_API_KEY;
// Los documentos viajan en Base64 (~33% más grandes que el archivo original)
const JSON_LIMIT = process.env.WHATSAPP_JSON_LIMIT || '25mb';

if (!API_KEY) {
    console.error('[CONFIG] WHATSAPP_API_KEY no está definida. Todas las peticiones serán rechazadas hasta configurarla.');
}

app.use(express.json({ limit: JSON_LIMIT }));

// Autenticación obligatoria mediante cabecera X-API-Key
app.use((req, res, next) => {
    if (!API_KEY) {
        return res.status(503).json({ error: 'Service not configured. WHATSAPP_API_KEY is missing.' });
    }
    if (req.headers['x-api-key'] !== API_KEY) {
        console.warn(`[UNAUTHORIZED] Access blocked for ${req.method} ${req.path} from IP: ${req.ip}`);
        return res.status(401).json({ error: 'Unauthorized. Invalid or missing X-API-Key.' });
    }
    next();
});

// Registro de peticiones sin cabeceras ni cuerpo (contienen la API Key, teléfonos y documentos)
app.use((req, res, next) => {
    const start = Date.now();
    res.on('finish', () => {
        const duration = Date.now() - start;
        console.log(`[REQUEST] ${req.method} ${req.path} - Status: ${res.statusCode} - Duration: ${duration}ms`);
    });
    next();
});

let sock: WASocket | null = null;
let qrCodeData: string | null = null;
let connectionStatus: 'CONNECTED' | 'CONNECTING' | 'DISCONNECTED' = 'DISCONNECTED';
let reconnectAttempts = 0;
let reconnectTimer: NodeJS.Timeout | null = null;
let isConnecting = false;

function maskNumber(number: string): string {
    const digits = String(number).replace(/\D/g, '');
    return digits.length > 4 ? `***${digits.slice(-4)}` : '***';
}

function scheduleReconnect(delay: number, resetAttempts = false) {
    // Garantiza un único intento de reconexión pendiente
    if (reconnectTimer) {
        clearTimeout(reconnectTimer);
    }
    reconnectTimer = setTimeout(() => {
        reconnectTimer = null;
        if (resetAttempts) reconnectAttempts = 0;
        connectToWhatsApp();
    }, delay);
}

function closeCurrentSocket() {
    if (!sock) return;
    try {
        sock.ev.removeAllListeners('connection.update');
        sock.ev.removeAllListeners('creds.update');
        sock.end(undefined);
    } catch (err) {
        console.warn('Error closing previous WhatsApp socket:', err);
    }
    sock = null;
}

async function connectToWhatsApp() {
    if (isConnecting) {
        console.log('Connection attempt already in progress, skipping duplicate request.');
        return;
    }
    isConnecting = true;

    try {
        closeCurrentSocket();

        const { state, saveCreds } = await useMultiFileAuthState(authFolder);

        // Fetch the latest WhatsApp Web version dynamically to bypass the 405 error
        let version: [number, number, number] = [2, 3000, 1017585084]; // Last known stable fallback
        try {
            const latest = await fetchLatestBaileysVersion();
            version = latest.version as [number, number, number];
            console.log(`Using fetched WhatsApp Web version: ${version.join('.')}, is latest: ${latest.isLatest}`);
        } catch (err) {
            console.warn('Failed to fetch latest WaWeb version, using hardcoded stable fallback:', version.join('.'), err);
        }

        const currentSock = makeWASocket({
            version,
            auth: state,
            printQRInTerminal: true,
            logger: pino({ level: 'silent' }),
            browser: ['Chrome', 'Windows', '110.0.5481.177']
        });
        sock = currentSock;

        currentSock.ev.on('creds.update', saveCreds);

        currentSock.ev.on('connection.update', async (update) => {
            // Ignorar eventos de sockets que ya fueron reemplazados
            if (sock !== currentSock) return;

            const { connection, lastDisconnect, qr } = update;

            if (qr) {
                try {
                    qrCodeData = await qrcode.toDataURL(qr);
                } catch (err) {
                    console.error('Error generating QR data URL:', err);
                }
            }

            if (connection === 'close') {
                const statusCode = (lastDisconnect?.error as Boom)?.output?.statusCode;
                const shouldReconnect = statusCode !== DisconnectReason.loggedOut;
                console.log('Connection closed due to', lastDisconnect?.error?.message, ', status code:', statusCode, ', reconnecting', shouldReconnect);
                connectionStatus = 'DISCONNECTED';
                qrCodeData = null;

                if (statusCode === 401) {
                    console.warn('Unauthorized (401) detected. Clearing credentials folder to force a fresh QR scan.');
                    try {
                        if (fs.existsSync(authFolder)) {
                            fs.rmSync(authFolder, { recursive: true, force: true });
                        }
                    } catch (err) {
                        console.error('Failed to clear authFolder on 401 error:', err);
                    }
                    // Delay reconnection slightly to ensure filesystem release
                    scheduleReconnect(2000, true);
                } else if (shouldReconnect) {
                    reconnectAttempts++;
                    const delay = Math.min(1000 * Math.pow(2, reconnectAttempts), 60000);
                    console.log(`Scheduling reconnection in ${delay}ms (attempt ${reconnectAttempts})...`);
                    scheduleReconnect(delay);
                }
            } else if (connection === 'connecting') {
                connectionStatus = 'CONNECTING';
                console.log('Connecting to WhatsApp...');
            } else if (connection === 'open') {
                connectionStatus = 'CONNECTED';
                qrCodeData = null;
                reconnectAttempts = 0;
                console.log('WhatsApp connection is open and active.');
            }
        });
    } catch (err) {
        console.error('Error initializing WhatsApp connection:', err);
        connectionStatus = 'DISCONNECTED';
        reconnectAttempts++;
        scheduleReconnect(Math.min(1000 * Math.pow(2, reconnectAttempts), 60000));
    } finally {
        isConnecting = false;
    }
}

function normalizeMexicanNumber(number: string): string {
    let formattedNumber = String(number).replace(/\D/g, '');

    // Clean prefix variations to isolate the 10-digit number
    if (formattedNumber.startsWith('521') && formattedNumber.length === 13) {
        formattedNumber = formattedNumber.substring(3);
    } else if (formattedNumber.startsWith('52') && formattedNumber.length === 12) {
        formattedNumber = formattedNumber.substring(2);
    } else if (formattedNumber.length > 10) {
        // General fallback: take the last 10 digits
        formattedNumber = formattedNumber.slice(-10);
    }

    return formattedNumber;
}

async function resolveJid(activeSock: WASocket, formattedNumber: string): Promise<string> {
    // WhatsApp E.164 format for mobile numbers in Mexico is strictly: 52 + 1 + 10 digits
    let jid = `521${formattedNumber}@s.whatsapp.net`;

    // Attempt onWhatsApp lookup but fallback gracefully to PN E.164 if lookup fails or returns exists: false
    try {
        const results = await activeSock.onWhatsApp(formattedNumber);
        if (results && results.length > 0 && results[0].exists) {
            jid = results[0].jid;
        } else {
            console.warn(`[WhatsApp] onWhatsApp returned false/empty for ${maskNumber(formattedNumber)}. Using fallback JID.`);
        }
    } catch (checkErr: any) {
        console.warn(`[WhatsApp] Error during onWhatsApp check for ${maskNumber(formattedNumber)}, using fallback JID:`, checkErr?.message || checkErr);
    }

    return jid;
}

// Endpoints
app.get('/status', (req, res) => {
    res.json({
        status: connectionStatus,
        qr: qrCodeData
    });
});

app.post('/send', async (req, res) => {
    const { number, message } = req.body ?? {};

    if (!number || !message) {
        return res.status(400).json({ error: 'Number and message are required.' });
    }

    const activeSock = sock;
    if (connectionStatus !== 'CONNECTED' || !activeSock) {
        return res.status(503).json({ error: 'WhatsApp client is not connected.' });
    }

    try {
        const formattedNumber = normalizeMexicanNumber(number);
        const jid = await resolveJid(activeSock, formattedNumber);

        await activeSock.sendMessage(jid, { text: message });
        console.log(`[WhatsApp SEND] Message sent to ${maskNumber(formattedNumber)} (length: ${String(message).length})`);

        res.json({ success: true, message: 'Message sent successfully.' });
    } catch (error: any) {
        console.error('[WhatsApp SEND] Error sending message:', error);
        res.status(500).json({ error: error?.message || 'Failed to send message.' });
    }
});

app.post('/send-document', async (req, res) => {
    const { number, fileBase64, fileName, mimeType, caption } = req.body ?? {};

    if (!number || !fileBase64 || !fileName || !mimeType) {
        return res.status(400).json({ error: 'Number, fileBase64, fileName, and mimeType are required.' });
    }

    const activeSock = sock;
    if (connectionStatus !== 'CONNECTED' || !activeSock) {
        return res.status(503).json({ error: 'WhatsApp client is not connected.' });
    }

    try {
        const formattedNumber = normalizeMexicanNumber(number);
        const jid = await resolveJid(activeSock, formattedNumber);
        const buffer = Buffer.from(fileBase64, 'base64');

        await activeSock.sendMessage(jid, {
            document: buffer,
            mimetype: mimeType,
            fileName: fileName,
            caption: caption
        });
        console.log(`[WhatsApp SEND DOCUMENT] Document "${fileName}" (${buffer.length} bytes) sent to ${maskNumber(formattedNumber)}`);

        res.json({ success: true, message: 'Document sent successfully.' });
    } catch (error: any) {
        console.error('[WhatsApp SEND DOCUMENT] Error sending document:', error);
        res.status(500).json({ error: error?.message || 'Failed to send document.' });
    }
});

app.post('/logout', async (req, res) => {
    try {
        const activeSock = sock;
        if (activeSock) {
            try {
                await activeSock.logout();
            } catch (err) {
                console.warn('Error during WhatsApp logout, continuing with credential cleanup:', err);
            }
        }
        closeCurrentSocket();

        // Clean auth info directory
        if (fs.existsSync(authFolder)) {
            fs.rmSync(authFolder, { recursive: true, force: true });
        }
        connectionStatus = 'DISCONNECTED';
        qrCodeData = null;
        scheduleReconnect(1000, true);
        res.json({ success: true, message: 'Logged out successfully.' });
    } catch (error: any) {
        console.error('[WhatsApp LOGOUT] Error:', error);
        res.status(500).json({ error: error?.message || 'Failed to logout.' });
    }
});

// Start Baileys connection
connectToWhatsApp();

app.listen(PORT, () => {
    console.log(`WhatsApp synchronization service listening on port ${PORT}`);
});
