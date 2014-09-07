$(document).ready(function() {
	var sApiUrl;

	var oHeadersFunc;

	var oTabs = $('#tabs').tabs();
	
	$('.set-anonymous-api').click(setAnonymousApi);
	$('.set-named-api').click(setNamedApi);

	$('.do-login').click(login);
	$('.do-login-admin').click(function(evt) { login(evt, 'admin', '123456'); });
	$('.do-login-user').click(function(evt) { login(evt, 'user', '654321'); });
	$('.do-read-all').click(readAll);
	$('.do-read-one').click(readOne);
	$('.do-create').click(create);
	$('.do-update').click(update);
	$('.do-delete').click(deleteOne);

	var oLastStatus = $('.last-action-status');

	function getBaseUrl() {
		return 'http://localhost:57973/api/v' + $('[name="api-version"]:checked').val();
	} // getBaseUrl

	function getAnonymousHeaders() {
		return {
			'app-key': $('.app-key').val(),
		};
	} // getAnonymousHeaders

	function getNamedHeaders() {
		return {
			'session-token': $('.session-token').val(),
		};
	} // getNamedHeaders

	function setAnonymousApi() {
		sApiUrl = '/values';

		$('.named-related').hide();

		oHeadersFunc = getAnonymousHeaders;

		if (oTabs.tabs('option', 'active') === 0)
			oTabs.tabs('option', 'active', 1);

		oTabs.tabs('disable', 0);

		$('.set-anonymous-api').hide();
		$('.set-named-api').show();

		$('.page-title').text('Anonymous API demo');

		console.log('Anonymous API mode selected.');
	} // setAnonymousApi

	function setNamedApi() {
		sApiUrl = '/svals';

		$('.named-related').show();

		oHeadersFunc = getNamedHeaders;

		oTabs.tabs('enable', 0);

		$('.set-anonymous-api').show();
		$('.set-named-api').hide();

		$('.page-title').text('Named API demo');

		console.log('Named API mode selected.');
	} // setNamedApi

	function login(evt, sUserName, sPassword) {
		var sFuncName = 'login';

		console.log(sFuncName, 'started...');

		var oContainer = $('#login-form');

		var oInputData = {
			UserName: sUserName || $('.user-name', oContainer).val(),
			Password: sPassword || $('.password', oContainer).val(), 
		};

		if ((oInputData.UserName === '') || (oInputData.Password === '')) {
			oLastStatus.text('Both user name and password must be specified.');
			console.log(sFuncName, 'complete.');
			return;
		} // if

		console.log(sFuncName + ':', 'sending', oInputData);

		$('.logged-in-user-name').text('');
		$('.session-token').val('');

		$.ajax(getBaseUrl() + '/login', {
			type: 'POST',

			contentType: 'application/json',

			data: JSON.stringify(oInputData),

			headers: getAnonymousHeaders(),

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				$('.user-name, .password', oContainer).val('');

				$('.logged-in-user-name').text(oInputData.UserName);
				$('.session-token').val(oData);
			}, // on success

			error: function(jqXhr, sTextStatus, sErrorThrown) {
				console.log(sFuncName + ':', 'error with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'error thrown:', sErrorThrown);

				oLastStatus.text(sFuncName + ': error status code: ' + jqXhr.status + ', status text: "' + jqXhr.statusText + '", error: ' + sErrorThrown);
			}, // on error
		}); // ajax call

		console.log(sFuncName, 'complete.');
	} // function login

	function readAll() {
		var sFuncName = 'readAll';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#read-all');

		var oResults = $('.results', oContainer).empty();

		$.ajax(getBaseUrl() + sApiUrl, {
			type: 'GET',

			contentType: 'application/json',

			headers: oHeadersFunc(),

			success: function(oData, sTextStatus, jqXhr) {
				console.log(sFuncName + ':', 'success with status "', jqXhr.statusText, '" and code', jqXhr.status);
				console.log(sFuncName + ':', 'data is', oData);

				oLastStatus.text(sFuncName + ': success with status code ' + jqXhr.status + ' and status text: ' + jqXhr.statusText);

				var oTemplate = $('.result-template', oContainer);

				if (oData && oData.length) {
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
	} // function readAll

	function readOne() {
		var sFuncName = 'readOne';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#read-one');

		var sValueID = parseInt($('.value-id', oContainer).val());

		if (isNaN(sValueID)) {
			oLastStatus.text('Please specify value id.');
			console.log(sFuncName, 'complete.');
			return;
		} // if

		var oResults = $('.results', oContainer);

		$('span', oResults).empty();

		$.ajax(getBaseUrl() + sApiUrl + '/' + sValueID, {
			type: 'GET',

			contentType: 'application/json',

			headers: oHeadersFunc(),

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
	} // function readOne

	function create() {
		var sFuncName = 'create';

		console.log(sFuncName, 'started...');

		var oContainer = $('#create');

		var oInputData = {
			Title: $('.title', oContainer).val(),
			Content: $('.content', oContainer).val(), 
		};

		console.log(sFuncName + ':', 'sending', oInputData);

		$.ajax(getBaseUrl() + sApiUrl, {
			type: 'POST',

			contentType: 'application/json',

			data: JSON.stringify(oInputData),

			headers: oHeadersFunc(),

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
	} // function create

	function update() {
		var sFuncName = 'update';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#update');

		var sValueID = parseInt($('.value-id', oContainer).val());

		if (isNaN(sValueID)) {
			oLastStatus.text('Please specify value id.');
			console.log(sFuncName, 'complete.');
			return;
		} // if

		var oInputData = {
			Title: $('.title', oContainer).val(),
			Content: $('.content', oContainer).val(), 
		};

		console.log(sFuncName + ':', 'sending', oInputData);

		$.ajax(getBaseUrl() + sApiUrl + '/' + sValueID, {
			type: 'PUT',

			contentType: 'application/json',

			data: JSON.stringify(oInputData),

			headers: oHeadersFunc(),

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
	} // function update

	function deleteOne() {
		var sFuncName = 'deleteOne';

		oLastStatus.text('Processing...');

		console.log(sFuncName, 'started...');

		var oContainer = $('#delete');

		var sValueID = parseInt($('.value-id', oContainer).val());

		if (isNaN(sValueID)) {
			oLastStatus.text('Please specify value id.');
			console.log(sFuncName, 'complete.');
			return;
		} // if

		$.ajax(getBaseUrl() + sApiUrl + '/' + sValueID, {
			type: 'DELETE',

			contentType: 'application/json',

			headers: oHeadersFunc(),

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
	} // function deleteOne

	setAnonymousApi();
}); // on document ready
