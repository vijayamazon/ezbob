var EzBob = EzBob || {};

EzBob.VipView = Backbone.Marionette.ItemView.extend({
	template: "#vip-template",
	initialize: function (options) {
		this.model = options.model;
		EzBob.App.on('wizard:vipRequest', this.requestVipBtnClicked, this);
	}, // initialize

	events: {
		"click .vip-image": "vipClicked"
	}, // events

	vipClicked: function () {
		var that = this;
		this.model.fetch().done(function () {
			if (that.model.get('VipFullName') && that.model.get('VipPhone') && that.model.get('VipEmail')) {
				that.submitRequest();
			} else {
				that.$el.find('input[name="VipPhone"]').numericOnly(11);
				if (that.model.get('VipPhone')) {
					that.$el.find('input[name="VipPhone"]').val(that.model.get('VipPhone')).change().attr('readonly', 'readonly');
				}

				if (that.model.get('VipEmail')) {
					that.$el.find('input[name="VipEmail"]').val(that.model.get('VipEmail')).change().attr('readonly', 'readonly');
				}
				$.colorbox({
					href: '#vip_help',
					inline: true, open: true,
					close: '<i class="pe-7s-close"></i>',
					returnFocus: true,
					trapFocus: true,
					onOpen: function() {
						$('body').addClass('stop-scroll');
					},
					onClosed: function() {
						$('body').removeClass('stop-scroll');
					}
				});
			}
		});
	},

	onRender: function () {
		EzBob.UiAction.registerView(this);
	},

	requestVipBtnClicked: function () {
		document.getElementById("requestVipBtn").classList.add('disabled');
		var data = $('#vip-form').serializeArray();
		$.colorbox.close();
		var that = this;
		_.each(data, function (obj) { that.model.set(obj.name, obj.value); });
		this.submitRequest();
	},

	submitRequest: function () {
		this.model.save();
		this.$el.hide();
		var now = moment.utc();
		//19:00-7:00 and weekend Friday 13:00 - Sunday 7:00 off hours
		if ((now.hours() > 19 || now.hours() < 7) || (now.day() == 5 && now.hours() > 13) || (now.day() == 6)) {
			EzBob.App.trigger('info', 'Your VIP request was submitted. We will contact you during office hours.');
		} else {
			EzBob.App.trigger('info', 'Your VIP request was submitted. We will contact you asap.');
		}

	}
});

EzBob.VipModel = Backbone.Model.extend({
	url: function () {
		return window.gRootPath + "Customer/Wizard/Vip";
	},
});