(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.MakeEarlyPayment = (function(_super) {
    __extends(MakeEarlyPayment, _super);

    function MakeEarlyPayment() {
      _ref = MakeEarlyPayment.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    MakeEarlyPayment.prototype.template = "#payEaryly-template";

    MakeEarlyPayment.prototype.initialize = function(options) {
      var currentLoanId, firstLate;

      this.infoPage = _.template($("#infoPageTemplate").html());
      this.customerModel = options.customerModel;
      this.bindTo(this.customerModel, "change:LateLoans change:TotalBalance change:NextPayment change:ActiveLoans change:hasLateLoans", this.render, this);
      this.loans = this.customerModel.get("Loans");
      this.model = new EzBob.Profile.MakeEarlyPaymentModel({
        customer: this.customerModel
      });
      firstLate = _.find(this.loans.toJSON(), function(val, i) {
        return val.Status === "Late";
      });
      currentLoanId = void 0;
      if (this.model.get('rollovers').length > 0) {
        currentLoanId = this.model.get('rollovers').toJSON()[0].LoanId;
      } else {
        if (firstLate) {
          currentLoanId = firstLate.Id;
        } else {
          if (options.loanId) {
            currentLoanId = options.loanId;
          }
        }
      }
      if (currentLoanId) {
        this.model.set({
          loan: this.loans.get(currentLoanId)
        });
      }
      this.bindTo(this.model, "change", this.render, this);
      return this;
    };

    MakeEarlyPayment.prototype.serializeData = function() {
      var data;

      data = this.model.toJSON();
      data.hasLateLoans = this.customerModel.get("hasLateLoans");
      return data;
    };

    MakeEarlyPayment.prototype.onRender = function() {
      this.$el.find("li[rel]").setPopover('left');
      return this;
    };

    MakeEarlyPayment.prototype.events = {
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
    };

    MakeEarlyPayment.prototype.ui = {
      submit: ".submit"
    };

    MakeEarlyPayment.prototype.defaultCardChanged = function() {
      return this.model.set("defaultCard", !this.model.get("defaultCard"));
    };

    MakeEarlyPayment.prototype.submit = function() {
      var view,
        _this = this;

      if (this.ui.submit.hasClass("disabled")) {
        return false;
      }
      if (this.model.get("defaultCard")) {
        this.payFast();
        return false;
      }
      view = new EzBob.Profile.PayPointCardSelectView({
        model: this.customerModel,
        date: moment()
      });
      if (!view.hasCards()) {
        return;
      }
      view.on('select', function(cardId) {
        return _this.payFast(cardId);
      });
      view.on('existing', function() {
        return document.location.href = _this.ui.submit.attr("href");
      });
      EzBob.App.modal.show(view);
      return false;
    };

    MakeEarlyPayment.prototype.payFast = function(cardId) {
      var data,
        _this = this;

      if (cardId == null) {
        cardId = -1;
      }
      this.ui.submit.addClass("disabled");
      data = {
        amount: parseFloat(this.model.get("amount")),
        type: this.model.get("paymentType"),
        paymentType: this.model.getPaymentType(),
        loanId: this.model.get("loan").id,
        cardId: cardId,
        rolloverId: this.model.get("currentRollover") && this.model.get("currentRollover").Id
      };
      BlockUi("on");
      return $.post(window.gRootPath + "Customer/Paypoint/PayFast", data).done(function(res) {
        var hadRollover, loan;

        if (res.error) {
          EzBob.App.trigger("error", res.error);
          _this.back();
          return;
        }
        loan = _this.model.get("loan");
        hadRollover = _this.model.get("currentRollover");
        _this.$el.html(_this.infoPage({
          amount: res.PaymentAmount,
          card_no: res.CardNo,
          email: _this.customerModel.get("Email"),
          name: _this.customerModel.get("CustomerPersonalInfo").FirstName,
          surname: _this.customerModel.get("CustomerPersonalInfo").Surname,
          refnum: (loan ? loan.get("RefNumber") : ""),
          transRefnums: res.TransactionRefNumbersFormatted,
          saved: res.Saved,
          savedPounds: res.SavedPounds,
          hasLateLoans: _this.customerModel.get("hasLateLoans"),
          isRolloverPaid: res.RolloverWasPaid,
          IsEarly: res.IsEarly
        }));
        return EzBob.App.trigger("clear");
      }).complete(function() {
        _this.ui.submit.removeClass("disabled");
        return BlockUi("off");
      });
    };

    MakeEarlyPayment.prototype.backToProfile = function() {
      this.customerModel.fetch();
      this.trigger("submit");
      return false;
    };

    MakeEarlyPayment.prototype.paymentAmountChanged = function() {
      var amount, maxAmount, minAmount;

      amount = this.$el.find("[name='paymentAmount']").autoNumericGet();
      maxAmount = this.model.get("loan").get("TotalEarlyPayment");
      minAmount = this.model.get("currentRollover") === null ? 30 : this.model.get("currentRollover").RolloverPayValue;
      if (maxAmount < minAmount) {
        amount = maxAmount;
      } else if (amount < minAmount) {
        amount = minAmount;
      } else if (amount > maxAmount) {
        amount = maxAmount;
      }
      this.model.set("amount", parseFloat(amount));
      return this.render();
    };

    MakeEarlyPayment.prototype.rolloverAmountChanged = function() {
      var amount, maxAmount, minAmount;

      amount = this.$el.find("[name='rolloverAmount']").autoNumericGet();
      maxAmount = this.model.get("total");
      minAmount = this.model.get("currentRollover").RolloverPayValue;
      if (amount < minAmount) {
        amount = minAmount;
      }
      if (amount > maxAmount) {
        amount = maxAmount;
      }
      this.model.set("amount", parseFloat(amount));
      return this.render();
    };

    MakeEarlyPayment.prototype.paymentTypeChanged = function() {
      var type;

      type = this.$el.find("input[name='paymentType']:checked").val();
      this.model.set({
        paymentType: type
      });
      return this.loanChanged();
    };

    MakeEarlyPayment.prototype.loanPaymentTypeChanged = function() {
      var type;

      type = this.$el.find("input[name='loanPaymentType']:checked").val();
      if (this.model.get("paymentType") !== "loan") {
        this.model.set({
          paymentType: "loan"
        }, {
          silent: true
        });
      }
      return this.model.set({
        loanPaymentType: type
      });
    };

    MakeEarlyPayment.prototype.rolloverPaymentTypeChanged = function() {
      var type;

      type = this.$el.find("input[name='rolloverPaymentType']:checked").val();
      return this.model.set({
        rolloverPaymentType: type
      });
    };

    MakeEarlyPayment.prototype.loanChanged = function() {
      var loan, loanId;

      loanId = $("select:NOT(:disabled):visible").val();
      if (loanId !== void 0) {
        loan = this.customerModel.get("Loans").get(loanId);
        return this.model.set({
          loan: loan
        });
      }
    };

    MakeEarlyPayment.prototype.back = function() {
      this.trigger("back");
      return false;
    };

    return MakeEarlyPayment;

  })(Backbone.Marionette.ItemView);

  EzBob.PayEarlyConfirmation = Backbone.Marionette.ItemView.extend({
    template: "#pay-early-confirmation",
    events: {
      "click a.cancel": "btnClose",
      "click a.save": "btnSave"
    },
    btnClose: function() {
      this.close();
      return false;
    },
    btnSave: function() {
      this.trigger("modal:save");
      this.onOk();
      this.close();
      return false;
    },
    onOk: function() {}
  });

}).call(this);
