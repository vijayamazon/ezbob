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
      var that;

      that = this;
      _.each(this.stores, function(store) {
        store.button.on("selected", that.connect, that);
        store.view.on("completed", _.bind(that.completed, that, store.button.name));
        store.view.on("back", that.back, that);
        return store.button.on("ready", that.ready, that);
      });
      this.storeList = $($("#store-info").html());
      EzBob.App.on("ct:storebase." + this.name + ".connect", this.connect, this);
      return this.isReady = false;
    };

    StoreInfoBaseView.prototype.completed = function(name) {
      this.stores[name].button.update();
      this.$el.empty();
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
<<<<<<< HEAD
      var accountsList, row, shopsList, that;
=======
      var buttonList, row, that;
>>>>>>> Volusion login form

      that = this;
      shopsList = this.storeList.find(".shops-list");
      accountsList = this.storeList.find(".accounts-list");
      row = null;
      _.each(this.stores, function(store) {
        if (!store.active) {
          return;
        }
<<<<<<< HEAD
        if (!store.isShop) {
          return;
        }
        store.button.render().$el.appendTo(shopsList);
        store.view.render().$el.hide().appendTo(that.$el);
        return console.log(store.button.model);
      });
      _.each(this.stores, function(store) {
        if (!store.active) {
          return;
        }
        if (store.isShop) {
          return;
        }
        store.button.render().$el.appendTo(accountsList);
=======
        store.button.render().$el.appendTo(buttonList);
>>>>>>> Volusion login form
        return store.view.render().$el.hide().appendTo(that.$el);
      });
      this.storeList.appendTo(this.$el);
      if (this.stores["bank-account"] != null ? this.stores["bank-account"].button.model.get("bankAccountAdded") : void 0) {
        that.ready();
      }
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
      EzBob.CT.recordEvent("ct:storebase." + this.name + ".connect", storeName);
      this.$el.find(">div").hide();
      this.stores[storeName].view.$el.show();
      this.oldTitle = $(document).attr("title");
      this.setDocumentTitle(storeName);
      this.setFocus(storeName);
      return false;
    };

    StoreInfoBaseView.prototype.setFocus = function(storeName) {
<<<<<<< HEAD
=======
      var sText;

      console.log("setFocus", storeName);
>>>>>>> Volusion login form
      switch (storeName) {
        case "EKM":
          return this.$el.find("#ekm_login").focus();
        case "Volusion":
          sText = $("#header_description").text().trim();
          if ("" === this.$el.find("#volusion_login").val()) {
            this.$el.find("#volusion_login").val(sText.substr(0, sText.indexOf(" ")));
          }
          return this.$el.find("#volusion_shopname").focus();
        case "payPoint":
          return this.$el.find("#payPoint_login").focus();
        case "bank-account":
          return this.$el.find("#AccountNumber").focus();
      }
    };

    StoreInfoBaseView.prototype.setDocumentTitle = function(storeName) {
      switch (storeName) {
        case "amazon":
          return $(document).attr("title", "Wizard 2 Amazon: Link Your Amazon Shop | EZBOB");
        case "ebay":
          return $(document).attr("title", "Wizard 2 Ebay: Link Your Ebay Shop | EZBOB");
        case "bank-account":
          return $(document).attr("title", "Wizard 3 Bank: Bank Account Details | EZBOB");
        case "paypal":
          return $(document).attr("title", "Wizard 3 PayPal: Link Your PayPal Account | EZBOB");
      }
    };

    StoreInfoBaseView.prototype.close = function() {
      return this;
    };

    return StoreInfoBaseView;

  })(Backbone.View);

}).call(this);
