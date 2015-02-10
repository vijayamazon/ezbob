(function($, Drupal) {
  Drupal.Everline = Drupal.Everline || {};

  Drupal.Everline.carousel = {
    init: function() {
      var me = this;

      (function timer() {
        setTimeout(function() {
          setTimeout(function() {
            me.nextSlide();
          }, 500);
          timer();
        }, 5000);
      })();
    },

    nextSlide: function() {
      var backPos = parseInt($('.carousel').css('background-position-x')),
          slideWidth = $('.slide').width(),
          leftPos = parseInt($('.slide').css('left')),
          count = 0;

      if ($(window).width() > 800) {
        $('.carousel').find('.slide').animate({
          'left': leftPos -= slideWidth
        }, 1500);

        $('.carousel').animate({
          'background-position-x': (backPos - 150) + 'px'
        }, 1500, function() {
          $('.carousel').append($('.carousel').find('.slide:first'));
          $('.carousel').find('.slide').css('left', 0);
        });
      }
      else {
        $('.carousel').find('.slide').animate({
          'left': leftPos -= slideWidth
        }, 1500, function() {
          if (count === 0) {
            $('.carousel').append($('.carousel').find('.slide:first'));
          }
          $('.carousel').find('.slide').css('left', 0);
          ++count;
        });

      }
    }
  };
})(jQuery, Drupal);
