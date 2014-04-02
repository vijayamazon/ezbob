EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.DashboardView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.DashboardView.__super__.initialize.apply(this, arguments);

		this.theTable = null;
		this.leadTable = null;

		this.$el = $('.section-dashboard');

		this.router.on('broker-properties-updated', this.displayBrokerProperties, this);
	}, // initialize

	events: function() {
		var evt = {};

		evt['click #AddNewCustomer'] = 'addNewCustomer';
		evt['click .reload-customer-list'] = 'reloadCustomerList';
		evt['click .lead-send-invitation'] = 'sendInvitation';
		evt['click .lead-fill-wizard'] = 'fillWizard';

		return evt;
	}, // events

	clear: function() {
		EzBob.Broker.DashboardView.__super__.clear.apply(this, arguments);

		if (this.theTable) {
			this.theTable.fnClearTable();
			this.theTable = null;
		} // if

		if (this.leadTable) {
			this.leadTable.fnClearTable();
			this.leadTable = null;
		} // if
	}, // clear

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

		this.$el.tabs();

		this.reloadCustomerList();

		this.displayBrokerProperties();

		return this;
	}, // render

	displayBrokerProperties: function() {
		var oProps = this.router.getBrokerProperties();

		if (!this.router.isMyBroker(oProps)) // e.g. not yet loaded
			return;

		var oSampleLink = function(sSourceRef, sNL) {
			return '<a target=_blank href="http://www.ezbob.com?sourceref=' + sSourceRef + '">' + sNL +
				'\t<img src="http://www.ezbob.com/wp-content/themes/ezbob-new/images/new-ezbob-logo.png" alt="business loans">' + sNL +
				'</a>';
		};

		this.$el.find('#section-dashboard-marketing .value').load_display_value({
			data_source: oProps,

			realFieldName: function(sFieldName) {
				switch (sFieldName) {
				case 'SourceRefToLink':
				case 'SourceRefToText':
					return 'SourceRef';
				} // switch

				return sFieldName;
			}, // realFieldName

			callback: function(sFieldName, oFieldValue) {
				switch (sFieldName) {
				case 'BrokerWebSiteUrl':
					return '<a target=_blank href="' + oFieldValue + '">' + oFieldValue + '</a>';

				case 'ContactEmail':
					return '<a href="mailto:' + oFieldValue + '">' + oFieldValue + '</a>';

				case 'SourceRef':
					return '<a target=_blank href="http://www.ezbob.com?sourceref=' + oFieldValue + '">http://www.ezbob.com?sourceref=' + oFieldValue + '</a>';

				case 'SourceRefToLink':
					return oSampleLink(oFieldValue, '');

				case 'SourceRefToText':
					return oSampleLink(oFieldValue, '\n');

				default:
					return oFieldValue;
				} // switch
			}, // callback

			fieldNameSetText: function(sFieldName) {
				return sFieldName === 'SourceRefToText';
			}, // fieldNameSetText
		});
	}, // displayBrokerProperties

	reloadCustomerList: function() {
		this.clear();

		var self = this;

		$.getJSON(
			'' + window.gRootPath + 'Broker/BrokerHome/LoadCustomers',
			{ sContactEmail: this.router.getAuth(), },
			function(oResponse) {
				var theTableOpts = self.initDataTablesOptions(
					'FirstName,LastName,WizardStep,Status,Marketplaces,^ApplyDate,$LoanAmount,$SetupFee,^LoanDate,@LastInvitationSent',
					'brk-grid-state-' + self.router.getAuth() + '-customer-list'
				);

				theTableOpts.aaData = oResponse.customers;

				theTableOpts.aaSorting = [ [5, 'desc'] ];

				self.adjustAoColumn(theTableOpts, [ 'LoanAmount', 'SetupFee' ], function(oCol) {
					var oStdMoneyRender = oCol.mRender;

					oCol.mRender = function(oData, sAction, oFullSource) {
						if (oData > 0)
							return oStdMoneyRender(oData, sAction, oFullSource);

						switch (sAction) {
							case 'display':
							case 'filter':
								return '';

							case 'type':
							case 'sort':
							default:
								return 0;
						} // switch
					}; // mRender for LoanAmount
				});

				var oSomeTimeAgo = moment([2012, 7]).utc();

				self.adjustAoColumn(theTableOpts, [ 'ApplyDate', 'LoanDate' ], function(oCol) {
					oCol.mRender = function(oData, sAction, oFullSource) {
						switch (sAction) {
							case 'display':
								return (oSomeTimeAgo.diff(moment(oData)) > 0) ? '' : EzBob.formatDate(oData);

							case 'filter':
								return (oSomeTimeAgo.diff(moment(oData)) > 0) ? '' : oData + ' ' + EzBob.formatDate(oData);

							case 'type':
							case 'sort':
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
							case 'sort':
							default:
								return oData;
						} // switch
					}; // mRender
				});

				theTableOpts.aoColumns.push({
					mData: null,
					sClass: 'center',
					mRender: function(oData, sAction, oFullSource) {
						if (sAction !== 'display')
							return '';

						if ((oFullSource.LeadID <= 0) || oFullSource.IsLeadDeleted || (oFullSource.WizardStep === 'Wizard complete'))
							return '';

						var sTitle = '';

						if (moment([1976, 7]).utc().diff(moment(oFullSource.DateLastInvitationSent)) > 0)
							sTitle = 'Send invitation to this lead.';
						else
							sTitle = 'Send another invitation to this lead.';

						return '<button class=lead-send-invitation data-lead-id=' + oFullSource.LeadID + ' title="' + sTitle + '">' +
							'<i class="fa fa-envelope-o"></i>' +
							'</button>';
					}, // mRender
				});

				theTableOpts.aoColumns.push({
					mData: null,
					sClass: 'center',
					mRender: function(oData, sAction, oFullSource) {
						if ((oFullSource.LeadID <= 0) || oFullSource.IsLeadDeleted || (oFullSource.WizardStep === 'Wizard complete'))
							return '';

						if (sAction === 'display')
							return '<button class=lead-fill-wizard data-lead-id=' + oFullSource.LeadID + ' title="Fill all the data for this lead.">' +
								'<i class="fa fa-desktop"></i>' +
								'</button>';

						return '';
					}, // mRender
				});

				theTableOpts.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					if (oData.hasOwnProperty('Marketplaces'))
						$('.grid-item-Marketplaces', oTR).empty().html(EzBob.DataTables.Helper.showMPsIcon(oData.Marketplaces));

					if (oData.RefNumber) {
						var sLinkBase = '<a class=profileLink title="Show customer details" href="#customer/' + oData.RefNumber + '">';

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
					// console.log('footer callback', aryData, nVisibleStart, nVisibleEnd, aryVisual);
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

				self.theTable = self.$el.find('.customer-list').dataTable(theTableOpts);

				self.$el.find('.dataTables_top_right').prepend(
					$('<button type=button class="reload-customer-list" title="Reload customer and lead lists">Reload</button>')
				);
			} // on success
		); // get json
	}, // reloadCustomerList

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

	addNewCustomer: function() {
		EzBob.App.trigger('clear');
		location.assign('#add');
	}, // addNewCustomer

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

	sendInvitation: function(event) {
		var nLeadID = parseInt($(event.currentTarget).attr('data-lead-id'), 10);

		if (nLeadID < 1)
			return;

		BlockUi();

		var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/SendInvitation', {
			nLeadID: nLeadID,
			sContactEmail: this.router.getAuth(),
		});

		var self = this;

		oRequest.success(function(res) {
			UnBlockUi();

			if (res.success) {
				EzBob.App.trigger('info', 'An invitation has been sent.');
				self.reloadCustomerList();
				return;
			} // if

			if (res.error)
				EzBob.App.trigger('error', res.error);
			else
				EzBob.App.trigger('error', 'Failed to send an invitation.');
		}); // on success

		oRequest.fail(function() {
			UnBlockUi();
			EzBob.App.trigger('error', 'Failed to send an invitation.');
		});
	}, // sendInvitation

	fillWizard: function(event) {
		var nLeadID = parseInt($(event.currentTarget).attr('data-lead-id'), 10);

		if (nLeadID < 1)
			return;

		location.assign(
			'' + window.gRootPath + 'Broker/BrokerHome/FillWizard' +
			'?nLeadID=' + nLeadID +
			'&sContactEmail=' + encodeURIComponent(this.router.getAuth())
		);
	}, // fillWizard
}); // EzBob.Broker.SubmitView
