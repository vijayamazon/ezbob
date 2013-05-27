root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.YodleeAccountButtonView extends EzBob.StoreButtonView
    initialize: ->
        super({name: 'Yodlee', logoText: '', shops: @model})
    update: ->
        @model.fetch().done -> EzBob.App.trigger 'ct:storebase.shop.connected'

class EzBob.YodleeAccountInfoView extends Backbone.Marionette.ItemView
    template: '#YodleeAccoutInfoTemplate'
 
    events: 
        'click a.back': 'back'
        'change input': 'inputChanged'
        'keyup input': 'inputChanged'
        'change input[name="Bank"]': 'bankChanged'
        'click #yodleeContinueBtn': 'continueClicked'
        'click .radio-fx': 'parentBankSelected'
        'click .SubBank': 'subBankSelectionChanged'

    loadBanks: () ->
        @YodleeBanks.fetch().done =>
            if @YodleeBanks.length > 0
                @render

    initialize: (options) ->
        that = this;

        @YodleeBanks = new EzBob.YodleeBanks()        

        window.YodleeAccountAdded = (result) ->
            if (result.error)
                EzBob.App.trigger('error', result.error);
            else
                EzBob.App.trigger('info', 'Congratulations. Yodlee account was added successfully.');
            
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');

        window.AccountAddingError = (msg) ->
            EzBob.App.trigger('error', msg)
            that.trigger('back')

    subBankSelectionChanged:(el) ->
        url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{@$el.find("option:selected").val()}&bankName=#{this.$el.find("input[type='radio'][name='Bank']:checked").attr('value')}"
        @$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled')
        
    bankChanged: ->
        @$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked')

        currentSubBanks = @$el.find(".SubBank:not([class*='hide'])")
        currentSubBanks.addClass('hide')
        currentSubBanks.find('option').removeAttr('selected')
        
        @$el.find("." + this.$el.find("input[type='radio'][name='Bank']:checked").attr('value') + "Container").removeClass('hide')

    radioChanged:(el) ->
        checked = @$el.find("input[type='radio'][name!='Bank']:checked") 
        if checked.length > 0
            url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{checked.val()}&bankName=#{checked.attr('name')}"
            @$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled')
            return

    bankChanged: ->
        @$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked')
        currentSubBanks = @$el.find(".SubBank:not([class*='hide'])")
        currentSubBanks.addClass('hide')        
        currentSubBanks.find('option').removeAttr('selected')
        bank = @$el.find("input[type='radio'][name='Bank']:checked").val()
        @$el.find("." + bank + "Container").removeClass('hide')
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

    parentBankSelected: (evt)->
        evt.preventDefault()
        @$el.find('#Bank_' + evt.currentTarget.id).click()
        @$el.find('span.on').removeClass('on').addClass('off')
        $(evt.currentTarget).find('span.off').removeClass('off').addClass('on')
        return

    render: ->
        super()

        oFieldStatusIcons = $ 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })
        @validator = EzBob.validatePayPointShopForm @ui.form
        return @

    serializeData: ->
        return {YodleeBanks: @YodleeBanks.toJSON()}

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        "Link Yodlee Account"

    YodleeAccountAdded: (model) -> 
        EzBob.App.trigger 'ct:storebase.shop.connected'


class EzBob.YodleeAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

class EzBob.YodleeAccounts extends Backbone.Collection
    model: EzBob.YodleeAccountModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"
    
class EzBob.YodleeBankModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Banks"

class EzBob.YodleeBanks extends Backbone.Collection
    model: EzBob.YodleeBankModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Banks"

