var EzBob = EzBob || {};

EzBob.ModalRegion = Backbone.Marionette.Region.extend({
    el: "#modalRegion",
    isUnderwriter: document.location.href.indexOf("Underwriter") > -1,

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
            $(this).data('modal', null);
        });
        this.setDisplayOptions();
    },
    
    setDisplayOptions: function () {
        if (!this.isUnderwriter) return;

        var $el = $(this.$el),
            modalHeight = $el.height(),
            modalWidth = $el.width(),
            $modalBody = $el.find('.modal-body'),
            bodyHeight = $modalBody.height(),
            bodyWidht = $modalBody.width();
        
        $el.resizable({
            minHeight: modalHeight,
            minWidth: modalWidth,
            resize: function(e, ui) {
                $modalBody.height(bodyHeight + ui.size.height - modalHeight);
                $modalBody.width(bodyWidht + ui.size.width - modalWidth);
            }
        }).draggable();
    },

    hideModal: function () {
        this.$el.modal('hide');
        
        if (this.isUnderwriter) {
            $(this.$el).resizable("destroy");
            $(this.$el).draggable("destroy");
            $(this.$el).css({                
                width: '',
                height:''
            });
        }
    },
});

EzBob.ModalRegion2 = EzBob.ModalRegion.extend({
    el: "#modalRegion2"
})