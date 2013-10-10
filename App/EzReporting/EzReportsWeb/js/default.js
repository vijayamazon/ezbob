$(document).ready(function () {
	jQuery.extend(jQuery.fn.dataTableExt.oSort, {
		"formatted-num-pre": function (a) {
			a = (a === "-" || a === "") ? 0 : a.replace(/[^\d\-\.]/g, "");
			return parseFloat(a);
		},

		"formatted-num-asc": function (a, b) {
			return a - b;
		},

		"formatted-num-desc": function (a, b) {
			return b - a;
		}
	}); // sorting plugin for formatted numbers

	InitAdminArea();

	$('#divReportData table').addClass('table table-bordered table-striped blue-header centered');

	var oDataTableArgs = {
		aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
		iDisplayLength: 100,
		aaSorting: [],
		fnRowCallback: function(nRow, aData, iDataIndex, iDataIndexFull) {
			var ary = /^(\d\d)\/(\d\d)\/(\d\d\d\d) (\d\d):(\d\d):(\d\d)$/.exec(aData[0]);
			if (ary)
				aData[0] = new Date(ary[3], ary[2] - 1, ary[1], ary[4], ary[5], ary[6]);
		}
	};

	var aoColumns = [];

	try {
		aoColumns = $.parseJSON($('#divReportColumnTypes').text());
	}
	catch (e) {
		// silently ignore
	} // try

	if (aoColumns instanceof Array && aoColumns.length > 0)
		oDataTableArgs.aoColumns = aoColumns;

	$('#tableReportData').dataTable(oDataTableArgs);

	$("#tdEntries").append($("#tableReportData_length"));
	$("#tdSearch").append($("#tableReportData_filter"));
	$("#tdShowing").append($("#tableReportData_info"));
	$("#tdNavigation").append($("#tableReportData_paginate"));
	$("tr").removeAttr("style");
}); // on document ready
