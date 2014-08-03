(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.LoanHistoryDetailsModel = (function(_super) {

    __extends(LoanHistoryDetailsModel, _super);

    function LoanHistoryDetailsModel() {
      return LoanHistoryDetailsModel.__super__.constructor.apply(this, arguments);
    }

    LoanHistoryDetailsModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/LoanHistory/Details?customerid=" + this.id + "&loanid=" + this.loanid;
    };

    return LoanHistoryDetailsModel;

  })(Backbone.Model);

  EzBob.Underwriter.LoanDetailsView = (function(_super) {

    __extends(LoanDetailsView, _super);

    function LoanDetailsView() {
      return LoanDetailsView.__super__.constructor.apply(this, arguments);
    }

    LoanDetailsView.prototype.initialize = function() {
      this.template = _.template($("#loan-details-template").html());
      return this.bindTo(this.model, "change reset", this.render, this);
    };

    LoanDetailsView.prototype.attributes = {
      "class": "underwriter-loan-details"
    };

    LoanDetailsView.prototype.render = function() {
      var that;
      that = this;
      this.$el.dialog({
        modal: true,
        resizable: true,
        title: "Loan Details - " + this.options.loan.RefNumber,
        position: "center",
        draggable: true,
        width: "1000",
        height: "900",
        close: function() {
          $(this).dialog("destroy");
          return that.trigger("close");
        }
      });
      this.renderContent();
      return this;
    };

    LoanDetailsView.prototype.renderContent = function() {
      var details, model, modelLoan;
      modelLoan = this.options.loan;
      model = this.model.toJSON();
      details = model.details;
      this.$el.html(this.template({
        loan: modelLoan,
        transactions: details && details.Transactions,
        schedule: details && details.Schedule,
        pacnetTransactions: details && details.PacnetTransactions,
        area: "Underwriter",
        rollovers: details && details.Rollovers,
        charges: details && details.Charges,
        showFailed: this.$el.find('.filter-errors').is(':checked'),
        rolloverCount: model.rolloverCount
      }));
      if (modelLoan.Modified) {
        return this.$el.find('.offer-status').append("<strong>Loan was manually modified</strong>").css({
          "margin-top": "-20px"
        });
      }
    };

    LoanDetailsView.prototype.events = {
      "click .rollover": "rollover",
      "click .make-payment": "makePayment",
      "click #btn-options": "showDialogOptions",
      "change .filter-errors": "renderContent",
      "click .pdf-link": "exportToPdf",
      "click .excel-link": "exportToExcel"
    };

    LoanDetailsView.prototype.rollover = function(e) {
      var model;
      if (!this.checkForActiveLoan()) {
        return false;
      }
      if (this.model.get("notExperiedRollover") && this.model.get("notExperiedRollover").PaidPaymentAmount > 0) {
        EzBob.ShowMessage("Rollover is partially paid. Cannot be edited.");
        return false;
      }
      model = {
        schedule: this.model.get("details").Schedule,
        rollover: this.model.get("details").Rollovers,
        configValues: this.model.get("configValues"),
        notExperiedRollover: this.model.get("notExperiedRollover"),
        loanId: this.model.loanid
      };
      this.rolloverView = new EzBob.Underwriter.RolloverView({
        model: model
      });
      EzBob.App.jqmodal.show(this.rolloverView);
      this.rolloverView.on("addRollover", this.addRollover, this);
      return this.rolloverView.on("removeRollover", this.removeRollover, this);
    };

    LoanDetailsView.prototype.removeRollover = function(roloverId) {
      var that;
      that = this;
      BlockUi("on");
      return $.post(window.gRootPath + "Underwriter/LoanHistory/RemoveRollover", roloverId).success(function(request) {
        if (request.success === false) {
          EzBob.ShowMessage(request.error, "Something went wrong");
          return;
        }
        EzBob.ShowMessage("Rollover succesfully removed");
        return that.model.fetch();
      }).done(function() {
        EzBob.App.jqmodal.hideModal(that.rolloverView);
        return BlockUi("off");
      });
    };

    LoanDetailsView.prototype.addRollover = function(model) {
      var that;
      that = this;
      BlockUi("on");
      return $.post(window.gRootPath + "Underwriter/LoanHistory/AddRollover", model).success(function(request) {
        if (request.success === false) {
          EzBob.ShowMessage(request.error, "Something went wrong");
          return;
        }
        EzBob.ShowMessage("Rollover succesfully " + (SerializeArrayToEasyObject(model).isEditCurrent === "true" ? "edited" : "added"));
        that.model.fetch();
        return that.trigger("RolloverAdded");
      }).done(function() {
        EzBob.App.jqmodal.hideModal(that.rolloverView);
        return BlockUi("off");
      });
    };

    LoanDetailsView.prototype.makePayment = function(e) {
      var model, view;
      model = {
        loanId: this.options.loan.Id
      };
      view = new EzBob.Underwriter.ManualPaymentView({
        model: new Backbone.Model(model)
      });
      EzBob.App.jqmodal.show(view);
      return view.on("addPayment", this.addPayment, this);
    };

    LoanDetailsView.prototype.addPayment = function(data) {
      var that;
      that = this;
      data += "&CustomerId=" + this.model.id;
      data += "&LoanId=" + this.options.loan.Id;
      BlockUi("on");
      return $.post(window.gRootPath + "Underwriter/LoanHistory/ManualPayment", data).success(function(response) {
        if (response.error) {
          return EzBob.ShowMessage(response.error, "Something went wrong", function() {});
        } else {
          return EzBob.ShowMessage("Manual payment succesfully added", "", function() {
            that.model.fetch();
            that.trigger("ManualPaymentAdded");
            return true;
          });
        }
      }).done(function() {
        return BlockUi("off");
      });
    };

    LoanDetailsView.prototype.showDialogOptions = function() {
      var that, xhr;
      this.loanOptionsModel = new EzBob.Underwriter.LoanOptionsModel();
      this.loanOptionsModel.loanId = this.model.loanid;
      xhr = this.loanOptionsModel.fetch();
      that = this;
      return xhr.done(function() {
        this.optionsView = new EzBob.Underwriter.LoanOptionsView({
          model: that.loanOptionsModel
        });
        this.optionsView.render();
        return EzBob.App.jqmodal.show(this.optionsView);
      });
    };

    LoanDetailsView.prototype.checkForActiveLoan = function() {
      if (this.options.loan.Status === "PaidOff") {
        EzBob.ShowMessage("Loan is  paid off", "Info");
        return false;
      }
      return true;
    };

    LoanDetailsView.prototype.exportToPdf = function(e) {
      var $el, customerId;
      customerId = this.model.id;
      $el = $(e.currentTarget);
      return $el.attr("href", window.gRootPath + "Underwriter/LoanHistory/ExportDetails?id=" + customerId + "&loanid=" + this.options.loan.Id + "&isExcel=false" + "&wError=" + this.$el.find('.filter-errors').is(':checked'));
    };

    LoanDetailsView.prototype.exportToExcel = function(e) {
      var $el, customerId;
      customerId = this.model.id;
      $el = $(e.currentTarget);
      return $el.attr("href", window.gRootPath + "Underwriter/LoanHistory/ExportDetails?id=" + customerId + "&loanid=" + this.options.loan.Id + "&isExcel=true" + "&wError=" + this.$el.find('.filter-errors').is(':checked'));
    };

    return LoanDetailsView;

  })(Backbone.Marionette.View);

}).call(this);
