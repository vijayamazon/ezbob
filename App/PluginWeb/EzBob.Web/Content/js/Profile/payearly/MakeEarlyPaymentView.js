var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.MakeEarlyPayment = Backbone.Marionette.ItemView.extend({
	template: "#payEaryly-template",

	initialize: function(options) {
		var currentLoanId, firstLate;

		this.infoPage = _.template($("#infoPageTemplate").html());

		this.customerModel = options.customerModel;

		this.bindTo(
			this.customerModel,
			"change:LateLoans change:TotalBalance change:NextPayment change:ActiveLoans change:hasLateLoans",
			this.render,
			this
		);

		this.loans = this.customerModel.get("Loans");

		this.model = new EzBob.Profile.MakeEarlyPaymentModel({
			customer: this.customerModel
		});

		firstLate = _.find(this.loans.toJSON(), function(val, i) {
			return val.Status === "Late";
		});

		currentLoanId = void 0;

		if (this.model.get('rollovers').length > 0)
			currentLoanId = this.model.get('rollovers').toJSON()[0].LoanId;
		else {
			if (firstLate)
				currentLoanId = firstLate.Id;
			else {
				if (options.loanId)
					currentLoanId = options.loanId;
			} // if
		} // if

		if (currentLoanId) {
			this.model.set({ loan: this.loans.get(currentLoanId) });
		}

		this.bindTo(this.model, "change", this.render, this);

		return this;
	}, // initialize

	serializeData: function() {
		var data;
		data = this.model.toJSON();
		data.hasLateLoans = this.customerModel.get("hasLateLoans");
		return data;
	}, // serializeData

	onRender: function() {
		this.$el.find('[name="paymentAmount"],[name="rolloverAmount"]').autoNumeric('init', EzBob.moneyFormat);
		this.$el.find('li[rel]').setPopover('left');
		EzBob.UiAction.registerView(this);
		return this;
	}, // onRender

	events: {
		"click .submit": "submit",
		"change input[name='paymentAmount']": "paymentAmountChanged",
		"change input[name='rolloverAmount']": "rolloverAmountChanged",
		"change input[name='loanPaymentType']": "loanPaymentTypeChanged",
		"change input[name='rolloverPaymentType']": "rolloverPaymentTypeChanged",
		"change input[name='defaultCard']": "defaultCardChanged",
		"change select": "loanChanged",
		"click .back": "back",
		"click .back-to-profile": "backToProfile",
		"change input[name='paymentType']": "paymentTypeChanged"
	}, // events

	ui: { submit: ".submit" }, // ui

	defaultCardChanged: function() {
		return this.model.set("defaultCard", !this.model.get("defaultCard"));
	}, // defaultCardChanged

	submit: function() {

		if (this.ui.submit.hasClass("disabled"))
			return false;

		if (this.model.get("defaultCard")) {
			this.payFast();
			return false;
		} // if

		var view = new EzBob.Profile.PayPointCardSelectView({
			model: this.customerModel,
			date: moment()
		});

		if (!view.hasCards())
			return;

		var self = this;

		view.on('select', function(cardId) { return self.payFast(cardId); });
		view.on('existing', function() { document.location.href = self.ui.submit.attr("href"); });

		EzBob.App.jqmodal.show(view);

		return false;
	}, // submit

	payFast: function(cardId) {
		if (cardId == null)
			cardId = -1;

		this.ui.submit.addClass("disabled");

		var data = {
			amount: parseFloat(this.model.get("amount")),
			type: this.model.get("paymentType"),
			paymentType: this.model.getPaymentType(),
			loanId: this.model.get("loan").id,
			cardId: cardId,
			rolloverId: this.model.get("currentRollover") && this.model.get("currentRollover").Id
		};

		BlockUi("on");
		var self = this;

		return $.post(window.gRootPath + "Customer/Paypoint/PayFast", data).done(function(res) {
			if (res.error) {
				EzBob.App.trigger("error", res.error);
				self.back();
				return;
			} // if

			var loan = self.model.get("loan");

			self.$el.html(self.infoPage({
				amount: res.PaymentAmount,
				card_no: res.CardNo,
				email: self.customerModel.get("Email"),
				name: self.customerModel.get("CustomerPersonalInfo").FirstName,
				surname: self.customerModel.get("CustomerPersonalInfo").Surname,
				refnum: (loan ? loan.get("RefNumber") : ""),
				transRefnums: res.TransactionRefNumbersFormatted,
				saved: res.Saved,
				savedPounds: res.SavedPounds,
				hasLateLoans: self.customerModel.get("hasLateLoans"),
				isRolloverPaid: res.RolloverWasPaid,
				IsEarly: res.IsEarly
			}));

			EzBob.UiAction.registerView(self);

			EzBob.App.trigger("clear");
		}).complete(function() {
			self.ui.submit.removeClass("disabled");
			BlockUi("off");
		});
	}, // payFast

	backToProfile: function() {
		this.customerModel.fetch();
		this.trigger("submit");
		return false;
	}, // backToProfile

	paymentAmountChanged: function() {
		var amount = this.$el.find("[name='paymentAmount']").autoNumeric('get');
		var maxAmount = this.model.get("loan").get("TotalEarlyPayment");
		var minAmount = this.model.get("currentRollover") === null ? 30 : this.model.get("currentRollover").RolloverPayValue;

		if (maxAmount < minAmount)
			amount = maxAmount;
		else if (amount < minAmount)
			amount = minAmount;
		else if (amount > maxAmount)
			amount = maxAmount;

		this.model.set("amount", parseFloat(amount));

		this.render();
	}, // paymentAmountChanged

	rolloverAmountChanged: function() {
		var amount = this.$el.find("[name='rolloverAmount']").autoNumeric('get');
		var maxAmount = this.model.get("total");
		var minAmount = this.model.get("currentRollover").RolloverPayValue;

		if (amount < minAmount)
			amount = minAmount;

		if (amount > maxAmount)
			amount = maxAmount;

		this.model.set("amount", parseFloat(amount));

		return this.render();
	}, // rolloverAmountChanged

	paymentTypeChanged: function() {
		var type = this.$el.find("input[name='paymentType']:checked").val();

		this.model.set({ paymentType: type });

		this.loanChanged();
	}, // paymentTypeChanged

	loanPaymentTypeChanged: function() {
		var type = this.$el.find("input[name='loanPaymentType']:checked").val();

		if (this.model.get("paymentType") !== "loan")
			this.model.set({ paymentType: "loan" }, { silent: true });

		this.model.set({ loanPaymentType: type });
	}, // loanPaymentTypeChanged

	rolloverPaymentTypeChanged: function() {
		var type = this.$el.find("input[name='rolloverPaymentType']:checked").val();

		this.model.set({ rolloverPaymentType: type });
	}, // rolloverPaymentTypeChanged

	loanChanged: function() {
		var loanId = parseInt($("select:NOT(:disabled):visible").val(), 10);
		if (loanId !== void 0 && !isNaN(loanId)) {
			var loan = this.customerModel.get("Loans").get(loanId);
			this.model.set({ loan: loan });
		} // if
	}, // loanChanged

	back: function() {
		this.trigger("back");
		return false;
	}, // back
}); // EzBob.Profile.MakeEarlyPayment

EzBob.PayEarlyConfirmation = Backbone.Marionette.ItemView.extend({
	template: "#pay-early-confirmation",

	events: {
		"click a.cancel": "btnClose",
		"click a.save": "btnSave"
	}, // events

	btnClose: function() {
		this.close();
		return false;
	}, // btnClose

	btnSave: function() {
		this.trigger("modal:save");
		this.onOk();
		this.close();
		return false;
	}, // btnSave

	onOk: function() {
	}, // onOk
}); // EzBob.PayEarlyConfirmation
