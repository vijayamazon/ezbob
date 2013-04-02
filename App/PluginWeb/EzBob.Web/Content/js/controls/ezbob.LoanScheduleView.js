(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.LoanScheduleView = (function(_super) {

    __extends(LoanScheduleView, _super);

    function LoanScheduleView() {
      return LoanScheduleView.__super__.constructor.apply(this, arguments);
    }

    LoanScheduleView.prototype.template = "#loan-schedule-template";

    LoanScheduleView.prototype.serializeData = function() {
      return {
        schedule: this.options.schedule.schedule,
        apr: this.options.schedule.apr,
        setupFee: this.options.schedule.setupFee,
        realInterestCost: this.options.schedule.realInterestCost,
        total: this.options.schedule.total,
        totalInterest: this.options.schedule.totalInterest,
        totalPrincipal: this.options.schedule.totalPrincipal,
        isShowGift: this.options.isShowGift
      };
    };

    return LoanScheduleView;

  })(Backbone.Marionette.ItemView);

  EzBob.LoanScheduleViewDlg = (function(_super) {

    __extends(LoanScheduleViewDlg, _super);

    function LoanScheduleViewDlg() {
      return LoanScheduleViewDlg.__super__.constructor.apply(this, arguments);
    }

    LoanScheduleViewDlg.prototype.jqoptions = function() {
      return {
        modal: true,
        width: 600
      };
    };

    return LoanScheduleViewDlg;

  })(EzBob.LoanScheduleView);

}).call(this);
