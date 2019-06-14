'use strict';

var notificator = {
    init: function () {
        notificator.hide();

        $("#btn-confirm-no").click(function () {
            notificator.hide();
        });

        $("#btn-ok").click(function () {
            notificator.hide();
        });
    },
    hasFooter: function (val) {
        if (val) {
            $(".notification-footer").show();
        }
        else {
            $(".notification-footer").hide();
        }
    },
    show: function () {
        $(".notificaton-panel").show();
    },
    hide: function () {
        $(".notificaton-panel").hide();
    },
    setHeader: function (value) {
        $("#notification-header-title").text(value);
    },
    setBody: function (value) {
        $("#notification-body-content").text(value);
    },
    setActionForConfirmYes: function (callback) {
        if (callback) {
            $("#btn-confirm-yes").click(function () {
                callback();
            });
        }
    },
    switchFooter: function (isConfirm) {
        if (isConfirm) {
            $("#footer-confirm").show();
            $("#footer-alert").hide();
        }
        else {
            $("#footer-confirm").hide();
            $("#footer-alert").show();
        }
    }
}







