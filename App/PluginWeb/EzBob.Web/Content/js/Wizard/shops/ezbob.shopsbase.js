(function() {
  var root, that;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreInfoBaseView = Backbone.View.extend({
    initialize: function() {}
  }, that = this, _.each(this.stores, function(store) {
    store.button.on("selected", that.connect, that);
    store.view.on("completed", _.bind(that.completed, that, store.button.name));
    store.view.on("back", that.back, that);
    return store.button.on("ready", that.ready, that);
  }), this.storeList = $($("#store-info").html()), EzBob.App.on("ct:storebase." + this.name + ".connect", this.connect, this), this.isReady = false, {
    completed: function(name) {
      this.stores[name].button.update();
      this.$el.find(">div").hide();
      this.storeList.show();
      return this.trigger("completed");
    },
    back: function() {
      this.$el.find(">div").hide();
      this.storeList.show();
      $(document).attr("title", this.oldTitle);
      return false;
    },
    next: function() {
      this.trigger("next");
      EzBob.App.trigger("clear");
      return false;
    },
    ready: function(name) {
      this.trigger("ready", name);
      if (!this.isReady) {
        this.isReady = true;
        return this.$el.find(".next").show();
      }
    },
    render: function() {
      var buttonList, i, row;

      that = this;
      buttonList = this.storeList.find(".buttons-list");
      row = null;
      i = 0;
      _.each(this.stores, function(store) {
        if (!store.active) {
          return;
        }
        if ((i % 2) === 0) {
          row = $("<div class='row-fluid'/>");
          row.appendTo(buttonList);
        }
        i++;
        store.button.render().$el.appendTo(row);
        return store.view.render().$el.hide().appendTo(that.$el);
      });
      this.storeList.appendTo(this.$el);
      if (this.stores["bank-account"] != null ? this.stores["bank-account"].button.model.get("bankAccountAdded") : void 0) {
        that.ready();
      }
      return this;
    },
    events: {
      "click a.connect-store": "close",
      "click a.next": "next",
      "click a.back-step": "previousClick"
    },
    previousClick: function() {
      this.trigger("previous");
      return false;
    },
    connect: function(storeName) {
      EzBob.CT.recordEvent("ct:storebase." + this.name + ".connect", storeName);
      this.$el.find(">div").hide();
      this.stores[storeName].view.$el.show();
      this.oldTitle = $(document).attr("title");
      this.setDocumentTitle(storeName);
      this.setFocus(storeName);
      return false;
    },
    setFocus: function(storeName) {
      switch (storeName) {
        case "ekm":
          return this.$el.find("#ekm_login").focus();
        case "volusion":
          return this.$el.find("#volusion_login").focus();
        case "payPoint":
          return this.$el.find("#payPoint_login").focus();
        case "bank-account":
          return this.$el.find("#AccountNumber").focus();
      }
    },
    setDocumentTitle: function(storeName) {
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
    },
    close: function() {}
  });

}).call(this);
