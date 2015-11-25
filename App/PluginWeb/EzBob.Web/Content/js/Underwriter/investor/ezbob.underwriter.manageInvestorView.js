var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManageInvestorView = Backbone.Marionette.ItemView.extend({
	template: '#manage-investor-template',
	initialize: function () {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on('change reset', this.render, this);
		this.stateModel = new Backbone.Model({ state: 'details' });
		this.stateModel.on('change:state', this.render, this);
		this.views = {
			details: { view: this.investorDetailsView },
			addEditBank: { view: this.addEditInvestorBankView },
			addEditContact: { view: this.addEditInvestorContactView },
		};

		return this;
	},//initialize

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

		var view = this.views[this.stateModel.get('state')].view(this);
		view.on('back', this.backClicked, this);

		var region = new Backbone.Marionette.Region({
			el: this.$el.find('#manage-investor-region')
		});
		region.show(view);
		return this;
	},//onRender

	show: function (id) {
		this.model.set('InvestorID', id);
		var self = this;
		this.model.fetch().done(function() {
			self.$el.show();
		});
	},//show

	hide: function () {
		return this.$el.hide();
	},

	investorDetailsView: function(self) {
		var view = new EzBob.Underwriter.ManageInvestorDetailsView({
			model: self.model,
		});
		view.on('addBank', self.addBank, self);
		view.on('addContact', self.addContact, self);
		return view;
	},//investorDetailsView
	
	addEditInvestorBankView: function (self) {
		var view = new EzBob.Underwriter.ManageInvestorBankView({
			model: self.model,
		});
		return view;
	},//addEditInvestorBankView

	addEditInvestorContactView: function (self) {
		var view = new EzBob.Underwriter.ManageInvestorContactView({
			model: self.model,
		});
		return view;
	},//addEditInvestorContactView

	addBank: function () {
		this.stateModel.set('state', 'addEditBank');
	},

	addContact: function () {
		this.stateModel.set('state', 'addEditContact');
	},

	backClicked: function () {
		this.stateModel.set('state', 'details');
	},
});

EzBob.Underwriter.ManageInvestorDetailsView = Backbone.Marionette.ItemView.extend({
	template: "#manage-investor-details-template",
	initialize: function() {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on('change reset', this.render, this);
	},//initialize

	events:{
		'click #addInvestorContact': 'addContact',
		'click #addInvestorBank': 'addBank'
	},

	addContact: function() {
		this.trigger('addContact');
	},

	addBank: function() {
		this.trigger('addBank');
	}
});//EzBob.Underwriter.ManageInvestorDetailsView

EzBob.Underwriter.ManageInvestorContactView = Backbone.Marionette.ItemView.extend({
	template: '#manage-investor-contact-template',
	initialize: function () {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on('change reset', this.render, this);
	},//initialize

	events: {
		'click #investorContactBack': 'back'
	},

	back: function () {
		this.trigger('back');
	}
});//EzBob.Underwriter.ManageInvestorContactView

EzBob.Underwriter.ManageInvestorBankView = Backbone.Marionette.ItemView.extend({
	template: '#manage-investor-bank-template',
	initialize: function () {
		this.model = new EzBob.Underwriter.InvestorModel();
		this.model.on('change reset', this.render, this);
	},//initialize

	events: {
		'click #investorBankBack': 'back'
	},

	back: function () {
		this.trigger('back');
	}
});//EzBob.Underwriter.ManageInvestorBankView