(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.BankAccountDetailsView = (function(_super) {

    __extends(BankAccountDetailsView, _super);

    function BankAccountDetailsView() {
      return BankAccountDetailsView.__super__.constructor.apply(this, arguments);
    }

    BankAccountDetailsView.prototype.template = "#bank-account-details-template";

    BankAccountDetailsView.prototype.initialize = function() {
      return this.bindTo(this.model, "change init", this.render, this);
    };

    BankAccountDetailsView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: 'Bank Account Info',
        position: "center",
        draggable: false,
        width: "550px",
        height: "700",
        dialogClass: "paypalDetail"
      };
    };

    return BankAccountDetailsView;

  })(Backbone.Marionette.ItemView);

}).call(this);
