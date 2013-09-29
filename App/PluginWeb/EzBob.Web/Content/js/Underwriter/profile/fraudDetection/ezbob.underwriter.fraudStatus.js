(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4, _ref5, _ref6, _ref7, _ref8,
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
      var _this = this;

      this.model.fetch().done(function() {
        return _this.$el.find('#fraud-view').show();
      });
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

  EzBob.Underwriter.FraudModel = (function(_super) {
    __extends(FraudModel, _super);

    function FraudModel() {
      _ref3 = FraudModel.__super__.constructor.apply(this, arguments);
      return _ref3;
    }

    FraudModel.prototype.defaults = function() {
      return {
        FirstName: "",
        LastName: "",
        Addresses: [],
        Phones: [],
        Emails: [],
        EmailDomains: [],
        BankAccounts: [],
        Companies: [],
        Shops: []
      };
    };

    FraudModel.prototype.sendToServer = function() {
      var xhr,
        _this = this;

      xhr = Backbone.sync("create", this, {
        url: "" + gRootPath + "Underwriter/Fraud/AddNewUser"
      });
      return xhr.complete(function() {
        return _this.trigger("saved");
      });
    };

    return FraudModel;

  })(Backbone.Model);

  EzBob.Underwriter.FraudModels = (function(_super) {
    __extends(FraudModels, _super);

    function FraudModels() {
      _ref4 = FraudModels.__super__.constructor.apply(this, arguments);
      return _ref4;
    }

    FraudModels.prototype.url = "" + gRootPath + "Underwriter/Fraud/GetAll";

    FraudModels.prototype.model = EzBob.Underwriter.FraudModel;

    return FraudModels;

  })(Backbone.Collection);

  EzBob.Underwriter.simpleValueAddView = (function(_super) {
    __extends(simpleValueAddView, _super);

    function simpleValueAddView() {
      _ref5 = simpleValueAddView.__super__.constructor.apply(this, arguments);
      return _ref5;
    }

    simpleValueAddView.prototype.initialize = function(options) {
      this.template = options.template;
      return this.type = options.type;
    };

    simpleValueAddView.prototype.events = {
      "click .ok": "okClicked"
    };

    simpleValueAddView.prototype.ui = {
      form: "form"
    };

    simpleValueAddView.prototype.onRender = function() {
      if (this.type !== "Addresses") {
        this.ui.form.find("input, textarea").addClass('required');
      }
      this.validator = this.ui.form.validate({
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
      return this;
    };

    simpleValueAddView.prototype.okClicked = function() {
      var model;

      if (!this.validator.form()) {
        return;
      }
      model = new Backbone.Model(SerializeArrayToEasyObject(this.ui.form.serializeArray()));
      this.trigger("added", {
        model: model,
        type: this.type
      });
      this.close();
      return false;
    };

    return simpleValueAddView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.SimpleValueView = (function(_super) {
    __extends(SimpleValueView, _super);

    function SimpleValueView() {
      _ref6 = SimpleValueView.__super__.constructor.apply(this, arguments);
      return _ref6;
    }

    SimpleValueView.prototype.initialize = function(options) {
      return this.template = options.template;
    };

    SimpleValueView.prototype.serializeData = function() {
      return {
        models: this.model
      };
    };

    return SimpleValueView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.AddEditFraudView = (function(_super) {
    __extends(AddEditFraudView, _super);

    function AddEditFraudView() {
      _ref7 = AddEditFraudView.__super__.constructor.apply(this, arguments);
      return _ref7;
    }

    AddEditFraudView.prototype.template = "#fraud-add-edit-template";

    AddEditFraudView.prototype.ui = {
      form: "form"
    };

    AddEditFraudView.prototype.onRender = function() {
      this.ui.form.find("input, textarea").addClass('required');
      return this.validator = this.ui.form.validate({
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
    };

    AddEditFraudView.prototype.events = {
      "click .save": "saveButtonClicked",
      "click .add": "addClicked",
      "click .remove": "removeClicked"
    };

    AddEditFraudView.prototype.removeClicked = function(e) {
      var $el, index, type;

      $el = $(e.currentTarget);
      index = $el.data("index");
      type = $el.data("type");
      (this.model.get(type)).splice(index, 1);
      this.reRenderArea(type);
      return false;
    };

    AddEditFraudView.prototype.saveButtonClicked = function() {
      var formData, isValid;

      isValid = this.validator.form();
      if (!isValid) {
        this.ui.form.closest('.modal-body').animate({
          scrollTop: 0
        }, 500);
        return;
      }
      formData = SerializeArrayToEasyObject(this.ui.form.serializeArray());
      this.model.set({
        FirstName: formData.FirstName,
        LastName: formData.LastName
      });
      this.model.sendToServer();
      return this.close();
    };

    AddEditFraudView.prototype.addClicked = function(e) {
      var $el, template, type, view;

      $el = $(e.currentTarget);
      type = $el.data("type");
      template = "#add-" + type + "-template";
      view = new EzBob.Underwriter.simpleValueAddView({
        template: template,
        type: type
      });
      EzBob.App.modal2.show(view);
      view.on("added", this.simpleValueAdded, this);
      return false;
    };

    AddEditFraudView.prototype.simpleValueAdded = function(data) {
      (this.model.get(data.type)).push(data.model.toJSON());
      this.reRenderArea(data.type);
      return false;
    };

    AddEditFraudView.prototype.reRenderArea = function(type) {
      var $el;

      $el = this.$el.find("." + type);
      return (new EzBob.Underwriter.SimpleValueView({
        template: "#" + type + "-template",
        el: $el,
        model: this.model.get(type)
      })).render();
    };

    return AddEditFraudView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.FraudView = (function(_super) {
    __extends(FraudView, _super);

    function FraudView() {
      _ref8 = FraudView.__super__.constructor.apply(this, arguments);
      return _ref8;
    }

    FraudView.prototype.template = "#fraud-template";

    FraudView.prototype.initialize = function() {
      this.model = new EzBob.Underwriter.FraudModels();
      this.model.on("change reset", this.render, this);
      return this.model.fetch();
    };

    FraudView.prototype.events = {
      "click .add": "addButtonClicked",
      "click .all": "allClicked"
    };

    FraudView.prototype.allClicked = function() {
      var cid, xhr,
        _this = this;

      BlockUi("on");
      cid = prompt("Customer Id");
      if (!cid) {
        return;
      }
      xhr = $.get("" + gRootPath + "Underwriter/Fraud/RunCheck", {
        id: cid
      });
      return xhr.complete(function(data) {
        _this.showData(data.responseText);
        return BlockUi("off");
      });
    };

    FraudView.prototype.showData = function(data) {
      var dialog;

      dialog = $('<div/>').html("<table class='table table-bordered'><tr><td>Check Type</td><td>Current Field</td><td>Compare Field</td><td>Value</td><td>Concurrence</td></tr>" + data + "</table>");
      return dialog.dialog({
        width: '75%',
        height: 600
      });
    };

    FraudView.prototype.serializeData = function() {
      return {
        data: this.model.toJSON()
      };
    };

    FraudView.prototype.addButtonClicked = function() {
      var model, view,
        _this = this;

      model = new EzBob.Underwriter.FraudModel();
      view = new EzBob.Underwriter.AddEditFraudView({
        model: model
      });
      view.modalOptions = {
        show: true,
        keyboard: false,
        width: 600,
        height: 600
      };
      EzBob.App.modal.show(view);
      model.on("saved", function() {
        return _this.model.fetch();
      });
      return false;
    };

    return FraudView;

  })(Backbone.Marionette.ItemView);

}).call(this);
