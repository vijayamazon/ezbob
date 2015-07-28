EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.ProccessingAutomationPopupView = Backbone.View.extend({
	initialize: function (options) {
		this.templates = {
			processingTemplate: _.template($('#request-processing-template').html()),
			rejectNoChanceTemplate: _.template($('#request-processing-reject-nochance-template').html()),
			rejectHasChanceTemplate: _.template($('#request-processing-reject-haschance-template').html()),
			noDecisionTemplate: _.template($('#request-processing-nodecision-template').html()),
			approvedTemplate: _.template($('#request-processing-approved-template').html())
		};

		this.model.on('change', this.render, this);

		setInterval(_.bind(this.refreshTimer, this), 1000);

	},
	events: {
		'click button': 'continueClicked'
	},
	
	render: function () {
		//this.model.get('CustomerPersonalInfo').FirstName
		var timeoutSeconds = 5; //todo config
		this.$el.html(this.templates['processingTemplate']({ name: 'Name', automationTimeout: timeoutSeconds, offerValidFormatted: '' }));
		this.popup = this.$el;
		if (this.popup.length > 0)
			$.colorbox({ inline: true, transition: 'elastic', href: this.popup, open: true, show: true, width: 600, scrolling: false, className: 'automation-popup' });
	
		var self = this;
		var progress = 0;
		
		var time = timeoutSeconds * 1000 / 100;
		this.progressTimeout = setInterval(function () {
			self.$el.find('.automation-processing-bar').width((++progress) + '%');
			if (progress >= 95) {
				self.model.customer.fetch().done(function () {
					console.log(self.model.customer);
				});
			}

			if (progress >= 100) {
				clearInterval(self.progressTimeout);

				//todo select template by state
				var template = 'noDecisionTemplate';
				self.$el.html(self.templates[template]({ name: 'Name', automationTimeout: timeoutSeconds, offerValidFormatted: '' }));
				$.colorbox.resize();
			}
		}, time);

		

		EzBob.UiAction.registerView(this);
		return this;
	},

	continueClicked: function () {
		console.log('continue clicked');
		if (this.progressTimeout) {
			clearTimeout(this.progressTimeout);
		}
	},

	refreshTimer: function () {
		this.$el.find('.offerValidFor').text(this.model.customer.offerValidFormatted() + " hours");
	}, // refreshTimer
});