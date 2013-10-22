///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
///<reference path="~/Content/js/Wizard/yourInfo/ezbob.steps.personinfo.js" />
///<reference path="~/Content/js/Wizard/ezbob.wizard.signupstep.js" />
///<reference path="~/Content/js/Wizard/ezbob.wizard.js" />
///<reference path="~/Content/js/Wizard/accounts/ezbob.accounts.js" />

var EzBob = EzBob || {};

function HeartOfActivity() {

    var sessionTimeout = EzBob.Config.SessionTimeout;
    if (sessionTimeout <= 0) return;

    if (EzBob.Config.HeartBeatEnabled) var heartInterval = setInterval(heartBeat, 5000);
    
    var timer;
    var timeoutValue = 1000 * 60 * sessionTimeout;

    set();

    function heartBeat() {
        $.get(window.gRootPath + "HeartBeat");
    }

    function timeout() {
        reset();
        document.location = window.gRootPath + "Account/LogOff";
    }

    function reset() {
        clearInterval(heartInterval);
        clearTimeout(timer);
    }

    function set() {
        timer = setTimeout(timeout, timeoutValue);
    }
}




EzBob.SignUpWizard = EzBob.Wizard.extend({
	initialize: function (options) {
		this.customer = options.customer;

		var modelArgs = {
			ebayMarketPlaces: options.ebayMarketPlaces,
			amazonMarketPlaces: options.amazonMarketPlaces,
			ekmMarketPlaces: options.ekmMarketPlaces,
			freeagentMarketPlaces: options.freeagentMarketPlaces,
			sageMarketPlaces: options.sageMarketPlaces,
			yodleeAccounts: options.yodleeAccounts,
			paypointMarketPlaces: options.paypointMarketPlaces
		};

		if ((this.customer.get('Id')) != 0) {
		    HeartOfActivity();
		}

		var cgShops = options.cgShops;

		for (var i in cgShops) {
			if (!cgShops.hasOwnProperty(i))
				continue;

			var o = cgShops[i];

			if (!modelArgs[o.storeInfoStepModelShops])
				modelArgs[o.storeInfoStepModelShops] = [];

			modelArgs[o.storeInfoStepModelShops].push(o);
		} // for each cg shop

		function getCookie(c_name) {
			var c_value = document.cookie;
			var c_start = c_value.indexOf(" " + c_name + "=");
			if (c_start == -1) {
				c_start = c_value.indexOf(c_name + "=");
			}
			if (c_start == -1) {
				c_value = null;
			}
			else {
				c_start = c_value.indexOf("=", c_start) + 1;
				var c_end = c_value.indexOf(";", c_start);
				if (c_end == -1) {
					c_end = c_value.length;
				}
				c_value = unescape(c_value.substring(c_start, c_end));
			}
			return c_value;
		} // getCookie

		if (!this.customer.get('IsOffline') && (getCookie('isoffline') == 'yes'))
			this.customer.set('IsOffline', true);

		modelArgs.isOffline = this.customer.get('IsOffline');
	    modelArgs.IsProfile = false;
		var storeInfoStepModel = new EzBob.StoreInfoStepModel(modelArgs);

		var yourInformationStepModel = new EzBob.YourInformationStepModel();

		this.steps = [];

		this.signUpStepView = new EzBob.QuickSignUpStepView({ model: this.customer });
		this.signUpStepView.on('ready', this.signed, this);
		this.addStep("Create an Account", this.signUpStepView);

		this.personInfoView = new EzBob.YourInformationStepView({ model: this.customer });
		this.personInfoView.on('ready', this.evaluate, this);

		this.evaluated = false; // if the evaluation strategy has ran

		var stepModels = new EzBob.WizardSteps([this.customer, storeInfoStepModel, yourInformationStepModel]);
		var model = new EzBob.WizardModel({ stepModels: stepModels, total: stepModels.length, allowed: 3 });

		this.storeView = new EzBob.StoreInfoStepView({ model: storeInfoStepModel });
		this.storeView.on('ready', this.signed, this);

		this.addStep("Link Accounts", this.storeView);
		this.addStep("Enter Information", this.personInfoView);

		this.constructor.__super__.initialize.apply(this, [{ model: model, steps: this.steps }]);
	},
	signed: function () {
		this.storeView.StoreInfoView.YodleeAccountInfoView.loadBanks();
	},
	evaluate: function () {
		$(document).attr("title", "Wizard Complete: Welcome to EZBOB | EZBOB");
		if (this.evaluated) return;
		this.evaluated = true;
		EzBob.App.GA.trackPage('/Customer/Wizard/Success');
	}
});

