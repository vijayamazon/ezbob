﻿root = exports ? this
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
        #currentStatus = @model.get "CurrentStatus"
        #if currentStatus == 3 or currentStatus == 4
        @model.fetch().done =>
            @$el.find('#fraud-view').show()
        #    FraudStatusView = new EzBob.Underwriter.FraudStatusView model:@model
        #    @content.show(FraudStatusView)
        #else
        #    @$el.find('#fraud-view').hide()
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

class EzBob.Underwriter.FraudModel extends Backbone.Model
    defaults: ->
        FirstName: ""
        LastName: ""
        Addresses: []
        Phones: []
        Emails: []
        EmailDomains: []
        BankAccounts: []
        Companies: []
        Shops: []

    sendToServer: ->
        xhr = Backbone.sync "create", @, url: "#{gRootPath}Underwriter/Fraud/AddNewUser"
        xhr.complete =>
            @trigger "saved"

class EzBob.Underwriter.FraudModels extends Backbone.Collection
    url: "#{gRootPath}Underwriter/Fraud/GetAll"

    model: EzBob.Underwriter.FraudModel

class EzBob.Underwriter.simpleValueAddView extends Backbone.Marionette.ItemView
    initialize: (options)->
        @template = options.template
        @type = options.type

    events:
        "click .ok":"okClicked"

    ui:
        form: "form"

    onRender: ->
        #addresses can contains null. Skip required for it.
        if @type != "Addresses"
            @ui.form.find("input, textarea").addClass('required')
        @validator = @ui.form.validate
            errorPlacement: EzBob.Validation.errorPlacement
            unhighlight: EzBob.Validation.unhighlight
        @

    okClicked: ->
        return unless @validator.form()
        model = new Backbone.Model(SerializeArrayToEasyObject(@ui.form.serializeArray()))
        @trigger "added", {model: model, type: @type}
        @close()
        false

class EzBob.Underwriter.SimpleValueView extends Backbone.Marionette.ItemView
    initialize: (options)->
        @template = options.template

    serializeData: ->
        models: @model

class EzBob.Underwriter.AddEditFraudView extends Backbone.Marionette.ItemView
    template: "#fraud-add-edit-template"

    ui:
        form: "form"

    onRender: ->
        @ui.form.find("input, textarea").addClass('required')
        @validator = @ui.form.validate
            errorPlacement: EzBob.Validation.errorPlacement
            unhighlight: EzBob.Validation.unhighlight

    events:
        "click .save":"saveButtonClicked"
        "click .add":"addClicked"
        "click .remove": "removeClicked"

    removeClicked: (e)->
        $el = ($ e.currentTarget)
        index = $el.data "index"
        type = $el.data "type"
        (@model.get type).splice(index, 1)
        @reRenderArea(type)
        false

    saveButtonClicked: ->
        isValid = @validator.form()
        if not isValid
            @ui.form.closest('.modal-body').animate({ scrollTop: 0 }, 500)
            return 
        formData = SerializeArrayToEasyObject(@ui.form.serializeArray())
        @model.set
            FirstName: formData.FirstName
            LastName: formData.LastName
        @model.sendToServer()
        @close()

    addClicked: (e)->
        $el = ($ e.currentTarget)
        type = $el.data "type"
        template = "#add-#{type}-template"
        view = new EzBob.Underwriter.simpleValueAddView({template: template, type: type})
        EzBob.App.modal2.show view
        view.on("added", @simpleValueAdded, @)
        false

    simpleValueAdded: (data)->
        (@model.get data.type).push data.model.toJSON()
        @reRenderArea(data.type)
        false

    reRenderArea: (type)->
        $el = @$el.find(".#{type}")
        (new EzBob.Underwriter.SimpleValueView({template: "##{type}-template",el: $el, model: @model.get type})).render()

class EzBob.Underwriter.FraudView extends Backbone.Marionette.ItemView
    template: "#fraud-template"

    initialize: ->
        @model = new EzBob.Underwriter.FraudModels()
        @model.on "change reset", @render, @
        @model.fetch()

    events:
        "click .add":"addButtonClicked"
        "click .all":"allClicked"
        
    allClicked: ->
        BlockUi "on"
        cid = prompt "Customer Id"
        return unless cid
        xhr = $.get "#{gRootPath}Underwriter/Fraud/RunCheck", {id: cid }
        xhr.complete (data)=>  
            @showData data.responseText
            BlockUi "off"

    showData: (data)->
        dialog = $('<div/>').html("<table class='table table-bordered'><tr><td>Check Type</td><td>Current Field</td><td>Compare Field</td><td>Value</td><td>Concurrence</td></tr>#{data}</table>" )
        dialog.dialog
            width: '75%'
            height: 600

    serializeData: ->
        data: @model.toJSON()

    addButtonClicked: ->
        model = new EzBob.Underwriter.FraudModel()
        view = new EzBob.Underwriter.AddEditFraudView({model: model})
        view.modalOptions = 
            show: true
            keyboard: false
            width: 600
            height: 600
        EzBob.App.modal.show view
        model.on "saved", => @model.fetch()
        false