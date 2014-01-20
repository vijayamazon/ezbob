(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.StoreButtonView = (function(_super) {

    __extends(StoreButtonView, _super);

    function StoreButtonView() {
      return StoreButtonView.__super__.constructor.apply(this, arguments);
    }

    StoreButtonView.prototype.template = "#store-button-template";

    StoreButtonView.prototype.initialize = function(options) {
      this.name = options.name;
      this.mpAccounts = options.mpAccounts.get('mpAccounts');
      this.shops = this.mpAccounts ? this.shops = _.where(this.mpAccounts, {
        MpName: this.name
      }) : [];
      return this.shopClass = options.name.replace(' ', '');
    };

    StoreButtonView.prototype.serializeData = function() {
      var data;
      data = {
        name: this.name,
        shopClass: this.shopClass
      };
      return data;
    };

    StoreButtonView.prototype.onRender = function() {
      var btn, oHelpWindowTemplate, oLinks, sTitle;
      btn = this.$el.find('.marketplace-button-account-' + this.shopClass);
      this.$el.removeClass('marketplace-button-full marketplace-button-empty');
      sTitle = (this.shops.length || 'No') + ' account' + (this.shops.length === 1 ? '' : 's') + ' linked. Click to link ';
      if (this.shops.length) {
        this.$el.addClass('marketplace-button-full');
        sTitle += 'more.';
      } else {
        this.$el.addClass('marketplace-button-empty');
        sTitle += 'one.';
      }
      this.$el.attr('title', sTitle);
      switch (this.shopClass) {
        case 'eBay':
        case 'paypal':
        case 'FreeAgent':
        case 'Sage':
          oHelpWindowTemplate = _.template($('#store-button-help-window-template').html());
          this.$el.append(oHelpWindowTemplate(this.serializeData()));
          oLinks = JSON.parse($('#store-button-help-window-links').html());
          this.$el.find('.help-window-continue-link').attr('href', oLinks[this.shopClass]);
          btn.attr('href', '#' + this.shopClass + '_help');
          btn.colorbox({
            inline: true,
            transition: 'none',
            onClosed: function() {
              var oBackLink;
              oBackLink = $('#link_account_implicit_back');
              if (oBackLink.length) {
                return EzBob.UiAction.saveOne('click', oBackLink);
              }
            }
          });
          break;
        default:
          btn.click((function(arg) {
            return function() {
              return EzBob.App.trigger('ct:storebase.shops.connect', arg);
            };
          })(this.shopClass));
      }
      return btn.hover((function(evt) {
        return $('.onhover', this).animate({
          top: 0,
          opacity: 1
        }, 'fast');
      }), (function(evt) {
        return $('.onhover', this).animate({
          top: '60px',
          opacity: 0
        }, 'fast');
      }));
    };

    StoreButtonView.prototype.isAddingAllowed = function() {
      return true;
    };

    StoreButtonView.prototype.update = function(data) {
      return this.shops = data ? this.shops = _.where(data, {
        MpName: this.name
      }) : [];
    };

    return StoreButtonView;

  })(Backbone.Marionette.ItemView);

}).call(this);
