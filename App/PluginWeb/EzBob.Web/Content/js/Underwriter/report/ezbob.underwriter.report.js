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
      "reportArea": "#reportDiv"
    };

    ReportView.prototype.serializeData = function() {
      return {
        reports: this.model.toJSON()
      };
    };

    ReportView.prototype.events = {
      "change #reportsDdl": "reportChanged",
      "click #getReportBtn": "getReportClicked"
    };

    ReportView.prototype.onRender = function() {
      this.ui.reportsDdl.chosen();
      return this.ui.datesDdl.chosen();
    };

    ReportView.prototype.reportChanged = function() {
      return console.log("report changed");
    };

    ReportView.prototype.getReportClicked = function() {
      var xhr,
        _this = this;
      console.log('get report clicked', this.ui.reportsDdl.val(), this.ui.datesDdl.val());
      xhr = $.post("" + window.gRootPath + "Underwriter/Report/GetReport", {
        reportId: this.ui.reportsDdl.val(),
        reportDate: this.ui.datesDdl.val()
      });
      return xhr.done(function(res) {
        if (res.report != null) {
          _this.ui.reportArea.html(res.report);
          return _this.formatTable(res.columns);
        }
      });
    };

    ReportView.prototype.formatTable = function(columns) {
      var oDataTableArgs;
      $("#tableReportData").addClass("table table-bordered table-striped blue-header centered");
      oDataTableArgs = {
        aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
        iDisplayLength: 100,
        aaSorting: [],
        aoColumns: columns
      };
      return $("#tableReportData").dataTable(oDataTableArgs);
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
