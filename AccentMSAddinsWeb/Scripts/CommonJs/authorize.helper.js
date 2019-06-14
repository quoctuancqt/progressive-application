var authHash = "";

var authorize = {
    login: function (loginObj, handleLoginCallback) {
        var result = null;

        this.getWebClientUrl(loginObj.username, loginObj.environmentName, loginObj.libraryName, function (resp) {
            var webClientUrl = resp[0] || null;

            if (webClientUrl != null) {
                storageLibrary.saveToLocalStorage("webClientUrl", webClientUrl.webClientUrl);
                storageLibrary.saveToLocalStorage("libraryShortName", webClientUrl.libraryShortName);

                processLogon(loginObj, webClientUrl.webClientUrl, function (resp) {
                    if (loginObj.isRemember === true) {
                        rememberLoginInfo(loginObj);
                    }
                    else {
                        clearLoginInfo();
                    }

                    storageLibrary.saveToLocalStorage("authHash", authHash);

                    handleLoginCallback(resp);
                }, function (error) {
                    result = {
                        messageError: settings.errorMsgs.wrongUsrPass
                    };

                    if (handleLoginCallback) {
                        handleLoginCallback(result);
                    }
                });
            }
            else {
                result = {
                    messageError: settings.errorMsgs.wrongLib
                };

                if (handleLoginCallback) {
                    handleLoginCallback(result);
                }
            }
        });
    },
    getWebClientUrl: function (username, environmentName, libraryName, callbackSuccess, callbackError) {
        var endpoint = urls.getWebClientUrl;
        endpoint = endpoint.replace("{username}", username);
        endpoint = endpoint.replace("{environmentName}", environmentName);
        endpoint = endpoint.replace("{libraryName}", libraryName);
        $.ajax({
            method: 'GET',
            url: endpoint,
            success: function (resp) {
                callbackSuccess(resp);
            },
            error: function (error) {
                callbackError(error);
            }
        });
    }
}

function processLogon(loginObj, webClientUrl, callbackSuccess, callbackError) {
    var endpoint = '';
    var hasPassword = CryptoJS.MD5(loginObj.password).toString(CryptoJS.enc.Base64);
    authHash = "Hash " + window.btoa(loginObj.username + ':' + hasPassword.substring(0, 21));
    var authorization = encodeURIComponent(authHash);
    endpoint = webClientUrl + loginObj.libraryName + "/Default.ashx?m=logon&app=" + settings.appName + "&auth=" + authorization + "&out=cjson";
    $.ajax({
        method: "GET",
        url: endpoint,
        success: function (resp) {
            callbackSuccess(resp);
        },
        error: function (error) {
            callbackError(error);
        }
    })
}

function rememberLoginInfo(loginObj) {
    localStorage.setItem("username", loginObj.username);
    localStorage.setItem("password", loginObj.password);
    localStorage.setItem("libraryName", loginObj.libraryName);
    localStorage.setItem("environmentName", loginObj.environmentName);
    localStorage.setItem("isRemember", loginObj.isRemember);
}

function clearLoginInfo() {
    localStorage.clear();
}