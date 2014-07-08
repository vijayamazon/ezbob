var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.CompanyDirectorsView = Backbone.Marionette.ItemView.extend({
	template: "#company-directors-template",

	initialize: function() {
		this.directorsList = null;
	}, // initialize

	events: {
		'click .add-director': 'addDirectorClicked',
		'click .btn-edit-director': 'startEditDirector',
	}, // events

	startEditDirector: function(event) {
		var oRow = $(event.target).closest('TR');

		if (oRow.length !== 1)
			return;

		var oView = new EzBob.EditExperianDirectorView({
			data: oRow.data('for-edit'),

			saveUrl: window.gRootPath + 'Customer/CustomerDetails/SaveExperianDirector',

			row: oRow,

			editBtn: oRow.find('.btn-edit-director'),
			saveBtn: oRow.find('.btn-save-director'),
			cancelBtn: oRow.find('.btn-cancel-edit'),

			emailCell: oRow.find('.grid-item-email'),
			mobilePhoneCell: oRow.find('.grid-item-phone'),
			addressCell: oRow.find('.grid-item-address'),
		});

		oView.render();
	}, // startEditDirector

	onRender: function() {
		if (!EzBob.Config.EchoSignEnabledCustomer)
			return;

		this.reload();
	}, // onRender

	reload: function() {
		var company = this.model.get("CompanyInfo") || {};

		var aryData = [];

		_.each(company.Directors, function(director) {
			var bHasAddress = director.DirectorAddress && director.DirectorAddress[0];

			aryData.push({
				id: director.Id,
				name: director.Name + ' ' + director.Middle + ' ' + director.Surname,
				email: director.Email,
				phone: director.Phone,
				birthDate: director.DateOfBirth,
				address: (bHasAddress ? director.DirectorAddress[0].FormattedAddress : ''),
				isShareholder: director.IsShareholder,
				isDirector: director.IsDirector,
				isExperian: director.IsExperian,
				forEdit:  new EzBob.EditExperianDirectorData({
					directorID: director.Id,
					email: director.Email,
					mobilePhone: director.Phone,
					line1: (bHasAddress ? director.DirectorAddress[0].Line1 : ''),
					line2: (bHasAddress ? director.DirectorAddress[0].Line2 : ''),
					line3: (bHasAddress ? director.DirectorAddress[0].Line3 : ''),
					town: (bHasAddress ? director.DirectorAddress[0].Town : ''),
					county: (bHasAddress ? director.DirectorAddress[0].County : ''),
					postcode: (bHasAddress ? director.DirectorAddress[0].Postcode : ''),
				}),
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

		oRow.data('for-edit', oData.forEdit);

		if (oData.isExperian) {
			oRow.find('.grid-item-edit').empty()
				.append(this.$el.find('.templates').find('.btn-edit-director').clone(true))
				.append(this.$el.find('.templates').find('.btn-save-director').clone(true).hide())
				.append(this.$el.find('.templates').find('.btn-cancel-edit').clone(true).hide());
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
