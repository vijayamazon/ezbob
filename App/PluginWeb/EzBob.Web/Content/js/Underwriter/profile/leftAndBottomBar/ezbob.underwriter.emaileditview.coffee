root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.EmailEditView extends Backbone.Marionette.ItemView
    template: "#email-edit-template"

    events:
        'click .email-confirm-manually' : 'confirmManually'
        'click .email-send-new-request' : 'sendNewRequest'
        'click .email-change-address' : 'changeEmail'
        'keypress input[name="edit-email"]' : 'editEmailEnterPressed'

    ui: {'email' : 'input[name="edit-email"]'}

    jqoptions: ->
        modal: true
        resizable: false
        title: "Edit Email"
        position: "center"
        draggable: false
        dialogClass: "edit-email-popup"

    confirmManually: ->
        xhr = $.post window.gRootPath + "Underwriter/EmailVerification/ManuallyConfirm", id: @model.id
        xhr.success =>
            @model.fetch()
            @close()
        false
    
    sendNewRequest: ->
        xhr = $.post window.gRootPath + "Underwriter/EmailVerification/Resend", id: @model.id
        xhr.success =>
            @model.fetch()
            @close()
        false

    changeEmail: ->
        return false if not @validator.form()
        xhr = $.post window.gRootPath + "Underwriter/EmailVerification/ChangeEmail", {id: @model.id, email: @ui.email.val()}
        xhr.success (response) =>
            EzBob.ShowMessage(response.error) if response.error != undefined
            @model.fetch()
            @close()
        false

    onRender: ->
        @form = @.$el.find('#email-edit-form')
        @validator = EzBob.validateChangeEmailForm(@form);
        @

    editEmailEnterPressed: (e)->
        return false if e.keyCode == 13;