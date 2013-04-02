///<reference path="~/Content/js/Wizard/ezbob.storebutton.js" />
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

EzBob.PayPalButtonView = EzBob.StoreButtonWithListView.extend({
    initialize: function () {
        this.listView = new EzBob.StoreListView({ model: this.model });
        this.constructor.__super__.initialize.apply(this, [{ name: "paypal", logoText: "Add account to get more cash"}]);  
    },
    update: function () {
        this.model.fetch();
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
            EzBob.App.trigger('info', 'Congratulations. Your PayPal account was added successfully.');
            that.trigger('completed');
            that.trigger('ready');
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
