///<reference path="~/Content/js/lib/backbone.js" />
///<reference path="~/Content/js/lib/underscore.js" />
/// <reference path="../lib/jquery.maskedinput-1.2.2.js" />

var EzBob = EzBob || {};

EzBob.BankAccountModel = Backbone.Model.extend({
    defaults: {
        displayName: null
    }
});

EzBob.BankAccounts = Backbone.Collection.extend({
    model: EzBob.BankAccountModel,
    url: window.gRootPath + 'Customer/PaymentAccounts/BankAccountsListFormatted'
});

EzBob.BankAccountButtonView = EzBob.StoreButtonView.extend({
    initialize: function () {
        EzBob.CT.bindShopToCT(this, 'bank');
        this.bankAccounts = new EzBob.BankAccounts();
        var accountNumber = this.model.get('bankAccount');
        if (accountNumber) {
            this.bankAccounts.add({ displayName: 'XXXX' + accountNumber.substring(4) });
        }
        this.constructor.__super__.initialize.apply(this, [{ name: "bank-account", logoText: "Add account for cash transfer" }]);
    },
    update: function () {
        this.bankAccounts.fetch();
    }
});

EzBob.BankAccountInfoView = Backbone.View.extend({
    events: {
        'change input[type="text"]': 'inputsChanged',
        'keyup input[type="text"]': 'inputsChanged',
        'click a.connect-bank': 'connect',
        "click a.back": "back"
    },

    initialize: function () {
        this.template = _.template($('#bank-account-instructions').html());
        this.model.on('change', this.render, this);
    },

    render: function () {
        var that = this;

        this.$el.html(this.template());
        this.$el.find('#AccountNumber, #SortCode1, #SortCode2, #SortCode3').numericOnly();
        this.form = this.$el.find('form');
        this.validator = EzBob.validateBankDetailsForm(this.form);

        this.$el.find("#SortCode #SortCode1").on("keyup change focusout", function () { that.$el.find("#SortCode .SortCodeSplit").val(that.$el.find("#SortCode #SortCode1").val() + that.$el.find("#SortCode #SortCode2").val() + that.$el.find("#SortCode #SortCode3").val()); });
        this.$el.find("#SortCode #SortCode2").on("keyup change focusout", function () { that.$el.find("#SortCode #SortCode1").trigger("change"); });
        this.$el.find("#SortCode #SortCode3").on("keyup change focusout", function () { that.$el.find("#SortCode #SortCode1").trigger("change"); });
        
        this.$el.find('input[nextSerial]').serialFill();

        $(".dashboard-steps li[data-step-num=0]").addClass("complete").removeClass("current");
        $(".dashboard-steps li[data-step-num=0] .inner-circle").addClass("complete");
        $(".dashboard-steps li[data-step-num=0] .progress-line-current").addClass("progress-line-complete");
        $(".dashboard-steps li[data-step-num=1]").addClass("current");
        $(".dashboard-steps li[data-step-num=1] .inner-circle").addClass("current");
        $(".dashboard-steps li[data-step-num=1] .progress-line-").addClass("progress-line-current");

        if (this.model.get('bankAccountAdded'))
            this.ready();

        this.$el.find('.field_status').field_status({ required: true });

	    EzBob.UiAction.registerView(this);

        return this;
    },

    inputsChanged: function () {
        var enabled = EzBob.Validation.checkForm(this.validator);
        this.$el.find('a.connect-bank').toggleClass('disabled', !enabled);
    },

    back: function () {
        this.trigger('back');
        return false;
    },

    connect: function (e) {
        var $el = $(e.currentTarget);
        if ($el.hasClass("disabled")) return false;
        /*
        if (this.$el.find('a.connect-bank').hasClass('disabled')) {
            EzBob.App.trigger("error", "You must fill in all of the fields.");
            return false;
        }*/

        var accNum = this.$el.find('#AccountNumber').val(),
			sortCode = this.$el.find('#SortCodeSplit').val(),
			bankAccountType = this.$el.find('input[name="bankAccountType"]:checked').val(),
			that = this;

        this.blockBtn(true);

        $.post(window.gRootPath + "Customer/PaymentAccounts/AddBankAccount", { accountNumber: accNum, sortCode: sortCode, bankAccountType: bankAccountType })
			.success(function (result) {
			    that.blockBtn(false);
			    if (result.error) {
			        EzBob.App.trigger('error', result.error);
			        return;
			    }
			    that.model.set('bankAccount', accNum);
			    that.model.set('sortCode', sortCode);

			    that.model.set('bankAccountAdded', true);
			    EzBob.App.trigger('info', result.msg);
			    that.trigger('completed');
			})
			.error(function () {
			    EzBob.App.trigger('error', 'Bank account adding failed');
			});

        return false;
    },

    blockBtn: function (isBlock) {
        BlockUi(isBlock ? "on" : "off");
        this.$el.find('.connect-bank').toggleClass("disabled", isBlock);
    },

    ready: function () {
        var accountNumber = this.model.get('bankAccount');
        var sortCode = this.model.get('sortCode');
        this.$el.find("#SortCode #SortCode1").val(sortCode.substring(0, 2));
        this.$el.find("#SortCode #SortCode2").val(sortCode.substring(2, 4));
        this.$el.find("#SortCode #SortCode3").val(sortCode.substring(4, 6));
        this.$el.find('#AccountNumber').val(accountNumber);
        this.$el.find(':input').not(':submit').attr('disabled', 'disabled').attr('readonly', 'readonly').css('disabled');
    }
});
