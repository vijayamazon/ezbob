EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.DashboardView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.DashboardView.__super__.initialize.apply(this, arguments);

		this.theTable = null;
		this.leadTable = null;

		this.$el = $('.section-dashboard');
	}, // initialize

	events: function() {
		var evt = {};

		evt['click #AddNewCustomer'] = 'addNewCustomer';
		evt['click .reload-customer-list'] = 'reloadCustomerList';

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

		this.reloadCustomerList();

		return this;
	}, // render

	reloadCustomerList: function() {
		this.clear();

		var self = this;

		$.getJSON(
			'' + window.gRootPath + 'Broker/BrokerHome/LoadCustomers',
			{ sContactEmail: this.router.getAuth(), },
			function(oResponse) {
				var theTableOpts = self.initDataTablesOptions(
					'#CustomerID,FirstName,LastName,Email,WizardStep,Status,Marketplaces,^ApplyDate,$LoanAmount,^LoanDate',
					'brk-grid-state-' + self.router.getAuth() + '-customer-list'
				);

				theTableOpts.aaData = oResponse.customers;

				theTableOpts.fnRowCallback = function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
					if (oData.hasOwnProperty('Marketplaces'))
						$('.grid-item-Marketplaces', oTR).empty().html(EzBob.DataTables.Helper.showMPsIcon(oData.Marketplaces));

					if (oData.hasOwnProperty('CustomerID')) {
						var sLinkBase = '<a class=profileLink title="Show customer details" href="#customer/' + oData.CustomerID + '">';

						$('.grid-item-CustomerID', oTR).empty().html(EzBob.DataTables.Helper.withScrollbar(
							sLinkBase + oData.CustomerID + '</a>'
						));

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

						$(oTR).dblclick(function() {
							location.assign($(oTR).find('.profileLink').first().attr('href'));
						});
					} // if has id
				}; // fnRowCallback

				self.theTable = self.$el.find('.customer-list').dataTable(theTableOpts);

				self.$el.find('.dataTables_top_right').prepend(
					$('<button type=button class="reload-customer-list" title="Reload customer and lead lists">Reload</button>')
				);

				var leadTableOpts = self.initDataTablesOptions(
					'#LeadID,FirstName,LastName,Email,AddMode,^DateCreated,^DateLastInvitationSent',
					'brk-grid-state-' + self.router.getAuth() + '-lead-list'
				);

				leadTableOpts.aaData = oResponse.leads;

				self.leadTable = self.$el.find('.lead-list').dataTable(leadTableOpts);
			} // on success
		); // get json

	}, // reloadCustomerList

	addNewCustomer: function() {
		location.assign('#add');
	}, // addNewCustomer

	initDataTablesOptions: function(sColumns, sGridKey) {
		return {
			bDestroy: true,
			bProcessing: true,
			aoColumns: EzBob.DataTables.Helper.extractColumns(sColumns),

			bDeferRender: true,

			aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
			iDisplayLength: 10,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			aaSorting: [[0, 'desc']],

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
}); // EzBob.Broker.SubmitView
