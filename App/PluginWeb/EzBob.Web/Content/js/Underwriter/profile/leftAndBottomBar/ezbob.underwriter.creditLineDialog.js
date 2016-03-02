var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CreditLineDialog = EzBob.ItemView.extend({
	template: '#credit-line-dialog-template',

	initialize: function(options) {
		this.cloneModel = this.model.clone();
		this.medalModel = options.medalModel;

		this.feesManuallyUpdated = !!this.model.get('UwUpdatedFees');

		if (!this.feesManuallyUpdated) {
			this.cloneModel.set('BrokerSetupFeePercent', options.brokerCommissionDefaultResult.brokerCommission);
			this.cloneModel.set('ManualSetupFeePercent', options.brokerCommissionDefaultResult.setupFeePercent);
		} // if

		this.resetFees(
			options.brokerCommissionDefaultResult.brokerCommission,
			options.brokerCommissionDefaultResult.setupFeePercent,
			this.feesManuallyUpdated
		);

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
		'click .btnLogicalGlue': 'logicalGlueTryout',
		'change #offeredCreditLine': 'onChangeOfferedAmout',

		'change #manualSetupFeePercent' : 'setupFeeUpdatedManually',
		'change #brokerSetupFeePercent' : 'brokerFeeUpdatedManually',
	}, // events

	setupFeeUpdatedManually: function() {
		this.updateFees(this.brokerFee, this.$el.find('#manualSetupFeePercent').autoNumeric('get'));
	}, // setupFeeUpdatedManually

	brokerFeeUpdatedManually: function() {
		this.updateFees(this.$el.find('#brokerSetupFeePercent').autoNumeric('get'), this.setupFee);
	}, // brokerFeeUpdatedManually

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
		discountPlan: '#discount-plan',
		fundingType: '#funding-type',
		btnLogicalGlue: '.btnLogicalGlue'
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
			self.resetFees(result.brokerCommission, result.setupFeePercent);
		}).always(function() {
			// UnBlockUi();
			btnOk.show();
		});
	}, // onChangeOfferedAmout

	resetFees: function(brokerFee, setupFee, manuallyUpdated) {
		this.brokerFee = brokerFee;
		this.setupFee = setupFee;

		this.feesManuallyUpdated = (manuallyUpdated === undefined) ? false : manuallyUpdated;
		console.log('loaded fees', this.brokerFee, this.setupFee, this.feesManuallyUpdated);
	}, // resetFees

	updateFees: function(brokerFee, setupFee) {
		var oldBrokerFee = this.brokerFee;
		var oldSetupFee = this.setupFee;

		this.brokerFee = brokerFee;
		this.setupFee = setupFee;

		var brokerChanged = (oldBrokerFee !== brokerFee);
		var setupChanged = (oldSetupFee !== setupFee);

		this.feesManuallyUpdated = this.feesManuallyUpdated || brokerChanged || setupChanged;

		console.log(
			'fees updated from', oldBrokerFee, oldSetupFee,
			'to', this.brokerFee, this.setupFee,
			'changed', (brokerChanged ? 'broker' : ''), (setupChanged ? 'setup' : ''),
			(this.feesManuallyUpdated ? 'manually updated' : '')
		);
	}, // updateFees

	onChangeProduct: function () {
		this.populateDropDowns();
	},

	onChangeProductType: function () {
		this.populateDropDowns();
	},

	onChangeLoanType: function () {
		this.populateDropDowns();
	}, // onChangeLoanType

	onChangeLoanSource: function (that) {
		if (that !== null) {
			this.populateDropDowns();
		}

		var self = this;
		var currentLoanSource = _.find(
			this.cloneModel.get('AllLoanSources'),
			function (l) { return l.Id == self.cloneModel.get('LoanSourceID'); }
		);

		if (!currentLoanSource.IsCustomerRepaymentPeriodSelectionAllowed) {
			this.cloneModel.set('IsCustomerRepaymentPeriodSelectionAllowed', false);
			this.cloneModel.set('IsLoanTypeSelectionAllowed', 0);
			this.setSomethingEnabled(this.$el.find('#repaymentPeriodSelection'), false);
		} else
			this.setSomethingEnabled(this.$el.find('#repaymentPeriodSelection'), true);
	}, // onChangeLoanSource

	save: function() {
		if (!this.validator.form()) {
			return;
		}

		var postData = this.getPostData();

		if (!postData)
			return;

		var action = '' + window.gRootPath + 'Underwriter/ApplicationInfo/ChangeCreditLine';
		var post = $.post(action, postData);
		var self = this;

		post.done(function() {
			self.close();
			EzBob.App.vent.trigger('newCreditLine:updated');
		});
	}, // save

	logicalGlueTryout: function () {
		BlockUi();
		var action = '' + window.gRootPath + 'Underwriter/ApplicationInfo/LogicalGlueTryout';
		var m = this.cloneModel.toJSON();
		
		var postData = {
			customerID: m.CustomerId,
			cashRequestID: m.CashRequestId,
			amount: m.OfferedCreditLine,
			repaymentPeriod: m.RepaymentPeriod,
		};
		
		var xhr = $.post(action, postData);
		var self = this;

		xhr.done(function (res) {
			var logicalGlueModel = new Backbone.Model(res);
			var dialog = new EzBob.Underwriter.LogicalGluePopupView({
				model: logicalGlueModel,
				customerID: m.CustomerId
			});
			dialog.render();
			dialog.on('bucketChanged', self.fetchModels, self);

			if (self.medalModel) {
				self.medalModel.fetch();
			}
		});

		xhr.always(function() {
			UnBlockUi();
		});
	},

	fetchModels: function () {
		this.model.fetch();
		if (this.medalModel) {
			this.medalModel.fetch();
		}
	},

	getCurrentProductSubType: function () {
		var self = this;
		var originID = this.model.get('OriginID');
		var gradeID = this.model.get('GradeID');
		
		var productSubTypesPerOrigin = _.filter(this.cloneModel.get('ProductSubTypes'), function (pst) {
			return (pst.OriginID == originID && (pst.FundingTypeID == null || gradeID != null));
		});

		var productSubType = _.find(productSubTypesPerOrigin, function (pst) {
			return pst.ProductTypeID == self.ui.loanProductType.val() &&
				   pst.OriginID == originID &&
				   pst.LoanSourceID  == self.ui.loanSource.val();
		});

		if (!productSubType) {
			var productType = _.find(this.cloneModel.get('ProductTypes'), function(pt) { return pt.ProductTypeID == self.cloneModel.get('CurrentProductTypeID'); }) || {};
			var loanSource = _.find(this.cloneModel.get('AllLoanSources'), function (ls) { return ls.Id == self.cloneModel.get('LoanSourceID'); }) || {};  
			this.ui.errorMessage.text('No product found for such offer origin: ' +
				this.cloneModel.get('Origin') +
				', loanSource: ' + (loanSource.Name || '') +
				', product type: ' + (productType.Name || ''));
			return null;
		}else {
			this.ui.errorMessage.empty();
		}
		return productSubType;
	},

	getPostData: function() {
		var m = this.cloneModel.toJSON();
		var productSubType = this.getCurrentProductSubType();

		if (!productSubType)
			return null;

		return {
			id: m.CashRequestId,
			productID: m.CurrentProductID,
			productTypeID: m.CurrentProductTypeID,
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
			spreadSetupFee: m.SpreadSetupFee,
			feesManuallyUpdated: this.feesManuallyUpdated,
		};
	}, // getPostData

	onRender: function () {
		var bindings = {
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
			Origin: {
				selector: 'span[name="origin"]'
			},
			Grade: {
				selector: 'span[name="gradeValue"]',
				converter: this.currentGrade
			},
			SubGrade: {
				selector: 'span[name="subGradeValue"]',
				converter: this.currentSubGrade
			},
			BrokerID: {
				selector: 'span[name="broker"]',
				converter: this.currentBroker
			}
		};

		this.modelBinder.bind(this.cloneModel, this.el, bindings);
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

		if (this.model.get('IsLimited')) {
			this.ui.btnLogicalGlue.show();
		}

		this.populateDropDowns();
		this.onChangeLoanSource(null);
	}, // onRender

	currentGrade: function (direction, value, attribute, model) {
		var gradeID = model.get('GradeID');
		if (!gradeID) { return 'medal: ' + model.get('Medal'); }
		var grade = _.find(model.get('Grades'), function(gr) { return gr.GradeID == gradeID; }) || { name: gradeID };
		return 'grade: ' + grade.Name;
	},

	currentSubGrade: function (direction, value, attribute, model) {
		var subGradeID = model.get('SubGradeID');
		if (!subGradeID) { return ''; }
		var subGrade = _.find(model.get('SubGrades'), function (sg) { return sg.SubGradeID == subGradeID; }) || { name: subGradeID };
		return subGrade.Name;
	},

	currentBroker: function (direction, value, attribute, model) {
		var brokerID = model.get('BrokerID');
		return brokerID ? 'broker' : '';
	},

	populateDropDowns: function() {
		var self = this;
		var originID = this.model.get('OriginID');
		var gradeID = this.model.get('GradeID');
		var subGradeID = this.model.get('SubGradeID');
		var grade = _.find(this.model.get('Grades'), function (g) { return g.GradeID == gradeID; }) || { Name: '' };

		var productSubTypesPerOrigin = _.filter(this.cloneModel.get('ProductSubTypes'), function(pst) {
			return (pst.OriginID == originID && (pst.FundingTypeID == null || gradeID != null));
		});
		if (productSubTypesPerOrigin.length <= 0) {
			
			this.ui.errorMessage.text('No products available for origin: ' + this.model.get('Origin') + ', and grade: ' + grade.Name);
			return;
		} else {
			this.ui.errorMessage.empty();
		}

		var availableProductTypes = _.filter(this.model.get('ProductTypes'), function (pt) {
			var found = _.find(productSubTypesPerOrigin, function (pst) { return pst.ProductTypeID === pt.ProductTypeID; });
			return found;
		});

		var availableProducts = _.filter(this.model.get('Products'), function (p) {
			var found = _.find(availableProductTypes, function (pt) { return pt.ProductID === p.ProductID; });
			return found;
		});

		var availableLoanSources = _.filter(this.model.get('AllLoanSources'), function (ls) {
			var found = _.find(productSubTypesPerOrigin, function (pst) { return pst.LoanSourceID === ls.Id; });
			return found;
		});

		var currentProduct = this.ui.loanProduct.val();
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

		var currentProductType = this.ui.loanProductType.val();
		this.ui.loanProductType.empty();
		_.each(availableProductTypes, function (pt) {
			var selected = '';
			if (currentProductType && pt.ProductTypeID === currentProductType.ProductTypeID) {
				selected = ' selected="selected" ';
			}
			self.ui.loanProductType.append($('<option value="' + pt.ProductTypeID + '"' + selected + '>' + pt.Name + '</option>'));
		});

		var currentLoanTypeID = this.ui.loanType.val();
		this.ui.loanType.empty();
		_.each(this.cloneModel.get('LoanTypes'), function (lt) {
			var selected = '';
			if (lt.Id === currentLoanTypeID) {
				selected = ' selected="selected" ';
			}
			self.ui.loanType.append($('<option value="' + lt.Id + '"' + selected + '>' + lt.Name + '</option>'));
		});

		var currentLoanSourceID = this.ui.loanSource.val();
		if (currentLoanSourceID)
			currentLoanSourceID = parseInt(currentLoanSourceID, 10);
		this.ui.loanSource.empty();
		_.each(availableLoanSources, function (ls) {
			var opt = $('<option></option>').text(ls.Name).val(ls.Id);

			if (ls.Id === currentLoanSourceID)
				opt.attr('selected', 'selected');

			self.ui.loanSource.append(opt);
		});
		if (!currentLoanSourceID)
			currentLoanSourceID = this.ui.loanSource.val();

		var currentDiscountPlanID = this.ui.discountPlan.val();
		this.ui.discountPlan.empty();
		_.each(self.model.get('DiscountPlans'), function (dp) {
			var selected = '';
			if (dp.Id == currentDiscountPlanID) {
				selected = ' selected="selected" ';
			}
			self.ui.discountPlan.append($('<option value="' + dp.Id + '"' + selected + '>' + dp.Name + '</option>'));
		});
		
		var isFirstLoan = this.model.get('NumOfLoans') === 0;

		var productSubType = this.getCurrentProductSubType();
		if (productSubType && productSubType.FundingTypeID) {
			var fundingType = _.find(this.model.get('FundingTypes'), function(ft) {
				return ft.FundingTypeID == productSubType.FundingTypeID;
			});

			if (fundingType) {
				this.ui.fundingType.val(fundingType.Name);
			} else {
				this.ui.fundingType.val('');
			}
		}

		var gradeRange = _.find(this.model.get('GradeRanges'), function(gr) {
			return gr.OriginID     == originID            &&
				   gr.LoanSourceID == currentLoanSourceID &&
				   gr.GradeID      == gradeID             &&
				   gr.SubGradeID   == subGradeID          &&
				   gr.IsFirstLoan  == isFirstLoan;
		});

		if (!gradeRange) {
			var subGrade = _.find(this.model.get('SubGrades'), function (sg) { return sg.SubGradeID == subGradeID; }) || { Name: subGradeID };
			var loanSource = _.find(this.model.get('AllLoanSources'), function (ls) { return ls.Id == currentLoanSourceID; }) || { Name: currentLoanSourceID };
			this.ui.errorMessage.text('No pricing for such product combination found origin: ' +
				this.model.get('Origin') +
				', grade: ' + grade.Name +
				', subGrade: ' + subGrade.Name,
				', loan source: ' + loanSource.Name +
				', is first loan: ' + isFirstLoan ? 'yes' : 'no');
			console.log(originID, currentLoanSourceID, gradeID, subGradeID, isFirstLoan, this.model.get('GradeRanges'));
			gradeRange = {};
		} else {
			this.ui.errorMessage.empty();
		}
		this.ui.form.removeData('validator');
		this.validator = EzBob.validateCreditLineDialogForm(this.ui.form, gradeRange, this.model.get('BrokerID'));
		this.validator.form();

		this.setTooltips(gradeRange);
	
	},//populateDropDowns

	setTooltips: function (gradeRanges) {
		var offeredCreditLineTooltip = 'Valid between ' + GBPValues(gradeRanges.MinLoanAmount, true) + ' and ' + GBPValues(gradeRanges.MaxLoanAmount, true);
		this.ui.offeredCreditLine.parent().tooltip('destroy').tooltip({ title: offeredCreditLineTooltip, trigger: 'hover focus', placement: 'bottom' });

		var repaymentPeriodTooltip = 'Valid between ' + gradeRanges.MinTerm + ' and ' + gradeRanges.MaxTerm + ' months';
		this.ui.repaymentPeriod.parent().tooltip('destroy').tooltip({ title: repaymentPeriodTooltip, trigger: 'hover focus', placement: 'bottom' });

		var interestRateTooltip = 'Valid between ' + EzBob.formatPercents(gradeRanges.MinInterestRate) + ' and ' + EzBob.formatPercents(gradeRanges.MaxInterestRate);
		this.ui.interestRate.parent().tooltip('destroy').tooltip({ title: interestRateTooltip, trigger: 'hover focus', placement: 'bottom' });

		var manualSetupFeePercentTooltip = 'Valid between ' + EzBob.formatPercents(gradeRanges.MinSetupFee) + ' and ' + EzBob.formatPercents(gradeRanges.MaxSetupFee);
		this.ui.manualSetupFeePercent.parent().tooltip('destroy').tooltip({ title: manualSetupFeePercentTooltip, trigger: 'hover focus', placement: 'bottom' });
	},//setTooltips
}); // EzBob.Underwriter.CreditLineDialog

EzBob.Underwriter.LogicalGluePopupView = EzBob.ItemView.extend({
	template: '#logical-glue-template',

	initialize: function (options) {
		this.$el.dialog(this.jqoptions());
		this.customerID = options.customerID;
	},//initialize

	events: {
		'click #logical-glue-set-current-btn': 'setAsCurrent'
	},//events

	onRender: function() {
		this.$el.find('[data-toggle="tooltip"]').tooltip({
			html: true,
			'placement': 'bottom'
		});
	},//onRender

	jqoptions: function () {
		var that = this;
		return {
			modal: true,
			resizable: false,
			title: 'Logical glue',
			position: 'center',
			draggable: true,
			dialogClass: 'logical-glue-popup',
			width: 400,
			close: function () {
				$(this).dialog("destroy");
				return that.trigger("close");
			}
		};
	}, // jqoptions

	serializeData: function () {
		return {
			logicalGlue: this.model.toJSON()
		}
	},//serializeData

	setAsCurrent: function () {
		BlockUi();
		var uniqueID = this.model.get('UniqueID');
		var action = '' + window.gRootPath + 'Underwriter/ApplicationInfo/LogicalGlueSetAsCurrent';
		
		var postData = {
			customerID: this.customerID,
			uniqueID: uniqueID
		};

		var xhr = $.post(action, postData);
		var self = this;

		xhr.done(function (res) {
			self.trigger('bucketChanged');
			self.close();
		});

		xhr.always(function () {
			UnBlockUi();
		});

		
		return false;
	}, // setAsCurrent
});

EzBob.validateCreditLineDialogForm = function(el, gradeRange, isBroker) {
	var e = el || $('form');

	var isManagerUW = $('body').hasClass('role-SuperUser') || $('body').hasClass('role-manager') || $('body').hasClass('role-Underwriter');

	var minSetupFeePercent = isBroker || isManagerUW ? 0 : (gradeRange.MinSetupFee * 100).toFixed(2);
	var maxSetupFeePercent = isManagerUW ? 100 : (gradeRange.MaxSetupFee * 100).toFixed(2);
	var minInterestRate = isManagerUW ? 0 : (gradeRange.MinInterestRate * 100).toFixed(2);
	var maxInterestRate = isManagerUW ? 100 : (gradeRange.MaxInterestRate * 100).toFixed(2);
	return e.validate({
		rules: {
			offeredCreditLine: {
				required: true,
				autonumericMin: _.max([EzBob.Config.XMinLoan, gradeRange.MinLoanAmount]),
				autonumericMax: _.min([EzBob.Config.MaxLoan, gradeRange.MaxLoanAmount]),
			},
			repaymentPeriod: { required: true, min: gradeRange.MinTerm, max: gradeRange.MaxTerm },
			startingFromDate: { required: true, dateCheck: true, },
			offerValidUntil: { required: true, dateCheck: true, },
			brokerSetupFeePercent: { autonumericMin: 0, autonumericMax: 100, required: false, },
			interestRate: { required: true, autonumericMin: minInterestRate, autonumericMax: maxInterestRate, },
			manualSetupFeePercent: { autonumericMin: minSetupFeePercent, autonumericMax: maxSetupFeePercent, required: true, },
		},
		messages: {
			offeredCreditLine: {
				autonumericMin: $.validator.format('Offered credit line is below &pound;{0}'),
				autonumericMax: $.validator.format('Offered credit line is above &pound;{0}'),
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
} //validateCreditLineDialogForm