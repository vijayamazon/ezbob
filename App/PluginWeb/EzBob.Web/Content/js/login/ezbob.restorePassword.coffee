root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.ResetPasswordView extends Backbone.Marionette.ItemView
  template: "#restore-pass-template"

  initialize: ->
    @mail = undefined
    @answerEnabled = true
    @emailEnabled = false
    @captchaEnabled = false
    
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
    "captcha": "#CaptchaInputText"

  onRender: ->
    @captcha = new EzBob.Captcha({ elementId: "captcha", tabindex: 11 });
    @captcha.render();
    @ui.email.data "changed", true
    @validator = EzBob.validateRestorePasswordForm(@ui.form)
    @initStatusIcons()
    $('#email').focus()
    @

  events:
    "click #getQuestion": "getQuestionBtnClicked"
    "keyup #email": "emailKeyuped"
    "click #restore": "restoreClicked"
    "keyup #email": "inputEmailChanged"
    "change #email": "inputEmailChanged"
    "keyup #Answer": "inputAnswerChanged"
    "change #Answer": "inputAnswerChanged"
    "keyup #CaptchaInputText": "inputCaptchaChanged"
    "change #CaptchaInputText": "inputCaptchaChanged"

  restoreClicked: (e) ->
    return false if @ui.restoreBtn.hasClass("disabled")
    $el = $(e.currentTarget)
    return false if $el.hasClass("disabled")
    $el.addClass "disabled"
        
    $.post("RestorePassword", @ui.form.serializeArray())
        .done (data) =>
            if not EzBob.isNullOrEmpty(data.errorMessage) or not EzBob.isNullOrEmpty(data.error)
                EzBob.App.trigger "error", data.errorMessage or data.error
                @ui.questionArea.hide()
                return false

            @ui.passwordRestoredArea.show()
            @ui.restorePasswordArea.hide()
            scrollTop()

        .fail (data) =>
            EzBob.App.trigger "error", data.responceText
            @initStatusIcons()

        .always (data) =>
            @ui.email.closest('div').hide()
            $el.removeClass "disabled"
            @ui.email.data "changed", false
            @emailKeyuped()
            
  inputCaptchaChanged: ->
    @captchaEnabled = EzBob.Validation.element(@validator, $(@ui.captcha.selector))
    enabled = @answerEnabled && @emailEnabled && @captchaEnabled
    @ui.getQuestionBtn.toggleClass('disabled', !enabled)

  inputEmailChanged: ->
    @emailEnabled = EzBob.Validation.element(@validator,@ui.email)
    enabled = @answerEnabled && @emailEnabled && @captchaEnabled
    @ui.getQuestionBtn.toggleClass('disabled', !enabled)

  inputAnswerChanged: ->
    @answerEnabled = EzBob.Validation.element(@validator,@ui.answer)
    enabled = @answerEnabled && @emailEnabled && @captchaEnabled
    @ui.restoreBtn.toggleClass('disabled', !enabled) 

  emailKeyuped: ->
    return false if @ui.email.data("changed")
    @ui.email.data "changed", true
    @ui.questionArea.hide()
    @ui.getQuestionBtn.show()
    @captcha.$el.closest('.control-group').insertAfter(@ui.email.closest('.control-group'))

  getQuestionBtnClicked: ->
    return false if @ui.getQuestionBtn.hasClass("disabled")
    @mail = @ui.email.val()
    EzBob.App.trigger 'clear'
    @ui.questionArea.hide()
    
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
            @ui.email.data "changed", false
            @answerEnabled=false
            @ui.email.closest('div').hide()
            $('#captcha').hide()
            $('#Answer').focus()

  initStatusIcons: (e) ->
    oFieldStatusIcons = this.$el.find('IMG.field_status')
    oFieldStatusIcons.filter('.required').field_status({ required: true })
    oFieldStatusIcons.not('.required').field_status({ required: false })
    @ui.email.change() if e is 'email' 
