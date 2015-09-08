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

EzBob.BankAccountInfoView = Backbone.View.extend({
    events: {
        'change input[type="text"]': 'inputsChanged',
        'keyup input[type="text"]': 'inputsChanged',
        'change input[type="checkbox"]': 'inputsChanged',
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

        if (this.model.get('bankAccountAdded'))
            this.ready();

        this.$el.find('.field_status').field_status({ required: true });

        var notifications = new EzBob.NotificationsView({ el: this.$el.find('.notifications') });
        notifications.render();
        
	    EzBob.UiAction.registerView(this);
	    this.inputsChanged();
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
        if ($el.hasClass("disabled")) {
            return false;
        }
        
        if (!EzBob.Validation.checkForm(this.validator)) {
            return false;
        }
        
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
