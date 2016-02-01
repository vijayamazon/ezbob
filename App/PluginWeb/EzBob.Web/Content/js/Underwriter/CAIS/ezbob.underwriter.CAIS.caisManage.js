var EzBob = EzBob || {};

EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.CAIS = EzBob.Underwriter.CAIS || {};

EzBob.Underwriter.CAIS.ListOfFilesModel = Backbone.Model.extend({
	url: gRootPath + 'Underwriter/CAIS/ListOfFiles',

	'default': {
		cais: {}
	}, // default

	initialize: function() {
		var self = this;

		var interval = setInterval(function() { return self.fetch(); }, 2000);

		this.set('interval', interval);
	}, // initialize

}); // EzBob.Underwriter.CAIS.ListOfFilesModel

EzBob.Underwriter.CAIS.SelectedFile = Backbone.Model.extend({
	'default': { id: '', },
});  // EzBob.Underwriter.CAIS.SelectedFile

EzBob.Underwriter.CAIS.SelectedFiles = Backbone.Collection.extend({
	model: EzBob.Underwriter.CAIS.SelectedFile,

	getModelById: function(id) {
		return this.filter(function(val) {
			return val.get('id') === id;
		});
	}, // getModelById
}); // EzBob.Underwriter.CAIS.SelectedFiles

EzBob.Underwriter.CAIS.CaisManageView = Backbone.Marionette.ItemView.extend({
	template: _.template($('#cais-template').length > 0 ? $('#cais-template').html() : ''),

	initialize: function() {
		this.model = new EzBob.Underwriter.CAIS.ListOfFilesModel();
		this.bindTo(this.model, 'change reset', this.render, this);

		BlockUi('on');

		this.model.fetch().done(function() {
			return BlockUi('off');
		});

		this.checkedModel = new EzBob.Underwriter.CAIS.SelectedFiles();

		this.bindTo(this.checkedModel, 'add remove reset', this.checkedFileModelChanged, this);
	}, // initialize

	ui: {
		count: '.reports-count',
		download: '.download',
	}, // ui

	onRender: function() {
	    this.checkedFileModelChanged();
	    EzBob.handleUserLayoutSetting();
	}, // onRender

	serializeData: function() {
		return {
			model: this.model.get('cais'),
			checkedModel: this.checkedModel.toJSON(),
		};
	}, // serializeData

	checkedFileModelChanged: function() {
		if (this.checkedModel.length === 0)
			this.ui.download.hide();
		else {
			this.ui.download.show();
			this.ui.count.text(this.checkedModel.length);
		} // if
	}, // checkedFileModelChanged

	events: {
		'click .generate': 'generateClicked',
		'click .download ': 'downloadFile',
		'dblclick [data-id]': 'fileSelected',
		'click [data-id]': 'fileChecked',
		'click .set-cais-uploaded-status': 'fileUploaded'
	}, // events

	downloadFile: function() {
		_.each(this.checkedModel.toJSON(), function(val) {
			window.open('' + window.gRootPath + 'Underwriter/CAIS/DownloadFile?Id=' + val.id, '_blank');
		});
	}, // downloadFile

	fileViewChanged: function(e) {
		var $el = $(e.currentTarget);
		$('.save-change').removeClass('disabled');
		$el.css('border', '1px solid lightgreen');
	}, // fileViewChanged

	resetFileView: function() {
		$('textarea').css('border', '1px solid #cccccc');
		$('.save-change').addClass('disabled');
	}, // resetFileView

	fileChecked: function(e) {
		if (_.keys($(e.target).data()).length > 0)
			return;

		var $el = $(e.currentTarget);
		var checked = $el.hasClass('checked');
		var id = $el.data('id');

		$el.toggleClass('checked', !checked);

		if (!checked)
			this.checkedModel.add(new EzBob.Underwriter.CAIS.SelectedFile({ id: id }));
		else
			this.checkedModel.remove(this.checkedModel.getModelById(id));
	}, // fileChecked

	generateClicked: function(e) {
		var $el = $(e.currentTarget);

		if ($el.hasClass("disabled"))
			return;

		$el.addClass("disabled");

		$.post(window.gRootPath + 'Underwriter/CAIS/Generate').done(function(response) {
			if (response.error !== void 0) {
				EzBob.ShowMessage('Something went wrong', 'Error occured');
				return;
			} // if
			EzBob.ShowMessage('Generating current CAIS reports. Please wait few minutes.', 'Successful');
		}).always(function() {
			$el.removeClass('disabled');
		});
	}, // generateClicked

	fileSelected: function(e) {
		var self = this;
		var $el = $(e.currentTarget);
		var id = $el.data('id');

		BlockUi('on');

		$.get('' + window.gRootPath + 'Underwriter/CAIS/GetOneFile', {
			id: id
		}).done(function(response) {
			if (response.error) {
				EzBob.ShowMessage(response.error, 'Error');
				return;
			} // if

			var dialog = $('<div/>').html("<textarea wrap='off' class='cais-file-view'>" + response + "</textarea>");

			dialog.dialog({
				title: id,
				width: '75%',
				height: 600,
				modal: true,
				draggable: false,
				resizable: false,
				buttons: [
					{
						text: 'Save file changes',
						click: function(evt) { self.saveFileChange(evt); },
						'class': 'btn btn-primary save-change disabled',
						'data-id': id
					}, {
						html: "<i class='fa fa-refresh'></i>Set Status Uploaded",
						click: function(evt) { self.fileUploaded(evt); },
						'class': 'btn btn-primary save-change',
						'data-id': id
					}, {
						text: 'Close',
						click: function() { dialog.dialog('destroy'); },
						'class': 'btn btn-primary'
					}
				]
			});

			dialog.find('.cais-file-view').on('keypress keyup keydown', self.fileViewChanged);
		}).always(function() {
			BlockUi('off');
		});
	}, // fileSelected

	fileUploaded: function(e) {
		var self = this;
		var $el = $(e.currentTarget);
		var id = $el.data('id');

		BlockUi('on');

		var xhr = $.post('' + window.gRootPath + 'CAIS/UpdateStatus', {
			id: id
		});

		xhr.done(function(response) {
			if (response.error) {
				EzBob.ShowMessage(response.error, 'Something went wrong');
				return;
			} // if

			EzBob.ShowMessage('Status Updated ', 'Successful');
			self.resetFileView();
		});

		xhr.fail(function() {
			EzBob.ShowMessage('Error occured', 'Something went wrong');
		});

		xhr.always(function() {
			BlockUi('off');
		});
	}, // fileUploaded

	saveFileChange: function(e) {
		var self = this;
		var $el = $(e.currentTarget);

		if ($el.hasClass("disabled"))
			return;

		var $caisTextarea = $("textarea:visible");
		var caisContent = $caisTextarea.val();
		var id = $el.data("id");

		var saveFn = function() {
			BlockUi('on');

			var xhr = $.post('' + window.gRootPath + 'CAIS/SaveFileChange', {
				fileContent: caisContent,
				id: id,
			});

			xhr.done(function(response) {
				if (response.error) {
					EzBob.ShowMessage(response.error, 'Something went wrong');
					return;
				} // if

				EzBob.ShowMessage('File #' + id + ' successfully saved ', 'Successful');
				self.resetFileView();
			});

			xhr.fail(function() {
				EzBob.ShowMessage('Error occurred', 'Something went wrong');
			});

			xhr.always(function() {
				BlockUi('off');
			});
		}; // saveFn

		EzBob.ShowMessage("Are you sure you want to save the change?", "Confirmation", saveFn, "Save", null, "Cancel");
	}, // saveFileChange
}); // EzBob.Underwriter.CAIS.CaisManageView
