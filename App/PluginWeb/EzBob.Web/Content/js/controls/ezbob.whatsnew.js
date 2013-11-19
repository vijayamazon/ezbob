/// <reference path="~/Content/js/lib/backbone.js" />
/// <reference path="~/Content/js/lib/underscore.js" />
/// <reference path="~/Content/js/App/ezbob.app.js" />
/// <reference path="~/Content/js/lib/alertify.js" />

var EzBob = EzBob || {};


EzBob.WhatsNewView = Backbone.View.extend({
    initialize: function () {
        this.events = _.extend({}, this.events, {
            "click #btnGotIt": "gotItClicked",
            "click #btnShowLater": "showLaterClicked",
            "click .close": "showLaterClicked"
        });

        var that = this;
        $.getJSON(window.gRootPath + "Customer/WhatsNew/Index")
            .success(function (result) {
                if (result.noWhatsNew || !result.success) {
                    that.clearAll();
                } else {
                    that.whatsNewId = result.whatsNewId;
                    that.showWhatsNew(result.whatsNew);
                }
            });
    },

    showWhatsNew: function (whatsNew) {
        var whatsNewDiv = $('<div style="background-image: url(' + window.gRootPath +'Content/css/images/'+ whatsNew + ');" class="whats-new"></div>');
        scrollTop();
        this.$el.prepend(whatsNewDiv);
        whatsNewDiv.alert();
        this.$el.notification();
        this.$el.show();
    },
    render: function () {
        this.$el.html();
        return this;
    },
    gotItClicked: function () {
        this.save(true);
    },
    showLaterClicked: function () {
        this.save(false);
    },
    save: function(gotIt) {
        this.clearAll();
        $.post("" + window.gRootPath + "Customer/WhatsNew/CustomerSaw", { whatsNewId: this.whatsNewId, gotIt: gotIt });
    },
    clearAll: function () {
        var that = this;
        $(this.$el).animate({
                       "opacity": 0,
                       "min-height": 0
                   }, function () {
                       $(that.$el).hide(500);
                   });
        $(this.$el).fadeOut();
    }
});
