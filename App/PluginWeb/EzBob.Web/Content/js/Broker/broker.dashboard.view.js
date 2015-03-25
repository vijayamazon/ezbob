var EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.DashboardView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.DashboardView.__super__.initialize.apply(this, arguments);

		this.theTable = null;
		this.leadTable = null;

		this.$el = $('.section-dashboard');

		this.router.on('broker-properties-updated', this.displayBrokerProperties, this);

		this.instantOfferView = new EzBob.Broker.InstantOfferView({ router: this.router });
		this.changePasswordView = new EzBob.Broker.ChangePasswordView({router: this.router});
	}, // initialize

	events: function() {
		var evt = EzBob.Broker.DashboardView.__super__.events.apply(this, arguments);

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

	render: function () {
		$('body').addClass('broker-dashboard');
		this.$el.tabs();

		this.reloadCustomerList();

		this.displayBrokerProperties();

		this.displaySignedTerms();

		this.instantOfferView.render();
	}, // onRender

	displaySignedTerms: function() {
		var self = this;

		$.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadSignedTerms',
			{ sContactEmail: this.router.getAuth(), }
		).done(function(res) {
			if (res.success) {
				self.$el.find('.terms-and-conditions').html(res.terms);
				self.$el.find('.signed-time').text(res.signedTime);
			}
			else {
				self.$el.find('.terms-and-conditions').html('Failed to load terms and conditions.');
				self.$el.find('.signed-time').html('&mdash;');
			} // if
		});
	}, // displaySignedTerms

	displayBrokerProperties: function() {
		var oProps = this.router.getBrokerProperties();

		if (!this.router.isMyBroker(oProps)) // e.g. not yet loaded
			return;

		var oSampleLink = function(sSourceRef, sImagePath, sNewLine, nWidth, nHeight) {
			return '<a target=_blank href="http://www.ezbob.com?sourceref=' + sSourceRef + '" rel="nofollow">' + sNewLine +
				'\t<img src="' + sImagePath + '" ' +
				'width=' + nWidth + ' height=' + nHeight +
				' alt="business loans">' + sNewLine +
				'</a>';
		};

		this.$el.find('#section-dashboard-account-info .value, #section-dashboard-marketing .value').load_display_value({
			data_source: oProps,

			realFieldName: function(sFieldName) {
				if (sFieldName.indexOf('SourceRef') === 0)
					return 'SourceRef';

				return sFieldName;
			}, // realFieldName

			callback: function(sFieldName, oFieldValue) {
				switch (sFieldName) {
				case 'BrokerWebSiteUrl':
					return '<a target=_blank href="' + oFieldValue + '">' + oFieldValue + '</a>';

				case 'ContactEmail':
					return '<a href="mailto:' + oFieldValue + '">' + oFieldValue + '</a>';

				case 'SourceRef':
					return '<a target=_blank href="http://www.ezbob.com?sourceref=' + oFieldValue + '" rel="nofollow">http://www.ezbob.com?sourceref=' + oFieldValue + '</a>';
				} // switch

				if (sFieldName.indexOf('SourceRef') !== 0)
					return oFieldValue;

				var aryMatch = /^SourceRefTo(.{4})_(\d+)x(\d+)$/.exec(sFieldName);

				return oSampleLink(oFieldValue,
					(aryMatch[1] === 'Text' ? "http://www.ezbob.com/wp-content/themes/ezbob-new/images/new-ezbob-logo.png" : "/Content/img/ezbob-logo-2014.png"),
					(aryMatch[1] === 'Text' ? '\n' : ''),
					parseInt(aryMatch[2], 10),
					parseInt(aryMatch[3], 10));
			}, // callback

			fieldNameSetText: function(sFieldName) {
				return sFieldName.indexOf('SourceRefToText') === 0;
			}, // fieldNameSetText
		});
	}, // displayBrokerProperties

	reloadCustomerList: function() {
		this.clear();

		var self = this;

		var oXhr = $.getJSON(
			'' + window.gRootPath + 'Broker/BrokerHome/LoadCustomers',
			{ sContactEmail: this.router.getAuth(), }
		);

		oXhr.done(function(oResponse) {
			if (!oResponse.is_auth) {
				self.router.logoff();
				return;
			} // if

			var theTableOpts = self.initDataTablesOptions(
				'FirstName,LastName,Status,^ApplyDate,$LoanAmount',
				'brk-grid-state-' + self.router.getAuth() + '-customer-list',
				2 // increase this version number every time table structure is changed.
			);

			theTableOpts.aaData = oResponse.customers;

			theTableOpts.aaSorting = [[5, 'desc']];

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

			theTableOpts.aoColumns.push({
				mData: null,
				sClass: 'center',
				asSorting: [],
				mRender: function(oData, sAction, oFullSource) {
					if ((oFullSource.LeadID <= 0) || oFullSource.IsLeadDeleted || (oFullSource.WizardStep === 'Wizard complete'))
						return '';

					if (sAction === 'display')
						return '<button class="lead-fill-wizard button btn-green" data-lead-id=' + oFullSource.LeadID + ' title="Complete the application on behalf of the client.">' +
							'<i class="fa fa-desktop"></i> Fill' +
							'</button>';

					return '';
				}, // mRender
			});

			theTableOpts.aoColumns.push({
				mData: null,
				sClass: 'center',
				asSorting: [],
				mRender: function(oData, sAction, oFullSource) {
					if (sAction !== 'display')
						return '';

					if ((oFullSource.LeadID <= 0) || oFullSource.IsLeadDeleted || (oFullSource.WizardStep === 'Wizard complete'))
						return '';

					var sTitle = '';

					if (moment([1976, 7]).utc().diff(moment(oFullSource.DateLastInvitationSent)) > 0)
						sTitle = 'Send an invitation to the client to fill himself.';
					else
						sTitle = 'Send another invitation to the client to fill himself.';

					return '<button class="lead-send-invitation button btn-green" data-lead-id=' + oFullSource.LeadID + ' title="' + sTitle + '">' +
						'<i class="fa fa-envelope"></i> Send' +
						'</button>';
				}, // mRender
			});

			theTableOpts.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
				//if (oData.hasOwnProperty('Marketplaces'))
				//	$('.grid-item-Marketplaces', oTR).empty().html(EzBob.DataTables.Helper.showMPsIcon(oData.Marketplaces));

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

				$('.grid-item-LoanAmount', oTR).empty().text(EzBob.formatPoundsNoDecimals(nLoanSum));
				$('.grid-item-SetupFee', oTR).empty().text(EzBob.formatPoundsNoDecimals(nSetupFeeSum));
				$('.grid-item-LoanDate', oTR).empty().text(EzBob.formatIntWithCommas(nCount) + ' loan' + (nCount === 1 ? '' : 's'));
			}; // fnFooterCallback

			self.theTable = self.$el.find('.customer-list').dataTable(theTableOpts);

			self.$el.find('.dataTables_top_right').prepend(
				$('<button type=button class="button btn-green reload-customer-list" title="Reload customer and lead lists">Reload</button>')
			);
		}); // on success
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

	initDataTablesOptions: function(sColumns, sGridKey, tableStuctVersionNo) {
		var versionNoKey = sGridKey + '-struct-version';

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
				localStorage.setItem(versionNoKey, tableStuctVersionNo);
				localStorage.setItem(sGridKey, JSON.stringify(oData));
			}, // fnStateSave

			fnStateLoad: function(oSettings) {
				var storedVersionNo = parseInt(localStorage.getItem(versionNoKey) || 0, 10);

				if (storedVersionNo < tableStuctVersionNo)
					localStorage.removeItem(sGridKey);

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

}); // EzBob.Broker.DashboardView
