(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Profile = EzBob.Profile || {};

  EzBob.Profile.InviteFriendView = (function(_super) {

    __extends(InviteFriendView, _super);

    function InviteFriendView() {
      return InviteFriendView.__super__.constructor.apply(this, arguments);
    }

    InviteFriendView.prototype.template = "#invite-friend-template";

    InviteFriendView.prototype.events = {
      'click .inviteFriendHelp': 'inviteFriendHelpClicked'
    };

    InviteFriendView.prototype.inviteFriendHelpClicked = function() {
      return this.$el.find('.inviteFriendHelp').colorbox({
        href: "#inviteFriendHelp",
        inline: true,
        transition: 'none',
        open: true
      });
    };

    InviteFriendView.prototype.reload = function() {
      var _this = this;
      return this.model.fetch().done(function() {
        _this.render();
        return scrollTop();
      });
    };

    return InviteFriendView;

  })(Backbone.Marionette.Layout);

}).call(this);
