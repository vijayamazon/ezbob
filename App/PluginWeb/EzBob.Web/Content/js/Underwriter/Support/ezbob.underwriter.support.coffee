root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.SupportView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.SupportModel()
        @model.on "change reset", @render, @
        @model.fetch()
        @preHeight = 20

    template: "#support-template"

    events: 
        "click pre": "preClicked"
        "click .reCheckMP": "recheckClicked"
        'click [data-sort-type]': 'sortClicked'

    onRender: ->
        @$el.find("[data-sort-type]").css('cursor', 'pointer')
        @$el.find("pre").tooltip({title: 'Click to see detail info'}).tooltip('fixTitle')
        @$el.find("pre").height(@preHeight).css("overflow", "hidden").css('cursor','pointer')
        (@$el.find '.arrow').hide()
        arrow = @$el.find "[data-sort-type=#{@model.get('sortField')}] .arrow"
        arrow.show()
        arrow.removeClass().addClass(if @model.get('sortType') == 'asc' then 'arrow icon-arrow-up' else 'arrow icon-arrow-down')
        BlockUi("off")
        EzBob.handleUserLayoutSetting()

    sortClicked: (e) ->
        BlockUi("on")
        $el = $ e.currentTarget
        field = $el.data 'sort-type'
        currentField = @model.get 'sortField'
        currentSortType = @model.get 'sortType'
        @model.set
            'sortField': field
            'sortType': if field != currentField or currentSortType == 'desc' then 'asc' else 'desc'
        ,
            silent: true
        @model.fetch()

    recheckClicked: (e)->
        $el = $(e.currentTarget)
        return false if $el.hasClass('disabled')
        umi = $el.attr "umi"
        mpType = $el.attr "marketplaceType"
        customerId = @model.customerId
        okFn = =>
            $el.addClass('disabled')
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
        BlockUi 'off'

    show: ->
        @$el.show()
        @modelUpdater = setInterval(=> 
            @model.fetch()
        , 2000 )

    serializeData: ->
        model: @model.get 'models'

class EzBob.Underwriter.SupportModel extends Backbone.Model
    initialize:->
        @set {
            sortField: 4
            sortType: 'desc' #desc/asc
            models: []
            }, {
            silent: true
            }

    urlRoot: -> "#{gRootPath}Underwriter/Support/Index?sortField=#{@get('sortField')}&sortType=#{@get('sortType')}"