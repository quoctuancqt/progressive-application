var xhrHelper = {
    makeCorsRequest: function (url, options, callbackSuccess, callbackError) {
        var xhr = createCORSRequest(options.method, url);

        if (!xhr) {
            return;
        }

        if (options.responseType) {
            xhr.responseType = options.responseType;
        }

        if (options.token) {
            xhr.setRequestHeader("Authorization", options.token);
        }

        if (options.contentType) {
            xhr.setRequestHeader("Content-Type", options.contentType);
        }

        xhr.onreadystatechange = function (oEvent) {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    console.log(xhr.responseText)
                } else {
                    if (callbackError) {
                        callbackError(xhr);
                    }
                }
            }
        }; 

        xhr.onload = function (resp) {
            if (callbackSuccess) {
                callbackSuccess(resp);
            }
        };

        xhr.onerror = function (error) {
            if (callbackError) {
                callbackError(error);
            }
        };

        if (options.postData) {
            xhr.send(options.postData);
        }
        else {
            xhr.send();
        }
    }
};

function createCORSRequest(method, url) {
    var xhr = new XMLHttpRequest();
    if ("withCredentials" in xhr) {
        xhr.open(method, url, true);
    }
    else if (typeof XDomainRequest != "undefined") {
        xhr = new XDomainRequest();
        xhr.open(method, url);
    }
    else {
        xhr = null;
    }
    return xhr;
}