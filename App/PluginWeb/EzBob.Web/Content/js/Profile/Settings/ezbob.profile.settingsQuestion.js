var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.SettingsQuestionView = Backbone.View.extend({
    initialize: function () {
        this.template = _.template($('#settings-question-template').html());
    },
    render: function () {
        this.$el.html(this.template({ settings: this.model.toJSON() }));
        return this;
    },
    events: {
        'click .back': 'back',
        'click .submit': 'submit',
        'keyup input': 'inputKeyuped'
    },
    back: function () {
        this.trigger('back');
        return false;
    },
    submit: function (e) {
        var isDisable = $(e.currentTarget).hasClass("disabled");
        if (isDisable) return false;
        
        var questionId = this.$el.find('select option:selected').val(),
            question = this.$el.find('select option:selected').text(),
            answer = this.$el.find('input[name="answer"]').val(),
            password = this.$el.find('input[name="password"]').val(),
            that = this;
        $.post(window.gRootPath + "Customer/AccountSettings/UpdateSecurityQuestion", {
            Question: questionId,
            Answer: answer,
            Password: password
        }).done(function (r) {
            if (r.error) {
                EzBob.App.trigger('error', r.error);
                return false;
            }
            that.model.set({
                SecurityQuestionId: questionId,
                Answer: answer
            });
            
            EzBob.App.trigger('info', 'Security question has been changed');
            that.trigger("SecurityQuestionUpdated", question);
            that.$el.find('input[name="answer"]').val("");
            that.back();
        });
        this.$el.find('input[name="password"]').val("");
        return false;
    },
    inputKeyuped: function () {
        var $inputs = this.$el.find('input'),
            isEmpty = false,
            $submitButton = this.$el.find('.submit');

        _.each($inputs, function (value) {
            isEmpty = $(value).val().toString().isNullOrEmpty();
            if (isEmpty) return;
        });
        $submitButton.toggleClass("disabled", isEmpty);
    }
});