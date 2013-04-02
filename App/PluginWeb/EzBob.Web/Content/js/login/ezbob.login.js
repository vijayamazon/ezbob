var EzBob = EzBob || {};

EzBob.LoginView = Backbone.View.extend({
    initialize: function () {
        this.content = $('#login-template').html();
        EzBob.App.on('signedIn', this.signedIn, this);
        EzBob.App.on('ct:showLogin', this.ctShowLogin, this);
        EzBob.App.on('ct:hideLogin', this.ctHideLogin, this);
    },
    ctShowLogin: function () {
        $('a.login').popover('show');
    },
    ctHideLogin: function () {
        $('a.login').popover('hide');
    },
    render: function () {
        $('a.login')
            .popover({
                placement: 'bottom',
                trigger: 'manual',
                content: this.content,
                title: "Login form",
                html: true
            })
            .on('click', function (e) {
                $('a.contactUsTop').popover('hide');
                $(e.target).popover('show');
                $(window).trigger('resize');
                EzBob.CT.recordEvent('ct:showLogin');
                $('.simle-login-input').placeholder();
                if(!$.browser.msie) $("#UserName").focus();
                $("#loginSubmitBtn").blur(function () {
                    $("#UserName").focus();
                });

                return false;
            });

        $('body').on('click', function (el) {
            if (!$(el.target).parents('.popover').length && $('.popover.in #UserName').length > 0) {
                $('a.login').popover('hide');
                if (EzBob.CT != undefined) {
                    EzBob.CT.recordEvent('ct:hideLogin');
                }

            }
        });
        
        return this;
    },
    signedIn: function () {
        this.$el.find('.login').hide();
        this.$el.find('.logoff').show();
    }
});