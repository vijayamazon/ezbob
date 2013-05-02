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

EzBob.PayPalButtonView = EzBob.StoreButtonView.extend({
    initialize: function () {
        this.constructor.__super__.initialize.apply(this, [{ name: "paypal", logoText: "Add account to get more cash", shops: this.model }]);  
    },
    update: function () {
        this.model.fetch().done(function () { EzBob.App.trigger("ct:storebase.shop.connected"); });
    },
    disabledText: "The PayPal service is temporary offline. Contact us for support."
});

EzBob.PayPalInfoView = Backbone.View.extend({
    events: {
        'click a.back': 'back'
    },
    initialize: function () {
        var that = this;
        
        EzBob.CT.bindShopToCT(this, 'paypal');
        this.template = _.template($('#paypal-instructions').html());
        
        window.PayPalAdded = function (result) {
            EzBob.App.trigger('info', 'Congratulations. PayPal account was added successfully.');
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');
        };

        window.PayPalAddingError = function (msg) {
            EzBob.App.trigger('error', msg);
            that.trigger('back');
        };

    },
    signedIn: function () {
        this.bindToButton();
    },
    render: function () {
        this.$el.html(this.template());
        return this;
    },
    back: function () {
        this.trigger('back');
        return false;
    }
});
