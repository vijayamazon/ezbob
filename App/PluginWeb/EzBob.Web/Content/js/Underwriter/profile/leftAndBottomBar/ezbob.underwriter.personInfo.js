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
      return this.bindTo(this.model, "change sync", this.render, this);
    };

    PersonInfoView.prototype.onRender = function() {
      if (this.model.get('BrokerID')) {
        this.$el.find('#with-broker').addClass('with-broker');
      }
      this.$el.find(".tltp").tooltip();
      return this.$el.find(".tltp-left").tooltip({
        placement: "left"
      });
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
          return _this.setAlertStatus(result.mark, '.cci-mark', '.cci-mark-td', 'on', 'off');
        }
      }).always(function() {
        return UnBlockUi();
      });
    };

    PersonInfoView.prototype.toggleIsTest = function() {
      var id,
        _this = this;

      id = this.model.get('Id');
      BlockUi();
      return $.post(window.gRootPath + 'Underwriter/ApplicationInfo/ToggleIsTest', {
        id: id
      }).done(function(result) {
        if (result.error) {
          return EzBob.App.trigger('error', result.error);
        } else {
          return _this.setAlertStatus(result.isTest, '.is-test', '.is-test-td', 'Yes', 'No');
        }
      }).always(function() {
        return UnBlockUi();
      });
    };

    PersonInfoView.prototype.setAlertStatus = function(isAlert, span, td, alertText, okText) {
      var oSpan, oTd;

      if (alertText == null) {
        alertText = '';
      }
      if (okText == null) {
        okText = '';
      }
      oSpan = this.$el.find(span);
      oTd = this.$el.find(td);
      if (isAlert) {
        if (alertText !== '') {
          oSpan.text(alertText);
        }
        oSpan.closest('td').addClass('red_cell');
        return oTd.addClass('red_cell');
      } else {
        if (okText !== '') {
          oSpan.text(okText);
        }
        oSpan.closest('td').removeClass('red_cell');
        return oTd.removeClass('red_cell');
      }
    };

    PersonInfoView.prototype.events = {
      "click button[name=\"changeDisabledState\"]": "changeDisabledState",
      "click button[name=\"editEmail\"]": "editEmail",
      "click [name=\"avoidAutomaticDecisionButton\"]": "avoidAutomaticDecisionButton",
      "click [name=\"changeFraudStatusManualy\"]": "changeFraudStatusManualyClicked",
      'click button.cci-mark-toggle': 'toggleCciMark',
      'click button.istest-toggle': 'toggleIsTest',
      'click [name="TrustPilotStatusUpdate"]': 'updateTrustPilotStatus',
      'click #MainStrategyHidden': 'activateMainStratgey',
      'click #FinishWizardHidden': 'activateFinishWizard'
    };

    PersonInfoView.prototype.activateMainStratgey = function() {
      var xhr;

      return xhr = $.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ActivateMainStrategy", {
        customerId: this.model.get('Id')
      });
    };

    PersonInfoView.prototype.activateFinishWizard = function() {
      var xhr;

      return xhr = $.post("" + window.gRootPath + "Underwriter/ApplicationInfo/ActivateFinishWizard", {
        customerId: this.model.get('Id')
      });
    };

    PersonInfoView.prototype.updateTrustPilotStatus = function() {
      var d,
        _this = this;

      d = new EzBob.Dialogs.ComboEdit({
        model: this.model,
        propertyName: 'TrustPilotStatusName',
        title: 'Trust Pilot Status',
        width: 500,
        postValueName: 'status',
        comboValues: this.model.get('TrustPilotStatusList'),
        url: "Underwriter/ApplicationInfo/UpdateTrustPilotStatus",
        data: {
          id: this.model.get('Id')
        }
      });
      d.render();
      d.on('done', function() {
        return _this.model.fetch();
      });
    };

    PersonInfoView.prototype.changeFraudStatusManualyClicked = function() {
      var fraudStatusModel, that, xhr,
        _this = this;

      fraudStatusModel = new EzBob.Underwriter.FraudStatusModel({
        customerId: this.model.get('Id'),
        currentStatus: this.model.get('FraudCheckStatusId')
      });
      BlockUi("on");
      that = this;
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
          var currentStatus;

          currentStatus = fraudStatusModel.get('currentStatus');
          _this.model.set('FraudCheckStatusId', currentStatus);
          _this.model.set('FraudCheckStatus', fraudStatusModel.get('currentStatusText'));
          return that.setAlertStatus(currentStatus !== 0, '.fraud-status', '.fraud-status-td');
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

    PersonInfoView.prototype.serializeData = function() {
      var data;

      data = this.model.toJSON();
      return {
        data: data,
        getIcon: this.templateHelpers.getIcon
      };
    };

    PersonInfoView.prototype.changeDisabledState = function() {
      var collectionStatusModel, customerId, prevStatus, xhr,
        _this = this;

      collectionStatusModel = new EzBob.Underwriter.CollectionStatusModel({
        customerId: this.model.get('Id'),
        currentStatus: this.model.get('CustomerStatusId')
      });
      prevStatus = this.model.get('CustomerStatusId');
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
            var isWarning, xhr2;

            BlockUi("on");
            isWarning = result;
            that.model.fetch();
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

    PersonInfoView.prototype.avoidAutomaticDecisionButton = function() {
      var d;

      d = new EzBob.Dialogs.CheckBoxEdit({
        model: this.model,
        propertyName: "IsAvoid",
        title: "Manual Decision",
        width: 350,
        postValueName: "enbaled",
        checkboxName: "Enable Manual Decision",
        url: "Underwriter/ApplicationInfo/AvoidAutomaticDecision",
        data: {
          id: this.model.get("Id")
        }
      });
      d.render();
    };

    PersonInfoView.prototype.editEmail = function() {
      var view;

      view = new EzBob.EmailEditView({
        model: this.model
      });
      EzBob.App.jqmodal.show(view);
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
      var status, _i, _len, _ref2, _results;

      this.on("change:FraudCheckStatusId", this.changeFraudCheckStatus, this);
      this.changeFraudCheckStatus();
      if (this.StatusesArr === void 0) {
        this.statuses = EzBob.Underwriter.StaticData.CollectionStatuses;
      }
      this.StatusesArr = {};
      _ref2 = this.statuses.models;
      _results = [];
      for (_i = 0, _len = _ref2.length; _i < _len; _i++) {
        status = _ref2[_i];
        _results.push(this.StatusesArr[status.get('Id')] = status.get('Name'));
      }
      return _results;
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
