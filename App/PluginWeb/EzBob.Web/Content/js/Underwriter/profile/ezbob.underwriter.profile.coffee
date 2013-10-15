root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.ProfileView extends Backbone.View
    initialize: ->
        @template = _.template($("#profile-template-main").html())

    render: ->
        @$el.html @template()
        profileInfo = @$el.find(".profile-person-info")
        loanInfo = @$el.find(".profile-loan-info")
        summaryInfo = @$el.find("#profile-summary")
        marketplaces = @$el.find("#marketplaces")
        experianInfo = @$el.find("#credit-bureau")
        paymentAccounts = @$el.find("#payment-accounts")
        loanhistorys = @$el.find("#loanhistorys")
        medalCalculations = @$el.find("#medal-calculator")
        messages = @$el.find("#messages")
        apiChecks = @$el.find("#apiChecks")
        customerRelations = @$el.find("#customerRelations")
        alertPassed = @$el.find("#alerts-passed")
        controlButtons = @$el.find "#controlButtons"
        fraudDetection = @$el.find("#fraudDetection")

        @personalInfoModel = new EzBob.Underwriter.PersonalInfoModel()
        @profileInfoView = new EzBob.Underwriter.PersonInfoView(
            el: profileInfo
            model: @personalInfoModel
        )
        @personalInfoModel.on "change", @changeDecisionButtonsState, this
        @marketPlaces = new EzBob.Underwriter.MarketPlaces()
        @marketPlaceView = new EzBob.Underwriter.MarketPlacesView(
            el: marketplaces
            model: @marketPlaces
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
        @loanInfoView = new EzBob.Underwriter.LoanInfoView(
            el: loanInfo
            model: @loanInfoModel
            personalInfo: @personalInfoModel
        )

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
        @crossCheckView = new EzBob.Underwriter.CrossCheckView(el: @$el.find("#customer-info"))

        @companyScoreModel = new EzBob.Underwriter.CompanyScoreModel()
        @companyScoreView = new EzBob.Underwriter.CompanyScoreView(
            el: that.$el.find('#company-score-list')
            model: @companyScoreModel
        )

        that.$el.find('a.company-score-tab').on('shown.bs.tab', (evt) ->
            that.companyScoreView.showAccordion()
        )

        @messagesModel = new EzBob.Underwriter.MessageModel()
        @Message = new EzBob.Underwriter.Message(
            el: messages
            model: @messagesModel
        )
        @Message.on "creditResultChanged", @changedSystemDecision, @
        
        @alertDocs = new EzBob.Underwriter.Docs()
        @alertDocsView = new EzBob.Underwriter.AlertDocsView(el: @$el.find("#alert-docs"), model: @alertDocs)
        
        @ApicCheckLogs = new EzBob.Underwriter.ApiChecksLogs()
        @ApiChecksLogView = new EzBob.Underwriter.ApiChecksLogView(
            el: apiChecks
            model: @ApicCheckLogs
        )
        @CustomerRelationsData = new EzBob.Underwriter.CustomerRelationsData()
        @CustomerRelationsView = new EzBob.Underwriter.CustomerRelationsView(
            el: customerRelations
            model: @CustomerRelationsData
        )
        @FraudDetectionLogs = new EzBob.Underwriter.FraudDetectionLogs()
        @FraudDetectionLogView = new EzBob.Underwriter.FraudDetectionLogView(
            el: fraudDetection
            model: @FraudDetectionLogs
        )
        @showed = true
        @controlButtons = new EzBob.Underwriter.ControlButtonsView(
            el: controlButtons
        )

        this

    events:
        "click #RejectBtn": "RejectBtnClick"
        "click #ApproveBtn": "ApproveBtnClick"
        "click #EscalateBtn": "EscalateBtnClick"
        "click #SuspendBtn": "SuspendBtnClick"
        "click #ReturnBtn": "ReturnBtnClick"

    checkCustomerAvailability: (model) ->
        data = model.toJSON()
        if data.success is false
            EzBob.ShowMessage data.error, "Error", (->
                Redirect "#"
            ), "OK"
            false
        else
            @$el.show() if @showed 
            $(".tabbable a[href=\"#profile-summary\"]").tab "show"

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
            EzBob.App.modal.show(approveLoanWithoutAMLDialog);
            return false        

        @CheckCustomerStatusAndCreateApproveDialog()

    CheckCustomerStatusAndCreateApproveDialog: ->
        if @personalInfoModel.get("IsWarning")        
            approveLoanForWarningStatusCustomer = new EzBob.Underwriter.ApproveLoanForWarningStatusCustomer(model: @personalInfoModel, parent: this)
            EzBob.App.modal.show(approveLoanForWarningStatusCustomer);
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
        @changeDecisionButtonsState()

    show: (id, isHistory, history) ->
        @hide()
        BlockUi "on"
        scrollTop()
        that = this

        this.companyScoreView.hideAccordion()

        @customerId = id
        fullModel = new EzBob.Underwriter.CustomerFullModel(customerId: id, history: (EzBob.parseDate(history) ? history : null))
        fullModel.fetch().done =>

            switch fullModel.get "State"
                when "NotFound" 
                    EzBob.ShowMessage res.error,"Customer id. ##{id} was not found"
                    @router.navigate("", { trigger: true, replace: true });
                    return
                when "NotSuccesfullyRegistred" 
                    @trigger "customerNotFull", id
                    return

            @personalInfoModel.set {Id: id}, {silent: true}
            @personalInfoModel.set fullModel.get("PersonalInfoModel"), silent: true
            @personalInfoModel.changeDisabled(true)
            @changeDecisionButtonsState @personalInfoModel.get("Editable")
            @personalInfoModel.trigger "sync"

            @loanInfoModel.set {Id: id}, {silent: true}
            @loanInfoModel.set fullModel.get("ApplicationInfoModel"), silent: true
            @loanInfoModel.trigger "sync"

            @marketPlaces.customerId = id
            @marketPlaces.history = history
            @marketPlaces.reset fullModel.get("Marketplaces"), silent: true
            @marketPlaces.trigger "sync"

            @loanHistory.customerId = id
            @loanHistoryView.idCustomer = id
            @loanHistory.set fullModel.get("LoansAndOffers"), silent: true
            @loanHistory.trigger "sync"

            @summaryInfoModel.set {Id: id, success: true}, {silent: true}
            @summaryInfoModel.set fullModel.get("SummaryModel"), silent: true
            @summaryInfoModel.trigger "sync"

            @checkCustomerAvailability @summaryInfoModel
            
            EzBob.UpdateBugsIcons fullModel.get("Bugs")
            
            if that.$el.find(".vsplitbar").length is 0
                $("#spl").splitter
                    minLeft: 280
                    sizeLeft: 300
                    minRight: 600
        
            @experianInfoModel.set {Id: id} , {silent: true}
            @experianInfoModel.set fullModel.get("CreditBureauModel"), silent: true
            @experianInfoModel.trigger "sync"

            @paymentAccountsModel.customerId = id
            @paymentAccountsModel.set fullModel.get("PaymentAccountModel"), silent: true
            @paymentAccountsModel.trigger "sync"

            @medalCalculationModel.set {Id: id}, {silent: true}
            @medalCalculationModel.set fullModel.get("MedalCalculations"), silent: true
            @medalCalculationModel.trigger "sync"

            @FraudDetectionLogs.customerId = id
            @FraudDetectionLogView.idCustomer = id
            @FraudDetectionLogs.reset fullModel.get("FraudDetectionLog"), silent: true
            @FraudDetectionLogs.trigger "sync"

            @ApicCheckLogs.customerId = id
            @ApiChecksLogView.idCustomer = id
            @ApicCheckLogs.reset fullModel.get("ApiCheckLogs"), silent: true
            @ApicCheckLogs.trigger "sync"

            @messagesModel.set {Id: id}, {silent: true}
            @messagesModel.set( attaches: fullModel.get("Messages"), silent: true)
            @messagesModel.trigger "sync"

            @CustomerRelationsData.customerId = id
            @CustomerRelationsView.idCustomer = id
            @CustomerRelationsData.reset fullModel.get("CustomerRelations"), silent: true
            @CustomerRelationsData.trigger "sync"

            @alertDocs.reset fullModel.get("AlertDocs"), silent: true
            @alertDocsView.create id
            @alertDocs.trigger "sync"

            @companyScoreModel.customerId = id
            @companyScoreModel.set fullModel.get("CompanyScore"), silent: true
            @companyScoreModel.trigger "sync"

            $('a[href=#marketplaces]').click() if isHistory
            BlockUi "Off"


        @crossCheckView.render customerId: id

        @controlButtons.model = new Backbone.Model(
            customerId: id
        )
        @controlButtons.render()

    hide: ->
        @$el.hide()

    changeDecisionButtonsState: (isHideAll)->
        disabled = @personalInfoModel.get("Disabled") is 1 #disabled
        creditResult = @personalInfoModel.get("CreditResult")
       
        @$el.find("#SuspendBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn").toggleClass "disabled", disabled
        @$el.find("#SuspendBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn").hide() if isHideAll

        switch creditResult
            when  "WaitingForDecision"
                @$el.find("#ReturnBtn").hide()
                @$el.find("#RejectBtn").show()
                @$el.find("#ApproveBtn").show()
                @$el.find("#SuspendBtn").show()
                @$el.find("#EscalateBtn").show() if !escalatedFlag
            when "Rejected", "Approved", "Late"
                @$el.find("#ReturnBtn").hide()
                @$el.find("#RejectBtn").hide()
                @$el.find("#ApproveBtn").hide()
                @$el.find("#SuspendBtn").hide()
                @$el.find("#EscalateBtn").hide()
            when "Escalated"
                @$el.find("#ReturnBtn").hide()
                @$el.find("#RejectBtn").show()
                @$el.find("#ApproveBtn").show()
                @$el.find("#SuspendBtn").show()
                @$el.find("#EscalateBtn").hide()
            when "ApprovedPending"
                @$el.find("#ReturnBtn").show()
                @$el.find("#RejectBtn").hide()
                @$el.find("#ApproveBtn").hide()
                @$el.find("#SuspendBtn").hide()
                @$el.find("#EscalateBtn").hide()

    updateAlerts: ->
        @alertsModel.fetch()