root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.YodleeAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'Yodlee', logoText: '', shops: @model})
    update: ->
        @model.fetch()

class EzBob.YodleeAccountInfoView extends Backbone.Marionette.ItemView
    template: '#YodleeAccoutInfoTemplate'
 
    events: 
        #'click a.connect-yodlee': 'connect'
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'
        'change input[name="Bank"]': 'bankChanged'
        'change input[type="radio"]': 'radioChanged'
        'click #yodleeContinueBtn': 'continueClicked'

    initialize: ->
        that = this;
        window.YodleeAccountAdded = (result) ->
            EzBob.App.trigger('info', 'Congratulations. Yodlee account was added successfully.');
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');
        

        window.AccountAddingError = (msg) ->
            EzBob.App.trigger('error', msg)
            that.trigger('back')

    radioChanged:(el) ->
        checked = @$el.find("input[type='radio'][name!='Bank']:checked") 
        if checked.length > 0
            url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{checked.val()}&bankName=#{checked.attr('name')}"
            @$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled')
            console.log 'remove'
            return

    bankChanged: ->
        @$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked')
        @$el.find(".SubBank:not([class*='hide'])").addClass('hide')
        bank = @$el.find("input[type='radio'][name='Bank']:checked").val()
        @$el.find("." + bank + "Container").removeClass('hide')
        console.log 'add'
        $("#yodleeContinueBtn:not([class*='disabled'])").addClass('disabled')
       
    ui:
        id : '#yodleeId'
        connect: 'a.connect-dag'
        form: 'form'

    inputChanged: ->
        enabled =  EzBob.Validation.checkForm(@validator) 
        @ui.connect.toggleClass('disabled', !enabled)

    continueClicked: ->
        return false if @$el.find('#yodleeContinueBtn').hasClass('disabled')

    connect: ->
        return false if not @validator.form()
        return false if @$el.find('a.connect-yodlee').hasClass('disabled')

        acc = new EzBob.YodleeAccountModel({bankId: 1234, bankName: 'Santander'})
        xhr = acc.save()
        if not xhr
            EzBob.App.trigger 'error', 'Yodlee Account Saving Error'
            return false

        BlockUi('on')
        xhr.always =>
            BlockUi('off')

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            console.log textStatus
            EzBob.App.trigger 'error', 'Yodlee Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false
            try
                @model.add(acc)

            EzBob.App.trigger('info', "Yodlee Account Added Successfully");
            @ui.mid.val("") 
            @ui.vpnPassword.val("")
            @ui.remotePassword.val("")
            @inputChanged()
            @trigger('completed');
            @trigger 'back'

        false

    render: ->
        super()

        oFieldStatusIcons = $ 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        @validator = EzBob.validatePayPointShopForm @ui.form
        return @

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link Yodlee Account"

    YodleeAccountAdded: ->
        EzBob.App.trigger 'ct:storebase.shop.connected'


class EzBob.YodleeAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

class EzBob.YodleeAccounts extends Backbone.Collection
    model: EzBob.YodleeAccountModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

