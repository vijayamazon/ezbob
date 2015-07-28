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

		this.refreshTimerInterval = setInterval(_.bind(this.refreshTimer, this), 1000);

	},
	events: {
		'click button': 'onClose'
	},

	render: function () {
		var self = this;
		var width = 600;
		var availableWidth = window.screen.availWidth || document.documentElement.clientWidth;
		if (availableWidth < width) {
			width = availableWidth;
		}
		var timeoutSeconds = EzBob.Config.WizardAutomationTimeout || 20;
		this.$el.html(this.templates['processingTemplate']({
			name: this.model.customer.get('FirstName'),
			automationTimeout: timeoutSeconds,
			offerValid: this.model.customer.offerValidFormatted(),
			amount: this.model.customer.get('CreditSum')
		}));
		this.popup = this.$el;
		this.colorboxPopup = $.colorbox({
			inline: true,
			transition: 'elastic',
			href: this.popup,
			open: true,
			show: true,
			width: width,
			scrolling: false,
			className: 'automation-popup',
			onClosed: function() {
				self.onClose();
			}
		});
		
		var progress = 0;
		
		var time = timeoutSeconds * 1000 / 100;
		this.progressTimeout = setInterval(function () {
			self.$el.find('.automation-processing-bar').width((++progress) + '%');
			if (progress >= 95) {
				self.model.customer.fetch().done(function () {
					var state = self.model.customer.get('state');
					var hasChance = self.model.customer.get('HasApprovalChance');
					var template = '';
					switch (state) {
						case 'bad':
							template = hasChance ? 'rejectHasChanceTemplate' : 'rejectNoChanceTemplate';
							break;
						case 'get':
							template = 'approvedTemplate';
							break;
						default:
							template = 'noDecisionTemplate';
							break;
					}

					self.$el.html(self.templates[template]({
						name: self.model.customer.get('FirstName'),
						automationTimeout: timeoutSeconds,
						offerValid: self.model.customer.offerValidFormatted(),
						amount: self.model.customer.get('CreditSum')
					}));
					$.colorbox.resize();
					if (self.progressTimeout) {
						clearInterval(self.progressTimeout);
					}
				});
			}

			if (progress >= 100 && self.progressTimeout) {
				clearInterval(self.progressTimeout);
			}
		}, time);

		EzBob.UiAction.registerView(this);
		return this;
	},

	onClose: function () {
		if (this.progressTimeout) {
			clearTimeout(this.progressTimeout);
		}
		if (this.refreshTimerInterval) {
			clearTimeout(this.refreshTimerInterval);
		}
	},

	refreshTimer: function () {
		this.$el.find('.offerValidFor').text(this.model.customer.offerValidFormatted() + " hours");
	}, // refreshTimer


});