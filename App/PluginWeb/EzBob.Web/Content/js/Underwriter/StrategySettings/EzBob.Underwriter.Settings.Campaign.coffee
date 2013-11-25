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
        "click .campaignCustomers" : "campaignCustomers"

    campaignCustomers: (e) ->
        clients = $(e.currentTarget).attr('data-campaign-clients')
        if not clients
            clients = 'No clients in this campaign'
        EzBob.ShowMessage(clients , "Campaign clients", null, "OK")

    addCampaign: ->
        BlockUi("On")
        @addCampaignView = new EzBob.Underwriter.Settings.AddCampaignView(model: @model)
        @addCampaignView.on('campaign-added', @update, @)
        EzBob.App.jqmodal.show(@addCampaignView)
        BlockUi("Off")

    serializeData: ->
        data = 
            campaigns: @model.get('campaigns')

        console.log('ser', data)
       # _.each data.campaigns, (campaign) -> campaign.customersStr = campaign.customers.join()
       # console.log('ser2', data)
        return data

    update: ->
        console.log('update')
        if @addCampaignView 
            EzBob.App.jqmodal.hideModal(@addCampaignView)
        console.log('update2')
        @model.fetch().done => 
            console.log("@model", @model)
            @render()
        console.log('update3')

    onRender: -> 
        console.log('render')
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
        @update()
        @

    events:
        "click .addCampaignBtn": "addCampaign"

    ui:
        form : "form"

    addCampaign: ->
        BlockUi "on"
        data = @ui.form.serialize()
        console.log data
        that = @
        ok = () =>
            that.trigger('campaign-added')

        xhr = $.post("#{window.gRootPath}Underwriter/StrategySettings/AddCampaign/?#{data}")
        xhr.done (res) ->
            console.log 'res', res
            if(not res.success)
                EzBob.ShowMessage("Failed to add the campaign. #{res.error}" , "Failure", ok, "OK")
                return
            EzBob.ShowMessage("Successfully Added. #{res.error}", "The campaign added successfully.", ok, "OK")
        xhr.fail -> EzBob.ShowMessage("Failed to add the campaign. ", "Failure", ok, "OK")
        xhr.always -> BlockUi "off"
        false

    update: ->
        xhr = @model.fetch()
        xhr.done => @render()

    onRender: ->
        @$el.find('input.date').datepicker(format: 'dd/mm/yyyy')
        @$el.find('input[data-content], span[data-content]').setPopover()

    jqoptions: ->
        {
            modal: true
            resizable: false
            title: "Add campaign"
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
