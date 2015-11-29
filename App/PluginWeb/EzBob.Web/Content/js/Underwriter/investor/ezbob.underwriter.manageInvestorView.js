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
			manageBank: { view: this.manageInvestorBankView },
			manageContact: { view: this.manageInvestorContactView },
		};

		return this;
	},//initialize

	ui: {
		manageRegion: '#manage-investor-region'
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
			el: this.ui.manageRegion
		});
		region.show(view);
		return this;
	},//onRender

	show: function (id) {
		this.model.set('InvestorID', id);
		this.stateModel.set({ state: 'details' }, { silent: true });
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
		view.on('manageBank', self.manageBank, self);
		view.on('manageContact', self.manageContact, self);
		return view;
	},//investorDetailsView
	
	manageInvestorBankView: function (self) {
		var view = new EzBob.Underwriter.ManageInvestorBankView({
			model: self.model,
			stateModel: self.stateModel
		});
		return view;
	},//manageInvestorBankView

	manageInvestorContactView: function (self) {
		var view = new EzBob.Underwriter.ManageInvestorContactView({
			model: self.model,
			stateModel: self.stateModel
		});
		return view;
	},//manageInvestorContactView

	manageBank: function (id) {
		this.stateModel.set('editID', id, { silent: true });
		this.stateModel.set('state', 'manageBank');
		return false;
	},

	manageContact: function (id) {
		this.stateModel.set('editID', id, { silent: true });
		this.stateModel.set('state', 'manageContact');
		return false;
	},

	backClicked: function () {
		this.stateModel.set('state', 'details');
		return false;
	},
});
