root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

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

    jqoptions: ->
        modal: true
        resizable: false
        title: "Value Add"
        position: "center"
        draggable: false
        width: 530
        dialogClass: "value-add-popup"

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

    jqoptions: ->
        modal: true
        resizable: false
        title: "Fraud"
        position: "center"
        draggable: false
        dialogClass: "fraud-popup"
        width: 600

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
        EzBob.App.jqmodal.show view
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
        EzBob.App.jqmodal.show view
        model.on "saved", => @model.fetch()
        false

    show: ->
        @$el.show()

    hide: ->
        @$el.hide()
