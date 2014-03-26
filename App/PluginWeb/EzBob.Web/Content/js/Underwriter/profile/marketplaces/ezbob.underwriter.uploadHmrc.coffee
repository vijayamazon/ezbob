root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.UploadHmrcView extends Backbone.Marionette.ItemView
    template: "#hmrc-upload-template"

    initialize :(options) ->
        @customerId = options.customerId

    ui:
        hmrcUploadZone: "#hmrcUploadZone"
        uploadHmrcButton : ".uploadHmrc"

    events:
        "click .uploadHmrc": "uploadHmrcClicked"
        "click .back": "backClicked"

    serializeData: ->
        data = {
            customerId : @customerId
        }

    onRender: ->
        that = @
        Dropzone.options.hmrcUploadZone = init: ->
            @on "complete", (file) ->
                if @getUploadingFiles().length is 0 and @getQueuedFiles().length is 0
                    enabled = @getAcceptedFiles() != 0
                    that.ui.uploadHmrcButton.toggleClass('disabled', !enabled)
        @ui.hmrcUploadZone.dropzone()
        @

    uploadHmrcClicked: ->
        return false if @ui.uploadHmrcButton.hasClass('disabled')
        
        that = this
        BlockUi 'on'
        xhr = $.post(window.gRootPath + "UploadHmrc/UploadFiles", { customerId : @customerId })
        xhr.done (res) ->
            if (res.error != undefined)
                EzBob.App.trigger 'error', 'Failed to Save HMRC Account'
            else                
                EzBob.App.vent.trigger 'ct:marketplaces.history', null
        xhr.always ->
            BlockUi 'off'

    backClicked: ->
        EzBob.App.vent.trigger 'ct:marketplaces.uploadHmrcBack'
        