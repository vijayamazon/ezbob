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
      'click .suggested-amount-link': 'suggestedAmountClicked',
      "keydown": "onEnterKeydown",
      'change .percent': 'percentChanged'
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
        width: 800
      };
    };

    CreditLineEditDialog.prototype.percentChanged = function(event) {
      var $elem, link, method, percent, value;
      $elem = $(event.target);
      percent = $elem.autoNumericGet() / 100;
      method = $elem.data('method');
      value = $elem.data('value');
      link = this.$el.find('a.Manual' + method);
      return link.text("Manual offer " + EzBob.formatPoundsNoDecimals(value * percent) + " (" + EzBob.formatPercents(percent) + ")").data('value', value * percent).data('percent', percent);
    };

    CreditLineEditDialog.prototype.onEnterKeydown = function(event) {
      if (event.keyCode === 13) {
        this.ui.amount.change().blur();
        this.save();
        return false;
      }
      return true;
    };

    CreditLineEditDialog.prototype.save = function() {
      var post,
        _this = this;
      if (!this.validator.checkForm()) {
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
      this.percent = $elem.data('percent');
      this.ui.amount.val(this.value).change().blur();
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
        value: this.value,
        percent: this.percent
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
      this.$el.find(".percent").autoNumeric(EzBob.percentFormat).blur();
      if (this.$el.find("#edit-offer-amount").val() === "-") {
        this.$el.find("#edit-offer-amount").val("");
      }
      return this.validator = this.setValidator();
    };

    CreditLineEditDialog.prototype.serializeData = function() {
      return {
        m: this.model.toJSON()
      };
    };

    CreditLineEditDialog.prototype.setValidator = function() {
      return this.ui.form.validate({
        rules: {
          editOfferAmount: {
            required: true,
            autonumericMin: EzBob.Config.XMinLoan,
            autonumericMax: EzBob.Config.MaxLoan
          }
        },
        messages: {
          editOfferAmount: {
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
