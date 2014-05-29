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
			primaryRefNum: '',
			uiEventControlIDs: { form: null, backBtn: null, doneBtn: null, },
			uploadAppError: null,
			uploadComplete: null,
			uploadSuccess: null,
			uploadSysError: null,
			uploadUrl: '',
		}, // defaults

		init: function(options) {
			this.options = _.defaults(options, this.defaults);

			this.options.primaryRefNum = parseInt(this.options.primaryRefNum, 10) || 0;

			this.FirstLoad = true;

			this.Dropzone = null;
			this.BackButton = null;
			this.DoneButton = null;
			this.PeriodsChart = null;
			this.PeriodsDetails = null;
			this.DetailsDataTable = null;

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

			if (this.DetailsDataTable) {
				this.DetailsDataTable.fnDestroy();
				this.DetailsDataTable = null;
			} // if

			this.$el.empty();

			this.PeriodsChart = $('<div />');
			this.$el.append(this.PeriodsChart);

			this.initUiForm();
			this.initUiButtons();

			this.PeriodsDetails = $('<div />');
			this.$el.append(this.PeriodsDetails);

			this.initDropzone();

			this.reloadPeriods();
		}, // render

		initUiButtons: function() {
			this.BackButton = $('<button type=button>Back</button>')
				.attr('ui-event-control-id', this.options.uiEventControlIDs.backBtn)
				.addClass(this.options.classes.backBtn)
				.click(_.bind(function() { this.trigger(this.evtClickBack()); }, this))
				.hide();

			this.DoneButton = $('<button type=button>Done</button>')
				.attr('ui-event-control-id', this.options.uiEventControlIDs.doneBtn)
				.addClass(this.options.classes.doneBtn)
				.click(_.bind(function() { this.trigger(this.evtClickDone()); }, this))
				.hide();

			/*jshint multistr: true */
			var oDivLegend = $('<div class=legend>\
<table class=periods-chart><tbody><tr>\
<th>Legend:</th>\
<td class=found>Data is available</td>\
<td>Data is not available</td></tr>\
</tbody></table>\
</div>');
			/*jshint multistr: false */

			this.$el.append(
				$('<div class=buttons-and-legend></div>')
					.append(oDivLegend)
					.append($('<div class=buttons></div>').append(this.BackButton).append(this.DoneButton))
			);
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
			this.DoneButton.hide();

			var oTableOpts = {
				bDestroy: true,
				bProcessing: false,
				aoColumns: EzBob.DataTables.Helper.extractColumns('^DateFrom,^DateTo,#RegistrationNo,Name'),

				aLengthMenu: [[-1], ['all']],
				iDisplayLength: -1,

				bJQueryUI: false,

				bAutoWidth: true,
				sDom: 't',

				bSort: false,
			}; // dataTable options

			var oXhr = $.getJSON(this.options.loadPeriodsUrl);

			var self = this;

			oXhr.done(function(oResponse) {
				console.log(oResponse);

				if (self.FirstLoad) {
					console.log('first time');
					self.BackButton.show();
					self.FirstLoad = false;
				}
				else {
					console.log('other time');
					var bHasRows = oResponse.aaData && oResponse.aaData.length;

					if (bHasRows)
						self.DoneButton.show();
					else
						self.BackButton.show();
				} // if

				oTableOpts.aaData = self.fillGaps(oResponse.aaData);

				if (self.DetailsDataTable) {
					self.DetailsDataTable.fnDestroy();
					self.DetailsDataTable = null;
				} // if

				/*jshint multistr: true */
				var oTable = $('<table class=period-details>\
<caption>Available VAT return data</caption>\
<thead><tr>\
<th>From</th><th>To</th><th>Registration #</th><th>Company name</th>\
</tr></thead><tbody><tr>\
<td></td><td></td><td></td><td></td>\
</tr></body>\
</table>');
				/*jshint multistr: false */

				oTableOpts.fnRowCallback = function(oTR, oCur, nDisplayIndex, nDisplayIndexFull) {
					if (oCur.IsOtherFirm)
						$(oTR).addClass('other-firm');
					else if (oCur.IsGap)
						$(oTR).addClass('not-just-after');
				}; // row callback

				self.PeriodsDetails.empty().append(oTable);

				self.DetailsDataTable = oTable.dataTable(oTableOpts);

				self.drawPeriodsChart(oTableOpts.aaData);
			});
		}, // reloadPeriods

		fillGaps: function(aryData) {
			var oResult = [];

			for (var i = 0; i < aryData.length; i++) {
				var oCur = aryData[i];
				oCur.Interval = new EzBob.DateInterval(moment.utc(oCur.DateFrom), moment.utc(oCur.DateTo));
				oCur.IsGap = false;
				oCur.IsOtherFirm = false;

				if (i === 0) {
					oResult.push(oCur);
					continue;
				} // if

				var oPrev = aryData[i - 1];

				if (oPrev.RegistrationNo !== oCur.RegistrationNo) {
					oCur.IsOtherFirm = true;
					oResult.push(oCur);
					continue;
				} // if

				if (!this.isJustBefore(oPrev.DateTo, oCur.DateFrom)) {
					var oFrom = moment.utc(oPrev.DateTo).add('days', 1);
					var oTo = moment.utc(oCur.DateFrom).subtract('days', 1);

					oResult.push({
						DateFrom: oFrom,
						DateTo: oTo,
						Interval: null,
						IsGap: true,
						RegistrationNo: 'missing',
						Name: '',
					});
				} // if

				oResult.push(oCur);
			} // for i

			return oResult;
		}, // fillGaps

		drawPeriodsChart: function(aryPeriods) {
			var nPrimaryRefNum = this.options.primaryRefNum;

			var aryData = nPrimaryRefNum ? _.filter(aryPeriods, function(x) { return !x.IsGap && (x.RegistrationNo === nPrimaryRefNum); }) : [];

			console.log('relevant periods', aryData);

			var oTbl = $('<table class=periods-chart><tbody><tr><td class=older>Older</td></tr></tbody></table>');
			var oTR = oTbl.find('tr');

			this.PeriodsChart.empty().append(oTbl);

			var nWayBack = 18;

			var oCurMonth = moment.utc().date(1).subtract('month', nWayBack);

			_.any(aryData, function(x) {
				var bResult = !x.IsGap && moment.utc(x.DateTo).isBefore(oCurMonth);

				if (bResult)
					oTR.find('.older').addClass('found');

				return bResult;
			});

			var oFilter = function(x) { return !x.IsGap && x.Interval.contains(oCurMonth); };

			for (var i = 0; i < nWayBack; i++, oCurMonth = oCurMonth.add('month', 1)) {
				oTR.append(
					$('<td />')
						.text(oCurMonth.format("MMM 'YY"))
						.addClass(_.find(aryData, oFilter) ? 'found' : '')
				);
			} // for
		}, // drawPeriodsChart
	}); // extend
})(); // scope
