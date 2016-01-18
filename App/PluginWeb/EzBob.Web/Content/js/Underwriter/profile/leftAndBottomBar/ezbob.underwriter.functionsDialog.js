var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.FunctionsDialogView = Backbone.View.extend({
	initialize: function () {
		this.template = _.template($('#functionsDialogTemplate').html());
		this.type = this.getType();
		return this;
	}, // initialize

	getType: function () { return null; },

	render: function () {
		this.$el.html(this.template(this.model));

		this.$el.find('.button-ok').val(this.getButtonName());

		this.ReasonField = this.$el.find('.reason');

		this.YodleeReasonField = this.$el.find('.yodleeReason');

		this.RejectionReason = this.$el.find('.rejectReason');

		if (!this.showReasonField()) {
			this.ReasonField.css('display', 'none');
			this.$el.find('h3').css('display', 'none');
		} // if

		this.ReasonField.val(this.model.get('Reason'));

		this.$el.dialog({
			autoOpen: true,
			position: ['top', 60],
			draggable: true,
			title: 'Are you sure?',
			modal: true,
			resizable: true,
			width: this.dlgWidth || 550,
			height: this.dlgHeight || 300,
			dialogClass: 'functionsPopup',
			open: _.bind(this.onShow, this)
		});

		return this;
	}, // render

	getButtonName: function () { return 'Ok'; },

	showReasonField: function () { return true; },

	getIsSignature: function () { return 0; },

	onShow: function () { },

	events: {
		'click .button-ok': 'BtnOkClicked',
		'click .button-cancel': 'BtnCancelClicked',
		'keydown textarea.reason': 'TextAreaChanged',
		'keydown textarea.yodleeReason': 'TextAreaChanged',
	}, // events

	ReasonFieldEmptyError: function (field, isShow) { field.css('border', isShow ? '1px solid red' : ''); },

	TextAreaChanged: function (field) {
		var removeDisabled = (this.getType() !== 'Approved') ||
			EzBob.isNullOrEmpty(this.model.get('OfferedCreditLine')) ||
			(this.model.get('OfferedCreditLine') !== 0);

		if (removeDisabled)
			$('.button-ok').removeClass('disabled');

		this.ReasonFieldEmptyError($(field.currentTarget), false);
	}, // TextAreaChanged

	BtnOkClicked: function (e) {
		var that = this;

		if ($(e.currentTarget).hasClass('disabled'))
			return;

		var rejectionReasons = this.RejectionReason.val();

		var req = false;

		if (this.type !== 'Rejected') {
			if ((this.ReasonField.val() === '') && this.showReasonField()) {
				this.ReasonFieldEmptyError(this.ReasonField, true);
				req = true;
			} // if

			if ((this.YodleeReasonField.val() === '') && this.NoYodlee) {
				this.ReasonFieldEmptyError(this.YodleeReasonField, true);
				req = true;
			} // if
		} // if

		if ((this.type === 'Rejected') && !rejectionReasons) {
			this.ReasonFieldEmptyError(this.$el.find('.rejectReasonDiv'), true);
			req = true;
		} // if

		if (req)
			return;

		$(e.currentTarget).addClass('disabled');

		var data = {
			customerID: this.model.get('CustomerId'),
			status: this.type,
			signature: this.getIsSignature(),
			cashRequestID: this.model.get('CashRequestId'),
			cashRequestRowVersion: this.model.get('CashRequestRowVersion'),
		};

		if (this.showReasonField())
			data.reason = this.ReasonField.val();

		if (rejectionReasons && (rejectionReasons.length > 0))
			rejectionReasons = rejectionReasons.map(Number);

		data.rejectionReasons = rejectionReasons;

		if (this.NoYodlee)
			data.reason += ' | ' + this.YodleeReasonField.val();

		BlockUi();

		var xhr = $.ajax({
			type: 'POST',
			url: window.gRootPath + 'Underwriter/Customers/SetDecision',
			data: data,
			dataType: 'json',
			traditional: true,
		});

		xhr.done(function (res) {
			if (res.error) {
				EzBob.ShowMessage(res.error, 'Error occurred');
				that.$el.css('border', '1px solid red');
				return;
			} // if

			if (res.warning)
				EzBob.ShowMessage(res.warning, 'Warning signaled');

			that.$el.dialog('close');
			that.trigger('changedSystemDecision');
			$('.ui-icon-refresh').click();
		});

		xhr.always(function() {
			UnBlockUi();
			$(e.currentTarget).removeClass('disabled');
		});
	}, // BtnOkClicked

	BtnCancelClicked: function () { return this.$el.dialog('close'); },
}); // EzBob.Underwriter.FunctionsDialogView

EzBob.Underwriter.RejectedDialog = EzBob.Underwriter.FunctionsDialogView.extend({
	onShow: function () {
		this.$el.find('.rejectReasonDiv').show();
		return this.$el.find('.chosen').chosen();
	}, // onShow

	dlgHeight: 400,

	getType: function () { return 'Rejected'; },

	getButtonName: function () { return 'Reject'; },
}); // EzBob.Underwriter.RejectedDialog

EzBob.Underwriter.Escalated = EzBob.Underwriter.FunctionsDialogView.extend({
	getType: function () { return 'Escalated'; },

	getButtonName: function () { return 'Escalate'; },
}); // EzBob.Underwriter.Escalated

EzBob.Underwriter.Suspended = EzBob.Underwriter.FunctionsDialogView.extend({
	getType: function () { return 'ApprovedPending'; },

	getButtonName: function () { return 'Suspend'; },

	showReasonField: function () { return false; },

	dlgHeight: 120,

	dlgWidth: 300,
}); // EzBob.Underwriter.Suspended

EzBob.Underwriter.Signature = EzBob.Underwriter.FunctionsDialogView.extend({
	getType: function () { return 'ApprovedPending'; },

	getIsSignature: function () { return 1; },

	showReasonField: function () { return false; },

	getButtonName: function () { return 'Waiting for signature'; },

	dlgHeight: 120,

	dlgWidth: 300,
}); // EzBob.Underwriter.Signature

EzBob.Underwriter.Returned = EzBob.Underwriter.FunctionsDialogView.extend({
	getType: function () { return 'WaitingForDecision'; },

	getButtonName: function () { return 'Return'; },

	showReasonField: function () { return false; },

	dlgHeight: 120,

	dlgWidth: 300,
}); // EzBob.Underwriter.Returned

EzBob.Underwriter.ApproveDialog = EzBob.Underwriter.FunctionsDialogView.extend({
	events: function () {
		return _.extend({}, EzBob.Underwriter.FunctionsDialogView.prototype.events, {
			'click .change-offer-details': 'changeLoanDetails',
			'click .pdf-link': 'exportToPdf',
			'click .excel-link': 'exportToExcel',
			'click .print-link': 'exportToPrint'
		});
	}, // events

	getType: function () { return 'Approved'; },

	showReasonField: function () { return true; },

	onShow: function () {
		this.renderDetails();

		this.renderSchedule();

		this.model.on('change', this.renderDetails, this);

		if (!this.model.get('OfferedCreditLine') || this.model.get('OfferedCreditLine') === 0)
			this.$el.find('.button-ok').addClass('disabled');

		if (this.model.get('OfferExpired'))
			this.$el.find('.button-ok').addClass('disabled');

		var isLoanTypeSelectionAllowed = this.model.get('IsLoanTypeSelectionAllowed');

		if ((isLoanTypeSelectionAllowed === 1) || (isLoanTypeSelectionAllowed === '1'))
			this.$el.find('.change-offer-details').attr('disabled', 'disabled');

		this.NoYodlee = this.model.get('IsOffline') && !this.model.get('HasYodlee');
		this.$el.find('#noYodleeReasonDiv').toggleClass('hide', !this.NoYodlee).toggleClass('uwReason', this.NoYodlee);
	}, // onShow

	renderDetails: function () {
		var details = _.template($('#approve-details').html(), this.model.toJSON());

		this.$el.find('#details').html(details);

		if (this.model.get('IsModified')) {
			this.$el.find('.offer-status').append('<strong>Offer was manually modified</strong>').css({
				'margin-top': '-20px'
			});
		} // if
	}, // renderDetails

	renderSchedule: function () {
		var that = this;

		$.getJSON(window.gRootPath + 'Underwriter/Schedule/Calculate', {
			id: this.model.get('CashRequestId')
		}).done(function (data) {
			if (data.hasOwnProperty('success') && !data.success) {
				EzBob.ShowMessage(data.error, 'Error occurred');
				that.$el.dialog('close');
				return false;
			} // if

			var scheduleView = new EzBob.LoanScheduleView({
				el: that.$el.find('.loan-schedule'),
				schedule: data,
				isShowGift: false,
				isShowExportBlock: false,
				isShowExceedMaxInterestForSource: true,
				ManualAddressWarning: data.ManualAddressWarning,
				customer: that.model.get('CustomerName'),
				refNum: that.model.get('CustomerRefNum'),
				isPersonal: (that.model.get('TypeOfBusinessReduced') !== 1),
			});
			scheduleView.render();
			return that.$el.find('#loan-schedule .simple-well').hide();
		});
	}, // renderSchedule

	getButtonName: function () { return 'Approve'; },

	dlgWidth: 880,

	dlgHeight: 900,

	onSaved: function () {
		this.renderSchedule();
	}, // onSaved

	changeLoanDetails: function () {
		var that = this;

		var loan = new EzBob.LoanModelTemplate({
			CashRequestId: this.model.get('CashRequestId'),
			CustomerId: this.model.get('CustomerId')
		});

		var xhr = loan.fetch();

		xhr.done(function () {
			var view = new EzBob.EditLoanView({ model: loan, });
			EzBob.App.jqmodal.show(view);
			view.on('item:saved', that.onSaved, that);
		});

		return false;
	}, // changeLoanDetails

	exportToPdf: function (e) {
		var $el = $(e.currentTarget);

		$el.attr(
			'href',
			window.gRootPath + 'Underwriter/Schedule/Export?id=' + this.model.get('CashRequestId') +
			'&isExcel=false&isShowDetails=true&customerId=' + this.model.get('CustomerId')
		);
	}, // exportToPdf

	exportToExcel: function (e) {
		var $el = $(e.currentTarget);

		$el.attr(
			'href',
			window.gRootPath + 'Underwriter/Schedule/Export?id=' + this.model.get('CashRequestId') +
			'&isExcel=true&isShowDetails=true&customerId=' + this.model.get('CustomerId')
		);
	}, // exportToExcel

	exportToPrint: function () {
		var elem = $('.loan-schedule:visible')[0];

		var domClone = elem.cloneNode(true);

		var $printSection = document.getElementById('printSection');

		if (!$printSection) {
			$printSection = document.createElement('div');
			$printSection.id = 'printSection';
			document.body.appendChild($printSection);
		} // if

		$printSection.innerHTML = '';
		$printSection.appendChild(domClone);
		window.print();

		return false;
	}, // exportToPrint
}); // EzBob.Underwriter.ApproveDialog
