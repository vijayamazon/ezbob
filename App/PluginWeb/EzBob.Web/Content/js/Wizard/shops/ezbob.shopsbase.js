(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoBaseView = (function(_super) {

    __extends(StoreInfoBaseView, _super);

    function StoreInfoBaseView() {
      return StoreInfoBaseView.__super__.constructor.apply(this, arguments);
    }

    StoreInfoBaseView.prototype.isOffline = false;

    StoreInfoBaseView.prototype.initialize = function() {
      var name, ordpi, store, _ref;
      this.allowFinishOnlineWizardWithoutMarketplaces = $('#allowFinishWizardWithoutMarketplaces').attr('online').toLowerCase() === 'true';
      this.allowFinishOfflineWizardWithoutMarketplaces = $('#allowFinishWizardWithoutMarketplaces').attr('offline').toLowerCase() === 'true';
      if (typeof ordpi === 'undefined') {
        ordpi = Math.random() * 10000000000000000;
      }
      this.storeList = $(_.template($("#store-info").html(), {
        ordpi: ordpi
      }));
      this.isReady = false;
      _ref = this.stores;
      for (name in _ref) {
        store = _ref[name];
        store.button.on("selected", this.connect, this);
        store.view.on("completed", _.bind(this.completed, this, store.button.name));
        store.view.on("back", this.back, this);
        store.button.on("ready", this.ready, this);
      }
      return EzBob.App.on("ct:storebase." + this.name + ".connect", this.connect, this);
    };

    StoreInfoBaseView.prototype.completed = function(name) {
      this.shopConnected(name);
      this.render();
      return this.trigger("completed");
    };

    StoreInfoBaseView.prototype.back = function() {
      this.$el.find(">div").hide();
      this.storeList.show();
      $(document).attr("title", this.oldTitle);
      this.updateEarnedPoints();
      return false;
    };

    StoreInfoBaseView.prototype.next = function() {
      if (this.$el.find(".continue").hasClass("disabled")) {
        return;
      }
      $.post(window.gRootPath + 'CustomerDetails/LinkAccountsComplete');
      this.trigger("next");
      EzBob.App.trigger("clear");
      return false;
    };

    StoreInfoBaseView.prototype.ready = function(name) {
      this.trigger("ready", name);
      if (!this.isReady) {
        this.isReady = true;
        return this.$el.find(".continue").show();
      }
    };

    StoreInfoBaseView.prototype.updateEarnedPoints = function() {
      return $.getJSON("" + window.gRootPath + "Customer/Wizard/EarnedPointsStr").done(function(data) {
        if (data.EarnedPointsStr) {
          return $('#EarnedPoints').text(data.EarnedPointsStr);
        }
      });
    };

    StoreInfoBaseView.prototype.shopConnected = function(name) {
      var _this = this;
      return this.model.safeFetch().done(function() {
        _this.stores[name].button.update(_this.model.get('mpAccounts'));
        _this.updateEarnedPoints();
        return _this.render();
      });
    };

    StoreInfoBaseView.prototype.render = function() {
      var accountsList, canContinue, ebayPaypalRuleMessageVisible, foundAllMandatories, hasEbay, hasFilledShops, hasHmrc, hasPaypal, isProfile, key, sRemove, sShow, shop, sortedShopsByNumOfShops, sortedShopsByPriority, _i, _j, _len, _len1, _ref;
      hasHmrc = this.stores.HMRC.button.shops.length > 0;
      sShow = '';
      sRemove = '';
      sortedShopsByPriority = _.sortBy(this.stores, function(s) {
        return s.priority;
      });
      sortedShopsByNumOfShops = _.sortBy(sortedShopsByPriority, function(s) {
        return -s.button.shops.length;
      });
      hasFilledShops = sortedShopsByNumOfShops[0].button.shops.length > 0;
      hasEbay = this.stores.eBay.button.shops.length > 0;
      hasPaypal = this.stores.paypal.button.shops.length > 0;
      ebayPaypalRuleMessageVisible = hasEbay && !hasPaypal;
      this.$el.find(".eBayPaypalRule").toggleClass("hide", !ebayPaypalRuleMessageVisible);
      foundAllMandatories = true;
      _ref = Object.keys(this.stores);
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        key = _ref[_i];
        if (this.stores[key].button.shops.length === 0 && this.stores[key].mandatory) {
          foundAllMandatories = false;
        }
      }
      isProfile = this.model.get("isProfile");
      if (!isProfile) {
        $(this.storeList).find(".back-store").hide();
      }
      canContinue = isProfile || ((hasFilledShops && (!hasEbay || (hasEbay && hasPaypal)) && foundAllMandatories) || (this.isOffline && this.allowFinishOfflineWizardWithoutMarketplaces) || (!this.isOffline && this.allowFinishOnlineWizardWithoutMarketplaces));
      this.storeList.find('.continue').toggleClass('disabled', !canContinue);
      this.handleMandatoryText(hasFilledShops, canContinue, ebayPaypalRuleMessageVisible);
      if (this.isOffline) {
        sShow = '.offline_entry_message';
        sRemove = '.online_entry_message';
        if (hasHmrc) {
          this.storeList.find('.btn-continue').text("Continue").removeClass('disabled');
        } else {
          this.storeList.find('.btn-continue').text("Skip, I\'ll do it later").removeClass('disabled');
        }
        this.storeList.find('.importantnumber').text('£150,000');
      } else {
        sShow = '.online_entry_message';
        sRemove = '.offline_entry_message';
      }
      this.storeList.find(sShow).show();
      this.storeList.find(sRemove).remove();
      if (isProfile) {
        sShow = ".profile_message";
        sRemove = ".wizard_message";
      } else {
        sShow = ".wizard_message";
        sRemove = ".profile_message";
      }
      this.storeList.find(sShow).show();
      this.storeList.find(sRemove).remove();
      accountsList = this.storeList.find(".accounts-list");
      for (_j = 0, _len1 = sortedShopsByNumOfShops.length; _j < _len1; _j++) {
        shop = sortedShopsByNumOfShops[_j];
        if (shop.active) {
          shop.button.render().$el.appendTo(accountsList);
        }
      }
      this.storeList.appendTo(this.$el);
      EzBob.UiAction.registerView(this);
      return this;
    };

    StoreInfoBaseView.prototype.events = {
      "click a.connect-store": "close",
      "click a.continue": "next"
    };

    StoreInfoBaseView.prototype.handleMandatoryText = function(hasFilledShops, canContinue, ebayPaypalRuleMessageVisible) {
      var first, foundAllMandatories, key, shouldHide, text, _i, _len, _ref;
      shouldHide = !hasFilledShops || canContinue || ebayPaypalRuleMessageVisible;
      if (!shouldHide) {
        first = true;
        text = 'Please add the following accounts in order to continue: ';
        _ref = Object.keys(this.stores);
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          key = _ref[_i];
          if (this.stores[key].button.shops.length === 0 && this.stores[key].mandatory) {
            foundAllMandatories = false;
            if (!first) {
              text += ', ';
            }
            first = false;
            text += key;
          }
        }
        this.storeList.find('.AddMoreRule').text(text);
      }
      return this.storeList.find('.AddMoreRule').toggleClass('hide', shouldHide);
    };

    StoreInfoBaseView.prototype.connect = function(storeName) {
      var oFieldStatusIcons, storeView;
      EzBob.CT.recordEvent("ct:storebase." + this.name + ".connect", storeName);
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

    StoreInfoBaseView.prototype.setFocus = function(storeName) {
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

    StoreInfoBaseView.prototype.setDocumentTitle = function(view) {
      var title;
      title = view.getDocumentTitle();
      if (title) {
        return $(document).attr("title", "Step 2: " + title + " | EZBOB");
      }
    };

    StoreInfoBaseView.prototype.close = function() {
      return this;
    };

    return StoreInfoBaseView;

  })(Backbone.View);

}).call(this);
