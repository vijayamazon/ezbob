(function() {
  var root, _ref, _ref1,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.LoanHistoryModel = (function(_super) {
    __extends(LoanHistoryModel, _super);

    function LoanHistoryModel() {
      _ref = LoanHistoryModel.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    LoanHistoryModel.prototype.idAttribute = "Id";

    LoanHistoryModel.prototype.url = function() {
      return "" + window.gRootPath + "Underwriter/LoanHistory/Index/" + this.customerId;
    };

    return LoanHistoryModel;

  })(Backbone.Model);

  EzBob.Underwriter.LoanHistoryView = (function(_super) {
    __extends(LoanHistoryView, _super);

    function LoanHistoryView() {
      _ref1 = LoanHistoryView.__super__.constructor.apply(this, arguments);
      return _ref1;
    }

    LoanHistoryView.prototype.initialize = function() {
      this.template = _.template($("#loanhistory-template").html());
      this.templateView = _.template($("#loanhistory-view-template").html());
      this.offersTemplate = _.template($("#offrers-template").html());
      this.bindTo(this.model, "reset fetch change sync", this.render, this);
      return this.isRejections = true;
    };

    LoanHistoryView.prototype.events = {
      "click tr.loans.tr-link": "rowClick",
      "click .export-to-exel": "exportExcel",
      "click .edit-loan": "editLoan",
      "click .show-schedule": "showSchedule",
      "click .show-rejections": "showRejections"
    };

    LoanHistoryView.prototype.showRejections = function() {
      this.isRejections = this.$el.find('.show-rejections').is(':checked');
      return this.renderOffers();
    };

    LoanHistoryView.prototype.exportExcel = function() {
      return location.href = "" + window.gRootPath + "Underwriter/LoanHistory/ExportToExel?id=" + this.model.customerId;
    };

    LoanHistoryView.prototype.rowClick = function(e) {
      var details, detailsView, id, loan;

      id = +e.currentTarget.getAttribute("data-id");
      if (id == null) {
        return;
      }
      details = new EzBob.Underwriter.LoanHistoryDetailsModel();
      details.loanid = id;
      loan = _.find(this.model.get("loans"), function(l) {
        return l.Id === id;
      });
      details.id = this.idCustomer;
      detailsView = new EzBob.Underwriter.LoanDetailsView({
        model: details,
        loan: loan
      });
      detailsView.on("RolloverAdded", this.updateView, this);
      detailsView.on("ManualPaymentAdded", this.updateView, this);
      return details.fetch();
    };

    LoanHistoryView.prototype.updateView = function() {
      return this.model.fetch();
    };

    LoanHistoryView.prototype.editLoan = function(e) {
      var id, loan, xhr,
        _this = this;

      id = e.currentTarget.getAttribute("data-id");
      loan = new EzBob.LoanModel({
        Id: id
      });
      xhr = loan.fetch();
      xhr.done(function() {
        var view;

        view = new EzBob.EditLoanView({
          model: loan
        });
        view.on("item:saved", _this.updateView, _this);
        return EzBob.App.jqmodal.show(view);
      });
      return false;
    };

    LoanHistoryView.prototype.render = function() {
      var viewModel;

      this.$el.html(this.templateView());
      this.table = this.$el.find("#loanhistory-table");
      viewModel = this.model.toJSON();
      this.table.html(this.template(viewModel));
      this.renderOffers();
      return this;
    };

    LoanHistoryView.prototype.renderOffers = function() {
      var data;

      data = {
        offers: this.filterOffers()
      };
      this.offersConteiner = this.$el.find("#offers-conteiner");
      this.offersConteiner.html(this.offersTemplate(data));
      return this;
    };

    LoanHistoryView.prototype.filterOffers = function() {
      var ofers;

      if (this.isRejections) {
        return ofers = _.filter(this.model.get("offers"), function(o) {
          return o.UnderwriterDecision === "Rejected" || o.UnderwriterDecision === "Approved";
        });
      } else {
        return ofers = _.filter(this.model.get("offers"), function(o) {
          return o.UnderwriterDecision === "Approved";
        });
      }
    };

    LoanHistoryView.prototype.showSchedule = function(e) {
      var offerId, xhr,
        _this = this;

      offerId = $(e.currentTarget).data('id');
      xhr = $.getJSON("" + window.gRootPath + "Underwriter/Schedule/Calculate/" + offerId);
      xhr.done(function(data) {
        var view;

        view = new EzBob.LoanScheduleViewDlg({
          schedule: data,
          isShowGift: false,
          isShowExportBlock: true,
          offerId: offerId,
          customerId: _this.model.customerId
        });
        return EzBob.App.jqmodal.show(view);
      });
      return false;
    };

    return LoanHistoryView;

  })(Backbone.Marionette.View);

}).call(this);
