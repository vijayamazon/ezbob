var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings || {};

EzBob.Underwriter.Settings.CampaignModel = Backbone.Model.extend({
	url: window.gRootPath + "Underwriter/StrategySettings/SettingsCampaign"
});

EzBob.Underwriter.Settings.CampaignView = Backbone.Marionette.ItemView.extend({
	template: "#campaign-settings-template",
	initialize: function (options) {
		this.model.on("reset change update", this.render, this);
		this.update();
		return this;
	},
	events: {
		"click .addCampaign": "addCampaign",
		"click .editCampaign": "editCampaign",
		"click .campaignCustomers": "campaignCustomers"
	},
	campaignCustomers: function (e) {
		var campaignId;
		campaignId = parseInt($(e.currentTarget).attr('data-campaign-id'));
		this.campaignCustomersView = new EzBob.Underwriter.Settings.CampaignCustomersView({
			campaign: this.getCampaign(campaignId)
		});
		return EzBob.App.jqmodal.show(this.campaignCustomersView);
	},
	editCampaign: function (e) {
		var campaignId;
		campaignId = parseInt($(e.currentTarget).attr('data-campaign-id'));
		return this.addCampaign(e, this.getCampaign(campaignId));
	},
	getCampaign: function (campaignId) {
		var campaign, campaigns;
		campaigns = this.model.get("campaigns");
		for (campaign in campaigns) {
			if (campaigns[campaign].Id === campaignId) {
				return campaigns[campaign];
			}
		}
	},
	addCampaign: function (e, campaign) {
		BlockUi("On");
		this.addCampaignView = new EzBob.Underwriter.Settings.AddCampaignView({
			model: this.model,
			campaign: campaign
		});
		this.addCampaignView.on('campaign-added', this.update, this);
		EzBob.App.jqmodal.show(this.addCampaignView);
		return BlockUi("Off");
	},
	serializeData: function () {
		var data;
		data = {
			campaigns: this.model.get('campaigns')
		};
		return data;
	},
	update: function () {
		if (this.addCampaignView) {
			EzBob.App.jqmodal.hideModal(this.addCampaignView);
		}
		this.model.fetch({ reset: true });
	},
	onRender: function () {
		if (!$("body").hasClass("role-manager") && !$("body").hasClass("role-Underwriter")) {
			this.$el.find("select").addClass("disabled").attr({
				readonly: "readonly",
				disabled: "disabled"
			});
			return this.$el.find("button").hide();
		}
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});

EzBob.Underwriter.Settings.AddCampaignView = Backbone.Marionette.ItemView.extend({
	template: "#add-campaign-template",
	initialize: function (options) {
		this.model.on("reset change update", this.render, this);
		this.isUpdate = false;
		if (options.campaign) {
			this.isUpdate = true;
			this.campaign = options.campaign;
		}
		this.update();
		return this;
	},
	events: {
		"click .addCampaignBtn": "addCampaign"
	},
	ui: {
		form: "form",
		name: "#campaign-name",
		description: "#campaign-description",
		type: "#campaign-type option",
		startdate: "#campaign-start-date",
		enddate: "#campaign-end-date",
		clients: "#campaign-customers",
		addCampaignBtn: ".addCampaignBtn"
	},
	addCampaign: function () {
		var data, ok, that, xhr;
		BlockUi("on");
		data = this.ui.form.serialize();
		if (this.isUpdate) {
			data += "&campaignId=" + this.campaign.Id;
		}
		that = this;
		ok = function () {
			return that.trigger('campaign-added');
		};
		xhr = $.post("" + window.gRootPath + "Underwriter/StrategySettings/AddCampaign/?" + data);
		xhr.done(function (res) {
			res.errorText = res.errorText ? res.errorText : res.error ? res.error : "";
			if (!res.success) {
				EzBob.ShowMessage("Failed to add/update the campaign. " + res.errorText, "Failure", null, "OK");
				return;
			}
			return EzBob.ShowMessage("Successfully Added/Updated. " + res.errorText, "The campaign added/updated successfully.", ok, "OK");
		});
		xhr.fail(function () {
			return EzBob.ShowMessage("Failed to add/update the campaign. ", "Failure", null, "OK");
		});
		xhr.always(function () {
			return BlockUi("off");
		});
		return false;
	},
	update: function () {
		this.model.fetch({ reset: true });
	},
	onRender: function () {
		var that;
		this.$el.find('input.date').datepicker({
			format: 'dd/mm/yyyy'
		});
		that = this;
		if (this.isUpdate) {
			this.ui.name.val(this.campaign.Name);
			this.ui.description.val(this.campaign.Description);
			this.$el.find("#campaign-type option").filter(function () {
				return this.text === that.campaign.Type;
			}).prop('selected', true);
			this.ui.startdate.val(EzBob.formatDate2(this.campaign.StartDate));
			this.ui.enddate.val(EzBob.formatDate2(this.campaign.EndDate));
			this.ui.clients.val(_.pluck(this.campaign.Customers, 'Id').join().replace(/,/g, ' '));
			this.ui.addCampaignBtn.html('Update Campaign');
		}
	},
	jqoptions: function () {
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
	},
	serializeData: function () {
		var data;
		data = {
			campaignTypes: this.model.get('campaignTypes')
		};
		return data;
	}
});

EzBob.Underwriter.Settings.CampaignCustomersView = Backbone.Marionette.ItemView.extend({
	template: "#campaign-customers-template",
	initialize: function (options) {
		if (options.campaign) {
			this.campaign = options.campaign;
		}
		return this;
	},
	jqoptions: function () {
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
	},
	serializeData: function () {
		var data;
		data = {
			campaign: this.campaign
		};
		return data;
	},
	show: function (type) {
		return this.$el.show();
	},
	hide: function () {
		return this.$el.hide();
	}
});
