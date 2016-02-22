var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.LoanHistoryModel = Backbone.Model.extend({
	idAttribute: 'Id',
	url: function() {
		return '' + window.gRootPath + 'Underwriter/LoanHistory/Index/' + this.customerId;
	}
});

EzBob.Underwriter.LoanHistoryView = Backbone.Marionette.View.extend({
	initialize: function() {
		this.template = _.template($('#loanhistory-template').html());
		this.templateView = _.template($('#loanhistory-view-template').html());
		this.offersTemplate = _.template($('#offers-history-template').html());
		this.bindTo(this.model, 'reset fetch change sync', this.render, this);
		this.isAll = true;
	},

	events: {
		'click tr.loans.tr-link': 'rowClick',
		'click .export-to-exel': 'exportExcel',
		'click .edit-loan': 'editLoan',
		'click .show-schedule': 'showSchedule',
		'click .show-all-decisions': 'showAllDecisions'
	},

	showAllDecisions: function () {
		this.isAll = this.$el.find('.show-all-decisions').is(':checked');
		this.renderOffers();
	},

	exportExcel: function() {
		location.href = '' + window.gRootPath + 'Underwriter/LoanHistory/ExportToExel?id=' + this.model.customerId;
	},

	rowClick: function(e) {
		var id = e.currentTarget.getAttribute('data-id');
		if (!id)
			return;

		var details = new EzBob.Underwriter.LoanHistoryDetailsModel();
		details.loanid = id;

		var loan = _.find(this.model.get('loans'), function(l) {
			return l.Id === id;
		});

		details.id = this.idCustomer;

		var detailsView = new EzBob.Underwriter.LoanDetailsView({
			model: details,
			loan: loan
		});
		detailsView.on('RolloverAdded', this.updateView, this);
		detailsView.on('ManualPaymentAdded', this.updateView, this);
		details.fetch();
	},

	updateView: function() {
		this.model.fetch();
	},

	editLoan: function(e) {
		var id = e.currentTarget.getAttribute('data-id');

		var loan = new EzBob.LoanModel({
			Id: id
		});

		var xhr = loan.fetch();

		BlockUi('on');
		var self = this;

		xhr.done(function() {
			var view = new EzBob.EditLoanView({
				model: loan
			});
			view.on('item:saved', self.updateView, self);
			BlockUi('off');
			EzBob.App.jqmodal.show(view);
		});

		return false;
	},

	render: function() {
		this.$el.html(this.templateView());

		this.table = this.$el.find('#loanhistory-table');

		var viewModel = this.model.toJSON();

		this.table.html(this.template(viewModel));

		this.renderOffers();

		return this;
	},

	renderOffers: function() {
		var data = { offers: this.filterOffers() };
		this.offersConteiner = this.$el.find('#offers-container');
		this.offersConteiner.html(this.offersTemplate(data));
		return this;
	},

	filterOffers: function () {
		if (this.isAll) {
			return this.model.get('offers');
		} else {
			return _.filter(this.model.get('offers'), function(o) {
				return o.Action === 'Approve' || o.Action === 'ReApprove' || o.Action === 'Approved';
			});
		}
	},

	showSchedule: function(e) {
		var offerId = $(e.currentTarget).data('id');

		var xhr = $.getJSON("" + window.gRootPath + 'Underwriter/Schedule/Calculate/' + offerId);

		var self = this;

		xhr.done(function(data) {
			var view = new EzBob.LoanScheduleViewDlg({
				schedule: data,
				isShowGift: false,
				isShowExportBlock: true,
				offerId: offerId,
				customerId: self.model.customerId
			});

			EzBob.App.jqmodal.show(view);
		});

		return false;
	}
});
