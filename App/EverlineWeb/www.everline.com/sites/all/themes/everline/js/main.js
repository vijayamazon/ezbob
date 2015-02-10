(function($, Drupal) {
  Drupal.Everline.navigation.init();
  Drupal.Everline.banners.init();
  Drupal.Everline.accordion.init();
  Drupal.Everline.carousel.init();

  if ($('.meet-the-team').length > 0) {
    Drupal.Everline.meetTheTeam.init();
  }

  // Three Step Tabs
  if ($(window).width() < 800) {
    $('.three-steps').find('.active').removeClass('active');
  }

  $('.three-steps').on('click', 'li', function() {
    var target = $(this).data('target'), openLI;
    var steps = $(this).parent();
    if (!$(this).hasClass('active')) {
      openLI = steps.find('.active');

      if ($(window).width() < 800) {
        openLI.find('.step-content').css('height', 'auto');
        $(this).find('.step-content').
          animate(
            {
              'height': $(this).find('.step-content').height()
            },
            function() {
              $(this).css('height', 'auto');
            }
          );
      }

      openLI.removeClass('active');

      $('#' + target).addClass('active');
      $(this).addClass('active');
    }
    // close on mobile
    else if ($(window).width() < 800) {
      openLI = steps.find('.active');

      $(this).find('.step-content').slideUp(function() {
        openLI.removeClass('active');
        openLI.find('.step-content').removeAttr('style');
      });
    }
  });


})(jQuery, Drupal);
