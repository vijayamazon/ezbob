﻿root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.ProfileView extends Backbone.View
    initialize: ->
        @template = _.template($("#profile-template-main").html())

    render: ->
        @$el.html @template()
        profileInfo = @$el.find(".profile-person-info")
        loanInfo = @$el.find(".profile-loan-info")
        summaryInfo = @$el.find("#profile-summary")
        profileTabs = @$el.find("#profile-tabs")
        marketplaces = @$el.find("#marketplaces")
        experianInfo = @$el.find("#credit-bureau")
        paymentAccounts = @$el.find("#payment-accounts")
        loanhistorys = @$el.find("#loanhistorys")
        medalCalculations = @$el.find("#medal-calculator")
        messages = @$el.find("#messages")
        apiChecks = @$el.find("#apiChecks")
        alertPassed = @$el.find("#alerts-passed")
        controlButtons = @$el.find "#controlButtoons"
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
        @marketPlaceView.on "rechecked", @mpRechecked, @marketPlaces
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
        @paymentAccountsModel = new EzBob.Underwriter.PaymentAccountsModel()
        @paymentAccountsView = new EzBob.Underwriter.PaymentAccountView(
            el: paymentAccounts
            model: @paymentAccountsModel
        )
        @paymentAccountsView.on "rechecked", @mpRechecked, @paymentAccountsModel
        unless EzBob.Config.HideAlertsTab
            @alertsModel = new EzBob.Underwriter.AlertsModel()
            @alertsView = new EzBob.Underwriter.AlertsView(
                el: @$el.find("#alerts")
                model: @alertsModel
            )
        unless EzBob.Config.HidePassedAlertsTab
            @alertsPassedModel = new EzBob.Underwriter.AlertsModel()
            @alertsPassedModel.showPassed = true
            @alertsPassedView = new EzBob.Underwriter.AlertsView(
                el: alertPassed
                model: @alertsPassedModel
            )
        @medalCalculationModel = new EzBob.Underwriter.MedalCalculationModel()
        @medalCalculationView = new EzBob.Underwriter.MedalCalculationView(
            el: medalCalculations
            model: @medalCalculationModel
        )
        @crossCheckView = new EzBob.Underwriter.CrossCheckView(el: @$el.find("#customer-info"))
        @messagesModel = new EzBob.Underwriter.MessageModel()
        @Message = new EzBob.Underwriter.Message(
            el: messages
            model: @messagesModel
        )
        @Message.on "creditResultChanged", @changedSystemDecision, @
        @alertDocsView = new EzBob.Underwriter.AlertDocsView(el: @$el.find("#alert-docs"))
        @ApicCheckLogs = new EzBob.Underwriter.ApiChecksLogs()
        @ApiChecksLogView = new EzBob.Underwriter.ApiChecksLogView(
            el: apiChecks
            model: @ApicCheckLogs
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
        @show id

    RejectBtnClick: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        functionPopupView = new EzBob.Underwriter.RejectedDialog(model: @loanInfoModel)
        functionPopupView.render()
        functionPopupView.on "changedSystemDecision", @changedSystemDecision, this
        false

    ApproveBtnClick: (e) ->
        return false  if $(e.currentTarget).hasClass("disabled")
        if @loanInfoModel.get('OfferedCreditLine') <= 0
            EzBob.ShowMessage 'Wrong Offered credit line value (' + @loanInfoModel.get('OfferedCreditLine') + '), please enter the valid value (above zero)', 'Error'
            return
        if @loanInfoModel.get("OfferExpired")
            EzBob.ShowMessage "Loan offer has expired. Set new validity date.", "Error"
            return false
        dialog = new EzBob.Underwriter.ApproveDialog(model: @loanInfoModel)
        dialog.render()
        dialog.on "changedSystemDecision", @changedSystemDecision, this
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

    show: (id) ->
        @hide()
        scrollTop()
        BlockUi "On"
        that = this
        @customerId = id
        @personalInfoModel.set
            Id: id
        ,
            silent: true

        @personalInfoModel.fetch().done ->
            that.changeDecisionButtonsState that.personalInfoModel.get("Editable")

        @loanInfoModel.set
            Id: id
        ,
            silent: true

        @loanInfoModel.fetch()
        @marketPlaces.customerId = id
        @marketPlaces.fetch()
        @loanHistory.customerId = id
        @loanHistoryView.idCustomer = id
        @loanHistory.fetch()
        @experianInfoModel.set
            Id: id
        ,
            silent: true

        @experianInfoModel.fetch()
        @summaryInfoModel.set
            Id: id
        ,
            silent: true

        @summaryInfoModel.set
            success: true
        ,
            silent: true

        @summaryInfoModel.fetch().complete ->
            that.checkCustomerAvailability that.summaryInfoModel
            BlockUi "Off"
            EzBob.GlobalUpdateBugsIcon(id)
            if that.$el.find(".vsplitbar").length is 0
                $("#spl").splitter
                    minLeft: 280
                    sizeLeft: 300
                    minRight: 600

        @paymentAccountsModel.customerId = id
        @paymentAccountsModel.fetch()
        unless EzBob.Config.HideAlertsTab
            @alertsModel.clear silent: true
            @alertsModel.set
                Id: id
            ,
                silent: true

            @alertsModel.fetch()
        unless EzBob.Config.HidePassedAlertsTab
            @alertsPassedModel.clear silent: true
            @alertsPassedModel.set
                Id: id
            ,
                silent: true

            @alertsPassedModel.fetch()
        @medalCalculationModel.set
            Id: id
        ,
            silent: true

        @medalCalculationModel.fetch()
        @crossCheckView.render customerId: id
        @messagesModel.set
            Id: id
        ,
            silent: true

        @messagesModel.fetch()
        @alertDocsView.create id
        @ApicCheckLogs.customerId = id
        @ApiChecksLogView.idCustomer = id
        @ApicCheckLogs.fetch()

        @controlButtons.model = new Backbone.Model(
            customerId: id
        )
        @controlButtons.render()

    hide: ->
        @$el.hide()

    changeDecisionButtonsState: (isHideAll)->
        disabled = !!@personalInfoModel.get("Disabled")
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