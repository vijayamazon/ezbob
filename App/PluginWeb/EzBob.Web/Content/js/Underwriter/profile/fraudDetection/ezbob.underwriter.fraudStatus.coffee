root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.FraudStatusModel extends Backbone.Model
    urlRoot: -> "#{window.gRootPath}Underwriter/FraudStatus/Index?Id=#{@get('customerId')}"

class EzBob.FraudStatusItemsView extends Backbone.Marionette.ItemView
    template: '#fraud-status-items-template'

class EzBob.Underwriter.FraudStatusLayout extends Backbone.Marionette.Layout
    template: '#fraud-status-layout-template'
    
    initialize: () ->
        @modelBinder = new Backbone.ModelBinder()

    bindings:
        currentStatus:
            selector: "input[name='currentStatus']"
        customerId:
            selector:"input[name='customerId']"

    regions: 
        list: '#list-fraud-items'
        content: '#fraud-view'

    events: 
        'change #fraud-status-items' : 'changeStatus'
 
    changeStatus: ->
        currentStatusId = $("#fraud-status-items option:selected").val()
        currentStatus = $("#fraud-status-items option:selected").text()
        @model.set "currentStatus": parseInt(currentStatusId)
        @model.set "currentStatusText": currentStatus
        @renderStatusValue()
        @

    renderStatusValue: =>
        @model.fetch().done =>
            @$el.find('#fraud-view').show()
        false

    save : =>
        BlockUi "on"
        form = @$el.find('form')
        postData = form.serialize() 
        action = "#{window.gRootPath}Underwriter/FraudStatus/Save/"
        $.post(action, postData )
        .done =>
            @trigger('saved')
            @close()
        .complete ->
            BlockUi "off"
        false


    onRender: ->
        @modelBinder.bind @model, @el, @bindings
        common = new EzBob.FraudStatusItemsView 
        @list.show(common)
        @$el.find("#fraud-status-items [value='#{@model.get('CurrentStatusId')}']").attr("selected", "selected")
        @renderStatusValue()
        @

    jqoptions: ->
        modal: true
        resizable: false
        title: 'Fraud Status'
        draggable: true
        width: "400"
        buttons:
            "OK": @onSave
            "Cancel": @onCancel
    
    onCancel: =>
        @close()
    
    onSave: =>
        @save()
