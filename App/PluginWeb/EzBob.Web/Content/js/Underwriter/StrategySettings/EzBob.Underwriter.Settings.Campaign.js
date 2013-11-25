(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

  EzBob.Underwriter.Settings.CampaignModel = (function(_super) {

    __extends(CampaignModel, _super);

    function CampaignModel() {
      return CampaignModel.__super__.constructor.apply(this, arguments);
    }

    CampaignModel.prototype.url = window.gRootPath + "Underwriter/StrategySettings/SettingsCampaign";

    return CampaignModel;

  })(Backbone.Model);

  EzBob.Underwriter.Settings.CampaignView = (function(_super) {

    __extends(CampaignView, _super);

    function CampaignView() {
      return CampaignView.__super__.constructor.apply(this, arguments);
    }

    CampaignView.prototype.template = "#campaign-settings-template";

    CampaignView.prototype.initialize = function(options) {
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    CampaignView.prototype.events = {
      "click .addCampaign": "addCampaign",
      "click .campaignCustomers": "campaignCustomers"
    };

    CampaignView.prototype.campaignCustomers = function(e) {
      console.log(e, $(e));
      return EzBob.ShowMessage($(e.currentTarget).attr('data-campaign-clients'), "Campaign clients", null, "OK");
    };

    CampaignView.prototype.addCampaign = function() {
      BlockUi("On");
      this.addCampaignView = new EzBob.Underwriter.Settings.AddCampaignView({
        model: this.model
      });
      this.addCampaignView.on('campaign-added', this.update, this);
      EzBob.App.jqmodal.show(this.addCampaignView);
      return BlockUi("Off");
    };

    CampaignView.prototype.serializeData = function() {
      var data;
      data = {
        campaigns: this.model.get('campaigns')
      };
      console.log('ser', data);
      return data;
    };

    CampaignView.prototype.update = function() {
      var _this = this;
      console.log('update');
      if (this.addCampaignView) {
        EzBob.App.jqmodal.hideModal(this.addCampaignView);
      }
      console.log('update2');
      this.model.fetch().done(function() {
        console.log("@model", _this.model);
        return _this.render();
      });
      return console.log('update3');
    };

    CampaignView.prototype.onRender = function() {
      console.log('render');
      if (!$("body").hasClass("role-manager")) {
        this.$el.find("select").addClass("disabled").attr({
          readonly: "readonly",
          disabled: "disabled"
        });
        return this.$el.find("button").hide();
      }
    };

    CampaignView.prototype.show = function(type) {
      return this.$el.show();
    };

    CampaignView.prototype.hide = function() {
      return this.$el.hide();
    };

    CampaignView.prototype.onClose = function() {};

    return CampaignView;

  })(Backbone.Marionette.ItemView);

  EzBob.Underwriter.Settings.AddCampaignView = (function(_super) {

    __extends(AddCampaignView, _super);

    function AddCampaignView() {
      return AddCampaignView.__super__.constructor.apply(this, arguments);
    }

    AddCampaignView.prototype.template = "#add-campaign-template";

    AddCampaignView.prototype.initialize = function(options) {
      this.model.on("reset", this.render, this);
      this.update();
      return this;
    };

    AddCampaignView.prototype.events = {
      "click .addCampaignBtn": "addCampaign"
    };

    AddCampaignView.prototype.ui = {
      form: "form"
    };

    AddCampaignView.prototype.addCampaign = function() {
      var data, ok, that, xhr,
        _this = this;
      BlockUi("on");
      data = this.ui.form.serialize();
      console.log(data);
      that = this;
      ok = function() {
        return that.trigger('campaign-added');
      };
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/AddCampaign/?" + data);
      xhr.done(function(res) {
        console.log('res', res);
        if (!res.success) {
          EzBob.ShowMessage("Failed to add the campaign. " + res.error, "Failure", ok, "OK");
          return;
        }
        return EzBob.ShowMessage("Successfully Added. " + res.error, "The campaign added successfully.", ok, "OK");
      });
      xhr.fail(function() {
        return EzBob.ShowMessage("Failed to add the campaign. ", "Failure", ok, "OK");
      });
      xhr.always(function() {
        return BlockUi("off");
      });
      return false;
    };

    AddCampaignView.prototype.update = function() {
      var xhr,
        _this = this;
      xhr = this.model.fetch();
      return xhr.done(function() {
        return _this.render();
      });
    };

    AddCampaignView.prototype.onRender = function() {
      this.$el.find('input.date').datepicker({
        format: 'dd/mm/yyyy'
      });
      return this.$el.find('input[data-content], span[data-content]').setPopover();
    };

    AddCampaignView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Add campaign",
        position: "center",
        draggable: false,
        width: "40%",
        height: 670,
        dialogClass: "addCampaign"
      };
    };

    AddCampaignView.prototype.serializeData = function() {
      var data;
      data = {
        campaignTypes: this.model.get('campaignTypes')
      };
      return data;
    };

    return AddCampaignView;

  })(Backbone.Marionette.ItemView);

}).call(this);
