var EzBob = EzBob || {};
EzBob.Profile = EzBob.Profile || {};

EzBob.Profile.InviteFriendView = Backbone.Marionette.Layout.extend({
	template: "#invite-friend-template",

	events: {
		'click .inviteFriendHelp': 'inviteFriendHelpClicked'
	}, // events

	inviteFriendHelpClicked: function() {
		var oBox = this.$el.find('.inviteFriendHelp');

		EzBob.UiAction.registerChildren(oBox);

		oBox.colorbox({
			href: "#inviteFriendHelp",
			inline: true,
			open: true,
		});
	}, // inviteFriendHelpClicked

	reload: function() {
		var self = this;

		this.model.fetch().done(function() {
			self.render();
			return scrollTop();
		});
	}, // reload
}); // EzBob.Profile.InviteFriendView
