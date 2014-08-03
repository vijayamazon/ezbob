$(document).ready(function() {
	function formatDateTime(date) {
		if (!date)
			return '';

		var oMoment = moment.utc(date, 'DD/MM/YYYY HH:mm:ss');

		if (oMoment.year() === 1 && oMoment.months() === 0 && oMoment.date() === 1)
			return '';

		return oMoment.format('MMM DD, YYYY HH:mm:ss');
	} // formatDateTime

	function formatDateTimeSort(date) {
		if (!date)
			return '';

		var oMoment = moment.utc(date, 'DD/MM/YYYY HH:mm:ss');

		if (oMoment.year() === 1 && oMoment.months() === 0 && oMoment.date() === 1)
			return '';

		return oMoment.format('YYYYMMDDHHmmss');
	} // formatDateTimeSort

	jQuery.extend(jQuery.fn.dataTableExt.oSort, {
		"formatted-num-pre": function(a) {
			a = (a === "-" || a === "") ? 0 : a.replace(/[^\d\-\.]/g, "");
			return parseFloat(a);
		},

		"formatted-num-asc": function(a, b) {
			return a - b;
		},

		"formatted-num-desc": function(a, b) {
			return b - a;
		}
	}); // sorting plugin for formatted numbers

	InitAdminArea();

	$('#divReportData table').addClass('table table-bordered table-striped blue-header centered');

	var oDataTableArgs = {
		aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
		iDisplayLength: 100,
		aaSorting: [],
	};

	var aoColumns = [];

	try {
		aoColumns = $.parseJSON($('#divReportColumnTypes').text());
	}
	catch (e) {
		// silently ignore
	} // try

	if (aoColumns instanceof Array && aoColumns.length > 0) {
		for (var i = 0; i < aoColumns.length; i++) {
			var oColumn = aoColumns[i];

			if (oColumn.sType === 'date') {
				oColumn.sType = 'string';

				oColumn.mRender = function(oData, sAction, oFullSource) {
					switch (sAction) {
					case 'display':
						return formatDateTime(oData);

					case 'filter':
						return oData + ' ' + formatDateTime(oData);

					case 'sort':
						return formatDateTimeSort(oData);

					case 'type':
						return '';

					default:
						return oData;
					} // switch
				}; // renderDate
			}
			else if (oColumn.sType === 'user-id') {
				oColumn.sType = 'formatted-num';

				oColumn.mRender = function(oData, sAction, oFullSource) {
					switch (sAction) {
					case 'display':
						return oData;

					case 'filter':
					case 'sort':
						var ary = />(\d+)<\/a>$/.exec(oData);
						return ary[1];

					case 'type':
						return '';

					default:
						return oData;
					} // switch
				}; // renderDate
			} // if
		} // for each column

		oDataTableArgs.aoColumns = aoColumns;
	} // if

	$('#tableReportData').dataTable(oDataTableArgs);

	$("#tdEntries").append($("#tableReportData_length"));
	$("#tdSearch").append($("#tableReportData_filter"));
	$("#tdShowing").append($("#tableReportData_info"));
	$("#tdNavigation").append($("#tableReportData_paginate"));
	$("tr").removeAttr("style");
}); // on document ready
