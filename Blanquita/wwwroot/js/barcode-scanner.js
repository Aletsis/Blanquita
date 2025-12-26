let stream = null;
let scannerInterval = null;
let dotNetHelper = null;

window.initBarcodeScanner = (helper) => {
    dotNetHelper = helper;
};

window.startScanning = async () => {
    const video = document.getElementById('video');
    const canvas = document.getElementById('canvas');
    const context = canvas.getContext('2d');

    try {
        // Solicitar acceso a la cámara
        stream = await navigator.mediaDevices.getUserMedia({
            video: {
                width:
                {
                    min: 1280,
                    ideal: 1920,
                    max: 2560,
                },
                height:
                {
                    min: 720,
                    ideal: 1080,
                    max: 1440
                },
                facingMode:
                {
                    exact: 'environment' // Cámara trasera
                }
            }
        });

        video.srcObject = stream;
        await video.play();

        // Configurar canvas con las dimensiones del video
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        // Cargar ZXing dinámicamente
        if (!window.ZXing) {
            await loadZXingScript();
        }

        const codeReader = new ZXing.BrowserMultiFormatReader();

        // Escanear continuamente
        scannerInterval = setInterval(async () => {
            if (video.readyState === video.HAVE_ENOUGH_DATA) {
                context.drawImage(video, 0, 0, canvas.width, canvas.height);
                const imageData = context.getImageData(0, 0, canvas.width, canvas.height);

                try {
                    const result = await codeReader.decodeFromImageData(imageData);
                    if (result) {
                        dotNetHelper.invokeMethodAsync('OnBarcodeDetected',
                            result.text,
                            result.format);

                        // Opcional: vibración al detectar código
                        if (navigator.vibrate) {
                            navigator.vibrate(200);
                        }
                    }
                } catch (err) {
                    // No se encontró código en este frame, continuar escaneando
                }
            }
        }, 100); // Escanear cada 100ms

    } catch (err) {
        console.error('Error al acceder a la cámara:', err);
        let errorMsg = 'No se pudo acceder a la cámara. ';

        if (err.name === 'NotAllowedError') {
            errorMsg += 'Permiso denegado. Por favor, permite el acceso a la cámara.';
        } else if (err.name === 'NotFoundError') {
            errorMsg += 'No se encontró ninguna cámara en el dispositivo.';
        } else {
            errorMsg += err.message;
        }

        dotNetHelper.invokeMethodAsync('OnError', errorMsg);
    }
};

window.stopScanning = () => {
    // Detener el intervalo de escaneo
    if (scannerInterval) {
        clearInterval(scannerInterval);
        scannerInterval = null;
    }

    // Detener el stream de la cámara
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        stream = null;
    }

    // Limpiar el video
    const video = document.getElementById('video');
    if (video) {
        video.srcObject = null;
    }
};

// Cargar librería ZXing desde CDN
function loadZXingScript() {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = 'https://unpkg.com/@zxing/library@latest/umd/index.min.js';
        script.onload = resolve;
        script.onerror = reject;
        document.head.appendChild(script);
    });
}