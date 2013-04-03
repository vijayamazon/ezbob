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

    checkedFileModelChanged: ->
        if @checkedModel.length == 0
            @ui.send.hide()
        else
            @ui.send.show()
            @ui.count.text  @checkedModel.length

    events:
        "click .generate": "generateClicked"
        "click [data-path]": "fileSelected"
        "click [data-file-path]": "fileChecked"
    
    fileChecked: (e) ->
        $el = $(e.currentTarget)
        checked = $el.hasClass("checked")
        filePath = $el.data "file-path"
        $el.toggleClass "checked", !checked
        $el.css "border-collapse", ""
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
        $el = $(e.currentTarget)
        filePath = $el.data "path"
        BlockUi "on"
        ($.get "#{gRootPath}Underwriter/CAIS/GetOneFile", {path: filePath})
        .done (response) -> 
            if response.error
                EzBob.ShowMessage response.error, "Error"
                return
            dialog = $('<div/>').html("<pre class='cais-file-view'>#{response}</pre>")
            dialog.dialog({
                title: filePath
                width: '75%'
                height: 600
                modal: true
                draggable: false
            })
        .always =>BlockUi "off"