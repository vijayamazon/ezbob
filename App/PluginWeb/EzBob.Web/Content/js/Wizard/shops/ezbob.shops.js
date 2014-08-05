(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoView = (function(_super) {

    __extends(StoreInfoView, _super);

    function StoreInfoView() {
      return StoreInfoView.__super__.constructor.apply(this, arguments);
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
      var acc, accountTypeName, ignore, lc, ordpi, vendorInfo, _ref, _ref1;
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
        model: this.YodleeAccounts,
        isProfile: this.isProfile()
      });
      this.payPalAccounts = new EzBob.PayPalAccounts(this.model.get("paypalAccounts"));
      this.PayPalInfoView = new EzBob.PayPalInfoView({
        model: this.payPalAccounts
      });
      this.companyFilesAccounts = new EzBob.CompanyFilesAccounts(this.model.get("companyUploadAccounts"));
      this.companyFilesAccountInfoView = new EzBob.CompanyFilesAccountInfoView({
        model: this.companyFilesAccounts
      });
      _ref = EzBob.CgVendors.all();
      for (accountTypeName in _ref) {
        ignore = _ref[accountTypeName];
        lc = accountTypeName.toLowerCase();
        acc = new EzBob.CgAccounts([], {
          accountType: accountTypeName
        });
        this[lc + 'Accounts'] = acc;
        if (lc === 'hmrc') {
          this[lc + 'AccountInfoView'] = new EzBob.HmrcAccountInfoView({
            model: acc,
            companyRefNum: (this.fromCustomer('CompanyInfo') || {}).ExperianRefNum
          });
        } else {
          this[lc + 'AccountInfoView'] = new EzBob.CgAccountInfoView({
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
        },
        "CompanyFiles": {
          view: this.companyFilesAccountInfoView
        }
      };
      _ref1 = EzBob.CgVendors.all();
      for (accountTypeName in _ref1) {
        vendorInfo = _ref1[accountTypeName];
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
      'click .btn-showmore': 'toggleShowMoreAccounts',
      'click .btn-go-to-link-accounts': 'showLinkAccountsForm',
      'click .btn-take-quick-offer': 'takeQuickOffer',
      'click .btn-back-to-quick-offer': 'backToQuickOffer'
    };

    StoreInfoView.prototype.backToQuickOffer = function() {
      if (this.shouldRemoveQuickOffer()) {
        return this.storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove();
      } else {
        this.storeList.find('.link-accounts-form').addClass('hide');
        return this.storeList.find('.quick-offer-form').removeClass('hide');
      }
    };

    StoreInfoView.prototype.takeQuickOffer = function() {
      var xhr;
      xhr = $.post(window.gRootPath + 'CustomerDetails/TakeQuickOffer');
      xhr.done(function() {
        EzBob.App.trigger('clear');
        return setTimeout((function() {
          return window.location = window.gRootPath + 'Customer/Profile#GetCash';
        }), 500);
      });
      return false;
    };

    StoreInfoView.prototype.showLinkAccountsForm = function() {
      this.storeList.find('.quick-offer-form').addClass('hide');
      return this.storeList.find('.link-accounts-form').removeClass('hide');
    };

    StoreInfoView.prototype.render = function() {
      var accountsList, bFirst, grp, grpid, grpui, j, name, oTarget, relevantMpGroups, sActiveField, sBtnClass, sGroupClass, sPriorityField, shop, showMoreBtn, sortedShopsByPriority, store, storeTypeName, _i, _j, _k, _l, _len, _len1, _len2, _len3, _ref, _ref1, _ref2, _ref3;
      UnBlockUi();
      this.mpGroups = {};
      _ref = EzBob.Config.MarketPlaceGroups;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        grp = _ref[_i];
        this.mpGroups[grp.Id] = grp;
        grp.ui = null;
      }
      _ref1 = EzBob.Config.MarketPlaces;
      for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
        j = _ref1[_j];
        storeTypeName = j.Name === "Pay Pal" ? "paypal" : j.Name;
        if (this.stores[storeTypeName]) {
          this.stores[storeTypeName].active = this.isProfile() ? (this.isOffline() ? j.ActiveDashboardOffline : j.ActiveDashboardOnline) : (this.isOffline() ? j.ActiveWizardOffline : j.ActiveWizardOnline);
          this.stores[storeTypeName].priority = this.isOffline() ? j.PriorityOffline : j.PriorityOnline;
          this.stores[storeTypeName].ribbon = j.Ribbon ? j.Ribbon : "";
          this.stores[storeTypeName].button = new EzBob.StoreButtonView({
            name: storeTypeName,
            mpAccounts: this.model,
            description: j.Description
          });
          this.stores[storeTypeName].button.ribbon = j.Ribbon ? j.Ribbon : "";
          this.stores[storeTypeName].mandatory = this.isOffline() ? j.MandatoryOffline : j.MandatoryOnline;
          this.stores[storeTypeName].groupid = j.Group != null ? j.Group.Id : 0;
        }
      }
      _ref2 = this.stores;
      for (name in _ref2) {
        store = _ref2[name];
        store.button.on("selected", this.connect, this);
        store.view.on("completed", _.bind(this.completed, this, store.button.name));
        store.view.on("back", this.back, this);
        store.button.on("ready", this.ready, this);
      }
      this.canContinue();
      if (this.renderExecuted) {
        if (this.shouldRemoveQuickOffer()) {
          this.storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove();
          this.storeList.find('.link-accounts-form').removeClass('hide');
        }
      } else {
        this.storeList.find('.quick-offer-form, .link-accounts-form').addClass('hide');
        if (this.shouldShowQuickOffer()) {
          this.storeList.find('.quick-offer-form').removeClass('hide');
          this.renderQuickOfferForm();
        } else {
          this.storeList.find('.link-accounts-form').removeClass('hide');
          if (this.shouldRemoveQuickOffer()) {
            this.storeList.find('.quick-offer-form, .btn-back-to-quick-offer').remove();
          }
        }
      }
      this.renderExecuted = true;
      this.showOrRemove();
      accountsList = this.storeList.find('.accounts-list');
      accountsList.empty();
      sActiveField = 'Active' + (this.isProfile() ? 'Dashboard' : 'Wizard') + (this.isOffline() ? 'Offline' : 'Online');
      sPriorityField = 'Priority' + (this.isOffline() ? 'Offline' : 'Online');
      relevantMpGroups = [];
      _ref3 = this.mpGroups;
      for (grpid in _ref3) {
        grp = _ref3[grpid];
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
      showMoreBtn = this.$el.find('.btn-showmore');
      showMoreBtn.hover((function(evt) {
        return $('.onhover', this).animate({
          top: 0,
          opacity: 1
        });
      }), (function(evt) {
        return $('.onhover', this).animate({
          top: '60px',
          opacity: 0
        });
      }));
      return this;
    };

    StoreInfoView.prototype.renderQuickOfferForm = function() {
      this.storeList.find('.immediate-offer .amount, .quick-offer-amount').text(EzBob.formatPoundsNoDecimals(this.quickOffer.Amount));
      this.storeList.find('.immediate-offer .term').text(this.quickOffer.ImmediateTerm + ' months');
      this.storeList.find('.immediate-offer .interest-rate .value').text(EzBob.formatPercentsWithDecimals(this.quickOffer.ImmediateInterestRate));
      this.setQuickOfferFormOnHover(this.storeList.find('.immediate-offer .btn-take-quick-offer'));
      this.storeList.find('.potential-offer .amount').text(EzBob.formatPoundsNoDecimals(this.quickOffer.PotentialAmount));
      this.storeList.find('.potential-offer .term').text('up to ' + this.quickOffer.PotentialTerm + ' months');
      this.storeList.find('.potential-offer .interest-rate .value').text(EzBob.formatPercentsWithDecimals(this.quickOffer.PotentialInterestRate));
      return this.setQuickOfferFormOnHover(this.storeList.find('.potential-offer .btn-go-to-link-accounts'));
    };

    StoreInfoView.prototype.setQuickOfferFormOnHover = function(oLinkBtn) {
      return oLinkBtn.hover((function(evt) {
        var nHeight, onHover;
        oLinkBtn = $(this);
        nHeight = oLinkBtn.outerHeight();
        nHeight = oLinkBtn.outerWidth();
        onHover = oLinkBtn.parent().find('.onhover');
        onHover.css({
          position: 'absolute',
          top: nHeight,
          height: nHeight,
          left: 0,
          width: nHeight,
          display: 'block'
        });
        return onHover.animate({
          top: 0,
          opacity: 1
        });
      }), (function(evt) {
        var nHeight, onHover;
        oLinkBtn = $(this);
        onHover = oLinkBtn.parent().find('.onhover');
        nHeight = oLinkBtn.outerHeight();
        return onHover.animate({
          top: nHeight,
          opacity: 0
        });
      }));
    };

    StoreInfoView.prototype.shouldRemoveQuickOffer = function() {
      if (!this.quickOffer) {
        return true;
      }
      return moment.utc().diff(moment.utc(this.quickOffer.ExpirationDate)) > 0;
    };

    StoreInfoView.prototype.shouldShowQuickOffer = function() {
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
      this.storeList.find('.marketplace-button.show-more').show();
      if (isOffline) {
        sShow = '.offline_entry_message';
        sRemove = '.online_entry_message';
        this.storeList.find('.importantnumber').text('£150,000');
        if (isProfile) {
          this.storeList.find('.marketplace-button.show-more').hide();
          this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
        } else {
          this.storeList.find('.btn-showmore').show();
        }
      } else {
        sShow = '.online_entry_message';
        sRemove = '.offline_entry_message';
        this.storeList.find('.marketplace-button.show-more').hide();
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

    StoreInfoView.prototype.toggleShowMoreAccounts = function() {
      var oBtn;
      oBtn = this.storeList.find('.btn-showmore');
      if (oBtn.attr('data-current') === 'more') {
        return this.showMoreAccounts();
      } else {
        return this.showLessAccounts();
      }
    };

    StoreInfoView.prototype.showLessAccounts = function() {
      var oBtn;
      oBtn = this.storeList.find('.btn-showmore');
      oBtn.attr('data-current', 'more');
      oBtn.find('.caption').text('Show more account types');
      oBtn.find('.rotate90').html('&laquo;');
      oBtn.find('.onhover').text('Show more data source connectors');
      this.storeList.find('.AddMoreRuleBottom').addClass('hide');
      this.storeList.find('.marketplace-button-more, .marketplace-group.following').hide();
      return this.storeList.find('.marketplace-button').not('.show-more, .marketplace-button-less').css('display', 'none');
    };

    StoreInfoView.prototype.showMoreAccounts = function() {
      var oBtn;
      oBtn = this.storeList.find('.btn-showmore');
      oBtn.attr('data-current', 'less');
      oBtn.find('.caption').text('Show less account types');
      oBtn.find('.rotate90').html('&raquo;');
      oBtn.find('.onhover').text('Show less data source connectors');
      this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
      this.storeList.find('.marketplace-button-more, .marketplace-group.following').show();
      return this.storeList.find('.marketplace-button').not('.show-more').css('display', 'table');
    };

    StoreInfoView.prototype.canContinue = function() {
      var canContinue, hasEbay, hasFilledShops, hasPaypal, mpType, oStore, sAttrName, _ref;
      hasFilledShops = false;
      _ref = this.stores;
      for (mpType in _ref) {
        oStore = _ref[mpType];
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
          canContinue = $('#allowFinishWizardWithoutMarketplaces').data(sAttrName).toLowerCase() === 'true';
        }
      }
      this.storeList.find('.continue').toggleClass('disabled', !canContinue);
      this.storeList.find('.AddMoreRule').toggleClass('hide', canContinue);
      return hasFilledShops;
    };

    StoreInfoView.prototype.extractBtnClass = function(jqTarget) {
      var sClass;
      sClass = 'pull-left';
      if ($('.marketplace-button-less', jqTarget).length < 3) {
        sClass += ' marketplace-button-less';
      } else {
        sClass += ' marketplace-button-more';
        if (this.isOffline() && !this.isProfile() && $('.marketplace-button-more', jqTarget).length === 0) {
          sClass += ' marketplace-button-more-first';
        }
      }
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
      return false;
    };

    StoreInfoView.prototype.setFocus = function(storeName) {
      $.colorbox.close();
      switch (storeName) {
        case "EKM":
          return this.$el.find("#ekm_login").focus();
        case "PayPoint":
          return this.$el.find("#payPoint_login").focus();
        default:
          if (EzBob.CgVendors.pure()[storeName]) {
            return $('.form_field', '#' + storeName.toLowerCase() + 'Account').first().focus();
          }
      }
    };

    StoreInfoView.prototype.setDocumentTitle = function(view) {
      var title;
      title = view.getDocumentTitle();
      if (title) {
        return $(document).attr("title", "Step 4: " + title + " | EZBOB");
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
      return false;
    };

    StoreInfoView.prototype.next = function() {
      var btn, xhr;
      btn = this.$el.find(".continue");
      if (btn.hasClass("disabled")) {
        return;
      }
      BlockUi('on');
      btn.addClass('disabled');
      xhr = $.post(window.gRootPath + 'CustomerDetails/LinkAccountsComplete');
      xhr.done(function() {
        EzBob.App.trigger("clear");
        return window.location = window.gRootPath + 'CustomerDetails/Dashboard';
      });
      xhr.error(function() {
        return UnBlockUi();
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

    StoreInfoView.prototype.shopConnected = function(name) {
      var _this = this;
      return this.model.get('customer').safeFetch().done(function() {
        _this.stores[name].button.update(_this.fromCustomer('mpAccounts'));
        return _this.render();
      });
    };

    return StoreInfoView;

  })(Backbone.View);

}).call(this);
