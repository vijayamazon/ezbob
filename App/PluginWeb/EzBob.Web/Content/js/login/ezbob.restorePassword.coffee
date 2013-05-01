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
    "getQuestionBtn": "#getQuestion"
    "restoreBtn" : "#restore"
    "passwordRestoredArea":".passwordRestoredArea"
    "restorePasswordArea":".restorePasswordArea"
    "answer": "#Answer"

  onRender: ->
    @captcha = new EzBob.Captcha({ elementId: "captcha", tabindex: 3 });
    @captcha.render();
    @ui.email.data "changed", true
    @validator = EzBob.validateRestorePasswordForm(@ui.form)
    @initStatusIcons()
    @

  events:
    "click #getQuestion": "getQuestionBtnClicked"
    "keyup #email": "emailKeyuped"
    "click #restore": "restoreClicked"

    "keydown #email": "inputEmailChanged"
    "paste #email": "inputEmailChanged"
    "input #email": "inputEmailChanged"
    "change #email": "emailChanged"

    "keydown #Answer": "inputAnswerChanged"
    "paste #Answer": "inputAnswerChanged"
    "input #Answer": "inputAnswerChanged"
    "change #Answer": "answerChanged"

  restoreClicked: (e)->
    false if @ui.restoreBtn.hasClass("disabled")
    $el = $(e.currentTarget)
    false if $el.hasClass("disabled")
    $el.addClass "disabled"

    $.post("RestorePassword", @ui.form.serializeArray())
        .done (data) =>
            console.log 'done',data
            if not EzBob.isNullOrEmpty(data.errorMessage) or not EzBob.isNullOrEmpty(data.error)
                EzBob.App.trigger "error", data.errorMessage or data.error
                @ui.questionArea.hide()
                false
            console.log 'done2'
            @ui.passwordRestoredArea.show()
            @ui.restorePasswordArea.hide()
            scrollTop()

        .fail (data) ->
            EzBob.App.trigger "error", data.responceText
            @initStatusIcons()
            console.log 'fail', data

        .complete =>
            $el.removeClass "disabled"
            @ui.email.data "changed", false
            @emailKeyuped()
            @captcha.reload()
            console.log 'complete', data

  emailChanged: ->
    @$el.find('#emailImage').field_status('set', if EzBob.Validation.element(@validator,@ui.email) then 'ok' else 'fail')
  
  answerChanged: ->
    @$el.find('#AnswerImage').field_status('set', if EzBob.Validation.element(@validator,@ui.answer) then 'ok' else 'fail')
  
  inputEmailChanged: ->
    enabled = EzBob.Validation.element(@validator,@ui.email)
    @ui.getQuestionBtn.toggleClass('disabled', !enabled)
  
  inputAnswerChanged: ->
    enabled = EzBob.Validation.element(@validator,@ui.answer)
    @ui.restoreBtn.toggleClass('disabled', !enabled) 
    
  emailKeyuped: ->
    false if @ui.email.data("changed")
    @ui.email.data "changed", true
    @ui.questionArea.hide()
    @ui.getQuestionBtn.show()
    @captcha.$el.closest('.control-group').insertAfter(@ui.email.closest('.control-group'))
    #@captcha.reload()

  getQuestionBtnClicked: ->
    false if @ui.getQuestionBtn.hasClass("disabled")
    @mail = @ui.email.val()
    EzBob.App.trigger 'clear'
    @ui.questionArea.hide()
    @ui.answer.val("")
    
    $.post("QuestionForEmail", @ui.form.serialize())
        .done (response)=>
            if not EzBob.isNullOrEmpty(response.errorMessage) or not EzBob.isNullOrEmpty(response.error)
                EzBob.App.trigger 'error', response.errorMessage or response.error
                @ui.questionArea.hide()
                return true

            if EzBob.isNullOrEmpty(response.question)
                    EzBob.App.trigger "warning", "To recover your password security question fields must be completely filled in the account settings"
                    @ui.questionArea.hide()
                    return true

            @ui.questionField.text response.question
            @ui.questionArea.show()
            @initStatusIcons('email')
            @ui.getQuestionBtn.hide()
            @captcha.$el.closest('.control-group').insertAfter(@ui.answer.closest('.control-group'))
            #@captcha.reload()
            @ui.email.data "changed", false

        .complete =>
            @captcha.reload()
  
  initStatusIcons: (e) ->
    oFieldStatusIcons = this.$el.find('IMG.field_status')
    oFieldStatusIcons.filter('.required').field_status({ required: true })
    oFieldStatusIcons.not('.required').field_status({ required: false })
    @ui.email.change() if e is 'email' 
