var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.PaymentAccountModel = Backbone.Model.extend();

EzBob.PaymentAccounts = Backbone.Collection.extend({
    model: EzBob.PaymentAccountModel
});

EzBob.Profile.PaymentAccountsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#payment-accounts-template').html());

        this.model.on('change:paypalAccounts', this.render, this);
    },
    render: function () {
        var accounts = new EzBob.PaymentAccounts();
        if (this.model.get('BankAccountNumber')) {
            accounts.add({ displayName: this.model.get('BankAccountNumber'), type: "Bank" });
        }
        this.$el.html(this.template({ accounts: accounts.toJSON() }));
        return this;
    }
});