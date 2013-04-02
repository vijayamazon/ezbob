﻿var EzBob = EzBob || {};

EzBob.Application = Backbone.Marionette.Application.extend({});

EzBob.App = new EzBob.Application();

EzBob.App.GA = new EzBob.GA();
//EzBob.App.GA = new EzBob.GATest();

EzBob.App.addRegions({
    modal: EzBob.ModalRegion,
    modal2: EzBob.ModalRegion2,
    jqmodal: EzBob.JqModalRegion
});

EzBob.Config = {
    CaptchaMode: 'off',
    WizardTopNaviagtionEnabled: false,
    TargetsEnabled : false,
    GetCashSliderStep: 50,
    ShowChangePasswordPage: false,
    SessionTimeout: 15,
    HeartBeatEnabled: true,
    MinLoan: 1000,
    XMinLoan: 100,
    MaxLoan: 10000,
    PayPalEnabled: false,
    HideAlertsTab: false,
    HidePassedAlertsTab: false,
    PasswordPolicyType: "simple",
    CAISDisplayedPath: ""
}