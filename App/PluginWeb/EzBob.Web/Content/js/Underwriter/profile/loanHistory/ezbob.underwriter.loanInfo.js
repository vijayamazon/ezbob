var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

(function() {
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
			'click [name="newCreditLineBtn"]': 'runNewCreditLine',
			'click .create-loan-hidden-toggle': 'toggleCreateLoanHidden',
			'click #create-loan-hidden-btn': 'createLoanHidden',
			'click #editOfferButton': 'showCreditLineDialog'
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

		runNewCreditLine: function(e) {
			if ($(e.currentTarget).hasClass("disabled"))
				return false;

			var el = $('<select></select>')
				.css({ height: '30px', width: '270px', })
				.append('<option value=1>Skip everything, go to manual decision</option>')
				.append('<option value=2>Skip everything and apply auto rules</option>')
				.append('<option value=3>Update everything and apply auto rules</option>')
				.append('<option value=4>Update everything and go to manual decision</option>');

			var self = this;

			EzBob.ShowMessage(el, 'New Credit Line Option', (function() {
				self.createNewCreditLine(el.val());
			}), 'OK', null, 'Cancel');

			return false;
		}, // runNewCreditLine

		createNewCreditLine: function(newCreditLineOption) {
			var that = this;

			BlockUi();

			$.post(window.gRootPath + 'Underwriter/ApplicationInfo/RunNewCreditLine', {
				Id: this.model.get('CustomerId'),
				NewCreditLineOption: newCreditLineOption
			}).done(function(response) {
				if (response.status !== 'WaitingForDecision') {
					EzBob.App.vent.trigger('newCreditLine:pass');
					return;
				} // if

				if (response.strategyError)
					EzBob.App.vent.trigger('newCreditLine:error', response.strategyError);
				else
					EzBob.App.vent.trigger('newCreditLine:done');
			}).always(function() {
				UnBlockUi();
			});
		}, // createNewCreditLine

		
		validateLoanSourceRelated: function () {
		    var loanSourceID = this.model.get('LoanSourceID');
		    var loanSourceModel = _.find(this.model.get("AllLoanSources"), function (l) { return l.Id == loanSourceID; });

			this.validateInterestVsSource(loanSourceModel.MaxInterest);

			//if (loanSourceModel.DefaultRepaymentPeriod === -1)
			//	this.$el.find('button[name=repaymentPeriodChangeButton]').removeAttr('disabled');
			//else
			//	this.$el.find('button[name=repaymentPeriodChangeButton]').attr('disabled', 'disabled');

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
			var self = this;

			this.model.fetch().done(function() {
				var dialog = new EzBob.Underwriter.CreditLineDialog({
					model: self.model
				});

				EzBob.App.jqmodal.show(dialog);
			});
		}, // showCreditLineDialog

		showErrorDialog: function(errorMsg) {
			EzBob.ShowMessage(errorMsg, "Something went wrong");
		}, // showErrorDialog

		showNothing: function() { }
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
