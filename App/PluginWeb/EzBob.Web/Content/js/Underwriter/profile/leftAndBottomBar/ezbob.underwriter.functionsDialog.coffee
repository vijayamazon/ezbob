root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Underwriter = EzBob.Underwriter or {}

EzBob.Underwriter.FunctionsDialogView = Backbone.View.extend(
    initialize: ->
        @template = _.template(@getTemplate())
        @type = @getType()

    getTemplate: ->
        $("#functionsDialogTemplate").html()

    getType: ->
        null

    render: (id) ->
        @$el.html @template(@model)
        buttonName = @getButtonName()
        @$el.find(".button-ok").val buttonName
        @ReasonField = @$el.find(".reason")
        @YodleeReasonField = @$el.find(".yodleeReason")
        @RejectionReason = @$el.find(".rejectReason")
        unless @showReasonField()
            @ReasonField.css "display", "none"
            @$el.find("h3").css "display", "none"
        @ReasonField.val @model.get("Reason")
        
        @$el.dialog
            autoOpen: true
            position: ["top", 60]
            draggable: true
            title: "Are you sure?"
            modal: true
            resizable: true
            width: @dlgWidth or 550
            height: @dlgHeight or 300
            dialogClass: "functionsPopup"
            open: _.bind(@onShow, this)

        this

    getButtonName: ->
        "Ok"

    showReasonField: ->
        true

    onShow: ->

    events:
        "click .button-ok": "BtnOkClicked"
        "click .button-cancel": "BtnCancelClicked"
        "keydown textarea.reason": "TextAreaChanged"
        "keydown textarea.yodleeReason": "TextAreaChanged"

    ReasonFieldEmptyError: (field, isShow) ->
        if isShow
            field.css "border", "1px solid red"
        else
            field.css "border", ""

    TextAreaChanged: (field) ->
        $(".button-ok").removeClass "disabled" if @getType() isnt "Approved" or EzBob.isNullOrEmpty(@model.get("OfferedCreditLine")) or @model.get("OfferedCreditLine") isnt 0
        @ReasonFieldEmptyError($(field.currentTarget), false)

    BtnOkClicked: (e) ->
        that = this
        return false if $(e.currentTarget).hasClass("disabled")
        

        rejectionReasons = @RejectionReason.val()
        req = false
        if(@type != 'Rejected')
            if @ReasonField.val() is "" and @showReasonField()
                @ReasonFieldEmptyError(@ReasonField, true)
                req = true

            if @YodleeReasonField.val() is "" and @NoYodlee
                @ReasonFieldEmptyError(@YodleeReasonField, true)
                req = true
        if(@type == 'Rejected' and not rejectionReasons)
            @ReasonFieldEmptyError(@$el.find(".rejectReasonDiv"), true)
            req=true
        return false if req
        $(e.currentTarget).addClass "disabled"
        
        data =
            id: @model.get("CustomerId")
            status: @type

        data.reason = @ReasonField.val() if @showReasonField()
        
        if(rejectionReasons && rejectionReasons.length > 0)
            rejectionReasons = rejectionReasons.map(Number)

        data.rejectionReasons = rejectionReasons
        if @NoYodlee
            data.reason += " | " + @YodleeReasonField.val()
        
        BlockUi "on"

        req = $.ajax({
            type: "POST"
            url: window.gRootPath + "Underwriter/Customers/ChangeStatus"
            data: data
            dataType: "json"
            traditional: true
        })
        
        req.done (res) ->
            if res.error
                EzBob.ShowMessage res.error,"Error occured"
                that.$el.css "border", "1px solid red"
                return

            if res.warning
                EzBob.ShowMessage res.warning, "Warning signaled"

            that.$el.dialog "close"
            #refresh grid after decision
            that.trigger "changedSystemDecision"
            $(".ui-icon-refresh").click()

        req.always (res) ->
            BlockUi "off"
            $(e.currentTarget).removeClass "disabled"

    BtnCancelClicked: ->
        @$el.dialog "close"
)
EzBob.Underwriter.RejectedDialog = EzBob.Underwriter.FunctionsDialogView.extend(
    onShow: ->
        @$el.find(".rejectReasonDiv").show()
        @$el.find(".chosen").chosen()
        
    dlgHeight: 400
    getType: ->
        "Rejected"

    getButtonName: ->
        "Reject"
)
EzBob.Underwriter.Escalated = EzBob.Underwriter.FunctionsDialogView.extend(
    getType: ->
        "Escalated"

    getButtonName: ->
        "Escalate"
)
EzBob.Underwriter.Suspended = EzBob.Underwriter.FunctionsDialogView.extend(
    getType: ->
        "ApprovedPending"

    getButtonName: ->
        "Suspend"

    showReasonField: ->
        false
    dlgHeight: 120
    dlgWidth: 300
)
EzBob.Underwriter.Returned = EzBob.Underwriter.FunctionsDialogView.extend(
    getType: ->
        "WaitingForDecision"

    getButtonName: ->
        "Return"

    showReasonField: ->
        false
    dlgHeight: 120
    dlgWidth: 300
)
EzBob.Underwriter.ApproveDialog = EzBob.Underwriter.FunctionsDialogView.extend(
    events: ->
        _.extend {}, EzBob.Underwriter.FunctionsDialogView::events,
            "click .change-offer-details": "changeLoanDetails"
            "click .pdf-link": "exportToPdf"
            "click .excel-link": "exportToExcel"
            "click .print-link":"exportToPrint"

    getType: ->
        "Approved"

    showReasonField: ->
        true

    onShow: ->
        @renderDetails()
        @renderSchedule()
        @model.on "change", @renderDetails, this
        @$el.find(".button-ok").addClass "disabled"    if not @model.get("OfferedCreditLine") or @model.get("OfferedCreditLine") is 0
        @$el.find(".button-ok").addClass "disabled"    if @model.get("OfferExpired")
        @$el.find(".change-offer-details").attr 'disabled', 'disabled' if @model.get("IsLoanTypeSelectionAllowed") in [ 1, '1' ]
        @NoYodlee = @model.get('IsOffline') and not @model.get('HasYodlee')
        @$el.find("#noYodleeReasonDiv").toggleClass('hide', !@NoYodlee).toggleClass('uwReason', @NoYodlee)

    renderDetails: ->
        details = _.template($("#approve-details").html(), @model.toJSON())
        @$el.find("#details").html details
        @$el.find(".offer-status").append("<strong>Offer was manually modified</strong>").css "margin-top": "-20px"    if @model.get("IsModified")

    renderSchedule: ->
        that = this
        $.getJSON(window.gRootPath + "Underwriter/Schedule/Calculate",
            id: @model.get("CashRequestId")
        ).done (data) ->
            if data.hasOwnProperty('success') and not data.success
                EzBob.ShowMessage data.error, 'Error occured'
                that.$el.dialog 'close'
                return

            scheduleView = new EzBob.LoanScheduleView(
                el: that.$el.find(".loan-schedule")
                schedule: data
                isShowGift: false
                isShowExportBlock:false
                isShowExceedMaxInterestForSource: true
                ManualAddressWarning: data.ManualAddressWarning
                customer: that.model.get('CustomerName')
                refNum: that.model.get('CustomerRefNum')
                isPersonal: that.model.get('TypeOfBusiness') of [0, 4, 2]
            )
            scheduleView.render()
            that.$el.find("#loan-schedule .simple-well").hide()

    getButtonName: ->
        "Approve"

    dlgWidth: 880
    dlgHeight: 900
    onSaved: ->
        that = this
        @renderSchedule()

        $.post(
            window.gRootPath + "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed",
            { id: @model.get("CashRequestId"), loanTypeSelection: 2 }
        ).done( ->
            that.model.fetch()
        )

    changeLoanDetails: ->
        that = this
        loan = new EzBob.LoanModelTemplate(
            CashRequestId: @model.get("CashRequestId")
            CustomerId: @model.get("CustomerId")
        )
        xhr = loan.fetch()
        xhr.done ->
            view = new EzBob.EditLoanView(model: loan)
            EzBob.App.jqmodal.show view
            view.on "item:saved", that.onSaved, that
        false
    
    exportToPdf: (e) ->
        $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" + this.model.get("CashRequestId")+"&isExcel=false&isShowDetails=true&customerId="+@model.get("CustomerId"));
    
    exportToExcel: (e) ->
        $el = $(e.currentTarget);
        $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" + this.model.get("CashRequestId")+"&isExcel=true&isShowDetails=true&customerId="+@model.get("CustomerId"));
        
    exportToPrint: ->
        elem = $(".loan-schedule:visible")[0]
        domClone = elem.cloneNode(true)
        $printSection = document.getElementById("printSection");
        if (!$printSection)
            $printSection = document.createElement("div");
            $printSection.id = "printSection";
            document.body.appendChild($printSection);

        $printSection.innerHTML = "";
        $printSection.appendChild(domClone)
        window.print()
        return false

)