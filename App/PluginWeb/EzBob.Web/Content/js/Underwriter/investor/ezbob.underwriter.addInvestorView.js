var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.InvestorModel = Backbone.Model.extend({
	url: "" + gRootPath + "Underwriter/Investor/Index"
});

EzBob.Underwriter.AddInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#add-investor-template",
	initialize: function () {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on("change reset", this.render, this);
		return this;
	},
	ui: {

	},
	serializeData: function () {
		return {

		};
	},
	events: {
	},
	onRender: function () {
		EzBob.handleUserLayoutSetting();
		return this;
	},
	show: function () {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});
