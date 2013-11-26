root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.Settings = EzBob.Underwriter.Settings or {}

class EzBob.Underwriter.Settings.CampaignModel extends Backbone.Model
    url: window.gRootPath + "Underwriter/StrategySettings/SettingsCampaign"

class EzBob.Underwriter.Settings.CampaignView extends Backbone.Marionette.ItemView
    template: "#campaign-settings-template"

    initialize: (options) ->
        @model.on "reset", @render, @
        @update()
        @


    events:
        "click .addCampaign": "addCampaign"
        "click .editCampaign": "editCampaign"
        "click .campaignCustomers" : "campaignCustomers"

    campaignCustomers: (e) ->
        clients = $(e.currentTarget).attr('data-campaign-clients')
        if not clients
            clients = 'No clients in this campaign'
        EzBob.ShowMessage(clients , "Campaign clients", null, "OK")

    editCampaign: (e) ->
        campaignId = parseInt($(e.currentTarget).attr('data-campaign-id'))
        campaigns = @model.get("campaigns")
        
        for campaign of campaigns
            if campaigns[campaign].Id == campaignId
                @addCampaign(e, campaigns[campaign])
                break
            


    addCampaign: (e, campaign)->
        BlockUi("On")
        @addCampaignView = new EzBob.Underwriter.Settings.AddCampaignView(model: @model, campaign: campaign)
        @addCampaignView.on('campaign-added', @update, @)
        EzBob.App.jqmodal.show(@addCampaignView)
        BlockUi("Off")

    serializeData: ->
        data = 
            campaigns: @model.get('campaigns')
        return data

    update: ->
        if @addCampaignView 
            EzBob.App.jqmodal.hideModal(@addCampaignView)
        @model.fetch().done => 
            @render()

    onRender: -> 
        if !$("body").hasClass("role-manager") 
            @$el.find("select").addClass("disabled").attr({readonly:"readonly", disabled: "disabled"})
            @$el.find("button").hide()

    show: (type) ->
        this.$el.show()

    hide: () ->
        this.$el.hide()

    onClose: ->
        

class EzBob.Underwriter.Settings.AddCampaignView extends Backbone.Marionette.ItemView
    template: "#add-campaign-template"

    initialize: (options) ->
        @model.on "reset", @render, @
        @isUpdate = false
        if(options.campaign)
            @isUpdate = true
            @campaign = options.campaign
        @update()
        @

    events:
        "click .addCampaignBtn": "addCampaign"

    ui:
        form : "form"
        name: "#campaign-name"
        description: "#campaign-description"
        type: "#campaign-type option"
        startdate: "#campaign-start-date"
        enddate: "#campaign-end-date"
        clients: "#campaign-customers"
        addCampaignBtn: ".addCampaignBtn"

    addCampaign: ->
        BlockUi "on"
        data = @ui.form.serialize()
        if @isUpdate
            data += "&campaignId=#{@campaign.Id}"
        that = @
        ok = () =>
            that.trigger('campaign-added')

        xhr = $.post("#{window.gRootPath}Underwriter/StrategySettings/AddCampaign/?#{data}")
        xhr.done (res) ->
            res.errorText = if res.errorText then res.errorText else if res.error then res.error else ""
            if(not res.success)
                EzBob.ShowMessage("Failed to add/update the campaign. #{res.errorText}" , "Failure", null, "OK")
                return
            EzBob.ShowMessage("Successfully Added/Updated. #{res.errorText}", "The campaign added/updated successfully.", ok, "OK")
        xhr.fail -> EzBob.ShowMessage("Failed to add/update the campaign. ", "Failure", null, "OK")
        xhr.always -> BlockUi "off"
        false

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    onRender: ->
        @$el.find('input.date').datepicker(format: 'dd/mm/yyyy')
        #@$el.find('input[data-content], span[data-content]').setPopover()
        that = @
        if @isUpdate
            @ui.name.val(@campaign.Name)
            @ui.description.val @campaign.Description
            @$el.find("#campaign-type option").filter(-> 
                return @.text is that.campaign.Type
            ).prop 'selected', true
            @ui.startdate.val EzBob.formatDate2(@campaign.StartDate)
            @ui.enddate.val EzBob.formatDate2(@campaign.EndDate)
            @ui.clients.val @campaign.Customers.replace(/, /g,' ')
            @ui.addCampaignBtn.html 'Update Campaign'


    jqoptions: ->
        {
            modal: true
            resizable: false
            title: if @isUpdate then "Update campaign" else "Add Campaign"
            position: "center"
            draggable: false
            width: "40%"
            height: 670
            dialogClass: "addCampaign"
        }

    serializeData: ->
        data = 
            campaignTypes: @model.get('campaignTypes')
        return data
