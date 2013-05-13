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
        'click a.connect-payPoint': 'connect'
        "click a.back": "back"
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'

    ui:
        id : '#yodleeId'
        connect: 'a.connect-payPoint'
        form: 'form'

    inputChanged: ->
        enabled =  EzBob.Validation.checkForm(@validator) 
        @ui.connect.toggleClass('disabled', !enabled)

    connect: ->
        return false if not @validator.form()
        return false if @$el.find('a.connect-payPoint').hasClass('disabled')

        acc = new EzBob.YodleeAccountModel({mid: @ui.mid.val(), vpnPassword: @ui.vpnPassword.val(), remotePassword: @ui.remotePassword.val()})
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


class EzBob.YodleeAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

class EzBob.YodleeAccounts extends Backbone.Collection
    model: EzBob.YodleeAccountModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

