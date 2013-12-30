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
      "click .editCampaign": "editCampaign",
      "click .campaignCustomers": "campaignCustomers"
    };

    CampaignView.prototype.campaignCustomers = function(e) {
      var campaignId;
      campaignId = parseInt($(e.currentTarget).attr('data-campaign-id'));
      this.campaignCustomersView = new EzBob.Underwriter.Settings.CampaignCustomersView({
        campaign: this.getCampaign(campaignId)
      });
      return EzBob.App.jqmodal.show(this.campaignCustomersView);
    };

    CampaignView.prototype.editCampaign = function(e) {
      var campaignId;
      campaignId = parseInt($(e.currentTarget).attr('data-campaign-id'));
      return this.addCampaign(e, this.getCampaign(campaignId));
    };

    CampaignView.prototype.getCampaign = function(campaignId) {
      var campaign, campaigns;
      campaigns = this.model.get("campaigns");
      for (campaign in campaigns) {
        if (campaigns[campaign].Id === campaignId) {
          return campaigns[campaign];
        }
      }
    };

    CampaignView.prototype.addCampaign = function(e, campaign) {
      BlockUi("On");
      this.addCampaignView = new EzBob.Underwriter.Settings.AddCampaignView({
        model: this.model,
        campaign: campaign
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
      return data;
    };

    CampaignView.prototype.update = function() {
      var _this = this;
      if (this.addCampaignView) {
        EzBob.App.jqmodal.hideModal(this.addCampaignView);
      }
      return this.model.fetch().done(function() {
        return _this.render();
      });
    };

    CampaignView.prototype.onRender = function() {
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
      this.isUpdate = false;
      if (options.campaign) {
        this.isUpdate = true;
        this.campaign = options.campaign;
      }
      this.update();
      return this;
    };

    AddCampaignView.prototype.events = {
      "click .addCampaignBtn": "addCampaign"
    };

    AddCampaignView.prototype.ui = {
      form: "form",
      name: "#campaign-name",
      description: "#campaign-description",
      type: "#campaign-type option",
      startdate: "#campaign-start-date",
      enddate: "#campaign-end-date",
      clients: "#campaign-customers",
      addCampaignBtn: ".addCampaignBtn"
    };

    AddCampaignView.prototype.addCampaign = function() {
      var data, ok, that, xhr,
        _this = this;
      BlockUi("on");
      data = this.ui.form.serialize();
      if (this.isUpdate) {
        data += "&campaignId=" + this.campaign.Id;
      }
      that = this;
      ok = function() {
        return that.trigger('campaign-added');
      };
      xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/AddCampaign/?" + data);
      xhr.done(function(res) {
        res.errorText = res.errorText ? res.errorText : res.error ? res.error : "";
        if (!res.success) {
          EzBob.ShowMessage("Failed to add/update the campaign. " + res.errorText, "Failure", null, "OK");
          return;
        }
        return EzBob.ShowMessage("Successfully Added/Updated. " + res.errorText, "The campaign added/updated successfully.", ok, "OK");
      });
      xhr.fail(function() {
        return EzBob.ShowMessage("Failed to add/update the campaign. ", "Failure", null, "OK");
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
      var that;
      this.$el.find('input.date').datepicker({
        format: 'dd/mm/yyyy'
      });
      that = this;
      if (this.isUpdate) {
        this.ui.name.val(this.campaign.Name);
        this.ui.description.val(this.campaign.Description);
        this.$el.find("#campaign-type option").filter(function() {
          return this.text === that.campaign.Type;
        }).prop('selected', true);
        this.ui.startdate.val(EzBob.formatDate2(this.campaign.StartDate));
        this.ui.enddate.val(EzBob.formatDate2(this.campaign.EndDate));
        this.ui.clients.val(_.pluck(this.campaign.Customers, 'Id').join().replace(/,/g, ' '));
        return this.ui.addCampaignBtn.html('Update Campaign');
      }
    };

    AddCampaignView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: this.isUpdate ? "Update campaign" : "Add Campaign",
        position: "center",
        draggable: false,
        width: "45%",
        height: 690,
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

  EzBob.Underwriter.Settings.CampaignCustomersView = (function(_super) {

    __extends(CampaignCustomersView, _super);

    function CampaignCustomersView() {
      return CampaignCustomersView.__super__.constructor.apply(this, arguments);
    }

    CampaignCustomersView.prototype.template = "#campaign-customers-template";

    CampaignCustomersView.prototype.initialize = function(options) {
      if (options.campaign) {
        this.campaign = options.campaign;
      }
      return this;
    };

    CampaignCustomersView.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: false,
        title: "Campaign " + this.campaign.Name + " clients",
        position: "center",
        draggable: true,
        width: "40%",
        height: 670,
        dialogClass: "CampaignClients"
      };
    };

    CampaignCustomersView.prototype.serializeData = function() {
      var data;
      data = {
        campaign: this.campaign
      };
      return data;
    };

    CampaignCustomersView.prototype.show = function(type) {
      return this.$el.show();
    };

    CampaignCustomersView.prototype.hide = function() {
      return this.$el.hide();
    };

    return CampaignCustomersView;

  })(Backbone.Marionette.ItemView);

}).call(this);
