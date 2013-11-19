///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/lib/backbone.js" />

var EzBob = EzBob || {};

EzBob.PayPalAccountModel = Backbone.Model.extend({
    defaults: {
        displayName: null
    }
});

EzBob.PayPalAccounts = Backbone.Collection.extend({
    model: EzBob.PayPalAccountModel,
    url: window.gRootPath + 'Customer/PaymentAccounts/PayPalList'
});

EzBob.PayPalInfoView = Backbone.View.extend({
    events: {
        'click a.back': 'back'
    },
    initialize: function () {
        var that = this;

        EzBob.CT.bindShopToCT(this, 'paypal');

        window.PayPalAdded = function (result) {
            if (result) {
                EzBob.App.trigger('info', 'Congratulations. PayPal account was added successfully.');
			} 
			$.colorbox.close();
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');
        };

        window.PayPalAddingError = function (msg) {
            EzBob.App.trigger('error', msg);
			$.colorbox.close();
            that.trigger('completed');
            that.trigger('back');
        };

    },
    // signedIn: function () { this.bindToButton(); },
    back: function () {
        this.trigger('back');
        return false;
    },
    getDocumentTitle: function() {
        return "Link PayPal Account";
    }
});
