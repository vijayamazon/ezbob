var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsApprovalModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/AutomationApproval"
});

EzBob.Underwriter.SettingsApprovalView = Backbone.Marionette.ItemView.extend({
	template: "#approval-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	bindings: {
		EnableAutomaticApproval: "select[name='enableAutomaticApproval']",
		EnableAutomaticReApproval: "select[name='enableAutomaticReApproval']",
		MaxCapHomeOwner: "input[name='maxCapHomeOwner']",
		MaxCapNotHomeOwner: "input[name='maxCapNotHomeOwner']"
	},
	events: {
		"click button[name='SaveApprovalSettings']": "saveSettings",
		"click button[name='CancelApprovalSettings']": "cancelSettings"
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
		this.model.fetch();
	},
	cancelSettings: function () {
		this.update();
		return false;
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		if (!$("body").hasClass("role-manager")) {
			this.$el.find(" select[name='enableAutomaticApproval'], select[name='enableAutomaticReApproval'], input[name='maxCapHomeOwner'], input[name='maxCapNotHomeOwner']").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button[name='SaveApprovalSettings'], button[name='CancelApprovalSettings']").hide();
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
				maxCapHomeOwner: {
					required: true,
					min: 0
				},
				maxCapNotHomeOwner: {
					required: true,
					min: 0
				}
			}
		});
	}
});
