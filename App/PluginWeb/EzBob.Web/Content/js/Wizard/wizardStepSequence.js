var EzBob = EzBob || {};

EzBob.WizardStepInfo = function(basics, nPosition, oProgressAndType, oViewList) {
	if (!EzBob.WizardStepInfo.prototype.onFocus) {
		EzBob.WizardStepInfo.prototype.onFocus = function() {};
	} // if

	var nProgress = 0;
	var nType = 0;

	if (oProgressAndType === 0) {
		nProgress = 7;
		nType = 1;
	}
	else if (oProgressAndType === 100) { //todo remove
		nProgress = 0;
		nType = 4;
	}
	else {
		nProgress = oProgressAndType.position;
		nType = oProgressAndType.type;
	}

	for (var sProp in basics)
		this[sProp] = basics[sProp];

	this.num      = nPosition;
	this.progress = nProgress;
	this.type     = nType;

	this.ready    = false;

	this.view = (oViewList && oViewList[this.name]) ? oViewList[this.name] : null;
}; // EzBob.WizardStepInfo

EzBob.WizardStepSequence = function(args) {
	this.online = [];
	this.offline = [];

	var oSignupStep = {
		name: 'signup',
		documentTitle: 'Register account',
		title: 'create',
		trackPage: '/Customer/Wizard/SignUp',
		marketingStrKey: 'MarketingWizardStepSignup',
		showTitleInWizardSteps: true,
	};

	var oShopInfoStep = {
		name: 'link',
		documentTitle: 'Link accounts',
		title: 'accounts',
		trackPage: '/Customer/Wizard/Shops',
		marketingStrKey: 'MarketingWizardStepLinkAccounts',
		showTitleInWizardSteps: true,

		onFocus: function() { this.view.StoreInfoView.back(); },
	};

	var oYourDetailsStep = {
		name: 'details',
		documentTitle: 'Personal information',
		title: 'personal',
		trackPage: '/Customer/Wizard/PersonalDetails',
		marketingStrKey: 'MarketingWizardStepPersonalInfo',
		showTitleInWizardSteps: true,
	};

	var oCompanyDetailsStep = {
		name: 'companydetails',
		documentTitle: 'Business information',
		title: 'company',
		trackPage: '/Customer/Wizard/CompanyDetails',
		marketingStrKey: 'MarketingWizardStepPersonalInfo',
		showTitleInWizardSteps: true,
	};

	var lst = {};
	lst[oShopInfoStep.name]       = oShopInfoStep;
	lst[oYourDetailsStep.name]    = oYourDetailsStep;
	lst[oCompanyDetailsStep.name] = oCompanyDetailsStep;

	var oCfg = JSON.parse($('#wizard-step-sequence').text());

	var self = this;

	function PushSteps(sConfiguration) {
		var oWizard = self[sConfiguration];
		var oConfig = oCfg[sConfiguration];

		var ary = [];

		for (var sName in oConfig)
			if (lst[sName])
				ary.push(sName);

		ary.sort(function(a, b) { return -(oConfig[a].position - oConfig[b].position); });

		for (var i = 0; i < ary.length; i++)
			oWizard.push(new EzBob.WizardStepInfo(lst[ary[i]], oWizard.length, oConfig[ary[i]], args.views));
	} // PushSteps

	this.online.push(new EzBob.WizardStepInfo(oSignupStep,  this.online.length,   0, args.views));
	PushSteps('online');

	this.offline.push(new EzBob.WizardStepInfo(oSignupStep,  this.offline.length,   0, args.views));
	PushSteps('offline');
}; // EzBob.WizardStepSequence
