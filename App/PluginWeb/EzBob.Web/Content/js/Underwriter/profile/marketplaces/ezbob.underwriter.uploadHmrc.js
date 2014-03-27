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

    UploadHmrcView.prototype.initialize = function(options) {
      return this.customerId = options.customerId;
    };

    UploadHmrcView.prototype.ui = {
      hmrcUploadZone: "#hmrcUploadZone",
      uploadHmrcButton: ".uploadHmrc"
    };

    UploadHmrcView.prototype.events = {
      "click .uploadHmrc": "uploadHmrcClicked",
      "click .back": "backClicked"
    };

    UploadHmrcView.prototype.serializeData = function() {
      var data;
      return data = {
        customerId: this.customerId
      };
    };

    UploadHmrcView.prototype.onRender = function() {
      var that;
      that = this;
      Dropzone.options.hmrcUploadZone = {
        init: function() {
          return this.on("complete", function(file) {
            var enabled;
            if (this.getUploadingFiles().length === 0 && this.getQueuedFiles().length === 0) {
              enabled = this.getAcceptedFiles() !== 0;
              return that.ui.uploadHmrcButton.toggleClass('disabled', !enabled);
            }
          });
        }
      };
      this.ui.hmrcUploadZone.dropzone();
      return this;
    };

    UploadHmrcView.prototype.uploadHmrcClicked = function(event) {
      var that, xhr;
      event.preventDefault();
      event.stopPropagation();
      if (this.ui.uploadHmrcButton.hasClass('disabled')) {
        return false;
      }
      that = this;
      BlockUi('on');
      xhr = $.post(window.gRootPath + "UploadHmrc/UploadFiles", {
        customerId: this.customerId
      });
      xhr.done(function(res) {
        if (res.error !== void 0) {
          return EzBob.App.trigger('error', 'Failed to Save HMRC Account');
        } else {
          return EzBob.App.vent.trigger('ct:marketplaces.history', null);
        }
      });
      return xhr.always(function() {
        return BlockUi('off');
      });
    };

    UploadHmrcView.prototype.backClicked = function(event) {
      event.preventDefault();
      event.stopPropagation();
      return EzBob.App.vent.trigger('ct:marketplaces.uploadHmrcBack');
    };

    return UploadHmrcView;

  })(Backbone.Marionette.ItemView);

}).call(this);
