(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.AmazonStoreInfoView = (function(_super) {

    __extends(AmazonStoreInfoView, _super);

    function AmazonStoreInfoView() {
      return AmazonStoreInfoView.__super__.constructor.apply(this, arguments);
    }

    AmazonStoreInfoView.prototype.initialize = function() {
      return EzBob.CT.bindShopToCT(this, 'amazon');
    };

    AmazonStoreInfoView.prototype.render = function() {
      this.$el.html($('#amazon-store-info').html());
      this.form = this.$el.find('.AmazonForm');
      this.validator = EzBob.validateAmazonForm(this.form);
      this.marketplaceId = this.$el.find('#amazonMarketplaceId');
      this.merchantId = this.$el.find('#amazonMerchantId');
      this.marketplaceId.withoutSpaces();
      this.merchantId.withoutSpaces();
      EzBob.UiAction.registerView(this);
      return this;
    };

    AmazonStoreInfoView.prototype.events = {
      'click a.go-to-amazon': 'enableControls',
      'click a.connect-amazon': 'connect',
      'click a.back': 'back',
      'click .amazonscreenshot': 'runTutorial',
      'click a.print': 'print',
      'change input': 'inputChanged',
      'focusout input': 'inputChanged',
      'keyup input': 'inputChanged'
    };

    AmazonStoreInfoView.prototype.enableControls = function() {
      this.$el.find('#amazonMarketplaceId, #amazonMerchantId').removeAttr('disabled');
      if (this.marketplaceId.val().length === 0) {
        this.$el.find('#amazonMarketplaceIdImage').field_status('clear', 'right away');
        return;
      }
      if (this.merchantId.val().length === 0) {
        this.$el.find('#amazonMerchantIdImage').field_status('clear', 'right away');
      }
    };

    AmazonStoreInfoView.prototype.inputChanged = function() {
      var enabled;
      enabled = EzBob.Validation.checkForm(this.validator);
      return this.$el.find('a.connect-amazon').toggleClass('disabled', !enabled);
    };

    AmazonStoreInfoView.prototype.runTutorial = function() {
      var content, div, tutorial;
      div = $('<div/>');
      tutorial = new EzBob.AmazonTutorialView();
      tutorial.render();
      content = tutorial.$el;
      div.html(content.html());
      div.find('.amazon-tutorial-slider').attr('id', 'amazon-tutorial-slider' + (new Date().getTime())).show();
      div.dialog({
        width: '960',
        height: '573',
        modal: true,
        draggable: false,
        resizable: false,
        close: function() {
          return div.empty();
        },
        dialogClass: 'amazon-tutor-dlg',
        title: 'Link Your Amazon Shop to EZBOB'
      });
      div.find('.amazon-tutorial-slider').coinslider({
        width: 930,
        height: 471,
        delay: 1000000,
        effect: 'rain',
        sDelay: 30,
        titleSpeed: 50,
        links: false
      });
      return false;
    };

    AmazonStoreInfoView.prototype.back = function() {
      this.trigger('back');
      return false;
    };

    AmazonStoreInfoView.prototype.connect = function(e) {
      var marketplaceId, merchantId,
        _this = this;
      if (!EzBob.Validation.checkForm(this.validator)) {
        this.validator.form();
        return false;
      }
      if (this.$el.find('a.connect-amazon').hasClass('disabled')) {
        return false;
      }
      marketplaceId = this.$el.find('#amazonMarketplaceId');
      merchantId = this.$el.find('#amazonMerchantId');
      this.blockBtn(true);
      $.post(window.gRootPath + 'Customer/AmazonMarketplaces/ConnectAmazon', {
        marketplaceId: marketplaceId.val(),
        merchantId: merchantId.val()
      }).success(function(result) {
        _this.blockBtn(false);
        if (result.error) {
          EzBob.App.trigger('error', result.error);
          _this.trigger('back');
          return;
        }
        EzBob.App.trigger('info', result.msg);
        _this.trigger('completed');
        _this.trigger('back');
        marketplaceId.val('');
        return merchantId.val('');
      }).error(function() {
        return EzBob.App.trigger('error', 'Amazon Account Adding Failed');
      });
      return false;
    };

    AmazonStoreInfoView.prototype.print = function() {
      window.print();
      return false;
    };

    AmazonStoreInfoView.prototype.blockBtn = function(isBlock) {
      BlockUi((isBlock ? 'on' : 'off'));
      return this.$el.find('connect-amazon').toggleClass('disabled', isBlock);
    };

    AmazonStoreInfoView.prototype.getDocumentTitle = function() {
      EzBob.App.trigger('clear');
      return 'Link Amazon Account';
    };

    return AmazonStoreInfoView;

  })(Backbone.View);

  EzBob.AmazonTutorialView = (function(_super) {

    __extends(AmazonTutorialView, _super);

    function AmazonTutorialView() {
      return AmazonTutorialView.__super__.constructor.apply(this, arguments);
    }

    AmazonTutorialView.prototype.template = "#amazon-tutorial";

    return AmazonTutorialView;

  })(Backbone.Marionette.ItemView);

  EzBob.AmazonStoreModel = (function(_super) {

    __extends(AmazonStoreModel, _super);

    function AmazonStoreModel() {
      return AmazonStoreModel.__super__.constructor.apply(this, arguments);
    }

    AmazonStoreModel.prototype.defaults = {
      marketplaceId: null
    };

    return AmazonStoreModel;

  })(Backbone.Model);

  EzBob.AmazonStoreModels = (function(_super) {

    __extends(AmazonStoreModels, _super);

    function AmazonStoreModels() {
      return AmazonStoreModels.__super__.constructor.apply(this, arguments);
    }

    AmazonStoreModels.prototype.model = EzBob.AmazonStoreModel;

    AmazonStoreModels.prototype.url = "" + window.gRootPath + "Customer/AmazonMarketPlaces";

    return AmazonStoreModels;

  })(Backbone.Collection);

}).call(this);
