(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.FundingView = (function(_super) {
    __extends(FundingView, _super);

    function FundingView() {
      _ref = FundingView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    FundingView.prototype.initialize = function() {
      var xhr, xhr1,
        _this = this;

      this.model = new EzBob.Underwriter.FundingModel();
      this.model.on("change reset", this.render, this);
      this.model.fetch();
      this.requiredFunds = -1;
      xhr1 = $.post("" + window.gRootPath + "Underwriter/Funding/GetRequiredFunds");
      xhr1.done(function(res) {
        _this.requiredFunds = res;
        return _this.render();
      });
      xhr = $.post("" + window.gRootPath + "Underwriter/Funding/GetAvailableFundsInterval");
      return xhr.done(function(res) {
        return _this.modelUpdater = setInterval(function() {
          return _this.model.fetch();
        }, res);
      });
    };

    FundingView.prototype.template = "#funding-template";

    FundingView.prototype.events = {
      "click #addFundsBtn": "addFunds",
      "click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds"
    };

    FundingView.prototype.addFunds = function(e) {
      return console.log('placeholder');
    };

    FundingView.prototype.cancelManuallyAddedFunds = function(e) {
      return console.log('placeholder');
    };

    FundingView.prototype.onRender = function() {
      var li;

      if (!$("body").hasClass("role-manager")) {
        this.$el.find('#addFundsBtn').hide();
        this.$el.find('#cancelManuallyAddedFundsBtn').hide();
      }
      li = $(document.getElementById("liFunding"));
      if (this.requiredFunds > this.model.get('AvailableFunds')) {
        if (!li.hasClass('available-funds-alert')) {
          return li.addClass('available-funds-alert');
        }
      } else {
        if (li.hasClass('available-funds-alert')) {
          return li.removeClass('available-funds-alert');
        }
      }
    };

    FundingView.prototype.hide = function() {
      return this.$el.hide();
    };

    FundingView.prototype.show = function() {
      return this.$el.show();
    };

    FundingView.prototype.serializeData = function() {
      return {
        model: this.model
      };
    };

    return FundingView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.FundingModel = (function(_super) {
    __extends(FundingModel, _super);

    function FundingModel() {
      _ref1 = FundingModel.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    FundingModel.prototype.urlRoot = function() {
      return "" + gRootPath + "Underwriter/Funding/GetCurrentFundingStatus";
    };

    return FundingModel;

  })(Backbone.Model);

}).call(this);
