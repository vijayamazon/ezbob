EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.CustomerDetailsView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.CustomerDetailsView.__super__.initialize.apply(this, arguments);

		this.CustomerID = this.options.customerid;
		this.CrmTable = null;
		this.FileTable = null;
		this.Dropzone = null;
		this.DirectorsList = null;

		this.$el = $('.section-customer-details');
		this.$el.off();

		this.initDropzone();
	}, // initialize

	events: function() {
		var evt = {};

		evt['click .back-to-list'] = 'backToList';
		evt['click .add-crm-note'] = 'addCrmNote';
		evt['click .download-customer-file'] = 'downloadCustomerFile';
		evt['click .remove-file-mark'] = 'fileMarkChanged';
		evt['click .delete-selected-files'] = 'deleteSelectedFiles';
		evt['click .btn-edit-director'] = 'startEditDirector';
		evt['click .lead-send-invitation'] = 'sendInvitation';
		evt['click .lead-fill-wizard'] = 'fillWizard';

		return evt;
	}, // events

	startEditDirector: function(event) {
		var oRow = $(event.target).closest('TR');

		if (oRow.length !== 1)
			return;

		var oView = new EzBob.EditExperianDirectorView({
			data: oRow.data('for-edit'),

			saveUrl: window.gRootPath + 'Broker/BrokerHome/SaveExperianDirector',

			row: oRow,

			editBtn: oRow.find('.btn-edit-director'),
			saveBtn: oRow.find('.btn-save-director'),
			cancelBtn: oRow.find('.btn-cancel-edit'),

			emailCell: oRow.find('.grid-item-email'),
			mobilePhoneCell: oRow.find('.grid-item-phone'),
			addressCell: oRow.find('.grid-item-address'),

			additionalData: { sCustomerID: this.CustomerID, sContactEmail: this.router.getAuth(), },
		});

		oView.render();
	}, // startEditDirector

	sendInvitation: function () {
	    var nLeadID = this.LeadID;

	    if (nLeadID < 1)
	        return;

	    BlockUi();

	    var oRequest = $.post('' + window.gRootPath + 'Broker/BrokerHome/SendInvitation', {
	        nLeadID: nLeadID,
	        sContactEmail: this.router.getAuth(),
	    });

	    var self = this;

	    oRequest.success(function (res) {
	        UnBlockUi();

	        if (res.success) {
	            EzBob.App.trigger('info', 'An invitation has been sent.');
	            self.reloadData();
	            return;
	        } // if

	        if (res.error)
	            EzBob.App.trigger('error', res.error);
	        else
	            EzBob.App.trigger('error', 'Failed to send an invitation.');
	    }); // on success

	    oRequest.fail(function () {
	        UnBlockUi();
	        EzBob.App.trigger('error', 'Failed to send an invitation.');
	    });
	}, // sendInvitation

	fillWizard: function () {
	    var nLeadID = this.LeadID;

	    if (nLeadID < 1)
	        return;

	    location.assign(
			'' + window.gRootPath + 'Broker/BrokerHome/FillWizard' +
			'?nLeadID=' + nLeadID +
			'&sContactEmail=' + encodeURIComponent(this.router.getAuth())
		);
	}, // fillWizard

	clear: function() {
		EzBob.Broker.CustomerDetailsView.__super__.clear.apply(this, arguments);

		this.clearData();

		this.clearFiles();

		this.clearDropzone();
	}, // clear

	clearFiles: function() {
		if (this.FileTable) {
			this.FileTable.fnClearTable();
			this.FileTable = null;
		} // if
	}, // clearFiles

	clearDropzone: function() {
		if (this.Dropzone) {
			this.Dropzone.destroy();
			this.Dropzone = null;
		} // if
	}, // clearDropzone

	clearData: function() {
		this.$el.find('.value').load_display_value({ data_source: {}, });

		if (this.CrmTable) {
			this.CrmTable.fnClearTable();
			this.CrmTable = null;
		} // if

		this.$el.find('.lead-fill-wizard, .lead-send-invitation').addClass('hide');
	}, // clearData

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

		var oDirsArea = this.$el.find('.customer-directors-in-broker');
		oDirsArea.html($('#company-directors-template').html());
		oDirsArea.find('h2').remove();

		this.reloadData();

		this.reloadFileList();

		return this;
	}, // render

	reloadData: function() {
		this.clearData();

		var self = this;

		$.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadCustomerDetails',
			{ sCustomerID: this.CustomerID, sContactEmail: this.router.getAuth(), },
			function (oResponse) {
				if (!oResponse.success) {
					if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to load customer details.');

					return;
				} // if

				self.LeadID = oResponse.personal_data.leadID;

				self.$el.find('.lead-fill-wizard, .lead-send-invitation')
					.toggleClass('hide', oResponse.personal_data.finishedWizard);

				self.$el.find('.value').load_display_value({
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

				var opts = self.initDataTablesOptions('crm', '@CrDate,ActionName,StatusName,Comment', 'tr<"clear">');

				opts.aaData = oResponse.crm_data;

				self.CrmTable = self.$el.find('.customer-crm-list').dataTable(opts);

				var oGroup = self.$el.find('.crm-group');
				var oCaption = $('.aka-caption', oGroup);

				oCaption.find('.dataTables_filter').remove();
				oCaption.append($('.dataTables_filter', oGroup));
				self.$el.find('.directors-list').wrap('<div class="dashboard-table-wrap"><div class="dashboard-table-scorllable"></div></div>');

				self.reloadDirectors(oResponse.potential_signers);
			} // on success loading customer details
		);
	}, // reloadData

	reloadDirectors: function(oPotentialSigners) {
		var self = this;

		var aaData = [];

		_.each(oPotentialSigners, function(director) {
			if (director.DirectorID === 0)
				return;

			var sAddress = _.filter([director.Line1, director.Line2, director.Line3, director.Town, director.County, director.Postcode], $.trim).join(' ');

			var sBirthDate = (director.BirthDate) ? moment.utc(director.BirthDate).format('MMMM Do YYYY') : '';

			aaData.push({
				id: director.DirectorID,
				name: director.FirstName + ' ' + director.LastName,
				email: director.Email,
				phone: director.MobilePhone,
				birthDate: sBirthDate,
				address: sAddress,
				isShareholder: director.IsShareholder,
				isDirector: director.IsDirector,
				isExperian: director.Type === 'experian',
				forEdit: new EzBob.EditExperianDirectorData({
					directorID: director.DirectorID,
					email: director.Email,
					mobilePhone: director.MobilePhone,
					line1: director.Line1,
					line2: director.Line2,
					line3: director.Line3,
					town: director.Town,
					county: director.County,
					postcode: director.Postcode,
				}),
			});
		});

		var opts = {
			bDestroy: true,
			bProcessing: true,
			aoColumns: EzBob.DataTables.Helper.extractColumns('name,birthDate,email,phone,address,isDirector,isShareholder'),

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

		if (this.DirectorsList !== null) {
			this.DirectorsList.fnClearTable();
			this.DirectorsList = null;
		} // if

		var oTbl = this.$el.find('.directors-list');
		this.DirectorsList = oTbl.dataTable(opts);
		oTbl.css('width', '');
	}, // reloadDirectors

	directorsListRowCallback: function(oTR, oData, iDisplayIndex, iDisplayIndexFull) {
		var oRow = $(oTR);

		oRow.data('for-edit', oData.forEdit);

		if (oData.isExperian) {
			oRow.find('.grid-item-edit').empty()
				.append(this.$el.find('.templates').find('.btn-edit-director').clone(true).removeClass('orange'))
				.append(this.$el.find('.templates').find('.btn-save-director').clone(true).removeClass('orange').hide())
				.append(this.$el.find('.templates').find('.btn-cancel-edit').clone(true).removeClass('orange').hide());
		} // if

		var oCell = oRow.find('.grid-item-isDirector').empty();
		if (oData.isDirector === 'yes')
			oCell.append($('<i class="fa fa-check-square-o"></i>'));

		oCell = oRow.find('.grid-item-isShareholder').empty();
		if (oData.isShareholder === 'yes')
			oCell.append($('<i class="fa fa-check-square-o"></i>'));
	}, // directorsListRowCallback

	reloadFileList: function() {
		this.clearFiles();

		var self = this;

		$.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadCustomerFiles',
			{ sCustomerID: this.CustomerID, sContactEmail: this.router.getAuth(), },
			function(oResponse) {
				if (!oResponse.success) {
					if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to load customer files.');

					return;
				} // if
			    var opts = {};
				if (EzBob.Config.Origin !== 'everline') {
				     opts = self.initDataTablesOptions('files', 'FileName', 'ftr<"bottom"<"col-md-6 dataTables_bottom_left"il><"col-md-6 dataTables_bottom_right"p>><"clear">');

				} else {
				    opts = self.initDataTablesOptions('files', 'FileName', 'ftr<"bottom"<"col-md-12 dataTables_bottom_right clearfix"p><"col-md-6 dataTables_bottom_left"i>><"clear">');

				}
                //shlomi

				opts.aaData = oResponse.file_list;

				opts.aoColumns[0].mRender = function(oData, sAction, oFullSource) {
					switch (sAction) {
					case 'display':
						return '<a href="#" class=download-customer-file data-file-id=' + oFullSource.FileID + '>' + (oFullSource.FileDescription || oData) + '</a>';
					case 'filter':
						return (oFullSource.FileDescription || '') + ' ' + oData;
					default:
						return oData;
					} // switch
				}; // mRender

				opts.aoColumns[1] = {
					mData: null,
					sClass: 'center',
					mRender: function(oData, sAction, oFullSource) {
						if (sAction === 'display')
							return '<input type=checkbox class=remove-file-mark data-file-id=' + oFullSource.FileID + '>';

						return '';
					}, // mRender
				};

				self.FileTable = self.$el.find('.customer-file-list').dataTable(opts);

				var oGroup = self.$el.find('.files-group');
				var bGroup = self.$el.find('.dataTables_bottom_right');
				var oCaption = $('.aka-caption', oGroup);
				
				oCaption.find('.dataTables_filter, .delete-selected-files').remove();
				oCaption.append($('.dataTables_filter', oGroup));
				self.$el.find('.customer-file-list').wrap('<div class="dashboard-table-wrap"><div class="dashboard-table-scorllable"></div></div>');
				if (EzBob.Config.Origin !== 'everline') {
				    oCaption.prepend(self.setSomethingEnabled(
				        $('<button type=button class="button btn-green delete-selected-files ev-btn-org" title="Delete selected files">Delete</button>'),
				        false
				    ));
				} else {
				    bGroup.prepend(self.setSomethingEnabled(
					$('<div class="delete-selected-files" >Delete Selected</div>'),
					false
				    ));
				}
				
				

			
			} // on success loading customer details
		); // getJSON
	}, // reloadFileList

	fileMarkChanged: function() {
		this.setSomethingEnabled('.delete-selected-files', this.$el.find('.remove-file-mark:checked').length > 0);
	}, // fileMarkChanged

	deleteSelectedFiles: function() {
		var nSelectedCount = this.$el.find('.remove-file-mark:checked').length;

		if (nSelectedCount < 1)
			return;

		var sMsg = 'Are you sure to delete ' + nSelectedCount + ' selected file' + ((nSelectedCount === 1) ? '' : 's') + '?';

		var self = this;

		EzBob.ShowMessageEx({
				message: sMsg,
				title: 'Please confirm',
				timeout: 0,
				onOk: function() { self.doDeleteSelectedFiles(); },
				okText: 'Yes',
				onCancel: null,
				cancelText: 'No',
				closeOnEscape: true,
				dialogWidth: '500px'
			});
		}, // deleteSelectedFiles

	doDeleteSelectedFiles: function() {
		var arySelected = [];
		this.$el.find('.remove-file-mark:checked').each(function() {
			arySelected.push($(this).attr('data-file-id'));
		});

		if (arySelected.length < 1)
			return;

		EzBob.App.trigger('clear');

		BlockUi();

		var self = this;

		var oXhr = $.ajax({
			type: 'POST',
			url: window.gRootPath + 'Broker/BrokerHome/DeleteCustomerFiles',
			traditional: true,
			data: {
				sCustomerID: this.CustomerID,
				sContactEmail: this.router.getAuth(),
				aryFileIDs: arySelected,
			}, // data
			dataType: 'json',
		});

		oXhr.done = function(oResponse) {
			if (oResponse.success) {
				EzBob.App.trigger('info', 'Selected files have been removed.');
				return;
			} // if

			if (oResponse.error)
				EzBob.App.trigger('error', oResponse.error);
			else
				EzBob.App.trigger('error', 'Failed to remove selected files.');
		}; // on success

		oXhr.fail = function() {
			EzBob.App.trigger('error', 'Failed to remove selected files.');
		}; // on fail

		oXhr.always = function() {
			UnBlockUi();
			self.reloadFileList();
		}; // always
	}, // doDeleteSelectedFiles

	initDataTablesOptions: function(sGridKey, sColumns, sDom) {
		sGridKey = 'brk-grid-state-' + this.router.getAuth() + '-customer-' + sGridKey;
		var tableConf = {
		    bDestroy: true,
		    bProcessing: true,
		    aoColumns: EzBob.DataTables.Helper.extractColumns(sColumns),
		    sDom: sDom,
		    bDeferRender: true,

	        bJQueryUI: false,

	        aaSorting: [[0, 'desc']],
	        iDisplayLength: 10,
	        bAutoWidth: true,
	        aLengthMenu: [[-1, 10, 25, 50, 100], ['all', 10, 25, 50, 100]],
	        bStateSave: true,

	        fnStateSave: function (oSettings, oData) {
	            localStorage.setItem(sGridKey, JSON.stringify(oData));
	        }, // fnStateSave

	        fnStateLoad: function (oSettings) {
	            var sData = localStorage.getItem(sGridKey);
	            var oData = sData ? JSON.parse(sData) : null;
	            return oData;
	        }, // fnStateLoad
	    };
	    if (EzBob.Config.Origin !== 'everline') {
	        tableConf.sPaginationType = 'bootstrap';
	    } else {
	        tableConf.sPaginationType = 'full_numbers';
	        tableConf.oLanguage = {
	         
	            "oPaginate": {
	                "sNext": "...",
	                "sPrevious": "...",
	                "sFirst": "",
	                "sLast": "..."
	            }
	        };
	    
	    }
	    return tableConf;
	}, // initDataTablesOptions

	backToList: function(event) {
		event.preventDefault();
		event.stopPropagation();

		this.clear();
		location.assign('#dashboard');

		return false;
	}, // backToList

	addCrmNote: function(event) {
		event.preventDefault();
		event.stopPropagation();

		var self = this;
		var model = new Backbone.Model({ isBroker: true });
		model.customerId = this.CustomerID;
		var options = {
			url: window.gRootPath + 'Broker/BrokerHome/SaveCrmEntry',
			onsave: function() { self.reloadData(); },
			onbeforesave: function(opts) { opts.sContactEmail = self.router.getAuth(); },
			model: model,
			isBroker: false,
		};

		var view = new EzBob.Underwriter.AddCustomerRelationsEntry(options);

		EzBob.App.jqmodal.show(view);

		return false;
	}, // addCrmNote

	initDropzone: function() {
		this.clearDropzone();

		Dropzone.options.customerFilesUploader = false;

		var self = this;

		this.Dropzone = new Dropzone(this.$el.find('#customerFilesUploader').addClass('dropzone dz-clickable')[0], {
			url: window.gRootPath + 'Broker/BrokerHome/HandleUploadFile',
			parallelUploads: 4,
			uploadMultiple: true,
			acceptedFiles: EzBob.Config.BrokerAcceptedFiles,
			autoProcessQueue: true,
			maxFilesize: EzBob.Config.BrokerMaxFileSize,
			headers: {
				'ezbob-broker-contact-email': self.router.getAuth(),
				'ezbob-broker-customer-id': self.CustomerID,
			}, // headers
			init: function() {
				var oDropzone = this;

				oDropzone.on('success', function(oFile, oResponse) {
					// console.log('Upload succeeded:', oFile, oResponse);

					if (oResponse.success) {
						self.reloadFileList();
						EzBob.App.trigger('info', 'Upload successful: ' + oFile.name);
					}
					else if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to upload ' + oFile.name);
				}); // on success

				oDropzone.on('error', function(oFile, sErrorMsg, oXhr) {
					console.error('Upload error:', oFile, sErrorMsg, oXhr);

					if (oXhr && (oXhr.status === 404)) {
						EzBob.App.trigger('error',
							'Error uploading ' + oFile.name + ': file is too large. ' +
							'Please contact customercare@ezbob.com'
						);
					}
					else
						EzBob.App.trigger('error', 'Error uploading ' + oFile.name + ': ' + sErrorMsg);
				}); // always

				oDropzone.on('complete', function(oFile) {
					oDropzone.removeFile(oFile);
				}); // always
			}, // init
		});
	}, // initDropzone

	downloadCustomerFile: function(event) {
		event.preventDefault();
		event.stopPropagation();

		window.open(
			window.gRootPath + 'Broker/BrokerHome/DownloadCustomerFile' +
			'?sCustomerID=' + encodeURIComponent(this.CustomerID) +
			'&sContactEmail=' + encodeURIComponent(this.router.getAuth()) +
			'&nFileID=' + encodeURIComponent($(event.currentTarget).data('file-id'))
		);
	}, // downloadCustomerFile
}); // EzBob.Broker.CustomerDetailsView
