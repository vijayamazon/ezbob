(function() {
	$('<div/>').on('ajaxComplete', function(e, xhr) {
		try {
			if (xhr.status === 423) {
				console.log('Request', xhr, 'is not authorized by server (error code 423), refreshing page...');
				var dialog = EzBob.ShowMessageTimeout('You are not authorized', 'Error', 3, function () {
					location.reload();
				}, 'Ok');
				dialog.on('dialogclose', function () {
					location.reload();
				});
				return;
			} // if

			if (!((xhr.responseText != null) || xhr.responseText === "" || typeof xhr.responseText === 'string'))
				return;

			// console.log('Response text:', xhr.responseText);

			var data = JSON.parse(xhr.responseText);

			if (!data.error)
				return;

			alertify.error(data.error);
			console.log(data.error);
			console.log(xhr);
		} catch (oErr) {
			// console.log('Error while handling ajaxComplete event:', oErr);
		} // try
	});
})();
