var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorDetailsView = Backbone.Marionette.ItemView.extend({
	template: "#manage-investor-details-template",
	initialize: function() {
		this.model.on('change reset', this.render, this);
	},//initialize

	serializeData: function () {
		return {
			data: this.model.toJSON()
		};
	},

	events:{
		'click #addInvestorContact': 'addContact',
		'click #addInvestorBank': 'addBank',
		'click .editInvestorContact': 'editContact',
		'click .editInvestorBank': 'editBank',
	},

	addContact: function() {
		this.trigger('addContact');
	},

	addBank: function() {
		this.trigger('addBank');
	},

	editContact: function (el) {
		this.trigger('addContact', $(el.currentTarget).data('contactid'));
	},

	editBank: function (el) {
		this.trigger('addBank', $(el.currentTarget).data('bankid'));
	}
});//EzBob.Underwriter.ManageInvestorDetailsView
