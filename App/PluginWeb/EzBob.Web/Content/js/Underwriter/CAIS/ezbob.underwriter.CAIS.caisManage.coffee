root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}
EzBob.Underwriter.CAIS = EzBob.Underwriter.CAIS or {}

class EzBob.Underwriter.CAIS.ListOfFilesModel extends Backbone.Model
    url: "#{gRootPath}Underwriter/CAIS/ListOfFiles"
    default:
        cais: {}

    initialize: -> 
        interval = setInterval (=>
            @fetch()
        ), 2000
        @set "interval", interval

class EzBob.Underwriter.CAIS.SelectedFile extends Backbone.Model
    default:
        path: ""

class EzBob.Underwriter.CAIS.SelectedFiles extends Backbone.Collection
    model: EzBob.Underwriter.CAIS.SelectedFile

    getModelByPath: (path)->
       @filter (val) -> return val.get("path") == path

    url: "#{gRootPath}Underwriter/CAIS/SendFiles"

    sendToServer: ->
        Backbone.sync "create", @, url: @url

class EzBob.Underwriter.CAIS.CaisManageView extends Backbone.Marionette.ItemView
    template: _.template(if $("#cais-template").length>0 then $("#cais-template").html() else "")

    initialize: ->
        @model = new EzBob.Underwriter.CAIS.ListOfFilesModel()
        @bindTo @model, "change reset", @render, @
        BlockUi "on"
        @model.fetch().done => BlockUi "off"
        @checkedModel = new EzBob.Underwriter.CAIS.SelectedFiles()
        @bindTo @checkedModel, "add remove", @checkedFileModelChanged, @
   
    ui:
        count:".reports-count"
        send: ".send"

    onRender: ->
        @$el.find('[data-toggle="tooltip"]').tooltip()
        @checkedFileModelChanged()
        
    serializeData: ->
        model: @model.get "cais"
        checkedModel: @checkedModel.toJSON()

    checkedFileModelChanged: ->
        if @checkedModel.length == 0
            @ui.send.hide()
        else
            @ui.send.show()
            @ui.count.text  @checkedModel.length

    events:
        "click .generate": "generateClicked"
        "click .send ": "sendFile"
        "click [data-path]": "fileSelected"
        "click [data-file-path]": "fileChecked"

    sendFile: ->
        sendFn = =>
            BlockUi "on"
            xhr = @checkedModel.sendToServer()
            xhr.done (response)->
                if response and response.error != undefined
                    console.info "<pre>{0}<pre>".f(response.error), response.error
                    EzBob.ShowMessage "<pre>{0}<pre>".f(response.error), "Error"
                    return
                EzBob.ShowMessage "File(s) successfully sended ", "Successful"
            xhr.fail ()->
                EzBob.ShowMessage "Something went wrong", "Error occured"
            xhr.always ->
                BlockUi "off"
                
        EzBob.ShowMessage "Are you sure you want to send selected files?", "Confirmation", sendFn, "Send", null, "Cancel"
    
    fileViewChanged: (e)->
        $el = $(e.currentTarget)
        ($ ".save-change").removeClass "disabled"
        $el.css "border", "1px solid lightgreen"

    resetFileView: ->
        ($ "textarea").css "border", "1px solid #cccccc"
        ($ ".save-change").addClass "disabled"

    fileChecked: (e) ->
        return if _.keys($(e.target).data()).length > 0
        $el = $(e.currentTarget)
        checked = $el.hasClass("checked")
        filePath = $el.data "file-path"
        $el.toggleClass "checked", !checked
        if not checked then @checkedModel.add(new EzBob.Underwriter.CAIS.SelectedFile({ path: filePath })) else
            @checkedModel.remove(@checkedModel.getModelByPath(filePath))

    generateClicked: (e)->
        $el = $(e.currentTarget)
        return if $el.hasClass "disabled"
        $el.addClass "disabled"
        $.post(gRootPath + 'Underwriter/CAIS/Generate')
        .done (response)->
            if response.error != undefined
                EzBob.ShowMessage "Something went wrong", "Error occured"
                return
            EzBob.ShowMessage "Generating current CAIS reports. Please wait few minutes.", "Successful"
        .always ->
            $el.removeClass "disabled"
            
    fileSelected: (e)->
        self = @
        $el = $(e.currentTarget)
        filePath = $el.data "path"
        BlockUi "on"
        ($.get "#{gRootPath}Underwriter/CAIS/GetOneFile", {path: filePath})
        .done (response) => 
            if response.error
                EzBob.ShowMessage response.error, "Error"
                return
            dialog = $('<div/>').html("<textarea wrap='off' class='cais-file-view'>#{response}</textarea>" )    
            dialog.dialog
                title: filePath
                width: '75%'
                height: 600
                modal: true
                draggable: false
                resizable: false
                buttons:[
                    { 
                        text: "Save file changes"
                        click: (e)-> self.saveFileChange(e)
                        class:'btn btn-primary save-change disabled',
                        'data-file-path': filePath 
                    }
                    {
                        text: "Close"
                        click: -> dialog.dialog('destroy')
                        class: 'btn'
                    }
                ]
            (dialog.find ".cais-file-view").on("keypress keyup keydown", @fileViewChanged)
        .always =>BlockUi "off"

    saveFileChange: (e)->
        self = @
        $el = $(e.currentTarget)
        return if $el.hasClass "disabled"
        $caisTextarea = ($ "textarea:visible")
        caisContent = $caisTextarea.val()
        filePath = $el.data "file-path"
        saveFn = ->
            BlockUi "on"
            xhr = $.post "#{gRootPath}CAIS/SaveFileChange", { fileContent: caisContent, fullFileName: filePath }
            xhr.done (response)->
                if response.error 
                    EzBob.ShowMessage response.error, "Something went wrong"
                    return false
                EzBob.ShowMessage "File #{filePath} successfully saved ", "Successful"
                self.resetFileView()
            xhr.fail ->
                EzBob.ShowMessage "Error occured", "Something went wrong"
            xhr.always ->
                BlockUi "off"
        EzBob.ShowMessage "Are you sure you want to save the change?", "Confirmation", saveFn, "Save", null, "Cancel"