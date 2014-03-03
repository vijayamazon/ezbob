root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

class EzBob.Underwriter.UploadHmrcView extends Backbone.Marionette.ItemView
    template: "#hmrc-upload-template"

    initialize :(options) ->

    events:
        "click .uploadHmrc": "uploadHmrcClicked"
        "click .back": "backClicked"


    uploadHmrcClicked: ->
        
    backClicked: ->
        EzBob.App.vent.trigger 'ct:marketplaces.uploadHmrcBack'
        