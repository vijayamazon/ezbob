var EzBob = EzBob || {};

EzBob.NotificationsView = Backbone.View.extend({
	initialize: function() {
		EzBob.App.on("info", this.info, this);
		EzBob.App.on("error", this.error, this);
		EzBob.App.on("warning", this.warning, this);
		EzBob.App.on("clear", this.clearAll, this);
	},

	info: function(msg) {
		this.makeAlert(msg, 'notification_green effect_fadein');
	},

	error: function(msg) {
		this.makeAlert(msg, 'notification_red effect_fadein');
	},

	warning: function(msg) {
		this.makeAlert(msg, 'notification_yellow effect_fadein');
	},

	makeAlert: function(msg, alertClass) {
		if (!msg) return;
		var alert = $('<div class="' + alertClass + '"> ' + msg + ' </div>');
		this.scrollTop();
		this.$el.html(alert);
		alert.alert();
		this.$el.notification();
	},

	render: function() {
		this.$el.html();
		return this;
	},

	clearAll: function() {
		this.$el.html("");
	},

	scrollTop: function() {
		$('html,body').animate({ scrollTop: 0 }, 500);
	}
});


(function() {
	EzBob.App.on("info", info);
	EzBob.App.on("error", error);
	EzBob.App.on("warning", warning);
	EzBob.App.on("clear", clear);

	function info(text) {
		if (EzBob.CT)
			EzBob.CT.recordEvent("info", text);
	}

	function error(text) {
		if (EzBob.CT)
			EzBob.CT.recordEvent("error", text);
	}

	function warning(text) {
		if (EzBob.CT)
			EzBob.CT.recordEvent("warning", text);
	}

	function clear() {
		if (EzBob.CT)
			EzBob.CT.recordEvent("clear");
	}
})();