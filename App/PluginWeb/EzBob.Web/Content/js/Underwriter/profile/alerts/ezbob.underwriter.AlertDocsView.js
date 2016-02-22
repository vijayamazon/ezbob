var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.DocModel = Backbone.Model.extend({});

EzBob.Underwriter.Docs = Backbone.Collection.extend({
	model: EzBob.Underwriter.DocModel,

	url: function() {
		return '' + window.gRootPath + 'Underwriter/AlertDocs/List/' + this.customerId;
	}, // url
}); // EzBob.Underwriter.Docs

EzBob.Underwriter.UploadDocView = Backbone.Marionette.ItemView.extend({
	template: '#uploadAlertDocDialog',

	initialize: function(options) {
		this.customerId = options.customerId;
	}, // initialize

	jqoptions: function() {
		return {
			modal: true,
			resizable: false,
			title: 'Add document',
			position: 'center',
			draggable: false,
			width: 530,
			dialogClass: 'upload-doc-popup',
		};
	}, // jqoptions

	events: {
		'click .button-upload': 'upload',
		'change #uploadFile': 'uploadFilesChanged'
	}, // events

	uploadFilesChanged: function() {
		var fileNamesContainer = document.getElementById('fileNamesContainer');

		var files = this.$el.find('input[type="file"]')[0].files;

		var i = 0;

		while (i < files.length) {
			var name = this.$el.find('input[type="file"]')[0].files[i].name;
			var newdiv = document.createElement('div');
			newdiv.innerHTML = '<span>' + name + '</span>';
			fileNamesContainer.appendChild(newdiv);
			i++;
		} // while

		return false;
	}, // uploadFilesChanged

	upload: function(e) {
		if ($(e.currentTarget).hasClass('disabled'))
			return;

		$(e.currentTarget).addClass('disabled');

		var f = this.$el.find('input[type="file"]')[0];

		if (typeof f.files !== 'undefined' && f.files.length === 0 || !f.value) {
			EzBob.ShowMessage('Please select a file!', 'Warning');
			$(e.currentTarget).removeClass('disabled');
			return;
		} // if

		this.$el.find('#fileForm').find('input[name=CustomerId]').val(this.customerId);

		var self = this;

		this.$el.find('#fileForm').ajaxSubmit({
			cache: false,
			success: function(oResponse) {
				var bSuccess = !oResponse || oResponse.success;

				var sError = null;

				if (!bSuccess)
					sError = (oResponse && oResponse.error) ? oResponse.error : 'Failed to upload.';

				if (!sError)
					self.trigger('upload:ok');

				self.close();
			},
			error: function() {
				EzBob.ShowMessage('Upload failed, possible cause may be big file size (use smaller file) or session time out.', 'Error');
				$(e.currentTarget).removeClass('disabled');
			}
		});
	}, // upload
});

EzBob.Underwriter.AlertDocsView = Backbone.Marionette.ItemView.extend({
	root: window.gRootPath + 'Underwriter/AlertDocs/',

	template: '#docs-template',

	customerId: 0,

	dialogId: 'null',

	events: {
		'click #addNewDoc': 'addClick',
		'click #deleteDocs': 'deleteClick'
	}, // events

	initialize: function() {
		this.bindTo(this.model, 'change reset fetch sync', this.render, this);
	}, // initialize

	onRender: function(){
		$('.messages-tab').html('Messages / Files (' + this.model.length + ')');
	},

	create: function(customerId) {
		this.customerId = customerId;
		this.dialogId = '#uploadAlertDocDialog' + customerId;
		this.model.customerId = customerId;
	}, // create

	serializeData: function() {
		return {
			docs: this.model.toJSON()
		};
	}, // serializeData

	addClick: function() {
		var view = new EzBob.Underwriter.UploadDocView({ customerId: this.customerId });

		var cb = function() {
			this.model.fetch();
			EzBob.ShowMessage("File successfully downloaded to \"Messages\" tab", "Successful");
		};

		view.on('upload:ok', cb, this);
		EzBob.App.jqmodal.show(view);
	}, // addClick

	deleteClick: function() {
		var ids = [];

		this.$el.find('input[type="checkbox"]:checked').each(function() {
			ids.push($(this).data('id'));
		});

		if (ids.length === 0) {
			EzBob.ShowMessage('Please select at least one document to delete.', 'Warning');
			return false;
		} // if

		var self = this;

		EzBob.ShowMessage('Are you sure you want to delete selected docs?', '', function() {
			var xhr = $.ajax({
				type: 'POST',
				traditional: true,
				url: '' + self.root + 'DeleteDocs',
				data: { docIds: ids, },
				dataType: 'json'
			});

			xhr.done(function() {
				self.model.fetch();
				EzBob.ShowMessage('File successfully deleted', 'Successful');
			});
		}, 'OK', null, 'Cancel');

		return false;
	}, // deleteClick
}); // EzBob.Underwriter.AlertDocsView
