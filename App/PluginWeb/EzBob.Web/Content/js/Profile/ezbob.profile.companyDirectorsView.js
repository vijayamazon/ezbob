var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.CompanyDirectorsView = Backbone.Marionette.ItemView.extend({
	template: "#company-directors-template",

	initialize: function() {
		this.directorsList = null;
	}, // initialize

	events: {
		'click .add-director': 'addDirectorClicked',
	}, // events

	onRender: function() {
		if (!EzBob.Config.EchoSignEnabledCustomer)
			return;

		this.reload();
	}, // onRender

	reload: function() {
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
				isExperian: director.IsExperian,
			});
		});

		if (this.directorsList !== null) {
			this.directorsList.fnClearTable();
			this.directorsList = null;
		} // if

		var oTbl = this.$el.find('.directors-list');
		this.directorsList = oTbl.dataTable(
			this.getDataTableOpts(aryData, 'name,birthDate,email,phone,address,isDirector,isShareholder')
		);
		oTbl.css('width', '');
	}, // reload

	directorsListRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
		var oRow = $(oTR);

		if (oData.isExperian) {
			var oBtn = this.$el.find('.templates').find('.btn-edit-director').clone(true)
				.data('DirectorID', oData.id);

			oRow.find('.grid-item-edit').empty().append(oBtn);
		} // if

		var oCell = oRow.find('.grid-item-isDirector').empty();
		if (oData.isDirector === 'yes')
			oCell.append($('<i class="fa fa-check-square-o"></i>'));

		oCell = oRow.find('.grid-item-isShareholder').empty();
		if (oData.isShareholder === 'yes')
			oCell.append($('<i class="fa fa-check-square-o"></i>'));
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
			else
				col.sClass += ' narrow-as-possible';
		});

		opts.aoColumns.push({
			sClass: 'grid-item-edit narrow-as-possible',
			mData: null,
		});

		return opts;
	}, // getDataTableOpts

	addDirectorClicked: function(event) {
		event.stopPropagation();
		event.preventDefault();

		var director = new EzBob.DirectorModel();
		var directorEl = this.$el.find('.add-company-director');

		this.$el.find('.add-director, .dataTables_wrapper').hide();

		var customerInfo = _.extend({}, this.model.get('CustomerPersonalInfo'), {
			PostCode: this.model.get('PersonalAddress').models[0].get('Rawpostcode')
		}, {
			Directors: this.model.get('CompanyInfo').Directors
		});

		if (!this.addDirector) {
			this.addDirector = new EzBob.AddDirectorInfoView({
				model: director,
				el: directorEl,
				customerInfo: customerInfo,
				failOnDuplicate: true
			});

			var self = this;

			this.addDirector.setBackHandler(function() {
				directorEl.hide();
				self.$el.find('.add-director, .dataTables_wrapper').show();
			});

			this.addDirector.setSuccessHandler(function() {
				directorEl.hide();
				self.$el.find('.add-director, .dataTables_wrapper').show();
				self.model.fetch().done(function() { self.reload(); });
			});

			this.addDirector.setDupCheckCompleteHandler(function(bDupFound) {
				if (bDupFound) {
					if (!self.lastTimeDupDirFound) {
						EzBob.App.trigger('clear');
						EzBob.App.trigger('error', 'Duplicate director detected.');
					} // if

					self.lastTimeDupDirFound = true;
				} else {
					EzBob.App.trigger('clear');
					self.lastTimeDupDirFound = false;
				} // if
			});

			this.addDirector.render();
		} // if

		directorEl.show();

		return false;
	}, // addDirectorClicked
}); // EzBob.Profile.CompanyDirectorsView
