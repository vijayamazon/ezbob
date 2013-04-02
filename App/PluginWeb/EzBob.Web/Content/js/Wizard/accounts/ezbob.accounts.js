///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/Wizard/accounts/ezbob.accounts.paypal.js" />
///<reference path="~/Content/js/Wizard/accounts/ezbob.accounts.bank.js" />
///<reference path="~/Content/js/Wizard/ezbob.storebase.js" />
var EzBob = EzBob || {};


EzBob.AccountsView = EzBob.StoreInfoBaseView.extend({
    initialize: function () {

        this.isRendered = false;

        this.ebayStores = this.model.get("stores").get("ebayStores");

        this.ebayStores.on('fetch reset change', this.accountChanged, this);

        this.payPalAccounts = new EzBob.PayPalAccounts(this.model.get("paypalAccounts"));

        this.payPalAccounts.on('fetch reset change', this.accountChanged, this);
        this.PayPalButtonView = new EzBob.PayPalButtonView({ model: this.payPalAccounts });
        this.PayPalInfoView = new EzBob.PayPalInfoView();

        this.PayPalButtonView.disabled = !EzBob.Config.PayPalEnabled;

        this.model.on('change:bankAccountAdded', this.accountChanged, this);

        this.BankAccountButtonView = new EzBob.BankAccountButtonView({ model: this.model });
        this.BankAccountInfoView = new EzBob.BankAccountInfoView({ model: this.model });

        this.stores = {
            "paypal": { "view": this.PayPalInfoView, "button": this.PayPalButtonView },
            "bank-account": { "view": this.BankAccountInfoView, "button": this.BankAccountButtonView }
        };

        this.name = "accounts";

        this.PayPalInfoView.on('ready', this.PPready, this);
        this.PayPalInfoView.on('next', this.PPnext, this);
        EzBob.StoreInfoBaseView.prototype.initialize.apply(this);
    },
    PPready: function () {
        //this.PayPalButtonView.trigger('ready', 'paypal');
        //this.trigger('ready', 'paypal');
    },
    PPnext: function () {
        //this.PayPalButtonView.trigger('next', 'paypal');
    },
    render: function () {
        this.constructor.__super__.render.call(this);
        this.isRendered = true;
        this.$el.find(".store-add-page").html("<h2>Select your payment account</h2>");
        this.$el.find('.wizard-top-notification.stores-top-notification h1').html("&nbsp;Payment Accounts:");
        this.$el.find('.wizard-top-notification.stores-top-notification h2').text("So we can send you cash!");
        this.$el.find(".help-btn").attr("data-content", "Click on the bank account box  so we can transfer funds to you, and click on the PayPal account to be qualified for a loan increase. Currently PayPal is not available for transferring funds, but will be in the near future.");
        this.$el.find(".help-btn").attr("data-original-title", "Payment account");
        this.$el.find(".sidebar-container").html($('#payment-accounts-securitystatement').html());
        
        this.$el.find('img[rel]').setPopover();
        this.$el.find('li[rel]').setPopover("left");
        
        this.accountChanged();
        return this;
    },
    accountChanged: function () {
        if (!this.isRendered) return;

        if (!this.checkAccounts()) return;

        if (!this.model.get("bankAccountAdded")) {
            this.$el.find(".store-add-page").hide();
            return;
        }

        /*
        if (this.payPalAccounts.length == 0 && !this.model.get("bankAccountAdded")) {
        //this.$el.find('.wizard-top-notification.stores-top-notification h2').text(EzBob.dbStrings.NoPayPalOrBankAccount);
        return;
        }
        if (!this.model.get("bankAccountAdded")) {
        //this.$el.find('.wizard-top-notification.stores-top-notificatio h2').text(EzBob.dbStrings.NoBankAccount);
        return;
        }
        if (this.ebayStores.length > 0 && this.payPalAccounts.length == 0) {
        //this.$el.find('.wizard-top-notification.stores-top-notification h2').text(EzBob.dbStrings.NoPayPalAccount);
        return;
        }
        
        */
        //this.$el.find('.wizard-top-notification.stores-top-notification h2').text(EzBob.dbStrings.AccountsAdded);
        this.$el.find(".store-add-page").html("<h2>You can enter another account to get more cash</h2>");
        //        this.constructor.__super__.ready.call(this);
        //        this.trigger('ready');
        // this.ready();
    },
    ready: function () {

        if (!this.checkAccounts()) return;

        var accountAdded = this.model.get("bankAccountAdded");
        if (!accountAdded) return;
        this.constructor.__super__.ready.call(this);
    },
    checkAccounts: function () {
        var bankAdded = this.model.get("bankAccountAdded");

        //enters paypal account, but not bank account
        if (this.payPalAccounts.length > 0 && !bankAdded) {
            this.$el.find(".store-add-page").html("<h2>Please enter your bank account details</h2>");
            return false;
        }

        //bank account, ebay store, no paypal
        if (bankAdded && this.ebayStores.length > 0 && this.payPalAccounts.length == 0) {
            this.$el.find(".store-add-page").html("<h2>Please add your PayPal account to get more cash</h2>");
            return true;
        }

        //bank account, no ebay store, no paypal
        if (bankAdded && this.ebayStores.length == 0 && this.payPalAccounts.length == 0) {
            this.$el.find(".store-add-page").html("<h2>Please add your PayPal account to get more cash</h2>");
            return true;
        }
        return bankAdded;
    }
});