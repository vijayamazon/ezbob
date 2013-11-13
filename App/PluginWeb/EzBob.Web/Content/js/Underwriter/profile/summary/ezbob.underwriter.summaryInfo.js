(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.SummaryInfoView = (function(_super) {
    __extends(SummaryInfoView, _super);

    function SummaryInfoView() {
      _ref = SummaryInfoView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    SummaryInfoView.prototype.template = "#profile-summary-template";

    SummaryInfoView.prototype.initialize = function() {
      return this.bindTo(this.model, "change sync", this.render, this);
    };

    SummaryInfoView.prototype.events = {
      "keydown #CommentArea": "CommentAreaChanged",
      "click #commentSave": "saveComment"
    };

    SummaryInfoView.prototype.CommentAreaChanged = function() {
      this.CommentSave.removeClass("disabled");
      return this.CommentArea.css("border", "1px solid yellow");
    };

    SummaryInfoView.prototype.saveComment = function() {
      var that;

      that = this;
      $.post(window.gRootPath + "Underwriter/Profile/SaveComment", {
        Id: this.model.get("Id"),
        comment: this.CommentArea.val()
      }).done(function() {
        that.CommentArea.css("border", "1px solid #ccc");
        return that.CommentSave.addClass("disabled");
      }).fail(function(data) {
        that.CommentArea.css("border", "1px solid red");
        return console.error(data.responseText);
      });
      return false;
    };

    SummaryInfoView.prototype.serializeData = function() {
      return {
        m: this.model.toJSON()
      };
    };

    SummaryInfoView.prototype.onRender = function() {
      var isNew;

      this.CommentSave = this.$el.find("#commentSave");
      this.CommentArea = this.$el.find("#CommentArea");
      this.$el.find('a[data-bug-type]').tooltip({
        title: 'Report bug'
      });
      isNew = this.model.get("MarketPlaces") && this.model.get("MarketPlaces").IsNew;
      $("#new-ribbon-marketplaces").toggle(Convert.toBool(isNew));
      if (!this.model.get('IsOffline')) {
        return this.$el.find('.offline').remove();
      }
    };

    return SummaryInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.SummaryInfoModel = (function(_super) {
    __extends(SummaryInfoModel, _super);

    function SummaryInfoModel() {
      _ref1 = SummaryInfoModel.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    SummaryInfoModel.prototype.idAttribute = "Id";

    SummaryInfoModel.prototype.urlRoot = window.gRootPath + "Underwriter/Profile/Index";

    return SummaryInfoModel;

  })(Backbone.Model);

}).call(this);
