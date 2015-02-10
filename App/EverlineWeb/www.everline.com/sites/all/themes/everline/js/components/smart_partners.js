(function($, Drupal) {
  Drupal.behaviors.smart_partners = function (context) {
    $('iframe.smart-partner', context).iFrameResize();
  }
})(jQuery, Drupal);
