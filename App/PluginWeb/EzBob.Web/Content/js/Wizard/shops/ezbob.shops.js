(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoView = (function(_super) {
    __extends(StoreInfoView, _super);

    function StoreInfoView() {
      _ref = StoreInfoView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StoreInfoView.prototype.attributes = {
      "class": "stores-view"
    };

    StoreInfoView.prototype.isOffline = function() {
      return this.fromCustomer('IsOffline');
    };

    StoreInfoView.prototype.isProfile = function() {
      return this.fromCustomer('IsProfile');
    };

    StoreInfoView.prototype.fromCustomer = function(sPropName) {
      var oCustomer;

      oCustomer = this.model.get('customer');
      if (!oCustomer) {
        return false;
      }
      if (sPropName === 'IsProfile') {
        return oCustomer.createdInProfile;
      }
      return oCustomer.get(sPropName);
    };

    StoreInfoView.prototype.initialize = function() {
      var acc, accountTypeName, aryCGAccounts, ignore, lc, ordpi, vendorInfo;

      this.renderExecuted = false;
      this.ebayStores = new EzBob.EbayStoreModels();
      this.EbayStoreView = new EzBob.EbayStoreInfoView();
      this.ebayStores.on("reset change", this.marketplacesChanged, this);
      this.amazonMarketplaces = new EzBob.AmazonStoreModels();
      this.AmazonStoreInfoView = new EzBob.AmazonStoreInfoView();
      this.amazonMarketplaces.on("reset change", this.marketplacesChanged, this);
      this.ekmAccounts = new EzBob.EKMAccounts();
      this.EKMAccountInfoView = new EzBob.EKMAccountInfoView({
        model: this.ekmAccounts
      });
      this.freeAgentAccounts = new EzBob.FreeAgentAccounts();
      this.FreeAgentAccountInfoView = new EzBob.FreeAgentAccountInfoView({
        model: this.freeAgentAccounts
      });
      this.sageAccounts = new EzBob.SageAccounts();
      this.sageAccountInfoView = new EzBob.SageAccountInfoView({
        model: this.sageAccounts
      });
      this.PayPointAccounts = new EzBob.PayPointAccounts();
      this.PayPointAccountInfoView = new EzBob.PayPointAccountInfoView({
        model: this.PayPointAccounts
      });
      this.YodleeAccounts = new EzBob.YodleeAccounts();
      this.YodleeAccountInfoView = new EzBob.YodleeAccountInfoView({
        model: this.YodleeAccounts
      });
      this.payPalAccounts = new EzBob.PayPalAccounts(this.model.get("paypalAccounts"));
      this.PayPalInfoView = new EzBob.PayPalInfoView({
        model: this.payPalAccounts
      });
      aryCGAccounts = $.parseJSON($('div#cg-account-list').text());
      for (accountTypeName in aryCGAccounts) {
        ignore = aryCGAccounts[accountTypeName];
        lc = accountTypeName.toLowerCase();
        acc = new EzBob.CGAccounts([], {
          accountType: accountTypeName
        });
        this[lc + 'Accounts'] = acc;
        if (lc === 'hmrc') {
          this[lc + 'AccountInfoView'] = new EzBob.HMRCAccountInfoView({
            model: acc
          });
        } else {
          this[lc + 'AccountInfoView'] = new EzBob.CGAccountInfoView({
            model: acc,
            accountType: accountTypeName
          });
        }
      }
      this.stores = {
        "eBay": {
          view: this.EbayStoreView
        },
        "Amazon": {
          view: this.AmazonStoreInfoView
        },
        "paypal": {
          view: this.PayPalInfoView
        },
        "EKM": {
          view: this.EKMAccountInfoView
        },
        "PayPoint": {
          view: this.PayPointAccountInfoView
        },
        "Yodlee": {
          view: this.YodleeAccountInfoView
        },
        "FreeAgent": {
          view: this.FreeAgentAccountInfoView
        },
        "Sage": {
          view: this.sageAccountInfoView
        }
      };
      for (accountTypeName in aryCGAccounts) {
        vendorInfo = aryCGAccounts[accountTypeName];
        lc = accountTypeName.toLowerCase();
        this.stores[accountTypeName] = {
          view: this[lc + 'AccountInfoView']
        };
      }
      if (typeof ordpi === 'undefined') {
        ordpi = Math.random() * 10000000000000000;
      }
      this.storeList = $(_.template($("#store-info").html(), {
        ordpi: ordpi
      }));
      EzBob.App.on('ct:storebase.shops.connect', this.connect, this);
      return this.isReady = false;
    };

    StoreInfoView.prototype.events = {
      'click a.connect-store': 'close',
      'click a.continue': 'next',
      'click .btn-showmore': 'showMoreAccounts',
      'click .btn-go-to-link-accounts': 'showLinkAccountsForm',
      'click .btn-take-quick-offer': 'takeQuickOffer'
    };

    StoreInfoView.prototype.takeQuickOffer = function() {
      var xhr;

      xhr = $.post(window.gRootPath + 'CustomerDetails/TakeQuickOffer');
      xhr.done(function() {
        EzBob.App.trigger('clear');
        return EzBob.App.GA.trackEventReditect(window.gRootPath + 'Customer/Profile#GetCash', 'Wizard Complete', 'Go To account', 'Quick Offer Taken');
      });
      return false;
    };

    StoreInfoView.prototype.showLinkAccountsForm = function() {
      this.storeList.find('.quick-offer-form').remove();
      return this.storeList.find('.link-accounts-form').removeClass('hide');
    };

    StoreInfoView.prototype.render = function() {
      var accountsList, bFirst, grp, grpid, grpui, hasFilledShops, j, name, oTarget, relevantMpGroups, sActiveField, sBtnClass, sGroupClass, sPriorityField, shop, sortedShopsByPriority, store, storeTypeName, _i, _j, _k, _l, _len, _len1, _len2, _len3, _ref1, _ref2, _ref3, _ref4;

      this.mpGroups = {};
      _ref1 = EzBob.Config.MarketPlaceGroups;
      for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
        grp = _ref1[_i];
        this.mpGroups[grp.Id] = grp;
        grp.ui = null;
      }
      _ref2 = EzBob.Config.MarketPlaces;
      for (_j = 0, _len1 = _ref2.length; _j < _len1; _j++) {
        j = _ref2[_j];
        storeTypeName = j.Name === "Pay Pal" ? "paypal" : j.Name;
        if (this.stores[storeTypeName]) {
          this.stores[storeTypeName].active = this.isProfile() ? (this.isOffline() ? j.ActiveDashboardOffline : j.ActiveDashboardOnline) : (this.isOffline() ? j.ActiveWizardOffline : j.ActiveWizardOnline);
          this.stores[storeTypeName].priority = this.isOffline() ? j.PriorityOffline : j.PriorityOnline;
          this.stores[storeTypeName].ribbon = j.Ribbon ? j.Ribbon : "";
          this.stores[storeTypeName].button = new EzBob.StoreButtonView({
            name: storeTypeName,
            mpAccounts: this.model
          });
          this.stores[storeTypeName].button.ribbon = j.Ribbon ? j.Ribbon : "";
          this.stores[storeTypeName].mandatory = this.isOffline() ? j.MandatoryOffline : j.MandatoryOnline;
          this.stores[storeTypeName].groupid = j.Group != null ? j.Group.Id : 0;
        }
      }
      _ref3 = this.stores;
      for (name in _ref3) {
        store = _ref3[name];
        store.button.on("selected", this.connect, this);
        store.view.on("completed", _.bind(this.completed, this, store.button.name));
        store.view.on("back", this.back, this);
        store.button.on("ready", this.ready, this);
      }
      hasFilledShops = this.canContinue();
      this.storeList.find('.quick-offer-form, .link-accounts-form').addClass('hide');
      if (this.shouldShowQuickOffer(hasFilledShops)) {
        this.storeList.find('.quick-offer-form').removeClass('hide');
        this.renderQuickOfferForm();
      } else {
        this.storeList.find('.link-accounts-form').removeClass('hide');
      }
      this.renderExecuted = true;
      this.showOrRemove();
      accountsList = this.storeList.find('.accounts-list');
      accountsList.empty();
      sActiveField = 'Active' + (this.isProfile() ? 'Dashboard' : 'Wizard') + (this.isOffline() ? 'Offline' : 'Online');
      sPriorityField = 'Priority' + (this.isOffline() ? 'Offline' : 'Online');
      relevantMpGroups = [];
      _ref4 = this.mpGroups;
      for (grpid in _ref4) {
        grp = _ref4[grpid];
        if (grp[sActiveField]) {
          relevantMpGroups.push(grp);
        }
      }
      relevantMpGroups = _.sortBy(relevantMpGroups, function(g) {
        return g[sPriorityField];
      });
      bFirst = true;
      for (_k = 0, _len2 = relevantMpGroups.length; _k < _len2; _k++) {
        grp = relevantMpGroups[_k];
        if (bFirst) {
          bFirst = false;
          sGroupClass = 'first';
        } else {
          sGroupClass = 'following';
        }
        grpui = this.storeList.find('.marketplace-group-template').clone().removeClass('marketplace-group-template hide').addClass(sGroupClass).appendTo(accountsList);
        $('.group-title', grpui).text(grp.DisplayName);
        this.mpGroups[grp.Id].ui = grpui;
      }
      sortedShopsByPriority = _.sortBy(this.stores, function(s) {
        return s.priority;
      });
      for (_l = 0, _len3 = sortedShopsByPriority.length; _l < _len3; _l++) {
        shop = sortedShopsByPriority[_l];
        if (!shop.active) {
          continue;
        }
        oTarget = this.mpGroups[shop.groupid] && this.mpGroups[shop.groupid].ui ? this.mpGroups[shop.groupid].ui : accountsList;
        if (this.isProfile()) {
          sBtnClass = 'marketplace-button-profile';
        } else {
          sBtnClass = this.extractBtnClass(oTarget);
        }
        shop.button.render().$el.addClass('marketplace-button ' + sBtnClass).appendTo(oTarget);
      }
      if (this.isOffline() && !this.isProfile()) {
        this.storeList.find('.marketplace-button-more, .marketplace-group.following').hide();
      }
      if (this.storeList.find('.marketplace-group.following .marketplace-button-full, .marketplace-button-full.marketplace-button-more').length) {
        this.showMoreAccounts();
      }
      this.storeList.appendTo(this.$el);
      EzBob.UiAction.registerView(this);
      this.amazonMarketplaces.trigger("reset");
      this.ebayStores.trigger("reset");
      this.$el.find("img[rel]").setPopover("left");
      this.$el.find("li[rel]").setPopover("left");
      return this;
    };

    StoreInfoView.prototype.renderQuickOfferForm = function() {
      this.storeList.find('.immediate-offer .amount').text(EzBob.formatPounds(this.quickOffer.Amount));
      this.storeList.find('.immediate-offer .term').text('3 months');
      this.storeList.find('.immediate-offer .interest-rate .value').text('3.50%');
      this.storeList.find('.immediate-offer .setup-fee .value').text('1.50%');
      this.storeList.find('.potential-offer .amount').text(EzBob.formatPounds(this.quickOffer.Amount));
      this.storeList.find('.potential-offer .term').text('up to 12 months');
      this.storeList.find('.potential-offer .interest-rate .value').text('x%');
      return this.storeList.find('.potential-offer .setup-fee .value').text('1.50%');
    };

    StoreInfoView.prototype.shouldShowQuickOffer = function(hasFilledShops) {
      if (this.renderExecuted) {
        return false;
      }
      if (hasFilledShops) {
        return false;
      }
      if (this.isProfile()) {
        return false;
      }
      this.quickOffer = this.fromCustomer('QuickOffer');
      if (!this.quickOffer) {
        return false;
      }
      this.requestedAmount = this.fromCustomer('RequestedAmount');
      if (!this.requestedAmount) {
        return false;
      }
      return moment.utc().diff(moment.utc(this.quickOffer.ExpirationDate)) < 0;
    };

    StoreInfoView.prototype.showOrRemove = function() {
      var isOffline, isProfile, sRemove, sShow;

      isOffline = this.isOffline();
      isProfile = this.isProfile();
      $(this.storeList).find('.back-store').remove();
      sShow = '';
      sRemove = '';
      this.storeList.find('.btn-showmore').show();
      if (isOffline) {
        sShow = '.offline_entry_message';
        sRemove = '.online_entry_message';
        this.storeList.find('.importantnumber').text('£150,000');
        if (isProfile) {
          this.storeList.find('.btn-showmore').hide();
          this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
        } else {
          this.storeList.find('.btn-showmore').show();
        }
      } else {
        sShow = '.online_entry_message';
        sRemove = '.offline_entry_message';
        this.storeList.find('.btn-showmore').hide();
        this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
      }
      this.storeList.find(sShow).show();
      this.storeList.find(sRemove).remove();
      if (isProfile) {
        sShow = '.profile_message';
        sRemove = '.wizard_message';
      } else {
        sShow = '.wizard_message';
        sRemove = '.profile_message';
      }
      this.storeList.find(sShow).show();
      return this.storeList.find(sRemove).remove();
    };

    StoreInfoView.prototype.showMoreAccounts = function() {
      this.storeList.find('.btn-showmore').hide();
      this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
      this.storeList.find('.marketplace-button-more, .marketplace-group.following').show();
      return this.storeList.find('.marketplace-button').css('display', 'table');
    };

    StoreInfoView.prototype.canContinue = function() {
      var canContinue, hasEbay, hasFilledShops, hasPaypal, mpType, oStore, sAttrName, _ref1;

      hasFilledShops = false;
      _ref1 = this.stores;
      for (mpType in _ref1) {
        oStore = _ref1[mpType];
        if (oStore.button.shops.length) {
          hasFilledShops = true;
          break;
        }
      }
      hasEbay = this.stores.eBay.button.shops.length > 0;
      hasPaypal = this.stores.paypal.button.shops.length > 0;
      this.$el.find('.eBayPaypalRule').toggleClass('hide', !(hasEbay && !hasPaypal));
      canContinue = false;
      if (this.isProfile()) {
        canContinue = true;
      } else {
        if (hasFilledShops && (!hasEbay || (hasEbay && hasPaypal))) {
          canContinue = true;
        } else {
          sAttrName = this.isOffline() ? 'offline' : 'online';
          canContinue = $('#allowFinishWizardWithoutMarketplaces').attr(sAttrName).toLowerCase() === 'true';
        }
      }
      this.storeList.find('.continue').toggleClass('disabled', !canContinue);
      this.storeList.find('.AddMoreRule').toggleClass('hide', canContinue);
      return hasFilledShops;
    };

    StoreInfoView.prototype.extractBtnClass = function(jqTarget) {
      var sClass;

      sClass = 'pull-left';
      sClass += ' marketplace-button-' + ($('.marketplace-button-less', jqTarget).length < 2 ? 'less' : 'more');
      return sClass;
    };

    StoreInfoView.prototype.marketplacesChanged = function() {
      if (this.ebayStores.length > 0 || this.amazonMarketplaces.length > 0) {
        return this.$el.find(".wizard-top-notification h2").text("Add more shops to get more cash!");
      }
    };

    StoreInfoView.prototype.connect = function(storeName) {
      var oFieldStatusIcons, storeView;

      EzBob.CT.recordEvent("ct:storebase.shops.connect", storeName);
      this.$el.find(">div").hide();
      storeView = this.stores[storeName].view;
      storeView.render().$el.appendTo(this.$el);
      oFieldStatusIcons = storeView.$el.find('IMG.field_status');
      oFieldStatusIcons.filter('.required').field_status({
        required: true
      });
      oFieldStatusIcons.not('.required').field_status({
        required: false
      });
      storeView.$el.show();
      this.oldTitle = $(document).attr("title");
      this.setDocumentTitle(storeView);
      this.setFocus(storeName);
      event.preventDefault();
      return false;
    };

    StoreInfoView.prototype.setFocus = function(storeName) {
      var aryCGAccounts;

      $.colorbox.close();
      switch (storeName) {
        case "EKM":
          return this.$el.find("#ekm_login").focus();
        case "PayPoint":
          return this.$el.find("#payPoint_login").focus();
        default:
          aryCGAccounts = $.parseJSON($('div#cg-account-list').text());
          if (aryCGAccounts[storeName]) {
            return $('.form_field', '#' + storeName.toLowerCase() + 'Account').first().focus();
          }
      }
    };

    StoreInfoView.prototype.setDocumentTitle = function(view) {
      var title;

      title = view.getDocumentTitle();
      if (title) {
        return $(document).attr("title", "Step 2: " + title + " | EZBOB");
      }
    };

    StoreInfoView.prototype.close = function() {
      return this;
    };

    StoreInfoView.prototype.completed = function(name) {
      this.shopConnected(name);
      return this.trigger('completed');
    };

    StoreInfoView.prototype.back = function() {
      this.$el.find(">div").hide();
      this.storeList.show();
      $(document).attr("title", this.oldTitle);
      this.updateEarnedPoints();
      return false;
    };

    StoreInfoView.prototype.next = function() {
      var btn, xhr;

      btn = this.$el.find(".continue");
      if (btn.hasClass("disabled")) {
        return;
      }
      BlockUi('on');
      xhr = $.post(window.gRootPath + 'CustomerDetails/LinkAccountsComplete');
      xhr.done(function() {
        EzBob.App.trigger("clear");
        return EzBob.App.GA.trackEventReditect(window.gRootPath + 'Customer/Profile/Index', 'Wizard Complete', 'Go To account', 'Awaiting Approval');
      });
      xhr.always(function() {
        return BlockUi('off');
      });
      return false;
    };

    StoreInfoView.prototype.ready = function(name) {
      this.trigger("ready", name);
      if (!this.isReady) {
        this.isReady = true;
        return this.$el.find(".continue").show();
      }
    };

    StoreInfoView.prototype.updateEarnedPoints = function() {
      return $.getJSON("" + window.gRootPath + "Customer/Wizard/EarnedPointsStr").done(function(data) {
        if (data.EarnedPointsStr) {
          return $('#EarnedPoints').text(data.EarnedPointsStr);
        }
      });
    };

    StoreInfoView.prototype.shopConnected = function(name) {
      var _this = this;

      return this.model.get('customer').safeFetch().done(function() {
        _this.stores[name].button.update(_this.fromCustomer('mpAccounts'));
        _this.updateEarnedPoints();
        return _this.render();
      });
    };

    return StoreInfoView;

  })(Backbone.View);

}).call(this);
