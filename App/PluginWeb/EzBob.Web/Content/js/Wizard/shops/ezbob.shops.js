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
      return oCustomer.get(sPropName);
    };

    StoreInfoView.prototype.initialize = function() {
      var acc, accountTypeName, aryCGAccounts, ignore, lc, ordpi, vendorInfo;
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
      'click .btn-showmore': 'showMoreAccounts'
    };

    StoreInfoView.prototype.render = function() {
      var accountsList, bFirst, grp, grpid, grpui, j, name, oTarget, relevantMpGroups, sActiveField, sBtnClass, sGroupClass, sPriorityField, shop, sortedShopsByPriority, store, storeTypeName, _i, _j, _k, _l, _len, _len1, _len2, _len3, _ref, _ref1, _ref2, _ref3;
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
            mpAccounts: this.model
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
      this.storeList.appendTo(this.$el);
      EzBob.UiAction.registerView(this);
      this.amazonMarketplaces.trigger("reset");
      this.ebayStores.trigger("reset");
      this.$el.find("img[rel]").setPopover("left");
      this.$el.find("li[rel]").setPopover("left");
      return this;
    };

    StoreInfoView.prototype.showOrRemove = function() {
      var sRemove, sShow;
      $(this.storeList).find('.back-store').remove();
      sShow = '';
      sRemove = '';
      this.storeList.find('.btn-showmore').show();
      if (this.isOffline()) {
        sShow = '.offline_entry_message';
        sRemove = '.online_entry_message';
        this.storeList.find('.importantnumber').text('£150,000');
        if (this.isProfile()) {
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
      if (this.isProfile()) {
        sShow = '.profile_message';
        sRemove = '.wizard_message';
      } else {
        sShow = '.wizard_message';
        sRemove = '.profile_message';
      }
      this.storeList.find(sShow).show();
      return this.storeList.find(sRemove).remove();
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
          canContinue = $('#allowFinishWizardWithoutMarketplaces').attr(sAttrName).toLowerCase() === 'true';
        }
      }
      this.storeList.find('.continue').toggleClass('disabled', !canContinue);
      return this.storeList.find('.AddMoreRule').toggleClass('hide', canContinue);
    };

    StoreInfoView.prototype.showMoreAccounts = function() {
      this.storeList.find('.btn-showmore').hide();
      this.storeList.find('.AddMoreRuleBottom').removeClass('hide');
      return this.storeList.find('.marketplace-button-more, .marketplace-group.following').show();
    };

    StoreInfoView.prototype.extractBtnClass = function(jqTarget) {
      var sClass;
      sClass = jqTarget.attr('data-last-button-class') || 'pull-right';
      sClass = sClass === 'pull-right' ? 'pull-left' : 'pull-right';
      jqTarget.attr('data-last-button-class', sClass);
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
      this.render();
      return this.trigger("completed");
    };

    StoreInfoView.prototype.back = function() {
      this.$el.find(">div").hide();
      this.storeList.show();
      $(document).attr("title", this.oldTitle);
      this.updateEarnedPoints();
      return false;
    };

    StoreInfoView.prototype.next = function() {
      if (this.$el.find(".continue").hasClass("disabled")) {
        return;
      }
      $.post(window.gRootPath + 'CustomerDetails/LinkAccountsComplete');
      EzBob.App.trigger("clear");
      EzBob.App.GA.trackEventReditect(window.gRootPath + 'Customer/Profile/Index', 'Wizard Complete', 'Go To account', 'Awaiting Approval');
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
      return this.model.safeFetch().done(function() {
        _this.stores[name].button.update(_this.fromCustomer('mpAccounts'));
        _this.updateEarnedPoints();
        return _this.render();
      });
    };

    return StoreInfoView;

  })(Backbone.View);

}).call(this);
