import express from 'express';
import cors from 'cors';
import makeWASocket, { DisconnectReason, useMultiFileAuthState, fetchLatestBaileysVersion } from '@whiskeysockets/baileys';
import qrcode from 'qrcode';
import path from 'path';
import fs from 'fs';
import pino from 'pino';
import { fileURLToPath } from 'url';
const app = express();
const PORT = process.env.PORT || 3001;
app.use(cors());
app.use(express.json());
// Global logging middleware for iisnode to record all requests and responses
app.use((req, res, next) => {
    const start = Date.now();
    console.log(`[REQUEST] ${req.method} ${req.url} - Headers: ${JSON.stringify(req.headers)} - Body: ${JSON.stringify(req.body)}`);
    res.on('finish', () => {
        const duration = Date.now() - start;
        console.log(`[RESPONSE] ${req.method} ${req.url} - Status: ${res.statusCode} - Duration: ${duration}ms`);
    });
    next();
});
let sock = null;
let qrCodeData = null;
let connectionStatus = 'DISCONNECTED';
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const authFolder = path.join(__dirname, '../auth_info');
async function connectToWhatsApp() {
    const { state, saveCreds } = await useMultiFileAuthState(authFolder);
    // Fetch the latest WhatsApp Web version dynamically to bypass the 405 error
    let version = [2, 3000, 1017585084]; // Last known stable fallback
    try {
        const latest = await fetchLatestBaileysVersion();
        version = latest.version;
        console.log(`Using fetched WhatsApp Web version: ${version.join('.')}, is latest: ${latest.isLatest}`);
    }
    catch (err) {
        console.warn('Failed to fetch latest WaWeb version, using hardcoded stable fallback:', version.join('.'), err);
    }
    sock = makeWASocket({
        version,
        auth: state,
        printQRInTerminal: true,
        logger: pino({ level: 'silent' }),
        browser: ['Chrome', 'Windows', '110.0.5481.177']
    });
    sock.ev.on('creds.update', saveCreds);
    sock.ev.on('connection.update', async (update) => {
        const { connection, lastDisconnect, qr } = update;
        if (qr) {
            try {
                qrCodeData = await qrcode.toDataURL(qr);
            }
            catch (err) {
                console.error('Error generating QR data URL:', err);
            }
        }
        if (connection === 'close') {
            const statusCode = lastDisconnect?.error?.output?.statusCode;
            const shouldReconnect = statusCode !== DisconnectReason.loggedOut;
            console.log('Connection closed due to', lastDisconnect?.error, ', status code:', statusCode, ', reconnecting', shouldReconnect);
            connectionStatus = 'DISCONNECTED';
            qrCodeData = null;
            if (statusCode === 401) {
                console.warn('Unauthorized (401) detected. Clearing credentials folder to force a fresh QR scan.');
                try {
                    if (fs.existsSync(authFolder)) {
                        fs.rmSync(authFolder, { recursive: true, force: true });
                    }
                }
                catch (err) {
                    console.error('Failed to clear authFolder on 401 error:', err);
                }
                // Delay reconnection slightly to ensure filesystem release
                setTimeout(() => {
                    connectToWhatsApp();
                }, 2000);
            }
            else if (shouldReconnect) {
                connectToWhatsApp();
            }
        }
        else if (connection === 'connecting') {
            connectionStatus = 'CONNECTING';
            console.log('Connecting to WhatsApp...');
        }
        else if (connection === 'open') {
            connectionStatus = 'CONNECTED';
            qrCodeData = null;
            console.log('WhatsApp connection is open and active.');
        }
    });
}
// Endpoints
app.get('/status', (req, res) => {
    res.json({
        status: connectionStatus,
        qr: qrCodeData
    });
});
app.post('/send', async (req, res) => {
    console.log('[WhatsApp SEND] Raw Headers:', JSON.stringify(req.headers));
    console.log('[WhatsApp SEND] Raw Body:', JSON.stringify(req.body));
    const { number, message } = req.body;
    if (!number || !message) {
        return res.status(400).json({ error: 'Number and message are required.' });
    }
    if (connectionStatus !== 'CONNECTED' || !sock) {
        return res.status(503).json({ error: 'WhatsApp client is not connected.' });
    }
    try {
        console.log(`[WhatsApp SEND] Request received. Number: ${number}, Message length: ${message?.length}`);
        let formattedNumber = number.replace(/\D/g, '');
        console.log(`[WhatsApp SEND] Clean digits: ${formattedNumber}`);
        // Clean prefix variations to isolate the 10-digit number
        if (formattedNumber.startsWith('521') && formattedNumber.length === 13) {
            formattedNumber = formattedNumber.substring(3);
        }
        else if (formattedNumber.startsWith('52') && formattedNumber.length === 12) {
            formattedNumber = formattedNumber.substring(2);
        }
        else if (formattedNumber.length > 10) {
            // General fallback: take the last 10 digits
            formattedNumber = formattedNumber.slice(-10);
        }
        console.log(`[WhatsApp SEND] Isolate 10-digit: ${formattedNumber}`);
        // WhatsApp E.164 format for mobile numbers in Mexico is strictly: 52 + 1 + 10 digits
        const fallbackJid = `521${formattedNumber}@s.whatsapp.net`;
        let jid = fallbackJid;
        // Attempt onWhatsApp lookup but fallback gracefully to PN E.164 if lookup fails or returns exists: false
        try {
            console.log(`[WhatsApp SEND] Performing onWhatsApp check for: ${formattedNumber}`);
            const results = await sock.onWhatsApp(formattedNumber);
            console.log(`[WhatsApp SEND] onWhatsApp Raw Results: ${JSON.stringify(results)}`);
            if (results && results.length > 0 && results[0].exists) {
                jid = results[0].jid;
                console.log(`[WhatsApp SEND] Successfully mapped JID: ${jid}`);
            }
            else {
                console.warn(`[WhatsApp SEND] onWhatsApp returned false/empty. Fallback JID: ${jid}`);
            }
        }
        catch (checkErr) {
            console.warn(`[WhatsApp SEND] Error during onWhatsApp check, falling back to ${jid}:`, checkErr.message || checkErr);
        }
        console.log(`[WhatsApp SEND] Sending WhatsApp message to JID: ${jid}`);
        await sock.sendMessage(jid, { text: message });
        res.json({ success: true, message: 'Message sent successfully.' });
    }
    catch (error) {
        console.error('[WhatsApp SEND] CRITICAL EXCEPTION:', error);
        res.status(500).json({
            error: error.message || 'Failed to send message.',
            details: error.stack || 'No stack trace available',
            raw: JSON.stringify(error)
        });
    }
});
app.post('/logout', async (req, res) => {
    try {
        if (sock) {
            await sock.logout();
        }
        // Clean auth info directory
        if (fs.existsSync(authFolder)) {
            fs.rmSync(authFolder, { recursive: true, force: true });
        }
        connectionStatus = 'DISCONNECTED';
        qrCodeData = null;
        connectToWhatsApp();
        res.json({ success: true, message: 'Logged out successfully.' });
    }
    catch (error) {
        res.status(500).json({ error: error.message || 'Failed to logout.' });
    }
});
// Start Baileys connection
connectToWhatsApp();
app.listen(PORT, () => {
    console.log(`WhatsApp synchronization service listening on port ${PORT}`);
});
