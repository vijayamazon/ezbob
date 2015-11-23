var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

EzBob.Underwriter.Settings.ProductModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/Product"
});

EzBob.Underwriter.Settings.ProductView = Backbone.Marionette.ItemView.extend({
	template: "#product-settings-template",
	initialize: function(options) {
		this.modelBinder = new Backbone.ModelBinder();
		this.model.on("reset change update", this.render, this);
		/*this.update();*/
		return this;
	},
	bindings: {

	},

	events: {

	},

	onRender: function() {
		
	},

	update: function() {
		this.model.fetch({ reset: true });
	}
});