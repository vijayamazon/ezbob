(function() {
  var root, _ref, _ref1, _ref2,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

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
      "keyup input[type='text']": "inputChanged",
      "change input[type='text']": "inputChanged",
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
      var marketplaceId, merchantId,
        _this = this;

      if (!this.validator.form()) {
        EzBob.App.trigger("error", "The Fields Merchant ID or Marketplace ID are not Filled");
        return false;
      }
      marketplaceId = this.$el.find("#amazonMarketplaceId");
      merchantId = this.$el.find("#amazonMerchantId");
      if (this.$el.find("a.connect-amazon").hasClass("disabled")) {
        return false;
      }
      this.blockBtn(true);
      $.post(window.gRootPath + "Customer/AmazonMarketplaces/ConnectAmazon", {
        marketplaceId: marketplaceId.val(),
        merchantId: merchantId.val()
      }).success(function(result) {
        _this.blockBtn(false);
        if (result.error) {
          EzBob.App.trigger("error", result.error);
          _this.trigger("back");
          return;
        }
        EzBob.App.trigger("info", result.msg);
        _this.trigger("completed");
        _this.trigger('back');
        marketplaceId.val("");
        return merchantId.val("");
      }).error(function() {
        return EzBob.App.trigger("error", "Amazon Account Adding Failed");
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

  EzBob.AmazonStoreModel = (function(_super) {
    __extends(AmazonStoreModel, _super);

    function AmazonStoreModel() {
      _ref = AmazonStoreModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    AmazonStoreModel.prototype.defaults = {
      marketplaceId: null
    };

    return AmazonStoreModel;

  })(Backbone.Model);

  EzBob.AmazonStoreModels = (function(_super) {
    __extends(AmazonStoreModels, _super);

    function AmazonStoreModels() {
      _ref1 = AmazonStoreModels.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    AmazonStoreModels.prototype.model = EzBob.AmazonStoreModel;

    AmazonStoreModels.prototype.url = "" + window.gRootPath + "Customer/AmazonMarketPlaces";

    return AmazonStoreModels;

  })(Backbone.Collection);

  EzBob.AmazonButtonView = (function(_super) {
    __extends(AmazonButtonView, _super);

    function AmazonButtonView() {
      _ref2 = AmazonButtonView.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    AmazonButtonView.prototype.initialize = function() {
      return AmazonButtonView.__super__.initialize.call(this, {
        name: "Amazon",
        logoText: "",
        shops: this.model
      });
    };

    AmazonButtonView.prototype.update = function() {
      return this.model.fetch();
    };

    return AmazonButtonView;

  })(EzBob.StoreButtonView);

}).call(this);
