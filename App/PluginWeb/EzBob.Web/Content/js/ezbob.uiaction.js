var EzBob = EzBob || {};

(function () {
	function UiActionEvent(options) {
		if (!UiActionEvent.prototype.LastSeqNum)
			UiActionEvent.prototype.LastSeqNum = 0;

		this.userName = $('body').attr('data-user-name');
		this.controlName = options.controlName;
		this.htmlID = options.htmlID;
		this.actionName = options.type;
		this.eventTime = EzBob.UiAction.now();

		UiActionEvent.prototype.LastSeqNum++;
		this.eventID = EzBob.UiAction.createID(this.LastSeqNum);

		this.eventArgs = '';

		if (options.type === 'checked')
			this.eventArgs = options.checked;
		else if (options.saveValue)
			this.eventArgs = options.value;
	} // UiActionEvent

	function UiCachePkg() {
		if (!UiCachePkg.prototype.MaxSize) {
			UiCachePkg.prototype.MaxSize = 24;
			UiCachePkg.prototype.LastSeqNum = 0;
		} // if

		this.data = [];

		UiCachePkg.prototype.LastSeqNum++;
		this.id = EzBob.UiAction.createID(this.LastSeqNum);

		var self = this;

		this.isFull = function() {
			return self.data.length >= self.MaxSize;
		};

		this.add = function (options) {
			self.data.push(new UiActionEvent(options));
		}; // add
	} // UiCachePkg

	EzBob.UiAction = EzBob.UiAction || {
		flushInterval: 30, // seconds

		evtLinked: function () { return 'linked'; }, // linked
		evtChange: function () { return 'change'; }, // change
		evtClick: function () { return 'click'; }, // click
		evtFocusIn: function() { return 'focusin'; }, // focus in
		evtFocusOut: function() { return 'focusout'; }, // focus out

		evtListenerAttr: function () { return 'ezbob:ui-action:event-listener'; }, // listener attr

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

		registerView: function(oView) {
			if (!oView || !oView.$el)
				return;

			this.register(oView.$el.find('[ui-event-control-id][ui-event-control-id!=""]'));
		}, // registerView

		register: function (jqElementList) {
			if (!jqElementList)
				return;

			var self = this;

			jqElementList.each(function () {
				switch (this.tagName.toLowerCase()) {
				case 'select':
				case 'textarea':
					self.attach(self.evtChange(), this, true);
					break;

				case 'a':
				case 'button':
				case 'div':
				case 'img':
				case 'span':
					self.attach(self.evtClick(), this);
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
							self.attach(self.evtChange(), this, true);
							break;

						case 'password':
							self.attach(self.evtChange(), this, false);
							break;

						case 'radio':
						case 'checkbox':
						case 'button':
						case 'submit':
						case 'reset':
						case 'image':
							self.attach(self.evtClick(), this);
							break;

						default:
							console.error('Unsupported INPUT.type', this);
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
			var jqElement = $(oDomElement);

			if (jqElement.data(this.evtListenerAttr()))
				return;

			if (sEventName === this.evtChange())
				sEventName += ' ' + this.evtFocusIn() + ' ' + this.evtFocusOut();

			jqElement
				.on(sEventName, { saveValue: bSaveValue || false }, $.proxy(this.save, this))
				.data(this.evtListenerAttr(), true);
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
						var i;

						if (oData.saved && oData.saved.length) {
							for (i = 0; i < oData.saved.length; i++)
								delete self.cache.history[oData.saved[i]];
						} // if has success

						if (oData.failures && oData.failures.length) {
							for (i = 0; i < oData.failures.length; i++) {
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

		saveOne: function(sType, oTarget, bSaveValue) {
			this.save({
				type: sType,
				target: oTarget,
				data: {
					saveValue: bSaveValue || false
				},
			});
		}, // saveOne

		save: function (evt) {
			// console.log('ui event save(', evt.type, $(evt.target).attr('ui-event-control-id'), evt.target, evt.data.saveValue, ')');

			var oElm = $(evt.currentTarget);

			var oControlName = oElm.attr('ui-event-control-id');

			if (!oControlName)
				return;

			if (this.cache.current && this.cache.current.isFull()) {
				this.cache.history[this.cache.current.id] = this.cache.current;
				this.cache.current = null;

				this.writeDown();
			} // if

			if (!this.cache.current)
				this.cache.current = new UiCachePkg();

			this.cache.current.add({
				type: evt.type,
				saveValue: evt.data.saveValue ? true : false,
				controlName: String(oControlName),
				htmlID: String(oElm.attr('id') || ''),
				value: String(oElm.val() || ''),
				checked: (evt.type === 'checked') ? (evt.target.checked ? 'on' : 'off') : 'off',
			});
		}, // save

		domProps: function (sEventName, oDomElement, b) {
			var oElm = $(oDomElement);

			return ;
		}, // domProps

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
