var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.PaymentAccountsModel = Backbone.Model.extend({
	urlRoot: function () {
		return "" + window.gRootPath + "Underwriter/PaymentAccounts/Index/" + this.customerId;
	},
	getCardById: function (id) {
		var acc, current, _i, _len, _ref;
		current = this.get('CurrentBankAccount');
		if (parseInt(current.Id) === parseInt(id)) {
			return current;
		}
		_ref = this.get('BankAccounts');
		for (_i = 0, _len = _ref.length; _i < _len; _i++) {
			acc = _ref[_i];
			if (parseInt(acc.Id) === parseInt(id)) {
				return acc;
			}
		}
		return null;
	}
});

EzBob.Underwriter.PaymentAccountView = Backbone.Marionette.ItemView.extend({
	template: "#payment-accounts-template",
	initialize: function () {
		this.bindTo(this.model, "change reset sync", this.render, this);
		var self = this;
		window.paypointAdded = (function (amount) {
			if (amount == null) {
				amount = 5;
			}
			EzBob.ShowMessage("Please deduct the " + amount + " pounds from this card using manual payment", "Reminder");
			self.model.fetch();
		});
		return this;
	},
	serializeData: function () {
		var bankAccounts, current;
		bankAccounts = this.model.get("BankAccounts") || [];
		current = this.model.get("CurrentBankAccount");
		if (current) {
			current.isDefault = true;
			bankAccounts.push(current);
		}
		bankAccounts = _.sortBy(bankAccounts, "BankAccount");
		return {
			bankAccounts: bankAccounts,
			paymentAccounts: this.model.toJSON(),
			customerId: this.model.customerId
		};
	},
	ui: {
		'allowSelection': '.debitCardCustomerSelection'
	},
	events: {
		"click .bankAccounts tbody tr": "showBankAccount",
		"click .checkeBankAccount": "checkBanckAccount",
		"click .add-existing": "addExistingCard",
		"click .setDefault": "setDefault",
		"click .addNewDebitCard": "addNewDebitCard",
		"click .set-paypoint-default": "setPaypointDefault"
	},
	onRender: function() {
		this.$el.find('.bankAccounts i[data-title]').tooltip({
			placement: "right"
		});
		this.ui.allowSelection.bootstrapSwitch();
		this.ui.allowSelection.bootstrapSwitch('setState', this.model.get('CustomerDefaultCardSelectionAllowed'));
		var self = this;
		
		this.ui.allowSelection.on('switch-change', (function (event, state) {
			self.changeAllowSelection(event, state);
		}));
		
		return this;
	},
	changeAllowSelection: function (event, state) {
		var xhr;
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/ChangeCustomerDefaultCardSelection", {
			customerId: this.model.customerId,
			state: state.value
		});
		var self = this;
		xhr.done(function () {
			self.model.fetch();
		});
		
		xhr.always(function () {
			BlockUi("off");
		});
	},
	setPaypointDefault: function (e) {
		var $el, card, transactionId, xhr;
		$el = $(e.currentTarget);
		transactionId = $el.data("transactionid");
		
		card = _.find(this.model.get("PayPointCards"), function (c) {
			return c.TransactionId === transactionId;
		});
		
		BlockUi("on");
		xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/SetPaypointDefaultCard", {
			customerId: this.model.customerId,
			transactionId: transactionId,
			cardNo: card.CardNo
		});
		xhr.complete(function () {
			return BlockUi("off");
		});
		return xhr.done((function (_this) {
			return function () {
				return _this.model.fetch();
			};
		})(this));
	},
	showBankAccount: function (e) {
		var id;
		if (e.target.tagName === 'BUTTON') {
			return false;
		}
		id = $(e.currentTarget).data('card-id');
		return this._showBankAccount(id);
	},
	_showBankAccount: function (cardId) {
		var card, message, view;
		card = this.model.getCardById(cardId);
		if ((card != null ? card.Bank : void 0) == null) {
			message = (card != null ? card.StatusInformation : void 0) || "Validation was not performed";
			EzBob.ShowMessage(message, "Bank account check");
			return false;
		}
		view = new EzBob.Underwriter.BankAccountDetailsView({
			model: new Backbone.Model(card)
		});
		EzBob.App.jqmodal.show(view);
		return false;
	},
	addNewDebitCard: function () {
		var view;
		var self = this;
		view = new EzBob.Underwriter.AddBankAccount({
			customerId: this.model.customerId
		});
		view.on('saved', function () {
			return self.model.fetch();
		});
		
		EzBob.App.jqmodal.show(view);
		return false;
	},
	setDefault: function (e) {
		var id, xhr;
		id = $(e.currentTarget).parents('tr').data('card-id');
		xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/SetDefaultCard", {
			customerId: this.model.customerId,
			cardId: id
		});
		var self = this;
		
		xhr.done(function () {
			return self.model.fetch();
		});
	},
	checkBanckAccount: function (e) {
		var id, xhr;
		id = $(e.currentTarget).parents('tr').data('card-id');
		BlockUi('On');
		xhr = $.ajax({
			url: "" + window.gRootPath + "Underwriter/PaymentAccounts/PerformCheckBankAccount",
			data: {
				id: this.model.customerId,
				cardid: id
			},
			global: false,
			type: 'POST'
		});

		var self = this;
		xhr.done(function (r) {
			var xhr2;
			if (r.error) {
				self.model.fetch();
				BlockUi('Off');
				EzBob.ShowMessage(r.error, "Error");
				return false;
			}
			
			xhr2 = self.model.fetch();
			xhr2.done(function () {
				self._showBankAccount(id);
				BlockUi('Off');
			});
			return false;
		});
	},
	addExistingCard: function () {
		var model, view;
		model = new EzBob.Underwriter.PayPointCardModel();
		view = new EzBob.Underwriter.AddPayPointCardView({
			model: model
		});

		var self = this;
		view.on('save', (function () {
			var data, xhr;
			BlockUi("on");
			data = model.toJSON();
			data.customerId = self.model.customerId;
			xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/AddPayPointCard", data);
			xhr.done(function () {
				BlockUi("off");
				self.model.fetch();
			});
		}));
		
		EzBob.App.jqmodal.show(view);
		return false;
	}
});
