root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.BugModel extends Backbone.Model
    url: -> "#{window.gRootPath}Underwriter/Bugs/Report"
    idAttribute: 'Id'
    defaults:
        TextOpened: ""
        Type: "Other"
        DateOpened: new Date()
        State: 'Opened'
        MarketPlaceId: null

    save: ->
        if @isNew()
            super({}, {url: "#{window.gRootPath}Underwriter/Bugs/CreateBug"})
        else
            super({}, {url: "#{window.gRootPath}Underwriter/Bugs/UpdateBug"})

class EzBob.Underwriter.Bugs extends Backbone.Collection
    model: EzBob.Underwriter.BugModel
    #url: "#{window.gRootPath}Underwriter/Bugs/Report"

class EzBob.Underwriter.ReportBugView  extends EzBob.BoundItemView
    template: '#bug-report-template'

    events:
        'click  [data-dismiss="modal"]': "closed"

    bindings:
        TextOpened:
            selector: "textarea[name='description']"

    closed: ->
        @trigger "closed"

class EzBob.Underwriter.EditBugView extends EzBob.BoundItemView
    events:
        'click .btn-danger' : 'closeBug'
        'click .btn-warning': 'reopenBug'
        'click  [data-dismiss="modal"]': "closed"

    template: '#bug-edit-template'

    eqConverter: (v) ->
        (direction, value) -> 
            value is v
    notEqConverter: (v) ->
        (direction, value) -> 
            value isnt v
    
    bindings:
        TextOpened:
            selector: "textarea[name='description']"
        TextClosed:
            selector: "textarea[name='closeDescription']"
        State: [
            {selector: "textarea", converter: @::notEqConverter('Closed'), elAttribute: 'enabled'},
            {selector: ".btn-danger", converter: @::notEqConverter('Closed'), elAttribute: 'displayed'},
            {selector: ".underwriter-closed", converter: @::eqConverter('Closed'), elAttribute: 'displayed'},
            {selector: ".btn-warning", converter: @::eqConverter('Closed'), elAttribute: 'displayed'},
            {selector: ".btn-primary", converter: @::notEqConverter('Closed'), elAttribute: 'enabled'}
        ]

    closeBug: ->
        @trigger('closeBug')
        @close()

    reopenBug: ->
        @trigger "reopenBug"

    closed: ->
        @trigger "closed"

$('body').on 'click', 'a[data-bug-type]', (e) ->
    $e = $(e.currentTarget)
    bugType = $e.data 'bug-type'
    bugMP = $e.data 'bug-mp'
    bugCustomer = $e.data 'bug-customer'
    director = $e.data 'credit-bureau-director-id'

    return false unless bugType? && bugCustomer?

    xhr = $.getJSON "#{window.gRootPath}Underwriter/Bugs/TryGet", {MP: bugMP, CustomerId: bugCustomer, BugType: bugType, Director: director}
    xhr.done (data) =>
        return if (data?.error)

        view = null
        model = null

        if data?.Id
            model = new EzBob.Underwriter.BugModel data
            view = new EzBob.Underwriter.EditBugView( model: model)
            view.on 'closeBug', ->
                req = $.post "#{window.gRootPath}Underwriter/Bugs/Close", model.toJSON()
                req.done (response)-> 
                    EzBob.UpdateBugsIcon $e, response.State
                    view.close()

            view.on 'reopenBug', ->
                req = $.post "#{window.gRootPath}Underwriter/Bugs/Reopen", model.toJSON()
                req.done (response)->
                    model = new EzBob.Underwriter.BugModel response
                    view.model = model
                    view.render()
                    EzBob.UpdateBugsIcon $e, response.State

            view.on "closed", ->
                EzBob.UpdateBugsIcon $e, model.get('State')
        else
            model = new EzBob.Underwriter.BugModel
                Type : bugType
                CustomerId: bugCustomer
                MarketPlaceId: bugMP
                DirectorId: director
            view = new EzBob.Underwriter.ReportBugView( model: model)
            EzBob.UpdateBugsIcon $e, model.get('State')
            view.on "closed", ->
                EzBob.UpdateBugsIcon $e, "New"

        view.on 'save', ->
            model.save() unless model.get("State") is "Closed"
            view.close()

        view.options = {show: true, keyboard: false, focusOn: "textarea:first"}
        EzBob.App.jqmodal.show view
    false
