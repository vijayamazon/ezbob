var EzBob = EzBob || {};

(function() {
	EzBob.HmrcUploadUi = function(options) { this.init(options); }; // constructor

	_.extend(EzBob.HmrcUploadUi.prototype, EzBob.SimpleView.prototype, {
		defaults: {
			classes: { form: null, backBtn: null, doneBtn: null, },
			clickBack: null,
			clickDone: null,
			el: '',
			formID: '',
			headers: null,
			loadPeriodsUrl: null,
			uiEventControlIDs: { form: null, backBtn: null, doneBtn: null, },
			uploadAppError: null,
			uploadComplete: null,
			uploadSuccess: null,
			uploadSysError: null,
			uploadUrl: '',
		}, // defaults

		init: function(options) {
			this.options = _.defaults(options, this.defaults);

			this.Dropzone = null;
			this.BackButton = null;
			this.DoneButton = null;
			this.Periods = null;

			this.$el = undefined;

			this.eventHandlers = {};

			this.on(this.evtClickBack(), this.options.clickBack);
			this.on(this.evtClickDone(), this.options.clickDone);

			this.on(this.evtUploadSuccess(), _.bind(this.reloadPeriods, this));

			this.on(this.evtUploadSuccess(), this.options.uploadSuccess);
			this.on(this.evtUploadAppError(), this.options.uploadAppError);
			this.on(this.evtUploadSysError(), this.options.uploadSysError);
			this.on(this.evtUploadComplete(), this.options.uploadComplete);
		}, // init

		on: function(sEventName, oEventHandler) {
			if (!oEventHandler)
				return;

			if (!this.eventHandlers[sEventName])
				this.eventHandlers[sEventName] = [oEventHandler];
			else
				this.eventHandlers[sEventName].push(oEventHandler);
		}, // on

		trigger: function() {
			if (!arguments.length)
				return;

			var sEventName = arguments[0];

			var oList = this.eventHandlers[sEventName];
			if (!oList || !oList.length)
				return;

			var args = Array.prototype.slice.call(arguments, 1);

			for (var i = 0; i < oList.length; i++)
				oList[i].apply(null, args);
		}, // trigger

		clearDropzone: function() {
			if (this.Dropzone) {
				this.Dropzone.destroy();
				this.Dropzone = null;
			} // if
		}, // clearDropzone

		evtClickBack: function () { return 'c-b'; },
		evtClickDone: function () { return 'c-d'; },
		evtUploadSuccess: function() { return 'u-ok'; },
		evtUploadAppError: function() { return 'u-ae'; },
		evtUploadSysError: function() { return 'u-se'; },
		evtUploadComplete: function() { return 'u-c'; },

		initDropzone: function() {
			this.clearDropzone();

			Dropzone.options[this.options.formID] = false;

			var self = this;

			this.Dropzone = new Dropzone(this.$el.find('#' + this.options.formID).addClass('dropzone dz-clickable')[0], {
				parallelUploads: 1,
				uploadMultiple: true,
				acceptedFiles: 'application/pdf',
				autoProcessQueue: true,
				maxFilesize: 10,
				headers: this.options.headers,

				init: function() {
					var oDropzone = this;

					oDropzone.on('success', function(oFile, oResponse) {
						console.log('Upload', (oResponse.success ? '' : 'NOT'), 'succeeded:', oFile, oResponse);

						if (oResponse.success) {
							EzBob.App.trigger('info', 'Upload successful: ' + oFile.name);
							self.trigger(self.evtUploadSuccess(), oFile, oResponse);
						}
						else if (oResponse.error) {
							EzBob.App.trigger('error', oResponse.error);
							self.trigger(self.evtUploadAppError(), oFile, oResponse);
						}
						else {
							EzBob.App.trigger('error', 'Failed to upload ' + oFile.name);
							self.trigger(self.evtUploadAppError(), oFile, oResponse);
						} // if
					}); // on success

					oDropzone.on('error', function(oFile, sErrorMsg, oXhr) {
						console.log('Upload error:', oFile, sErrorMsg, oXhr);
						EzBob.App.trigger('error', 'Error uploading ' + oFile.name + ': ' + sErrorMsg);
						self.trigger(self.evtUploadSysError(), oFile, sErrorMsg, oXhr);
					}); // always

					oDropzone.on('complete', function(oFile) {
						oDropzone.removeFile(oFile);
						self.trigger(self.evtUploadComplete(), oFile);
					}); // always
				}, // init
			}); // create a dropzone
		}, // initDropzone

		render: function() {
			if (this.options.el)
				this.$el = $(this.options.el);
			else if (this.options.$el)
				this.$el = $(this.options.$el);

			this.$el.empty();

			this.initUiForm();
			this.initUiButtons();

			this.Periods = $('<div />');
			this.$el.append(this.Periods);

			this.initDropzone();
		}, // render

		initUiButtons: function() {
			this.BackButton = $('<button type=button>Back</button>')
				.attr('ui-event-control-id', this.options.uiEventControlIDs.backBtn)
				.addClass(this.options.classes.backBtn)
				.click(_.bind(function() { this.trigger(this.evtClickBack()); }, this));

			this.DoneButton = $('<button type=button>Done</button>')
				.attr('ui-event-control-id', this.options.uiEventControlIDs.doneBtn)
				.addClass(this.options.classes.doneBtn)
				.click(_.bind(function() { this.trigger(this.evtClickDone()); }, this))
				.hide();

			var oDiv = $('<div class="attardi-button"></div>');
			oDiv.append(this.BackButton).append(this.DoneButton);

			oDiv = $('<div class="form_buttons_container hmrc_margin_from_header"></div>').append(oDiv);

			oDiv = $('<div class="clearfix"></div>').append(oDiv);

			this.$el.append(oDiv);
		}, // initUiButtons

		initUiForm: function() {
			var oForm = $('<form />').attr({
				id: this.options.formID,
				action: this.options.uploadUrl,
			});

			oForm.append($('<div class="dz-message">Drag or Click to upload VAT files</div>'));

			this.$el.append(oForm);
		}, // initUiForm

		reloadPeriods: function() {
			this.BackButton.hide();
			this.DoneButton.show();
		}, // reloadPeriods
	}); // extend
})(); // scope
