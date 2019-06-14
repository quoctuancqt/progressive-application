"use strict";
var authHash = "";
var webClientUrl = "";
var libraryShortName = "";
var environmentName = "";
var fileUrl = "";
var downloadLatestProcess = false;
var currentFile = {
    id: "",
    text: ""
}
var canApply = false;
var loadingComponent;

Office.initialize = function (reason) {
    $(document).ready(function () {
        initUI();

        notificator.init();

        fileUrl = Office.context.document.url || "";

        if (fileUrl != "") {
            isValidFile(function (resp) {
                if (resp == "true") {
                    if (!Office.context.document.settings.get("userInfo")) {
                        openDialog(window.location.origin + "/Components/Authorization/Login.html", 60, 72);
                    }
                    else {
                        $("#content-main").show();
                        authHash = storageLibrary.getFromLocalStorage("authHash");
                        webClientUrl = storageLibrary.getFromLocalStorage("webClientUrl");
                        libraryShortName = storageLibrary.getFromLocalStorage("libraryShortName");
                        environmentName = storageLibrary.getFromLocalStorage("environmentName");

                        checkUpdate(isAsk(), !isAsk());
                    }

                    $('#apply-all').click(function () {
                        downloadLatest(true);
                    });
                }
                else {
                    hideLoading();
                    $("#no-content").show();
                }
            });
        }
        else {
            $("#no-content").show();
        }
    });

};

function initUI() {
    var TableElements = document.querySelectorAll(".ms-Table");
    for (var b = 0; b < TableElements.length; b++) {
        new fabric['Table'](TableElements[b]);
    }

    var ButtonElements = document.querySelectorAll(".ms-Button");
    for (var a = 0; a < ButtonElements.length; a++) {
        new fabric['Button'](ButtonElements[a], function () {
        });
    }

    var SpinnerElements = document.querySelectorAll(".ms-Spinner");
    for (var i = 0; i < SpinnerElements.length; i++) {
        new fabric['Spinner'](SpinnerElements[i]);
    }

    $('#grid').w2grid({
        name: 'grid',
        columns: [
            {
                field: 'isSelected', caption: '', size: '10px', sortable: false, render: function (rec) {
                    var isChecked = this.isSelected ? true : false;
                    return '<input type="checkbox" disabled="disabled" checked="' + isChecked + '"/>';
                }
            },
            { field: 'pptSlideIndex', caption: 'Slide No.', size: '20%', resizable: true, sortable: false },
            { field: 'status', caption: 'Status', size: '20%', resizable: true, sortable: false },
            { field: 'lastModDate', caption: 'Modified Date', size: '30%', resizable: true, sortable: false },
            { field: 'source', caption: 'Source', size: '30%', resizable: true, sortable: false }
        ]
    });
}

function showPanel(content) {
    $("#content-main").show();
    if (!Office.context.document.settings.get("userInfo")) {
        Office.context.document.settings.set("userInfo", JSON.stringify(content));
        Office.context.document.settings.saveAsync();
    }

    authHash = storageLibrary.getFromLocalStorage("authHash");
    webClientUrl = storageLibrary.getFromLocalStorage("webClientUrl");
    libraryShortName = storageLibrary.getFromLocalStorage("libraryShortName");
    environmentName = storageLibrary.getFromLocalStorage("environmentName");

    window.location.reload();
}

function checkUpdate(askFirst, confirmMsg) {
    showLoading();

    var pData = {
        ServerInfo: {
            WebClientUrl: webClientUrl,
            ShortLibraryName: libraryShortName,
            AuthHash: authHash,
            EnvironmentName: environmentName
        },
        FileId: currentFile.id,
        FileName: currentFile.text,
        FileUrl: fileUrl,
        AskFirst: askFirst,
        IsConfirm: confirmMsg
    };

    var endpoint = urls.middlewareUrl + "files/checkupdate";

    var options = {
        method: "POST",
        postData: JSON.stringify(pData),
        contentType: "application/json"
    };

    xhrHelper.makeCorsRequest(endpoint, options, function (response) {
        handleCheckUpdateResult(response.srcElement.response);
    }, function (error) {
        console.log(error);
    });
}

function handleCheckUpdateResult(resp) {
    var respObj = JSON.parse(resp);

    if (currentFile.id == "" && respObj.fileId != "") {
        currentFile.id = respObj.fileId;
    }

    if (currentFile.text == "" && respObj.fileName != "") {
        currentFile.text = respObj.fileName;
    }

    if (currentFile.text == null) {
        //var fileName = fileUrl.split("/");
        var fileName = fileUrl.split("\\");
        currentFile.text = fileName[fileName.length - 1];
    }

    if (respObj.statusCode == 211) {
        isAsked();
        notificator.setHeader("Check For Updated Version");
        notificator.setBody("Would you like to check the library for a newer version of this presentation?");
        notificator.setActionForConfirmYes(function () {
            checkUpdate(isAsk, true);
        });
        notificator.hasFooter(true);
        notificator.switchFooter(true);
        notificator.show();
    }

    if (respObj.statusCode == 212) {
        isAsked();
        notificator.setHeader("Check For Updated Slides");
        notificator.setBody("Would you like to check the library for updates to the slides?");
        notificator.setActionForConfirmYes(function () {
            checkUpdate(isAsk, true);
        });
        notificator.hasFooter(true);
        notificator.switchFooter(true);
        notificator.show();
    }

    if (respObj.statusCode == 213) {
        isAsked();
        notificator.setHeader("Check For Updates");
        notificator.setBody("This presentation is up to date.");
        notificator.hasFooter(true);
        notificator.switchFooter(false);
        notificator.show();
    }

    if (respObj.statusCode == 214) {
        if (!downloadLatestProcess) {
            isAsked();
            notificator.setHeader("Updated Version is Available");
            notificator.setBody("The content in this presentation has been updated in the library.\nWould you like to download the updated version?");
            notificator.setActionForConfirmYes(function () {
                downloadLatest(false);
            });
            notificator.hasFooter(true);
            notificator.switchFooter(true);
            notificator.show();
        }
    }

    if (respObj.statusCode == 215) {
        isAsked();
        notificator.hide();
        canApply = false;

        updateTableUpdated(respObj.value);

        if (canApply) {
            setStatusCheckbox(true);
            $('#apply-all').attr("disabled", false);
        }
    }

    if (respObj.statusCode == 400) {
        isAsked();
        notificator.setHeader("Error");
        notificator.setBody(respObj.statusText);
        notificator.hasFooter(true);
        notificator.switchFooter(false);
        notificator.show();
    }

    hideLoading();
}

function downloadLatest(isApplyAll) {
    showLoading();
    downloadLatestProcess = true;

    var pData = {
        ServerInfo: {
            WebClientUrl: webClientUrl,
            ShortLibraryName: libraryShortName,
            AuthHash: authHash,
            EnvironmentName: environmentName
        },
        FileId: currentFile.id,
        FileName: "Latest " + currentFile.text,
        FileUrl: "",
        AskFirst: false,
        IsConfirm: false,
        IsOverwrite: true,
        IsApplyAll: isApplyAll
    };

    var options = {
        method: "POST",
        postData: JSON.stringify(pData),
        contentType: "application/json"
    };

    xhrHelper.makeCorsRequest(urls.middlewareUrl + "files/getfile", options, function (response) {
        notificator.setHeader("Check For Updated Version");
        notificator.setBody("The latest file is downloaded and store at: " + response.srcElement.response);
        notificator.switchFooter(false);
        notificator.show();
        hideLoading();
    }, function (error) {
        console.log(error);
    });
}

function updateTableUpdated(data) {
    var lstRecords = {
        total: 0,
        records: []
    };

    $.each(data, function (i, v) {
        var item = {
            pptSlideIndex: v.pptSlideIndex,
            status: v.status,
            lastModDate: v.lastModDate,
            source: v.source,
            isSelected: false
        };

        if (v.status != 'No change') {
            canApply = true;
        }

        lstRecords.records.push(item);
    });

    w2ui.grid.records = lstRecords.records;
    w2ui.grid.refresh();
}

function setStatusCheckbox(isCheck) {
    $.each(w2ui.grid.records, function (i, v) {
        v.isSelected = isCheck;
    });
    w2ui.grid.refresh();
}

function isValidFile(successCallback, errorCallback) {
    showLoading();

    var pData = {
        ServerInfo: {
            WebClientUrl: "",
            ShortLibraryName: "",
            AuthHash: "",
            EnvironmentName: ""
        },
        FileId: "",
        FileName: "",
        FileUrl: fileUrl,
        AskFirst: false,
        IsConfirm: false,
        IsOverwrite: false
    };

    var options = {
        method: "POST",
        postData: JSON.stringify(pData),
        contentType: "application/json"
    };

    xhrHelper.makeCorsRequest(urls.middlewareUrl + "files/verifyfile", options, function (response) {
        successCallback(response.srcElement.response);
    }, function (error) {
        hideLoading();
        $("#no-content").show();
    });
}

function isAsk() {
    if (!Office.context.document.settings.get("isAsk")) {
        return true;
    }
    return false;
}

function isAsked() {
    if (!Office.context.document.settings.get("isAsk")) {
        Office.context.document.settings.set("isAsk", "true");
        Office.context.document.settings.saveAsync();
    }
}

function showLoading() {
    notificator.hide();
    $("#overlay").addClass("dark-class");
    $("#loading-indicator").addClass("focus");
    $("#loading-indicator").show();
}

function hideLoading() {
    $("#overlay").removeClass("dark-class");
    $("#loading-indicator").removeClass("focus");
    $("#loading-indicator").hide();
}