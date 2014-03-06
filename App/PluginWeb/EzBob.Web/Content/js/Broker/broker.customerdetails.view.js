EzBob = EzBob || {};
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
		this.$el.find('.customer-personal-details .value').load_display_value({ data_source: {}, });

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

		EzBob.App.trigger('clear');

		this.reloadData();

		this.reloadFileList();

		return this;
	}, // render

	reloadData: function() {
		this.clearData();

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

				var opts = self.initDataTablesOptions('crm', '@CrDate,ActionName,StatusName,Comment');

				opts.aaData = oResponse.crm_data;

				self.CrmTable = self.$el.find('.customer-crm-list').dataTable(opts);
			} // on success loading customer details
		);
	}, // reloadData

	reloadFileList: function() {
		this.clearFiles();

		var self = this;

		$.getJSON(
			window.gRootPath + 'Broker/BrokerHome/LoadCustomerFiles',
			{ nCustomerID: this.CustomerID, sContactEmail: this.router.getAuth(), },
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
				}; // fnRowCallback

				self.FileTable = self.$el.find('.customer-file-list').dataTable(opts);
			} // on success loading customer details
		);
	}, // reloadFileList

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
					console.log('upload succeded:', oFile, oResponse);

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
			'?nCustomerID=' + this.CustomerID +
			'&sContactEmail=' + encodeURIComponent(this.router.getAuth()) +
			'&nFileID=' + encodeURIComponent($(event.currentTarget).attr('data-file-id'))
		);
	}, // downloadCustomerFile
}); // EzBob.Broker.SubmitView
