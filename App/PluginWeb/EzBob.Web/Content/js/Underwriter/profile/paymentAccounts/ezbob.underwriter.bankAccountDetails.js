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

    BankAccountDetailsView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Bank Account Details",
        position: "center",
        draggable: false,
        width: "73%",
        height: Math.max(window.innerHeight * 0.9, 600),
        dialogClass: "bank-account-details-popup"
      };
    };

    return BankAccountDetailsView;

  })(Backbone.Marionette.ItemView);

}).call(this);
