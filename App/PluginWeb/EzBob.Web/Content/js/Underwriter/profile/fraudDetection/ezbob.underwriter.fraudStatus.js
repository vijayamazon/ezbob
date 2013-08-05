(function() {
  var root, _ref, _ref1, _ref2,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.FraudStatusModel = (function(_super) {
    __extends(FraudStatusModel, _super);

    function FraudStatusModel() {
      _ref = FraudStatusModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    FraudStatusModel.prototype.urlRoot = function() {
      return "" + window.gRootPath + "Underwriter/FraudStatus/Index?Id=" + (this.get('customerId'));
    };

    return FraudStatusModel;

  })(Backbone.Model);

  EzBob.FraudStatusItemsView = (function(_super) {
    __extends(FraudStatusItemsView, _super);

    function FraudStatusItemsView() {
      _ref1 = FraudStatusItemsView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    FraudStatusItemsView.prototype.template = '#fraud-status-items-template';

    return FraudStatusItemsView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.FraudStatusLayout = (function(_super) {
    __extends(FraudStatusLayout, _super);

    function FraudStatusLayout() {
      this.onSave = __bind(this.onSave, this);
      this.onCancel = __bind(this.onCancel, this);
      this.save = __bind(this.save, this);
      this.renderStatusValue = __bind(this.renderStatusValue, this);      _ref2 = FraudStatusLayout.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    FraudStatusLayout.prototype.template = '#fraud-status-layout-template';

    FraudStatusLayout.prototype.initialize = function() {
      return this.modelBinder = new Backbone.ModelBinder();
    };

    FraudStatusLayout.prototype.bindings = {
      currentStatus: {
        selector: "input[name='currentStatus']"
      },
      customerId: {
        selector: "input[name='customerId']"
      }
    };

    FraudStatusLayout.prototype.regions = {
      list: '#list-fraud-items',
      content: '#fraud-view'
    };

    FraudStatusLayout.prototype.events = {
      'change #fraud-status-items': 'changeStatus'
    };

    FraudStatusLayout.prototype.changeStatus = function() {
      var currentStatus, currentStatusId;

      currentStatusId = $("#fraud-status-items option:selected").val();
      currentStatus = $("#fraud-status-items option:selected").text();
      this.model.set({
        "currentStatus": parseInt(currentStatusId)
      });
      this.model.set({
        "currentStatusText": currentStatus
      });
      this.renderStatusValue();
      return this;
    };

    FraudStatusLayout.prototype.renderStatusValue = function() {
      this.model.fetch();
      this.$el.find('#fraud-view').show();
      return false;
    };

    FraudStatusLayout.prototype.save = function() {
      var action, form, postData,
        _this = this;

      BlockUi("on");
      form = this.$el.find('form');
      postData = form.serialize();
      action = "" + window.gRootPath + "Underwriter/FraudStatus/Save/";
      $.post(action, postData).done(function() {
        _this.trigger('saved');
        return _this.close();
      }).complete(function() {
        return BlockUi("off");
      });
      return false;
    };

    FraudStatusLayout.prototype.onRender = function() {
      var common;

      this.modelBinder.bind(this.model, this.el, this.bindings);
      common = new EzBob.FraudStatusItemsView;
      this.list.show(common);
      this.$el.find("#fraud-status-items [value='" + (this.model.get('CurrentStatusId')) + "']").attr("selected", "selected");
      this.renderStatusValue();
      return this;
    };

    FraudStatusLayout.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: 'Fraud Status',
        draggable: true,
        width: "400",
        buttons: {
          "OK": this.onSave,
          "Cancel": this.onCancel
        }
      };
    };

    FraudStatusLayout.prototype.onCancel = function() {
      return this.close();
    };

    FraudStatusLayout.prototype.onSave = function() {
      return this.save();
    };

    return FraudStatusLayout;

  })(Backbone.Marionette.Layout);

}).call(this);
