EzBob = EzBob || {};

EzBob.DataTables = EzBob.DataTables || {};

EzBob.DataTables.Helper = {
	CGAccounts: null,

	withScrollbar: function(sContent) {
		return '<div style="overflow: auto; width: auto;">' + sContent + '</div>';
	}, // withScrollbar

	extractColumns: function(sColumns) {
		var aryResult = [];

		var aryNames = sColumns.split(',');

		function renderMoney(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatPoundsNoDecimals(oData);

				case 'filter':
				case 'type':
				case 'sort':
				default:
					return oData;
			} // switch
		} // renderMoney

		function renderDate(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatDate(oData);

				case 'filter':
					return oData + ' ' + EzBob.formatDate(oData);

				case 'type':
				case 'sort':
				default:
					return oData;
			} // switch
		} // renderDate

		for (var i = 0; i < aryNames.length; i++) {
			var sName = aryNames[i];

			if (!sName)
				continue;

			var oRenderFunc = null;
			var sClass = '';

			if (sName[0] === '#') {
				sClass = 'numeric';
				sName = sName.substr(1);
			}
			else if (sName[0] === '$') {
				sClass = 'numeric';
				sName = sName.substr(1);
				oRenderFunc = renderMoney;
			}
			else if (sName[0] === '^') {
				sName = sName.substr(1);
				oRenderFunc = renderDate;
			}

			aryResult.push({
				mData: sName,
				sClass: sClass + ' grid-item-' + sName,
				mRender: oRenderFunc,
			});
		} // for

		return aryResult;
	}, // extractColumns

	showMPIcon: function(cellval) {
		if (!EzBob.DataTables.Helper.CGAccounts)
			EzBob.DataTables.Helper.CGAccounts = $.parseJSON($('div#cg-account-list').text());

		var className, text;
		text = cellval || '';
		className = text.replace(/\s|\d/g, '');
		className = EzBob.DataTables.Helper.CGAccounts[className] ? 'cgaccount' : className.toLowerCase();
		return '<i data-toggle=tooltip title="' + text + '" class="' + className + '"></i>';
	}, // showMPIcon

	showMPsIcon: function(cellval) {
		var mps, retVal;

		mps = cellval || '';
		mps = mps.split(',').clean('');
		retVal = '';

		_.each(mps, function(val) {
			return retVal += EzBob.DataTables.Helper.showMPIcon(val);
		});

		return '<div style="overflow: auto; width: 102%;">' + (retVal + ' ') + '</div>';
	}, // showMPsIcon

}; // EzBob.DataTables.Helper
