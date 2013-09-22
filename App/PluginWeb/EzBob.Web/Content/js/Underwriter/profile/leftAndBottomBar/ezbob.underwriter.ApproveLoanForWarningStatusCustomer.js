(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.ApproveLoanForWarningStatusCustomer = (function(_super) {
    __extends(ApproveLoanForWarningStatusCustomer, _super);

    function ApproveLoanForWarningStatusCustomer() {
      _ref = ApproveLoanForWarningStatusCustomer.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    ApproveLoanForWarningStatusCustomer.prototype.template = '#approve-loan-for-warning-status-customer';

    ApproveLoanForWarningStatusCustomer.prototype.initialize = function(options) {
      this.model = options.model;
      this.parent = options.parent;
      return ApproveLoanForWarningStatusCustomer.__super__.initialize.call(this);
    };

    ApproveLoanForWarningStatusCustomer.prototype.render = function() {
      ApproveLoanForWarningStatusCustomer.__super__.render.call(this);
      return this;
    };

    ApproveLoanForWarningStatusCustomer.prototype.onSave = function() {
      this.close();
      this.parent.CreateApproveDialog();
      return false;
    };

    return ApproveLoanForWarningStatusCustomer;

  })(EzBob.BoundItemView);

}).call(this);
