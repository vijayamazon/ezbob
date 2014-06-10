EzBob = EzBob || {};

EzBob.DataTables = EzBob.DataTables || {};

EzBob.DataTables.Helper = {
	withScrollbar: function(sContent) {
		return '<div style="overflow: auto; width: auto;">' + sContent + '</div>';
	}, // withScrollbar

	extractColumns: function (sColumns) {
		var aryResult = [];

		var aryNames = sColumns.split(',');

		function renderMoney(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatPoundsNoDecimals(oData);

				case 'filter':
					return oData;

				case 'type':
					return oData;

				case 'sort':
					return oData;

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
					return oData;

				case 'sort':
					return oData;

				default:
					return oData;
			} // switch
		} // renderDate

		function renderDateTime(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatDateTime(oData);

				case 'filter':
					return oData + ' ' + EzBob.formatDateTime(oData);

				case 'type':
					return oData;

				case 'sort':
					return oData;

				default:
					return oData;
			} // switch
		} // renderDateTime

		for (var i = 0; i < aryNames.length; i++) {
			var sName = aryNames[i];

			if (!sName)
				continue;

			var oRenderFunc = null;
			var sClass = '';
			var bVisible = true;
		    
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
			else if (sName[0] === '@') {
			    sName = sName.substr(1);
			    oRenderFunc = renderDateTime;
			} else if (sName[0] === '~') {
			    sName = sName.substr(1);
			    bVisible = false;
			}

			aryResult.push({
				mData: sName,
				sClass: sClass + ' grid-item-' + sName,
				mRender: oRenderFunc,
				bVisible: bVisible
				
			});
		} // for

		return aryResult;
	}, // extractColumns

	showMPIcon: function(cellval) {
		var className, text;
		text = cellval || '';
		className = text.replace(/\s|\d/g, '');
		className = EzBob.CgVendors.all()[className] ? 'cgaccount' : className.toLowerCase();
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
	

	showNewMPsIcon: function (mps, segment) {
	    var mpsList = mps || '';
	    mpsList = mpsList.split(',');
	    var mpsLen = mpsList.length >= 5 ? 5 : mpsList.length;
	    return '<i data-toggle=tooltip title="' + mps.replace(/,/g, '<br>') + '" class="' + segment + mpsLen + '"></i>';
    },
	
	showEmail: function (email) {
	    if (email == null || email == "undefined") return "";
	    return '<span data-toggle=tooltip title="' + email + '">' + email.substr(0, 15) + '</span>';
	}, // showMPsIcon

    showBroker: function(broker, firstSale) {
        if (firstSale) {
            return broker + '<span class="label label-info" data-toggle=tooltip title="First sale">1st</span>';
        }
        return broker;
    }
}; // EzBob.DataTables.Helper
