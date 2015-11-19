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
		var timeoutSeconds = EzBob.Config.WizardAutomationTimeout || 20;
		var availableWidth = window.screen.availWidth || document.documentElement.clientWidth;
		if (availableWidth < width) {
			width = availableWidth;
		}

		//only if customer has one of those mps there is a chance of automatic decision and customer should wait for 20 seconds for it.
		//in any other way should be manual decision and no reason for customer to wait
		var hasAutomationMps = _.some(this.model.customer.get('mpAccounts'), function (mp) {
			return _.contains(['paypal', 'Amazon', 'eBay', 'Yodlee', 'HMRC'], mp.MpName);
		});

		var templateName = 'noDecisionTemplate';
		if (hasAutomationMps) {
			templateName = 'processingTemplate';
		}//if
		
		this.$el.html(this.templates[templateName]({
				name: this.model.customer.get('FirstName'),
				automationTimeout: timeoutSeconds,
				offerValid: this.model.customer.offerValidFormatted(),
				amount: this.model.customer.get('CreditSum')
			})
		);//html

		this.popup = this.$el;
		this.colorboxPopup = $.colorbox({
			inline: true,
			transition: 'elastic',
			href: this.popup,
			open: true,
			show: true,
			width: width,
			maxWidth: '100%',
			maxHeight: '100%',
			close: '<i class="pe-7s-close"></i>',
			className: 'automation-popup',
			onOpen: function() {
				$('body').addClass('stop-scroll');
			},
			onClosed: function() {
				$('body').removeClass('stop-scroll');
				self.onClose();
			},
			onComplete: function() {
				$.colorbox.resize();
			}
		});//colorbox
		
		if (hasAutomationMps) {
			var progress = 0;
			var time = timeoutSeconds * 1000 / 100;
			this.progressTimeout = setInterval(function() {
				self.$el.find('.automation-processing-bar').width((++progress) + '%');
				if (progress >= 95) {
					self.model.customer.fetch().done(function() {
						var state = self.model.customer.get('state');
						var hasChance = self.model.customer.get('HasApprovalChance');
						switch (state) {
						case 'bad':
							templateName = hasChance ? 'rejectHasChanceTemplate' : 'rejectNoChanceTemplate';
							break;
						case 'get':
							templateName = 'approvedTemplate';
							break;
						default:
							templateName = 'noDecisionTemplate';
							break;
						}//switch

						self.$el.html(self.templates[templateName]({
							name: self.model.customer.get('FirstName'),
							automationTimeout: timeoutSeconds,
							offerValid: self.model.customer.offerValidFormatted(),
							amount: self.model.customer.get('CreditSum')
						}));

						$.colorbox.resize();
						if (self.progressTimeout) {
							clearInterval(self.progressTimeout);
						}//if
					}); //fetch done
				} //if

				if (progress >= 100 && self.progressTimeout) {
					clearInterval(self.progressTimeout);
				} //if
			}, time); //set interval
		}//if
		EzBob.UiAction.registerView(this);
		return this;
	},//render

	onClose: function () {
		if (this.progressTimeout) {
			clearTimeout(this.progressTimeout);
		}
		if (this.refreshTimerInterval) {
			clearTimeout(this.refreshTimerInterval);
		}
		EzBob.UiAction.saveOne('click', this.$el.find('button'));
	},//onClose

	refreshTimer: function () {
		this.$el.find('.offerValidFor').text(this.model.customer.offerValidFormatted() + " hours");
	}, // refreshTimer


});