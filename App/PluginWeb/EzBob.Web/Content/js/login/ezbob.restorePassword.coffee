root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.ResetPasswordView extends Backbone.Marionette.ItemView
  template: "#restore-pass-template"

  initialize: ->
    @mail = undefined
    @answerEnabled = true
    @emailEnabled = false
    @captchaEnabled = EzBob.Config.CaptchaMode == 'off'
    
  focus:
    null

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
    EzBob.UiAction.registerView @
    $('.header-info-text').text('PASSWORD RECOVERY')
    @

  events:
    "click #getQuestion": "getQuestionBtnClicked"
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
        
    @focus = null

    $.post("RestorePassword", @ui.form.serializeArray())
        .done (data) =>
            if not EzBob.isNullOrEmpty(data.errorMessage) or not EzBob.isNullOrEmpty(data.error)
                EzBob.App.trigger "error", data.errorMessage or data.error
                @ui.questionArea.hide()
                @ui.email.closest('div').show()
                $('#captcha').show()
                @focus = @focusCaptcha
                @ui.answer.val('')
                @ui.getQuestionBtn.addClass('disabled')
                return false

            @ui.passwordRestoredArea.show()
            @ui.restorePasswordArea.hide()
            scrollTop()

        .fail (data) =>
            EzBob.App.trigger "error", data.responceText
            @initStatusIcons()
            @focus = @focusCaptcha

        .always (data) =>
            @ui.email.data "changed", false
            @emailKeyuped()
            @captcha.reload @focus

  focusEmail: => $('#email').focus()

  focusAnswer: => $('#Answer').focus()

  focusCaptcha: => $('#CaptchaInputText').focus()
            
  inputCaptchaChanged: ->
    @captchaEnabled = @validator.check($(@ui.captcha.selector))
    enabled = @answerEnabled && @emailEnabled && @captchaEnabled
    @ui.getQuestionBtn.toggleClass('disabled', !enabled)

  inputEmailChanged: ->
    @emailEnabled = @validator.check(@ui.email)
    enabled = @answerEnabled && @emailEnabled && @captchaEnabled
    @ui.getQuestionBtn.toggleClass('disabled', !enabled)

  inputAnswerChanged: ->
    @answerEnabled = @validator.check(@ui.answer)
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

    @focus = null
    
    $.post("QuestionForEmail", @ui.form.serialize())
        .done (response)=>
            if response.broker
                document.location.href = "#{window.gRootPath}Broker#ForgotPassword"
                return true 

            if not EzBob.isNullOrEmpty(response.errorMessage) or not EzBob.isNullOrEmpty(response.error)
                EzBob.App.trigger 'error', response.errorMessage or response.error
                @ui.questionArea.hide()
                @focus = @focusCaptcha
                @ui.getQuestionBtn.addClass('disabled')
                return true

            if EzBob.isNullOrEmpty(response.question)
                EzBob.App.trigger "warning", "To recover your password security question fields must be completely filled in the account settings"
                @ui.questionArea.hide()
                @focus = @focusEmail
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
            @focus = @focusAnswer
            
        .always =>
            @captcha.reload @focus

  initStatusIcons: (e) ->
    oFieldStatusIcons = this.$el.find('IMG.field_status')
    oFieldStatusIcons.filter('.required').field_status({ required: true })
    oFieldStatusIcons.not('.required').field_status({ required: false })
    @ui.email.change() if e is 'email' 
