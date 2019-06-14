var dialog;

function dialogCallback(asyncResult) {
    if (asyncResult.status == "failed") {
        switch (asyncResult.error.code) {
            case 12004:
                console.log("Domain is not trusted");
                break;
            case 12005:
                console.log("HTTPS is required");
                break;
            case 12007:
                console.log("A dialog is already opened.");
                break;
            default:
                console.log(asyncResult.error.message);
                break;
        }
    }
    else {
        dialog = asyncResult.value;
        dialog.addEventHandler(Office.EventType.DialogMessageReceived, messageHandler);
        dialog.addEventHandler(Office.EventType.DialogEventReceived, eventHandler);
    }
}


function messageHandler(arg) {
    if (arg.message.indexOf("userInfo") > -1) {
        var data = JSON.parse(arg.message);
        if (data.close) {
            dialog.close();
            showPanel(data.userInfo);
        }
        else {
            storeData(data.userInfo);
        }
    }
    else {
        if (arg.message === "closeDialog") {
            dialog.close();
        }
    }
}

function eventHandler(arg) {
    switch (arg.error) {
        case 12002:
            console.log("Cannot load URL, no such page or bad URL syntax.");
            break;
        case 12003:
            console.log("HTTPS is required.");
            break;
        case 12006:
            console.log("Dialog closed by user");
            break;
        default:
            console.log("Undefined error in dialog window");
            break;
    }
}

function dialogCloseAsync(dialog, asyncResult) {
    dialog.close();
    setTimeout(function () {
        try {
            dialog.addEventHandler(Office.EventType.DialogMessageReceived, function () { });
            dialogCloseAsync(dialog, asyncResult);
        } catch (e) {
            asyncResult();
        }
    }, 0);
}

function openDialog(url, w, h) {
    Office.context.ui.displayDialogAsync(url,
        { height: h, width: w }, dialogCallback);
}

function calculateDialogHeight() {
    var height = window.innerHeight;
    var dialogHeight = height * 2 / 3;
    var ratioHeight = dialogHeight / height * 100;
    return ratioHeight;
}