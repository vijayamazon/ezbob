function InitAdminArea() {
	var bIsAdmin = $('#chkIsAdmin').attr('checked');

	$('#chkIsAdmin').remove();

	if (!bIsAdmin) {
		$('#divAdminArea, #aToggleAdminArea').remove();
		return;
	} // if

	if ($('#divAdminMsg').text() != '') {
		$('#divAdminArea').show();
		$('#hdReportSelector,#divReportData').hide();
	} // if

	SetToggleAdminAreaText();

	InitReportUserMap();
} // InitAdminArea

function ToggleAdminArea() {
	$('#divAdminArea,#hdReportSelector,#divReportData').toggle();
	SetToggleAdminAreaText();
} // ToggleAdminArea

function SetToggleAdminAreaText() {
	$('#aToggleAdminArea').text(
		$('#divAdminArea').is(':visible') ? 'Reports' : 'Administration'
	);
} // SetToggleAdminAreaText

function InitReportUserMap() {
	$('#divReportUserMap').empty().append(
		$('<table id=tblReportUserMap><thead><tr><th></th></tr></thead><tbody><tr><td></td></tr></tbody></table>')
	);

	var oThead = $('#tblReportUserMap thead tr');
	var oTbody = $('#tblReportUserMap tbody tr');

	var aryMap = $.parseJSON($('#txtReportUserMap').val());

	var oMap = {};

	for (var i = 0; i < aryMap.length; i++) {
		var ru = aryMap[i];

		var nReportID = parseInt(ru.reportId);

		if (!oMap[nReportID])
			oMap[nReportID] = {};

		oMap[nReportID][ru.userId] = 1;
	} // for

	var oUsers = $.parseJSON($('#txtUserList').val());
	var oReports = $.parseJSON($('#txtReportList').val());

	var aryFields = [{
		mData: 'reportName',
		sTitle: 'Report name',
		sCellType: 'th',
	}];

	for (var i in oUsers) {
		oThead.append($('<th></th>'));
		oTbody.append($('<td></td>'));

		aryFields.push({
			mData: i,
			sTitle: oUsers[i],
			sCellType: 'td',
		});
	} // for each user

	var aaData = [];

	for (var rptIdx in oReports) {
		var rpt = oReports[rptIdx];

		var oRow = { reportId: rptIdx, reportName: rpt };

		var oMapEntry = oMap[rptIdx];

		for (var usrIdx in oUsers) {
			var usr = oUsers[usrIdx];

			oRow[usrIdx] = (oMapEntry && oMapEntry[usrIdx]) ? true : false;
		} // for each user

		aaData.push(oRow);
	} // for each report

	$('#tblReportUserMap').addClass('tblReportUserMap table table-bordered table-striped blue-header centered').dataTable({
		bDestroy: true,
		bProcessing: true,
		aaData: aaData,
		aoColumns: aryFields,

		bPaginate: true,
		bSort: false,

		bSortClasses: false,
		asStripClasses: [],

		aLengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, 'All']],
		iDisplayLength: 10,

		bJQueryUI: false,

		fnRowCallback: function(oTR, aryData, nDisplayIdx, nDisplayIdxFull) {
			console.log(oTR, aryData, nDisplayIdx, nDisplayIdxFull);

			var nReportID = aryData.reportId;

			for (var i = 1; i < aryFields.length; i++) {
				var nUserID = aryFields[i].mData;

				// in the following: (i - 1) because the first cell in the row is TH
				var oTD = $('td:eq(' + (i - 1) + ')', oTR);

				var bSelected = aryData[nUserID];

				oTD.empty().attr(
					'data-title', 'Executing of "' + oReports[nReportID] + '" by ' + oUsers[nUserID]
				);

				oTD.attr({
					title: oTD.attr('data-title'),
					id: 'td_map_' + nUserID + '_' + nReportID,
					'data-checked': bSelected ? 1 : 0
				})
				.append(
					$('<img class=map-icon>').attr('src', 'images/' + (bSelected ? 'ok' : 'cross') + '.png')
				).click(function(evt) {
					$('#divPendingActions').show();

					var oTD = $(evt.currentTarget);

					var sID = oTD.attr('id');

					var ary = sID.match(/^td_map_(\d+)_(\d+)$/);

					if (!ary || !ary[1] || !ary[2])
						return;

					var nUserID = parseInt(ary[1]);
					var nReportID = parseInt(ary[2]);

					var bNewValue = parseInt(oTD.attr('data-checked')) ? 0 : 1;

					oTD.attr({
						'data-checked': bNewValue,
						title: oTD.attr('data-title') + ' (PENDING)',
					});

					$('.map-icon', oTD).attr('src', 'images/' + (bNewValue ? 'ok' : 'cross') + '-pending.png');

					var oBase = $('#divPendingActionList');

					var sActionKey = 'action_' + nUserID + '_' + nReportID;

					$('div[data-action-key="' + sActionKey + '"]', oBase).remove();

					oBase.append(
						$('<div></div>').attr({
							'data-action-key': sActionKey,
							'data-action-details': nUserID + ',' + nReportID + ',' + (bNewValue ? '1' : '0')
						}).text((bNewValue ? 'En' : 'Dis') + 'able executing of "' + oReports[nReportID] + '" by ' + oUsers[nUserID])
					);

					var sActions = '';

					$('div[data-action-key]', oBase).each(function() { sActions += "\n" + $(this).attr('data-action-details'); });

					$('#txtPendingActionList').val($.trim(sActions));
				});
			} // for
		}, // fnRowCallback

		aaSorting: [[0, 'asc']],

		bAutoWidth: false,
	}); // create data table

} // InitReportUserMap