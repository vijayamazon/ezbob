root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter || {}

class EzBob.Underwriter.FundingView extends Backbone.Marionette.ItemView
    initialize: ->
        @model = new EzBob.Underwriter.FundingModel()
        @model.on "change reset", @render, @
        @model.fetch()
        
        @requiredFunds = -1

        xhr1 = $.post "#{window.gRootPath}Underwriter/Funding/GetRequiredFunds"
        xhr1.done (res) =>
            @requiredFunds = res
            @render()

        xhr = $.post "#{window.gRootPath}Underwriter/Funding/GetAvailableFundsInterval"
        xhr.done (res) =>
            @modelUpdater = setInterval(=> 
                @model.fetch()
            , res )

    template: "#funding-template"

    events:
        "click #addFundsBtn": "addFunds"
        "click #cancelManuallyAddedFundsBtn": "cancelManuallyAddedFunds"

    addFunds: (e) ->
        console.log('placeholder')

    cancelManuallyAddedFunds: (e) ->
        console.log('placeholder')

    onRender: ->
        if !$("body").hasClass("role-manager")
            @$el.find('#addFundsBtn').hide()
            @$el.find('#cancelManuallyAddedFundsBtn').hide()

        li = $(document.getElementById("liFunding"))
        if (@requiredFunds > @model.get('AvailableFunds'))
            if !li.hasClass('available-funds-alert')
                li.addClass('available-funds-alert')
        else
            if li.hasClass('available-funds-alert')
                li.removeClass('available-funds-alert')

    hide: ->
        @$el.hide()

    show: ->
        @$el.show()

    serializeData: ->
        model: @model

class EzBob.Underwriter.FundingModel extends Backbone.Model
    urlRoot: -> "#{gRootPath}Underwriter/Funding/GetCurrentFundingStatus"