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
        logoText: this.logoText,
        shopClass: this.shopClass,
        shops: [],
        ribbon: this.ribbon,
        shopNames: ""
      };
      if (this.shops) {
        data.shops = this.shops;
        data.shopNames = _.pluck(this.shops, "displayName").join(", ");
      }
      return data;
    };

    StoreButtonView.prototype.onRender = function() {
      this.$el.find('.tooltipdiv').tooltip();
      return this.$el.find('.source_help').colorbox({
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
