EzBob = EzBob || {};

EzBob.DataTables = EzBob.DataTables || {};

EzBob.DataTables.Helper = {
	withScrollbar: function(sContent) {
		return '<div style="overflow: auto; width: auto;">' + sContent + '</div>';
	}, // withScrollbar

	extractColumns: function (sColumns) {
		var aryResult = [];

		var aryNames = sColumns.split(',');
		function renderPercent(oData, sAction, oFullSource) {
		    switch (sAction) {
		        case 'display':
		            return EzBob.formatPercents(oData);

		        case 'filter':
		            return oData;

		        case 'type':
		            return oData;

		        case 'sort':
		            return oData;

		        default:
		            return oData;
		    } // switch
		}
	    
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
					return EzBob.formatDateUK(oData);

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
					return oData + ' ' + EzBob.formatDateTimeUK(oData);

				case 'type':
					return oData;

				case 'sort':
					return oData;

				default:
					return oData;
			} // switch
		} // renderDateTime

		function renderTimeFromNow(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatTimeFromNow(oData);

				case 'filter':
					return oData;

				case 'type':
					return oData;

				case 'sort':
					return oData;

				default:
					return oData;
			} // switch
		} // renderTimeSpanToAutoReject
		
		function renderDateTimeDelimited(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatDateTimeDelimitedUK(oData);

				case 'filter':
					return oData + ' ' + EzBob.formatDateTimeUK(oData);

				case 'type':
					return oData;

				case 'sort':
					return oData;

				default:
					return oData;
			} // switch
		} // renderDateTimeDelimited

		function renderMonth(oData, sAction, oFullSource) {
			switch (sAction) {
				case 'display':
					return EzBob.formatMonths(oData);

				case 'filter':
					return oData;

				case 'type':
					return oData;

				case 'sort':
					return oData;

				default:
					return oData;
			} // switch
		} // renderMonth

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
				sClass = 'numeric grid-money';
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
			}
			else if (sName[0] === '~') {
			    sName = sName.substr(1);
			    bVisible = false;
			}
			else if (sName[0] === '%') {
			    sClass = 'numeric';
			    sName = sName.substr(1);
			    oRenderFunc = renderPercent;
			}
			else if (sName[0] === '&') {
				sName = sName.substr(1);
				oRenderFunc = renderTimeFromNow;
			}
			else if (sName[0] === '*') {
				sName = sName.substr(1);
				oRenderFunc = renderDateTimeDelimited;
			}
			else if (sName[0] === '!') {
				sName = sName.substr(1);
				oRenderFunc = renderMonth;
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
		var numMps = mpsList.length - 1;
		var mpsLen = numMps >= 5 ? 5 : numMps;
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
    },
    
    initCustomFiltering: function(){
        if ($.fn.dataTableExt.afnFiltering.length == 0) {
            $.fn.dataTableExt.afnFiltering.push(
                function (oSettings, aData, iDataIndex) {
                    if (oSettings.sTableId != "YodleeTransactionsTable") {
                        // if not table should be ignored
                        return true;
                    }
                    var dateRange = $('#date-range').attr("value");
                    if (dateRange == null || dateRange == "") return true;

                    var dateMin = dateRange.substring(0, 4) + dateRange.substring(5, 7) + dateRange.substring(8, 10);
                    var dateMax = dateRange.substring(13, 17) + dateRange.substring(18, 20) + dateRange.substring(21, 23);
                    var date = aData[2];
                    if (date == null) {
                        return true;
                    }
                    date = date.substring(0, 10);
                    date = date.substring(6, 10) + date.substring(3, 5) + date.substring(0, 2);
                    if (dateMin == "" && date <= dateMax) {
                        return true;
                    } else if (dateMin == "" && date <= dateMax) {
                        return true;
                    } else if (dateMin <= date && "" == dateMax) {
                        return true;
                    } else if (dateMin <= date && date <= dateMax) {
                        return true;
                    }
                    return false;
                }
            );
            
            $.fn.dataTableExt.afnFiltering.push(
                function (oSettings, aData, iDataIndex) {
                    if (oSettings.sTableId != "journalTable") {
                        // if not table should be ignored
                        return true;
                    }
                    
                    if ($("#allJournal").is(":checked")) {
                        return true;
                    }
                    
                    var uwColumn = 12;
                    var isCrmColumn = 14;
                    
                    if ($("#uwNotes").is(":checked") &&
                        aData[uwColumn] &&
                        EzBob.DataTables.Helper.isUW(aData[uwColumn].toLowerCase()) &&
                        aData[uwColumn] != "System" &&
                        aData[isCrmColumn]) {
                        return true;
                    }
                    
                    if ($("#crmNotes").is(":checked") &&
                        aData[uwColumn] &&
                        EzBob.DataTables.Helper.isCrm(aData[uwColumn].toLowerCase()) &&
                        aData[isCrmColumn]) {
                        return true;
                    }
                    
                    if ($("#loanAction").is(":checked") &&
                        aData[uwColumn] &&
                        aData[uwColumn] == "System" &&
                        aData[isCrmColumn]) {
                        return true;
                    }
                    
                    if ($("#manualDecision").is(":checked") &&
                        aData[uwColumn] &&
                        aData[uwColumn] != "se" &&
                        !aData[isCrmColumn]) {
                        return true;
                    }
                    
                    if ($("#systemDecision").is(":checked") &&
                        aData[uwColumn] &&
                        aData[uwColumn] == "se" &&
                        !aData[isCrmColumn]) {
                        return true;
                    }
                    
                    return false;
                }
            );
        }
    },
    
    isCrm: function (uwName) {
    	if (uwName.indexOf("emma")      === 0 ||
			uwName.indexOf("ros")       === 0 ||
			uwName.indexOf('clareh')    === 0 ||
			uwName.indexOf("jamiem")    === 0 ||
			uwName.indexOf("sarahb")    === 0 ||
			uwName.indexOf("sarahd")    === 0 ||
			uwName.indexOf("travis")    === 0 ||
			uwName.indexOf("emanuelle") === 0) {
            return true;
        }
        return false;
    },

    isUW: function (uwName) {
    	if (uwName.indexOf('vitasd')   === 0 ||
			uwName.indexOf('songulo')  === 0 ||
			uwName.indexOf('galitg')   === 0 ||
			uwName.indexOf('dinusanp') === 0 ||
			uwName.indexOf('adic')     === 0) {
			return true;
		}
		return false;
	}
}; // EzBob.DataTables.Helper
