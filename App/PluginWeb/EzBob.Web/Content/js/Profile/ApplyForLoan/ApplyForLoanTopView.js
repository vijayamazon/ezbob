var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ApplyForLoanTopViewModel = Backbone.Model.extend({
	defaults: { state: 'apply', },
});

// a top view that manages process of choosing loan amount
// adding a bank account and displaying confirmation screen
EzBob.Profile.ApplyForLoanTopView = Backbone.Marionette.ItemView.extend({
	template: '#apply-forloan-top-template',

	initialize: function (options) {
		this.customer = options.customer;

		this.applyForLoanViewModel = new EzBob.Profile.ApplyForLoanModel({
			maxCash: this.customer.get('CreditSum'),
			OfferStart: this.customer.get('OfferStart'),
			loanType: this.customer.get('LastApprovedLoanTypeID'),
			repaymentPeriod: this.customer.get('LastRepaymentPeriod'),
			approvedRepaymentPeriod: this.customer.get('LastApprovedRepaymentPeriod'),
			isLoanSourceEU: this.customer.get('IsLastApprovedLoanSourceEu'),
			isLoanSourceCOSME: this.customer.get('IsLastApprovedLoanSourceCOSME'),
			isCurrentCashRequestFromQuickOffer: this.customer.get('IsCurrentCashRequestFromQuickOffer'),
			isCustomerRepaymentPeriodSelectionAllowed: this.customer.get('IsCustomerRepaymentPeriodSelectionAllowed'),
			isLoanTypeSelectionAllowed: this.customer.get('IsLoanTypeSelectionAllowed'),
			numOfActiveLoans: this.customer.get('ActiveLoans').length,
			isTest: this.customer.get('IsTest'),
		});

		this.states = {
			apply: { view: this.createApplyForLoanView, step: 0, },
			bank: { view: this.createAddBankAccountView, step: 1, },
		};

		this.model.on('change', this.render, this);
	}, // initialize

	onRender: function () {
		var view = this.states[this.model.get('state')].view(this);

		view.on('back', this.stateBack, this);

		var region = new Backbone.Marionette.Region({
			el: this.$el.find('.apply-for-loan-div'),
		});

		region.show(view);

		var steps = _.template($('#steps-dashboard-template').html());
		$('.dashboard-steps-container').html(steps({ current: this.states[this.model.get('state')].step, }));

		return false;
	}, // onRender

	createApplyForLoanView: function (self) {
		var view = new EzBob.Profile.ApplyForLoanView({
			model: self.applyForLoanViewModel,
			customer: self.customer,
		});
		view.on('submit', self.amountSelected, self);
		view.on('back', self.backClicked, self);
		return view;
	}, // createApplyForLoanView

	createAddBankAccountView: function (self) {
		var view = new EzBob.BankAccountInfoView({ model: self.customer, });
		view.on('back', function () { return self.model.set('state', 'apply'); });
		view.on('completed', function () { return self.submit(); });
		return view;
	}, // createAddBankAccountView

	backClicked: function () {
		$('.wizard-steps-wrapper').remove();
	}, // backClicked

	stateBack: function () {
		this.customer.fetch();
		this.backClicked();
		this.trigger('back');
	}, // stateBack

	amountSelected: function () {
		var form = this.$el.find('form');

		var pi = this.customer.get('CustomerPersonalInfo');

		this.$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10);

		var enabled = EzBob.Validation.checkForm(EzBob.validateLoanLegalForm(
			form,
			[pi.FirstName, pi.MiddleInitial, pi.Surname]
		));

		if (!enabled)
			return;

		var data = form.serialize();

		BlockUi('on');

		var self = this;
		var xhr = $.post('' + window.gRootPath + 'Customer/GetCash/LoanLegalSigned', data);

		xhr.done(function (res) {
			if (res.error) {
				EzBob.App.trigger('error', res.error);
				return;
			} // if

			if (!self.customer.get('bankAccountAdded')) {
				self.model.set('state', 'bank');
				return;
			} // if

			self.submit();
		});

		xhr.always(function () {
			BlockUi('off');
		});
	}, // amountSelected

	submit: function () {
		var view = new EzBob.Profile.PayPointCardSelectView({
			model: this.customer,
			date: this.lastPaymentDate
		});

		if (view.cards.length > 0) {
			view.on('select', (function (self) {
				return function (cardId) {
					BlockUi();

					var xhr = $.post('' + window.gRootPath + 'Customer/GetCash/Now', {
						cardId: cardId,
						amount: self.applyForLoanViewModel.get('neededCash')
					});

					xhr.done(function (data) {
						if (data.error !== void 0)
							EzBob.ShowMessage(data.error, 'Error occurred');
						else
							document.location.href = data.url;
					});

					return xhr.complete(function () { UnBlockUi(); });
				};
			})(this));

			view.on('existing', (function (self) { return function () { self._submit(); }; })(this));
			view.on('cancel', (function (self) { return function () { self.model.set('state', 'apply'); }; })(this));

			EzBob.App.jqmodal.show(view);
			return false;
		} // if

		this._submit();
		return false;
	}, // submit

	_submit: function () {
		BlockUi();
		this.applyForLoanViewModel.buildUrl();
		document.location.href = this.applyForLoanViewModel.get('url');
	}, // _submit
}); // EzBob.Profile.ApplyForLoanTopView
