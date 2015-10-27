var EzBob = EzBob || {};

EzBob.AmazonStoreInfoView = Backbone.View.extend({
	initialize: function () {
		return EzBob.CT.bindShopToCT(this, 'amazon');
	},
	render: function () {
		this.$el.html($('#amazon-store-info').html());
		this.form = this.$el.find('.AmazonForm');
		this.validator = EzBob.validateAmazonForm(this.form);
		this.marketplaceId = this.$el.find('#amazonMarketplaceId');
		this.merchantId = this.$el.find('#amazonMerchantId');
		this.amazonMWSAccessToken = this.$el.find('#amazonMWSAccessToken');

		this.marketplaceId.withoutSpaces();
		this.merchantId.withoutSpaces();
		$('body').scrollTop(0);
		EzBob.UiAction.registerView(this);
		return this;
	},
	events: {
		'click a.go-to-amazon': 'enableControls',
		'click a.connect-amazon': 'connect',
		'click a.back': 'back',
		'click .amazonscreenshot': 'runTutorial',
		'click a.print': 'print',
		'change input': 'inputChanged',
		'focusout input': 'inputChanged',
		'keyup input': 'inputChanged'
	},
	enableControls: function () {
		this.$el.find('.amazon_field').removeAttr('disabled');
		if (this.marketplaceId.val().length === 0) {
			this.marketplaceId.field_status('clear', 'right away');
		}
		if (this.merchantId.val().length === 0) {
			this.merchantId.field_status('clear', 'right away');
		}
		if (this.amazonMWSAccessToken.val().length === 0) {
			this.amazonMWSAccessToken.field_status('clear', 'right away');
		}
	},
	inputChanged: function () {
		var enabled = EzBob.Validation.checkForm(this.validator);
		return this.$el.find('a.connect-amazon').toggleClass('disabled', !enabled);
	},
	runTutorial: function () {
		var tutorial = new EzBob.AmazonTutorialView();
		tutorial.render();
		EzBob.App.jqmodal.show(tutorial);

		tutorial.$el.find('.amazon-tutorial-slider').coinslider({
			width: 930,
			height: 471,
			delay: 1000000,
			effect: 'rain',
			sDelay: 30,
			titleSpeed: 50,
			links: false
		});
		return false;
	},
	back: function () {
		this.trigger('back');
		return false;
	},
	connect: function (e) {
		var marketplaceId, merchantId;
		if (!EzBob.Validation.checkForm(this.validator)) {
			this.validator.form();
			return false;
		}
		if (this.$el.find('a.connect-amazon').hasClass('disabled')) {
			return false;
		}
		
		this.blockBtn(true);
		$.post(window.gRootPath + 'Customer/AmazonMarketplaces/ConnectAmazon', {
			marketplaceId: this.marketplaceId.val(),
			merchantId: this.merchantId.val(),
			mwsAccessToken: this.amazonMWSAccessToken.val()
		})
		
		.success((function (_this) {
			return function (result) {
				_this.blockBtn(false);
				if (result.error) {
					EzBob.App.trigger('error', result.error);
					_this.trigger('back');
					return;
				}
				EzBob.App.trigger('info', result.msg);
				_this.trigger('completed');
				_this.trigger('back');
				marketplaceId.val('');
				return merchantId.val('');
			};
		})(this))
		
		.error(function () {
			return EzBob.App.trigger('error', 'Amazon Account Adding Failed');
		});
		
		return false;
	},
	print: function () {
		window.print();
		return false;
	},
	blockBtn: function (isBlock) {
		BlockUi((isBlock ? 'on' : 'off'));
		return this.$el.find('connect-amazon').toggleClass('disabled', isBlock);
	},
	getDocumentTitle: function () {
		EzBob.App.trigger('clear');
		return 'Link Amazon Account';
	}
});

EzBob.AmazonTutorialView = Backbone.Marionette.ItemView.extend({
	template: '#amazon-tutorial',

	onRender: function (){
		this.$el.find('.amazon-tutorial-slider').attr('id', 'amazon-tutorial-slider' + (new Date().getTime())).show();
	},
	jqoptions: function() {
		return {
			autoOpen: true,
			title: 'Amazon tutorial',
			modal: true,
			resizable: false,
			draggable: false,
			width: 960,
			height: 'auto',
			closeOnEscape: true,
		}
	}
});

EzBob.AmazonStoreModel = Backbone.Model.extend({
	defaults: {
		marketplaceId: null
	}
});

EzBob.AmazonStoreModels = Backbone.Collection.extend({
	model: EzBob.AmazonStoreModel,
	url: "" + window.gRootPath + "Customer/AmazonMarketPlaces"
});
