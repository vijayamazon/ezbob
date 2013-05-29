var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.PaymentAccountModel = Backbone.Model.extend({
    defaults: {
        displayName: null,
        type: "Bank"
    }
});

EzBob.PaymentAccounts = Backbone.Collection.extend({
    model: EzBob.PaymentAccountModel
});

EzBob.Profile.PaymentAccountsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#payment-accounts-template').html());
    },
    render: function () {
        var accounts = new EzBob.PaymentAccounts({ displayName: this.model.get('BankAccountNumber'), type: "Bank" });
        this.$el.html(this.template({ accounts: accounts.toJSON() }));
        return this;
    }
});