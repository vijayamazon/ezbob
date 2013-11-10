var EzBob = EzBob || {};

EzBob.WizardStepInfo = function(basics, nPosition, oProgressAndType, oViewList) {
	if (!EzBob.WizardStepInfo.prototype.onFocus) {
		EzBob.WizardStepInfo.prototype.onFocus = function() {};
		EzBob.WizardStepInfo.prototype.onInStepProgressChanged = function(inStepSectionID, nextStepProgress) {};
	} // if

	var nProgress = 0;
	var nType = 0;

	if (oProgressAndType === 0) {
		nProgress = 0;
		nType = 1;
	}
	else if (oProgressAndType === 100) {
		nProgress = 100;
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
		documentTitle: 'Create an EZBOB account',
		title: 'Create an account',
		trackPage: '/Customer/Wizard/SignUp',
		marketingStrKey: 'MarketingWizardStepSignup',
		showTitleInWizardSteps: true,
	};

	var oShopInfoStep = {
		name: 'link',
		documentTitle: 'Link your accounts',
		title: 'Link accounts',
		trackPage: '/Customer/Wizard/Shops',
		marketingStrKey: 'MarketingWizardStepLinkAccounts',
		showTitleInWizardSteps: true,

		onFocus: function() { this.view.StoreInfoView.back(); },
	};

	var oYourDetailsStep = {
		name: 'details',
		documentTitle: 'Fill personal details',
		title: 'Enter information',
		trackPage: '/Customer/Wizard/PersonalDetails',
		marketingStrKey: 'MarketingWizardStepPersonalInfo',
		showTitleInWizardSteps: true,

		onInStepProgressChanged: function(inStepSectionID, nextStepProgress) {
			var nProgress = this.progress;

			if ((inStepSectionID != 0) && (this.progress + 10 < nextStepProgress))
				nProgress = 10 * parseInt(parseFloat(nextStepProgress - 10) / 10.0);

			EzBob.App.trigger('wizard:progress', nProgress);
		}, // onInStepProgressChanged
	};

	var oSuccessStep = {
		name: 'success',
		documentTitle: 'Wizard complete: welcome to EZBOB',
		title: 'Complete',
		trackPage: '/Customer/Wizard/Success',
		marketingStrKey: 'MarketingWizardStepDone',
		showTitleInWizardSteps: false,
	};

	var lst = {};
	lst[oShopInfoStep.name]    = oShopInfoStep;
	lst[oYourDetailsStep.name] = oYourDetailsStep;

	var oCfg = JSON.parse($('#wizard-step-sequence').text());

	var self = this;

	function PushSteps(sConfiguration) {
		var oWizard = self[sConfiguration];
		var oConfig = oCfg[sConfiguration];

		var ary = [];

		for (var sName in oConfig)
			if (lst[sName])
				ary.push(sName);

		ary.sort(function(a, b) { return oConfig[a].position - oConfig[b].position; });

		for (var i = 0; i < ary.length; i++)
			oWizard.push(new EzBob.WizardStepInfo(lst[ary[i]], oWizard.length, oConfig[ary[i]], args.views));
	} // PushSteps

	this.online.push(new EzBob.WizardStepInfo(oSignupStep,  this.online.length,   0, args.views));
	PushSteps('online');
	this.online.push(new EzBob.WizardStepInfo(oSuccessStep, this.online.length, 100, args.views));

	this.offline.push(new EzBob.WizardStepInfo(oSignupStep,  this.offline.length,   0, args.views));
	PushSteps('offline');
	this.offline.push(new EzBob.WizardStepInfo(oSuccessStep, this.offline.length, 100, args.views));
}; // EzBob.WizardStepSequence
