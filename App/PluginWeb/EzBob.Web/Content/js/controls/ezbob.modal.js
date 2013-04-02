var EzBob = EzBob || {};

EzBob.ModalRegion = Backbone.Marionette.Region.extend({
    el: "#modalRegion",

    constructor: function(){
      _.bindAll(this);
      Backbone.Marionette.Region.prototype.constructor.apply(this, arguments);
      this.on("view:show", this.showModal, this);
    },

    getEl: function(selector){
      var $el = $(selector);
      $el.on("hidden", this.close);
      return $el;
    },

    showModal: function (view) {
        var options = view.modalOptions || view.options || { show: true, keyboard: false };

        view.on("close", this.hideModal, this);
        
        this.$el.modal(options);
        
        this.$el.on("shown", function () {
            view.trigger("showed");
        });
        
        this.$el.on("hidden", function () {
            view.trigger("hidded");
        });
    },

    hideModal: function(){
      this.$el.modal('hide');
    }
});

EzBob.ModalRegion2 = EzBob.ModalRegion.extend({
    el: "#modalRegion2"
})