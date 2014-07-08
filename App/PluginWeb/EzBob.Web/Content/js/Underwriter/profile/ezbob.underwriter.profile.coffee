root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.ProfileView extends EzBob.View
    initialize: ->
        @template = _.template($("#profile-template-main").html())
        if (EzBob.CrmActions? or EzBob.CrmActions.length == 0)
            xhr = $.get(window.gRootPath + "Underwriter/CustomerRelations/CrmStatic", (data) ->
                EzBob.CrmActions = data.CrmActions
                EzBob.CrmStatuses = data.CrmStatuses
                EzBob.CrmRanks = data.CrmRanks
            )
        if(EzBob.RejectReasons? or EzBob.RejectReasons.length == 0)
            xhr = $.get(window.gRootPath + "Underwriter/Customers/RejectReasons", (data) ->
                EzBob.RejectReasons = data.reasons
            )


    render: ->
        @$el.html @template()

        profileInfo = @$el.find(".profile-person-info")
        
        summaryInfo = @$el.find("#profile-summary")
        dashboardInfo = @$el.find("#dashboard")
        marketplaces = @$el.find("#marketplaces")
        experianInfo = @$el.find("#credit-bureau")
        paymentAccounts = @$el.find("#payment-accounts")
        loanhistorys = @$el.find("#loanhistorys")
        medalCalculations = @$el.find("#medal-calc")
        messages = @$el.find("#messages")
        apiChecks = @$el.find("#apiChecks")
        customerRelations = @$el.find("#customerRelations")
        alertPassed = @$el.find("#alerts-passed")
        profileHead = @$el.find("#profileHead")
        fraudDetection = @$el.find("#fraudDetection")
        @personalInfoModel = new EzBob.Underwriter.PersonalInfoModel()
        @profileInfoView = new EzBob.Underwriter.PersonInfoView(
            el: profileInfo
            model: @personalInfoModel
        )
        
        @marketPlaces = new EzBob.Underwriter.MarketPlaces()
        @marketPlaceView = new EzBob.Underwriter.MarketPlacesView(
            el: marketplaces
            model: @marketPlaces
            personalInfoModel: @personalInfoModel
        )

        that = @
        @marketPlaceView.on "rechecked", @mpRechecked, @marketPlaces
        EzBob.App.vent.on 'ct:marketplaces.history', (history) =>
            that.show that.marketPlaces.customerId, true, history

        @loanHistory = new EzBob.Underwriter.LoanHistoryModel()
        @loanHistoryView = new EzBob.Underwriter.LoanHistoryView(
            el: loanhistorys
            model: @loanHistory
        )
        @experianInfoModel = new EzBob.Underwriter.ExperianInfoModel()
        @experianInfoView = new EzBob.Underwriter.ExperianInfoView(
            el: experianInfo
            model: @experianInfoModel
        )

        @loanInfoModel = new EzBob.Underwriter.LoanInfoModel()
        

        @summaryInfoModel = new EzBob.Underwriter.SummaryInfoModel()
        @summaryInfoView = new EzBob.Underwriter.SummaryInfoView(
            el: summaryInfo
            model: @summaryInfoModel
        )
        
        EzBob.App.vent.on 'newCreditLine:done', => @summaryInfoModel.fetch()
        EzBob.App.vent.on 'newCreditLine:pass', => @summaryInfoModel.fetch()

        @paymentAccountsModel = new EzBob.Underwriter.PaymentAccountsModel()
        @paymentAccountsView = new EzBob.Underwriter.PaymentAccountView(
            el: paymentAccounts
            model: @paymentAccountsModel
        )
        @paymentAccountsView.on "rechecked", @mpRechecked, @paymentAccountsModel
        @medalCalculationModel = new EzBob.Underwriter.MedalCalculationModel()
        @medalCalculationView = new EzBob.Underwriter.MedalCalculationView(
            el: medalCalculations
            model: @medalCalculationModel
        )

        @pricingModelScenarios = new EzBob.Underwriter.PricingModelScenarios()
        @pricingModelCalculationsModel = new EzBob.Underwriter.PricingModelCalculationsModel()
        @pricingModelScenarios.fetch()
        @pricingModelCalculationsView = new EzBob.Underwriter.PricingModelCalculationsView(
            el: @$el.find("#pricing-calc")
            model: @pricingModelCalculationsModel
            scenarios: @pricingModelScenarios
        )

        @companyScoreModel = new EzBob.Underwriter.CompanyScoreModel()
        @companyScoreView = new EzBob.Underwriter.CompanyScoreView(
            el: that.$el.find('#company-score-list')
            model: @companyScoreModel
        )

        that.$el.find('a.company-score-tab').on('shown.bs.tab', (evt) ->
            that.companyScoreView.redisplayAccordion()
        )

        @crossCheckView = new EzBob.Underwriter.CrossCheckView(
            el: @$el.find("#customer-info")
        )

        @messagesModel = new EzBob.Underwriter.MessageModel()
        @Message = new EzBob.Underwriter.Message(
            el: messages
            model: @messagesModel
        )
        @Message.on "creditResultChanged", @changedSystemDecision, @

        @signatureMonitorView = new EzBob.Underwriter.SignatureMonitorView
            el: @$el.find "#signature-monitor"
            personalInfoModel: @personalInfoModel

        @alertDocs = new EzBob.Underwriter.Docs()
        @alertDocsView = new EzBob.Underwriter.AlertDocsView(el: @$el.find("#alert-docs"), model: @alertDocs)
        
        @ApicCheckLogs = new EzBob.Underwriter.ApiChecksLogs()
        @ApiChecksLogView = new EzBob.Underwriter.ApiChecksLogView(
            el: apiChecks
            model: @ApicCheckLogs
        )
        @crmModel = new EzBob.Underwriter.CustomerRelationsModel()
        @CustomerRelationsView = new EzBob.Underwriter.CustomerRelationsView(
            el: customerRelations
            model: @crmModel
        )
        @FraudDetectionLogs = new EzBob.Underwriter.fraudDetectionLogModel()
        @FraudDetectionLogView = new EzBob.Underwriter.FraudDetectionLogView(
            el: fraudDetection
            model: @FraudDetectionLogs
        )

        @PropertiesModel = new EzBob.Underwriter.Properties()
        @affordability = new EzBob.Underwriter.Affordability()
        @fundingModel = new EzBob.Underwriter.FundingModel()
        @dashboardInfoView = new EzBob.Underwriter.DashboardView(
            el: dashboardInfo
            model: @summaryInfoModel
            crmModel: @crmModel
            personalModel: @personalInfoModel
            experianModel: @experianInfoModel
            companyModel: @companyScoreModel
            propertiesModel: @PropertiesModel
            mpsModel: @marketPlaces
            affordability: @affordability
            loanModel: @loanInfoModel
        )
        
        @profileHeadView = new EzBob.Underwriter.ProfileHeadView(
            el: profileHead
            model: @summaryInfoModel
            personalModel: @personalInfoModel
            loanModel: @loanInfoModel
            medalModel: @medalCalculationModel
            parentView: @
        )

        @showed = true
        
        @$el.find('.nav-list a[data-toggle="tab"]').on('shown.bs.tab', ((e) =>
            @setLastShownProfileSection $(e.target).attr('href').substr(1)

            if($(e.currentTarget).attr("href") is "#dashboard")
                @dashboardInfoView.render()
        ))

        this
    # end of render

    setState: (nCustomerID, sSection) ->
        @lastShownCustomerID = nCustomerID

        unless sSection
            @getLastShownProfileSection @$el.find('a.customer-tab:first').attr('href').substr(1)
    # end of setState

    restoreState: ->
        @$el.find('a.customer-tab').filter('[href="#' +
            @getLastShownProfileSection(@$el.find('a.customer-tab:first').attr('href').substr(1)) +
        '"]').tab('show')
        EzBob.handleUserLayoutSetting()
    # end of restoreState

    setLastShownProfileSection: (sSection) ->
        localStorage['underwriter.profile.lastShownProfileSection'] = sSection
    # end of setLastShownProfileSection

    getLastShownProfileSection: (sDefault) ->
        sSection = localStorage['underwriter.profile.lastShownProfileSection']

        unless sSection
            sSection = sDefault
            @setLastShownProfileSection sSection

        sSection
    # end of getLastShownProfileSection

    events:
        "click #RejectBtn": "RejectBtnClick"
        "click #ApproveBtn": "ApproveBtnClick"
        "click #EscalateBtn": "EscalateBtnClick"
        "click #SuspendBtn": "SuspendBtnClick"
        "click #ReturnBtn": "ReturnBtnClick"
        'click .add-director': 'addDirectorClicked'

    addDirectorClicked: (event) ->
        event.stopPropagation()
        event.preventDefault()

        @crossCheckView.$el.find('.add-director').hide()

        director = new EzBob.DirectorModel()

        directorEl = @crossCheckView.$el.find '.add-director-container'

        customerInfo =
            FirstName: @personalInfoModel.get 'FirstName'
            Surname: @personalInfoModel.get 'Surname'
            DateOfBirth: @personalInfoModel.get 'DateOfBirth'
            Gender: @personalInfoModel.get 'Gender'
            PostCode: @personalInfoModel.get 'PostCode'
            Directors: @personalInfoModel.get 'Directors'

        addDirectorView = new EzBob.AddDirectorInfoView
            model: director
            el: directorEl
            backButtonCaption: 'Cancel'
            failOnDuplicate: false
            customerInfo: customerInfo

        addDirectorView.setBackHandler ( => @onDirectorAddCanceled() )
        addDirectorView.setSuccessHandler ( => @onDirectorAdded() )
        addDirectorView.setDupCheckCompleteHandler ( (bDupFound) => @onDuplicateCheckComplete(bDupFound) )
        addDirectorView.render()
        addDirectorView.setCustomerID @customerId

        directorEl.show()

        false
    # end of addDirectorClicked

    onDuplicateCheckComplete: (bDupFound) ->
        if bDupFound
            @crossCheckView.$el.find('.duplicate-director-detected').show()
        else
            @crossCheckView.$el.find('.duplicate-director-detected').hide()
    # end of onDuplicateCheckComplete

    onDirectorAddCanceled: ->
        @crossCheckView.$el.find('.add-director-container').hide().empty()
        @crossCheckView.$el.find('.add-director').show()
    # end of onDirectorAddCanceled

    onDirectorAdded: ->
        @onDirectorAddCanceled()
        @show @customerId
    # end of onDirectorAdded

    recordRecentCustomers: (id) ->
        xhr = $.post "#{gRootPath}Underwriter/Customers/SetRecentCustomer", { id: id }
        xhr.done (recentCustomersModel)->
            localStorage.setItem('RecentCustomers', JSON.stringify(recentCustomersModel.RecentCustomers))

    checkCustomerAvailability: (model) ->
        data = model.toJSON()

        if data.success is false
            EzBob.ShowMessage data.error, "Error", (->
                Redirect "#"
            ), "OK"
            false
        else
            @$el.show() if @showed 
            @restoreState()
    # end of checkCustomerAvailability

    mpRechecked: (parameter) ->
        model = this
        umi = parameter.umi
        model.fetch().done ->
            el = $("#" + parameter.el.attr("id"))
            el.addClass "disabled"
            interval = setInterval(->
                req = $.get(window.gRootPath + "Underwriter/MarketPlaces/CheckForUpdatedStatus",
                    mpId: umi
                )
                req.done (response) ->
                    unless response.status is "In progress"
                        clearInterval interval
                        model.fetch().done ->
                            el.removeClass "disabled"
            , 1000)

    disableChange: (id) ->
        @show id, false

    RejectBtnClick: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        functionPopupView = new EzBob.Underwriter.RejectedDialog(model: @loanInfoModel)
        functionPopupView.render()
        functionPopupView.on "changedSystemDecision", @changedSystemDecision, this
        false

    ApproveBtnClick: (e) ->
        return false if $(e.currentTarget).hasClass("disabled")
        $('.editOfferDiv').hide()
        if @loanInfoModel.get('InterestRate') <= 0
            EzBob.ShowMessage 'Wrong Interest Rate value (' + @loanInfoModel.get('InterestRate') + '), please enter the valid value (above zero)', 'Error'
            return false
        if @loanInfoModel.get('OfferedCreditLine') <= 0
            EzBob.ShowMessage 'Wrong Offered credit line value (' + @loanInfoModel.get('OfferedCreditLine') + '), please enter the valid value (above zero)', 'Error'
            return false
        if @loanInfoModel.get("OfferExpired")
            EzBob.ShowMessage "Loan offer has expired. Set new validity date.", "Error"
            return false

        @skipPopupForApprovalWithoutAML = @loanInfoModel.get("SkipPopupForApprovalWithoutAML")
        if @loanInfoModel.get("AMLResult") != 'Passed' && !@skipPopupForApprovalWithoutAML        
            approveLoanWithoutAMLDialog = new EzBob.Underwriter.ApproveLoanWithoutAML(model: @loanInfoModel, parent: this, skipPopupForApprovalWithoutAML: @skipPopupForApprovalWithoutAML)
            EzBob.App.jqmodal.show(approveLoanWithoutAMLDialog);
            return false

        @CheckCustomerStatusAndCreateApproveDialog()

    CheckCustomerStatusAndCreateApproveDialog: ->
        if @personalInfoModel.get("IsWarning")        
            approveLoanForWarningStatusCustomer = new EzBob.Underwriter.ApproveLoanForWarningStatusCustomer(model: @personalInfoModel, parent: this)
            EzBob.App.jqmodal.show(approveLoanForWarningStatusCustomer);
            return false
        
        @CreateApproveDialog()

    CreateApproveDialog: ->
        dialog = new EzBob.Underwriter.ApproveDialog(model: @loanInfoModel)
        dialog.on "changedSystemDecision", @changedSystemDecision, this
        dialog.render()
        false

    EscalateBtnClick: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        functionPopupView = new EzBob.Underwriter.Escalated(model: @loanInfoModel)
        functionPopupView.render()
        functionPopupView.on "changedSystemDecision", @changedSystemDecision, this
        false

    SuspendBtnClick: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        functionPopupView = new EzBob.Underwriter.Suspended(model: @loanInfoModel)
        functionPopupView.render()
        functionPopupView.on "changedSystemDecision", @changedSystemDecision, this
        false
        
    ReturnBtnClick: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        functionPopupView = new EzBob.Underwriter.Returned(model: @loanInfoModel)
        functionPopupView.render()
        functionPopupView.on "changedSystemDecision", @changedSystemDecision, this
        false

    changedSystemDecision: ->
        @summaryInfoModel.fetch()
        @personalInfoModel.fetch()
        @loanInfoModel.fetch() 
        @loanHistory.fetch()

    show: (id, isHistory, history) ->
        @hide()
        BlockUi "on"
        scrollTop()
        that = this

        @customerId = id
        fullModel = new EzBob.Underwriter.CustomerFullModel(customerId: id, history: (EzBob.parseDate(history) ? history : null))
        fullModel.fetch().done =>
            switch fullModel.get "State"
                when "NotFound" 
                    EzBob.ShowMessage res.error,"Customer id. ##{id} was not found"
                    @router.navigate("", { trigger: true, replace: true });
                    return
            
            @personalInfoModel.set {Id: id}, {silent: true}
            @personalInfoModel.set fullModel.get("PersonalInfoModel"), silent: true
            
            @personalInfoModel.trigger "sync"

            @loanInfoModel.set {Id: id}, {silent: true}
            @loanInfoModel.set fullModel.get("ApplicationInfoModel"), silent: true
            @loanInfoModel.trigger "sync"

            @marketPlaces.customerId = id
            @marketPlaces.history = history
            @marketPlaces.reset fullModel.get("MarketPlaces"), silent: true
            @marketPlaces.trigger "sync"
            
            @affordability.customerId = id
            @affordability.clear().set(fullModel.get("Affordability"), silent: true)
            @affordability.trigger "sync"

            @loanHistory.customerId = id
            @loanHistoryView.idCustomer = id
            @loanHistory.set fullModel.get("LoansAndOffers"), silent: true
            @loanHistory.trigger "sync"

            @summaryInfoModel.set {Id: id, success: true}, {silent: true}
            @summaryInfoModel.set fullModel.get("SummaryModel"), silent: true
            @summaryInfoModel.trigger "sync"

            @checkCustomerAvailability @summaryInfoModel
            @recordRecentCustomers(id)
            
            @experianInfoModel.set {Id: id} , {silent: true}
            @experianInfoModel.set fullModel.get("CreditBureauModel"), silent: true
            @experianInfoModel.trigger "sync"

            @paymentAccountsModel.customerId = id
            @paymentAccountsModel.set fullModel.get("PaymentAccountModel"), silent: true
            @paymentAccountsModel.trigger "sync"

            @medalCalculationModel.set {Id: id}, {silent: true}
            @medalCalculationModel.set fullModel.get("MedalCalculations"), silent: true
            @medalCalculationModel.trigger "sync"
            
            @pricingModelCalculationsModel.set {Id: id}, {silent: true}
            @pricingModelCalculationsModel.set fullModel.get("PricingModelCalculations"), silent: true
            @pricingModelCalculationsModel.trigger "sync"

            @PropertiesModel.set {Id: id}, {silent: true}
            @PropertiesModel.set fullModel.get("Properties"), silent: true
            @PropertiesModel.trigger "sync"

            @FraudDetectionLogs.customerId = id
            @FraudDetectionLogView.customerId = id
            @FraudDetectionLogs.set fullModel.get("FraudDetectionLog"), silent: true
            @FraudDetectionLogs.trigger "sync"

            @ApicCheckLogs.customerId = id
            @ApiChecksLogView.idCustomer = id
            @ApicCheckLogs.reset fullModel.get("ApiCheckLogs"), silent: true
            @ApicCheckLogs.trigger "sync"

            @messagesModel.set {Id: id}, {silent: true}
            @messagesModel.set( attaches: fullModel.get("Messages"), silent: true)
            @messagesModel.trigger "sync"

            @crmModel.customerId = id
            @crmModel.set fullModel.get("CustomerRelations"), silent: true
            @crmModel.trigger "sync"

            @alertDocs.reset fullModel.get("AlertDocs"), silent: true
            @alertDocsView.create id
            @alertDocs.trigger "sync"

            @companyScoreModel.customerId = id
            @companyScoreModel.set fullModel.get("CompanyScore"), silent: true
            @companyScoreModel.trigger "sync"

            #@crossCheckView.segmentType = fullModel.get('PersonalInfoModel').SegmentType
            @crossCheckView.marketPlaces = @marketPlaces
            @crossCheckView.companyScore = @companyScoreModel
            @crossCheckView.experianDirectors = fullModel.get("ExperianDirectors")
            @crossCheckView.fullModel = fullModel
            @crossCheckView.render customerId: id

            $('a[href=#marketplaces]').click() if isHistory
            $('a.common-bug').attr('data-bug-customer', id)

            @signatureMonitorView.reload id

            @fundingModel.fetch().done(() =>
                that.fillFunds()
                setInterval (->
                    that.fundingModel.fetch()
                    that.fillFunds()
                ), that.fundingModel.get('RefreshInterval')
            )

            EzBob.InitBugs()
            EzBob.UpdateBugsIcons fullModel.get("Bugs")
            
            BlockUi "Off"

        EzBob.handleUserLayoutSetting()

    fillFunds: ->
        fundingAlert = @$el.find(".fundingAlert")
        availableFundsNum = @fundingModel.get('AvailableFunds')
        reqFunds = @fundingModel.get('RequiredFunds')
        availableFundsStr = 'Funding ' + EzBob.formatPoundsNoDecimals(availableFundsNum).replace(/\s+/g, '')
        fundingAlert.html(availableFundsStr)
        if (reqFunds > availableFundsNum)
            fundingAlert.addClass('red_cell')
        else
            fundingAlert.removeClass('red_cell')

    hide: ->
        @$el.hide()

    updateAlerts: ->
        @alertsModel.fetch()

    clearDecisionNotes: ->
        @$el.find('#DecisionNotes').empty()

    appendDecisionNote: (oNote) ->
        @$el.find('#DecisionNotes').append oNote
