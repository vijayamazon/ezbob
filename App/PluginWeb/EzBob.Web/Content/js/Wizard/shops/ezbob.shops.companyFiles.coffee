root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.CompanyFilesAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/CompanyFilesMarketPlaces/Accounts"

class EzBob.CompanyFilesAccounts extends Backbone.Collection
    model: EzBob.CompanyFilesAccountModel
    url: "#{window.gRootPath}Customer/CompanyFilesMarketPlaces/Accounts"


class EzBob.CompanyFilesAccountInfoView extends Backbone.Marionette.ItemView
    events:
        'click a.hmrcBack': 'back'
        'click a.connect-account': 'connect'
        
    initialize: (options) ->
        @accountType = 'CompanyFiles'
        @template = '#' + @accountType + 'AccountInfoTemplate'
        @isOldInternetExplorer = 'Microsoft Internet Explorer' == navigator.appName && navigator.appVersion.indexOf("MSIE 1") == -1

    ui:
        companyFilesUploadZone: "#companyFilesUploadZone"
        uploadButton : ".connect-account"

    render: ->
        super()
        that = this
        Dropzone.options.companyFilesUploadZone = init: ->
            @on "complete", (file) ->
                if @getUploadingFiles().length is 0 and @getQueuedFiles().length is 0
                    enabled = @getAcceptedFiles() != 0
                    that.ui.uploadButton.toggleClass('disabled', !enabled)
        @ui.companyFilesUploadZone.dropzone()
        @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        'Upload Company Files'

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



