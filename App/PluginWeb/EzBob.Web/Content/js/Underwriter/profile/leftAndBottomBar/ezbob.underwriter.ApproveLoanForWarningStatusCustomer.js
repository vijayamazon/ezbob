(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.ApproveLoanForWarningStatusCustomer = (function(_super) {

    __extends(ApproveLoanForWarningStatusCustomer, _super);

    function ApproveLoanForWarningStatusCustomer() {
      return ApproveLoanForWarningStatusCustomer.__super__.constructor.apply(this, arguments);
    }

    ApproveLoanForWarningStatusCustomer.prototype.template = '#approve-loan-for-warning-status-customer';

    ApproveLoanForWarningStatusCustomer.prototype.initialize = function(options) {
      this.model = options.model;
      this.parent = options.parent;
      return ApproveLoanForWarningStatusCustomer.__super__.initialize.call(this);
    };

    ApproveLoanForWarningStatusCustomer.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Warning",
        position: "center",
        draggable: false,
        width: "73%",
        height: Math.max(window.innerHeight * 0.9, 600),
        dialogClass: "warning-customer-status-popup"
      };
    };

    ApproveLoanForWarningStatusCustomer.prototype.render = function() {
      ApproveLoanForWarningStatusCustomer.__super__.render.call(this);
      return this;
    };

    ApproveLoanForWarningStatusCustomer.prototype.serializeData = function() {
      return {
        m: this.model.toJSON()
      };
    };

    ApproveLoanForWarningStatusCustomer.prototype.onSave = function() {
      this.close();
      this.parent.CreateApproveDialog();
      return false;
    };

    return ApproveLoanForWarningStatusCustomer;

  })(EzBob.BoundItemView);

}).call(this);
