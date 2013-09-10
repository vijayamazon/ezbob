(function() {
  var root, _ref, _ref1, _ref2, _ref3, _ref4, _ref5,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.FraudModel = (function(_super) {
    __extends(FraudModel, _super);

    function FraudModel() {
      _ref = FraudModel.__super__.constructor.apply(this, arguments);
      return _ref;
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
      _ref1 = FraudModels.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    FraudModels.prototype.url = "" + gRootPath + "Underwriter/Fraud/GetAll";

    FraudModels.prototype.model = EzBob.Underwriter.FraudModel;

    return FraudModels;

  })(Backbone.Collection);

  EzBob.Underwriter.simpleValueAddView = (function(_super) {
    __extends(simpleValueAddView, _super);

    function simpleValueAddView() {
      _ref2 = simpleValueAddView.__super__.constructor.apply(this, arguments);
      return _ref2;
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
      _ref3 = SimpleValueView.__super__.constructor.apply(this, arguments);
      return _ref3;
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
      _ref4 = AddEditFraudView.__super__.constructor.apply(this, arguments);
      return _ref4;
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
      _ref5 = FraudView.__super__.constructor.apply(this, arguments);
      return _ref5;
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
