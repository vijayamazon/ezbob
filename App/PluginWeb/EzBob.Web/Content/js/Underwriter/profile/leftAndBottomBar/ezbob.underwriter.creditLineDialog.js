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
		'change #offeredCreditLine': 'onChangeOfferedAmout',
	}, // events

	ui: {
		form: 'form',
		errorMessage: '#error-message',
		tooltips: '.tltp',
		offeredCreditLine: '#offeredCreditLine',
		interestRate: '#interestRate',
		manualSetupFeePercent: '#manualSetupFeePercent',
		repaymentPeriod: '#repaymentPeriod',
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
		this.populateDropDowns();
	},
	onChangeProductType: function () {
		console.log('change pt');
		this.populateDropDowns();
	},
	onChangeLoanType: function () {
		console.log('change lt');
		this.populateDropDowns();
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
		this.populateDropDowns();
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
		if (!this.validator.form()) {
			return;
		}

		var postData = this.getPostData();

		if (!postData) {
			return;
		}

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

		var productSubType = _.find(this.cloneModel.get('ProductSubTypes'), function(pst) {
			return pst.ProductTypeID == m.ProductTypeID && pst.OriginID == m.OriginID && pst.GradeID == m.GradeID && pst.LoanSourceID == m.LoanSourceID;
		});

		if (!productSubType) {
			this.ui.errorMessage.text('No product found for such offer');
			return null;
		}

		return {
			id: m.CashRequestId,
			productID: m.ProductID,
			productTypeID: m.ProductTypeID,
			productSubTypeID: productSubType.ProductSubTypeID,
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
	}, // onRender

	populateDropDowns: function() {
		var self = this;
		var originID = this.model.get('OriginID');
		var gradeID = this.model.get('GradeID');

		
		var productSubTypesPerGradeAndOrigin = _.filter(this.cloneModel.get('ProductSubTypes'), function(pst) {
			return pst.GradeID === gradeID && pst.OriginID === originID;
		});

		if (productSubTypesPerGradeAndOrigin.length <= 0) {
			//todo replace ids with names
			this.ui.errorMessage.text('No products available for origin ' + originID + ' and grade ' + gradeID);
			return;
		} else {
			this.ui.errorMessage.empty();
		}

		var availableProductTypes = _.filter(this.cloneModel.get('ProductTypes'), function (pt) {
			var found = _.find(productSubTypesPerGradeAndOrigin, function (pst) { return pst.ProductTypeID === pt.ProductTypeID; });
			return found;
		});

		var availableProducts = _.filter(this.cloneModel.get('Products'), function (p) {
			var found = _.find(availableProductTypes, function (pt) { return pt.ProductID === p.ProductID; });
			return found;
		});

		var availableLoanSources = _.filter(this.cloneModel.get('AllLoanSources'), function (ls) {
			var found = _.find(productSubTypesPerGradeAndOrigin, function (pst) { return pst.LoanSourceID === ls.Id; });
			return found;
		});

		var currentProduct = self.cloneModel.get('CurrentProduct');
		this.ui.loanProduct.empty();
		_.each(availableProducts, function (p) {
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
		_.each(availableProductTypes, function (pt) {
			var selected = '';
			if (currentProductType && pt.ProductTypeID === currentProductType.ProductTypeID) {
				selected = ' selected="selected" ';
			}
			self.ui.loanProductType.append($('<option value="' + pt.ProductTypeID + '"' + selected + '>' + pt.Name + '</option>'));
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
		_.each(availableLoanSources, function (ls) {
			var selected = '';
			if (ls.Id === currentLoanSourceID) {
				selected = ' selected="selected" ';
			}
			self.ui.loanSource.append($('<option value="' + ls.Id + '"' + selected + '>' + ls.Name + '</option>'));
		});

		var isFirstLoan = this.cloneModel.get('NumOfLoans') === 0;

		var gradeRange = _.find(this.cloneModel.get('GradeRanges'), function(gr) {
			return gr.OriginID     == originID            &&
				   gr.LoanSourceID == currentLoanSourceID &&
				   gr.GradeID      == gradeID             &&
				   gr.IsFirstLoan  == isFirstLoan;
		});

		if (!gradeRange) {
			this.ui.errorMessage.text('No pricing for such product combination found origin ' + originID + ' grade ' + gradeID + ' loan source ' + currentLoanSourceID + ' is first loan ' + isFirstLoan);
			gradeRange = {};
		} else {
			this.ui.errorMessage.empty();
		}
		this.ui.form.removeData('validator');
		this.validator = EzBob.validateCreditLineDialogForm(this.ui.form, gradeRange);
		this.validator.form();

		this.setTooltips(gradeRange);
	
	},//populateDropDowns

	setTooltips: function (gradeRanges) {
		var offeredCreditLineTooltip = 'Valid between ' + GBPValues(gradeRanges.MinLoanAmount) + ' and ' + GBPValues(gradeRanges.MaxLoanAmount);
		this.ui.offeredCreditLine.attr('data-original-title', offeredCreditLineTooltip);

		var repaymentPeriodTooltip = 'Valid between ' + gradeRanges.MinTerm + ' and ' + gradeRanges.MaxTerm + ' months';
		this.ui.repaymentPeriod.attr('data-original-title', repaymentPeriodTooltip);

		var interestRateTooltip = 'Valid between ' + EzBob.formatPercents(gradeRanges.MinInterestRate) + ' and ' + EzBob.formatPercents(gradeRanges.MaxInterestRate);
		this.ui.interestRate.attr('data-original-title', interestRateTooltip);

		var manualSetupFeePercentTooltip = 'Valid between ' + EzBob.formatPercents(gradeRanges.MinSetupFee) + ' and ' + EzBob.formatPercents(gradeRanges.MaxSetupFee);
		this.ui.manualSetupFeePercent.attr('data-original-title', manualSetupFeePercentTooltip);
		_.each(this.ui.tooltips, function (t) {
			$(t).tooltip({ placement: 'bottom', trigger: 'hover focus' });
		});
	},//setTooltips
}); // EzBob.Underwriter.CreditLineDialog


EzBob.validateCreditLineDialogForm = function (el, gradeRange) {
	var e = el || $('form');

	return e.validate({
		rules: {
			offeredCreditLine: {
				required: true,
				autonumericMin: _.max([EzBob.Config.XMinLoan, gradeRange.MinLoanAmount]),
				autonumericMax: _.min([EzBob.Config.MaxLoan, gradeRange.MaxLoanAmount]),
			},
			repaymentPeriod: { required: true, min: gradeRange.MinTerm || 1, max: gradeRange.MaxTerm || 60 },
			interestRate: { required: true, autonumericMin: gradeRange.MinInterestRate * 100 || 0, autonumericMax: gradeRange.MaxInterestRate * 100 || 100, },
			startingFromDate: { required: true, dateCheck: true, },
			offerValidUntil: { required: true, dateCheck: true, },
			manualSetupFeePercent: { autonumericMin: gradeRange.MinSetupFee * 100 || 0, autonumericMax: gradeRange.MaxSetupFee * 100 || 100, required: true, },
			brokerSetupFeePercent: { autonumericMin: 0, required: false, },
		},
		messages: {
			offeredCreditLine:{
				autonumericMin: $.validator.format('Offered credit line is below £{0}'),
				autonumericMax: $.validator.format('Offered credit line is above £{0}'),
			},
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
			manualSetupFeePercent: {
				autonumericMin: $.validator.format('Setup fee is below {0}%'),
				autonumericMax: $.validator.format('Setup fee is above {0}%')
			},
			brokerSetupFeePercent: { autonumericMin: 'Cannot be negative.', },
		},
		errorPlacement: EzBob.Validation.errorPlacement,
		unhighlight: EzBob.Validation.unhighlight,
	});
}