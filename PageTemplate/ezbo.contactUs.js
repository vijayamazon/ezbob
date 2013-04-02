var EzBob = EzBob || {};

EzBob.ContactUsView = Backbone.View.extend({
    initialize: function () {

    },
    render: function () {
        $('a.contactUsTop').popover({
            placement: 'bottom',
            trigger: 'manual',
            content: this.template,
            title: "Login form"
        }).on('click', function (e) {
            $('a.login').popover('hide');
            $(e.target).popover('toggle');
            $(window).trigger('resize');
            return false;
        });

        $('body').on('click', function (el) {
            if (!$(el.target).parents('.popover').length) {
                $('a.contactUsTop').popover('hide');
                $(window).trigger('resize');
            }
        });
        return this;
    }
});