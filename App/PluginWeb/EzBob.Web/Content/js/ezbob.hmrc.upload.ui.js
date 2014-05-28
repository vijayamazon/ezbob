var EzBob = EzBob || {};

(function() {
	EzBob.HmrcUploadUi = function(options) { this.init(options); }; // constructor

	_.extend(EzBob.HmrcUploadUi.prototype, EzBob.SimpleView.prototype, {
		init: function(options) {
			this.options = options || {};

			this.Dropzone = null;

			if (this.options.el)
				this.$el = this.options.el;
			else if (this.options.$el)
				this.$el = this.options.$el;
			else
				this.$el = undefined;

			this.eventHandlers = {};
			this.eventHandlers[this.evtUploadSuccess()] = [];
			this.eventHandlers[this.evtUploadAppError()] = [];
			this.eventHandlers[this.evtUploadSysError()] = [];
			this.eventHandlers[this.evtUploadComplete()] = [];

			if (this.options.uploadSuccess)
				this.eventHandlers[this.evtUploadSuccess()].push(this.options.uploadSuccess);

			if (this.options.uploadAppError)
				this.eventHandlers[this.evtUploadAppError()].push(this.options.uploadAppError);

			if (this.options.uploadSysError)
				this.eventHandlers[this.evtUploadSysError()].push(this.options.uploadSysError);

			if (this.options.uploadComplete)
				this.eventHandlers[this.evtUploadComplete()].push(this.options.uploadComplete);

			if (this.initialize)
				this.initialize();
		}, // init

		on: function(sEventName, oEventHandler) {
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
			this.initDropzone();
		}, // render
	}); // extend
})(); // scope
