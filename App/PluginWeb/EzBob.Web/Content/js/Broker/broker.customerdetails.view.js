EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.CustomerDetailsView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.CustomerDetailsView.__super__.initialize.apply(this, arguments);

		this.CustomerID = this.options.customerid;
		this.CrmTable = null;

		this.$el = $('.section-customer-details');
	}, // initialize

	events: function() {
		var evt = {};

		evt['click .back-to-list'] = 'backToList';

		return evt;
	}, // events

	clear: function() {
		EzBob.Broker.CustomerDetailsView.__super__.clear.apply(this, arguments);

		this.$el.find('.customer-personal-details .value').load_display_value({ data_source: {}, });

		if (this.CrmTable) {
			this.CrmTable.fnClearTable();
			this.CrmTable = null;
		} // if
	}, // clear

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

		EzBob.App.trigger('clear');

		this.reloadData();

		return this;
	}, // render

	reloadData: function() {
		this.clear();

		var self = this;

		$.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadCustomerDetails',
			{ nCustomerID: this.CustomerID, sContactEmail: this.router.getAuth(), },
			function(oResponse) {
				if (!oResponse.success) {
					if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to load customer details.');

					return;
				} // if

				self.$el.find('.customer-personal-details .value').load_display_value({
					data_source: oResponse.personal_data,
					callback: function(sFieldName, oFieldValue) {
						switch (sFieldName) {
						case 'birthdate':
							return EzBob.formatDate(oFieldValue);

						case 'address':
							return oFieldValue.replace(/\n+/g, '<br />');

						default:
							return oFieldValue;
						} // switch
					} // callback
				});

				var sGridKey = 'brk-grid-state-' + self.router.getAuth() + '-customer-crm';

				self.CrmTable = self.$el.find('.customer-crm-list').dataTable({
					bDestroy: true,
					bProcessing: true,
					aaData: oResponse.crm_data,
					aoColumns: EzBob.DataTables.Helper.extractColumns('@CrDate,ActionName,StatusName,Comment'),

					bDeferRender: true,

					aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
					iDisplayLength: 10,

					sPaginationType: 'bootstrap',
					bJQueryUI: false,

					aaSorting: [[ 0, 'desc' ]],

					bAutoWidth: true,
					sDom: '<"top"<"box"<"box-title"<"dataTables_top_right"f><"dataTables_top_left"i>>>>tr<"bottom"<"col-md-6"l><"col-md-6 dataTables_bottom_right"p>><"clear">',

					bStateSave: true,

					fnStateSave: function (oSettings, oData) {
						localStorage.setItem(sGridKey, JSON.stringify(oData));
					}, // fnStateSave

					fnStateLoad: function (oSettings) {
						var sData = localStorage.getItem(sGridKey);
						var oData = sData ? JSON.parse(sData) : null;
						return oData;
					}, // fnStateLoad
				});

			} // on success loading customer details
		);
	}, // reloadData

	backToList: function() {
		location.assign('#dashboard');
	}, // backToList
}); // EzBob.Broker.SubmitView