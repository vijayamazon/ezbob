(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.CreditLineEditDialog = (function(_super) {

    __extends(CreditLineEditDialog, _super);

    function CreditLineEditDialog() {
      return CreditLineEditDialog.__super__.constructor.apply(this, arguments);
    }

    CreditLineEditDialog.prototype.template = '#credit-line-edit-dialog-template';

    CreditLineEditDialog.prototype.initialize = function(options) {
      this.model = options.model;
      this.modelBinder = new Backbone.ModelBinder();
      this.method = null;
      this.medal = null;
      return this.value = null;
    };

    CreditLineEditDialog.prototype.events = {
      'click .btnOk': 'save',
      'click .suggested-amount-link': 'suggestedAmountClicked'
    };

    CreditLineEditDialog.prototype.ui = {
      form: "form",
      amount: "#edit-offer-amount"
    };

    CreditLineEditDialog.prototype.jqoptions = function() {
      return {
        modal: true,
        resizable: true,
        title: "Offer credit line edit",
        position: "center",
        draggable: true,
        dialogClass: "credit-line-edit-popup",
        width: 550
      };
    };

    CreditLineEditDialog.prototype.save = function() {
      var post,
        _this = this;
      if (!this.ui.form.valid()) {
        return;
      }
      post = $.post("" + window.gRootPath + "ApplicationInfo/ChangeCashRequestOpenCreditLine", this.getPostData());
      post.done(function() {
        _this.model.fetch();
        return _this.trigger('done');
      });
      return this.close();
    };

    CreditLineEditDialog.prototype.suggestedAmountClicked = function(el) {
      var $elem;
      $elem = $(el.currentTarget);
      this.method = $elem.data('method');
      this.medal = $elem.data('medal');
      this.value = $elem.data('value');
      this.ui.amount.val(this.value).change();
      this.save();
      return false;
    };

    CreditLineEditDialog.prototype.getPostData = function() {
      var data, m;
      m = this.model.toJSON();
      data = {
        id: m.CashRequestId,
        amount: m.amount,
        method: this.method,
        medal: this.medal,
        value: this.value
      };
      return data;
    };

    CreditLineEditDialog.prototype.bindings = {
      amount: {
        selector: "#edit-offer-amount",
        converter: EzBob.BindingConverters.moneyFormat
      }
    };

    CreditLineEditDialog.prototype.onRender = function() {
      this.modelBinder.bind(this.model, this.el, this.bindings);
      this.$el.find("#edit-offer-amount").autoNumeric(EzBob.moneyFormat);
      if (this.$el.find("#edit-offer-amount").val() === "-") {
        this.$el.find("#edit-offer-amount").val("");
      }
      return this.setValidator();
    };

    CreditLineEditDialog.prototype.serializeData = function() {
      return {
        m: this.model.toJSON()
      };
    };

    CreditLineEditDialog.prototype.setValidator = function() {
      return this.ui.form.validate({
        rules: {
          offeredCreditLine: {
            required: true,
            autonumericMin: EzBob.Config.XMinLoan,
            autonumericMax: EzBob.Config.MaxLoan
          }
        },
        messages: {
          offeredCreditLine: {
            autonumericMin: "Offer is below limit.",
            autonumericMax: "Offer is above limit."
          }
        },
        errorPlacement: EzBob.Validation.errorPlacement,
        unhighlight: EzBob.Validation.unhighlight
      });
    };

    return CreditLineEditDialog;

  })(Backbone.Marionette.ItemView);

}).call(this);
