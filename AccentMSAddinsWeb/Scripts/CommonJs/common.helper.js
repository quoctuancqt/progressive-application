"use strict";
var commonHelper = {
    writeFile: function (msg) {
        var logPath = "D:\\log.txt";
        var FileOpener = new ActiveXObject("Scripting.FileSystemObject");
        var FilePointer = FileOpener.OpenTextFile(logPath, 8, true);
        FilePointer.WriteLine(msg);
        FilePointer.Close();
    },
    getParameterByName: function (name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    },
    filterArray: function (src, filt) {
        var temp = {}, i, result = [];
        for (i = 0; i < filt.length; i++) {
            temp[filt[i]] = true;
        }

        for (i = 0; i < src.length; i++) {
            if (!(src[i] in temp)) {
                result.push(src[i]);
            }
        }
        return (result);
    },
    addClass: function (id, className, isGetParent) {
        var elem = $(id);
        if (isGetParent) {
            elem = elem.parent();
        }
        if (!elem.hasClass(className)) {
            elem.addClass(className);
        }
    },
    removeClass: function (id, className, isGetParent) {
        var elem = $(id);
        if (isGetParent) {
            elem = elem.parent();
        }
        if (elem.hasClass(className)) {
            elem.removeClass(className);
        }
    },
    getBase64DataResponse: function (response) {
        return response.srcElement.response.replace(/"/g, "")
    },
    base64ToBlob: function (str) {
        var pos = str.indexOf(';base64,');
        var type = str.substring(5, pos);
        var b64 = str.substr(pos + 8);
        var content = atob(b64);
        var buffer = new ArrayBuffer(content.length);
        var view = new Uint8Array(buffer);

        for (var n = 0; n < content.length; n++) {
            view[n] = content.charCodeAt(n);
        }

        var blob = new Blob([buffer], { type: type });

        return blob;
    },
    detectBrowser: function () {
        // Opera 8.0+
        var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
        if (isOpera) {
            return 'Opera';
        }

        // Firefox 1.0+
        var isFirefox = typeof InstallTrigger !== 'undefined';
        if (isFirefox) {
            return 'Firefox';
        }

        // Safari 3.0+ "[object HTMLElementConstructor]" 
        var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || (typeof safari !== 'undefined' && safari.pushNotification));
        if (isSafari) {
            return 'Safari'
        }

        // Internet Explorer 6-11
        var isIE = /*@cc_on!@*/false || !!document.documentMode;
        if (isIE) {
            return 'IE';
        }

        // Edge 20+
        var isEdge = !isIE && !!window.StyleMedia;
        if (isEdge) {
            return 'Edge';
        }

        // Chrome 1+
        var isChrome = !!window.chrome && !!window.chrome.webstore;
        if (isChrome) {
            return 'Chrome';
        }

        // Blink engine detection
        var isBlink = (isChrome || isOpera) && !!window.CSS;
        if (isBlink) {
            return 'Blink';
        }
    },
    removeSpecialChars: function (str) {
        return str.replace(/(?!\.[^.]+$)\.|[^\w.]+/g, ' ');
    }
}

var loadingIndicator = {
    register: function () {
        var SpinnerElements = document.querySelectorAll(".ms-Spinner");
        for (var i = 0; i < SpinnerElements.length; i++) {
            new fabric['Spinner'](SpinnerElements[i]);
        }

        var dialogElement = document.querySelector("#loading-indicator");
        return new fabric['Dialog'](dialogElement);
    },
    show: function () {
        $("#overlay").addClass("dark-class");
        $("#loading-indicator").addClass("focus");
        $("#loading-indicator").show();
    },
    hide: function (dialog) {
        $("#overlay").removeClass("dark-class");
        $("#loading-indicator").removeClass("focus");
        $("#loading-indicator").hide();
    }
}

var fileHelper = {
    handleSuccess: function (filename, blob) {
        if (typeof window.navigator.msSaveBlob !== 'undefined') {
            window.navigator.msSaveBlob(blob, filename);
        } else {
            var URL = window.URL || window.webkitURL;
            var downloadUrl = URL.createObjectURL(blob);

            if (filename) {
                var a = document.createElement("a");
                if (typeof a.download === 'undefined') {
                    window.location = downloadUrl;
                } else {
                    a.href = downloadUrl;
                    a.download = filename;
                    document.body.appendChild(a);
                    a.click();
                }
            } else {
                window.location = downloadUrl;
            }
            setTimeout(function () { URL.revokeObjectURL(downloadUrl); }, 100);
        }
    }
}