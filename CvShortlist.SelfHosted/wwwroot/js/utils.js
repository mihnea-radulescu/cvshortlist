window.utils = {
    getUrl: function (mimeType, fileData) {
        const uint8Array = new Uint8Array(fileData);
        const blob = new Blob([uint8Array], { type: mimeType });

        return URL.createObjectURL(blob);
    },
    openFile: function (fileName, mimeType, fileData) {
        const url = this.getUrl(mimeType, fileData);

        window.open(url, "_blank");
    },
    downloadFile: function (fileName, mimeType, fileData) {
        const url = this.getUrl(mimeType, fileData);

        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = fileName;
        anchor.type = mimeType;
        document.body.appendChild(anchor);

        anchor.click();
        anchor.remove();

        URL.revokeObjectURL(url);
    },

    getUserSettings: function () {
        const culture = navigator.language || 'en-US';

        const dateTimeOffsetInMinutes = (() => {
            try {
                const offset = -(new Date().getTimezoneOffset());
                return Number.isFinite(offset) ? offset : 0;
            } catch {
                return 0;
            }
        })();

        return {
            culture: culture,
            dateTimeOffsetInMinutes: dateTimeOffsetInMinutes
        };
    },

    scrollToTop: function () {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    },
    scrollToBottom: function () {
        window.scrollTo({
            top: document.documentElement.scrollHeight,
            behavior: 'smooth'
        });
    },
};
