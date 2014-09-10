(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.ReportModel = (function(_super) {

    __extends(ReportModel, _super);

    function ReportModel() {
      return ReportModel.__super__.constructor.apply(this, arguments);
    }

    ReportModel.prototype.url = "" + gRootPath + "Underwriter/Report/GetAll";

    return ReportModel;

  })(Backbone.Model);

  EzBob.Underwriter.ReportView = (function(_super) {

    __extends(ReportView, _super);

    function ReportView() {
      return ReportView.__super__.constructor.apply(this, arguments);
    }

    ReportView.prototype.template = "#report-template";

    ReportView.prototype.initialize = function() {
      this.model = new EzBob.Underwriter.ReportModel();
      this.model.on("change reset", this.render, this);
      return this.model.fetch();
    };

    ReportView.prototype.ui = {
      "reportsDdl": "#reportsDdl",
      "datesDdl": "#datesDdl",
      "reportArea": "#reportDiv",
      "dateRange": "#form-date-range",
      "customerDiv": "#reportCustomerDiv",
      "nonCashDiv": "#reportNonCashDiv",
      "customer": "#reportCustomer",
      "nonCash": "#reportNonCash"
    };

    ReportView.prototype.serializeData = function() {
      return {
        reports: this.model.toJSON()
      };
    };

    ReportView.prototype.events = {
      "change #reportsDdl": "reportChanged",
      "change #datesDdl": "dateChanged",
      "change #reportNonCash": "nonCashChanged",
      "click #getReportBtn": "getReportClicked",
      "click #downloadReportBtn": "downloadReportClicked"
    };

    ReportView.prototype.onRender = function() {
      this.ui.reportsDdl.chosen();
      this.ui.datesDdl.chosen();
      return EzBob.handleUserLayoutSetting();
    };

    ReportView.prototype.reportChanged = function() {
      var rep, reportId,
        _this = this;
      reportId = parseInt(this.ui.reportsDdl.val());
      rep = _.find(this.model.toJSON().reports, function(report) {
        return report.Id === reportId;
      });
      if ((rep != null) && rep.IsCustomer) {
        this.ui.customerDiv.show();
      } else {
        this.ui.customerDiv.hide();
      }
      if ((rep != null) && rep.ShowNonCash) {
        this.ui.nonCashDiv.show();
        return this.nonCashChanged();
      } else {
        this.ui.nonCashDiv.hide();
        return this.ui.nonCash.val("");
      }
    };

    ReportView.prototype.dateChanged = function() {
      if (this.ui.datesDdl.val() === 'Custom') {
        return this.initDateRange();
      } else {
        return this.destroyDateRange();
      }
    };

    ReportView.prototype.nonCashChanged = function() {
      return this.ui.nonCash.val(this.ui.nonCash.is(':checked') ? 'true' : 'false');
    };

    ReportView.prototype.downloadReportClicked = function() {
      var from, to;
      if (this.ui.reportsDdl.val() === '0' || this.ui.datesDdl.val() === '0') {
        alertify.error('Select report and/or date range');
        return false;
      }
      if (this.ui.datesDdl.val() === 'Custom') {
        from = moment(this.ui.dateRange.data('daterangepicker').startDate).format("YYYY-MM-DD");
        to = moment(this.ui.dateRange.data('daterangepicker').endDate).format("YYYY-MM-DD");
        return window.location = "" + window.gRootPath + "Underwriter/Report/DownloadReportDates/?reportId=" + (this.ui.reportsDdl.val()) + "&from=" + from + "&to=" + to + "&customer=" + (this.ui.customer.val()) + "&nonCash=" + (this.ui.nonCash.val());
      } else {
        return window.location = "" + window.gRootPath + "Underwriter/Report/DownloadReport/?reportId=" + (this.ui.reportsDdl.val()) + "&reportDate=" + (this.ui.datesDdl.val()) + "&customer=" + (this.ui.customer.val()) + "&nonCash=" + (this.ui.nonCash.val());
      }
    };

    ReportView.prototype.getReportClicked = function() {
      var fromDate, toDate, xhr,
        _this = this;
      if (this.ui.reportsDdl.val() === '0' || this.ui.datesDdl.val() === '0') {
        alertify.error('Select report and/or date range');
        return false;
      }
      if (this.ui.datesDdl.val() === 'Custom') {
        fromDate = moment(this.ui.dateRange.data('daterangepicker').startDate).format("YYYY-MM-DD");
        toDate = moment(this.ui.dateRange.data('daterangepicker').endDate).format("YYYY-MM-DD");
        xhr = $.post("" + window.gRootPath + "Underwriter/Report/GetReportDates", {
          reportId: this.ui.reportsDdl.val(),
          from: fromDate,
          to: toDate,
          customer: this.ui.customer.val(),
          nonCash: this.ui.nonCash.val()
        });
      } else {
        xhr = $.post("" + window.gRootPath + "Underwriter/Report/GetReport", {
          reportId: this.ui.reportsDdl.val(),
          reportDate: this.ui.datesDdl.val(),
          customer: this.ui.customer.val(),
          nonCash: this.ui.nonCash.val()
        });
      }
      BlockUi();
      xhr.done(function(res) {
        if (res.report != null) {
          _this.ui.reportArea.html(res.report);
          _this.ui.reportArea.children().addClass("row");
          return _this.formatTable(res.columns);
        }
      });
      return xhr.always(function() {
        return UnBlockUi();
      });
    };

    ReportView.prototype.formatTable = function(columns) {
      var oDataTableArgs;
      $("#tableReportData").addClass("table table-bordered table-striped blue-header centered");
      oDataTableArgs = {
        aLengthMenu: [[10, 20, 50, 100, 200, -1], [10, 20, 50, 100, 200, "All"]],
        iDisplayLength: 20,
        aaSorting: [],
        aoColumns: columns
      };
      return $("#tableReportData").dataTable(oDataTableArgs);
    };

    ReportView.prototype.initDateRange = function() {
      this.ui.dateRange.show();
      this.ui.dateRange.daterangepicker({
        format: "MM/dd/yyyy",
        startDate: Date.today().add({
          days: -29
        }),
        endDate: Date.today(),
        minDate: "01/01/2012",
        locale: {
          applyLabel: "Select",
          fromLabel: "From",
          toLabel: "To&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
        },
        showWeekNumbers: true,
        buttonClasses: ['btn-success', 'btn-fullwidth']
      }, function(start, end) {
        $("#form-date-range span").html(start.toString("MMMM d, yyyy") + " - " + end.toString("MMMM d, yyyy"));
      });
      return this.$el.find("#form-date-range span").html(Date.today().add({
        days: -29
      }).toString("MMMM d, yyyy") + " - " + Date.today().toString("MMMM d, yyyy"));
    };

    ReportView.prototype.destroyDateRange = function() {
      this.ui.dateRange.hide();
    };

    ReportView.prototype.show = function() {
      return this.$el.show();
    };

    ReportView.prototype.hide = function() {
      return this.$el.hide();
    };

    return ReportView;

  })(Backbone.Marionette.ItemView);

}).call(this);
