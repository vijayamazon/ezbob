var EzBob = EzBob || {};

EzBob.Underwriter.BrokerProfileView = EzBob.View.extend({
	initialize: function() {
		this.brokerID = null;
		this.theTable = null;

		this.nowLoadingCustomers = false;
		this.nowLoadingBrokerProperties = false;
		this.nowLoadingCrm = false;
	}, // initialize

	events: {
		'click .reset-password-123456': 'resetPassword123456',
	}, // events

	resetPassword123456: function() {
		event.preventDefault();
		event.stopPropagation();
		
		var self = this;

		EzBob.ShowMessage(
			"Do you really want to reset this broker password?",
			"Please confirm",
			function() {
				$.post("" + window.gRootPath + "Underwriter/Brokers/ResetPassword123456", {
					nBrokerID: self.brokerID,
				});

				EzBob.ShowMessageTimeout('Broker password has been reset to 123456.', 'Success', 3);
			}, // on ok
			"Reset",
			null,
			"Keep"
		);

		return false;
	}, // resetPassword123456

	render: function() {
		var self = this;

		this.myTabHeaders().on('shown.bs.tab', function(e) {
			var sSection = $(e.target).attr('href').substr(1);
			self.handleTabSwitch(sSection);
			self.redrawCurrentTab(sSection);
		});
	}, // render

	startDisplayBrokerRelations: function() {
		if (this.nowLoadingCrm)
			return;

		this.nowLoadingCrm = true;

		if (!this.brokerID) {
			EzBob.ShowMessage('Cannot display broker relations: no user id found.');
			this.nowLoadingCrm = false;
			return;
		} // if

		var crmModel = new EzBob.Underwriter.CustomerRelationsModel();

		var oView = new EzBob.Underwriter.CustomerRelationsView({
			el: this.$el.find('.broker-relations-root-el'),
			model: crmModel,
			isBroker: true,
		});

		crmModel.customerId = this.brokerID;
		crmModel.fetch();

		this.nowLoadingCrm = false;
	}, // startDisplayBrokerRelations

	reloadCustomerGrid: function() {
		if (this.nowLoadingCustomers)
			return;

		this.nowLoadingCustomers = true;

		if (this.theTable) {
			this.theTable.fnClearTable();
			this.theTable = null;
		} // if

		var oDiv = this.$el.find('.customer-list').empty();
		var oTbl = this.$el.find('.customer-list-template').clone(true, true).removeClass('hide');

		oDiv.append(oTbl);

		var self = this;

		var oXhr = $.getJSON(
			'' + window.gRootPath + 'Underwriter/Brokers/LoadCustomers',
			{ nBrokerID: this.brokerID, }
		);

		oXhr.done(function(oResponse) {
			var theTableOpts = self.initDataTablesOptions(
				'CustomerID,Email,FirstName,LastName,Status,^ApplyDate,$LoanAmount',
				'brk-grid-state-uw-broker-customer-list'
			);

			theTableOpts.aaData = oResponse.aaData;

			// theTableOpts.aaSorting = [ [5, 'desc'] ];

			self.adjustAoColumn(theTableOpts, 'Marketplaces', function(oCol) {
				oCol.asSorting = [];
			});

			self.adjustAoColumn(theTableOpts, ['LoanAmount', 'SetupFee'], function(oCol) {
				var oStdMoneyRender = oCol.mRender;

				oCol.mRender = function(oData, sAction, oFullSource) {
					if (oData > 0)
						return oStdMoneyRender(oData, sAction, oFullSource);

					switch (sAction) {
					case 'display':
						return '';

					case 'filter':
						return '';

					case 'type':
						return 0;

					case 'sort':
						return 0;

					default:
						return 0;
					} // switch
				}; // mRender for LoanAmount
			});

			var oSomeTimeAgo = moment([2012, 7]).utc();

			self.adjustAoColumn(theTableOpts, ['ApplyDate', 'LoanDate'], function(oCol) {
				oCol.mRender = function(oData, sAction, oFullSource) {
					switch (sAction) {
					case 'display':
						return (oSomeTimeAgo.diff(moment(oData)) > 0) ? '' : EzBob.formatDate(oData);

					case 'filter':
						return (oSomeTimeAgo.diff(moment(oData)) > 0) ? '' : oData + ' ' + EzBob.formatDate(oData);

					case 'type':
						return oData;

					case 'sort':
						return oData;

					default:
						return oData;
					} // switch
				}; // mRender
			});

			self.adjustAoColumn(theTableOpts, 'LastInvitationSent', function(oCol) {
				oCol.mRender = function(oData, sAction, oFullSource) {
					switch (sAction) {
					case 'display':
						return (oSomeTimeAgo.diff(moment(oData)) > 0) ? '' : EzBob.formatDateTime(oData);

					case 'filter':
						return (oSomeTimeAgo.diff(moment(oData)) > 0) ? '' : oData + ' ' + EzBob.formatDateTime(oData);

					case 'type':
						return oData;

					case 'sort':
						return oData;

					default:
						return oData;
					} // switch
				}; // mRender
			});

			theTableOpts.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
				if (oData.RefNumber) {
					var sLinkBase = '<a class=profileLink title="Show customer details" href="#profile/' + oData.CustomerID + '">';

					$('.grid-item-CustomerID', oTR).empty().html(EzBob.DataTables.Helper.withScrollbar(
						sLinkBase + oData.CustomerID + '</a>'
					));

					if (oData.hasOwnProperty('Email') && oData.Email) {
						$('.grid-item-Email', oTR).empty().html(EzBob.DataTables.Helper.withScrollbar(
							sLinkBase + oData.Email + '</a>'
						));
					} // if has email

					if (oData.hasOwnProperty('FirstName') && oData.FirstName) {
						$('.grid-item-FirstName', oTR).empty().html(EzBob.DataTables.Helper.withScrollbar(
							sLinkBase + oData.FirstName + '</a>'
						));
					} // if has first name

					if (oData.hasOwnProperty('LastName') && oData.LastName) {
						$('.grid-item-LastName', oTR).empty().html(EzBob.DataTables.Helper.withScrollbar(
							sLinkBase + oData.LastName + '</a>'
						));
					} // if has last name
				} // if has id
			}; // fnRowCallback

			theTableOpts.fnFooterCallback = function(oTR, aryData, nVisibleStart, nVisibleEnd, aryVisual) {
				var nLoanSum = 0;
				var nSetupFeeSum = 0;
				var nCount = 0;

				_.each(aryData, function(oRowData) {
					if (oRowData.LoanAmount) {
						nCount++;
						nLoanSum += oRowData.LoanAmount;
						nSetupFeeSum += oRowData.SetupFee;
					} // if
				});

				$('.grid-item-FirstName', oTR).empty().text('Total');

				if (nCount > 0) {
					$('.grid-item-LoanAmount', oTR).empty().text(EzBob.formatPoundsNoDecimals(nLoanSum));
					$('.grid-item-SetupFee', oTR).empty().text(EzBob.formatPoundsNoDecimals(nSetupFeeSum));
					$('.grid-item-LoanDate', oTR).empty().text(EzBob.formatIntWithCommas(nCount) + ' loan' + (nCount === 1 ? '' : 's'));
				} // if
			}; // fnFooterCallback

			self.theTable = oTbl.dataTable(theTableOpts);

			self.nowLoadingCustomers = false;
		});
	}, // reloadCustomerGrid

	displayBrokerProperties: function() {
		if (this.nowLoadingBrokerProperties)
			return;

		this.nowLoadingBrokerProperties = true;

		var self = this;

		var oXhr = $.getJSON(
			'' + window.gRootPath + 'Underwriter/Brokers/LoadProperties',
			{ nBrokerID: this.brokerID, }
		);

		oXhr.done(function(oResponse) {
			if (this.brokerID !== oResponse.brokerID) {
				self.nowLoadingBrokerProperties = false;
				return;
			} // if

			self.$el.find('#broker-profile-summary .value').load_display_value({
				data_source: oResponse,

				callback: function(sFieldName, oFieldValue) {
					switch (sFieldName) {
					case 'BrokerWebSiteUrl':
						return '<a target=_blank href="' + oFieldValue + '">' + oFieldValue + '</a>';
					case 'ContactEmail':
						return '<a href="mailto:' + oFieldValue + '">' + oFieldValue + '</a>';
					} // switch

					return oFieldValue;
				}, // callback
			});

			self.nowLoadingBrokerProperties = false;
		});
	}, // displayBrokerProperties

	show: function(id, type) {
		this.brokerID = id;
		
		this.whiteLabelModel = new EzBob.Underwriter.BrokerWhiteLabelModel({ brokerID: this.brokerID });
		
		this.$el.show();
		
		this.handleTabSwitch(type);

		EzBob.handleUserLayoutSetting();

		this.redrawCurrentTab(type);
	}, // show

	handleTabSwitch: function(sTabID) {
		var oTab = this.myTabHeaders(sTabID, true);
		var sSection = oTab.attr('href').substr(1);

		this.router.navigate('#broker/' + this.brokerID + '/' + sSection);

		if (!oTab.hasClass('active'))
			oTab.tab('show');
	}, // handelTabSwitch

	redrawCurrentTab: function(sSection) {
		if (!sSection) {
			var oTab = this.myTabHeaders().parent('.active').find('a').first();
			sSection = oTab.attr('href').substr(1);
		} // if

		switch (sSection) {
		case 'broker-customers-grid':
			this.reloadCustomerGrid();
			break;
		case 'broker-profile-summary':
			this.displayBrokerProperties();
			break;
		case 'broker-relations':
			this.startDisplayBrokerRelations();
			break;
		case 'broker-whitelabel':
			this.whiteLabel();
			break;
		} // switch
	}, // redrawCurrentTab

	myTabHeaders: function(sTabID, bReturnFirstIfNotFound) {
		var oAll = this.$el.find('a[data-toggle="tab"]');

		if (!sTabID)
			return bReturnFirstIfNotFound ? oAll.first() : oAll;

		var oFiltered = null;
		var bFound = false;

		try {
			oFiltered = oAll.filter('[href="#' + sTabID + '"]');
			bFound = oFiltered.length;
		}
		catch (e) {
			console.error('Error parsing tab header:', e);
		} // try

		if (bFound)
			return oFiltered;

		return bReturnFirstIfNotFound ? oAll.first() : null;
	}, // myTabHeaders

	hide: function() {
		this.$el.hide();
	}, // hide

	initDataTablesOptions: function(sColumns, sGridKey) {
		return {
			bDestroy: true,
			bProcessing: true,
			aoColumns: EzBob.DataTables.Helper.extractColumns(sColumns),

			aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
			iDisplayLength: 10,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			bAutoWidth: true,
			sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"f><"dataTables_top_left"i>>>>tr<"bottom"<"col-md-6"l><"col-md-6 dataTables_bottom_right"p>><"clear">',

			bStateSave: true,

			fnStateSave: function(oSettings, oData) {
				localStorage.setItem(sGridKey, JSON.stringify(oData));
			}, // fnStateSave

			fnStateLoad: function(oSettings) {
				var sData = localStorage.getItem(sGridKey);
				var oData = sData ? JSON.parse(sData) : null;
				return oData;
			}, // fnStateLoad
		};
	}, // initDataTablesOptions

	adjustAoColumn: function(oTableOpts, oColumnName, oAdjustFunc) {
		var aryNames = {};

		if (oColumnName === undefined)
			return;

		if (oColumnName === null)
			return;

		if ('boolean' === typeof oColumnName)
			return;

		if ('object' === typeof oColumnName)
			_.each(oColumnName, function(sName) { aryNames[sName] = 1; });
		else if (('string' === typeof oColumnName) || ('number' === typeof oColumnName))
			aryNames[oColumnName] = 1;

		_.each(oTableOpts.aoColumns, function(oCol) {
			if (aryNames[oCol.mData])
				oAdjustFunc(oCol);
		});
	}, // adjustAoColumn
	
	whiteLabel: function () {
		var self = this;
		self.BrokerWhiteLabelView = new EzBob.Underwriter.BrokerWhiteLabelView({ model: self.whiteLabelModel, el: self.$el.find('.broker-whitelabel-root-el'), brokerId: self.brokerID });
	}
	
}); // EzBob.Underwriter.BrokerProfileView
