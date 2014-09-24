var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsRejectionModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/AutomationRejection"
});

EzBob.Underwriter.SettingsRejectionView = Backbone.Marionette.ItemView.extend({
	template: "#rejection-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change ", this.render, this);
		this.update();
		return this;
	},
	bindings: {
		EnableAutomaticRejection: "select[name='EnableAutomaticRejection']",
		LowCreditScore: "input[name='LowCreditScore']",
		TotalAnnualTurnover: "input[name='TotalAnnualTurnover']",
		TotalThreeMonthTurnover: "input[name='TotalThreeMonthTurnover']",
		Reject_Defaults_CreditScore: "input[name='Reject_Defaults_CreditScore']",
		Reject_Defaults_AccountsNum: "input[name='Reject_Defaults_AccountsNum']",
		Reject_Defaults_Amount: "input[name='Reject_Defaults_Amount']",
		Reject_Defaults_MonthsNum: "input[name='Reject_Defaults_MonthsNum']",
		Reject_Minimal_Seniority: "input[name='Reject_Minimal_Seniority']",
		EnableAutomaticReRejection: "select[name='EnableAutomaticReRejection']",
		AutoRejectionException_CreditScore: "input[name='AutoRejectionException_CreditScore']",
		AutoRejectionException_AnualTurnover: "input[name='AutoRejectionException_AnualTurnover']",
		Reject_LowOfflineAnnualRevenue: "input[name='Reject_LowOfflineAnnualRevenue']",
		Reject_LowOfflineQuarterRevenue: "input[name='Reject_LowOfflineQuarterRevenue']",
		Reject_LateLastMonthsNum: "input[name='Reject_LateLastMonthsNum']",
		Reject_NumOfLateAccounts: "input[name='Reject_NumOfLateAccounts']",
		RejectionLastValidLate: "select[name='RejectionLastValidLate']",
		RejectionCompanyScore: "input[name='RejectionCompanyScore']",
		RejectionExceptionMaxConsumerScoreForMpError: "input[name='RejectionExceptionMaxConsumerScoreForMpError']",
		RejectionExceptionMaxCompanyScoreForMpError: "input[name='RejectionExceptionMaxCompanyScoreForMpError']",
		RejectionExceptionMaxCompanyScore: "input[name='RejectionExceptionMaxCompanyScore']",
		Reject_Defaults_CompanyScore: "input[name='Reject_Defaults_CompanyScore']",
		Reject_Defaults_CompanyAccountsNum: "input[name='Reject_Defaults_CompanyAccountsNum']",
		Reject_Defaults_CompanyMonthsNum: "input[name='Reject_Defaults_CompanyMonthsNum']",
		Reject_Defaults_CompanyAmount: "input[name='Reject_Defaults_CompanyAmount']"
	},
	events: {
		"click button[name='SaveRejectionSettings']": "saveSettings",
		"click button[name='CancelRejectionSettings']": "cancelSettings"
	},
	saveSettings: function () {
		if (!this.validator.form()) {
			return false;
		}
		BlockUi("on");
		this.model.save().done(function () {
			return EzBob.ShowMessage("Saved successfully", "Successful");
		});
		this.model.save().complete(function () {
			return BlockUi("off");
		});
		return false;
	},
	update: function () {
		return this.model.fetch();
	},
	cancelSettings: function () {
		this.update();
		return false;
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("select, input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button").hide();
		}
		this.setValidator();
	},
	show: function (type) {
		this.$el.show();
	},
	hide: function () {
		this.$el.hide();
	},
	onClose: function () {
		this.modelBinder.unbind();
	},
	setValidator: function () {
		this.validator = this.$el.find('form').validate({
			onfocusout: function () {
				return true;
			},
			onkeyup: function () {
				return false;
			},
			onclick: function () {
				return false;
			},
			rules: {
				LowCreditScore: {
					required: true,
					min: 0
				},
				TotalAnnualTurnover: {
					required: true,
					min: 0
				},
				TotalThreeMonthTurnover: {
					required: true,
					min: 0
				},
				Reject_Defaults_CreditScore: {
					required: true,
					min: 0
				},
				Reject_Defaults_AccountsNum: {
					required: true,
					min: 0
				},
				Reject_Defaults_Amount: {
					required: true,
					min: 0
				},
				Reject_Defaults_MonthsNum: {
					required: true,
					min: 0
				},
				Reject_Minimal_Seniority: {
					required: true,
					min: 0
				},
				AutoRejectionException_CreditScore: {
					required: true,
					min: 0
				},
				AutoRejectionException_AnualTurnover: {
					required: true,
					min: 0
				},
				Reject_LowOfflineAnnualRevenue: {
					required: true,
					min: 0
				},
				Reject_LowOfflineQuarterRevenue: {
					required: true,
					min: 0
				},
				Reject_LateLastMonthsNum: {
					required: true,
					min: 0
				},
				Reject_NumOfLateAccounts: {
					required: true,
					min: 0
				},
				RejectionLastValidLate: {
					required: true,
					min: 0
				},
				RejectionCompanyScore: {
					required: true,
					min: 0
				},
				RejectionExceptionMaxConsumerScoreForMpError: {
					required: true,
					min: 0
				},
				RejectionExceptionMaxCompanyScoreForMpError: {
					required: true,
					min: 0
				},
				RejectionExceptionMaxCompanyScore: {
					required: true,
					min: 0
				},
				Reject_Defaults_CompanyScore: {
					required: true,
					min: 0
				},
				Reject_Defaults_CompanyAccountsNum: {
					required: true,
					min: 0
				},
				Reject_Defaults_CompanyMonthsNum: {
					required: true,
					min: 0
				},
				Reject_Defaults_CompanyAmount: {
					required: true,
					min: 0
				}
			}
		});
	}
});
