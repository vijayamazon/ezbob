var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CreditLineDialog = EzBob.ItemView.extend({
	template: '#credit-line-dialog-template',

	initialize: function() {
		this.cloneModel = this.model.clone();
		this.modelBinder = new Backbone.ModelBinder();
		this.bindTo(this.cloneModel, "change:StartingFromDate", this.onChangeStartingDate, this);
		this.bind('close', this.closeDialog);
	}, // initialize

	events: {
		'click .btnOk': 'save',
		'change #loan-type ': 'onChangeLoanType',
		'click #isLoanTypeSelectionAllowed': 'onChangeLoanTypeSelectionAllowed',
		'change #isLoanTypeSelectionAllowed': 'onChangeLoanTypeSelectionAllowed',
		'click #enableSetupFee': 'onChangeEnableSetupFee',
	}, // events

	ui: {
		form: "form"
	}, // ui

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: "Credit Line",
			position: "center",
			draggable: false,
			dialogClass: "creditline-popup",
			width: 840
		};
	}, // jqoptions

	closeDialog: function() {
		return this.model.fetch();
	}, // closeDialog

	onChangeEnableSetupFee: function() {
		var bChecked = this.$el.find('#enableSetupFee').attr('checked') ? true : false;
		this.setSomethingEnabled('#manualSetupFeeAmount, #manualSetupFeePercent', bChecked);
	}, // onChangeEnableSetupFee

	onChangeLoanTypeSelectionAllowed: function() {
		var controlledElements = '#loan-type, #repaymentPeriod';

		var nIsLoanTypeSelectionAllowed = this.cloneModel.get('IsLoanTypeSelectionAllowed');

		if (nIsLoanTypeSelectionAllowed === 1 || nIsLoanTypeSelectionAllowed === '1') {
			this.$el.find(controlledElements).attr('disabled', 'disabled');

			if (this.cloneModel.get('LoanTypeId') !== 1)
				this.cloneModel.set('LoanTypeId', 1);
		}
		else
			this.$el.find(controlledElements).removeAttr('disabled');
	}, // onChangeLoanTypeSelectionAllowed

	onChangeStartingDate: function() {
		var startingDate = moment.utc(this.cloneModel.get("StartingFromDate"), "DD/MM/YYYY");

		if (startingDate !== null) {
			var endDate = startingDate.add('hours', this.cloneModel.get('OfferValidForHours'));
			this.cloneModel.set("OfferValidateUntil", endDate.format('DD/MM/YYYY'));
		} // if
	}, // onChangeStartingDate

	onChangeLoanType: function() {
		var loanTypeId = +this.$el.find("#loan-type option:selected").val();

		if (isNaN(loanTypeId) || (loanTypeId <= 0))
			return;

		var currentLoanType = _.find(this.cloneModel.get("LoanTypes"), function(l) { return l.Id === loanTypeId; });

		this.cloneModel.set("RepaymentPerion", currentLoanType.RepaymentPeriod);
	}, // onChangeLoanType

	save: function() {
		if (!this.ui.form.valid())
			return;

		var postData = this.getPostData();
		var action = "" + window.gRootPath + "Underwriter/ApplicationInfo/ChangeCreditLine";
		var post = $.post(action, postData);
		var self = this;

		post.done(function() {
			EzBob.App.vent.trigger('newCreditLine:updated');
		  	self.close();
		});
	}, // save

	getPostData: function() {
		var m = this.cloneModel.toJSON();
		return {
			id: m.CashRequestId,
			loanType: m.LoanTypeId,
			discountPlan: m.DiscountPlanId,
			amount: m.amount,
			interestRate: m.InterestRate,
			repaymentPeriod: m.RepaymentPerion,
			offerStart: m.StartingFromDate,
			offerValidUntil: m.OfferValidateUntil,
			useSetupFee: m.UseSetupFee,
			useBrokerSetupFee: m.UseBrokerSetupFee,
			manualSetupFeeAmount: m.ManualSetupFeeAmount,
			manualSetupFeePercent: m.ManualSetupFeePercent,
			allowSendingEmail: m.AllowSendingEmail,
			isLoanTypeSelectionAllowed: m.IsLoanTypeSelectionAllowed,
		};
	}, // getPostData

	bindings: {
		InterestRate: {
			selector: "input[name='interestRate']",
			converter: EzBob.BindingConverters.percentsFormat
		},
		RepaymentPerion: {
			selector: "input[name='repaymentPeriod']",
			converter: EzBob.BindingConverters.notNull
		},
		StartingFromDate: {
			selector: "input[name='startingFromDate']"
		},
		OfferValidateUntil: {
			selector: "input[name='offerValidUntil']"
		},
		UseSetupFee: {
			selector: "input[name='enableSetupFee']"
		},
		UseBrokerSetupFee: {
			selector: "input[name='enableBrokerSetupFee']"
		},
		AllowSendingEmail: {
			selector: "input[name='allowSendingEmail']"
		},
		IsLoanTypeSelectionAllowed: {
			selector: "select[name='isLoanTypeSelectionAllowed']"
		},
		DiscountPlanId: "select[name='discount-plan']",
		LoanTypeId: "select[name='loan-type']",
		amount: {
			selector: "#offeredCreditLine",
			converter: EzBob.BindingConverters.moneyFormat
		},
		ManualSetupFeePercent: {
			selector: "input[name='manualSetupFeePercent']",
			converter: EzBob.BindingConverters.percentsFormat
		},
		ManualSetupFeeAmount: {
			selector: "input[name='manualSetupFeeAmount']",
			converter: EzBob.BindingConverters.moneyFormat
		},
	}, // bindings

	onRender: function() {
		this.modelBinder.bind(this.cloneModel, this.el, this.bindings);

		this.$el.find("#startingFromDate, #offerValidUntil").mask("99/99/9999").datepicker({
			autoclose: true,
			format: 'dd/mm/yyyy'
		});

		this.$el.find("#offeredCreditLine").autoNumeric(EzBob.moneyFormat);

		if (this.$el.find("#offeredCreditLine").val() === "-")
			this.$el.find("#offeredCreditLine").val("");

		this.$el.find("#interestRate").autoNumeric(EzBob.percentFormat);
		this.$el.find("#manualSetupFeePercent").autoNumeric(EzBob.percentFormat);
		this.$el.find("#manualSetupFeeAmount").autoNumeric(EzBob.moneyFormat);
		this.$el.find("#repaymentPeriod").numericOnly();

		this.onChangeEnableSetupFee();

		this.ui.form.validate({
			rules: {
				offeredCreditLine: { required: true, autonumericMin: EzBob.Config.XMinLoan, autonumericMax: EzBob.Config.MaxLoan },
				repaymentPeriod: { required: true, autonumericMin: 1 },
				interestRate: { required: true, autonumericMin: 1, autonumericMax: 100 },
				startingFromDate: { required: true, dateCheck: true },
				offerValidUntil: { required: true, dateCheck: true },
				manualSetupFeeAmount: { autonumericMin: 0, required: false },
				manualSetupFeePercent: { autonumericMin: 0, required: false },
			},
			messages: {
				interestRate: { autonumericMin: "Interest Rate is below limit.", autonumericMax: "Interest Rate is above limit." },
				repaymentPeriod: { autonumericMin: "Repayment Period is below limit." },
				startingFromDate: { dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012" },
				offerValidUntil: { dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012" },
				manualSetupFeeAmount: { autonumericMin: "Can't be negative." },
				manualSetupFeePercent: { autonumericMin: "Can't be negative." }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight
		});
	}, // onRender
}); // EzBob.Underwriter.CreditLineDialog
