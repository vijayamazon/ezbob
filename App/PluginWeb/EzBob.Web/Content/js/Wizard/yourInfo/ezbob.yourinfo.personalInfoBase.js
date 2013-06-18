var EzBob = EzBob || {};

EzBob.YourInformationStepViewBase = Backbone.View.extend({
    initialize: function () {
        this.ViewName = "";
        this.delegateEvents();
    },

    render: function () {
        this.$el.html(this.template(this.model.toJSON()));
        this.form = this.$el.find('form');
        this.validator = this.getValidator()(this.form);

        this.$el.find(".ezDateTime").splittedDateTime();
        this.$el.find('.phonenumber').numericOnly(11);

        this.$el.find('li[rel]').setPopover("left");
        
        var oFieldStatusIcons = this.$el.find('IMG.field_status');
        oFieldStatusIcons.filter('.required').field_status({ required: true });
        oFieldStatusIcons.not('.required').field_status({ required: false });
        
        return this;
    },
    events: {
        "click a.back": "clickBack",
        "click .btn-continue": "next"
    },

    addAddressError: function (el) {
        var error = $('<label class="error" generated="true">This field is required</label>');
        EzBob.Validation.errorPlacement(error, this.$el.find(el));
    },
    clearAddressError: function (el) {
        EzBob.Validation.unhighlight(this.$el.find(el));
    },

    PrevModelChange: function (el, model) {
        this.PrevAddressValidator = model.collection && model.collection.length > 0;
        if (this.PrevAddressValidator)
            this.clearAddressError("#PrevPersonAddresses");
    },
    clickBack: function () {
        this.trigger('back');
        EzBob.App.trigger("clear");
        return false;
    },
    TimeAtAddressChanged: function (buttonContainer, select) {
        var button = buttonContainer + " .btn";
        this.clearAddressError("#PrevPersonAddresses");
        var currentVal = this.$el.find(select).val();

        if (currentVal == 3 || !currentVal) {
            this.$el.find(button).parents('div.control-group').hide();
            this.PrevAddressValidator = true;
        } else {
            this.$el.find(button).parents('div.control-group').show();
            this.PrevAddressValidator = false;
        }
    }
});