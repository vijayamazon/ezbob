var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ApplyForLoanTopViewModel = Backbone.Model.extend({
	defaults: {
		state: "apply"
	}
});

// a top view that manages process of choosing loan amount
// adding a bank account and displaying confirmation screen
EzBob.Profile.ApplyForLoanTopView = Backbone.Marionette.ItemView.extend({
	template: "#apply-forloan-top-template",
	initialize: function (options) {
		this.customer = options.customer;
		this.applyForLoanViewModel = new EzBob.Profile.ApplyForLoanModel({
			maxCash: this.customer.get("CreditSum"),
			OfferStart: this.customer.get("OfferStart"),
			loanType: this.customer.get("LastApprovedLoanTypeID"),
			repaymentPeriod: this.customer.get("LastRepaymentPeriod"),
			approvedRepaymentPeriod: this.customer.get("LastApprovedRepaymentPeriod"),
			isLoanSourceEU: this.customer.get("IsLastApprovedLoanSourceEu"),
			isLoanSourceCOSME: this.customer.get("IsLastApprovedLoanSourceCOSME"),
			isCurrentCashRequestFromQuickOffer: this.customer.get("IsCurrentCashRequestFromQuickOffer"),
			isCustomerRepaymentPeriodSelectionAllowed: this.customer.get('IsCustomerRepaymentPeriodSelectionAllowed'),
			isLoanTypeSelectionAllowed: this.customer.get('IsLoanTypeSelectionAllowed'),
			numOfActiveLoans: this.customer.get('ActiveLoans').length,
			isTest: this.customer.get('IsTest')
		});
		this.states = {
			apply: { view: this.createApplyForLoanView, step: 0 },
			bank: { view: this.createAddBankAccountView, step: 1 }
		};
		return this.model.on("change", this.render, this);
	},
	onRender: function () {
		var view = this.states[this.model.get("state")].view(this);
		view.on('back', this.stateBack, this);
		var region = new Backbone.Marionette.Region({
			el: this.$el.find('.apply-for-loan-div')
		});
		region.show(view);

		var steps = _.template($('#steps-dashboard-template').html());
		$('.dashboard-steps-container').html(steps({ current: this.states[this.model.get("state")].step }));
		return false;
	},
	createApplyForLoanView: function (_this) {
		var view = new EzBob.Profile.ApplyForLoanView({
			model: _this.applyForLoanViewModel,
			customer: _this.customer
		});
		view.on("submit", _this.amountSelected, _this);
		view.on("back", _this.backClicked, _this);
		return view;
	},
	createAddBankAccountView: function (_this) {
		var view = new EzBob.BankAccountInfoView({
			model: _this.customer
		});
		view.on("back", function () {
			return _this.model.set("state", "apply");
		});
		view.on("completed", function () {
			return _this.submit();
		});
		return view;
	},
	backClicked: function() {
		$('.wizard-steps-wrapper').remove();
	},

	stateBack: function() {
		this.customer.fetch();
		this.backClicked();
		this.trigger('back');
	},

	amountSelected: function () {
		var data, enabled, form, pi, xhr;
		form = this.$el.find('form');
		pi = this.customer.get('CustomerPersonalInfo');
		this.$el.find('#signedName').attr('maxlength', pi.Fullname.length + 10);
		enabled = EzBob.Validation.checkForm(EzBob.validateLoanLegalForm(form, [pi.FirstName, pi.MiddleInitial, pi.Surname]));
		if (!enabled) {
			return;
		}
		data = form.serialize();
		BlockUi("on");
	    var self = this;
		xhr = $.post("" + window.gRootPath + "Customer/GetCash/LoanLegalSigned", data);

		xhr.done(function (res) {
	        if (res.error) {
	            EzBob.App.trigger('error', res.error);
	            return;
	        }
	        if (!self.customer.get("bankAccountAdded")) {
	            self.model.set("state", "bank");
	            return;
	        }
	        return self.submit();
        });

	    xhr.always(function () {
			BlockUi("off");
		});
	},
	submit: function () {
		var view;
		view = new EzBob.Profile.PayPointCardSelectView({
			model: this.customer,
			date: this.lastPaymentDate
		});
		if (view.cards.length > 0) {
			view.on('select', (function (_this) {
				return function (cardId) {
					BlockUi();
					var xhr = $.post("" + window.gRootPath + "Customer/GetCash/Now", {
						cardId: cardId,
						amount: _this.applyForLoanViewModel.get("neededCash")
					});
					xhr.done(function (data) {
						if (data.error !== void 0) {
							EzBob.ShowMessage(data.error, "Error occurred");
						} else {
							document.location.href = data.url;
						}
					});
					return xhr.complete(function () {
						UnBlockUi();
					});
				};
			})(this));
			view.on('existing', (function (_this) {
				return function () {
					_this._submit();
				};
			})(this));
			view.on('cancel', (function (_this) {
				return function () {
					_this.model.set("state", "apply");
				};
			})(this));
			EzBob.App.jqmodal.show(view);
			return false;
		} else {
			this._submit();
		}
		return false;
	},
	_submit: function () {
		BlockUi();
		this.applyForLoanViewModel.buildUrl();
		document.location.href = this.applyForLoanViewModel.get('url');
	}
});
