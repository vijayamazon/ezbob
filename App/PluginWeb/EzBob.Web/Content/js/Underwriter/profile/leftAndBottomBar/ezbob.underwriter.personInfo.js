(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.PersonInfoView = (function(_super) {
    __extends(PersonInfoView, _super);

    function PersonInfoView() {
      _ref = PersonInfoView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    PersonInfoView.prototype.template = "#profile-person-info-template";

    PersonInfoView.prototype.initialize = function() {
      return this.bindTo(this.model, "change", this.render, this);
    };

    PersonInfoView.prototype.onRender = function() {
      this.$el.find(".tltp").tooltip();
      return this.$el.find(".tltp-left").tooltip({
        placement: "left"
      });
    };

    PersonInfoView.prototype.events = {
      "click button[name=\"changeDisabledState\"]": "changeDisabledState",
      "click button[name=\"editEmail\"]": "editEmail",
      "click [name=\"isTestEditButton\"]": "isTestEditButton",
      "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton",
      "click [name=\"updateCRM\"]": "updateCRM",
      "click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked"
    };

    PersonInfoView.prototype.changeFraudStatusManualyClicked = function() {
      var fraudStatusModel, xhr,
        _this = this;

      fraudStatusModel = new EzBob.Underwriter.FraudStatusModel({
        customerId: this.model.get('Id'),
        currentStatus: this.model.get('FraudCheckStatusId')
      });
      BlockUi("on");
      xhr = fraudStatusModel.fetch();
      return xhr.done(function() {
        var fraudStatusLayout;

        fraudStatusLayout = new EzBob.Underwriter.FraudStatusLayout({
          model: fraudStatusModel
        });
        fraudStatusLayout.render();
        EzBob.App.jqmodal.show(fraudStatusLayout);
        BlockUi("off");
        return fraudStatusLayout.on('saved', function() {
          _this.model.set('FraudCheckStatusId', fraudStatusModel.get('currentStatus'));
          return _this.model.set('FraudCheckStatus', fraudStatusModel.get('currentStatusText'));
        });
      });
    };

    PersonInfoView.prototype.templateHelpers = {
      getIcon: function() {
        if (this.EmailState === "Confirmed" || this.EmailState === "ManuallyConfirmed") {
          return "icon-ok";
        }
        return "icon-question-sign";
      }
    };

    PersonInfoView.prototype.changeDisabledState = function() {
      var collectionStatusModel, xhr,
        _this = this;

      collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel({
        customerId: this.model.get('Id'),
        currentStatus: this.model.get('Disabled')
      });
      BlockUi("on");
      xhr = collectionStatusModel.fetch();
      return xhr.done(function() {
        var collectionStatusLayout;

        collectionStatusLayout = new EzBob.Underwriter.CollectionStatusLayout({
          model: collectionStatusModel
        });
        collectionStatusLayout.render();
        EzBob.App.jqmodal.show(collectionStatusLayout);
        BlockUi("off");
        return collectionStatusLayout.on('saved', function() {
          return _this.model.set('Disabled', collectionStatusModel.get('currentStatus'));
        });
      });
    };

    PersonInfoView.prototype.isTestEditButton = function() {
      var d;

      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "IsTest",
        title: "Is Testing User",
        postValueName: "enbaled",
        checkboxName: "Test",
        url: "Underwriter/ApplicationInfo/ChangeTestStatus",
        data: {
          id: this.model.get("Id")
        }
      });
      d.render();
    };

    PersonInfoView.prototype.avoidAutomaticDecisionButton = function() {
      var d;

      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "IsAvoid",
        title: "Manual Decision",
        postValueName: "enbaled",
        checkboxName: "Enable Manual Decision",
        url: "Underwriter/ApplicationInfo/AvoidAutomaticDecision",
        data: {
          id: this.model.get("Id")
        }
      });
      d.render();
    };

    PersonInfoView.prototype.updateCRM = function() {
      var that, xhr;

      that = this;
      BlockUi("On");
      xhr = $.post("" + window.gRootPath + "Underwriter/CustomerInfo/UpdateCrm", {
        id: this.model.get("Id")
      });
      return xhr.done(function() {
        var xhr2;

        xhr2 = that.model.fetch();
        return xhr2.done(function() {
          return BlockUi("Off");
        });
      });
    };

    PersonInfoView.prototype.disablingChanged = function() {
      var disabled, id, that;

      disabled = this.$el.find("select[name=\"disabling\"] option:selected").val();
      id = this.model.get("Id");
      that = this;
      this.model.set("Disabled", disabled);
      return $.post(window.gRootPath + "Underwriter/ApplicationInfo/ChangeDisabled", {
        id: id,
        disabled: disabled
      }).done(function() {
        return that.trigger("DisableChange", id);
      });
    };

    PersonInfoView.prototype.editEmail = function() {
      var view;

      view = new EzBob.EmailEditView({
        model: this.model
      });
      EzBob.App.modal.show(view);
      view.on("showed", function() {
        return view.$el.find("input").focus();
      });
      return false;
    };

    return PersonInfoView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.PersonalInfoModel = (function(_super) {
    __extends(PersonalInfoModel, _super);

    function PersonalInfoModel() {
      _ref1 = PersonalInfoModel.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    PersonalInfoModel.prototype.idAttribute = "Id";

    PersonalInfoModel.prototype.urlRoot = window.gRootPath + "Underwriter/CustomerInfo/Index";

    PersonalInfoModel.prototype.initialize = function() {
      this.on("change:Disabled", this.changeDisabled, this);
      this.on("change:FraudCheckStatusId", this.changeFraudCheckStatus, this);
      this.changeDisabled();
      return this.changeFraudCheckStatus();
    };

    PersonalInfoModel.prototype.changeDisabled = function() {
      var disabled, disabledText;

      disabledText = "";
      disabled = this.get("Disabled");
      switch (disabled) {
        case 1:
          disabledText = "Disabled";
          break;
        case 2:
          disabledText = "Fraud";
          break;
        case 3:
          disabledText = "Legal";
          break;
        case 4:
          disabledText = "Default";
          break;
        case 5:
          disabledText = "Fraud Suspect";
          break;
        default:
          disabledText = "Enabled";
      }
      return this.set("DisabledText", disabledText);
    };

    PersonalInfoModel.prototype.changeFraudCheckStatus = function() {
      var fraud, fraudCss;

      fraud = this.get("FraudCheckStatusId");
      fraudCss = "";
      switch (fraud) {
        case 2:
          fraudCss = "red_cell";
      }
      return this.set("FraudHighlightCss", fraudCss);
    };

    return PersonalInfoModel;

  })(Backbone.Model);

}).call(this);
