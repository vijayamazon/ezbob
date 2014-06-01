root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.FundingView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.FundingModel()
        @model.on "change reset", @render, @
        @model.fetch()

    template: "#funding-template"

    events:
        "click #addFundsBtn": "addFunds"
        "click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds"

    onRender: ->
        console.log('placeholder')

    addFunds: (e) ->
        console.log('placeholder')

    cancelManuallyAddedFunds: (e) ->
        console.log('placeholder')

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
        model: @model

class EzBob.Underwriter.FundingModel extends Backbone.Model
    urlRoot: -> "#{gRootPath}Underwriter/Funding/GetCurrentFundingStatus"