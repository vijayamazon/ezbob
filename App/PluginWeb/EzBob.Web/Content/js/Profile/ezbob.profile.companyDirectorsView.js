var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.CompanyDirectorsView = Backbone.Marionette.ItemView.extend({
	template: "#company-directors-template",

	initialize: function() {
		this.directorsList = null;
	}, // initialize

	onRender: function() {
		if (!EzBob.Config.EchoSignEnabledCustomer)
			return;

		var company = this.model.get("CompanyInfo") || {};

		var aryData = [];

		_.each(company.Directors, function(director) {
			aryData.push({
				id: director.Id,
				name: director.Name + ' ' + director.Middle + ' ' + director.Surname,
				email: director.Email,
				phone: director.Phone,
				birthDate: director.DateOfBirth,
				address: (director.DirectorAddress && director.DirectorAddress[0] ? director.DirectorAddress[0].FormattedAddress : ''),
				isShareholder: director.IsShareholder,
				isDirector: director.IsDirector,
			});
		});

		if (this.directorsList !== null) {
			this.directorsList.fnClearTable();
			this.directorsList = null;
		} // if

		var oTbl = this.$el.find('.directors-list');
		this.directorsList = oTbl.dataTable(
			this.getDataTableOpts(aryData, 'name,email,phone,birthDate,address,isDirector,isShareholder')
		);
		oTbl.css('width', '');
	}, // onRender

	directorsListRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
		var oRow = $(oTR);

		var oBtn = this.$el.find('.templates').find('.btn-edit-director').clone(true)
			.data('DirectorID', oData.id);

		oRow.find('.grid-item-edit').empty().append(oBtn);

		var oCell = oRow.find('.grid-item-isDirector').empty();
		if (oData.isDirector === 'yes')
			oCell.append($('<i class="fa fa-check-square-o" />'));

		oCell = oRow.find('.grid-item-isShareholder').empty();
		if (oData.isShareholder === 'yes')
			oCell.append($('<i class="fa fa-check-square-o" />'));
	}, // directorsListRowCallback

	getDataTableOpts: function(aaData, sFieldNames) {
		var self = this;

		var opts = {
			bDestroy: true,
			bProcessing: true,
			aoColumns: EzBob.DataTables.Helper.extractColumns(sFieldNames),

			aaData: aaData,

			aaSorting: [],
			bSort: false,

			aLengthMenu: [[-1], ['all']],
			iDisplayLength: -1,

			sPaginationType: 'bootstrap',
			bJQueryUI: false,

			bAutoWidth: true,
			sDom: 'tr',

			fnRowCallback: _.bind(self.directorsListRowCallback, self),
		};

		_.each(opts.aoColumns, function(col) {
			if ((col.mData !== 'isDirector') && (col.mData !== 'isShareholder'))
			col.sClass += ' l';
		});

		opts.aoColumns.push({
			sClass: 'grid-item-edit',
			mData: null,
		});

		return opts;
	}, // getDataTableOpts
}); // EzBob.Profile.CompanyDirectorsView
