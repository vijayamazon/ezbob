var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsChargesModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsCharges"
});

EzBob.Underwriter.SettingsChargesView = Backbone.Marionette.ItemView.extend({
	template: "#charges-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	bindings: {
		LatePaymentCharge: "input[name='latePaymentCharge']",
		RolloverCharge: "input[name='rolloverCharge']",
		PartialPaymentCharge: "input[name='partialPaymentCharge']",
		AdministrationCharge: "input[name='administrationCharge']",
		OtherCharge: "input[name='otherCharge']",
		AmountToChargeFrom: "input[name='amountToChargeFrom']"
	},
	events: {
		"click button[name='SaveChargesSettings']": "saveSettings",
		"click button[name='CancelChargesSettings']": "cancelSettings"
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
	cancelSettings: function () {
		this.update();
		return false;
	},
	update: function () {
		return this.model.fetch({ reset: true });
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button[name='SaveChargesSettings'], button[name='CancelChargesSettings']").hide();
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
			rules: {
				latePaymentCharge: {
					required: true,
					min: 0
				},
				rolloverCharge: {
					required: true,
					min: 0
				},
				partialPaymentCharge: {
					required: true,
					min: 0
				},
				administrationCharge: {
					required: true,
					min: 0
				},
				otherCharge: {
					required: true,
					min: 0
				},
				amountToChargeFrom: {
					required: true,
					min: 0
				}
			}
		});
	}
});
