(function($, Drupal) {
  Drupal.Everline = Drupal.Everline || {};

  Drupal.Everline.navigation = {
    isTablet: navigator.userAgent.match(/(iPad)/),
    init: function() {
      this.addListeners();
    },

    addListeners: function() {
      $('header').on('click', '.apply, .menu-icon', $.proxy(this.iconClicked, this));
      $('.sliders.header-sliders').on('click', '.close', function(e) {
        e.preventDefault();
        $('header').find('.apply').trigger('click');
      })
      $(window).on('resize', this.windowResized);

     $(window).on('scroll', this.windowScroll);
     $(window).on('touchmove', this.windowStartScroll);
      $('.js-getquote').on('click', function(e) {
        e.preventDefault();
        $('.apply').trigger('click');
      });
    },

    iconClicked: function(e) {
      e.preventDefault();

      if ($(e.target).hasClass('apply')) {
        this.sliderState($(e.target));
      }
      else if ($(e.target).hasClass('menu-icon')) {
        this.menuState($(e.target));
      }

    },

    sliderState: function(target) {
      var $overlayImg = $('.sub-header .blog-header .overlay, .sub-header ' +
              '.hero-banner .hero-image, .img-overlay .container img');
      if (target.hasClass('inview')) {
        target.removeClass('inview');
        $('html,body').removeClass('disable-scrolling');

        //Show hero image overlay
        if ($overlayImg.length >= 0) {
          $overlayImg.css('z-index', 11);
        }
      }
      else {
        //Hide hero image overlay
        target.addClass('inview');
        $('html,body').addClass('disable-scrolling');
        $('body').scrollTop(0);
        if (typeof Drupal.WongaSliders !== 'undefined') {
          Drupal.WongaSliders.updateAllViews();
        }
        
        if ($overlayImg.length >= 0) {
          $overlayImg.css('z-index', 0);
        }
      }

      target.toggleClass('active');
    },

    loginState: function(target) {
      if (target.hasClass('inview')) {
        $('.login-form').slideUp(350);
        target.removeClass('inview');
      }
      else {
        $('.login-form').slideDown(350);
        target.addClass('inview');
      }
    },

    menuState: function(target) {
      if (target.hasClass('inview')) {
        $('nav').find('ul.navigation').slideUp(350);
        target.removeClass('inview');
      }
      else {
        $('nav').find('ul.navigation').slideDown(350);
        target.addClass('inview');
      }
    },

    checkOpenContent: function() {
      if ($('header').find('.mobile').find('.inview').length > 0) {
        $('header').find('.mobile').find('.inview').trigger('click');
      }
    },

    windowStartScroll: function (e) {
      $('header:not(.has-scrolled)').addClass('has-scrolled');
    },
    windowScroll: function(e) {
      if (e.currentTarget.pageYOffset > 0) {
        $('header:not(.has-scrolled)').addClass('has-scrolled');
      } else {
        $('header.has-scrolled').removeClass('has-scrolled');
      }
    }
  };

  // Show sliders when getquote hashbang is found.
  Drupal.behaviors.getQuote = function () {
    if(document.URL.indexOf("#getquote") >= 0){
      $('.apply').trigger('click');
    };
  };

})(jQuery, Drupal);
