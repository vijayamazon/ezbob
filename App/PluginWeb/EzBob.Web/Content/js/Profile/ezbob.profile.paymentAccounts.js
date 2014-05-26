﻿var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.PaymentAccountModel = Backbone.Model.extend();

EzBob.PaymentAccounts = Backbone.Collection.extend({
    model: EzBob.PaymentAccountModel
});

EzBob.Profile.PaymentAccountsView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#payment-accounts-template').html());
        this.model.on("change", this.render, this);
        var that = this;
        window.DebitCardAdded = function(result) {
            if (result && result.error) {
                EzBob.App.trigger('error', result.error);
            } else {
                EzBob.App.trigger('info', 'Debit card was added successfully.');
                $.colorbox.close();
                that.model.fetch();
            }
        };
    },
    render: function () {
        var accounts = new EzBob.PaymentAccounts();
        if (this.model.get('BankAccountNumber')) {
            accounts.add({ displayName: this.model.get('BankAccountNumber'), type: "Bank" });
        }
        
        if (this.model.get('PayPointCards')) {
            _.each(this.model.get('PayPointCards'), function (card) {
                accounts.add({ displayName: card.CardNo, type: "Card", isDefault: card.IsDefault, expire: moment(card.ExpireDate).format("MM/YY"), cardId: card.Id });
            });
        }

        this.$el.html(this.template({ accounts: accounts.toJSON() }));
        return this;
    },
    
    events: {
        "click .setDefaultCard": "setDefaultCard",
        "click .addDebitCardHelp": "addDebitCardHelp"
    },
    
    setDefaultCard: function(e) {
        var cardId = $(e.currentTarget).data("card-id");
        BlockUi("on");
        var xhr = $.post(window.gRootPath + "Customer/Profile/SetDefaultCard", { cardId: cardId });
        var that = this;
        xhr.done(function (res) {
            if (res.error) {
                EzBob.App.trigger("error", res.error);
                return;
            }
            that.model.fetch();
            return;
        });
        
        xhr.always(function() {
            BlockUi("off");
        });
    },
    
    addDebitCardHelp: function() {
        this.$el.find('.addDebitCardHelp').colorbox({ href: "#add_debit_card_help", inline: true, open: true });
    }
});