var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.Settings.OpenPlatformModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsOpenPlatform"
});

EzBob.Underwriter.Settings.OpenPlatformView = Backbone.Marionette.ItemView.extend({
	template: "#open-platform-settings-template",
	initialize: function(options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change update", this.render, this);
		this.update();
		return this;
	},
	bindings: {

	},

	events: {

	},

	onRender: function() {

	},

	show: function(type) {
		this.$el.show();
	},
	hide: function() {
		this.$el.hide();
	},

	update: function() {
		this.model.fetch({ reset: true });
	}
});