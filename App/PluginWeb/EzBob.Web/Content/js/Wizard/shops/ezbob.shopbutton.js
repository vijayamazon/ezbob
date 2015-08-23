var EzBob = EzBob || {};

EzBob.StoreButtonView = Backbone.Marionette.ItemView.extend({
	template: '#store-button-template',
	className: 'marketplace-button-wrapper',
	initialize: function(options) {
		this.name = options.name;
		this.description = options.description;
		this.mpAccounts = options.mpAccounts.get('customer').get('mpAccounts');
		this.shops = this.mpAccounts ? _.where(this.mpAccounts, { MpName: this.name }) : [];
		this.shopClass = options.name.replace(' ', '');
		this.isUpload = options.isUpload || false;
		this.isImage = options.isImage;
		this.hasOr = options.hasOr;
		this.origin = options.mpAccounts.get('customer').get('Origin');
	}, // initialize

	serializeData: function() {
		return {
			name: this.name,
			shopClass: this.shopClass,
			shopDescription: this.description,
			isUpload: this.isUpload,
			origin: this.origin,
			isImage: this.isImage
		};
	}, // serializeData

	onRender: function() {
		var btn = this.$el.find('.marketplace-button-account-' + this.shopClass);

		this.$el.find('.marketplace-button').removeClass('marketplace-button-full marketplace-button-empty');

		var sTitle = (this.shops.length ? 'Some' : 'No') + ' accounts linked. Click to link ';

		if (this.hasOr) {
			this.$el.find('.account-or').show();
		}

		if (this.shops.length) {
			this.$el.find('.marketplace-button').addClass('marketplace-button-full');
			sTitle += 'more.';
		} else {
			this.$el.find('.marketplace-button').addClass('marketplace-button-empty');
			sTitle += 'one.';
		} // if

		this.$el.find('.marketplace-button').attr('title', sTitle);

		switch (this.shopClass) {
		case 'eBay':
		case 'paypal':
		case 'FreeAgent':
		case 'Sage':
			var oHelpWindowTemplate = _.template($('#store-button-help-window-template').html());

			this.$el.append(oHelpWindowTemplate(this.serializeData()));

			var oLinks = JSON.parse($('#store-button-help-window-links').html());

			this.$el.find('.help-window-continue-link').attr('href', oLinks[this.shopClass]);

			btn.attr('href', '#' + this.shopClass + '_help');

			btn.colorbox({
				inline: true,
				transition: 'elastic',
				onClosed: function() {
					var oBackLink = $('#link_account_implicit_back');

					if (oBackLink.length)
						EzBob.UiAction.saveOne('click', oBackLink);
				}, // onClosed
			});
			break;

		default:
			btn.click((function(_this) {
				return function(evt) {
					EzBob.App.trigger('ct:storebase.shops.connect', _this.shopClass);
					evt.preventDefault();
				};
			})(this));
			break;
		} // switch

		var sTop;

		if ($.browser.name.toLowerCase() === 'firefox')
			sTop = '1px';
		else if (document.all)
			sTop = '2px';
		else
			sTop = 0;

		btn.hoverIntent(
			function() {
				var sTableToBlock = 'table-to-block-done';
				var oEl = $(this);
				var oDiv = oEl.parent();

				if (!oDiv.data(sTableToBlock)) {
					var sWidth = oDiv.width() + 'px';
					oEl.css('width', sWidth);
					oDiv.css({ width: sWidth, display: 'block', }).data(sTableToBlock, 'done');
				} // if

				// The purpose of code block from the beginning of the function to this point is to
				// bypass an incorrect behaviour in Firefox/IE. Or maybe Firefox/IE behave according
				// to standards while Chrome does not.

				$('.onhover', this).animate({ top: sTop, opacity: 1 });
			},
			function() { $('.onhover', this).animate({ top: '75px', opacity: 0 }); }
		);
	}, // onRender

	isAddingAllowed: function() {
		return true;
	}, // isAddingAllowed

	update: function(data) {
		this.shops = data ? this.shops = _.where(data, {
			MpName: this.name
		}) : [];
	}, // update
}); // EzBob.StoreButtonView
