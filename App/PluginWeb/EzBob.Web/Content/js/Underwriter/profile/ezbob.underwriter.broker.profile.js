var EzBob = EzBob || {};

EzBob.Underwriter.BrokerProfileView = EzBob.View.extend({
	initialize: function() {
		this.brokerID = null;
	}, // initialize

	render: function() {
		var self = this;

		this.myTabHeaders().on('shown.bs.tab', function(e) {
			var sSection = $(e.target).attr('href').substr(1);
			self.handleTabSwitch(sSection);
			console.log('TODO:', sSection);
		});
	}, // render

	events: {
	}, // events

	show: function(id, type) {
		this.brokerID = id;
		this.$el.show();
		this.handleTabSwitch(type);
		EzBob.handleUserLayoutSetting();
	}, // show

	handleTabSwitch: function(sTabID) {
		var oTab = this.myTabHeaders(sTabID, true);
		var sSection = oTab.attr('href').substr(1);

		this.router.navigate('#broker/' + this.brokerID + '/' + sSection);

		if (!oTab.hasClass('active'))
			oTab.tab('show');
	}, // handelTabSwitch

	myTabHeaders: function(sTabID, bReturnFirstIfNotFound) {
		var oAll = this.$el.find('a[data-toggle="tab"]');

		if (!sTabID)
			return bReturnFirstIfNotFound ? oAll.first() : oAll;

		var oFiltered = null;
		var bFound = false;

		try {
			oFiltered = oAll.filter('[href="#' + sTabID + '"]');
			bFound = oFiltered.length;
		}
		catch(e) {
			console.error('Error parsing tab header:', e);
		} // try

		if (bFound)
			return oFiltered;

		return bReturnFirstIfNotFound ? oAll.first() : null;
	}, // myTabHeaders

	hide: function() {
		this.$el.hide();
	}, // hide
}); // EzBob.Underwriter.BrokerProfileView
