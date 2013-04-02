(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.MakeEarlyPayment = (function(_super) {

    __extends(MakeEarlyPayment, _super);

    function MakeEarlyPayment() {
      return MakeEarlyPayment.__super__.constructor.apply(this, arguments);
    }

    MakeEarlyPayment.prototype.template = "#payEaryly-template";

    MakeEarlyPayment.prototype.initialize = function(options) {
      this.infoPage = _.template($("#infoPageTemplate").html());
      this.customerModel = options.customerModel;
      this.bindTo(this.customerModel, "change:LateLoans change:TotalBalance change:NextPayment change:ActiveLoans change:hasLateLoans", this.render, this);
      this.loans = this.customerModel.get("Loans");
      this.model = new EzBob.Profile.MakeEarlyPaymentModel({
        customer: this.customerModel
      });
      this.bindTo(this.model, "change", this.render, this);
      if (options.loanId) {
        return this.model.set({
          loan: this.loans.get(options.loanId)
        });
      }
    };

    MakeEarlyPayment.prototype.serializeData = function() {
      var data;
      data = this.model.toJSON();
      data.hasLateLoans = this.customerModel.get("hasLateLoans");
      return data;
    };

    MakeEarlyPayment.prototype.onRender = function() {
      this.$el.find("li[rel]").popover({
        placement: "left"
      });
      return this;
    };

    MakeEarlyPayment.prototype.events = {
      "click .submit": "submit",
      "change input[type=\"text\"]": "changed",
      "change input[name=\"paymentType\"]": "paymentTypeChanged",
      "change input[name=\"loanPaymentType\"]": "loanPaymentTypeChanged",
      "change input[name=\"defaultCard\"]": "defaultCardChanged",
      "change select": "loanChanged",
      "click .back": "back",
      "click .back-to-profile": "backToProfile"
    };

    MakeEarlyPayment.prototype.ui = {
      submit: ".submit"
    };

    MakeEarlyPayment.prototype.defaultCardChanged = function() {
      return this.model.set("defaultCard", !this.model.get("defaultCard"));
    };

    MakeEarlyPayment.prototype.submit = function() {
      if (this.ui.submit.hasClass("disabled")) {
        return false;
      }
      this.makePayment();
      return false;
    };

    MakeEarlyPayment.prototype.makePayment = function() {
      var view,
        _this = this;
      view = new EzBob.Profile.PayPointCardSelectView({
        model: this.customerModel
      });
      view.on('select', function(cardId) {
        var data;
        _this.ui.submit.addClass("disabled");
        data = {
          amount: parseFloat(_this.model.get("amount")),
          type: _this.model.get("paymentType"),
          loanPaymentType: _this.model.get("loanPaymentType"),
          loanId: _this.model.get("loan").id,
          cardId: cardId
        };
        return $.post(window.gRootPath + "Customer/Paypoint/PayFast", data).done(function(res) {
          var loan;
          if (res.error) {
            EzBob.App.trigger("error", res.error);
            _this.back();
            return;
          }
          loan = _this.model.get("loan");
          return _this.$el.html(_this.infoPage({
            amount: res.PaymentAmount,
            card_no: _this.customerModel.get("CreditCardNo"),
            email: _this.customerModel.get("Email"),
            name: _this.customerModel.get("CustomerPersonalInfo").FirstName,
            surname: _this.customerModel.get("CustomerPersonalInfo").Surname,
            refnum: (loan ? loan.get("RefNumber") : ""),
            transRefnums: res.TransactionRefNumbersFormatted,
            saved: res.Saved,
            savedPounds: res.SavedPounds
          }));
        }).complete(function() {
          return this.ui.submit.removeClass("disabled");
        });
      });
      view.on('existing', function() {
        return document.location.href = _this.ui.submit.attr("href");
      });
      EzBob.App.modal.show(view);
      return false;
    };

    MakeEarlyPayment.prototype.backToProfile = function() {
      this.customerModel.fetch();
      this.trigger("submit");
      return false;
    };

    MakeEarlyPayment.prototype.changed = function() {
      var amount;
      amount = this.$el.find("[name=\"paymentAmount\"]").autoNumericGet();
      this.model.set("amount", parseFloat(amount));
      return this.render();
    };

    MakeEarlyPayment.prototype.paymentTypeChanged = function() {
      var type;
      type = this.$el.find("input[name=\"paymentType\"]:checked").val();
      return this.model.set({
        paymentType: type
      });
    };

    MakeEarlyPayment.prototype.loanPaymentTypeChanged = function() {
      var type;
      type = this.$el.find("input[name=\"loanPaymentType\"]:checked").val();
      return this.model.set({
        loanPaymentType: type
      });
    };

    MakeEarlyPayment.prototype.loanChanged = function() {
      var loan, loanId;
      loanId = this.$el.find("select option:selected").val();
      loan = this.customerModel.get("Loans").get(loanId);
      return this.model.set({
        loan: loan
      });
    };

    MakeEarlyPayment.prototype.back = function() {
      this.trigger("back");
      return false;
    };

    return MakeEarlyPayment;

  })(Backbone.Marionette.ItemView.extend);

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
