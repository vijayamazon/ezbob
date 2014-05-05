var EzBob = EzBob || {};

(function() {
	EzBob.InternalDebug = EzBob.InternalDebug || {
		internalDebugEnabled: false,

		internalDebugName: 'InternalDebug',

		internalDebug: function() {
			if (!this.internalDebugEnabled)
				return;

			if (!console.__proto__ || !console.__proto__.log)
				return;

			console.__proto__.log.apply(console, [this.internalDebugNow(), this.internalDebugName + ':'].concat(Array.prototype.slice.call(arguments)));
		}, // internalDebug

		internalDebugNow: function() {
			var oDate = new Date();

			return this.internalDebugFormat(oDate.getHours()) + ':' +
				this.internalDebugFormat(oDate.getMinutes()) + ':' +
				this.internalDebugFormat(oDate.getSeconds());
		}, // internalDebugNow

		internalDebugFormat: function(x) {
			return ((x < 10) ? '0': '') + x;
		}, // internalDebugFormat
	}; // EzBob.InternalDebug
})();
