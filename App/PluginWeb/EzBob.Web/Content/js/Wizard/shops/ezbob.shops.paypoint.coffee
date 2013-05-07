root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.PayPointAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'PayPoint', logoText: '', shops: @model})
    update: ->
        @model.fetch()

class EzBob.PayPointAccountInfoView extends Backbone.Marionette.ItemView
    template: '#PayPointAccoutInfoTemplate'
 
    events: {
        'click a.connect-payPoint': 'connect',
        "click a.back": "back",
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'
        'change #payPoint_mid': 'payPointMidChanged'
        'change #payPoint_vpnPassword': 'payPointVpnPasswordChanged'
        'change #payPoint_remotePassword': 'payPointRemotePasswordChanged'
    }

    ui:
        mid: '#payPoint_mid'
        vpnPassword: '#payPoint_vpnPassword'
        remotePassword: '#payPoint_remotePassword'
        connect: 'a.connect-payPoint'
        form: 'form'

    inputChanged: ->
        enabled =  EzBob.Validation.checkForm(@validator) 
        @ui.connect.toggleClass('disabled', !enabled)

    payPointMidChanged: ->
        @$el.find('#payPoint_midImage').field_status('set', if EzBob.Validation.element(@validator, @ui.mid) then 'ok' else 'fail')

    payPointVpnPasswordChanged: ->
        @$el.find('#payPoint_vpnPasswordImage').field_status('set', if EzBob.Validation.element(@validator, @ui.vpnPassword) then 'ok' else 'fail')

    payPointRemotePasswordChanged: ->
        @$el.find('#payPoint_remotePasswordImage').field_status('set', if EzBob.Validation.element(@validator, @ui.remotePassword) then 'ok' else 'fail')

    connect: ->
        return false if not @validator.form()            
        return false if @$el.find('a.connect-payPoint').hasClass('disabled')

        acc = new EzBob.PayPointAccountModel({mid: @ui.mid.val(), vpnPassword: @ui.vpnPassword.val(), remotePassword: @ui.remotePassword.val()})
        
        xhr = acc.save()

        if not xhr
            EzBob.App.trigger 'error', 'PayPoint Account Saving Error'
            return false

        BlockUi('on')

        xhr.always =>
            BlockUi('off')

        xhr.fail (jqXHR, textStatus, errorThrown) =>
            console.log textStatus
            EzBob.App.trigger 'error', 'PayPoint Account Saving Error'

        xhr.done (res) =>
            if (res.error)
                EzBob.App.trigger 'error', res.error
                return false

            @model.add(acc)
            EzBob.App.trigger('info', "PayPoint Account Added Successfully");
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
        "Link PayPoint Account"


class EzBob.PayPointAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/PayPointMarketPlaces/Accounts"

class EzBob.PayPointAccounts extends Backbone.Collection
    model: EzBob.PayPointAccountModel
    url: "#{window.gRootPath}Customer/PayPointMarketPlaces/Accounts"
