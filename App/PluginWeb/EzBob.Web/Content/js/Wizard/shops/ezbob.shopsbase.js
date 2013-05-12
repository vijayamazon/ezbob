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

    StoreInfoBaseView.prototype.initialize = function() {
      var name, store, _ref1;

      this.storeList = $($("#store-info").html());
      this.isReady = false;
      _ref1 = this.stores;
      for (name in _ref1) {
        store = _ref1[name];
        store.button.on("selected", this.connect, this);
        store.view.on("completed", _.bind(this.completed, this, store.button.name));
        store.view.on("back", this.back, this);
        store.button.on("ready", this.ready, this);
      }
      EzBob.App.on("ct:storebase.shop.connected", this.render, this);
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

    StoreInfoBaseView.prototype.render = function() {
      var accountsList, hasEbay, hasFilledShops, hasPaypal, shop, sortedShopsByNumOfShops, sortedShopsByPriority, that, _i, _len;

      $.colorbox.close();
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
      this.$el.find(".next").toggleClass("disabled", !hasFilledShops || (hasEbay && !hasPaypal));
      for (_i = 0, _len = sortedShopsByNumOfShops.length; _i < _len; _i++) {
        shop = sortedShopsByNumOfShops[_i];
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
      $.colorbox.close();
      switch (storeName) {
        case "EKM":
          return this.$el.find("#ekm_login").focus();
        case "Volusion":
          return this.$el.find("#volusion_url").focus();
        case "Play":
          return this.$el.find("#play_name").focus();
        case "PayPoint":
          return this.$el.find("#payPoint_login").focus();
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
