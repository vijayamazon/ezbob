(function($, Drupal) {
  Drupal.Everline = Drupal.Everline || {};

  Drupal.Everline.meetTheTeam = {
    prevTargetPos: 0,
    init: function() {
      this.addListeners();

      this.teamWidth = ($('#team-carousel').find('li').width() + parseInt($('#team-carousel').find('li').css('margin-right'))) * $('.team-carousel').find('li').length;

    },

    addListeners: function() {
      $('.meet-the-team').on('click', 'a', $.proxy(this.loadContent, this));
      $(window).on('scroll', $.proxy(this.windowScrolled, this));
      $('.our-principles').on('mouseenter', 'li', this.principleHovered);

      this.ulScroll = new iScroll('team-carousel', {hScrollbar: true, hScroll: true, vScroll: false});
      $('#team-carousel').height($('#team-carousel').find('ul').height());
    },

    loadContent: function(e) {
      e.preventDefault();
      e.stopPropagation();

      // SET State of the person thumbnails
      if ($('.meet-the-team').find('.active').length > 0) {
        $('.meet-the-team').find('.active').removeClass('active');
      }
      $(e.currentTarget).parent().toggleClass('active');

      // Load correct content
      $('#' + $(e.currentTarget).attr('rel')).toggleClass('active');

      var el = $(e.target.parentNode),
          distToMove = (el.index() - this.prevTargetPos) * (el.width() + parseInt(el.css('margin-right')));

      if (this.ulScroll.startX == 0) {
        distToMove -= (el.width() + parseInt(el.css('margin-right'))) /2;
      }


      if (this.ulScroll.startX - distToMove < this.ulScroll.maxScrollX) {
        distToMove = -(this.ulScroll.maxScrollX - this.ulScroll.startX);
      } else if(this.ulScroll.startX - distToMove > 0){
        distToMove = this.ulScroll.startX;
      }

      if ($(e.target.parentNode).index() < this.prevTargetPos) {
        this.ulScroll.scrollTo(this.ulScroll.startX - distToMove, 0, 500);
      } else {
        this.ulScroll.scrollTo(this.ulScroll.startX - distToMove, 0, 500);
      }
      this.prevTargetPos = $(e.target.parentNode).index();

      var scrollTo =  $('.meet-the-team .content').offset().top;
      scrollTo -= parseInt($('body').css('paddingTop'));
      $('html, body').animate({
        scrollTop: scrollTo
      }, 300);
    },

    windowScrolled: function(e) {
      var principlesOffset = $('.our-principles').offset(),
          windowOffset = $(e.currentTarget).scrollTop() + $(window).height();

      if (windowOffset > principlesOffset.top) {
        this.principlesInView();
      }
    },

    principlesInView: function() {
      var principleEl = $('.our-principles'),
          principlesEl = $('.principles'),
          index = 1,
          amount = principleEl.find('li').length;

      if (!principleEl.hasClass('has-animated')) {

        principleEl.addClass('has-animated');
        principleEl.find('li').eq(0).addClass('active');

        (function timer() {
          setTimeout(function() {
            if (index !== amount && principleEl.hasClass('has-animated') && !principleEl.hasClass('stop-animated')) {
              principleEl.find('li').eq(index - 1).removeClass('active');
              principlesEl.eq(index - 1).removeClass('active');

              principleEl.find('li').eq(index).addClass('active');
              principlesEl.eq(index).addClass('active');

              ++index;
              timer();
            } else if (principleEl.hasClass('stop-animated')) {

            } else {
              principleEl.removeClass('is-animating');
              principleEl.find('li').eq(index - 1).removeClass('active');
              principlesEl.eq(index - 1).removeClass('active');

              principleEl.find('li').eq(0).addClass('active');
              principlesEl.eq(0).addClass('active');
            }
          }, 2000);
        })();
      }
    },

    principleHovered: function(e) {
      var elIndex = $(this).index(),
          principleEl = $('.our-principles'),
          principlesEl = $('.principles'),
          visIndex = principleEl.find('.active').index();

      principleEl.addClass('stop-animated');

      principleEl.find('li').eq(visIndex).removeClass('active');
      principlesEl.eq(visIndex).removeClass('active');

      principleEl.find('li').eq(elIndex).addClass('active');
      principlesEl.eq(elIndex).addClass('active');
    }
  };
})(jQuery, Drupal);
