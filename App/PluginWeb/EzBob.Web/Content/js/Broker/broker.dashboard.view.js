var EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.DashboardView = EzBob.Broker.BaseView.extend({
    initialize: function () {
        EzBob.Broker.DashboardView.__super__.initialize.apply(this, arguments);

        this.theTable = null;
        this.leadTable = null;
        this.firstMenuClick = 0;
        this.$el = $('.section-dashboard');

        this.router.on('broker-properties-updated', this.displayBrokerProperties, this);

        this.instantOfferView = new EzBob.Broker.InstantOfferView({ router: this.router });
        this.changePasswordView = new EzBob.Broker.ChangePasswordView({ router: this.router });
    }, // initialize

    events: function () {
        var evt = EzBob.Broker.DashboardView.__super__.events.apply(this, arguments);

        evt['click #AddNewCustomer'] = 'addNewCustomer';
        evt['click .add-bank'] = 'addBankDetails';
        evt['click .reload-customer-list'] = 'reloadCustomerList';
        evt['click .broker-hamburger'] = 'openCloseMenu';


        return evt;
    }, // events

    clear: function () {
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
        var self = this;
        this.$el.tabs({
            show: function (ev, ui) {
                if ($(window).width() < 801) {
                    if (self.firstMenuClick === 0) {
                        self.firstMenuClick = 1;
                    } else {
                        $(".profile-menu-ul-wrap").slideToggle("slow", function () {
                            var id = ev.currentTarget;
                            var tabText = ui.tab.text;
                            $('.profile-menu-title').html(tabText);

                        });
                    }

                }


            }
        });

        this.reloadCustomerList();

        this.displayBrokerProperties();

        this.displaySignedTerms();

        this.instantOfferView.render();
    }, // onRender

    openCloseMenu: function () {

        $('body').scrollTo('.broker-hamburger', { duration: 'slow', offset: -20 });
        $(".profile-menu-ul-wrap").slideToggle("slow", function () {

        });

    },//openclosemenu
    displaySignedTerms: function () {
        var self = this;

        $.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadSignedTerms',
			{ sContactEmail: this.router.getAuth(), }
		).done(function (res) {
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

    displayBrokerProperties: function () {
        var oProps = this.router.getBrokerProperties();

        if (!this.router.isMyBroker(oProps)) // e.g. not yet loaded
            return;

        var oSampleLink = function (sSourceRef, nWidth, nHeight) {
            var imagePath = oProps.Origin === 'ezbob'
				? 'https://www.ezbob.com/app/uploads/brokers/logos/ezbob-logo-200x83px.png'
				: 'https://www.everline.com/app/uploads/brokers/logos/Everline-primary-logo-200x83px.png';

            return '<a target=_blank href="' + oProps.FrontendSite + '?sourceref=' + sSourceRef + '" rel="nofollow">' +
				'\t<img src="' + imagePath + '" ' +
				'width=' + nWidth + ' height=' + nHeight +
				' alt="business loans">' +
				'</a>';
        };

        this.commissionView = new EzBob.Broker.CommissionView({ properties: oProps });
        this.commissionView.render();

        this.$el.find('#section-dashboard-account-info .value, #section-dashboard-marketing .value').load_display_value({
            data_source: oProps,

            realFieldName: function (sFieldName) {
                if (sFieldName.indexOf('SourceRef') === 0)
                    return 'SourceRef';

                return sFieldName;
            }, // realFieldName

            callback: function (sFieldName, oFieldValue) {
                switch (sFieldName) {
                    case 'BrokerWebSiteUrl':
                        return '<a target=_blank href="' + oFieldValue + '">' + oFieldValue + '</a>';

                    case 'ContactEmail':
                        return '<a href="mailto:' + oFieldValue + '">' + oFieldValue + '</a>';

                    case 'SourceRef':
                        return '<a target=_blank href="' + oProps.FrontendSite + '?sourceref=' + oFieldValue + '" rel="nofollow">' + oProps.FrontendSite + '?sourceref=' + oFieldValue + '</a>';
                } // switch

                if (sFieldName.indexOf('SourceRef') !== 0)
                    return oFieldValue;

                var aryMatch = /^SourceRefTo(.{4})_(\d+)x(\d+)$/.exec(sFieldName);

                return oSampleLink(oFieldValue, parseInt(aryMatch[2], 10), parseInt(aryMatch[3], 10));
            }, // callback

            fieldNameSetText: function (sFieldName) {
                return sFieldName.indexOf('SourceRefToText') === 0;
            }, // fieldNameSetText
        });
    }, // displayBrokerProperties

    reloadCustomerList: function () {
        this.clear();

        var self = this;

        var oXhr = $.getJSON(
			'' + window.gRootPath + 'Broker/BrokerHome/LoadCustomers',
			{ sContactEmail: this.router.getAuth(), }
		);

        oXhr.done(function (oResponse) {
            if (!oResponse.is_auth) {
                self.router.logoff();
                return;
            } // if

            var theTableOpts = self.initDataTablesOptions(
				'FirstName,LastName,Status,^ApplyDate,$ApprovedAmount,$LoanAmount,$CommissionAmount,^CommissionPaymentDate',
				'brk-grid-state-' + self.router.getAuth() + '-customer-list',
				3 // increase this version number every time table structure is changed.
			);

            theTableOpts.aaData = oResponse.customers;

            //theTableOpts.aaSorting = [[8, 'desc']];
            //theTableOpts.bSort = false;

            self.adjustAoColumn(theTableOpts, ['LoanAmount', 'ApprovedAmount', 'CommissionAmount'], function (oCol) {
                var oStdMoneyRender = oCol.mRender;

                oCol.mRender = function (oData, sAction, oFullSource) {
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

            self.adjustAoColumn(theTableOpts, 'Status', function (oCol) {
                // ocol.mrender will run through all the ocol.mData(for example = 'status') coloums.
                // in adjust Ao column function(ocol) we can assign the mRender function to all the cols that need the same functionality  for example 3 cols of dates
                oCol.mRender = function (oData, sAction, oFullSource) {
                    switch (sAction) {
                        case 'display':
                            if (oData === 'ApprovedPending') {
                                if (oFullSource.IsWaitingForSignature === true) {
                                    return 'Waiting for signature';
                                }
                                else {
                                    return 'Pending information';
                                }
                            }
                            return oData;

                        case 'filter':
                            return oData;

                        case 'type':
                            return oData;

                        case 'sort':
                            return oData;

                        default:
                            return oData;
                    } // switch
                }; // mRender
            });
            var oSomeTimeAgo = moment([2012, 7]).utc();

            self.adjustAoColumn(theTableOpts, ['ApplyDate', 'LoanDate', 'CommissionPaymentDate'], function (oCol) {

                oCol.mRender = function (oData, sAction, oFullSource) {
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

            self.adjustAoColumn(theTableOpts, 'LastInvitationSent', function (oCol) {
                oCol.mRender = function (oData, sAction, oFullSource) {
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

            theTableOpts.fnRowCallback = function (oTR, oData, iDisplayIndex, iDisplayIndexFull) {
                //if (oData.hasOwnProperty('Marketplaces'))
                //	$('.grid-item-Marketplaces', oTR).empty().html(EzBob.DataTables.Helper.showMPsIcon(oData.Marketplaces));
                var sLinkBase;

                if (oData.RefNumber) {
                    sLinkBase = '<a class=profileLink title="Show customer details" href="#customer/' + oData.RefNumber + '">';

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
                } else {
                    sLinkBase = '<a class=profileLink title="Show customer details" href="#lead/' + oData.LeadID + '">';
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

            theTableOpts.fnFooterCallback = function (oTR, aryData, nVisibleStart, nVisibleEnd, aryVisual) {
                // console.log('footer callback', aryData, nVisibleStart, nVisibleEnd, aryVisual);
                var nLoanSum = 0;
                var nCommissionAmountSum = 0;
                var nCount = 0;
                var nApprovedAmountSum = 0;
                _.each(aryData, function (oRowData) {
                    if (oRowData.LoanAmount) {
                        nCount++;
                        nLoanSum += oRowData.LoanAmount;
                        nCommissionAmountSum += oRowData.CommissionAmount;
                        nApprovedAmountSum += oRowData.ApprovedAmount;
                    } // if
                });

                $('.grid-item-FirstName', oTR).empty().text('Total');
                $('.grid-item-ApprovedAmount', oTR).empty().text(EzBob.formatPoundsNoDecimals(nApprovedAmountSum));
                $('.grid-item-LoanAmount', oTR).empty().text(EzBob.formatPoundsNoDecimals(nLoanSum));
                $('.grid-item-CommissionAmount', oTR).empty().text(EzBob.formatPoundsNoDecimals(nCommissionAmountSum));
                $('.grid-item-ApplyDate', oTR).empty().text(EzBob.formatIntWithCommas(nCount) + ' loan' + (nCount === 1 ? '' : 's'));
            }; // fnFooterCallback

            self.theTable = self.$el.find('.customer-list').dataTable(theTableOpts);
            self.$el.find('.customer-list').wrap('<div class="dashboard-table-wrap"><div class="dashboard-table-scorllable"></div></div>');
            self.$el.find('.dataTables_top_right').prepend(
				$('<button type=button class="button btn-green reload-customer-list clean-btn" title="Reload customer and lead lists">Reload</button>')
			);
            if (EzBob.Config.Origin !== 'everline') {
                self.$el.find('#section-dashboard-customers').prepend(
                  $('<button type="button" id="AddNewCustomer" class="button btn-green pull-right btn-wide ev-btn-org">Add a new client</button>')
              );
            } else {
                self.$el.find('.dataTables_top_right').prepend(
                    $('<button type="button" id="AddNewCustomer" class="button btn-green pull-right btn-wide ev-btn-org">Add a new client</button>')
                );
            }
          
        }); // on success
    }, // reloadCustomerList

    adjustAoColumn: function (oTableOpts, oColumnName, oAdjustFunc) {
        var aryNames = {};

        if (oColumnName === undefined)
            return;

        if (oColumnName === null)
            return;

        if ('boolean' === typeof oColumnName)
            return;

        if ('object' === typeof oColumnName)
            _.each(oColumnName, function (sName) { aryNames[sName] = 1; });
        else if (('string' === typeof oColumnName) || ('number' === typeof oColumnName))
            aryNames[oColumnName] = 1;

        _.each(oTableOpts.aoColumns, function (oCol) {
            if (aryNames[oCol.mData]) //checks if column is in oColumn name
                oAdjustFunc(oCol);
        });
    }, // adjustAoColumn

    addNewCustomer: function () {
        EzBob.App.trigger('clear');
        location.assign('#add');
    }, // addNewCustomer

    addBankDetails: function () {
        EzBob.App.trigger('clear');
        location.assign('#bank');
    }, // addBankDetails

    initDataTablesOptions: function (sColumns, sGridKey, tableStuctVersionNo) {
        var versionNoKey = sGridKey + '-struct-version';
        var tableConf = {
            bDestroy: true,
            bProcessing: true,
            aoColumns: EzBob.DataTables.Helper.extractColumns(sColumns),
            bJQueryUI: false,
            bAutoWidth: true,
            bStateSave: true,
            fnStateSave: function (oSettings, oData) {
                localStorage.setItem(versionNoKey, tableStuctVersionNo);
                localStorage.setItem(sGridKey, JSON.stringify(oData));
            }, // fnStateSave

            fnStateLoad: function (oSettings) {
                var storedVersionNo = parseInt(localStorage.getItem(versionNoKey) || 0, 10);

                if (storedVersionNo < tableStuctVersionNo)
                    localStorage.removeItem(sGridKey);

                var sData = localStorage.getItem(sGridKey);
                var oData = sData ? JSON.parse(sData) : null;
                return oData;
            } // fnStateLoad
        }
        if (EzBob.Config.Origin !== 'everline') {
            tableConf.aLengthMenu = [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]];
            tableConf.iDisplayLength = 10;
            tableConf.sPaginationType = 'bootstrap';
            tableConf.sDom = '<"top"<"box"<"box-title"<"dataTables_top_right"f><"dataTables_top_left"i>>>>tr<"bottom"<"col-md-6"l><"col-md-6 dataTables_bottom_right"p>><"clear">';

        } else {
            tableConf.aLengthMenu = [[-1, 5, 10, 50, 100], ['all', 5, 10, 50, 100]];
            tableConf.iDisplayLength = 5;
            tableConf.oLanguage = {
                "sSearch": "Search clients",
                "oPaginate": {
                    "sNext": "...",
                    "sPrevious": "...",
                    "sFirst": "",
                    "sLast": "..."
                }
            };
            tableConf.sPaginationType = 'full_numbers';
            tableConf.sDom = '<"top top-client-list"<"box"<"box-title clearfix"<"dataTables_top_right clearfix"f><"dataTables_top_left">>>>tr<"bottom"<"col-md-6 dataTables_bottom_right"p><"table-info"i>><"clear">';
        }
        return tableConf;

    }, // initDataTablesOptions
}); // EzBob.Broker.DashboardView
