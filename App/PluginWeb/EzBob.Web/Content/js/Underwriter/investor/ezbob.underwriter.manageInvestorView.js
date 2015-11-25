var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorView = Backbone.Marionette.ItemView.extend({
	template: "#manage-investor-template",
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
		console.log('this.model', this.model);
		return this;
	},

	show: function (id) {
		this.model.set('InvestorID', id);
		var self = this;
		this.model.fetch().done(function() {
			self.$el.show();
		});
	},
	hide: function () {
		return this.$el.hide();
	},
});
