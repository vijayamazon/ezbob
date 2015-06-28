var EzBob = EzBob || {};

(function () {
	function tLogMsg(sSeverity, sMsg) {
		if (!tLogMsg.prototype.LastSeqNum)
			tLogMsg.prototype.LastSeqNum = 0;

		this.msg = sMsg;
		this.severity = sSeverity;

		this.userName = $('body').attr('data-user-name');
		this.eventTime = EzBob.ServerLog.now();

		tLogMsg.prototype.LastSeqNum++;
		this.eventID = EzBob.ServerLog.createID(tLogMsg.prototype.LastSeqNum);
	} // tLogMsg

	function tCachePkg() {
		if (!tCachePkg.prototype.MaxSize) {
			tCachePkg.prototype.MaxSize = 24;
			tCachePkg.prototype.LastSeqNum = 0;
		} // if

		this.data = [];

		tCachePkg.prototype.LastSeqNum++;
		this.id = EzBob.ServerLog.createID(this.LastSeqNum);

		var self = this;

		this.isFull = function() {
			return self.data.length >= self.MaxSize;
		};

		this.add = function (sSeverity, sMsg) {
			self.data.push(new tLogMsg(sSeverity, sMsg));
		}; // add
	} // tCachePkg

	EzBob.ServerLog = EzBob.ServerLog || _.extend({}, EzBob.InternalDebug, {
		internalDebugEnabled: false,
		internalDebugName: 'ServerLog',

		flushInterval: 30, // seconds

		init: function() {
			if (!this.onbeforeunload)
				this.onbeforeunload = $(window).on('beforeunload', $.proxy(this.handleUnload, this));

			if (!this.ontimer)
				this.ontimer = this.startTimer();

			this.internalDebug('initialised');
		}, // init

		now: function () {
			return moment.utc().format('YYYY-MM-DD=HH:mm:ss');
		}, // now

		createID: function (nSeqNum) {
			return '' + nSeqNum + '--' + this.now();
		}, // createID

		startTimer: function () {
			return window.setTimeout((function (ctrlr) { return function () { return ctrlr.handleTimer(); }; })(this), this.flushInterval * 1000);
		}, // startTimer

		writeDown: function (bSync) {
			var self = this;

			this.internalDebug('write down, sync:', bSync ? 'yes' : 'no', 'history:', self.cache.history);

			if (!self.cache.history || self.cache.history.hasNoData()) {
				this.internalDebug('No data in cache, nothing to save.');
				return;
			} // if

			$.ajax({
				async: !bSync,
				type: 'POST',
				dataType: 'json',
				url: '/ServerLog/Say',
				data: {
					version: navigator.userAgent,
					history: JSON.stringify(self.cache.history),
				},
				success: function(oData, sStatus, jqXhr) {
					self.internalDebug('status:', sStatus, 'saved:', oData, 'jqXHR:', jqXhr);

					if (oData) {
						var i;

						if (oData.saved && oData.saved.length) {
							for (i = 0; i < oData.saved.length; i++)
								delete self.cache.history[oData.saved[i]];
						} // if has success
					} // if has data
				}, // success
			}); // $.ajax
		}, // writeDown

		stringify: function() {
			var ary = [];
			var self = this;
			$.each(arguments, function(idx, val) {
				if (val === undefined)
					ary.push('-+- undefined -+-');
				else if (val === null)
					ary.push('-+- null -+-');
				else if (typeof(val) === 'string')
					ary.push(self.strip(val));
				else
					ary.push(self.strip(JSON.stringify(val)));
			});
			
			return ary.join(' ');
		}, // stringify

		///fix for System.Web.HttpRequestValidationException (0x80004005): A potentially dangerous Request.Form value was detected from the client
		strip: function (html) {
			var tmp = document.createElement("DIV");
			tmp.innerHTML = html;
			return tmp.textContent || tmp.innerText || "";
		}, // strip

		debug: function() { this.say('Debug', EzBob.ServerLog.stringify.apply(this, arguments)); }, // debug
		msg:   function() { this.say('Msg',   EzBob.ServerLog.stringify.apply(this, arguments)); }, // msg
		info:  function() { this.say('Info',  EzBob.ServerLog.stringify.apply(this, arguments)); }, // info
		warn:  function() { this.say('Warn',  EzBob.ServerLog.stringify.apply(this, arguments)); }, // warn
		error: function() { this.say('Error', EzBob.ServerLog.stringify.apply(this, arguments)); }, // error
		alert: function() { this.say('Alert', EzBob.ServerLog.stringify.apply(this, arguments)); }, // alert
		fatal: function() { this.say('Fatal', EzBob.ServerLog.stringify.apply(this, arguments)); }, // fatal

		say: function (sSeverity, sMsg) {
			this.internalDebug('say(', sSeverity, sMsg, ')');

			if (this.cache.current && this.cache.current.isFull()) {
				this.cache.history[this.cache.current.id] = this.cache.current;
				this.cache.current = null;

				this.writeDown();
			} // if

			if (!this.cache.current)
				this.cache.current = new tCachePkg();

			this.cache.current.add(sSeverity, sMsg);
		}, // say

		flush: function (bSync) {
			this.internalDebug('flush', this);

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
				function tHistory() {} 

				tHistory.prototype.hasData = function() {
					var bHas = false;

					for (var i in this) {
						if (this.hasOwnProperty(i)) {
							bHas = true;
							break;
						} // if
					} // for

					return bHas;
				}; // hasData

				tHistory.prototype.hasNoData = function() { return !this.hasData(); }; // hasNoData

				return new tHistory();
			})(), // history

			current: null,
		}, // cache

		handleUnload: function () {
			this.internalDebug('unload');
			this.flush(true);
		}, // handleUnload

		handleTimer: function () {
			this.internalDebug('timer');
			this.flush();
			this.startTimer();
		}, // handleTimer

		onbeforeunload: null,

		ontimer: null,

		isFranFleischman: false,
	}); // EzBob.ServerLog

	EzBob.ServerLog.init();
})();
