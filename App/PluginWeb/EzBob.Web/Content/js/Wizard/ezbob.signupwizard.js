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
            amazonMarketPlaces: options.amazonMarketPlaces,
            ekmMarketPlaces: options.ekmMarketPlaces,
            yodleeAccounts: options.yodleeAccounts,
            volusionMarketPlaces: options.volusionMarketPlaces,
            playMarketPlaces: options.playMarketPlaces,
            paypointMarketPlaces: options.paypointMarketPlaces
        });

        var yourInformationStepModel = new EzBob.YourInformationStepModel();

        this.steps = [];

        this.signUpStepView = new EzBob.QuickSignUpStepView({ model: this.customer });
        this.signUpStepView.on('ready', this.signed, this);
        this.addStep("Create an Account", this.signUpStepView);

        this.personInfoView = new EzBob.YourInformationStepView({model: this.customer});
        this.personInfoView.on('ready', this.evaluate, this);

        this.evaluated = false; // if the evaluation strategy has ran

        var stepModels = new EzBob.WizardSteps([this.customer, storeInfoStepModel, yourInformationStepModel]);
        var model = new EzBob.WizardModel({ stepModels: stepModels, total: stepModels.length, allowed: 3 });

        this.storeView = new EzBob.StoreInfoStepView({ model: storeInfoStepModel });
        this.storeView.on('ready', this.signed, this);
        
        this.addStep("Link Accounts", this.storeView);
        this.addStep("Enter Information", this.personInfoView);

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

