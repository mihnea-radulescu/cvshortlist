window.pdfStore = {
    pdfFilesDbName: "CvShortlist-PdfFilesStorage",
    pdfFilesDbVersion: 1,
    pdfFilesObjectStoreName: "PdfFiles",
    pdfFilesKeyPath: "sha256Hash",
    readOnlyAccess: "readonly",
    readWriteAccess: "readwrite",

    pdfMimeType: "application/pdf",

    maximumFileUploadsAtOnce: 100,
    minimumPdfFileSizeInBytes: 2 * 1024, // 2 KB
    maximumPdfFileSizeInBytes: 10 * 1024 * 1024, // 10 MB
    maximumPdfFileNumberOfPages: 5,
    maximumZipArchiveSizeInBytes: 500 * 1024 * 1024, // 500 MB

    messageTypeInfo: "info",
    messageTypeWarning: "warning",
    messageTypeError: "error",

    db: null,

    generateSha256Hash: async function(data) {
        const hashBuffer = await crypto.subtle.digest("SHA-256", data);
        const hashArray = Array.from(new Uint8Array(hashBuffer));

        return hashArray
            .map(aNumber => aNumber.toString(16).padStart(2, "0"))
            .join("");
    },

    isZipFile: function(inputFileName) {
        return inputFileName.toLowerCase().endsWith(".zip");
    },
    isPdfFile: function(inputFileName) {
        return inputFileName.toLowerCase().endsWith(".pdf");
    },

    readZipFile: async function(anInputZipFile, pdfFiles, readFileMessages) {
        const inputZipFileName = anInputZipFile.name;
        const inputZipFileSize = anInputZipFile.size;

        if (inputZipFileSize > this.maximumZipArchiveSizeInBytes) {
            readFileMessages.push({
                messageType: this.messageTypeError,
                message: `'${inputZipFileName}' is too large. The maximum allowed ZIP archive size is 500MB.`
            });
        } else {
            let zipReader;

            try {
                zipReader = new zip.ZipReader(new zip.BlobReader(anInputZipFile));
                const entries = await zipReader.getEntries();

                for (const anEntry of entries) {
                    const fileNameWithoutPath = anEntry.filename.split("/").pop();
                    if (!anEntry.directory && this.isPdfFile(fileNameWithoutPath)) {
                        try {
                            const anInputPdfFile = await anEntry.getData(new zip.BlobWriter(this.pdfMimeType));
                            anInputPdfFile.name = fileNameWithoutPath;

                            await this.readPdfFile(anInputPdfFile, pdfFiles, readFileMessages, inputZipFileName);
                        }
                        catch {
                            readFileMessages.push({
                                messageType: this.messageTypeError,
                                message: `Could not read file '${fileNameWithoutPath}' (from '${inputZipFileName}').`
                            });
                        }
                    }
                }
            } catch {
                readFileMessages.push({
                    messageType: this.messageTypeError,
                    message: `'${inputZipFileName}' is not a valid ZIP archive.`
                });
            } finally {
                if (zipReader) {
                    await zipReader.close();
                }
            }
        }
    },

    readPdfFile: async function(anInputPdfFile, pdfFiles, readFileMessages, inputZipFileName) {
        const inputPdfFileName = anInputPdfFile.name;
        const inputPdfFileSize = anInputPdfFile.size;

        if (inputPdfFileSize < this.minimumPdfFileSizeInBytes) {
            readFileMessages.push({
                messageType: this.messageTypeError,
                message: `'${inputPdfFileName}' is too small. The minimum allowed PDF file size is 2KB.`
            });
        } else if (inputPdfFileSize > this.maximumPdfFileSizeInBytes) {
            readFileMessages.push({
                messageType: this.messageTypeError,
                message: `'${inputPdfFileName}' is too large. The maximum allowed PDF file size is 10MB.`
            });
        } else {
            try {
                const inputPdfFileArrayBuffer = await anInputPdfFile.arrayBuffer();
                const inputPdfFileData = new Uint8Array(inputPdfFileArrayBuffer);

                const inputPdfSha256Hash = await this.generateSha256Hash(inputPdfFileData);

                if (pdfFiles.has(inputPdfSha256Hash))
                {
                    const fileAlreadySelectedForUploadingMessage = inputZipFileName
                        ? `PDF file '${inputPdfFileName}' (from '${inputZipFileName}') has already been selected for uploading.`
                        : `PDF file '${inputPdfFileName}' has already been selected for uploading.`;
                    readFileMessages.push({
                        messageType: this.messageTypeWarning,
                        message: fileAlreadySelectedForUploadingMessage
                    });
                } else {
                    let isValidPdfFile = true;
                    try {
                        const pdfDocument = await PDFLib.PDFDocument.load(inputPdfFileData);
                        const pageCount = pdfDocument.getPageCount();

                        if (pageCount > this.maximumPdfFileNumberOfPages) {
                            isValidPdfFile = false;
                            readFileMessages.push({
                                messageType: this.messageTypeError,
                                message: `Cannot upload PDF file '${inputPdfFileName}', as it has more than ${this.maximumPdfFileNumberOfPages} pages.`});
                        }
                    } catch {
                        isValidPdfFile = false;
                        readFileMessages.push({
                            messageType: this.messageTypeError,
                            message: `Cannot upload PDF file '${inputPdfFileName}', as it is not a valid PDF.`});
                    }

                    if (isValidPdfFile) {
                        const inputPdfFileMetadata = {
                            fileName: inputPdfFileName,
                            sha256Hash: inputPdfSha256Hash
                        };
                        pdfFiles.set(inputPdfFileMetadata.sha256Hash, inputPdfFileMetadata);
                        await this.saveFile(inputPdfFileMetadata, inputPdfFileData);

                        const readFileSuccessMessage = inputZipFileName
                            ? `'${inputPdfFileName}' (from '${inputZipFileName}') is ready to be uploaded.`
                            : `'${inputPdfFileName}' is ready to be uploaded.`;
                        readFileMessages.push({
                            messageType: this.messageTypeInfo,
                            message: readFileSuccessMessage
                        });
                    }
                }
            } catch {
                const readFileErrorMessage = inputZipFileName
                    ? `Could not read file '${inputPdfFileName}' (from '${inputZipFileName}').`
                    : `Could not read file '${inputPdfFileName}'.`;
                readFileMessages.push({
                    messageType: this.messageTypeError,
                    message: readFileErrorMessage
                });
            }
        }
    },

    readInputFiles: async function (inputFileId) {
        const inputElement = document.getElementById(inputFileId);
        const inputFiles = inputElement.files;

        const pdfFiles = new Map();
        const readFileMessages = [];

        if (inputFiles.length > this.maximumFileUploadsAtOnce) {
            readFileMessages.push({
                messageType: this.messageTypeError,
                message: `Cannot upload more than ${this.maximumFileUploadsAtOnce} files at once.`,
            });
        } else {
            for (const anInputFile of inputFiles) {
                if (this.isZipFile(anInputFile.name)) {
                    await this.readZipFile(anInputFile, pdfFiles, readFileMessages);
                } else if (this.isPdfFile(anInputFile.name)) {
                    await this.readPdfFile(anInputFile, pdfFiles, readFileMessages, null);
                }
            }
        }

        return {
            pdfFiles: [...pdfFiles.values()],
            readFileMessages: readFileMessages
        }
    },

    setTextContent: function (inputFileInfoId, textContent) {
        const inputInfoElement = document.getElementById(inputFileInfoId);
        if (inputInfoElement) {
            inputInfoElement.textContent = textContent;
        }
    },

    openDb: function () {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.pdfFilesDbName, this.pdfFilesDbVersion);

            request.onupgradeneeded = () => {
                const db = request.result;
                if (!db.objectStoreNames.contains(this.pdfFilesObjectStoreName)) {
                    db.createObjectStore(this.pdfFilesObjectStoreName, { keyPath: this.pdfFilesKeyPath });
                }
            };

            request.onsuccess = () => {
                this.db = request.result;
                resolve();
            };

            request.onerror = (e) => reject(e);
        });
    },

    saveFile: async function (inputPdfFileMetadata, inputPdfFileData) {
        if (!this.db) {
            await this.openDb();
        }

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(this.pdfFilesObjectStoreName, this.readWriteAccess);
            const store = transaction.objectStore(this.pdfFilesObjectStoreName);

            const storeObject = { ...inputPdfFileMetadata, inputPdfFileData }
            store.put(storeObject);

            transaction.oncomplete = () => resolve();
            transaction.onerror = (e) => reject(e);
        });
    },

    getFileContent: async function (sha256Hash) {
        const file = await this.getFile(sha256Hash);

        if (!file) {
            return null;
        }

        return file.inputPdfFileData;
    },

    getFile: async function (sha256Hash) {
        if (!this.db) {
            await this.openDb();
        }

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(this.pdfFilesObjectStoreName, this.readOnlyAccess);
            const store = transaction.objectStore(this.pdfFilesObjectStoreName);

            const request = store.get(sha256Hash);

            request.onsuccess = () => resolve(request.result);
            request.onerror = (e) => reject(e);
        });
    },

    deleteFile: async function (name) {
        if (!this.db) {
            await this.openDb();
        }

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction(this.pdfFilesObjectStoreName, this.readWriteAccess);
            const store = transaction.objectStore(this.pdfFilesObjectStoreName);

            store.delete(name);

            transaction.oncomplete = () => resolve();
            transaction.onerror = (e) => reject(e);
        });
    },

    deleteDatabase: async function () {
        return new Promise((resolve, reject) => {
            if (this.db) {
                this.db.close();
                this.db = null;
            }

            const deleteRequest = indexedDB.deleteDatabase(this.pdfFilesDbName);

            deleteRequest.onsuccess = () => resolve();
            deleteRequest.onerror = (e) => reject(e.target.error);
        });
    }
};
