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

			console.__proto__.log.apply(console, [this.internalDebugName + ':'].concat(Array.prototype.slice.call(arguments)));
		}, // internalDebug
	}; // EzBob.InternalDebug
})();
