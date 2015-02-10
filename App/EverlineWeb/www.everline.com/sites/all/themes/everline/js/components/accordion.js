(function($, Drupal) {
  Drupal.Everline = Drupal.Everline || {};

  Drupal.Everline.accordion = {
    init: function() {
      $('.accordion').on('click', 'li',
        $.proxy(this.checkAccordionState, this));
    },

    checkAccordionState: function(e) {
      e.preventDefault();
      e.stopPropagation();

      if ($(e.currentTarget).hasClass('active')) {
        this.closeAccordionItem($(e.currentTarget));
      } else {
        this.openAccordionItem($(e.currentTarget));
      }
    },

    openAccordionItem: function(el) {
      if ($('.accordion').find('.active').length > 0) {
        this.closeAccordionItem($('.accordion').find('.active'));
      }
      el.find('.content').slideDown(350);
      el.addClass('active');
    },

    closeAccordionItem: function(el) {
      el.find('.content').slideUp(350);
      el.removeClass('active');
    }
  };
})(jQuery, Drupal);
