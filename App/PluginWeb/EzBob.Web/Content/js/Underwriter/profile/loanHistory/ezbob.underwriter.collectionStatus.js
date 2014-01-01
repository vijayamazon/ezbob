(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.CollectionStatusModel = (function(_super) {
    __extends(CollectionStatusModel, _super);

    function CollectionStatusModel() {
      _ref = CollectionStatusModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    CollectionStatusModel.prototype.urlRoot = function() {
      return "" + window.gRootPath + "Underwriter/CollectionStatus/Index?Id=" + (this.get('customerId')) + "&currentStatus=" + (this.get('currentStatus'));
    };

    return CollectionStatusModel;

  })(Backbone.Model);

  EzBob.Underwriter.CollectionStatuses = (function(_super) {
    __extends(CollectionStatuses, _super);

    function CollectionStatuses() {
      _ref1 = CollectionStatuses.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    CollectionStatuses.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/CollectionStatus/GetStatuses";
    };

    return CollectionStatuses;

  })(Backbone.Collection);

  EzBob.CollectionStatusItemsView = (function(_super) {
    __extends(CollectionStatusItemsView, _super);

    function CollectionStatusItemsView() {
      _ref2 = CollectionStatusItemsView.__super__.constructor.apply(this, arguments);
      return _ref2;
    }

    CollectionStatusItemsView.prototype.template = '#collection-status-items-template';

    CollectionStatusItemsView.prototype.initialize = function() {
      this.statuses = new EzBob.Underwriter.CollectionStatuses();
      return this.statuses.fetch({
        async: false
      });
    };

    CollectionStatusItemsView.prototype.serializeData = function() {
      return {
        statuses: this.statuses.toJSON()
      };
    };

    return CollectionStatusItemsView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.CollectionStatusLayout = (function(_super) {
    __extends(CollectionStatusLayout, _super);

    function CollectionStatusLayout() {
      this.onSave = __bind(this.onSave, this);
      this.onCancel = __bind(this.onCancel, this);
      this.save = __bind(this.save, this);
      this.renderStatusValue = __bind(this.renderStatusValue, this);      _ref3 = CollectionStatusLayout.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    CollectionStatusLayout.prototype.template = '#collection-status-layout-template';

    CollectionStatusLayout.prototype.initialize = function() {
      this.modelBinder = new Backbone.ModelBinder();
      this.statuses = new EzBob.Underwriter.CollectionStatuses();
      return this.statuses.fetch({
        async: false
      });
    };

    CollectionStatusLayout.prototype.bindings = {
      currentStatus: {
        selector: "input[name='currentStatus']"
      },
      customerId: {
        selector: "input[name='customerId']"
      }
    };

    CollectionStatusLayout.prototype.regions = {
      list: '#list-items',
      content: '#collection-view'
    };

    CollectionStatusLayout.prototype.events = {
      'change #collection-status-items': 'changeStatus'
    };

    CollectionStatusLayout.prototype.changeStatus = function() {
      var currentStatus;

      currentStatus = $("#collection-status-items option:selected").val();
      this.model.set({
        "currentStatus": parseInt(currentStatus)
      });
      this.renderStatusValue();
      return this;
    };

    CollectionStatusLayout.prototype.renderStatusValue = function() {
      var collectionStatusView, currentStatus;

      currentStatus = this.model.get("currentStatus");
      if (this.statuses !== void 0 && this.statuses.models[currentStatus] !== void 0 && this.statuses.models[currentStatus].get('Name') === 'Default') {
        this.model.fetch();
        this.$el.find('#collection-view').show();
        collectionStatusView = new EzBob.Underwriter.CollectionStatusView({
          model: this.model
        });
        this.content.show(collectionStatusView);
      } else {
        this.$el.find('#collection-view').hide();
      }
      return false;
    };

    CollectionStatusLayout.prototype.save = function() {
      var action, form, postData,
        _this = this;

      BlockUi("on");
      form = this.$el.find('form');
      postData = form.serialize();
      action = "" + window.gRootPath + "Underwriter/CollectionStatus/Save/";
      $.post(action, postData).done(function() {
        _this.trigger('saved');
        return _this.close();
      }).complete(function() {
        return BlockUi("off");
      });
      return false;
    };

    CollectionStatusLayout.prototype.onRender = function() {
      var common;

      this.modelBinder.bind(this.model, this.el, this.bindings);
      common = new EzBob.CollectionStatusItemsView;
      this.list.show(common);
      this.$el.find("#collection-status-items [value='" + (this.model.get('CurrentStatus')) + "']").attr("selected", "selected");
      this.renderStatusValue();
      return this;
    };

    CollectionStatusLayout.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: 'Customer Status',
        draggable: true,
        width: "400",
        buttons: {
          "OK": this.onSave,
          "Cancel": this.onCancel
        }
      };
    };

    CollectionStatusLayout.prototype.onCancel = function() {
      return this.close();
    };

    CollectionStatusLayout.prototype.onSave = function() {
      return this.save();
    };

    return CollectionStatusLayout;

  })(Backbone.Marionette.Layout);

  EzBob.Underwriter.CollectionStatusView = (function(_super) {
    __extends(CollectionStatusView, _super);

    function CollectionStatusView() {
      _ref4 = CollectionStatusView.__super__.constructor.apply(this, arguments);
      return _ref4;
    }

    CollectionStatusView.prototype.template = '#collection-status-template';

    CollectionStatusView.prototype.initialize = function() {
      return this.modelBinder = new Backbone.ModelBinder();
    };

    CollectionStatusView.prototype.events = {
      "click .uploadFiles": "upload"
    };

    CollectionStatusView.prototype.bindings = {
      CollectionDescription: {
        selector: "#collectionDescription"
      }
    };

    CollectionStatusView.prototype.upload = function() {
      $("#addNewDoc").click();
      return false;
    };

    CollectionStatusView.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      this.$el.parents('.ui-dialog').find("button").addClass('btn btn-primary btn-back');
      this.$el.find('.collectionFee').autoNumeric({
        'aSep': ',',
        'aDec': '.',
        'aPad': true,
        'mNum': 16,
        'mRound': 'F',
        mDec: '2',
        vMax: '999999999999999'
      });
      return this;
    };

    return CollectionStatusView;

  })(Backbone.Marionette.Layout);

}).call(this);
