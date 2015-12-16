var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CreditLineDialog = EzBob.ItemView.extend({
	template: '#credit-line-dialog-template',

	initialize: function(options) {
		this.cloneModel = this.model.clone();
		this.cloneModel.set('BrokerSetupFeePercent', options.brokerCommissionDefaultResult.brokerCommission);
		this.cloneModel.set('ManualSetupFeePercent', options.brokerCommissionDefaultResult.setupFeePercent);
		this.modelBinder = new Backbone.ModelBinder();
		this.bindTo(this.cloneModel, 'change:StartingFromDate', this.onChangeStartingDate, this);

		this.bindTo(this.cloneModel, 'change:ProductID', this.onChangeProduct, this);
		this.bindTo(this.cloneModel, 'change:ProductTypeID', this.onChangeProductType, this);
		this.bindTo(this.cloneModel, 'change:LoanTypeId', this.onChangeLoanType, this);
		this.bindTo(this.cloneModel, 'change:LoanSourceID', this.onChangeLoanSource, this);

		this.bind('close', this.closeDialog);
	}, // initialize

	events: {
		'click .btnOk': 'save',
		//'change #loan-type': 'onChangeLoanType',
		//'change #loan-source': 'onChangeLoanSource',
		'change #offeredCreditLine': 'onChangeOfferedAmout',
	}, // events

	ui: {
		form: 'form',
		offeredCreditLine: '#offeredCreditLine',
		loanProduct: '#product',
		loanProductType: '#product-type',
		loanType: '#loan-type',
		loanSource: '#loan-source',
		requestedLoanAmount: '#requestLoanAmount',
		requestedLoanTerm: '#requestedLoanTerm'
	}, // ui

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Edit offer',
			position: 'center',
			draggable: true,
			dialogClass: 'creditline-popup',
			width: 840,
		};
	}, // jqoptions

	closeDialog: function() {
		this.model.fetch();
	}, // closeDialog

	onChangeStartingDate: function() {
		var startingDate = moment.utc(this.cloneModel.get('StartingFromDate'), 'DD/MM/YYYY');

		if (startingDate) {
			var endDate = startingDate.add('hours', this.cloneModel.get('OfferValidForHours'));
			this.cloneModel.set('OfferValidateUntil', endDate.format('DD/MM/YYYY'));
		} // if
	}, // onChangeStartingDate

	onChangeOfferedAmout: function() {
		// BlockUi();
		var btnOk = this.$el.find('.btnOk');

		btnOk.hide();

		var self = this;

		$.post(window.gRootPath + 'Underwriter/ApplicationInfo/UpdateBrokerCommissionDefaults', {
			id: this.cloneModel.get('CashRequestId'),
			amount: self.ui.offeredCreditLine.autoNumeric('get')
		}).done(function(result) {
			self.cloneModel.set('BrokerSetupFeePercent', result.brokerCommission);
			self.cloneModel.set('ManualSetupFeePercent', result.setupFeePercent);
		}).always(function() {
			// UnBlockUi();
			btnOk.show();
		});
	}, // onChangeOfferedAmout

	onChangeProduct: function () {
		console.log('change p');
	},
	onChangeProductType: function () {
		console.log('change pt');
	},
	onChangeLoanType: function () {
		console.log('change lt');
		/*
		var loanTypeId = this.$el.find('#loan-type option:selected').val();

		if (isNaN(loanTypeId) || (loanTypeId <= 0))
			return;

		loanTypeId = parseInt(loanTypeId, 10);

		var currentLoanType = _.find(this.cloneModel.get('LoanTypes'), function(l) { return l.Id === loanTypeId; });

		this.cloneModel.set('RepaymentPeriod', currentLoanType.RepaymentPeriod);
		*/
	}, // onChangeLoanType

	onChangeLoanSource: function () {
		console.log('change ls');
		/*
		var loanSourceId = this.$el.find('#loan-source option:selected').val();
		if (isNaN(loanSourceId) || (loanSourceId <= 0))
			return;

		loanSourceId = parseInt(loanSourceId, 10);

		var currentLoanSource = _.find(
			this.cloneModel.get('AllLoanSources'),
			function(l) { return l.Id === loanSourceId; }
		);

		if (currentLoanSource) {
			if (currentLoanSource.DefaultRepaymentPeriod > 0)
				this.cloneModel.set('RepaymentPeriod', currentLoanSource.DefaultRepaymentPeriod);

			var maxInterestRate = currentLoanSource.MaxInterest;

			var fixInterestRate = maxInterestRate &&
			(maxInterestRate > 0) &&
			(this.cloneModel.get('InterestRate') > maxInterestRate);

			if (fixInterestRate)
				this.cloneModel.set('InterestRate', maxInterestRate);
		} // if

		if (!currentLoanSource.IsCustomerRepaymentPeriodSelectionAllowed) {
			this.cloneModel.set('IsCustomerRepaymentPeriodSelectionAllowed', false);
			this.cloneModel.set('IsLoanTypeSelectionAllowed', 0);
			this.setSomethingEnabled(this.$el.find('#repaymentPeriodSelection'), false);
		} else
			this.setSomethingEnabled(this.$el.find('#repaymentPeriodSelection'), true);

		*/
	}, // onChangeLoanSource

	save: function() {
		if (!this.ui.form.valid())
			return;

		var postData = this.getPostData();
		var action = '' + window.gRootPath + 'Underwriter/ApplicationInfo/ChangeCreditLine';
		var post = $.post(action, postData);
		var self = this;

		post.done(function() {
			self.close();
			EzBob.App.vent.trigger('newCreditLine:updated');
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
			repaymentPeriod: m.RepaymentPeriod,
			offerStart: m.StartingFromDate,
			offerValidUntil: m.OfferValidateUntil,
			brokerSetupFeePercent: m.BrokerSetupFeePercent,
			manualSetupFeePercent: m.ManualSetupFeePercent,
			allowSendingEmail: m.AllowSendingEmail,
			isLoanTypeSelectionAllowed: m.IsLoanTypeSelectionAllowed,
			isCustomerRepaymentPeriodSelectionAllowed: m.IsCustomerRepaymentPeriodSelectionAllowed,
			spreadSetupFee: m.SpreadSetupFee
		};
	}, // getPostData

	bindings: {
		OfferedCreditLine: {
			selector: '#offeredCreditLine',
			converter: EzBob.BindingConverters.moneyFormat,
		},
		InterestRate: {
			selector: 'input[name="interestRate"]',
			converter: EzBob.BindingConverters.percentsFormat,
		},
		RepaymentPeriod: {
			selector: 'input[name="repaymentPeriod"]',
			converter: EzBob.BindingConverters.notNull,
		},
		StartingFromDate: {
			selector: 'input[name="startingFromDate"]',
		},
		OfferValidateUntil: {
			selector: 'input[name="offerValidUntil"]',
		},
		AllowSendingEmail: {
			selector: 'input[name="allowSendingEmail"]',
		},
		DiscountPlanId: 'select[name="discount-plan"]',
		ProductID: 'select[name="product"]',
		ProductTypeID: 'select[name="product-type"]',
		LoanTypeId: 'select[name="loan-type"]',
		LoanSourceID: 'select[name="loan-source"]',
		ManualSetupFeePercent: {
			selector: 'input[name="manualSetupFeePercent"]',
			converter: EzBob.BindingConverters.percentsFormat,
		},
		BrokerSetupFeePercent: {
			selector: 'input[name="brokerSetupFeePercent"]',
			converter: EzBob.BindingConverters.percentsFormat,
		},
		IsLoanTypeSelectionAllowed: {
			selector: 'input[name="loanTypeSelection"]',
			converter: EzBob.BindingConverters.boolFormat,
		},
		SpreadSetupFee: {
			selector: 'input[name="spreadSetupFee"]'
		},
		IsCustomerRepaymentPeriodSelectionAllowed: {
			selector: 'input[name="repaymentPeriodSelection"]',
		},
		RequestedLoanAmount: {
			selector: 'span[name="requestedLoanAmount"]',
			converter: EzBob.BindingConverters.moneyFormat,
		},
		RequestedLoanTerm: {
			selector: 'span[name="requestedLoanTerm"]'
		},
	}, // bindings

	onRender: function() {
		this.modelBinder.bind(this.cloneModel, this.el, this.bindings);
		console.log('cloneModel', this.cloneModel);
		this.$el.find('#startingFromDate, #offerValidUntil').mask('99/99/9999').datepicker({
			autoclose: true,
			format: 'dd/mm/yyyy'
		});

		this.$el.find('#offeredCreditLine').autoNumeric('init', EzBob.moneyFormat);

		if (this.$el.find('#offeredCreditLine').val() === '-')
			this.$el.find('#offeredCreditLine').val('');

		this.$el.find('#interestRate').autoNumeric('init', EzBob.percentFormat);
		this.$el.find('#manualSetupFeePercent').autoNumeric('init', EzBob.percentFormat);
		this.$el.find('#brokerSetupFeePercent').autoNumeric('init', EzBob.percentFormat);
		this.$el.find('#repaymentPeriod').numericOnly();
		this.populateDropDowns();
		this.onChangeLoanSource();

		this.ui.form.validate({
			rules: {
				offeredCreditLine: {
					required: true,
					autonumericMin: EzBob.Config.XMinLoan,
					autonumericMax: EzBob.Config.MaxLoan,
				},
				repaymentPeriod: { required: true, autonumericMin: 1, },
				interestRate: { required: true, autonumericMin: 0, autonumericMax: 100, },
				startingFromDate: { required: true, dateCheck: true, },
				offerValidUntil: { required: true, dateCheck: true, },
				manualSetupFeePercent: { autonumericMin: 0, required: false, },
				brokerSetupFeePercent: { autonumericMin: 0, required: false, },
			},
			messages: {
				interestRate: {
					autonumericMin: $.validator.format('Interest rate is below {0}%'),
					autonumericMax: $.validator.format('Interest rate is above {0}%'),
				},
				repaymentPeriod: { autonumericMin: 'Repayment Period is below limit.', },
				startingFromDate: {
					dateCheck: 'Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012',
				},
				offerValidUntil: {
					dateCheck: 'Incorrect Date, please insert date in format DD/MM/YYYY, for example 21/06/2012',
				},
				manualSetupFeePercent: { autonumericMin: 'Cannot be negative.', },
				brokerSetupFeePercent: { autonumericMin: 'Cannot be negative.', },
			},
			errorPlacement: EzBob.Validation.errorPlacement,
			unhighlight: EzBob.Validation.unhighlight,
		});
	}, // onRender

	populateDropDowns: function() {
		var self = this;

		var currentProduct = self.cloneModel.get('CurrentProduct');
		this.ui.loanProduct.empty();
		_.each(this.cloneModel.get('Products'), function(p) {
			if (p.IsEnabled) {
				var selected = '';
				if (currentProduct && p.ProductID === currentProduct.ProductID) {
					selected = ' selected="selected" ';
				}
				self.ui.loanProduct.append($('<option value="'+ p.ProductID +'"'+ selected +'>' + p.Name +'</option>'));
			}
		});

		var currentProductType = self.cloneModel.get('CurrentProductType');
		this.ui.loanProductType.empty();
		_.each(this.cloneModel.get('ProductTypes'), function (pt) {
			if (currentProduct && pt.ProductID === currentProduct.ProductID) {
				var selected = '';
				if (currentProductType && pt.ProductTypeID === currentProductType.ProductTypeID) {
					selected = ' selected="selected" ';
				}
				self.ui.loanProductType.append($('<option value="' + pt.ProductTypeID + '"' + selected + '>' + pt.Name + '</option>'));
			}
		});

		var currentLoanTypeID = self.cloneModel.get('LoanTypeId');
		this.ui.loanType.empty();
		_.each(this.cloneModel.get('LoanTypes'), function (lt) {
			var selected = '';
			if (lt.Id === currentLoanTypeID) {
				selected = ' selected="selected" ';
			}
			self.ui.loanType.append($('<option value="' + lt.Id + '"' + selected + '>' + lt.Name + '</option>'));
		});

		var currentLoanSourceID = self.cloneModel.get('LoanSourceID');
		this.ui.loanSource.empty();
		_.each(this.cloneModel.get('AllLoanSources'), function (ls) {
			var selected = '';
			if (ls.Id === currentLoanSourceID) {
				selected = ' selected="selected" ';
			}
			self.ui.loanSource.append($('<option value="' + ls.Id + '"' + selected + '>' + ls.Name + '</option>'));
		});
	}//populateDropDowns
}); // EzBob.Underwriter.CreditLineDialog
