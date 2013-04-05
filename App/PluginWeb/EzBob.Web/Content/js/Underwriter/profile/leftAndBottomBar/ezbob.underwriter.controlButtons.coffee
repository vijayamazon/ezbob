root = exports ? this
root.EzBob = root.EzBob or {}

class EzBob.Underwriter.ControlButtonsView extends Backbone.Marionette.ItemView
    template: _.template( ( if $("#controls-button-template").length > 0 then $("#controls-button-template") else $("<div/>") ).html()) #fix for error on Reports