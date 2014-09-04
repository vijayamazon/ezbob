$(document).ready(function() {
	var sBaseUrl = 'http://localhost:57973/api/v1';

	var sAnoUrl = sBaseUrl + '/values';

	var oAnonymousBody = $('#anonymous-api-body');
	
	oAnonymousBody.tabs();
	
	$('.do-read-all', oAnonymousBody).click(anonymousReadAll);
	$('.do-read-one', oAnonymousBody).click(anonymousReadOne);
	$('.do-create', oAnonymousBody).click(anonymousCreate);
	$('.do-update', oAnonymousBody).click(anonymousUpdate);
	$('.do-delete', oAnonymousBody).click(anonymousDelete);

	var oLastStatus = $('.last-action-status');

	function anonymousReadAll() {
		var sFuncName = 'anonymousReadAll';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#anonymous-read-all');

		var oResults = $('.results', oContainer).empty();

		$.ajax(sAnoUrl, {
			type: 'GET',

			contentType: 'application/json',

			headers: {
				'app-key': $('.app-key').val(),
			}, // headers

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'data is', oData);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				var oTemplate = $('.result-template', oContainer);

				if (oData.length) {
					for (var i = 0; i < oData.length; i++) {
						var oItem = oData[i];

						var oDom = oTemplate.clone().removeClass('result-template');

						oDom.find('td').load_display_value({ data_source: oItem, });

						oResults.append(oDom);
					} // for each
				} // if
			}, // on success

			error: function(jqXhr, sTextStatus, sErrorThrown) {
				console.log(sFuncName + ':', 'error with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'error thrown:', sErrorThrown);

				oLastStatus.text(sFuncName + ': error status code: ' + jqXhr.status + ', status text: "' + jqXhr.statusText + '", error: ' + sErrorThrown);
			}, // on error
		}); // ajax call

		console.log(sFuncName, 'complete.');
	} // function anonymousReadAll

	function anonymousReadOne() {
		var sFuncName = 'anonymousReadOne';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#anonymous-read-one');

		var sValueID = parseInt($('.value-id', oContainer).val());

		if (isNaN(sValueID)) {
			oLastStatus.text('Please specify value id.');
			return;
		} // if

		var oResults = $('.results', oContainer);

		$('span', oResults).empty();

		$.ajax(sAnoUrl + '/' + sValueID, {
			type: 'GET',

			contentType: 'application/json',

			headers: {
				'app-key': $('.app-key').val(),
			}, // headers

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'data is', oData);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				oResults.find('span').load_display_value({ data_source: oData, });

				$('.value-id', oContainer).val('');
			}, // on success

			error: function(jqXhr, sTextStatus, sErrorThrown) {
				console.log(sFuncName + ':', 'error with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'error thrown:', sErrorThrown);

				oLastStatus.text(sFuncName + ': error status code: ' + jqXhr.status + ', status text: "' + jqXhr.statusText + '", error: ' + sErrorThrown);
			}, // on error
		}); // ajax call

		console.log(sFuncName, 'complete.');
	} // function anonymousReadOne

	function anonymousCreate() {
		var sFuncName = 'anonymousCreate';

		console.log(sFuncName, 'started...');

		var oContainer = $('#anonymous-create');

		var oInputData = {
			Title: $('.title', oContainer).val(),
			Content: $('.content', oContainer).val(), 
		};

		console.log(sFuncName + ':', 'sending', oInputData);

		$.ajax(sAnoUrl, {
			type: 'POST',

			contentType: 'application/json',

			data: JSON.stringify(oInputData),

			headers: {
				'app-key': $('.app-key').val(),
			}, // headers

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				$('.title, .content', oContainer).val('');
			}, // on success

			error: function(jqXhr, sTextStatus, sErrorThrown) {
				console.log(sFuncName + ':', 'error with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'error thrown:', sErrorThrown);

				oLastStatus.text(sFuncName + ': error status code: ' + jqXhr.status + ', status text: "' + jqXhr.statusText + '", error: ' + sErrorThrown);
			}, // on error
		}); // ajax call

		console.log(sFuncName, 'complete.');
	} // function anonymousCreate

	function anonymousUpdate() {
		var sFuncName = 'anonymousUpdate';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#anonymous-update');

		var sValueID = parseInt($('.value-id', oContainer).val());

		if (isNaN(sValueID)) {
			oLastStatus.text('Please specify value id.');
			return;
		} // if

		var oInputData = {
			Title: $('.title', oContainer).val(),
			Content: $('.content', oContainer).val(), 
		};

		console.log(sFuncName + ':', 'sending', oInputData);

		$.ajax(sAnoUrl + '/' + sValueID, {
			type: 'PUT',

			contentType: 'application/json',

			data: JSON.stringify(oInputData),

			headers: {
				'app-key': $('.app-key').val(),
			}, // headers

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'data is', oData);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				$('.value-id, .title, .content', oContainer).val('');
			}, // on success

			error: function(jqXhr, sTextStatus, sErrorThrown) {
				console.log(sFuncName + ':', 'error with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'error thrown:', sErrorThrown);

				oLastStatus.text(sFuncName + ': error status code: ' + jqXhr.status + ', status text: "' + jqXhr.statusText + '", error: ' + sErrorThrown);
			}, // on error
		}); // ajax call

		console.log(sFuncName, 'complete.');
	} // function anonymousUpdate

	function anonymousDelete() {
		var sFuncName = 'anonymousDelete';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#anonymous-delete');

		var sValueID = parseInt($('.value-id', oContainer).val());

		if (isNaN(sValueID)) {
			oLastStatus.text('Please specify value id.');
			return;
		} // if

		$.ajax(sAnoUrl + '/' + sValueID, {
			type: 'DELETE',

			contentType: 'application/json',

			headers: {
				'app-key': $('.app-key').val(),
			}, // headers

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'data is', oData);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				$('.value-id', oContainer).val('');
			}, // on success

			error: function(jqXhr, sTextStatus, sErrorThrown) {
				console.log(sFuncName + ':', 'error with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'error thrown:', sErrorThrown);

				oLastStatus.text(sFuncName + ': error status code: ' + jqXhr.status + ', status text: "' + jqXhr.statusText + '", error: ' + sErrorThrown);
			}, // on error
		}); // ajax call

		console.log(sFuncName, 'complete.');
	} // function anonymousDelete
}); // on document ready
