var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.SettingsAutomationModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/AutomationGeneral"
});

EzBob.Underwriter.SettingsAutomationView = Backbone.Marionette.ItemView.extend({
	template: "#automation-settings-template",
	initialize: function (options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click button[name='SaveAutomationSettings']": "saveSettings",
		"click button[name='CancelAutomationSettings']": "cancelSettings"
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
		return this.model.fetch();
	},
	cancelSettings: function () {
		this.update();
		return false;
	},
	onRender: function () {
		this.modelBinder.bind(this.model, this.el, this.bindings);
		if (!$("body").hasClass("role-manager")) {
			this.$el.find("button[name='SaveAutomationSettings'], button[name='CancelAutomationSettings']").hide();
		}
		return this;
	},
	show: function (type) {
		this.$el.show();
		return EzBob.handleUserLayoutSetting();
	},
	hide: function () {
		return this.$el.hide();
	},
	onClose: function () {
		return this.modelBinder.unbind();
	}
});
