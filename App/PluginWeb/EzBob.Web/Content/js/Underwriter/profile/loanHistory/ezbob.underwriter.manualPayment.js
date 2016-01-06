var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ManualPaymentView = Backbone.Marionette.ItemView.extend({
	template: "#manualPayment-template",

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: "Manual payment",
			position: "center",
			draggable: false,
			dialogClass: "manual-payment-popup",
			width: 600
		};
	},

	onRender: function() {
		this.$el.find('.ezDateTime').splittedDateTime();
		this.minAmount = 0.1;
		this.maxAmount = 0;
		this.updatePaymentData();
		return this;
	},

	events: {
		"click .confirm": "confirmClicked",
		"click .uploadFiles": "uploadFilesClicked",
		"change [name='paymentDate']": "updatePaymentData",
		"change [name='totalSumPaid']": "updatePaymentData"
	},

	ui: {
		form: '#payment-form',
		money: "[name='totalSumPaid']",
		date: "[name='paymentDate']",
		fees: "[name='fees']",
		interest: "[name='interest']",
		principal: "[name='principal']"
	},

	confirmClicked: function() {
		if (!this.validator.form())
			return;

		this.trigger("addPayment", this.ui.form.serialize());
		this.close();
	},

	uploadFilesClicked: function() {
		$("#addNewDoc").click();
		return false;
	},

	updatePaymentData: function () {
		if (this.validator && !this.validator.element(this.ui.money)) {
			this.ui.fees.val(0);
			this.ui.principal.val(0);
			this.ui.interest.val(0);
			return false;
		}

		var data = {
			date: this.ui.date.val(),
			money: ValueOrDefault(this.ui.money.val(), 0),
			loanId: this.model.get("loanId")
		};

		var request = $.get(window.gRootPath + "Underwriter/LoanHistory/GetPaymentInfo", data);

		var self = this;
		
		request.done(function(r) {
			if (r.error)
				return;
			self.ui.form.removeData('validator');
			self.ui.fees.val(r.Fee);
			self.ui.principal.val(r.Principal);
			self.ui.interest.val(r.Interest);
			self.ui.money.val(r.Amount);
			self.minAmount = r.MinValue;
			self.maxAmount = r.Balance;
			var moneyTitle = "Minium value = " + self.minAmount + ", maximum value = " + self.maxAmount;
			self.ui.money.attr('data-original-title', moneyTitle);
			self.ui.money.tooltip({
				'trigger': 'hover',
				'title': moneyTitle
			});
			self.ui.money.tooltip("enable").tooltip('fixTitle');
			
			self.validator = EzBob.validatemanualPaymentForm(self.ui.form, r.Balance, r.MinValue);
		});

		return false;
	}
});
