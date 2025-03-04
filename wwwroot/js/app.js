window.app = {
    page: null,

    createObjectUrl: function (fileInput) {
        if (fileInput.files.length > 0) {
            var file = fileInput.files[0];
            var url = window.URL.createObjectURL(file);
            if (window.app.page) {
                window.app.page.invokeMethodAsync('SetBlobUrl', url);
            }
        }
    },

    showFileSelector: function (elementId) {
        var fileSelector = document.getElementById(elementId);
        if (fileSelector) {
            fileSelector.click();
        }
    },

    registerPage: function (pageRef) {
        window.app.page = pageRef;
    }
};

function getUserAgent() {
    return navigator.userAgent;
}

window.getUserAgent = getUserAgent;
window.clipboardCopy = {
    copyText: function (text) {
        navigator.clipboard.writeText(text).then(function () {
            console.log(text);
        })
            .catch(function (error) {
                alert(error);
            });
    }
};