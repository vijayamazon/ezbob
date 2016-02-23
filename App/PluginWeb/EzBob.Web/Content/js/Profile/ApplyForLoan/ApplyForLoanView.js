var EzBob = EzBob || {};

EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ApplyForLoanView = Backbone.Marionette.ItemView.extend({
	template: '#apply-forloan-template',

	initialize: function (options) {
		this.manualSetupFeePercent = 0;
		this.brokerFeePercent = 0;

		this.customer = options.customer;

		if (this.customer.get('CreditSum') < EzBob.Config.XMinLoan) {
			this.trigger('back');
			document.location.href = '#';
			return;
		} // if

		this.fixed = this.customer.get('IsLoanDetailsFixed');

		this.currentLoanTypeID = 1; // for backward compatibility
		this.currentRepaymentPeriod = this.model.get('repaymentPeriod');

		this.recalculateThrottled = _.debounce(this.recalculateSchedule, 250);

		this.timerId = setInterval(_.bind(this.refreshTimer, this), 1000);

		this.model.set({
			'CreditSum': this.customer.get('CreditSum'),
			'OfferValid': this.customer.offerValidFormatted()
		});

		this.agreementView = null;

		if (this.fixed)
			return;

		this.model.on('change:neededCash', this.neededCashChanged, this);
		this.isLoanSourceEU = options.model.get('isLoanSourceEU');
		this.isLoanSourceCOSME = options.model.get('isLoanSourceCOSME');

		this.isAlibaba = this.customer.get('IsAlibaba');
		this.isEverline = this.customer.get('Origin') === 'everline';
	}, // initialize

	events: {
		'click .submit': 'submit',
		'change .DynamicAgreementTermsRead': 'DynamicAgreementTermsReadChange',
		'change .agreementTermsRead': 'showSubmit',
		'change .euAgreementTermsRead': 'showSubmit',
		'change .cosmeAgreementTermsRead': 'showSubmit',
		'change .directorConsentRead': 'showSubmit',
		'change #signedName': 'showSubmit',
		'blur #signedName': 'showSubmit',
		'keyup #signedName': 'showSubmit',
		'paste #signedName': 'showSubmit',
		'click .print': 'print',
		'change .notInBankruptcy': 'notInBankruptcyChange',
		'click .btn-back': 'backClicked'
	}, // events

	backClicked: function () {
		this.trigger('back');
	}, // backClicked

	notInBankruptcyChange: function () {
		var isChecked = !!this.$el.find('.notInBankruptcy').attr('checked');

		if (isChecked) {
			var msg = '<div class="megila-outer megila-outer-solvency1">' +
				'<div class="megila-inner megila-inner-solvency1">' + this.$el.find('.loan-disclosure-text1').html() +
				'</div></div>' +
				'<div class="megila-outer megila-outer-solvency2">' +
				'<div class="megila-inner megila-inner-solvency2">' + this.$el.find('.loan-disclosure-text2').html() +
				'</div></div>';

			EzBob.ShowMessageEx({
				message: msg,
				dialogWidth: 600,
				hideClose: true,
				okText: 'Confirm',
				customClass: 'megila',
				closeOnEscape: false,
				onOk: _.bind(this.onDisclosureClosed, this),
			});
		} else {
			this.$el.find('.sign-full-name').hide();
			this.showSubmit();
		} // if
	}, // notInBankruptcyChange

	onDisclosureClosed: function () {
		this.$el.find('.sign-full-name').show();
		this.showSubmit();
	}, // onDisclosureClosed

	ui: {
		submit: '.submit',
		agreement: '.agreement',
		dynamicCheckboxes: '.dynamic-agreements-checkboxes',
		form: 'form',
		loanAmountInput: 'input#loanAmount',
		repaymentPeriodInput: 'input#repaymentPeriod',
		cannotTakeAnotherLoan: '.cannot-take-another-loan',
		cannotTakeUnderMinLoan: '.cannot-take-under-minloan',
	}, // ui

	DynamicAgreementTermsReadChange: function (ev) {
		var curClicked = $(ev.currentTarget);

		var isClicked = curClicked.is(':checked');

		var checkboxName = $(ev.currentTarget).attr('id').split('TermsRead')[0];

		var tabSelector = 'a[page-name=' + checkboxName + ']';

		if (!isClicked)
			this.$el.find(tabSelector).tab('show');
	}, // DynamicAgreementTermsReadChange

	loanSelectionChanged: function () {
		this.currentRepaymentPeriod = this.$('#loan-sliders .period-slider').slider('value');

		var amount = this.$('#loan-sliders .amount-slider').slider('value');
		amount = parseInt(amount, 10);
		this.model.set('neededCash', amount);
		this.model.set('loanType', this.currentLoanTypeID);
		this.model.set('repaymentPeriod', this.currentRepaymentPeriod);

		this.neededCashChanged(true);
	}, // loanSelectionChanged

	showSubmit: function () {
		var enabled = EzBob.Validation.checkForm(this.validator);

		this.model.set('agree', enabled);
		this.ui.submit.toggleClass('disabled', !enabled);
	}, // showSubmit

	recalculateSchedule: function (args) {
		var self = this;
		var val = args.value;

		BlockUi('on', this.$el.find('#block-loan-schedule'));
		BlockUi('on', this.$el.find('#block-agreement'));

		if (!this.currentRepaymentPeriod)
			this.currentRepaymentPeriod = this.model.get('repaymentPeriod');

		var sMoreParams = '&loanType=' + this.currentLoanTypeID + '&repaymentPeriod=' + this.currentRepaymentPeriod;

		var url = ('' + window.gRootPath + 'Customer/Schedule/Calculate?amount=' + (parseInt(val))) + sMoreParams;

		$.getJSON(url).done(function (data) {
			self.renderSchedule(data);

			BlockUi('off', self.$el.find('#block-loan-schedule'));
			BlockUi('off', self.$el.find('#block-agreement'));
		});

		var requestedAmount = +this.model.get('neededCash');
		var numOfActiveLoans = +this.model.get('numOfActiveLoans');

		var cannotTakeAnother = (numOfActiveLoans > 0) &&
			(EzBob.Config.NumofAllowedActiveLoans - numOfActiveLoans <= 1) &&
			(requestedAmount < this.model.get('CreditSum'));

		if (cannotTakeAnother)
			this.ui.cannotTakeAnotherLoan.show();
		else
			this.ui.cannotTakeAnotherLoan.hide();

		var maxCash = this.model.get('maxCash');
		var minLoan = EzBob.Config.MinLoan;
		var remainingAmountForTopUp = maxCash - requestedAmount;

		if ((numOfActiveLoans === 0) && (remainingAmountForTopUp < minLoan) && (requestedAmount < maxCash))
			this.ui.cannotTakeUnderMinLoan.show();
		else
			this.ui.cannotTakeUnderMinLoan.hide();
	}, // recalculateSchedule

	renderSchedule: function (schedule) {
		if (!schedule)
			return;

		this.manualSetupFeePercent = schedule.ManualSetupFeePercent;
		this.brokerFeePercent = schedule.BrokerFeePercent;

		if (!schedule.Schedule)
			return;

		this.lastPaymentDate = moment(schedule.Schedule[schedule.Schedule.length - 1].Date);

		var scheduleView = new EzBob.LoanScheduleView({
			el: this.$el.find('.loan-schedule'),
			schedule: schedule,
			isShowGift: false,
			isShowExportBlock: false,
			isShowExceedMaxInterestForSource: false,
			isPersonal: _.contains([0, 4, 2], this.customer.get('CustomerPersonalInfo').TypeOfBusiness)
		});

		scheduleView.render();
		this.createAgreementView(schedule.Agreement, schedule.Templates);
		this.createCheckboxView(schedule.Templates);
	}, // renderSchedule

	neededCashChanged: function (reloadSelectedOnly) {
		this.$el.find(
			'.preAgreementTermsRead, .agreementTermsRead, .euAgreementTermsRead, .cosmeAgreementTermsRead, .notInBankruptcy'
		).prop('checked', false);

		this.$el.find('.sign-full-name').hide();

		var value = this.model.get('neededCash');

		this.ui.loanAmountInput.val(value);
		this.ui.repaymentPeriodInput.val(this.currentRepaymentPeriod);

		this.ui.submit.attr('href', this.model.get('url'));
		return this.recalculateThrottled({
			value: value,
			reloadSelectedOnly: reloadSelectedOnly
		});
	}, // neededCashChanged

	onRender: function () {
		if (this.fixed)
			this.$('.cash-question').hide();

		if (!this.model.get('isCustomerRepaymentPeriodSelectionAllowed') || this.isAlibaba)
			this.$('.duration-select-allowed').hide();

		if (!this.isLoanSourceEU)
			this.$('.eu-agreement-section').hide();

		if (!this.isLoanSourceCOSME)
			this.$('.cosme-agreement-section').hide();

		if (this.isAlibaba)
			this.$('.loan-amount-header-start').text('Review loan schedule');

		var self = this;

		if (this.model.get('isCurrentCashRequestFromQuickOffer'))
			this.$('.loan-amount-header-start').text('Confirm loan amount');
		else {
			this.$('.quick-offer-section').remove();
			if (!this.isAlibaba) {
				var view = new EzBob.TakeLoanSlidersView({ el: this.$('#loan-sliders'), model: this.model });
				view.render();
				EzBob.App.on('loanSelectionChanged', this.loanSelectionChanged, this);
			} // if
		} // if

		this.neededCashChanged();

		this.$el.find('img[rel]').setPopover('right');
		this.$el.find('li[rel]').setPopover('left');

		var pi = this.customer.get('CustomerPersonalInfo');
		pi.Fullname = pi.Fullname || '';
		this.$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10);

		this.validator = EzBob.validateLoanLegalForm(this.ui.form, [pi.FirstName, pi.MiddleInitial, pi.Surname]);

		this.$el.find('form.LoanLegal').submit(function (evt) {
			return self.submit(evt);
		});

		this.showSubmit();
		EzBob.UiAction.registerView(this);

		return this;
	}, // onRender

	refreshTimer: function () {
		return this.$el.find('.offerValidFor').text(this.customer.offerValidFormatted());
	}, // refreshTimer

	submit: function (e) {
		e.preventDefault();

		var creditSum = this.model.get('neededCash');
		var max = this.model.get('maxCash');
		var min = this.model.get('minCash');

		this.model.set('neededCash', creditSum);
		this.model.set('loanType', this.currentLoanTypeID);
		this.model.set('repaymentPeriod', this.currentRepaymentPeriod);

		if (creditSum > max || creditSum < min)
			return false;

		var enabled = EzBob.Validation.checkForm(this.validator);

		if (!enabled) {
			this.showSubmit();
			return false;
		} // if

		this.subtituteToUi('manualSetupFeePercent');
		this.subtituteToUi('brokerFeePercent');

		this.trigger('submit');
		return false;
	}, // submit

	subtituteToUi: function(argName) {
		this.$el.find('#' + argName).remove();

		var input = $('<input type=hidden />');
		input.val(this[argName]);
		input.attr({
			name: argName,
			id: argName,
		});

		this.$el.find('form').append(input);
	}, // subtituteToUi

	getCurrentViewId: function () {
		return this.$el.find('li.active a').attr('page-name');
	}, // getCurrentViewId

	print: function () {
		printElement(this.getCurrentViewId());
		return false;
	}, // print

	updateDownloadLink: function () {
		if (!this.agreementView)
			return;

		var amount = parseInt(this.model.get('neededCash'), 10);
		var loanType = this.currentLoanTypeID;
		var repaymentPeriod = this.currentRepaymentPeriod;
		var view = this.getCurrentViewId();

		this.agreementView.$el
			.find('.download')
			.attr(
				'href',
				'' + window.gRootPath + 'Customer/Agreement/Download?amount=' + amount +
				'&viewName=' + view + '&loanType=' + loanType + '&repaymentPeriod=' + repaymentPeriod
			);
	}, // updateDownloadLink

	createAgreementView: function (agreementdata, templates) {
		var oViewArgs = {
			el: this.ui.agreement,
			onTabSwitch: _.bind(this.updateDownloadLink, this),
			templates: templates
		};

		this.agreementView = new EzBob.Profile.AgreementView(oViewArgs);

		this.agreementView.render(agreementdata);

		this.showSubmit();

		this.updateDownloadLink();
	}, // createAgreementView

	createCheckboxView: function (templates) {
		console.log('el', this.ui.dynamicCheckboxes);
		this.dynamicAgreementsCheckboxesView = new EzBob.Profile.AgreementsDynamicCheckboxes({
			el: this.ui.dynamicCheckboxes,
			templates: templates,
		});
		this.dynamicAgreementsCheckboxesView.render();
	}, // createCheckboxView

	close: function () {
		clearInterval(this.timerId);
		this.model.off();
		ApplyForLoanView.__super__.close.call(this);
	}, // close
}); // EzBob.Profile.ApplyForLoanView
