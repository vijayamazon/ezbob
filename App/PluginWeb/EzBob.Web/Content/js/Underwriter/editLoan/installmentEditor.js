(function() {
  var root, _ref,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.AddInstallmentTrack = (function() {
    function AddInstallmentTrack(installment, loan) {
      this.installment = installment;
      this.loan = loan;
      this.installment.on("change:Date", this.dateChanged, this);
    }

    AddInstallmentTrack.prototype.dateChanged = function() {
      var after, balance, balanceBefore, before, date;

      date = moment.utc(this.installment.get("Date"));
      before = this.loan.getInstallmentBefore(date);
      after = this.loan.getInstallmentAfter(date);
      balance = 0;
      balanceBefore = 0;
      if (before === null) {
        balance = this.loan.get('Amount');
        balanceBefore = balance;
      }
      if (after === null) {
        balance = 0;
        balanceBefore = before.get('Balance');
      }
      if (before !== null && after !== null) {
        balance = before.get('Balance') - after.get('Balance');
        balance = balance * (date.toDate() - moment.utc(before.get("Date")).toDate());
        balance = balance / (moment.utc(after.get("Date")).toDate() - moment.utc(before.get("Date")).toDate());
        balance = before.get('Balance') - balance;
        balance = Math.round(balance * 100) / 100;
        balanceBefore = before.get('Balance');
      }
      this.installment.set({
        "BalanceBeforeRepayment": balanceBefore
      });
      return this.installment.set({
        "Balance": balance
      });
    };

    return AddInstallmentTrack;

  })();

  EzBob.InstallmentEditor = (function(_super) {
    __extends(InstallmentEditor, _super);

    function InstallmentEditor() {
      _ref = InstallmentEditor.__super__.constructor.apply(this, arguments);
      return _ref;
    }

    InstallmentEditor.prototype.template = "#loan_editor_edit_installment_template";

    InstallmentEditor.prototype.initialize = function() {
      this.oldValues = this.model.toJSON();
      this.modelBinder = new Backbone.ModelBinder();
      if (this.model.get("IsAdding")) {
        return new EzBob.AddInstallmentTrack(this.model, this.options.loan);
      }
    };

    InstallmentEditor.prototype.events = {
      "click .cancel": "cancelChanges",
      "click .apply": "saveChanges"
    };

    InstallmentEditor.prototype.bindings = {
      Date: {
        selector: "input[name='date']",
        converter: EzBob.BindingConverters.dateTime
      },
      Balance: {
        selector: "input[name='balance']",
        converter: EzBob.BindingConverters.floatNumbers
      },
      Principal: {
        selector: "input[name='loanRepayment']",
        converter: EzBob.BindingConverters.floatNumbers
      },
      InterestRate: {
        selector: "input[name='interestRate']",
        converter: EzBob.BindingConverters.percents
      },
      Total: {
        selector: "input[name='totalRepayment']",
        converter: EzBob.BindingConverters.floatNumbers
      }
    };

    InstallmentEditor.prototype.ui = {
      form: "form",
      shift: ".shift-installments :checkbox",
      shiftRates: ".shift-rates :checkbox"
    };

    InstallmentEditor.prototype.onRender = function() {
      this.setValidation();
      this.modelBinder.bind(this.model, this.el, this.bindings);
      this.$el.find('input[name="date"]').datepicker({
        format: 'dd/mm/yyyy'
      });
      return this.$el.find('input[data-content], span[data-content]').setPopover();
    };

    InstallmentEditor.prototype.setValidation = function() {
      return this.ui.form.validate({
        rules: {
          date: {
            required: true,
            minlength: 6,
            maxlength: 20
          },
          interestRate: {
            positive: true,
            max: 100
          },
          balance: {
            min: 0
          }
        },
        messages: {
          "date": {
            required: "Please, fill the installment date"
          },
          "interestRate": {
            positive: "Interest rate cannot be less than zero",
            max: "Interest rate cannot be greater than 100%"
          },
          "balance": "Balance cannot be less than zero"
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
    };

    InstallmentEditor.prototype.saveChanges = function() {
      if (!this.ui.form.valid()) {
        return;
      }
      if (this.ui.shift.prop('checked') && this.oldValues.Date !== this.model.get("Date")) {
        this.options.loan.shiftDate(this.model, this.model.get("Date"), this.oldValues.Date);
      }
      if (this.ui.shiftRates.prop('checked') && this.oldValues.InterestRate !== this.model.get("InterestRate")) {
        this.options.loan.shiftInterestRate(this.model, this.model.get("InterestRate"));
      }
      this.trigger("apply");
      this.close();
      return false;
    };

    InstallmentEditor.prototype.cancelChanges = function() {
      this.model.set('Principal', this.oldValues.Principal);
      this.close();
      return false;
    };

    InstallmentEditor.prototype.onClose = function() {
      return this.modelBinder.unbind();
    };

    return InstallmentEditor;

  })(Backbone.Marionette.ItemView);

}).call(this);
