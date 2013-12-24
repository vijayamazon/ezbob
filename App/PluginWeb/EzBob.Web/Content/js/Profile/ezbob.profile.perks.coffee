root = exports ? this
root.EzBob = root.EzBob or {}
EzBob.Profile = EzBob.Profile or {}

class EzBob.Profile.PerksView extends Backbone.Marionette.Layout
    template: "#perks-template"
