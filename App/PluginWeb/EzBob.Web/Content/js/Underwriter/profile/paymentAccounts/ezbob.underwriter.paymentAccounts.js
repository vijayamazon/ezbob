(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.PaymentAccountsModel = (function(_super) {

    __extends(PaymentAccountsModel, _super);

    function PaymentAccountsModel() {
      return PaymentAccountsModel.__super__.constructor.apply(this, arguments);
    }

    PaymentAccountsModel.prototype.urlRoot = function() {
      return "" + window.gRootPath + "Underwriter/PaymentAccounts/Index/" + this.customerId;
    };

    PaymentAccountsModel.prototype.getCardById = function(id) {
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
    };

    return PaymentAccountsModel;

  })(Backbone.Model);

  EzBob.Underwriter.PaymentAccountView = (function(_super) {

    __extends(PaymentAccountView, _super);

    function PaymentAccountView() {
      return PaymentAccountView.__super__.constructor.apply(this, arguments);
    }

    PaymentAccountView.prototype.template = "#payment-accounts-template";

    PaymentAccountView.prototype.initialize = function() {
      var _this = this;
      this.bindTo(this.model, "change reset sync", this.render, this);
      return window.paypointAdded = function(amount) {
        if (amount == null) {
          amount = 5;
        }
        EzBob.ShowMessage("Please deduct the " + amount + " pounds from this card using manual payment", "Reminder");
        return _this.model.fetch();
      };
    };

    PaymentAccountView.prototype.serializeData = function() {
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
    };

    PaymentAccountView.prototype.ui = {
      'allowSelection': '.debitCardCustomerSelection'
    };

    PaymentAccountView.prototype.events = {
      "click .bankAccounts tbody tr": "showBankAccount",
      "click .checkeBankAccount": "checkBanckAccount",
      "click .add-existing": "addExistingCard",
      "click .setDefault": "setDefault",
      "click .addNewDebitCard": "addNewDebitCard",
      "click .set-paypoint-default": "setPaypointDefault"
    };

    PaymentAccountView.prototype.onRender = function() {
      var _this = this;
      this.$el.find('.bankAccounts i[data-title]').tooltip({
        placement: "right"
      });
      this.ui.allowSelection.bootstrapSwitch();
      this.ui.allowSelection.bootstrapSwitch('setState', this.model.get('CustomerDefaultCardSelectionAllowed'));
      return this.ui.allowSelection.on('switch-change', function(event, state) {
        return _this.changeAllowSelection(event, state);
      });
    };

    PaymentAccountView.prototype.changeAllowSelection = function(event, state) {
      var xhr,
        _this = this;
      BlockUi("on");
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/ChangeCustomerDefaultCardSelection", {
        customerId: this.model.customerId,
        state: state.value
      });
      xhr.done(function() {
        return _this.model.fetch();
      });
      return xhr.always(function() {
        return BlockUi("off");
      });
    };

    PaymentAccountView.prototype.setPaypointDefault = function(e) {
      var $el, card, transactionId, xhr,
        _this = this;
      $el = $(e.currentTarget);
      transactionId = $el.data("transactionid");
      card = _.find(this.model.get("PayPointCards"), function(c) {
        return c.TransactionId === transactionId;
      });
      BlockUi("on");
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/SetPaypointDefaultCard", {
        customerId: this.model.customerId,
        transactionId: transactionId,
        cardNo: card.CardNo
      });
      xhr.complete(function() {
        return BlockUi("off");
      });
      return xhr.done(function() {
        return _this.model.fetch();
      });
    };

    PaymentAccountView.prototype.showBankAccount = function(e) {
      var id;
      if (e.target.tagName === 'BUTTON') {
        return false;
      }
      id = $(e.currentTarget).data('card-id');
      return this._showBankAccount(id);
    };

    PaymentAccountView.prototype._showBankAccount = function(cardId) {
      var card, message, view;
      card = this.model.getCardById(cardId);
      if (!((card != null ? card.Bank : void 0) != null)) {
        message = (card != null ? card.StatusInformation : void 0) || "Validation was not performed";
        EzBob.ShowMessage(message, "Bank account check");
        return false;
      }
      view = new EzBob.Underwriter.BankAccountDetailsView({
        model: new Backbone.Model(card)
      });
      EzBob.App.jqmodal.show(view);
      return false;
    };

    PaymentAccountView.prototype.addNewDebitCard = function() {
      var view,
        _this = this;
      view = new EzBob.Underwriter.AddBankAccount({
        customerId: this.model.customerId
      });
      view.on('saved', function() {
        return _this.model.fetch();
      });
      EzBob.App.jqmodal.show(view);
      return false;
    };

    PaymentAccountView.prototype.setDefault = function(e) {
      var id, xhr,
        _this = this;
      id = $(e.currentTarget).parents('tr').data('card-id');
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/SetDefaultCard", {
        customerId: this.model.customerId,
        cardId: id
      });
      xhr.done(function() {
        return _this.model.fetch();
      });
    };

    PaymentAccountView.prototype.checkBanckAccount = function(e) {
      var id, xhr,
        _this = this;
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
      return xhr.done(function(r) {
        var xhr2;
        if (r.error) {
          _this.model.fetch();
          BlockUi('Off');
          EzBob.ShowMessage(r.error, "Error");
          return;
        }
        xhr2 = _this.model.fetch();
        return xhr2.done(function() {
          _this._showBankAccount(id);
          return BlockUi('Off');
        });
      });
    };

    PaymentAccountView.prototype.addExistingCard = function() {
      var model, view,
        _this = this;
      model = new EzBob.Underwriter.PayPointCardModel();
      view = new EzBob.Underwriter.AddPayPointCardView({
        model: model
      });
      view.on('save', function() {
        var data, xhr;
        BlockUi("on");
        data = model.toJSON();
        data.customerId = _this.model.customerId;
        xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/AddPayPointCard", data);
        return xhr.done(function() {
          BlockUi("off");
          return _this.model.fetch();
        });
      });
      EzBob.App.jqmodal.show(view);
      return false;
    };

    return PaymentAccountView;

  })(Backbone.Marionette.ItemView);

}).call(this);
