root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.ProfilePopupModel extends Backbone.Model
    idAttribute: "Id"

    url: -> "#{window.gRootPath}Underwriter/Profile/GetRegisteredCustomerInfo/?customerId=#{@customerId}"

    defaults:
        title: "&nbsp;"
        isWizardFinished: false

    initialize: -> 
        interval = setInterval (=>
            @fetch()
        ), 2000
        @set "interval", interval

class EzBob.Underwriter.ProfilePopupView extends Backbone.Marionette.ItemView
    initialize: (options)->
        @model = new EzBob.Underwriter.ProfilePopupModel()
        @model.customerId = options.customerId
        @bindTo @model, 'change reset fetch', @render, @
        @model.fetch()

    serializeData: ->
        mps: @model.get "mps"
        sm:  (@model.get "sm") or {}
        title: (@model.get "title") or ""
        isWizardFinished: @model.get "isWizardFinished"
        cid : @model.customerId
        cName : @model.get "cName"

    template: "#profile-popup-view-template"

    jqoptions: ->
        modal: true
        resizable: false
        title: "Registered"
        position: "center"
        draggable: false
        width: "830"
        dialogClass: "registered-popup"

    events:
        "click .recheck-mp": "recheckMpClicked"
        "click .recheck-yodlee": "recheckYodleeClicked"
        "click #recheck-main-strat": "recheckMainStrat"

    recheckMpClicked: (e)->
        $el = $(e.currentTarget)
        return if $el.hasClass "disabled"
        $el.addClass 'disabled'
        model = 
            umi: $(e.currentTarget).data('mp-id')
        xhr = $.post "#{window.gRootPath}Underwriter/MarketPlaces/ReCheckMarketplaces", model

    recheckYodleeClicked: (e) ->
        $el = $(e.currentTarget)
        return if $el.hasClass "disabled"
        #$el.addClass 'disabled'
        ###
        model = 
            umi: $(e.currentTarget).data('mp-id')
        xhr = $.post "#{window.gRootPath}Underwriter/MarketPlaces/TryRecheckYodlee", model
        ###

    recheckMainStrat: (e)->
        $el = $(e.currentTarget)
        return if $el.hasClass "disabled"
        $el.addClass 'disabled'
        model = 
            customerId: @model.customerId
        xhr = $.post "#{window.gRootPath}Underwriter/Profile/StartMainStrategy", model

    onClose: ->
        clearInterval @model.get('interval')

