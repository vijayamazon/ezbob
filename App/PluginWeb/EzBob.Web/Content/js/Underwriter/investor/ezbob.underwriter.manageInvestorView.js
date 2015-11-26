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
			stateModel: self.stateModel
		});
		return view;
	},//addEditInvestorBankView

	addEditInvestorContactView: function (self) {
		var view = new EzBob.Underwriter.ManageInvestorContactView({
			model: self.model,
			stateModel: self.stateModel
		});
		return view;
	},//addEditInvestorContactView

	addBank: function (id) {
		this.stateModel.set('editID', id, { silent: true });
		this.stateModel.set('state', 'addEditBank');
		return false;
	},

	addContact: function (id) {
		this.stateModel.set('editID', id, { silent: true });
		this.stateModel.set('state', 'addEditContact');
		return false;
	},

	backClicked: function () {
		this.stateModel.set('state', 'details');
		return false;
	},
});
