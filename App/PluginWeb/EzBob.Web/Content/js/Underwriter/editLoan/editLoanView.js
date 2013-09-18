(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.EditLoanView = (function(_super) {
    __extends(EditLoanView, _super);

    function EditLoanView() {
      _ref = EditLoanView.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    EditLoanView.prototype.template = "#loan_editor_template";

    EditLoanView.prototype.scheduleTemplate = _.template($("#loan_editor_schedule_template").html());

    EditLoanView.prototype.freezeIntervalsTemplate = _.template($("#loan_editor_freeze_intervals_template").html());

    EditLoanView.prototype.initialize = function() {
      this.bindTo(this.model, "change sync", this.renderRegions, this);
      return this.editItemIndex = -1;
    };

    EditLoanView.prototype.serializeData = function() {
      var data;

      data = this.model.toJSON();
      data.editItemIndex = this.editItemIndex;
      return data;
    };

    EditLoanView.prototype.ui = {
      scheduleEl: ".editloan-schedule-region",
      freezeEl: ".editloan-freeze-intervals-region",
      ok: ".save",
      buttons: ".buttons"
    };

    EditLoanView.prototype.editors = {
      "Installment": EzBob.InstallmentEditor,
      "Fee": EzBob.FeeEditor
    };

    EditLoanView.prototype.events = {
      "click .edit-schedule-item": "editScheduleItem",
      "click .remove-schedule-item": "removeScheduleItem",
      "click .add-installment": "addInstallment",
      "click .add-fee": "addFee",
      "click .save": "onOk",
      "click .cancel": "onCancel",
      "click .add-freeze-interval": "onAddFreezeInterval",
      "click .remove-freeze-interval": "onRemoveFreezeInterval"
    };

    EditLoanView.prototype.addInstallment = function() {
      var add, date, editor, installment, view;

      date = new Date(this.model.get("Items").last().get("Date"));
      date.setMonth(date.getMonth() + 1);
      installment = new EzBob.Installment({
        "Editable": true,
        "Deletable": true,
        "Editor": "Installment",
        "Principal": 0,
        "Balance": 0,
        "BalanceBeforeRepayment": 0,
        "Interest": 0,
        "InterestRate": this.model.get("InterestRate"),
        "Fees": 0,
        "Total": 0,
        "Status": "StillToPay",
        "Type": "Installment",
        "IsAdding": true,
        "Date": date
      });
      editor = this.editors["Installment"];
      view = new editor({
        model: installment,
        loan: this.model
      });
      add = function() {
        return this.model.addInstallment(installment);
      };
      view.on("apply", add, this);
      return this.showEditView(view);
    };

    EditLoanView.prototype.addFee = function() {
      var add, editor, fee, view;

      fee = new EzBob.Installment({
        "Editable": true,
        "Deletable": true,
        "Editor": "Fee",
        "Principal": 0,
        "Balance": 0,
        "BalanceBeforeRepayment": 0,
        "Interest": 0,
        "InterestRate": this.model.get("InterestRate"),
        "Fees": 0,
        "Total": 0,
        "Type": "Fee",
        "IsAdding": true,
        "Date": this.model.get("Items").last().get("Date")
      });
      editor = this.editors["Fee"];
      view = new editor({
        model: fee,
        loan: this.model
      });
      add = function() {
        return this.model.addFee(fee);
      };
      view.on("apply", add, this);
      return this.showEditView(view);
    };

    EditLoanView.prototype.showEditView = function(view) {
      var _this = this;

      view.on("close", function() {
        return _this.ui.buttons.show();
      });
      this.editRegion.show(view);
      return this.ui.buttons.hide();
    };

    EditLoanView.prototype.removeScheduleItem = function(e) {
      var id, ok,
        _this = this;

      id = e.currentTarget.getAttribute("data-id");
      ok = function() {
        return _this.model.removeItem(id);
      };
      return EzBob.ShowMessage("Confirm deleting installment", "Delete installment", ok, "Ok", null, "Cancel");
    };

    EditLoanView.prototype.editScheduleItem = function(e) {
      var closed, editor, id, item, row, view;

      id = e.currentTarget.getAttribute("data-id");
      row = $(e.currentTarget).parents('tr');
      row.addClass("editing");
      item = this.model.get("Items").at(id);
      this.editItemIndex = id;
      editor = this.editors[item.get("Editor")];
      view = new editor({
        model: item,
        loan: this.model
      });
      view.on("apply", this.recalculate, this);
      closed = function() {
        row.removeClass("editing");
        this.editItemIndex = -1;
        return this.renderSchedule();
      };
      view.on("close", closed, this);
      return this.showEditView(view);
    };

    EditLoanView.prototype.recalculate = function() {
      return this.model.recalculate();
    };

    EditLoanView.prototype.onOk = function() {
      var xhr,
        _this = this;

      if (this.ui.ok.hasClass('disabled')) {
        return;
      }
      xhr = this.model.save();
      return xhr.done(function() {
        _this.trigger("item:saved");
        return _this.close();
      });
    };

    EditLoanView.prototype.onCancel = function() {
      return this.close();
    };

    EditLoanView.prototype.onRender = function() {
      this.editRegion = new Backbone.Marionette.Region({
        el: this.$(".editloan-item-editor-region")
      });
      return this.renderRegions();
    };

    EditLoanView.prototype.renderRegions = function() {
      this.renderSchedule();
      return this.renderFreeze();
    };

    EditLoanView.prototype.renderSchedule = function() {
      this.ui.scheduleEl.html(this.scheduleTemplate(this.serializeData()));
      return this.ui.ok.toggleClass("disabled", this.model.get("HasErrors"));
    };

    EditLoanView.prototype.renderFreeze = function() {
      return this.ui.freezeEl.html(this.freezeIntervalsTemplate(this.serializeData()));
    };

    EditLoanView.prototype.onAddFreezeInterval = function() {
      var nRate, sEnd, sStart;

      sStart = this.$el.find(".new-freeze-interval-start").val();
      sEnd = this.$el.find(".new-freeze-interval-end").val();
      nRate = this.$el.find(".new-freeze-interval-rate").val() / 100.0;
      this.$el.find('.new-freeze-interval-error').empty();
      if (this.validateFreezeIntervals(sStart, sEnd)) {
        return this.model.addFreezeInterval(sStart, sEnd, nRate);
      } else {
        return this.$el.find('.new-freeze-interval-error').text('New interval conflicts with one of existing intervals');
      }
    };

    EditLoanView.prototype.onRemoveFreezeInterval = function(evt) {
      return this.model.removeFreezeInterval(evt.currentTarget.getAttribute("data-id"));
    };

    EditLoanView.prototype.validateFreezeIntervals = function(sStartDate, sEndDate) {
      var bConflict, oEnd, oStart, tmp,
        _this = this;

      oStart = moment.utc(sStartDate);
      oEnd = moment.utc(sEndDate);
      if (oStart !== null && oEnd !== null && oStart > oEnd) {
        this.$el.find(".new-freeze-interval-start").val(sEndDate);
        this.$el.find(".new-freeze-interval-end").val(sStartDate);
        tmp = oEnd;
        oEnd = oStart;
        oStart = tmp;
      }
      bConflict = false;
      _.each(this.model.get('InterestFreeze'), function(item, idx) {
        var ary, bFirst, bSecond, oLeft, oRight;

        if (bConflict) {
          return;
        }
        ary = item.split('|');
        if (ary[4] !== '') {
          return;
        }
        oLeft = moment.utc(ary[0]);
        oRight = moment.utc(ary[1]);
        bFirst = _this.cmpDates(oStart, oRight);
        bSecond = _this.cmpDates(oLeft, oEnd);
        return bConflict = bFirst && bSecond;
      });
      return !bConflict;
    };

    EditLoanView.prototype.cmpDates = function(a, b) {
      if (a === null || b === null) {
        return true;
      }
      return a <= b;
    };

    EditLoanView.prototype.onClose = function() {
      return this.editRegion.close();
    };

    EditLoanView.prototype.jqoptions = function() {
      return {
        width: 1000,
        modal: true,
        title: 'Edit Loan Details',
        resizable: true
      };
    };

    return EditLoanView;

  })(Backbone.Marionette.ItemView);

}).call(this);
