window.fileDownloadHelper = {
    downloadFile: function (fileName, contentType, base64Content) {
        // Convert Base64 to Byte Array
        const byteCharacters = atob(base64Content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);

        // Convertir el array de bytes a Blob
        const blob = new Blob([byteArray], { type: contentType });

        // Crear URL temporal
        const url = window.URL.createObjectURL(blob);

        // Crear elemento de descarga
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName;

        // Simular click
        document.body.appendChild(anchorElement);
        anchorElement.click();

        // Limpiar
        document.body.removeChild(anchorElement);
        window.URL.revokeObjectURL(url);
    }
};