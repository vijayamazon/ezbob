var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.PaymentAccountModel = Backbone.Model.extend({
    defaults: {
        displayName: null,
        type: "PayPal"
    }
});

EzBob.PaymentAccounts = Backbone.Collection.extend({
    model: EzBob.PaymentAccountModel
});

EzBob.Profile.PaymentAccountsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#payment-accounts-template').html());

        this.model.on('change:paypalAccounts', this.render, this);
    },
    render: function () {
        var accounts = new EzBob.PaymentAccounts(this.model.get('paypalAccounts'));
        if (this.model.get('BankAccountNumber')) {
            accounts.add({ displayName: this.model.get('BankAccountNumber'), type: "Bank" });
        }
        this.$el.html(this.template({ accounts: accounts.toJSON() }));
        return this;
    },
    events: {
        'click .add-paypal': 'addPayPal'
    },
    addPayPal: function () {
        //this.$el.empty();
        if (!EzBob.Config.PayPalEnabled) {
            EzBob.ShowMessage("The PayPal service is temporary offline. Contact us for support.");
            return false;
        }

        var payPalInfoView = new EzBob.PayPalInfoView({ el: this.$el });
        payPalInfoView.on('ready', this.paypalAdded, this);
        payPalInfoView.on('back', this.render, this);
        payPalInfoView.render();
        return false;
    },
    paypalAdded: function () {
        this.model.fetch();
        this.render();
    }
});