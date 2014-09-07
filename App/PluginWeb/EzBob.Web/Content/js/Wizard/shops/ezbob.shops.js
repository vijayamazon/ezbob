var EzBob = EzBob || {};

EzBob.StoreInfoView = EzBob.View.extend({
	attributes: {
		"class": "stores-view"
	}, // attributes

	isOffline: function() { return this.fromCustomer('IsOffline'); },

	isProfile: function() { return this.fromCustomer('IsProfile'); },

	isWhiteLabel: function() {return this.fromCustomer('IsWhiteLabel'); },
	
	fromCustomer: function(sPropName) {
		var oCustomer = this.model.get('customer');

		if (!oCustomer)
			return false;

		if (sPropName === 'IsProfile')
			return oCustomer.createdInProfile;

		return oCustomer.get(sPropName);
	}, // fromCustomer

	initialize: function() {
		this.renderExecuted = false;

		this.ebayStores = new EzBob.EbayStoreModels();
		this.EbayStoreView = new EzBob.EbayStoreInfoView();
		this.ebayStores.on("reset change", this.marketplacesChanged, this);

		this.amazonMarketplaces = new EzBob.AmazonStoreModels();
		this.AmazonStoreInfoView = new EzBob.AmazonStoreInfoView();
		this.amazonMarketplaces.on("reset change", this.marketplacesChanged, this);

		this.ekmAccounts = new EzBob.EKMAccounts();
		this.EKMAccountInfoView = new EzBob.EKMAccountInfoView({
			model: this.ekmAccounts,
		});

		this.freeAgentAccounts = new EzBob.FreeAgentAccounts();
		this.FreeAgentAccountInfoView = new EzBob.FreeAgentAccountInfoView({
			model: this.freeAgentAccounts,
		});

		this.sageAccounts = new EzBob.SageAccounts();
		this.sageAccountInfoView = new EzBob.SageAccountInfoView({
			model: this.sageAccounts,
		});

		this.PayPointAccounts = new EzBob.PayPointAccounts();
		this.PayPointAccountInfoView = new EzBob.PayPointAccountInfoView({
			model: this.PayPointAccounts,
		});

		this.YodleeAccounts = new EzBob.YodleeAccounts();
		this.YodleeAccountInfoView = new EzBob.YodleeAccountInfoView({
			model: this.YodleeAccounts,
			isProfile: this.isProfile(),
		});

		this.payPalAccounts = new EzBob.PayPalAccounts(this.model.get("paypalAccounts"));
		this.PayPalInfoView = new EzBob.PayPalInfoView({
			model: this.payPalAccounts,
		});

		this.companyFilesAccounts = new EzBob.CompanyFilesAccounts(this.model.get("companyUploadAccounts"));
		this.companyFilesAccountInfoView = new EzBob.CompanyFilesAccountInfoView({
			model: this.companyFilesAccounts,
		});

		var oAllCgVendors = EzBob.CgVendors.all();
		var lc;
		var accountTypeName;

		if (this.isWhiteLabel() && !this.isProfile()) {
			this['HMRC'] = new EzBob.HmrcAccountInfoView({
				model: acc,
				companyRefNum: (this.fromCustomer('CompanyInfo') || {}).ExperianRefNum,
			});
			
			this.stores = {
				Yodlee: { view: this.YodleeAccountInfoView },
				CompanyFiles: { view: this.companyFilesAccountInfoView },
				HMRC: { view: this['HMRC'] }
			};
			
		} else {
			this.stores = {
				eBay: { view: this.EbayStoreView },
				Amazon: { view: this.AmazonStoreInfoView },
				paypal: { view: this.PayPalInfoView },
				EKM: { view: this.EKMAccountInfoView },
				PayPoint: { view: this.PayPointAccountInfoView },
				Yodlee: { view: this.YodleeAccountInfoView },
				FreeAgent: { view: this.FreeAgentAccountInfoView },
				Sage: { view: this.sageAccountInfoView },
				CompanyFiles: { view: this.companyFilesAccountInfoView },
			};

			for (accountTypeName in oAllCgVendors) {
				lc = accountTypeName.toLowerCase();

				var acc = new EzBob.CgAccounts([], { accountType: accountTypeName });

				this[lc + 'Accounts'] = acc;

				if (lc === 'hmrc') {
					this[lc + 'AccountInfoView'] = new EzBob.HmrcAccountInfoView({
						model: acc,
						companyRefNum: (this.fromCustomer('CompanyInfo') || {}).ExperianRefNum,
					});
				} else {
					this[lc + 'AccountInfoView'] = new EzBob.CgAccountInfoView({
						model: acc,
						accountType: accountTypeName,
					});
				} // if

				this.stores[accountTypeName] = { view: this[lc + 'AccountInfoView'] };
			} // for each account type
		}
		
		this.storeList = $(_.template($("#store-info").html(), { }));

		EzBob.App.on('ct:storebase.shops.connect', this.connect, this);

		this.isReady = false;
	}, // initialize

	events: {
		'click a.connect-store': 'close',
		'click .btn-showmore': 'toggleShowMoreAccounts',
		'click .btn-go-to-link-accounts': 'showLinkAccountsForm',
		'click .btn-take-quick-offer': 'takeQuickOffer',
		'click .btn-back-to-quick-offer': 'backToQuickOffer',
		'click #finish-wizard': 'finishWizard',
	}, // events

	finishWizard: function() {
		BlockUi();
		this.$el.find('#finish-wizard').hide();
		this.setSomethingEnabled('#finish-wizard-placeholder', false).removeClass('hide');
	}, // finishWizard

	backToQuickOffer: function() {
		if (this.shouldRemoveQuickOffer())
			this.storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove();
		else {
			this.storeList.find('.link-accounts-form').addClass('hide');
			this.storeList.find('.quick-offer-form').removeClass('hide');
		} // if
	}, // backToQuickOffer

	takeQuickOffer: function() {
		$.post(window.gRootPath + 'CustomerDetails/TakeQuickOffer').done(function() {
			EzBob.App.trigger('clear');

			setTimeout((function() {
				window.location = window.gRootPath + 'Customer/Profile#GetCash';
			}), 500);
		});

		return false;
	}, // takeQuickOffer

	showLinkAccountsForm: function() {
		this.storeList.find('.quick-offer-form').addClass('hide');
		this.storeList.find('.link-accounts-form').removeClass('hide');
	}, // showLinkAccountsForm

	render: function() {
		var i;
		var grp;

		UnBlockUi();

		this.mpGroups = {};

		for (i = 0; i < EzBob.Config.MarketPlaceGroups.length; i++) {
			grp = EzBob.Config.MarketPlaceGroups[i];
			this.mpGroups[grp.Id] = grp;
			grp.ui = null;
		} // for each group

		for (i = 0; i < EzBob.Config.MarketPlaces.length; i++) {
			var oMp = EzBob.Config.MarketPlaces[i];

			var storeTypeName = oMp.Name === "Pay Pal" ? "paypal" : oMp.Name;

			if (this.stores[storeTypeName]) {
				this.stores[storeTypeName].active = this.isProfile() ? (this.isOffline() ? oMp.ActiveDashboardOffline : oMp.ActiveDashboardOnline) : (this.isOffline() ? oMp.ActiveWizardOffline : oMp.ActiveWizardOnline);
				this.stores[storeTypeName].priority = this.isOffline() ? oMp.PriorityOffline : oMp.PriorityOnline;
				this.stores[storeTypeName].ribbon = oMp.Ribbon ? oMp.Ribbon : "";
				this.stores[storeTypeName].button = new EzBob.StoreButtonView({
					name: storeTypeName,
					mpAccounts: this.model,
					description: oMp.Description,
				});
				this.stores[storeTypeName].button.ribbon = oMp.Ribbon ? oMp.Ribbon : "";
				this.stores[storeTypeName].mandatory = this.isOffline() ? oMp.MandatoryOffline : oMp.MandatoryOnline;
				this.stores[storeTypeName].groupid = oMp.Group ? oMp.Group.Id : 0;
			} // if
		} // for

		for (var name in this.stores) {
			var store = this.stores[name];
			store.button.on("selected", this.connect, this);
			store.view.on("completed", _.bind(this.completed, this, store.button.name));
			store.view.on("back", this.back, this);
			store.button.on("ready", this.ready, this);
		} // for

		this.canContinue();

		if (this.renderExecuted) {
			if (this.shouldRemoveQuickOffer()) {
				this.storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove();
				this.storeList.find('.link-accounts-form').removeClass('hide');
			} // if
		}
		else {
			this.storeList.find('.quick-offer-form, .link-accounts-form').addClass('hide');

			if (this.shouldShowQuickOffer()) {
				this.storeList.find('.quick-offer-form').removeClass('hide');
				this.renderQuickOfferForm();
			}
			else {
				this.storeList.find('.link-accounts-form').removeClass('hide');

				if (this.shouldRemoveQuickOffer())
					this.storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove();
			} // if
		} // if

		this.renderExecuted = true;

		this.showOrRemove();

		var accountsList = this.storeList.find('.accounts-list').empty();

		var sActiveField = 'Active' + (this.isProfile() ? 'Dashboard' : 'Wizard') + (this.isOffline() ? 'Offline' : 'Online');
		var sPriorityField = 'Priority' + (this.isOffline() ? 'Offline' : 'Online');

		var relevantMpGroups = [];

		for (var grpid in this.mpGroups) {
			grp = this.mpGroups[grpid];

			if (grp[sActiveField])
				relevantMpGroups.push(grp);
		} // for

		relevantMpGroups = _.sortBy(relevantMpGroups, function(g) { return g[sPriorityField]; });

		var bFirst = true;

		for (i = 0; i < relevantMpGroups.length; i++) {
			grp = relevantMpGroups[i];

			var sGroupClass = 'first';

			if (bFirst)
				bFirst = false;
			else
				sGroupClass = 'following';

			var grpui = this.storeList.find('.marketplace-group-template').clone().removeClass('marketplace-group-template hide').addClass(sGroupClass).appendTo(accountsList);

			$('.group-title', grpui).text(grp.DisplayName);
			this.mpGroups[grp.Id].ui = grpui;
		} // for

		var sortedShopsByPriority = _.sortBy(this.stores, function(s) { return s.priority; });

		for (i = 0; i < sortedShopsByPriority.length; i++) {
			var shop = sortedShopsByPriority[i];

			if (!shop.active)
				continue;

			var oTarget = this.mpGroups[shop.groupid] && this.mpGroups[shop.groupid].ui ? this.mpGroups[shop.groupid].ui : accountsList;

			var sBtnClass = this.isProfile() ? 'marketplace-button-profile' : this.extractBtnClass(oTarget);

			shop.button.render().$el.addClass('marketplace-button ' + sBtnClass).appendTo(oTarget);
		} // for

		if (this.isOffline() && !this.isProfile())
			this.storeList.find('.marketplace-button-more, .marketplace-group.following').hide();

		if (this.storeList.find('.marketplace-group.following .marketplace-button-full, .marketplace-button-full.marketplace-button-more').length)
			this.showMoreAccounts();

		this.storeList.appendTo(this.$el);

		EzBob.UiAction.registerView(this);

		this.amazonMarketplaces.trigger("reset");
		this.ebayStores.trigger("reset");

		this.$el.find("img[rel]").setPopover("left");
		this.$el.find("li[rel]").setPopover("left");

		this.$el.find('.btn-showmore').hover(
			function() { $('.onhover', this).animate({ top: 0,      opacity: 1, }); },
			function() { $('.onhover', this).animate({ top: '60px', opacity: 0, }); }
		);

		return this;
	}, // render

	renderQuickOfferForm: function() {
		this.storeList.find('.immediate-offer .amount, .quick-offer-amount').text(EzBob.formatPoundsNoDecimals(this.quickOffer.Amount));
		this.storeList.find('.immediate-offer .term').text(this.quickOffer.ImmediateTerm + ' months');
		this.storeList.find('.immediate-offer .interest-rate .value').text(EzBob.formatPercentsWithDecimals(this.quickOffer.ImmediateInterestRate));
		this.setQuickOfferFormOnHover(this.storeList.find('.immediate-offer .btn-take-quick-offer'));

		this.storeList.find('.potential-offer .amount').text(EzBob.formatPoundsNoDecimals(this.quickOffer.PotentialAmount));
		this.storeList.find('.potential-offer .term').text('up to ' + this.quickOffer.PotentialTerm + ' months');
		this.storeList.find('.potential-offer .interest-rate .value').text(EzBob.formatPercentsWithDecimals(this.quickOffer.PotentialInterestRate));
		this.setQuickOfferFormOnHover(this.storeList.find('.potential-offer .btn-go-to-link-accounts'));
	}, // renderQuickOfferForm

	setQuickOfferFormOnHover: function(oLinkBtn) {
		oLinkBtn.hover(
			function() {
				var oBtn = $(this);

				var nHeight = oBtn.outerWidth();
				var onHover = oBtn.parent().find('.onhover');

				onHover.css({
					position: 'absolute',
					top: nHeight,
					height: nHeight,
					left: 0,
					width: nHeight,
					display: 'block'
				});

				onHover.animate({ top: 0, opacity: 1 });
			},
			function() {
				var oBtn = $(this);
				var onHover = oBtn.parent().find('.onhover');
				var nHeight = oBtn.outerHeight();

				onHover.animate({ top: nHeight, opacity: 0 });
			});
	}, // setQuickOfferFormOnHover

	shouldRemoveQuickOffer: function() {
		if (!this.quickOffer)
			return true;

		return moment.utc().diff(moment.utc(this.quickOffer.ExpirationDate)) > 0;
	}, // shouldRemoveQuickOffer

	shouldShowQuickOffer: function() {
		if (this.isProfile())
			return false;

		this.quickOffer = this.fromCustomer('QuickOffer');

		if (!this.quickOffer)
			return false;

		this.requestedAmount = this.fromCustomer('RequestedAmount');

		if (!this.requestedAmount)
			return false;

		return moment.utc().diff(moment.utc(this.quickOffer.ExpirationDate)) < 0;
	}, // shouldShowQuickOffer

	showOrRemove: function() {
		var sRemove, sShow;

		var isOffline = this.isOffline();
		var isProfile = this.isProfile();

		$(this.storeList).find('.back-store').remove();
		this.storeList.find('.marketplace-button.show-more').show();

		if (isOffline) {
			sShow = '.offline_entry_message';
			sRemove = '.online_entry_message';

			this.storeList.find('.importantnumber').text('£150,000');

			if (isProfile) {
				this.storeList.find('.marketplace-button.show-more').hide();
				this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
			} else if (this.isWhiteLabel()) {
				this.storeList.find('.marketplace-button.show-more').hide();
			} else {
				this.storeList.find('.btn-showmore').show();
			}
		}
		else {
			sShow = '.online_entry_message';
			sRemove = '.offline_entry_message';

			this.storeList.find('.marketplace-button.show-more').hide();
			this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
		} // if

		if (this.isWhiteLabel() && !this.isProfile()) {
			this.storeList.find('.group-title').hide();
		}
		
		this.storeList.find(sShow).show();
		this.storeList.find(sRemove).remove();

		if (isProfile) {
			sShow = '.profile_message';
			sRemove = '.wizard_message';
		}
		else {
			sShow = '.wizard_message';
			sRemove = '.profile_message';
		} // if

		this.storeList.find(sShow).show();
		this.storeList.find(sRemove).remove();
	}, // showOrRemove

	toggleShowMoreAccounts: function() {
		var oBtn = this.storeList.find('.btn-showmore');

		if (oBtn.attr('data-current') === 'more')
			this.showMoreAccounts();
		else
			this.showLessAccounts();
	}, // toggleShowMoreAccounts

	showLessAccounts: function() {
		var oBtn = this.storeList.find('.btn-showmore');

		oBtn.attr('data-current', 'more');
		oBtn.find('.caption').text('Show more account types');
		oBtn.find('.rotate90').html('&laquo;');
		oBtn.find('.onhover-cell').text('Show more data source connectors');

		this.storeList.find('.AddMoreRuleBottom').addClass('hide');
		this.storeList.find('.marketplace-button-more, .marketplace-group.following').hide();
		this.storeList.find('.marketplace-button').not('.show-more, .marketplace-button-less').css('display', 'none');
	}, // showLessAccounts

	showMoreAccounts: function() {
		var oBtn = this.storeList.find('.btn-showmore');

		oBtn.attr('data-current', 'less');
		oBtn.find('.caption').text('Show less account types');
		oBtn.find('.rotate90').html('&raquo;');
		oBtn.find('.onhover-cell').text('Show less data source connectors');

		this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
		this.storeList.find('.marketplace-button-more, .marketplace-group.following').show();
		this.storeList.find('.marketplace-button').not('.show-more').css('display', 'table');
	}, // showMoreAccounts

	canContinue: function() {
		var mpType, sAttrName;

		var hasFilledShops = false;

		for (mpType in this.stores) {
			var oStore = this.stores[mpType];

			if (oStore.button.shops.length) {
				hasFilledShops = true;
				break;
			} // if
		} // for

		var hasEbay = this.stores.eBay && this.stores.eBay.button.shops.length > 0;
		var hasPaypal = this.stores.paypal && this.stores.paypal.button.shops.length > 0;

		this.$el.find('.eBayPaypalRule').toggleClass('hide', !(hasEbay && !hasPaypal));

		var canContinue;

		if (this.isProfile())
			canContinue = true;
		else {
			if (hasFilledShops && (!hasEbay || hasPaypal))
				canContinue = true;
			else {
				sAttrName = this.isOffline() ? 'offline' : 'online';
				canContinue = $('#allowFinishWizardWithoutMarketplaces').data(sAttrName).toLowerCase() === 'true';
			} // if
		} // if

		this.storeList.find('.continue').toggleClass('disabled', !canContinue);
		this.storeList.find('.AddMoreRule').toggleClass('hide', canContinue);

		return hasFilledShops;
	}, // canContinue

	extractBtnClass: function(jqTarget) {
		var sClass = 'pull-left';

		if ($('.marketplace-button-less', jqTarget).length < 3)
			sClass += ' marketplace-button-less';
		else {
			sClass += ' marketplace-button-more';

			if (this.isOffline() && !this.isProfile() && $('.marketplace-button-more', jqTarget).length === 0)
				sClass += ' marketplace-button-more-first';
		} // if
		return sClass;
	}, // extractBtnClass

	marketplacesChanged: function() {
		if (this.ebayStores.length > 0 || this.amazonMarketplaces.length > 0)
			this.$el.find(".wizard-top-notification h2").text("Add more shops to get more cash!");
	}, // marketplacesChanged 

	connect: function(storeName) {
		EzBob.CT.recordEvent("ct:storebase.shops.connect", storeName);

		this.$el.find(">div").hide();

		var storeView = this.stores[storeName].view;
		storeView.render().$el.appendTo(this.$el);

		var oFieldStatusIcons = storeView.$el.find('IMG.field_status');
		oFieldStatusIcons.filter('.required').field_status({ required: true });
		oFieldStatusIcons.not('.required').field_status({ required: false });

		storeView.$el.show();

		this.oldTitle = $(document).attr("title");
		this.setDocumentTitle(storeView);
		this.setFocus(storeName);

		return false;
	}, // connect

	setFocus: function(storeName) {
		$.colorbox.close();

		switch (storeName) {
		case "EKM":
			this.$el.find("#ekm_login").focus();
			break;

		case "PayPoint":
			this.$el.find("#payPoint_login").focus();
			break;

		default:
			if (EzBob.CgVendors.pure()[storeName])
				$('.form_field', '#' + storeName.toLowerCase() + 'Account').first().focus();
			break;
		} // switch
	}, // setFocus

	setDocumentTitle: function(view) {
		var title = view.getDocumentTitle();

		if (title)
			$(document).attr("title", "Step 4: " + title + " | EZBOB");
	}, // setDocumentTitle

	close: function() {
		return this;
	}, // close

	completed: function(name) {
		this.shopConnected(name);
		this.trigger('completed');
	}, // completed

	back: function() {
		this.$el.find(">div").hide();

		this.storeList.show();

		$(document).attr("title", this.oldTitle);

		// this.updateEarnedPoints();

		return false;
	}, // back

	ready: function(name) {
		this.trigger("ready", name);

		if (!this.isReady) {
			this.isReady = true;
			this.$el.find(".continue").show();
		} // if
	}, // ready

	// updateEarnedPoints: function() {
	// 	$.getJSON("" + window.gRootPath + "Customer/Wizard/EarnedPointsStr").done(function(data) {
	// 		if (data.EarnedPointsStr)
	// 			$('#EarnedPoints').text(data.EarnedPointsStr);
	// 	});
	// }, // updateEarnedPoints

	shopConnected: function(name) {
		var self = this;

		this.model.get('customer').safeFetch().done(function() {
			self.stores[name].button.update(self.fromCustomer('mpAccounts'));
			// self.updateEarnedPoints();
			self.render();
		});
	}, // shopConnected
}); // EzBob.StoreInfoView
