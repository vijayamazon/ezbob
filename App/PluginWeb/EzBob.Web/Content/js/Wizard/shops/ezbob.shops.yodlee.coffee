root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.YodleeAccountInfoView extends Backbone.Marionette.ItemView
    template: '#YodleeAccoutInfoTemplate'
 
    events: 
        'click a.back': 'back'
        'change input[name="Bank"]': 'bankChanged'
        'click .radio-fx': 'parentBankSelected'
        'change .SubBank': 'subBankSelectionChanged'
        'click #yodleeLinkAccountBtn': 'linkAccountClicked'
        'click #OtherYodleeBanks': 'OtherYodleeBanksClicked'
        'change #OtherYodleeBanks': 'OtherYodleeBanksClicked'

    initialize: (options) ->
        that = @;

        window.YodleeAccountAdded = (result) ->
            if (result.error)
                EzBob.App.trigger('error', result.error);
            else
                EzBob.App.trigger('info', 'Congratulations. Bank account was added successfully.');
            
            $.colorbox.close()
            that.trigger('completed');
            that.trigger('ready');
            that.trigger('back');

        window.YodleeAccountAddingError = (msg) ->
            $.colorbox.close()
            EzBob.App.trigger('error', msg)
            that.trigger('back')

        window.YodleeAccountRetry = () ->
            that.attemptsLeft = (that.attemptsLeft || 5) - 1
            return {url: that.$el.find('#yodleeContinueBtn').attr('href'), attemptsLeft: that.attemptsLeft}

    render: =>
        super()
        EzBob.UiAction.registerView @
        @
    # end of render

    OtherYodleeBanksClicked:(el) ->
        selectedId = $(el.currentTarget).find(':selected').val()
        selectedName = $(el.currentTarget).find(':selected').text()

        if (selectedId)
            @$el.find("input[type='radio'][name='Bank']:checked").removeAttr('checked')
            @$el.find(".SubBank:not(.hide)").addClass('hide')
            @$el.find("a.radio-fx .on").addClass('off').removeClass('on')
            url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{selectedId}&bankName=#{selectedName}"
            @$el.find("#yodleeContinueBtn").attr("href", url)
            
            @$el.find("#yodleeLinkAccountBtn").removeClass('disabled')
        else
            @$el.find("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled')
            
        #return if(el.currentTarget.find(':selected').val()

    subBankSelectionChanged:(el) ->
        return false if (@$el.find(".SubBank option:selected").length == 0) 
        url = "#{window.gRootPath}Customer/YodleeMarketPlaces/AttachYodlee?csId=#{@$el.find("option:selected").val()}&bankName=#{this.$el.find("input[type='radio'][name='Bank']:checked").attr('value')}"
        @$el.find("#yodleeContinueBtn").attr("href", url)
        @$el.find("#yodleeLinkAccountBtn").removeClass('disabled')
        
    bankChanged: ->
        @$el.find("input[type='radio'][name!='Bank']:checked").removeAttr('checked')
        currentSubBanks = @$el.find(".SubBank:not([class*='hide'])")
        currentSubBanks.addClass('hide')  
        @$el.find("#subTypeHeader[class*='hide']").removeClass('hide') 
        currentSubBanks.find('option').removeAttr('selected')
        bank = @$el.find("input[type='radio'][name='Bank']:checked").val()
        @$el.find("." + bank + "Container").removeClass('hide')
        $("#yodleeLinkAccountBtn:not([class*='disabled'])").addClass('disabled')

    linkAccountClicked: ->
        return false if @$el.find('#yodleeLinkAccountBtn').hasClass('disabled')
        @$el.find('.yodlee_help').colorbox({ inline:true, transition: 'none' });
    
    parentBankSelected: (evt)->
        evt.preventDefault()
        @$el.find('#Bank_' + evt.currentTarget.id).click()
        @$el.find('span.on').removeClass('on').addClass('off')
        $(evt.currentTarget).find('span.off').removeClass('off').addClass('on')
        @$el.find(".SubBank:not(.hide) option:selected").prop('selected',false)
        @$el.find("#OtherYodleeBanks option").eq(0).prop('selected',true)
        @$el.find("#OtherYodleeBanks").change()
        return

    serializeData: ->
        return { YodleeBanks: JSON.parse $('#yodlee-banks').text() }

    back: ->
        @trigger 'back'
        false

    getDocumentTitle: ->
        EzBob.App.trigger 'clear'
        "Link Yodlee Account"
        
class EzBob.YodleeAccountModel extends Backbone.Model
    urlRoot: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"

class EzBob.YodleeAccounts extends Backbone.Collection
    model: EzBob.YodleeAccountModel
    url: "#{window.gRootPath}Customer/YodleeMarketPlaces/Accounts"
