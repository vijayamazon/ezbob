///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/App/ezbob.app.js" />

var EzBob = EzBob || {};


EzBob.NotificationsView = Backbone.View.extend({
    initialize: function () {
        EzBob.App.on("info", this.info, this);
        EzBob.App.on("error", this.error, this);
        EzBob.App.on("warning", this.warning, this);
        EzBob.App.on("clear", this.clearAll, this);
    },
    info: function (msg) {
        this.makeAlert(msg, 'notification_green effect_fadein');
    },
    error: function (msg) {
        this.makeAlert(msg, 'notification_red effect_fadein');
    },
    warning: function (msg) {
        this.makeAlert(msg, 'notification_yellow effect_fadein');
    },
    makeAlert: function (msg, alertClass) {
        if (!msg) return;
        var alert = $('<div class="' + alertClass + '"> <strong>' + msg + ' </strong></div>');
        scrollTop();
        this.$el.html(alert);
        alert.alert();
        this.$el.notification();
    },
    render: function () {
        this.$el.html();
        return this;
    },
    clearAll: function () {
        this.$el.html("");
    }
});


(function() {
    EzBob.App.on("info", info);
    EzBob.App.on("error", error);
    EzBob.App.on("warning", warning);
    EzBob.App.on("clear", clear);

    function info(text) {
        EzBob.CT.recordEvent("info", text);
    }
    
    function error(text) {
        EzBob.CT.recordEvent("error", text);
    }
    
    function warning(text) {
        EzBob.CT.recordEvent("warning", text);
    }
    
    function clear() {
        EzBob.CT.recordEvent("clear");
    }
})();