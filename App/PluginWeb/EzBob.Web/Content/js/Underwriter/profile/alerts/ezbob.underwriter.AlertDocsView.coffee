root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.DocModel  extends Backbone.Model

class EzBob.Underwriter.Docs extends Backbone.Collection
    model: EzBob.Underwriter.DocModel
    url: ->
        "#{window.gRootPath}Underwriter/AlertDocs/List/#{@customerId}"
        

class EzBob.Underwriter.UploadDocView extends Backbone.Marionette.ItemView
    template: '#uploadAlertDocDialog'

    initialize: (options) ->
        @customerId = options.customerId

    jqoptions: ->
        modal: true
        resizable: false
        title: "Upload Doc"
        position: "center"
        draggable: false
        width: 530
        dialogClass: "upload-doc-popup"

    events: ->
        'click .button-upload' : 'upload'
        'change #uploadFile': 'uploadFilesChanged'
    
    uploadFilesChanged: ->
        fileNamesContainer = document.getElementById('fileNamesContainer');
        files = @$el.find('input[type="file"]')[0].files
        i = 0
        while i < files.length
            name = @$el.find('input[type="file"]')[0].files[i].name
            newdiv = document.createElement('div')
            newdiv.innerHTML = '<span>' + name + '</span>'
            fileNamesContainer.appendChild(newdiv)
            i++

        return false

    upload: (e) ->
        return if $(e.currentTarget).hasClass "disabled"
        $(e.currentTarget).addClass "disabled"

        f = @$el.find('input[type="file"]')[0]
        if typeof (f.files) isnt "undefined" and f.files.length is 0 or not f.value
            EzBob.ShowMessage "Please select a file!", "Warning"
            $(e.currentTarget).removeClass "disabled"
            return

        @$el.find("#fileForm").find('input[name=CustomerId]').val @customerId
            
        @$el.find("#fileForm").ajaxSubmit
            cache: false,

            success: =>
                @trigger 'upload:ok'
                @close()

            error: ->
                EzBob.ShowMessage "Upload failed, possible cause may be big file size (use smaller file) or session time out.", "Error"
                $(e.currentTarget).removeClass "disabled"

class EzBob.Underwriter.AlertDocsView extends Backbone.Marionette.ItemView
    root: window.gRootPath + "Underwriter/AlertDocs/"

    template: '#docs-template'

    customerId: 0
    
    dialogId: "null"
    
    events:
        "click #addNewDoc": "addClick"
        "click #deleteDocs": "deleteClick"

    initialize: (options) ->
        @bindTo @model, 'change reset fetch sync', @render, @

    create: (customerId) ->
        @customerId = customerId
        @dialogId = "#uploadAlertDocDialog" + customerId
        @model.customerId = customerId
        $('.messages-tab').html('Messages / Files (' + @model.length + ')')

    serializeData: ->
        docs: @model.toJSON()

    addClick: ->
        view = new EzBob.Underwriter.UploadDocView(customerId: @customerId)
        cb = -> 
            @model.fetch()
            EzBob.ShowMessage "File successfully downloaded to \"Messages\" tab", "Successful"
        view.on('upload:ok', cb, this)
        EzBob.App.jqmodal.show(view)

    deleteClick: ->
        ids = ($(e).data('id') for e in @$el.find('input[type="checkbox"]:checked'))

        if ids.length is 0
            EzBob.ShowMessage "Please select at least one document to delete.", "Warning"
            return
        
        EzBob.ShowMessage "Are you sure you want to delete selected docs?",
            ""
            ()=>
                xhr = $.ajax
                    type: "POST"
                    traditional: true
                    url: "#{@root}DeleteDocs"
                    data:
                        docIds: ids
                    dataType: "json"
                xhr.done =>
                    @model.fetch()
                    EzBob.ShowMessage  "File successfully deleted", "Successful"
            "OK", null, "Cancel"
        false