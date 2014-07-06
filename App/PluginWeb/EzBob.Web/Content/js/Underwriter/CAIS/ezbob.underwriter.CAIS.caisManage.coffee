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
        id: ""

class EzBob.Underwriter.CAIS.SelectedFiles extends Backbone.Collection
    model: EzBob.Underwriter.CAIS.SelectedFile

    getModelById: (id)->
       @filter (val) -> return val.get("id") == id

class EzBob.Underwriter.CAIS.CaisManageView extends Backbone.Marionette.ItemView
    template: _.template(if $("#cais-template").length>0 then $("#cais-template").html() else "")

    initialize: ->
        @model = new EzBob.Underwriter.CAIS.ListOfFilesModel()
        @bindTo @model, "change reset", @render, @
        BlockUi "on"
        @model.fetch().done => BlockUi "off"
        @checkedModel = new EzBob.Underwriter.CAIS.SelectedFiles()
        @bindTo @checkedModel, "add remove reset", @checkedFileModelChanged, @

    ui:
        count:".reports-count"
        download: ".download"

    onRender: ->
        @checkedFileModelChanged()
        
    serializeData: ->
        model: @model.get "cais"
        checkedModel: @checkedModel.toJSON()

    checkedFileModelChanged: ->
        if @checkedModel.length == 0
            @ui.download.hide()
        else
            @ui.download.show()
            @ui.count.text  @checkedModel.length

    events:
        "click .generate": "generateClicked"
        "click .download ": "downloadFile"
        "dblclick [data-id]": "fileSelected"
        "click [data-id]": "fileChecked"

    downloadFile: ->
        _.each @checkedModel.toJSON(), (val)->
            window.open "#{gRootPath}Underwriter/CAIS/DownloadFile?Id=#{val.id}", "_blank"
    
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
        id= $el.data "id"
        $el.toggleClass "checked", !checked
        if not checked then @checkedModel.add(new EzBob.Underwriter.CAIS.SelectedFile({ id: id})) else
            @checkedModel.remove(@checkedModel.getModelById(id))

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
        id = $el.data "id"
        BlockUi "on"
        ($.get "#{gRootPath}Underwriter/CAIS/GetOneFile", {id: id})
        .done (response) => 
            if response.error
                EzBob.ShowMessage response.error, "Error"
                return
            dialog = $('<div/>').html("<textarea wrap='off' class='cais-file-view'>#{response}</textarea>" )    
            dialog.dialog
                title: id
                width: '75%'
                height: 600
                modal: true
                draggable: false
                resizable: false
                buttons:[
                    { 
                        text: "Save file changes"
                        click: (e)-> self.saveFileChange(e)
                        class:'btn btn-primary save-change disabled'
                        'data-id': id 
                    }
                    {
                        html: "<i class='fa fa-refresh'></i>Set Status Uploaded"
                        click: (e)-> self.fileUploaded(e)
                        class:'btn btn-primary save-change'
                        'data-id': id 
                    }
                    {
                        text: "Close"
                        click: -> dialog.dialog('destroy')
                        class: 'btn btn-primary'
                    }
                ]
            (dialog.find ".cais-file-view").on("keypress keyup keydown", @fileViewChanged)
        .always =>BlockUi "off"

    fileUploaded: (e) ->
        self = @
        $el = $(e.currentTarget)
        id = $el.data "id"
        
        BlockUi "on"
        xhr = $.post "#{gRootPath}CAIS/UpdateStatus", { id: id }
        xhr.done (response)->
            if response.error 
                EzBob.ShowMessage response.error, "Something went wrong"
                return false
            EzBob.ShowMessage "Status Updated ", "Successful"
            self.resetFileView()
        xhr.fail ->
            EzBob.ShowMessage "Error occured", "Something went wrong"
        xhr.always ->
            BlockUi "off"

    saveFileChange: (e)->
        self = @
        $el = $(e.currentTarget)
        return if $el.hasClass "disabled"
        $caisTextarea = ($ "textarea:visible")
        caisContent = $caisTextarea.val()
        id = $el.data "id"
        saveFn = ->
            BlockUi "on"
            xhr = $.post "#{gRootPath}CAIS/SaveFileChange", { fileContent: caisContent, id: id}
            xhr.done (response)->
                if response.error 
                    EzBob.ShowMessage response.error, "Something went wrong"
                    return false
                EzBob.ShowMessage "File ##{id} successfully saved ", "Successful"
                self.resetFileView()
            xhr.fail ->
                EzBob.ShowMessage "Error occured", "Something went wrong"
            xhr.always ->
                BlockUi "off"
        EzBob.ShowMessage "Are you sure you want to save the change?", "Confirmation", saveFn, "Save", null, "Cancel"