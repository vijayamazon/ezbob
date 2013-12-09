(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.PersonInfoView = (function(_super) {

    __extends(PersonInfoView, _super);

    function PersonInfoView() {
      return PersonInfoView.__super__.constructor.apply(this, arguments);
    }

    PersonInfoView.prototype.template = "#profile-person-info-template";

    PersonInfoView.prototype.initialize = function() {
      return this.bindTo(this.model, "change sync", this.render, this);
    };

    PersonInfoView.prototype.onRender = function() {
      console.log('der modelllle ist', this.model);
      this.setCciMark();
      this.$el.find(".tltp").tooltip();
      return this.$el.find(".tltp-left").tooltip({
        placement: "left"
      });
    };

    PersonInfoView.prototype.setCciMark = function() {
      var oSpan;
      oSpan = this.$el.find('.cci-mark');
      if (this.model.get('CciMark')) {
        return oSpan.text('on').closest('td').addClass('red_cell');
      } else {
        return oSpan.text('off').closest('td').removeClass('red_cell');
      }
    };

    PersonInfoView.prototype.toggleCciMark = function() {
      var id,
        _this = this;
      id = this.model.get('Id');
      BlockUi();
      return $.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleCciMark', {
        id: id
      }).done(function(result) {
        if (result.error) {
          return EzBob.App.trigger('error', result.error);
        } else {
          _this.model.set('CciMark', result.mark);
          return _this.setCciMark();
        }
      }).always(function() {
        return UnBlockUi();
      });
    };

    PersonInfoView.prototype.events = {
      "click button[name=\"changeDisabledState\"]": "changeDisabledState",
      "click button[name=\"editEmail\"]": "editEmail",
      "click [name=\"isTestEditButton\"]": "isTestEditButton",
      "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton",
      "click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked",
      'click button.cci-mark-toggle': 'toggleCciMark'
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
      var collectionStatusModel, customerId, prevStatus, xhr,
        _this = this;
      collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel({
        customerId: this.model.get('Id'),
        currentStatus: this.model.get('Disabled')
      });
      prevStatus = this.model.get('Disabled');
      customerId = this.model.get('Id');
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
          var newStatus, that;
          newStatus = collectionStatusModel.get('currentStatus');
          that = _this;
          xhr = $.post("" + window.gRootPath + "Underwriter/ApplicationInfo/GetIsStatusWarning", {
            status: newStatus
          });
          return xhr.done(function(result) {
            var disabled, isWarning, xhr2;
            BlockUi("on");
            isWarning = result;
            disabled = waiting || !isStatusEnabled;
            that.model.set('Disabled', newStatus);
            that.model.set('IsWarning', isWarning);
            xhr2 = $.post("" + window.gRootPath + "Underwriter/ApplicationInfo/LogStatusChange", {
              newStatus: newStatus,
              prevStatus: prevStatus,
              customerId: customerId
            });
            return xhr2.done(function() {
              return BlockUi("off");
            });
          });
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
      return PersonalInfoModel.__super__.constructor.apply(this, arguments);
    }

    PersonalInfoModel.prototype.idAttribute = "Id";

    PersonalInfoModel.prototype.urlRoot = window.gRootPath + "Underwriter/CustomerInfo/Index";

    PersonalInfoModel.prototype.initialize = function() {
      var status, _i, _len, _ref, _results;
      this.on("change:Disabled", this.changeDisabled, this);
      this.on("change:FraudCheckStatusId", this.changeFraudCheckStatus, this);
      this.changeDisabled();
      this.changeFraudCheckStatus();
      if (this.StatusesArr === void 0) {
        this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;
      }
      this.StatusesArr = {};
      _ref = this.statuses.models;
      _results = [];
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        status = _ref[_i];
        _results.push(this.StatusesArr[status.get('Id')] = status.get('Name'));
      }
      return _results;
    };

    PersonalInfoModel.prototype.changeDisabled = function(silent) {
      var disabled, disabledText;
      if (silent == null) {
        silent = false;
      }
      disabledText = "";
      disabled = this.get("Disabled");
      if (disabled === void 0) {
        return;
      }
      disabledText = this.StatusesArr[disabled];
      if (disabledText === void 0) {
        disabledText = "Enabled";
      }
      return this.set({
        "DisabledText": disabledText
      }, {
        silent: true
      });
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
