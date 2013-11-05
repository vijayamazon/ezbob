(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoBaseView = (function(_super) {
    __extends(StoreInfoBaseView, _super);

    function StoreInfoBaseView() {
      _ref = StoreInfoBaseView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    StoreInfoBaseView.prototype.isOffline = false;

    StoreInfoBaseView.prototype.initialize = function() {
      var name, ordpi, store, that, _ref1;

      this.allowFinishOnlineWizardWithoutMarketplaces = false;
      this.allowFinishOfflineWizardWithoutMarketplaces = false;
      that = this;
      $.getJSON("" + window.gRootPath + "Customer/Wizard/GetConfigurations").done(function(res) {
        that.allowFinishOnlineWizardWithoutMarketplaces = res.allowFinishOnlineWizardWithoutMarketplaces;
        return that.allowFinishOfflineWizardWithoutMarketplaces = res.allowFinishOfflineWizardWithoutMarketplaces;
      });
      if (typeof ordpi === 'undefined') {
        ordpi = Math.random() * 10000000000000000;
      }
      this.storeList = $(_.template($("#store-info").html(), {
        ordpi: ordpi
      }));
      this.isReady = false;
      _ref1 = this.stores;
      for (name in _ref1) {
        store = _ref1[name];
        store.button.on("selected", this.connect, this);
        store.view.on("completed", _.bind(this.completed, this, store.button.name));
        store.view.on("back", this.back, this);
        store.button.on("ready", this.ready, this);
      }
      EzBob.App.on("ct:storebase.shop.connected", this.shopConnected, this);
      return EzBob.App.on("ct:storebase." + this.name + ".connect", this.connect, this);
    };

    StoreInfoBaseView.prototype.completed = function(name) {
      this.stores[name].button.update();
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
      if (this.$el.find(".next").hasClass("disabled")) {
        return;
      }
      this.trigger("next");
      EzBob.App.trigger("clear");
      return false;
    };

    StoreInfoBaseView.prototype.ready = function(name) {
      this.trigger("ready", name);
      if (!this.isReady) {
        this.isReady = true;
        return this.$el.find(".next").show();
      }
    };

    StoreInfoBaseView.prototype.updateEarnedPoints = function() {
      return $.getJSON("" + window.gRootPath + "Customer/Wizard/EarnedPointsStr").done(function(data) {
        if (data.EarnedPointsStr) {
          return $('#EarnedPoints').text(data.EarnedPointsStr);
        }
      });
    };

    StoreInfoBaseView.prototype.shopConnected = function() {
      this.updateEarnedPoints();
      return this.render();
    };

    StoreInfoBaseView.prototype.render = function() {
      var accountsList, canContinue, foundAllMandatories, hasEbay, hasFilledShops, hasHmrc, hasPaypal, key, shop, sortedShopsByNumOfShops, sortedShopsByPriority, that, _i, _j, _len, _len1, _ref1;

      hasHmrc = this.stores.HMRC.button.model.length > 0;
      if (this.isOffline) {
        if (hasHmrc) {
          this.$el.find('.entry_message').empty().append('The more you link, the more funds you can get.');
        } else {
          this.$el.find('.entry_message').empty().append('You must <strong>link</strong> or <strong>upload</strong> your ').append($('<span class="green btn">HM Revenue & Customs</span>').click(function() {
            return EzBob.App.trigger('ct:storebase.shops.connect', 'HMRC');
          })).append(' account data').append('<br />').append('to be approved for a loan.');
        }
        this.$el.find('.importantnumber').text('£200,000');
      }
      that = this;
      accountsList = this.storeList.find(".accounts-list");
      sortedShopsByPriority = _.sortBy(this.stores, function(s) {
        return s.priority;
      });
      sortedShopsByNumOfShops = _.sortBy(sortedShopsByPriority, function(s) {
        return -s.button.model.length;
      });
      hasFilledShops = sortedShopsByNumOfShops[0].button.model.length > 0;
      hasEbay = this.stores.eBay.button.model.length > 0;
      hasPaypal = this.stores.paypal.button.model.length > 0;
      this.$el.find(".eBayPaypalRule").toggleClass("hide", !hasEbay || hasPaypal);
      foundAllMandatories = true;
      _ref1 = Object.keys(this.stores);
      for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
        key = _ref1[_i];
        if (this.stores[key].button.model.length === 0 && this.stores[key].mandatory) {
          foundAllMandatories = false;
        }
      }
      if (this.isOffline) {
        canContinue = (hasHmrc && foundAllMandatories) || this.allowFinishOfflineWizardWithoutMarketplaces;
        this.$el.find('.next').toggleClass('disabled', !canContinue);
        this.$el.find('.AddMoreRule').toggleClass('hide', !hasFilledShops || canContinue);
      } else {
        canContinue = (hasFilledShops && (!hasEbay || (hasEbay && hasPayPal)) && foundAllMandatories) || this.allowFinishOnlineWizardWithoutMarketplaces;
        this.$el.find(".next").toggleClass("disabled", !canContinue);
        this.$el.find('.AddMoreRule').toggleClass('hide', !hasFilledShops || canContinue);
      }
      for (_j = 0, _len1 = sortedShopsByNumOfShops.length; _j < _len1; _j++) {
        shop = sortedShopsByNumOfShops[_j];
        if (!shop.active) {
          continue;
        }
        shop.button.render().$el.appendTo(accountsList);
        shop.view.render().$el.hide().appendTo(that.$el);
      }
      this.storeList.appendTo(this.$el);
      return this;
    };

    StoreInfoBaseView.prototype.events = {
      "click a.connect-store": "close",
      "click a.next": "next",
      "click a.back-step": "previousClick"
    };

    StoreInfoBaseView.prototype.previousClick = function() {
      this.trigger("previous");
      return false;
    };

    StoreInfoBaseView.prototype.connect = function(storeName) {
      var storeView;

      EzBob.CT.recordEvent("ct:storebase." + this.name + ".connect", storeName);
      this.$el.find(">div").hide();
      storeView = this.stores[storeName].view;
      storeView.render();
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
