var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

EzBob.Underwriter.Settings.ExperianModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsExperian"
});

EzBob.Underwriter.Settings.ExperianView = Backbone.Marionette.ItemView.extend({
	template: "#experian-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	bindings: {
		FinancialAccounts_MainApplicant: "[name='FinancialAccounts_MainApplicant']",
		FinancialAccounts_AliasOfMainApplicant: "[name='FinancialAccounts_AliasOfMainApplicant']",
		FinancialAccounts_AssociationOfMainApplicant: "[name='FinancialAccounts_AssociationOfMainApplicant']",
		FinancialAccounts_JointApplicant: "[name='FinancialAccounts_JointApplicant']",
		FinancialAccounts_AliasOfJointApplicant: "[name='FinancialAccounts_AliasOfJointApplicant']",
		FinancialAccounts_AssociationOfJointApplicant: "[name='FinancialAccounts_AssociationOfJointApplicant']",
		FinancialAccounts_No_Match: "[name='FinancialAccounts_No_Match']"
	},
	events: {
		"click #SaveExperianSettings": "saveSettings",
		"click #CancelExperianSettings": "cancelSettings"
	},
	saveSettings: function () {
		BlockUi("on");
		this.model.save().done(function () {
			return EzBob.ShowMessage("Saved successfully", "Successful");
		});
		this.model.save().complete(function () {
			return BlockUi("off");
		});
		return false;
	},
	cancelSettings: function () {
		this.update();
		return false;
	},
	update: function () {
		this.model.fetch({ reset: true });
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
		}
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	},
	onClose: function () {
		return this.modelBinder.unbind();
	}
});
