var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ReportModel = Backbone.Model.extend({
	url: "" + gRootPath + "Underwriter/Report/GetAll"
});

EzBob.Underwriter.ReportView = Backbone.Marionette.ItemView.extend({
	template: "#report-template",
	initialize: function () {
		this.model = new EzBob.Underwriter.ReportModel();
		this.model.on("change reset", this.render, this);
		this.model.fetch();
		return this;
	},
	ui: {
		"reportsDdl": "#reportsDdl",
		"datesDdl": "#datesDdl",
		"reportArea": "#reportDiv",
		"dateRange": "#form-date-range",
		"customerDiv": "#reportCustomerDiv",
		"nonCashDiv": "#reportNonCashDiv",
		"customer": "#reportCustomer",
		"nonCash": "#reportNonCash"
	},
	serializeData: function () {
		return {
			reports: this.model.toJSON()
		};
	},
	events: {
		"change #reportsDdl": "reportChanged",
		"change #datesDdl": "dateChanged",
		"change #reportNonCash": "nonCashChanged",
		"click #getReportBtn": "getReportClicked",
		"click #downloadReportBtn": "downloadReportClicked"
	},
	onRender: function () {
		this.ui.reportsDdl.chosen();
		this.ui.datesDdl.chosen();
		EzBob.handleUserLayoutSetting();
		return this;
	},
	reportChanged: function () {
		var rep, reportId;
		reportId = parseInt(this.ui.reportsDdl.val());
		rep = _.find(this.model.toJSON().reports, (function (_this) {
			return function (report) {
				return report.Id === reportId;
			};
		})(this));
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
	},
	dateChanged: function () {
		if (this.ui.datesDdl.val() === 'Custom') {
			return this.initDateRange();
		} else {
			return this.destroyDateRange();
		}
	},
	nonCashChanged: function () {
		return this.ui.nonCash.val(this.ui.nonCash.is(':checked') ? 'true' : 'false');
	},
	downloadReportClicked: function () {
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
	},
	getReportClicked: function () {
		var fromDate, toDate, xhr;
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
		xhr.done((function (_this) {
			return function (res) {
				if (res.report != null) {
					_this.ui.reportArea.html(res.report);
					_this.ui.reportArea.children().addClass("row");
					return _this.formatTable(res.columns);
				}
			};
		})(this));
		return xhr.always(function () {
			return UnBlockUi();
		});
	},
	formatTable: function (columns) {
		var oDataTableArgs;
		$("#tableReportData").addClass("table table-bordered table-striped blue-header centered");
		oDataTableArgs = {
			aLengthMenu: [[10, 20, 50, 100, 200, -1], [10, 20, 50, 100, 200, "All"]],
			iDisplayLength: 20,
			aaSorting: [],
			aoColumns: columns
		};
		return $("#tableReportData").dataTable(oDataTableArgs);
	},
	initDateRange: function () {
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
		}, function (start, end) {
			$("#form-date-range span").html(start.toString("MMMM d, yyyy") + " - " + end.toString("MMMM d, yyyy"));
		});
		return this.$el.find("#form-date-range span").html(Date.today().add({
			days: -29
		}).toString("MMMM d, yyyy") + " - " + Date.today().toString("MMMM d, yyyy"));
	},
	destroyDateRange: function () {
		this.ui.dateRange.hide();
	},
	show: function () {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});
