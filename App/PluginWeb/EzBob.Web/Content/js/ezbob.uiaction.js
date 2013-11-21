EzBob = EzBob || {};

(function () {
	function UiActionEvent(sEventName, oDomElement, bSaveValueOnChange) {
		if (!UiActionEvent.prototype.LastSeqNum)
			UiActionEvent.prototype.LastSeqNum = 0;

		var oElm = $(oDomElement);

		this.userName = $('logged-in-user-name').text();
		this.controlName = oElm.attr('ui-event-control-id');
		this.htmlID = oElm.attr('id');
		this.actionName = sEventName;
		this.eventTime = EzBob.UiAction.now();

		UiActionEvent.prototype.LastSeqNum++;
		this.eventID = EzBob.UiAction.createID(this.LastSeqNum);

		this.eventArgs = '';

		if (bSaveValueOnChange)
			this.eventArgs = oElm.val();
		else if (sEventName == 'checked')
			this.eventArgs = oDomElement.checked ? 'on' : 'off';
	} // UiActionEvent

	function UiCachePkg() {
		if (!UiCachePkg.prototype.MaxSize) {
			UiCachePkg.prototype.MaxSize = 10;
			UiCachePkg.prototype.LastSeqNum = 0;
		} // if

		this.data = [];

		UiCachePkg.prototype.LastSeqNum++;
		this.id = EzBob.UiAction.createID(this.LastSeqNum);

		var self = this;

		this.isFull = function() {
			return self.data.length >= self.MaxSize;
		};

		this.add = function (sEventName, oDomElement, bSaveValue) {
			self.data.push(new UiActionEvent(sEventName, oDomElement, bSaveValue));
		}; // add
	} // UiCachePkg

	EzBob.UiAction = EzBob.UiAction || {
		flushInterval: 30, // seconds

		f: function (n) { return (n < 10) ? ('0' + n) : ('' + n); }, // f

		now: function () {
			var d = new Date();

			return this.f(d.getUTCFullYear()) +
				this.f(d.getUTCMonth() + 1) +
				this.f(d.getUTCDate()) +
				this.f(d.getUTCHours()) +
				this.f(d.getUTCMinutes()) +
				this.f(d.getUTCSeconds());
		}, // now

		createID: function (nSeqNum) {
			return '' + nSeqNum + '' + this.now();
		}, // createID

		register: function (jqElementList) {
			if (!jqElementList)
				return;

			var self = this;

			jqElementList.each(function () {
				switch (this.tagName.toLowerCase()) {
					case 'select':
					case 'textarea':
						self.attach('change', this, true);
						break;

					case 'a':
					case 'button':
						self.attach('click', this);
						break;

					case 'input':
						switch (this.type.toLowerCase()) {
							case 'text':
							case 'number':
							case 'email':
							case 'date':
							case 'datetime':
							case 'datetime-local':
							case 'file':
							case 'hidden':
							case 'month':
							case 'range':
							case 'search':
							case 'time':
							case 'url':
							case 'week':
							case 'tel':
								self.attach('change', this, true);
								break;

							case 'password':
								self.attach('change', this, false);
								break;

							case 'radio':
							case 'checkbox':
								self.attach('checked', this);
								break;

							case 'button':
							case 'submit':
							case 'reset':
							case 'image':
								self.attach('click', this);
								break;

							default:
								//console.error('Unsupported INPUT.type', this);
								break;
						} // switch for INPUT

						break;
				} // switch for element
			}); // for each

			if (!this.onbeforeunload)
				this.onbeforeunload = $(window).on('beforeunload', $.proxy(this.handleUnload, this));

			if (!this.ontimer)
				this.ontimer = this.startTimer();
		}, // register

		startTimer: function () {
			return window.setTimeout((function (ctrlr) { return function () { return ctrlr.handleTimer(); }; })(this), this.flushInterval * 1000);
		}, // startTimer

		attach: function (sEventName, oDomElement, bSaveValue) {
			if (sEventName == 'change')
				sEventName += ' focusin focusout';

			$(oDomElement).on(sEventName, { saveValue: bSaveValue || false }, $.proxy(this.save, this));
		}, // attach

		writeDown: function (bSync) {
			var self = this;

			//console.log('write down, sync:', bSync ? 'yes' : 'no', 'history:', self.cache.history);

			if (!self.cache.history || self.cache.history.hasNoData()) {
				//console.log('No data in cache, nothing to save.');
				return;
			} // if

			$.ajax({
				async: !bSync,
				type: 'POST',
				dataType: 'json',
				url: '/UiAction/Save',
				data: {
					version: navigator.userAgent,
					history: JSON.stringify(self.cache.history),
				},
				success: function(oData, sStatus, jqXHR) {
					//console.log('status:', sStatus, 'saved:', oData, 'jqXHR:', jqXHR);

					if (oData) {
						if (oData.saved && oData.saved.length) {
							for (var i = 0; i < oData.saved.length; i++)
								delete self.cache.history[oData.saved[i]];
						} // if has success

						if (oData.failures && oData.failures.length) {
							for (var i = 0; i < oData.failures.length; i++) {
								var oPkg = oData.failures[i];

								var oCached = self.cache.history[oPkg.id];

								for (var j = 0; j < oPkg.success.length; j++)
									delete oCached[oPkg.success[j]];
							} // for each failed package
						} // if has failures
					} // if has data
				}, // success
			}); // $.ajax
		}, // writeDown

		save: function (evt) {
			if (this.cache.current && this.cache.current.isFull()) {
				this.cache.history[this.cache.current.id] = this.cache.current;
				this.cache.current = null;

				this.writeDown();
			} // if

			if (!this.cache.current)
				this.cache.current = new UiCachePkg();

			this.cache.current.add(evt.type, evt.target, evt.data.saveValue);
		}, // save

		flush: function (bSync) {
			//console.log('UiAction.flush', this);

			if (this.isFranFleischman)
				return;

			this.isFranFleischman = true;

			if (this.cache.current) {
				this.cache.history[this.cache.current.id] = this.cache.current;
				this.cache.current = null;
			} // if

			this.writeDown(bSync);

			this.isFranFleischman = false;
		}, // flush

		cache: {
			history: (function() {
				function History() {} 

				History.prototype.hasData = function() {
					var bHas = false;

					for (var i in this) {
						if (this.hasOwnProperty(i)) {
							bHas = true;
							break;
						} // if
					} // for

					return bHas;
				}; // hasData

				History.prototype.hasNoData = function() { return !this.hasData(); }; // hasNoData

				return new History();
			})(), // history

			current: null,
		}, // cache

		handleUnload: function () {
			//console.log('unload');
			this.flush(true);
		}, // handleUnload

		handleTimer: function () {
			//console.log('timer');
			this.flush();
			this.startTimer();
		}, // handleTimer

		onbeforeunload: null,

		ontimer: null,

		isFranFleischman: false,
	}; // EzBob.UiAction
})();
