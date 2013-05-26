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
        'click .radio-fx': 'parentBankSelected' #should be removed
        'click img': 'parentBankImageClicked'
        'change select': "subBankSelectionChanged"

    initialize: (options) ->
        that = this;

        @YodleeBanks = new EzBob.YodleeBanks()
        @YodleeBanks.fetch().done => 
            # we don't get here when customer is in sign up
            if @YodleeBanks.length > 0
                @render

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

    parentBankImageClicked:(el) ->
        img = el.target
        currentVal = img.getAttribute('class')
        baseName = '#Bank_' + currentVal.split(" ")[0]
        inp = @$el.find(baseName)
        inp.trigger('click')

    subBankSelectionChanged:(el) ->
        url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{@$el.find("option:selected").val()}&bankName=#{this.$el.find("input[type='radio'][name='Bank']:checked").attr('value')}"
        @$el.find("#yodleeContinueBtn").attr("href", url).removeClass('disabled')
        
    bankChanged: ->
        @$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked')

        currentSubBanks = @$el.find(".SubBank:not([class*='hide'])")
        currentSubBanks.addClass('hide')
        currentSubBanks.find('option').removeAttr('selected')

        # Turn unchecked off
        arr = @$el.find("input[type='radio'][name='Bank']:not(checked)").next()
        length = arr.length
        i = 0
        while i < length
          currentVal = arr[i].getAttribute('class')
          baseName = currentVal.split(" ")[0]
          arr[i].setAttribute('class', arr[i].getAttribute('class').replace(baseName + '-on', baseName + '-off'))
          i++

        # Turn checked on
        element = @$el.find("input[type='radio'][name='Bank']:checked").next()
        currentVal = element.attr('class')
        baseName = currentVal.split(" ")[0]
        element.removeClass(baseName + '-off').addClass(baseName + '-on')    
        
        @$el.find("." + this.$el.find("input[type='radio'][name='Bank']:checked").attr('value') + "Container").removeClass('hide')
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

    render: ->
        super()

        oFieldStatusIcons = $ 'IMG.field_status'
        oFieldStatusIcons.filter('.required').field_status({ required: true })
        oFieldStatusIcons.not('.required').field_status({ required: false })

        @validator = EzBob.validatePayPointShopForm @ui.form

        return @

    parentBankSelected: ->
    
        $check = @$el.prev(".BankContainer input:radio")
        unique = "." + @className.split(" ")[1] + " span"
        $(unique).attr "class", "radio"
        @$el.find("span").attr "class", "radio-checked"
        $check.attr "checked", true
        false
    
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
