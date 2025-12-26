window.fileDownloadHelper = {
    downloadFile: function (fileName, contentType, content) {
        // Convertir el array de bytes a Blob
        const blob = new Blob([content], { type: contentType });

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