﻿(function() {
  var root;

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.FunctionsDialogView = Backbone.View.extend({
    initialize: function() {
      this.template = _.template(this.getTemplate());
      return this.type = this.getType();
    },
    getTemplate: function() {
      return $("#functionsDialogTemplate").html();
    },
    getType: function() {
      return null;
    },
    render: function(id) {
      var buttonName;
      this.$el.html(this.template(this.model));
      buttonName = this.getButtonName();
      this.$el.find(".button-ok").val(buttonName);
      this.ReasonField = this.$el.find(".reason");
      this.YodleeReasonField = this.$el.find(".yodleeReason");
      if (!this.showReasonField()) {
        this.ReasonField.css("display", "none");
        this.$el.find("h3").css("display", "none");
      }
      this.ReasonField.val(this.model.get("Reason"));
      this.$el.dialog({
        autoOpen: true,
        position: ["top", 60],
        draggable: true,
        title: "Are you sure?",
        modal: true,
        resizable: true,
        width: this.dlgWidth || 520,
        height: this.dlgHeight || 300,
        dialogClass: "functionsPopup",
        open: _.bind(this.onShow, this)
      });
      return this;
    },
    getButtonName: function() {
      return "Ok";
    },
    showReasonField: function() {
      return true;
    },
    onShow: function() {},
    events: {
      "click .button-ok": "BtnOkClicked",
      "click .button-cancel": "BtnCancelClicked",
      "keydown textarea.reason": "TextAreaChanged",
      "keydown textarea.yodleeReason": "TextAreaChanged"
    },
    ReasonFieldEmptyError: function(field, isShow) {
      if (isShow) {
        return field.css("border", "1px solid red");
      } else {
        return field.css("border", "");
      }
    },
    TextAreaChanged: function(field) {
      if (this.getType() !== "Approved" || EzBob.isNullOrEmpty(this.model.get("OfferedCreditLine")) || this.model.get("OfferedCreditLine") !== 0) {
        $(".button-ok").removeClass("disabled");
      }
      return this.ReasonFieldEmptyError($(field.currentTarget), false);
    },
    BtnOkClicked: function(e) {
      var data, req, that;
      that = this;
      if ($(e.currentTarget).hasClass("disabled")) {
        return false;
      }
      $(e.currentTarget).addClass("disabled");
      req = false;
      if (this.ReasonField.val() === "" && this.showReasonField()) {
        this.ReasonFieldEmptyError(this.ReasonField, true);
        req = true;
      }
      if (this.YodleeReasonField.val() === "" && this.NoYodlee) {
        this.ReasonFieldEmptyError(this.YodleeReasonField, true);
        req = true;
      }
      if (req) {
        return false;
      }
      data = {
        id: this.model.get("CustomerId"),
        status: this.type
      };
      if (this.showReasonField()) {
        data.reason = this.ReasonField.val();
      }
      if (this.NoYodlee) {
        data.reason += " | " + this.YodleeReasonField.val();
      }
      req = $.post(window.gRootPath + "Underwriter/Customers/ChangeStatus", data);
      BlockUi("on");
      req.done(function(res) {
        if (res.error) {
          EzBob.ShowMessage(res.error, "Error occured");
          that.$el.css("border", "1px solid red");
          return;
        }
        that.$el.dialog("close");
        that.trigger("changedSystemDecision");
        return $(".ui-icon-refresh").click();
      });
      return req.complete(function() {
        BlockUi("off");
        return $(e.currentTarget).removeClass("disabled");
      });
    },
    BtnCancelClicked: function() {
      return this.$el.dialog("close");
    }
  });

  EzBob.Underwriter.RejectedDialog = EzBob.Underwriter.FunctionsDialogView.extend({
    getType: function() {
      return "Rejected";
    },
    getButtonName: function() {
      return "Reject";
    }
  });

  EzBob.Underwriter.Escalated = EzBob.Underwriter.FunctionsDialogView.extend({
    getType: function() {
      return "Escalated";
    },
    getButtonName: function() {
      return "Escalate";
    }
  });

  EzBob.Underwriter.Suspended = EzBob.Underwriter.FunctionsDialogView.extend({
    getType: function() {
      return "ApprovedPending";
    },
    getButtonName: function() {
      return "Suspend";
    },
    showReasonField: function() {
      return false;
    },
    dlgHeight: 120,
    dlgWidth: 300
  });

  EzBob.Underwriter.Returned = EzBob.Underwriter.FunctionsDialogView.extend({
    getType: function() {
      return "WaitingForDecision";
    },
    getButtonName: function() {
      return "Return";
    },
    showReasonField: function() {
      return false;
    },
    dlgHeight: 120,
    dlgWidth: 300
  });

  EzBob.Underwriter.ApproveDialog = EzBob.Underwriter.FunctionsDialogView.extend({
    events: function() {
      return _.extend({}, EzBob.Underwriter.FunctionsDialogView.prototype.events, {
        "click .change-offer-details": "changeLoanDetails",
        "click .pdf-link": "exportToPdf",
        "click .excel-link": "exportToExcel"
      });
    },
    getType: function() {
      return "Approved";
    },
    showReasonField: function() {
      return true;
    },
    onShow: function() {
      var _ref;
      this.renderDetails();
      this.renderSchedule();
      this.model.on("change", this.renderDetails, this);
      if (!this.model.get("OfferedCreditLine") || this.model.get("OfferedCreditLine") === 0) {
        this.$el.find(".button-ok").addClass("disabled");
      }
      if (this.model.get("OfferExpired")) {
        this.$el.find(".button-ok").addClass("disabled");
      }
      if ((_ref = this.model.get("IsLoanTypeSelectionAllowed")) === 1 || _ref === '1') {
        this.$el.find(".change-offer-details").attr('disabled', 'disabled');
      }
      this.NoYodlee = this.model.get('IsOffline') && !this.model.get('HasYodlee');
      return this.$el.find("#noYodleeReasonDiv").toggleClass('hide', !this.NoYodlee).toggleClass('uwReason', this.NoYodlee);
    },
    renderDetails: function() {
      var details;
      details = _.template($("#approve-details").html(), this.model.toJSON());
      this.$el.find("#details").html(details);
      if (this.model.get("IsModified")) {
        return this.$el.find(".offer-status").append("<strong>Offer was manually modified</strong>").css({
          "margin-top": "-20px"
        });
      }
    },
    renderSchedule: function() {
      var that;
      that = this;
      return $.getJSON(window.gRootPath + "Underwriter/Schedule/Calculate", {
        id: this.model.get("CashRequestId")
      }).done(function(data) {
        var scheduleView;
        scheduleView = new EzBob.LoanScheduleView({
          el: that.$el.find(".loan-schedule"),
          schedule: data,
          isShowGift: false,
          isShowExportBlock: false,
          isShowExceedMaxInterestForSource: true
        });
        scheduleView.render();
        return that.$el.find("#loan-schedule .simple-well").hide();
      });
    },
    getButtonName: function() {
      return "Approve";
    },
    dlgWidth: 650,
    dlgHeight: 900,
    onSaved: function() {
      var that;
      that = this;
      this.renderSchedule();
      return $.post(window.gRootPath + "Underwriter/ApplicationInfo/IsLoanTypeSelectionAllowed", {
        id: this.model.get("CashRequestId"),
        loanTypeSelection: 2
      }).done(function() {
        return that.model.fetch();
      });
    },
    changeLoanDetails: function() {
      var loan, that, xhr;
      that = this;
      loan = new EzBob.LoanModelTemplate({
        CashRequestId: this.model.get("CashRequestId"),
        CustomerId: this.model.get("CustomerId")
      });
      xhr = loan.fetch();
      xhr.done(function() {
        var view;
        view = new EzBob.EditLoanView({
          model: loan
        });
        EzBob.App.jqmodal.show(view);
        return view.on("item:saved", that.onSaved, that);
      });
      return false;
    },
    exportToPdf: function(e) {
      var $el;
      $el = $(e.currentTarget);
      return $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" + this.model.get("CashRequestId") + "&isExcel=false&isShowDetails=true&customerId=" + this.model.get("CustomerId"));
    },
    exportToExcel: function(e) {
      var $el;
      $el = $(e.currentTarget);
      return $el.attr("href", window.gRootPath + "Underwriter/Schedule/Export?id=" + this.model.get("CashRequestId") + "&isExcel=true&isShowDetails=true&customerId=" + this.model.get("CustomerId"));
    }
  });

}).call(this);
