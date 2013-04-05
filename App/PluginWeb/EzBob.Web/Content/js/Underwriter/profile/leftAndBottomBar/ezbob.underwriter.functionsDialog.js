var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.FunctionsDialogView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template(this.getTemplate());
        this.type = this.getType();
    },
    getTemplate: function () {
        return $('#functionsDialogTemplate').html();
    },
    getType: function () {
        return null;
    },
    //---------------------------
    render: function (id) {
        this.$el.html(this.template(this.model));

        var buttonName = this.getButtonName();
        this.$el.find(".button-ok").val(buttonName);

        this.ReasonField = this.$el.find(".reason");

        if (!this.showReasonField()) {
            this.ReasonField.css("display", "none");
            this.$el.find("h3").css("display", "none");
        }

        this.ReasonField.val(this.model.get("Reason"));

        this.$el.dialog({
            autoOpen: true,
            position:['top',60],
            draggable: false,
            title: "Are you sure?",
            modal: true,
            resizable: false,
            width: this.dlgWidth || 520,
            height: this.dlgHeight || 300,
            dialogClass: "functionsPopup",
            open: _.bind(this.onShow, this)
        });

        return this;
    },
    getButtonName: function () {
        return "Ok";
    },
    showReasonField: function () {
        return true;
    },
    onShow: function () {
    },
    events: {
        "click .button-ok": "BtnOkClicked",
        "click .button-cancel": "BtnCancelClicked",
        "keydown textarea.reason" : "TextAreaChanged"
    },
    ReasonFieldEmptyError: function (isShow) {
        if (isShow) {
            this.ReasonField.css('border', '1px solid red');
        } else {
            this.ReasonField.css('border', '');
        }

    },
    TextAreaChanged: function () {
        if (this.getType() != "Approved" ||
            EzBob.isNullOrEmpty(this.model.get('OfferedCreditLine')) ||
            this.model.get('OfferedCreditLine') != 0) {
            
            $(".button-ok").removeClass("disabled");
        }
        this.ReasonFieldEmptyError(false);
    },
    BtnOkClicked: function (e) {
        var that = this;

        if ($(e.currentTarget).hasClass("disabled")) return false;
        
        $(e.currentTarget).addClass("disabled");

        if (this.ReasonField.val() == "" && this.showReasonField()) {
            this.ReasonFieldEmptyError(true);
            return false;
        }

        var data = {
            id: this.model.get('CustomerId'),
            status: this.type
        };

        if (this.showReasonField()) {
            data.reason = this.ReasonField.val();
        }        

        var req = $.post(window.gRootPath + "Underwriter/Customers/ChangeStatus", data);
        BlockUi("on");
        req.done(function (res) {
            if (res.error) {
                console.log(res.error);
                that.$el.css("border", "1px solid red");
                return;
            }
            that.$el.dialog("close");
            
            //refresh grid after decision
            that.trigger('changedSystemDecision');
            $(".ui-icon-refresh").click();
        });

        req.complete(function () {
            BlockUi("off");
            $(e.currentTarget).removeClass("disabled");
        });
    },

    BtnCancelClicked: function () {
        this.$el.dialog("close");
    }
});

EzBob.Underwriter.RejectedDialog = EzBob.Underwriter.FunctionsDialogView.extend({
    getType: function () {
        return "Rejected";
    },
    getButtonName: function () {
        return "Reject";
    }
});

EzBob.Underwriter.Escalated = EzBob.Underwriter.FunctionsDialogView.extend({
    getType: function () {
        return "Escalated";
    },
    getButtonName: function () {
        return "Escalate";
    }
});

EzBob.Underwriter.ApproveDialog = EzBob.Underwriter.FunctionsDialogView.extend({

    events: function(){
      return _.extend({},EzBob.Underwriter.FunctionsDialogView.prototype.events,{
          "click .change-offer-details" : "changeLoanDetails"
      });
   },

    getType: function () {
        return "Approved";
    },

    showReasonField: function () {
        return true;
    },

    onShow: function () {
        this.renderDetails();
        this.renderSchedule();
        this.model.on("change", this.renderDetails, this);


        if (!this.model.get('OfferedCreditLine') || this.model.get('OfferedCreditLine') === 0) {
            this.$el.find('.button-ok').addClass('disabled');
        }
        if (this.model.get('OfferExpired')) {
            this.$el.find('.button-ok').addClass('disabled');
        }
    },

    renderDetails: function () {
        var details = _.template($('#approve-details').html(), this.model.toJSON());
        this.$el.find('#details').html(details);

        if (this.model.get("IsModified")) {
            this.$el.find('.offer-status').append("<strong>Offer was manually modified</strong>").css({"margin-top": "-20px"});
        }
    },
    
    renderSchedule: function(){
        var that = this;
        $.getJSON(window.gRootPath + "Underwriter/Schedule/Calculate", {
            id: this.model.get("CashRequestId")
        })
        .done(function (data) {
            var scheduleView = new EzBob.LoanScheduleView({
                el: that.$el.find(".loan-schedule"),
                schedule: data,
                isShowGift: false
            });
            scheduleView.render();
            that.$el.find("#loan-schedule .simple-well").hide();
        });
    },

    getButtonName: function () {
        return "Approve";
    },

    dlgWidth: 540,
    dlgHeight: 750,

    onSaved: function () {
        this.renderSchedule();
        this.model.fetch();
    },

    changeLoanDetails: function () {
        var that = this;
        var loan = new EzBob.LoanModelTemplate({ CashRequestId: this.model.get("CashRequestId"), CustomerId: this.model.get("CustomerId") });
        var xhr = loan.fetch();
        xhr.done(function () {
            var view = new EzBob.EditLoanView({ model: loan });
            EzBob.App.jqmodal.show(view);
            view.on("item:saved", that.onSaved, that);
        });
        return false;
    }
});