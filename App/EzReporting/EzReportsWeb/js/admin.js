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