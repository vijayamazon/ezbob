root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.SupportView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.SupportModels()
        @model.on "change", @render, @
        @model.fetch().done =>
            @render()
        @preHeight = 20

    template: "#support-template"

    events: 
        "click pre": "preClicked"
        "click .reCheckMP": "recheckClicked"

    recheckClicked: (e)->
        $el = $(e.currentTarget)
        umi = $el.attr "umi"
        mpType = $el.attr "marketplaceType"
        customerId = @model.customerId
        okFn = =>
            xhr = $.get "#{window.gRootPath}Underwriter/MarketPlaces/ReCheckMarketplaces",
                customerId: customerId
                umi: umi
                marketplaceType: mpType
            xhr.done (response)=>
                if response and response.error != undefined
                    EzBob.ShowMessage response.error, "Error occured"
                else
                    EzBob.ShowMessage "Wait a few minutes", "The marketplace recheck is running. ", null, "OK"
                @trigger "rechecked",
                    umi: umi
                    el: $el
            xhr.fail (data) ->
                console.error data.responseText
        EzBob.ShowMessage "", "Are you sure?", okFn, "Yes", null, "No"
        false

    preClicked: (e)->
        $el = $ e.currentTarget
        elHeight = $el.height()
        $el.height if elHeight != @preHeight then @preHeight else "auto"
        $el.tooltip('destroy')
        $el.tooltip({title: if elHeight != @preHeight then 'Click to see detail info' else 'Click to hide detail info'})
        $el.tooltip("enable").tooltip('fixTitle')

    hide: ->
        @$el.hide()
        clearInterval(@modelUpdater)

    show: ->
        @$el.show()
        @modelUpdater = setInterval(=> 
            @model.fetch()
        , 2000 )

    serializeData: ->
        model: @model.toJSON()

    onRender: ->
        @$el.find("pre").tooltip({title: 'Click to see detail info'}).tooltip('fixTitle')
        @$el.find("pre").height(@preHeight).css("overflow", "hidden").css('cursor','pointer')

class EzBob.Underwriter.SupportModel extends Backbone.Model

class EzBob.Underwriter.SupportModels extends Backbone.Collection
    model: EzBob.Underwriter.SupportModel

    url: "#{gRootPath}Underwriter/Support/Index"