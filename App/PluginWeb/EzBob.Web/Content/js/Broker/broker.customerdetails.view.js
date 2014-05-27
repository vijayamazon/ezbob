﻿EzBob = EzBob || {};
EzBob.Broker = EzBob.Broker || {};

EzBob.Broker.CustomerDetailsView = EzBob.Broker.BaseView.extend({
	initialize: function() {
		EzBob.Broker.CustomerDetailsView.__super__.initialize.apply(this, arguments);

		this.CustomerID = this.options.customerid;
		this.CrmTable = null;
		this.FileTable = null;
		this.Dropzone = null;

		this.$el = $('.section-customer-details');

		this.initDropzone();
	}, // initialize

	events: function() {
		var evt = {};

		evt['click .back-to-list'] = 'backToList';
		evt['click .add-crm-note'] = 'addCrmNote';
		evt['click .download-customer-file'] = 'downloadCustomerFile';
		evt['click .remove-file-mark'] = 'fileMarkChanged';
		evt['click .delete-selected-files'] = 'deleteSelectedFiles';

		return evt;
	}, // events

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
	}, // clearData

	render: function() {
		if (this.router.isForbidden()) {
			this.clear();
			return this;
		} // if

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
			function(oResponse) {
				if (!oResponse.success) {
					if (oResponse.error)
						EzBob.App.trigger('error', oResponse.error);
					else
						EzBob.App.trigger('error', 'Failed to load customer details.');

					return;
				} // if

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

				var opts = self.initDataTablesOptions('crm', '@CrDate,ActionName,StatusName,Comment');

				opts.aaData = oResponse.crm_data;

				self.CrmTable = self.$el.find('.customer-crm-list').dataTable(opts);

				var oGroup = self.$el.find('.crm-group');
				var oCaption = $('.aka-caption', oGroup);

				oCaption.find('.dataTables_filter').remove();
				oCaption.append($('.dataTables_filter', oGroup));
			} // on success loading customer details
		);
	}, // reloadData

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

				var opts = self.initDataTablesOptions('files', 'FileName');

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
				var oCaption = $('.aka-caption', oGroup);

				oCaption.find('.dataTables_filter, .delete-selected-files').remove();
				oCaption.append($('.dataTables_filter', oGroup));

				oCaption.prepend(self.setSomethingEnabled(
					$('<button type=button class="delete-selected-files" title="Delete selected files">Delete</button>'),
					false
				));
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

		EzBob.ShowMessage(sMsg, 'Please confirm', function() { self.doDeleteSelectedFiles(); }, 'Yes', null, 'No');
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

	initDataTablesOptions: function(sGridKey, sColumns) {
		sGridKey = 'brk-grid-state-' + this.router.getAuth() + '-customer-' + sGridKey;

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
			sDom: 'ftr<"bottom"<"col-md-6"il><"col-md-6 dataTables_bottom_right"p>><"clear">',

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

		var options = {
			actions: JSON.parse($('#crm-lookups .actions').text()),
			statuses: JSON.parse($('#crm-lookups .statuses').text()),
			url: window.gRootPath + 'Broker/BrokerHome/SaveCrmEntry',
			onsave: function() { self.reloadData();  },
			onbeforesave: function(opts) { opts.sContactEmail = self.router.getAuth(); },
			customerId: this.CustomerID,
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
			acceptedFiles: 'image/*,application/pdf,.doc,.docx,.odt,.ppt,.pptx,.odp,.xls,.xlsx,.ods,.txt',
			autoProcessQueue: true,
			maxFilesize: 10,
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
					console.log('Upload error:', oFile, sErrorMsg, oXhr);
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
			'?sCustomerID=' + this.CustomerID +
			'&sContactEmail=' + encodeURIComponent(this.router.getAuth()) +
			'&nFileID=' + encodeURIComponent($(event.currentTarget).attr('data-file-id'))
		);
	}, // downloadCustomerFile
}); // EzBob.Broker.SubmitView
