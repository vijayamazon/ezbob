var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.Settings.ProductModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsProduct"
});

EzBob.Underwriter.Settings.ProductView = Backbone.Marionette.ItemView.extend({
	template: "#product-settings-template",
	initialize: function(options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset update change", this.render, this);
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