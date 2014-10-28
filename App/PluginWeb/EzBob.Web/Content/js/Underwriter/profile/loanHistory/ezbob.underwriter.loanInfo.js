var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

(function() {
	var ModelUpdater = (function() {
		function ModelUpdater(model, property) {
			this.model = model;
			this.property = property;
			this.start = _.bind(this.start, this);
		}

		ModelUpdater.prototype.start = function() {
			var self = this;

			var xhr = this.model.fetch();

			xhr.done(function() {
				self.check();
			});
		};

		ModelUpdater.prototype.check = function() {
			if (Convert.toBool(this.model.get(this.property))) {
				BlockUi('off');

				if (this.model.get('CreditResult') !== "WaitingForDecision") {
					EzBob.App.vent.trigger('newCreditLine:pass');
					return;
				} // if

				if (this.model.get('StrategyError') !== null)
					EzBob.App.vent.trigger('newCreditLine:error', this.model.get('StrategyError'));
				else
					EzBob.App.vent.trigger('newCreditLine:done');
			} else
				setTimeout(this.start, 1000);
		};

		return ModelUpdater;
	})();

	EzBob.Underwriter.LoanInfoView = Backbone.Marionette.ItemView.extend({
		template: "#profile-loan-info-template",

		initialize: function(options) {
			this.bindTo(this.model, "change reset sync", this.render, this);
			this.personalInfo = options.personalInfo;
			this.bindTo(this.personalInfo, "change", this.UpdateNewCreditLineState, this);
			this.bindTo(this.personalInfo, "change:CreditResult", this.changeCreditResult, this);
			EzBob.App.vent.on('newCreditLine:done', this.showCreditLineDialog, this);
			EzBob.App.vent.on('newCreditLine:error', this.showErrorDialog, this);
			EzBob.App.vent.on('newCreditLine:pass', this.showNothing, this);
			return this.parentView = options.parentView;
		},

		events: {
			"click [name='startingDateChangeButton']": "editStartingDate",
			"click [name='offerValidUntilDateChangeButton']": "editOfferValidUntilDate",
			"click [name='repaymentPeriodChangeButton']": "editRepaymentPeriod",
			"click [name='interestRateChangeButton']": "editInterestRate",
			"click [name='openCreditLineChangeButton']": "editOfferedCreditLine",
			"click [name='editDetails']": "editDetails",
			"click [name='manualSetupFeeEditAmountButton']": "editManualSetupFeeAmount",
			"click [name='manualSetupFeeEditPercentButton']": "editManualSetupFeePercent",
			"click [name='newCreditLineBtn']": "runNewCreditLine",
			'click [name="loanType"]': 'loanType',
			'click [name="isLoanTypeSelectionAllowed"]': 'isLoanTypeSelectionAllowed',
			'click [name="discountPlan"]': 'discountPlan',
			'click [name="loanSource"]': 'loanSource',
			'click .create-loan-hidden-toggle': 'toggleCreateLoanHidden',
			'click #create-loan-hidden-btn': 'createLoanHidden'
		},

		toggleCreateLoanHidden: function(event) {
			if (!event.ctrlKey)
				return;

			this.$el.find('#create-loan-hidden').toggleClass('hide');
		},

		createLoanHidden: function() {
			var nCustomerID = this.model.get('CustomerId');
			var nAmount = parseInt(this.$el.find('#create-loan-hidden-amount').val(), 10) || 0;
			var sDate = this.$el.find('#create-loan-hidden-date').val();

			if (nAmount <= 0) {
				EzBob.ShowMessageTimeout('Amount not specified.', 'Cannot create loan', 2);
				return;
			}

			if (!/^\d\d\d\d-\d\d-\d\d$/.test(sDate)) {
				EzBob.ShowMessageTimeout('Date not specified.', 'Cannot create loan', 2);
				return;
			}

			var oXhr = $.post(window.gRootPath + 'Underwriter/ApplicationInfo/CreateLoanHidden', {
				nCustomerID: nCustomerID,
				nAmount: nAmount,
				sDate: sDate
			});

			var self = this;

			oXhr.done(function(res) {
				if (res.success) {
					self.$el.find('#create-loan-hidden-amount').val('');
					self.$el.find('#create-loan-hidden-date').val('');
					self.$el.find('#create-loan-hidden').addClass('hide');
				}

				if (res.error)
					EzBob.ShowMessage(res.error, 'Cannot create loan');
				else
					EzBob.ShowMessage('Loan created successfully', 'Loan created successfully');
			});

			oXhr.fail(function() {
				EzBob.ShowMessage('Failed to create loan.', 'Cannot create loan');
			});
		},

		editOfferValidUntilDate: function() {
			var d = new EzBob.Dialogs.DateEdit({
				model: this.model,
				propertyName: "OfferValidateUntil",
				title: "Offer valid until edit",
				width: 370,
				postValueName: "date",
				url: "Underwriter/ApplicationInfo/ChangeOferValid",
				data: {
					id: this.model.get("CustomerId")
				}
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		editStartingDate: function() {
			var that = this;

			var d = new EzBob.Dialogs.DateEdit({
				model: this.model,
				propertyName: "StartingFromDate",
				title: "Starting date edit",
				width: 400,
				postValueName: "date",
				url: "Underwriter/ApplicationInfo/ChangeStartingDate",
				data: {
					id: this.model.get("CustomerId")
				}
			});

			d.render();

			d.on("done", function() {
				that.model.fetch();
			});
		},

		editRepaymentPeriod: function() {
			var d = new EzBob.Dialogs.IntegerEdit({
				model: this.model,
				propertyName: "RepaymentPerion",
				title: "Repayment period edit",
				width: 400,
				postValueName: "period",
				url: "Underwriter/ApplicationInfo/ChangeCashRequestRepaymentPeriod",
				data: {
					id: this.model.get("CashRequestId")
				},
				required: true
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		editOfferedCreditLine: function() {
			var that = this;

			this.model.set('amount', this.model.get('OfferedCreditLine'));

			var view = new EzBob.Underwriter.CreditLineEditDialog({
				model: this.model
			});

			EzBob.App.jqmodal.show(view);

			view.on("showed", function() {
				view.$el.find("input").focus();
			});

			view.on("done", function() {
				that.model.fetch();
			});
		},

		editInterestRate: function() {
			var d = new EzBob.Dialogs.PercentsEdit({
				model: this.model,
				propertyName: "InterestRate",
				title: "Interest rate edit",
				width: 400,
				postValueName: "interestRate",
				url: "Underwriter/ApplicationInfo/ChangeCashRequestInterestRate",
				data: {
					id: this.model.get("CashRequestId")
				},
				required: true
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		editDetails: function() {
			var d = new EzBob.Dialogs.TextEdit({
				model: this.model,
				propertyName: "Details",
				title: "Details edit",
				width: 400,
				postValueName: "details",
				url: "Underwriter/ApplicationInfo/SaveDetails",
				data: {
					id: this.model.get("CustomerId")
				}
			});
			d.render();
		},

		editManualSetupFeeAmount: function() {
			var d = new EzBob.Dialogs.PoundsNoDecimalsEdit({
				model: this.model,
				propertyName: "ManualSetupFeeAmount",
				title: "Manual setup fee amount edit",
				width: 400,
				postValueName: "manualAmount",
				url: "Underwriter/ApplicationInfo/ChangeManualSetupFeeAmount",
				data: {
					id: this.model.get("CashRequestId")
				},
				required: false
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		editManualSetupFeePercent: function() {
			var d = new EzBob.Dialogs.PercentsEdit({
				model: this.model,
				propertyName: "ManualSetupFeePercent",
				title: "Manual setup fee percent edit",
				width: 400,
				postValueName: "manualPercent",
				url: "Underwriter/ApplicationInfo/ChangeManualSetupFeePercent",
				data: {
					id: this.model.get("CashRequestId")
				},
				required: false
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		runNewCreditLine: function(e) {
			if ($(e.currentTarget).hasClass("disabled"))
				return false;

			var el = ($("<select/>")).css("height", "30px").css("width", "270px").append("<option value='1'>Skip everything, go to manual decision</option>").append("<option value='2'>Update everything except of MP's and go to manual decisions</option>").append("<option value='3'>Update everything and apply auto rules</option>").append("<option value='4'>Update everything and go to manual decision</option>");

			var self = this;

			EzBob.ShowMessage(el, "New Credit Line Option", (function() {
				self.RunCustomerCheck(el.val());
			}), "OK", null, "Cancel");
			return false;
		},

		RunCustomerCheck: function(newCreditLineOption) {
			var that = this;

			BlockUi("on");

			$.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLine", {
				Id: this.model.get("CustomerId"),
				NewCreditLineOption: newCreditLineOption
			}).done(function(response) {
				if (response.Message === "Go to new mode") {
					$.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLineNewMode1", {
						Id: that.model.get("CustomerId"),
						NewCreditLineOption: newCreditLineOption
					}).done(function(innerResponse) {
						$.post(window.gRootPath + "Underwriter/ApplicationInfo/RunNewCreditLineNewMode2", {
							Id: that.model.get("CustomerId"),
							NewCreditLineOption: newCreditLineOption
						}).done(function(innerResponse2) {
							that.personalInfo.fetch().done(function() {
								BlockUi('off');

								if (that.personalInfo.get('CreditResult') !== "WaitingForDecision") {
									EzBob.App.vent.trigger('newCreditLine:pass');
									return;
								}

								if (that.personalInfo.get('StrategyError') !== null && that.personalInfo.get('StrategyError') !== '') {
									EzBob.App.vent.trigger('newCreditLine:error', that.personalInfo.get('StrategyError'));
								} else {
									EzBob.App.vent.trigger('newCreditLine:done');
								}
							});
						});
					});
				} else {
					var updater = new ModelUpdater(self.personalInfo, 'IsMainStratFinished');
					updater.start();
				}
			}).fail(function(data) {
				console.error(data.responseText);
			});
		},

		isLoanTypeSelectionAllowed: function() {
			var d = new EzBob.Dialogs.ComboEdit({
				model: this.model,
				propertyName: "IsLoanTypeSelectionAllowed",
				title: "Customer selection",
				width: 400,
				postValueName: "loanTypeSelection",
				comboValues: [
				  {
				  	value: 0,
				  	text: 'Disabled'
				  }, {
				  	value: 1,
				  	text: 'Enabled'
				  }
				],
				url: "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed",
				data: {
					id: this.model.get("CashRequestId")
				}
			});

			d.render();

			var self = this;

			d.on('done', function() {
				self.LoanTypeSelectionAllowedChanged();
			});
		},

		LoanTypeSelectionAllowedChanged: function () {
		    // We used to set the loan type here and the availability of the edit repayment period button here
		    // Until a logic for that will be defined clearly we'll do nothing in this event
		},

		loanType: function() {
			var d = new EzBob.Dialogs.ComboEdit({
				model: this.model,
				propertyName: "LoanTypeId",
				title: "Loan type",
				width: 400,
				comboValues: this.model.get('LoanTypes'),
				postValueName: "LoanType",
				url: "Underwriter/ApplicationInfo/LoanType",
				data: {
					id: this.model.get("CashRequestId")
				}
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		loanSource: function() {
			var d = new EzBob.Dialogs.ComboEdit({
				model: this.model,
				propertyName: "LoanSource.LoanSourceID",
				title: "Loan source",
				width: 400,
				comboValues: _.map(this.model.get('AllLoanSources'), function(ls) {
					return {
						value: ls.Id,
						text: ls.Name
					};
				}),
				postValueName: "LoanSourceID",
				url: "Underwriter/ApplicationInfo/LoanSource",
				data: {
					id: this.model.get("CashRequestId")
				}
			});

			d.render();

			var self = this;

			d.on("done", function () {
				self.model.fetch();
			});
		},

		validateLoanSourceRelated: function() {
			var loanSourceModel = this.model.get('LoanSource') || {};

			this.validateInterestVsSource(loanSourceModel.MaxInterest);

			if (loanSourceModel.DefaultRepaymentPeriod === -1)
				this.$el.find('button[name=repaymentPeriodChangeButton]').removeAttr('disabled');
			else
				this.$el.find('button[name=repaymentPeriodChangeButton]').attr('disabled', 'disabled');

			this.parentView.clearDecisionNotes();

			if (loanSourceModel.MaxEmployeeCount !== -1) {
				var nEmployeeCount = this.model.get('EmployeeCount');

				if (nEmployeeCount >= 0 && nEmployeeCount > loanSourceModel.MaxEmployeeCount)
					this.parentView.appendDecisionNote('<div class=red>Employee count (' + nEmployeeCount + ') is greater than max employee count (' + loanSourceModel.MaxEmployeeCount + ') for this loan source.</div>');
			}

			if (loanSourceModel.MaxAnnualTurnover !== -1) {
				var nAnnualTurnover = this.model.get('AnnualTurnover');

				if (nAnnualTurnover >= 0 && nAnnualTurnover > loanSourceModel.MaxAnnualTurnover)
					this.parentView.appendDecisionNote('<div class=red>Annual turnover (' + EzBob.formatPoundsNoDecimals(nAnnualTurnover) + ') is greater than max annual turnover (' + EzBob.formatPoundsNoDecimals(loanSourceModel.MaxAnnualTurnover) + ') for this loan source.</div>');
			}

			if (loanSourceModel.AlertOnCustomerReasonType !== -1) {
				var nCustomerReasonType = this.model.get('CustomerReasonType');

				if (loanSourceModel.AlertOnCustomerReasonType === nCustomerReasonType)
					this.parentView.appendDecisionNote('<div class=red>Please note customer reason: "' + this.model.get('CustomerReason') + '".</div>');
			}
		},

		validateInterestVsSource: function(nMaxInterest) {
			if (nMaxInterest === -1)
				return [];

			this.$el.find('.interest-exceeds-max-by-loan-source').toggleClass('hide', this.model.get('InterestRate') <= nMaxInterest);

			this.$el.find('.discount-exceeds-max-by-loan-source').addClass('hide');

			var sPercentList = this.model.get('DiscountPlanPercents');

			if (!sPercentList)
				return [];

			var nBaseRate = this.model.get('InterestRate');

			var aryPercentList = sPercentList.split(',');
			var _len = aryPercentList.length;

			var _results = [];

			for (var _i = 0; _i < _len; _i++) {
				var pct = aryPercentList[_i];

				if (pct[0] === '(')
					pct = pct.substr(1);

				var nPct = parseFloat(pct);
				var nChange = 100.0 + nPct;
				var nRate = nBaseRate * nChange / 100.0;

				if (nRate > nMaxInterest) {
					this.$el.find('.discount-exceeds-max-by-loan-source').removeClass('hide');
					break;
				}
				else
					_results.push(void 0);
			}

			return _results;
		},

		discountPlan: function() {
			var d = new EzBob.Dialogs.ComboEdit({
				model: this.model,
				propertyName: "DiscountPlanId",
				title: "Discount Plan",
				width: 400,
				comboValues: _.map(this.model.get('DiscountPlans'), function(v) {
					return {
						value: v.Id,
						text: v.Name
					};
				}),
				postValueName: "DiscountPlanId",
				url: "Underwriter/ApplicationInfo/DiscountPlan",
				data: {
					id: this.model.get("CashRequestId")
				}
			});

			d.render();

			var self = this;

			d.on("done", function() {
				self.model.fetch();
			});
		},

		UpdateNewCreditLineState: function() {
			var waiting = this.personalInfo.get("CreditResult") === "WaitingForDecision";
			var disabled = waiting || !this.personalInfo.get("IsCustomerInEnabledStatus");
			$("input[name='newCreditLineBtn']").toggleClass("disabled", disabled);
			$("#newCreditLineLnkId").toggleClass("disabled", disabled);
		},

		statuses: {},

		serializeData: function() {
			return {
				m: this.model.toJSON()
			};
		},

		onRender: function() {
			this.$el.find(".tltp").tooltip();
			this.$el.find(".tltp-left").tooltip({
				placement: "left"
			});

			this.UpdateNewCreditLineState();
			this.LoanTypeSelectionAllowedChanged();

			this.initSwitch(".brokerCommisionSwitch", 'UseBrokerSetupFee', this.toggleValue, 'ChangeBrokerSetupFee');
			this.initSwitch(".setupFeeSwitch", 'UseSetupFee', this.toggleValue, 'ChangeSetupFee');
			this.initSwitch(".sendEmailsSwitch", 'AllowSendingEmail', this.toggleValue, 'AllowSendingEmails');

			var isLoanTypeSelectionAllowed = this.model.get('IsLoanTypeSelectionAllowed');

			if (isLoanTypeSelectionAllowed === 2 || isLoanTypeSelectionAllowed === '2')
				this.$el.find('button[name=isLoanTypeSelectionAllowed]').attr('disabled', 'disabled');
			else
				this.$el.find('button[name=isLoanTypeSelectionAllowed]').removeAttr('disabled');

			this.validateLoanSourceRelated();
			EzBob.handleUserLayoutSetting();
		},

		initSwitch: function(elemClass, param, func, method) {
			var that = this;
			var state = this.model.get(param);

			this.$el.find(elemClass).bootstrapSwitch();
			this.$el.find(elemClass).bootstrapSwitch('setState', state);

			this.$el.find(elemClass).on('switch-change', function(event, state) {
				func.call(that, event, state, method, param);
			});
		},

		toggleValue: function(event, state, method, param) {
			var id = this.model.get('CashRequestId');

			BlockUi();

			var self = this;

			$.post(window.gRootPath + 'Underwriter/ApplicationInfo/' + method, {
				id: id,
				enabled: state.value
			}).done(function(result) {
				if (result.error)
					EzBob.App.trigger('error', result.error);
				else
					self.model.fetch();
			}).always(function() {
				UnBlockUi();
			});
		},

		changeCreditResult: function() {
			this.model.fetch();
			this.personalInfo.fetch();
		},

		showCreditLineDialog: function() {
			var xhr,
			  _this = this;
			xhr = this.model.fetch();
			return xhr.done(function() {
				var dialog;
				dialog = new EzBob.Underwriter.CreditLineDialog({
					model: _this.model
				});
				return EzBob.App.jqmodal.show(dialog);
			});
		},

		showErrorDialog: function(errorMsg) {
			EzBob.ShowMessage(errorMsg, "Something went wrong");
		},

		showNothing: function(errorMsg) { }
	});

	EzBob.Underwriter.LoanInfoModel = Backbone.Model.extend({
		idAttribute: "Id",

		urlRoot: "" + window.gRootPath + "Underwriter/ApplicationInfo/Index",

		initialize: function() {
			this.on("change:OfferValidateUntil", this.offerChanged, this);
			this.on("change:LoanTypeId", this.loanTypeChanged, this);
		},

		offerChanged: function() {
			var until_ = moment(this.get("OfferValidateUntil"), "DD/MM/YYYY");
			var now = moment();

			this.set({
				OfferExpired: until_ < now
			});
		},

		loanTypeChanged: function() {
			var types = this.get('LoanTypes');
			var id = parseInt(this.get('LoanTypeId'), 10);

			var type = _.find(types, function(t) {
				return t.value === id;
			});

			if (type)
				this.set("LoanType", type.text);
		}
	});
})();
