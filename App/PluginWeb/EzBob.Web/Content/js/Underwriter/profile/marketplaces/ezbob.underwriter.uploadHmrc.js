(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.Underwriter = EzBob.Underwriter || {};

  EzBob.Underwriter.UploadHmrcView = (function(_super) {

    __extends(UploadHmrcView, _super);

    function UploadHmrcView() {
      return UploadHmrcView.__super__.constructor.apply(this, arguments);
    }

    UploadHmrcView.prototype.template = "#hmrc-upload-template";

    UploadHmrcView.prototype.initialize = function(options) {};

    UploadHmrcView.prototype.events = {
      "click .uploadHmrc": "uploadHmrcClicked",
      "click .back": "backClicked"
    };

    UploadHmrcView.prototype.uploadHmrcClicked = function() {};

    UploadHmrcView.prototype.backClicked = function() {
      return EzBob.App.vent.trigger('ct:marketplaces.uploadHmrcBack');
    };

    return UploadHmrcView;

  })(Backbone.Marionette.ItemView);

}).call(this);
