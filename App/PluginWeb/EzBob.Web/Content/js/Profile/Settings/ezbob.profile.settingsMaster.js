var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.AccountSettingsModel = Backbone.Model.extend({
    defaults: {
    },
    initialize: function (options) {
        this.set('SecurityQuestions', new EzBob.Profile.SecurityQuestions(options.SecurityQuestions));
        this.set('SecurityQuestionId', options.SecurityQuestionModel!=undefined ? options.SecurityQuestionModel.Question : null);
        this.set('Answer', options.SecurityQuestionModel!=null ?options.SecurityQuestionModel.Answer : null);
    }
});

EzBob.Profile.SecurityQuestionModel = Backbone.Model.extend({
    defaults: {
        Name: ""
    },
    idAttribute: 'Id'
});

EzBob.Profile.SecurityQuestions = Backbone.Collection.extend({
    model: EzBob.Profile.SecurityQuestionModel
});

EzBob.Profile.SettingsMasterView = Backbone.View.extend({
    initialize: function () {
        this.mainView = new EzBob.Profile.SettingsMainView({ model: this.model });
        this.passwordView = new EzBob.Profile.SettingsPasswordView({ model: this.model });
        this.questionView = new EzBob.Profile.SettingsQuestionView({ model: this.model });

        this.mainView.on('edit:password', this.editPassword, this);
        this.mainView.on('edit:question', this.editQuestion, this);

        this.passwordView.on('back', this.resetView, this);
        this.questionView.on('back', this.resetView, this);
        this.questionView.on('SecurityQuestionUpdated', this.SecurityQuestionUpdated, this);
    },
    render: function () {
        this.$el.empty();
        this.resetView();
        this.mainView.render().$el.appendTo(this.$el);
        this.passwordView.render().$el.appendTo(this.$el);
        this.questionView.render().$el.appendTo(this.$el);

        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });
        this.$el.find('#securityQuestionImage').field_status('set', 'ok');
		return this;
    },
    resetView: function () {
        this.mainView.$el.show();
        this.passwordView.$el.hide();
        this.questionView.$el.hide();
        return this;
    },
    editPassword: function () {
        this.mainView.$el.hide();
        this.passwordView.$el.show();
        this.questionView.$el.hide();
    },
    editQuestion: function () {
        this.mainView.$el.hide();
        this.passwordView.$el.hide();
        this.questionView.$el.show();
    },
    SecurityQuestionUpdated: function (value) {
        this.$el.find(".security-answer").val(value);
    }
});