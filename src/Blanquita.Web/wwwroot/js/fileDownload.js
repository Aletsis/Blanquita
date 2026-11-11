window.fileDownloadHelper = {
    _base64ToBlob: function (base64Content, contentType) {
        const byteCharacters = atob(base64Content);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        return new Blob([byteArray], { type: contentType });
    },

    downloadFile: function (fileName, contentType, base64Content) {
        const blob = this._base64ToBlob(base64Content, contentType);
        const url = window.URL.createObjectURL(blob);

        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName;

        document.body.appendChild(anchorElement);
        anchorElement.click();

        document.body.removeChild(anchorElement);
        window.URL.revokeObjectURL(url);
    },

    printPdf: function (base64Content) {
        const blob = this._base64ToBlob(base64Content, 'application/pdf');
        const url = window.URL.createObjectURL(blob);

        const iframe = document.createElement('iframe');
        iframe.style.position = 'fixed';
        iframe.style.right = '0';
        iframe.style.bottom = '0';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = '0';
        iframe.style.visibility = 'hidden';

        let cleaned = false;
        const cleanup = function () {
            if (cleaned) return;
            cleaned = true;
            if (iframe.parentNode) {
                iframe.parentNode.removeChild(iframe);
            }
            window.URL.revokeObjectURL(url);
        };

        iframe.onload = function () {
            try {
                if (iframe.contentWindow) {
                    try {
                        iframe.contentWindow.addEventListener('afterprint', cleanup, { once: true });
                    } catch (_) {
                        // Algunos visores de PDF no permiten listeners sobre contentWindow
                    }
                    iframe.contentWindow.focus();
                    iframe.contentWindow.print();
                } else {
                    window.open(url, '_blank');
                }
            } catch (_) {
                window.open(url, '_blank');
            }
            setTimeout(cleanup, 120000);
        };

        iframe.onerror = cleanup;
        iframe.src = url;
        document.body.appendChild(iframe);
    }
};