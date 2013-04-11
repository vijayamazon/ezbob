root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.ResetPasswordView extends Backbone.Marionette.ItemView
    template: "#restore-pass-template"

    initialize: ->
        @mail = undefined

    ui:
        "questionArea": "#questionArea"
        "questionField": "#questionField"
        "email": "#email"
        "form": "form"
        "getQuestion": "#getQuestion"
        "passwordRestoredArea":".passwordRestoredArea"
        "restorePasswordArea":".restorePasswordArea"
        "answer": "#Answer"

    onRender: ->
        @captcha = new EzBob.Captcha({ elementId: "captcha", tabindex: 3 });
        @captcha.render();
        @ui.email.data "changed", true
        @

    events:
        "click #getQuestion": "getQuestionClicked"
        "keyup #email": "emailKeyuped"
        "click #restore": "restoreClicked"

    restoreClicked: (e)->
        $el = $(e.currentTarget)
        return false  if $el.hasClass("disabled")
        $el.addClass "disabled"

        $.post("RestorePassword", @ui.form.serializeArray())
            .done (data) =>
                if not EzBob.isNullOrEmpty(data.errorMessage) or not EzBob.isNullOrEmpty(data.error)
                    EzBob.App.trigger "error", data.errorMessage or data.error
                    @ui.questionArea.css "display", "none"
                    return false
    
                @ui.passwordRestoredArea.css "display", ""
                @ui.restorePasswordArea.css "display", "none"
                scrollTop()

            .fail (data) ->
                EzBob.App.trigger "error", data.responceText

            .complete =>
                $el.removeClass "disabled"
                @ui.email.data "changed", false
                @emailKeyuped()

    emailKeyuped: ->
        return false if @ui.email.data("changed")
        @ui.email.data "changed", true
        @ui.questionArea.css "display", "none"
        @ui.getQuestion.css "display", ""
        @captcha.$el.closest('.control-group').insertAfter(@ui.email.closest('.control-group'))
        @captcha.reload()

    getQuestionClicked: ->
        @mail = @ui.email.val()
        EzBob.App.trigger 'clear'
        @ui.questionArea.css "display", "none"
        @ui.answer.val ""

        $.post("QuestionForEmail", @ui.form.serialize())
            .done (response)=>
                if not EzBob.isNullOrEmpty(response.errorMessage) or not EzBob.isNullOrEmpty(response.error)
                    EzBob.App.trigger 'error', response.errorMessage or response.error
                    @ui.questionArea.css "display", "none"
                    return true

                if EzBob.isNullOrEmpty(response.question)
                     EzBob.App.trigger "warning", "To recover your password security question fields must be completely filled in the account settings"
                     @ui.questionArea.css "display", "none"
                     return true

                @ui.questionField.text response.question
                @ui.questionArea.css "display", ""
                @ui.getQuestion.css "display", "none"
                @captcha.$el.closest('.control-group').insertAfter(@ui.answer.closest('.control-group'))
                @ui.email.data "changed", false

            .complete =>
                @captcha.reload()