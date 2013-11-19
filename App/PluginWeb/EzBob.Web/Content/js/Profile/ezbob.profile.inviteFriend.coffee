root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.InviteFriendView extends Backbone.Marionette.Layout
    template: "#invite-friend-template"

    events: 
        'click .inviteFriendHelp': 'inviteFriendHelpClicked'

    inviteFriendHelpClicked: ->
        @$el.find('.inviteFriendHelp').colorbox({ href: "#inviteFriendHelp", inline: true, transition: 'none', open: true });

    reload: -> 
        @model.fetch()
        .done =>
            @render()
            scrollTop()

