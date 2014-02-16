EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.DashboardView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.DashboardView.__super__.initialize.apply(this, arguments);

		this.$el = $('.section-dashboard');
	}, // initialize

	events: function() {
		var evt = {};

		evt['click #AddNewCustomer'] = 'addNewCustomer';

		return evt;
	}, // events

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

		EzBob.App.trigger('clear');

		var sGridKey = 'brk-grid-state-' + this.router.getAuth() + '-customer-list';

		var sColumns = '#CustomerID,FirstName,LastName,Email,WizardStep,Status,Marketplaces,^ApplyDate,$LoanAmount,^LoanDate';

		this.$el.find('.customer-list').dataTable({
			bDestroy: true,
			bProcessing: true,
			sAjaxSource: '' + window.gRootPath + 'Broker/BrokerHome/LoadCustomers?sContactEmail=' + encodeURIComponent(this.router.getAuth()),
			aoColumns: EzBob.DataTables.Helper.extractColumns(sColumns),

			bDeferRender: true,

			aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
			iDisplayLength: 50,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			fnRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
				if (oData.hasOwnProperty('Marketplaces'))
					$('.grid-item-Marketplaces', oTR).empty().html(EzBob.DataTables.Helper.showMPsIcon(oData.Marketplaces));

				/*
				if (oData.hasOwnProperty('Id')) {
					$('.grid-item-Id', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id));

					if (oData.hasOwnProperty('Name')) {
						if (oData.Name)
							$('.grid-item-Name', oTR).empty().html(EzBob.Underwriter.GridTools.profileLink(oData.Id, oData.Name));
					} // if has name

					$(oTR).dblclick(function() {
						location.assign($(oTR).find('.profileLink').first().attr('href'));
					});
				} // if has id
				*/
			}, // fnRowCallback

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

		return this;
	}, // render

	addNewCustomer: function() {

	}, // addNewCustomer
}); // EzBob.Broker.SubmitView
