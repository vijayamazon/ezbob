var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsGeneralModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsGeneral"
});

EzBob.Underwriter.SettingsGeneralView = Backbone.Marionette.ItemView.extend({
	template: "#general-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	bindings: {
		BWABusinessCheck: "select[name='bwaBusinessCheck']",
		HmrcSalariesMultiplier: {
			selector: "input[name='HmrcSalariesMultiplier']",
			converter: EzBob.BindingConverters.percentsFormat
		},
		FCFFactor: {
			selector: "input[name='FCFFactor']"
		}
	},
	events: {
		"click button[name='SaveGeneralSettings']": "saveSettings",
		"click button[name='CancelGeneralSettings']": "cancelSettings"
	},
	saveSettings: function () {
		BlockUi("on");
		this.model.save().done(function () {
			return EzBob.ShowMessage("Saved successfully", "Successful");
		});
		this.model.save().complete(function () {
			return BlockUi("off");
		});
		this.model.save();
		return false;
	},
	update: function () {
		this.model.fetch({reset: true});
	},
	cancelSettings: function () {
		this.update();
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("input").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			this.$el.find("button[name='SaveGeneralSettings'], button[name='CancelGeneralSettings']").hide();
		}
		this.$el.find("input[name='HmrcSalariesMultiplier']").percentFormat();
	},
	show: function (type) {
		this.$el.show();
	},
	hide: function () {
		this.$el.hide();
	},
	onClose: function () {
		this.modelBinder.unbind();
	}
});
