root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CompanyFilesAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/CompanyFilesMarketPlaces/Accounts"

class EzBob.CompanyFilesAccounts extends Backbone.Collection
    model: EzBob.CompanyFilesAccountModel
    url: "#{window.gRootPath}Customer/CompanyFilesMarketPlaces/Accounts"


class EzBob.CompanyFilesAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'click a.back': 'back'
        'click a.connect-account': 'connect'
        
    initialize: (options) ->
        @accountType = 'CompanyFiles'
        @template = '#' + @accountType + 'AccountInfoTemplate'
        @isOldInternetExplorer = 'Microsoft Internet Explorer' == navigator.appName && navigator.appVersion.indexOf("MSIE 1") == -1
        @Dropzone = null
        

    ui:
        companyFilesUploadZone: "#companyFilesUploadZone"
        uploadButton : ".connect-account"

    render: ->
        super()
        that = this
        @initDropzone()
        @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        'Upload Company Files'

    initDropzone: ->
        @clearDropzone()
        Dropzone.options.customerFilesUploader = false
        self = this
        
        @Dropzone = new Dropzone(self.ui.companyFilesUploadZone[0],
          init: ->
            oDropzone = this
            maxFilesize: 10
            maxFiles: 10
            dictFileTooBig: "File is too big, max file size is 10MB"
            oDropzone.on "success", (oFile, oResponse) ->
              if @getUploadingFiles().length is 0 and @getQueuedFiles().length is 0
                enabled = @getAcceptedFiles() != 0
                self.ui.uploadButton.toggleClass('disabled', !enabled)
              if oResponse.success
                EzBob.App.trigger "info", "Upload successful: " + oFile.name
              else if oResponse.error
                EzBob.App.trigger "error", oResponse.error
              return
            oDropzone.on "error", (oFile, sErrorMsg, oXhr) ->
              EzBob.App.trigger "error", "Error uploading " + oFile.name
              oDropzone.removeFile(oFile);
              return
            oDropzone.on "maxfilesexceeded", (o) ->
              EzBob.App.trigger "error", "You can upload up to 10 files"
              return
        )

    clearDropzone: ->
        if (@Dropzone)
            @Dropzone.destroy()
            @Dropzone = null

    connect: ->
        return false if @ui.uploadButton.hasClass('disabled')
        
        that = this
        BlockUi 'on'
        xhr = $.post(window.gRootPath + "CompanyFilesMarketPlaces/Connect", { customerId : @customerId })
        xhr.done (res) ->
            if (res.error != undefined)
                EzBob.App.trigger 'error', 'Failed to upload company files'
            else                
                EzBob.App.trigger 'info', 'Company files uploaded successfully'
        xhr.always ->
            BlockUi 'off'
            
        @trigger('completed');
        @trigger 'back'

        false



