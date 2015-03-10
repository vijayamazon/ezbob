var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ReviewSignCreditLineView = EzBob.View.extend({
	initialize: function(options) {
		this.$el = $('#review-sign-credit-line');
		this.$profile = $('#profile-main-to-be-replaced');

		this.hasEl = this.$el.length > 0;

		this.data = {
			customerID: options.customerID,
			cashRequestID: options.cashRequestID,
			signedLegalID: options.signedLegalID,
			fullName: [ options.firstName, options.middleName, options.lastName, ],

			approvedAmount: options.approvedAmount,
			repaymentPeriod: options.repaymentPeriod,
			loanTypeID: options.loanTypeID,

			isPersonal: options.isPersonal,

			alibabaCreditFacilityTemplate: options.alibabaCreditFacilityTemplate,
		};

		if (this.hasEl) {
			this.$loanSchedule = this.$el.find('.loan-schedule');
			this.$signatureBlock = this.$el.find('.signature-block');
			this.$fullNameBlock = this.$el.find('.sign-full-name');
			this.$signButton = this.$el.find('.btn-sign');
			this.$accepted = this.$el.find('#creditFacilityAccepted');
			this.$signedName = this.$el.find('#signedName');

			this.$document = this.$el.find('.credit-facility');
			this.$template = this.$el.find('.credit-facility-template');

			this.validator = this.$signatureBlock.validate({
				rules: {
					'signedName': { validateSignerName: this.data.fullName, },
				},
				errorPlacement: EzBob.Validation.errorPlacement,
				unhighlight: EzBob.Validation.unhighlightFS,
				highlight: EzBob.Validation.highlightFS,
				ignore: ':not(:visible)',
			});
		} // if
	}, // initialize

	events: {
		'click .btn-back': 'close',
		'click .btn-sign': 'sign',
		'click #creditFacilityAccepted': 'onToggleAccepted',
		'change #signedName': 'adjustSignatureBlockState',
		'blur #signedName': 'adjustSignatureBlockState',
		'keyup #signedName': 'adjustSignatureBlockState',
		'paste #signedName': 'adjustSignatureBlockState',
	}, // events

	render: function() {
		if (!this.hasEl)
			return this;

		this.$el.find('.agreement-title').text(this.data.signedLegalID ? 'Review signed' : 'Sign');

		var self = this;

		this.$profile.fadeOut('fast', function() {
			self.$el.fadeIn('fast');
		});

		this.onToggleAccepted();

		this.loadExampleSchedule();
		EzBob.UiAction.registerView(this);

		return this;
	}, // render

	loadExampleSchedule: function() {
		var self = this;

		var args = '?amount=' + this.data.approvedAmount +
			'&loanType=' + this.data.loanTypeID +
			'&repaymentPeriod=' + this.data.repaymentPeriod;

		$.getJSON('' + window.gRootPath + 'Customer/Schedule/Calculate' + args).done(function(data) {
			self.renderSchedule(data);
			self.renderDocument(data);
		});
	}, // loadExampleSchedule

	renderSchedule: function(schedule) {
		this.lastPaymentDate = moment(schedule.Schedule[schedule.Schedule.length - 1].Date);

		var scheduleView = new EzBob.LoanScheduleView({
			el: this.$loanSchedule,
			schedule: schedule,
			isShowGift: false,
			isShowExportBlock: false,
			isShowExceedMaxInterestForSource: false,
			isPersonal: this.data.isPersonal,
		});

		scheduleView.render();

		scheduleView.$el.find('.print-customer_details').hide();
	}, // renderSchedule

	renderDocument: function(data) {
		var template = Handlebars.compile(
			this.data.signedLegalID ? this.data.alibabaCreditFacilityTemplate : this.$template.html()
		);

		this.$document.html(template(data.Agreement));
	}, // renderDocument

	onToggleAccepted: function() {
		this.$fullNameBlock.toggleClass('hide', !this.isAccepted());
		this.adjustSignatureBlockState();
	}, // onToggleAccepted

	isAccepted: function() {
		return !!this.$accepted.attr('checked');
	}, // isAccepted

	adjustSignatureBlockState: function() {
		if (this.data.signedLegalID) {
			this.$signatureBlock.addClass('hide');
			this.$signButton.addClass('hide');
			return;
		} // if

		this.$signatureBlock.removeClass('hide');
		this.$signButton.removeClass('hide');

		this.setSomethingEnabled(this.$signButton, this.isSignEnabled());
	}, // adjustSignatureBlockState

	isSignEnabled: function() {
		if (this.data.signedLegalID)
			return false;

		return this.isAccepted() && EzBob.Validation.checkForm(this.validator);
	}, // isSignEnabled

	close: function() {
		var self = this;

		this.$el.fadeOut('fast', function() {
			self.$profile.fadeIn('fast');
		});
	}, // close

	sign: function() {
		if (!this.isSomethingEnabled(this.$signButton))
			return;

		BlockUi();

		var request = $.post(window.gRootPath + 'Customer/GetCash/CreditLineSigned', {
			customerID: this.data.customerID,
			cashRequestID: this.data.cashRequestID,
			signedName: this.$signedName.val(),
			creditFacilityAccepted: this.isAccepted(),
		});

		request.done(function(response) {
			UnBlockUi();

			if (response.success) {
				EzBob.ShowMessageEx({
					message: 'Your signature has been saved.',
					title: 'Thank you!',
					closeOnEscape: false,
					onOk: function() {
						BlockUi();
						window.location.reload(true);
					},
				});

				return;
			} // if

			EzBob.ShowMessageEx({
				message: response.error || 'Failed to save your signature, please try again.',
				title: 'Error'
			});
		});

		request.fail(function() {
			UnBlockUi();

			EzBob.ShowMessageEx({
				message: 'Failed to save your signature, please try again.',
				title: 'Error'
			});
		});
	}, // sign
}); // EzBob.Profile.ReviewSignCreditLineView
