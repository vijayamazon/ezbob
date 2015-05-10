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
		'change #loan-type': 'onChangeLoanType',
		'change #loan-source': 'onChangeLoanSource',
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

	onChangeStartingDate: function() {
		var startingDate = moment.utc(this.cloneModel.get("StartingFromDate"), "DD/MM/YYYY");

		if (startingDate !== null) {
			var endDate = startingDate.add('hours', this.cloneModel.get('OfferValidForHours'));
			this.cloneModel.set("OfferValidateUntil", endDate.format('DD/MM/YYYY'));
		} // if
	}, // onChangeStartingDate

	onChangeLoanType: function() {
		var loanTypeId = this.$el.find("#loan-type option:selected").val();

		if (isNaN(loanTypeId) || (loanTypeId <= 0))
			return;

		var currentLoanType = _.find(this.cloneModel.get("LoanTypes"), function(l) { return l.Id == loanTypeId; });

		this.cloneModel.set("RepaymentPerion", currentLoanType.RepaymentPeriod);
	}, // onChangeLoanType

	onChangeLoanSource: function () {
	    var loanSourceId = this.$el.find("#loan-source option:selected").val();
	    if (isNaN(loanSourceId) || (loanSourceId <= 0))
	        return;

	    var currentLoanSource = _.find(this.cloneModel.get("AllLoanSources"), function (l) { return l.Id == loanSourceId; });
	    if (currentLoanSource && currentLoanSource.DefaultRepaymentPeriod > 0) {
	        this.cloneModel.set("RepaymentPerion", currentLoanSource.DefaultRepaymentPeriod);
	    }

	    if (currentLoanSource && currentLoanSource.MaxInterest && currentLoanSource.MaxInterest > 0 && this.cloneModel.get("InterestRate") > currentLoanSource.MaxInterest) {
	        this.cloneModel.set("InterestRate", currentLoanSource.MaxInterest);
	    }

	    if (!currentLoanSource.IsCustomerRepaymentPeriodSelectionAllowed) {
	        this.cloneModel.set("IsCustomerRepaymentPeriodSelectionAllowed", false);
	        this.cloneModel.set("IsLoanTypeSelectionAllowed", 0);
	        this.$el.find("#repaymentPeriodSelection").addClass('disabled');
	    } else {
	        this.$el.find("#repaymentPeriodSelection").removeClass('disabled');
	    }
	},

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
			loanSource: m.LoanSourceID,
			discountPlan: m.DiscountPlanId,
			amount: m.OfferedCreditLine,
			interestRate: m.InterestRate,
			repaymentPeriod: m.RepaymentPerion,
			offerStart: m.StartingFromDate,
			offerValidUntil: m.OfferValidateUntil,
			brokerSetupFeePercent: m.BrokerSetupFeePercent,
			manualSetupFeePercent: m.ManualSetupFeePercent,
			allowSendingEmail: m.AllowSendingEmail,
			isLoanTypeSelectionAllowed: m.IsLoanTypeSelectionAllowed,
			isCustomerRepaymentPeriodSelectionAllowed: m.IsCustomerRepaymentPeriodSelectionAllowed
		};
	}, // getPostData

	bindings: {
	    OfferedCreditLine: {
	        selector: "#offeredCreditLine",
	        converter: EzBob.BindingConverters.moneyFormat
	    },
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
		AllowSendingEmail: {
			selector: "input[name='allowSendingEmail']"
		},
		DiscountPlanId: "select[name='discount-plan']",
		LoanTypeId: "select[name='loan-type']",
		LoanSourceID: "select[name='loan-source']",
		ManualSetupFeePercent: {
			selector: "input[name='manualSetupFeePercent']",
			converter: EzBob.BindingConverters.percentsFormat
		},
		BrokerSetupFeePercent: {
		    selector: "input[name='brokerSetupFeePercent']",
			converter: EzBob.BindingConverters.percentsFormat
		},
		IsLoanTypeSelectionAllowed: {
		    selector: "input[name='loanTypeSelection']",
		    converter: EzBob.BindingConverters.boolFormat
		},
		IsCustomerRepaymentPeriodSelectionAllowed: {
		    selector: "input[name='repaymentPeriodSelection']"
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
		this.$el.find("#brokerSetupFeePercent").autoNumeric(EzBob.percentFormat);
		
		this.$el.find("#repaymentPeriod").numericOnly();

		this.onChangeLoanSource();

		this.ui.form.validate({
			rules: {
				offeredCreditLine: { required: true, autonumericMin: EzBob.Config.XMinLoan, autonumericMax: EzBob.Config.MaxLoan },
				repaymentPeriod: { required: true, autonumericMin: 1 },
				interestRate: { required: true, autonumericMin: 1, autonumericMax: 100 },
				startingFromDate: { required: true, dateCheck: true },
				offerValidUntil: { required: true, dateCheck: true },
				manualSetupFeePercent: { autonumericMin: 0, required: false },
				brokerSetupFeePercent: { autonumericMin: 0, required: false },
			},
			messages: {
				interestRate: { autonumericMin: "Interest Rate is below limit.", autonumericMax: "Interest Rate is above limit." },
				repaymentPeriod: { autonumericMin: "Repayment Period is below limit." },
				startingFromDate: { dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012" },
				offerValidUntil: { dateCheck: "Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012" },
				manualSetupFeePercent: { autonumericMin: "Can't be negative." },
			    brokerSetupFeePercent: { autonumericMin: "Can't be negative." }
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight
		});
	}, // onRender
}); // EzBob.Underwriter.CreditLineDialog
