///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/Wizard/yourInfo/ezbob.steps.personinfo.js" />
///<reference path="~/Content/js/Wizard/ezbob.wizard.signupstep.js" />
///<reference path="~/Content/js/Wizard/ezbob.wizard.js" />
///<reference path="~/Content/js/Wizard/accounts/ezbob.accounts.js" />

var EzBob = EzBob || {};

EzBob.SignUpWizard = EzBob.Wizard.extend({
    initialize: function (options) {
        this.customer = options.customer;

        var storeInfoStepModel = new EzBob.StoreInfoStepModel({
            ebayMarketPlaces: options.ebayMarketPlaces,
            amazonMarketPlaces: options.amazonMarketPlaces
        });
        
        var paymentAccountStepModel = new EzBob.PaymentAccountStepModel({ stores: storeInfoStepModel, bankAccountAdded: options.bankAccountAdded, paypalAccounts: options.paypalAccounts, bankAccount: options.BankAccountNumber, sortCode:options.SortCode });
        var yourInformationStepModel = new EzBob.YourInformationStepModel();

        this.steps = [];

        this.signUpStepView = new EzBob.QuickSignUpStepView({ model: this.customer });
        this.signUpStepView.on('ready', this.signed, this);
        this.addStep("&nbsp;&nbsp;quick<br/>sign up", this.signUpStepView);

        this.personInfoView = new EzBob.YourInformationStepView({model: this.customer});
        this.personInfoView.on('ready', this.evaluate, this);

        this.evaluated = false; // if the evaluation strategy has ran

        var stepModels = new EzBob.WizardSteps([this.customer, storeInfoStepModel, paymentAccountStepModel, yourInformationStepModel]);
        var model = new EzBob.WizardModel({ stepModels: stepModels, total: stepModels.length, allowed: 3 });

        this.storeView = new EzBob.StoreInfoStepView({ model: storeInfoStepModel });
        this.paymentAccountsView = new EzBob.PaymentAccountStepView({ model: paymentAccountStepModel });

        this.addStep("&nbsp;&nbsp;your<br/>shop info", this.storeView);
        this.addStep("&nbsp;&nbsp;payment<br/>accounts", this.paymentAccountsView);
        this.addStep("&nbsp;&nbsp;your<br/>information", this.personInfoView);

        this.constructor.__super__.initialize.apply(this, [{ model: model, steps: this.steps}]);
    },
    signed: function () {
    },
    evaluate: function () {
        $(document).attr("title", "Wizard Complete: Welcome to EZBOB | EZBOB");
        if (this.evaluated) return;
        this.evaluated = true;
        EzBob.App.GA.trackPage('/Customer/Wizard/Success');
    }
});

EzBob.PaymentAccountStepModel = EzBob.WizardStepModel.extend({
    defaults: {
        bankAccountAdded: false
    }
});

EzBob.PaymentAccountStepView = Backbone.View.extend({
    initialize: function () {
        this.accounts = new EzBob.AccountsView({ model: this.model });
        this.accounts.on('ready', this.ready, this);
        this.accounts.on('next', this.next, this);
        this.accounts.on('previous', this.previous, this);
    },
    render: function () {
        this.accounts.render().$el.appendTo(this.$el);
        return this;
    },
    ready: function (name) {
        this.trigger('ready');
    },
    next: function () {
        this.trigger('next');
    },
    previous: function () {
        this.trigger('previous');
    }
});


