(function($, Drupal) {
  Drupal.behaviors.grid_expand = function (context) {
    var transitionevents = "transitionend webkitTransitionEnd oTransitionEnd MSTransitionEnd";
    $('.featured_partners > ul, .smart_partners_list > ul', context).each(function (grid) {
      $(".content .inner", grid).each(function(){
        $(this).prepend('<a class="close"></a>');
      });
      $("li>a, li>.content>.inner>.close", grid).live("click", function(){
        var li = $(this).closest("li");

        if(li.hasClass("viewing")) {
          // close self
          li.siblings().andSelf().removeClass("expanded").removeClass("viewing").removeAttr("style");
          pauseVideos();
          return;
        }
        if (li.siblings().hasClass("expanded")) {
          pauseVideos();
          // close others first
          li.siblings(".expanded").bind(transitionevents, function(){
            displayLi(li);
            $(this).unbind(transitionevents);
          }).removeClass("expanded").removeClass("viewing").removeAttr("style");
        } else {
          displayLi(li);
        }
      });
      function pauseVideos() {
        $("iframe", grid).each(function () {
          this.contentWindow.postMessage('{"event":"command","func":"pauseVideo","args":""}', '*')
        });
      }
      function displayLi(li) {

        li.addClass("expanded");
        var calculatedheight = $("a", li).height() + $(".content", li).height() + 25;

        li.css("height", calculatedheight).bind(transitionevents, function(){
          $(this).addClass("viewing")
          $(this).unbind(transitionevents);
        });
      }
    });
  }
})(jQuery, Drupal);
