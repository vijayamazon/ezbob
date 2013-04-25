(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.AmazonStoreInfoView = Backbone.View.extend({
    initialize: function() {
      return EzBob.CT.bindShopToCT(this, "amazon");
    },
    render: function() {
      this.$el.html($("#amazon-store-info").html());
      this.form = this.$el.find(".AmazonForm");
      this.validator = EzBob.validateAmazonForm(this.form);
      this.inputChanged();
      return this;
    },
    events: {
      "click a.connect-amazon": "connect",
      "click a.back": "back",
      "click .screenshots": "runTutorial",
      "keyup input[type=\"text\"]": "inputChanged",
      "change input[type=\"text\"]": "inputChanged",
      "click a.print": "print"
    },
    inputChanged: function() {
      var marketplaceId, merchantId;

      marketplaceId = this.$el.find("#amazonMarketplaceId").val();
      merchantId = this.$el.find("#amazonMerchantId").val();
      if (marketplaceId.length < 10 || marketplaceId.length > 15 || merchantId.length < 10 || merchantId.length > 15 || !this.validator.form()) {
        this.$el.find("a.connect-amazon").addClass("disabled");
        return;
      }
      return this.$el.find("a.connect-amazon").removeClass("disabled");
    },
    runTutorial: function() {
      var content, div;

      div = $("<div/>");
      content = $("#amazon-gallery-container");
      div.html(content.html());
      div.find(".amazon-tutorial-slider").attr("id", "amazon-tutorial-slider" + (new Date().getTime())).show();
      div.dialog({
        width: "960",
        height: "573",
        modal: true,
        draggable: false,
        resizable: false,
        close: function() {
          return div.empty();
        },
        dialogClass: "amazon-tutor-dlg",
        title: "Link Your Amazon Shop to EZBOB"
      });
      div.find(".amazon-tutorial-slider").coinslider({
        width: 930,
        height: 471,
        delay: 1000000,
        effect: "rain",
        sDelay: 30,
        titleSpeed: 50,
        links: false
      });
      return false;
    },
    back: function() {
      this.trigger("back");
      return false;
    },
    connect: function(e) {
      var marketplaceId, merchantId, that;

      if (!this.validator.form()) {
        EzBob.App.trigger("error", "The fields Merchant ID or Marketplace ID are not filled");
        return false;
      }
      marketplaceId = this.$el.find("#amazonMarketplaceId");
      merchantId = this.$el.find("#amazonMerchantId");
      that = this;
      if (this.$el.find("a.connect-amazon").hasClass("disabled")) {
        return false;
      }
      this.blockBtn(true);
      $.post(window.gRootPath + "Customer/AmazonMarketplaces/ConnectAmazon", {
        marketplaceId: marketplaceId.val(),
        merchantId: merchantId.val()
      }).success(function(result) {
        that.blockBtn(false);
        if (result.error) {
          EzBob.App.trigger("error", result.error);
          that.trigger("back");
          return;
        }
        EzBob.App.trigger("info", result.msg);
        that.trigger("completed");
        marketplaceId.val("");
        return merchantId.val("");
      }).error(function() {
        return EzBob.App.trigger("error", "Amazon account adding failed");
      });
      return false;
    },
    print: function() {
      window.print();
      return false;
    },
    blockBtn: function(isBlock) {
      BlockUi((isBlock ? "on" : "off"));
      return this.$el.find("connect-amazon").toggleClass("disabled", isBlock);
    }
  });

}).call(this);
