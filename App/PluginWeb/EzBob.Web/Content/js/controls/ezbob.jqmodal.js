(function() {
  var root,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  root = typeof exports !== "undefined" && exports !== null ? exports : this;

  root.EzBob = root.EzBob || {};

  EzBob.JqModalRegion = (function(_super) {

    __extends(JqModalRegion, _super);

    function JqModalRegion() {
      this.on('view:show', this.showModal, this);
      this.dialog = $('<div/>');
      JqModalRegion.__super__.constructor.call(this);
    }

    JqModalRegion.prototype.isUnderwriter = document.location.href.indexOf("Underwriter") > -1;

    JqModalRegion.prototype.el = 'fake';

    JqModalRegion.prototype.getEl = function(selector) {
      return this.dialog;
    };

    JqModalRegion.prototype.showModal = function(view) {
      var option,
        _this = this;
      view.on('close', this.hideModal, this);
      option = view.jqoptions();
      if (this.isUnderwriter) {
        option['resizable'] = true;
        option['draggable'] = true;
      }
      this.dialog.dialog(option);
      this.dialog.one('dialogclose', function() {
        return _this.close();
      });
      this.dialog.find('.ui-dialog').addClass('box');
      this.dialog.find('.ui-dialog-titlebar').addClass('box-title');
      this.dialog.parent('.ui-dialog').find('.ui-dialog-buttonset button').addClass('btn btn-info');
      if (view.onAfterShow) {
        return view.onAfterShow.call(view);
      }
    };

    JqModalRegion.prototype.hideModal = function() {
      return this.dialog.dialog('destroy');
    };

    return JqModalRegion;

  })(Backbone.Marionette.Region);

}).call(this);
