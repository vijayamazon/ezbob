(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.PayPalAccountDetailsModel = (function(_super) {

    __extends(PayPalAccountDetailsModel, _super);

    function PayPalAccountDetailsModel() {
      return PayPalAccountDetailsModel.__super__.constructor.apply(this, arguments);
    }

    PayPalAccountDetailsModel.prototype.idAttribute = "marketplaceId";

    PayPalAccountDetailsModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/PaymentAccounts/Details/" + this.id;
    };

    return PayPalAccountDetailsModel;

  })(Backbone.Model);

  EzBob.Underwriter.PayPalAccountDetailsView = (function(_super) {

    __extends(PayPalAccountDetailsView, _super);

    function PayPalAccountDetailsView() {
      return PayPalAccountDetailsView.__super__.constructor.apply(this, arguments);
    }

    PayPalAccountDetailsView.prototype.template = "#payPalAccount-values-template";

    PayPalAccountDetailsView.prototype.initialize = function() {
      return this.bindTo(this.model, "change init", this.render, this);
    };

    PayPalAccountDetailsView.prototype.serializeData = function() {
      return {
        paymentAccounts: this.model.toJSON()
      };
    };

    PayPalAccountDetailsView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: this.model.get("Name"),
        position: "center",
        draggable: false,
        width: "73%",
        height: "600",
        dialogClass: "paypalDetail"
      };
    };

    return PayPalAccountDetailsView;

  })(Backbone.Marionette.ItemView);

}).call(this);
