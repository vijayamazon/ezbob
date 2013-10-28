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
      var data;
      data = {
        schedule: this.options.schedule.Schedule,
        apr: this.options.schedule.Apr,
        setupFee: this.options.schedule.SetupFee,
        realInterestCost: this.options.schedule.RealInterestCost,
        total: this.options.schedule.Total,
        totalInterest: this.options.schedule.TotalInterest,
        totalPrincipal: this.options.schedule.TotalPrincipal,
        isShowGift: this.options.isShowGift,
        isShowExportBlock: this.options.isShowExportBlock,
        OfferedCreditLine: this.options.schedule.Details.OfferedCreditLine,
        RepaymentPerion: this.options.schedule.Details.RepaymentPeriod,
        InterestRate: this.options.schedule.Details.InterestRate,
        LoanType: this.options.schedule.Details.LoanType,
        isShowExceedMaxInterestForSource: this.options.isShowExceedMaxInterestForSource,
        MaxInterestForSource: this.options.schedule.MaxInterestForSource,
        LoanSourceName: this.options.schedule.LoanSourceName
      };
      if (data.MaxInterestForSource === null) {
        data.MaxInterestForSource = -1;
      }
      return data;
    };

    return LoanScheduleView;

  })(Backbone.Marionette.ItemView);

  EzBob.LoanScheduleViewDlg = (function(_super) {

    __extends(LoanScheduleViewDlg, _super);

    function LoanScheduleViewDlg() {
      return LoanScheduleViewDlg.__super__.constructor.apply(this, arguments);
    }

    LoanScheduleViewDlg.prototype.events = {
      "click .pdf-link": "exportToPdf",
      "click .excel-link": "exportToExcel"
    };

    LoanScheduleViewDlg.prototype.exportToPdf = function(e) {
      var $el;
      $el = $(e.currentTarget);
      return $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" + this.options.offerId + "&isExcel=false&isShowDetails=false&customerId=" + this.options.customerId);
    };

    LoanScheduleViewDlg.prototype.exportToExcel = function(e) {
      var $el;
      $el = $(e.currentTarget);
      return $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" + this.options.offerId + "&isExcel=true&isShowDetails=false&customerId=" + this.options.customerId);
    };

    LoanScheduleViewDlg.prototype.jqoptions = function() {
      return {
        modal: true,
        width: 600
      };
    };

    return LoanScheduleViewDlg;

  })(EzBob.LoanScheduleView);

}).call(this);
