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
      this.bindTo(this.model, "change reset", this.render, this);
      return window.paypointAdded = function() {
        return _this.model.fetch();
      };
    };

    PaymentAccountView.prototype.serializeData = function() {
      return {
        paymentAccounts: this.model.toJSON(),
        customerId: this.model.customerId
      };
    };

    PaymentAccountView.prototype.events = {
      "click .bankAccounts tbody tr": "showBankAccount",
      "click .paypalAccounts tbody tr": "rowClick",
      "click .reCheck-paypal": "reCheckPaypal",
      "click .checkeBankAccount": "checkBanckAccount",
      "click .add-existing": "addExistingCard"
    };

    PaymentAccountView.prototype.showBankAccount = function() {
      var view;
      if (this.model.get('BankAccounts')[0].Details === null) {
        return;
      }
      view = new EzBob.Underwriter.BankAccountDetailsView({
        model: this.model
      });
      EzBob.App.jqmodal.show(view);
      return false;
    };

    PaymentAccountView.prototype.checkBanckAccount = function() {
      var xhr,
        _this = this;
      BlockUi('On');
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/PerformCheckBankAccount", {
        id: this.model.customerId
      });
      return xhr.done(function(r) {
        var xhr2;
        if (r.error) {
          BlockUi('Off');
          EzBob.ShowMessage("Error", r.error);
          return;
        }
        xhr2 = _this.model.fetch();
        return xhr2.done(function() {
          _this.showBankAccount();
          return BlockUi('Off');
        });
      });
    };

    PaymentAccountView.prototype.rowClick = function(e) {
      var marketplaceId, view;
      if (e.target.nodeName === "A") {
        return;
      }
      marketplaceId = e.currentTarget.getAttribute("data-id");
      view = new EzBob.Underwriter.PayPalAccountDetailsView({
        model: new EzBob.Underwriter.PayPalAccountDetailsModel({
          marketplaceId: marketplaceId
        })
      });
      BlockUi("On");
      view.model.fetch().done(function() {
        BlockUi("Off");
        return EzBob.App.jqmodal.show(view);
      });
      return false;
    };

    PaymentAccountView.prototype.reCheckPaypal = function(e) {
      var el, umi,
        _this = this;
      el = $(e.currentTarget);
      umi = el.attr("umi");
      EzBob.ShowMessage("", "Are you sure?", (function() {
        return _this.doReCheck(umi, el);
      }), "Yes", null, "No");
      return false;
    };

    PaymentAccountView.prototype.doReCheck = function(umi, el) {
      var xhr,
        _this = this;
      xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/ReCheckPaypal", {
        customerId: this.model.customerId,
        umi: umi
      });
      xhr.done(function() {
        EzBob.ShowMessage("Wait a few minutes", "The marketplace recheck has been started. ", null, "OK");
        return _this.trigger("rechecked", {
          umi: umi,
          el: el
        });
      });
      return xhr.fail(function(data) {
        return console.error(data.responseText);
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
        data = model.toJSON();
        data.customerId = _this.model.customerId;
        console.log(data);
        xhr = $.post("" + window.gRootPath + "Underwriter/PaymentAccounts/AddPayPointCard", data);
        return xhr.done(function() {
          return _this.model.fetch();
        });
      });
      return EzBob.App.modal.show(view);
    };

    return PaymentAccountView;

  })(Backbone.Marionette.ItemView);

}).call(this);
