(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SupportView = (function(_super) {

    __extends(SupportView, _super);

    function SupportView() {
      return SupportView.__super__.constructor.apply(this, arguments);
    }

    SupportView.prototype.initialize = function() {
      this.model = new EzBob.Underwriter.SupportModel();
      this.model.on("change reset", this.render, this);
      this.model.fetch();
      return this.preHeight = 20;
    };

    SupportView.prototype.template = "#support-template";

    SupportView.prototype.events = {
      "click pre": "preClicked",
      "click .reCheckMP": "recheckClicked",
      'click [data-sort-type]': 'sortClicked'
    };

    SupportView.prototype.onRender = function() {
      var arrow;
      this.$el.find("[data-sort-type]").css('cursor', 'pointer');
      this.$el.find("pre").tooltip({
        title: 'Click to see detail info'
      }).tooltip('fixTitle');
      this.$el.find("pre").height(this.preHeight).css("overflow", "hidden").css('cursor', 'pointer');
      (this.$el.find('.arrow')).hide();
      arrow = this.$el.find("[data-sort-type=" + (this.model.get('sortField')) + "] .arrow");
      arrow.show();
      arrow.removeClass().addClass(this.model.get('sortType') === 'asc' ? 'arrow icon-arrow-up' : 'arrow icon-arrow-down');
      BlockUi("off");
      return EzBob.handleUserLayoutSetting();
    };

    SupportView.prototype.sortClicked = function(e) {
      var $el, currentField, currentSortType, field;
      BlockUi("on");
      $el = $(e.currentTarget);
      field = $el.data('sort-type');
      currentField = this.model.get('sortField');
      currentSortType = this.model.get('sortType');
      this.model.set({
        'sortField': field,
        'sortType': field !== currentField || currentSortType === 'desc' ? 'asc' : 'desc'
      }, {
        silent: true
      });
      return this.model.fetch();
    };

    SupportView.prototype.recheckClicked = function(e) {
      var $el, customerId, mpType, okFn, umi,
        _this = this;
      $el = $(e.currentTarget);
      if ($el.hasClass('disabled')) {
        return false;
      }
      umi = $el.attr("umi");
      mpType = $el.attr("marketplaceType");
      customerId = this.model.customerId;
      okFn = function() {
        var xhr;
        $el.addClass('disabled');
        xhr = $.get("" + window.gRootPath + "Underwriter/MarketPlaces/ReCheckMarketplaces", {
          customerId: customerId,
          umi: umi,
          marketplaceType: mpType
        });
        xhr.done(function(response) {
          if (response && response.error !== void 0) {
            EzBob.ShowMessage(response.error, "Error occured");
          } else {
            EzBob.ShowMessage("Wait a few minutes", "The marketplace recheck is running. ", null, "OK");
          }
          return _this.trigger("rechecked", {
            umi: umi,
            el: $el
          });
        });
        return xhr.fail(function(data) {
          return console.error(data.responseText);
        });
      };
      EzBob.ShowMessage("", "Are you sure?", okFn, "Yes", null, "No");
      return false;
    };

    SupportView.prototype.preClicked = function(e) {
      var $el, elHeight;
      $el = $(e.currentTarget);
      elHeight = $el.height();
      $el.height(elHeight !== this.preHeight ? this.preHeight : "auto");
      $el.tooltip('destroy');
      $el.tooltip({
        title: elHeight !== this.preHeight ? 'Click to see detail info' : 'Click to hide detail info'
      });
      return $el.tooltip("enable").tooltip('fixTitle');
    };

    SupportView.prototype.hide = function() {
      this.$el.hide();
      clearInterval(this.modelUpdater);
      return BlockUi('off');
    };

    SupportView.prototype.show = function() {
      var _this = this;
      this.$el.show();
      return this.modelUpdater = setInterval(function() {
        return _this.model.fetch();
      }, 2000);
    };

    SupportView.prototype.serializeData = function() {
      return {
        model: this.model.get('models')
      };
    };

    return SupportView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.SupportModel = (function(_super) {

    __extends(SupportModel, _super);

    function SupportModel() {
      return SupportModel.__super__.constructor.apply(this, arguments);
    }

    SupportModel.prototype.initialize = function() {
      return this.set({
        sortField: 4,
        sortType: 'desc',
        models: []
      }, {
        silent: true
      });
    };

    SupportModel.prototype.urlRoot = function() {
      return "" + gRootPath + "Underwriter/Support/Index?sortField=" + (this.get('sortField')) + "&sortType=" + (this.get('sortType'));
    };

    return SupportModel;

  })(Backbone.Model);

}).call(this);
