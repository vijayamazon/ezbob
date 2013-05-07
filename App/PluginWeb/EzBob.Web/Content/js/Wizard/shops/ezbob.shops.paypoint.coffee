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
    }

    ui:
        mid       : '#payPoint_mid'
        vpnPassword    : '#payPoint_vpnPassword'
        remotePassword    : '#payPoint_remotePassword'
        connect     : 'a.connect-payPoint'
        form        : 'form'

    inputChanged: ->
        @$el.find('#payPoint_midImage').field_status('set', if @ui.mid.val() then 'ok' else 'fail')
        @$el.find('#payPoint_vpnPasswordImage').field_status('set', if @ui.vpnPassword.val() then 'ok' else 'fail')
        @$el.find('#payPoint_remotePasswordImage').field_status('set', if @ui.remotePassword.val() then 'ok' else 'fail')

        enabled = @ui.mid.val() and @ui.vpnPassword.val() and @ui.remotePassword.val()
        @ui.connect.toggleClass('disabled', !enabled)

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
