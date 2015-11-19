var EzBob = EzBob || {};

EzBob.Application = Backbone.Marionette.Application.extend({});

EzBob.App = new EzBob.Application();

if (EzBob.GA)
	EzBob.App.GA = new EzBob.GA();

if (EzBob.Iovation)
    EzBob.App.Iovation = new EzBob.Iovation();


EzBob.App.addRegions({
	modal: EzBob.ModalRegion,
	jqmodal: EzBob.JqModalRegion
});

EzBob.Config = {
	CaptchaMode: 'off',
	WizardTopNaviagtionEnabled: false,
	TargetsEnabled: true,
	TargetsEnabledEntrepreneur: true,
	GetCashSliderStep: 50,
	ShowChangePasswordPage: false,
	SessionTimeout: 6000,
	HeartBeatEnabled: true,
	MinLoan: 1000,
	XMinLoan: 100,
	NumofAllowedActiveLoans: 2,
	MaxLoan: 10000,
	PayPalEnabled: false,
	PasswordPolicyType: 'simple',
	isTest: false,
	WizardAutomationTimeout: 20
};