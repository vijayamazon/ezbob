
/**
 * Override jQuery.fn.init to guard against XSS attacks.
 *
 * See http://bugs.jquery.com/ticket/9521
 */
(function () {
  var jquery_init = jQuery.fn.init;
  jQuery.fn.init = function (selector, context, rootjQuery) {
    // If the string contains a "#" before a "<", treat it as invalid HTML.
    if (selector && typeof selector === 'string') {
      var hash_position = selector.indexOf('#');
      if (hash_position >= 0) {
        var bracket_position = selector.indexOf('<');
        if (bracket_position > hash_position) {
          throw 'Syntax error, unrecognized expression: ' + selector;
        }
      }
    }
    return jquery_init.call(this, selector, context, rootjQuery);
  };
  jQuery.fn.init.prototype = jquery_init.prototype;
})();

var Drupal = Drupal || { 'settings': {}, 'behaviors': {}, 'themes': {}, 'locale': {} };

/**
 * Set the variable that indicates if JavaScript behaviors should be applied
 */
Drupal.jsEnabled = true;

/**
 * Attach all registered behaviors to a page element.
 *
 * Behaviors are event-triggered actions that attach to page elements, enhancing
 * default non-Javascript UIs. Behaviors are registered in the Drupal.behaviors
 * object as follows:
 * @code
 *    Drupal.behaviors.behaviorName = function () {
 *      ...
 *    };
 * @endcode
 *
 * Drupal.attachBehaviors is added below to the jQuery ready event and so
 * runs on initial page load. Developers implementing AHAH/AJAX in their
 * solutions should also call this function after new page content has been
 * loaded, feeding in an element to be processed, in order to attach all
 * behaviors to the new content.
 *
 * Behaviors should use a class in the form behaviorName-processed to ensure
 * the behavior is attached only once to a given element. (Doing so enables
 * the reprocessing of given elements, which may be needed on occasion despite
 * the ability to limit behavior attachment to a particular element.)
 *
 * @param context
 *   An element to attach behaviors to. If none is given, the document element
 *   is used.
 */
Drupal.attachBehaviors = function(context) {
  context = context || document;
  // Execute all of them.
  jQuery.each(Drupal.behaviors, function() {
    this(context);
  });
};

/**
 * Encode special characters in a plain-text string for display as HTML.
 */
Drupal.checkPlain = function(str) {
  str = String(str);
  var replace = { '&': '&amp;', '"': '&quot;', '<': '&lt;', '>': '&gt;' };
  for (var character in replace) {
    var regex = new RegExp(character, 'g');
    str = str.replace(regex, replace[character]);
  }
  return str;
};

/**
 * Translate strings to the page language or a given language.
 *
 * See the documentation of the server-side t() function for further details.
 *
 * @param str
 *   A string containing the English string to translate.
 * @param args
 *   An object of replacements pairs to make after translation. Incidences
 *   of any key in this array are replaced with the corresponding value.
 *   Based on the first character of the key, the value is escaped and/or themed:
 *    - !variable: inserted as is
 *    - @variable: escape plain text to HTML (Drupal.checkPlain)
 *    - %variable: escape text and theme as a placeholder for user-submitted
 *      content (checkPlain + Drupal.theme('placeholder'))
 * @return
 *   The translated string.
 */
Drupal.t = function(str, args) {
  // Fetch the localized version of the string.
  if (Drupal.locale.strings && Drupal.locale.strings[str]) {
    str = Drupal.locale.strings[str];
  }

  if (args) {
    // Transform arguments before inserting them
    for (var key in args) {
      switch (key.charAt(0)) {
        // Escaped only
        case '@':
          args[key] = Drupal.checkPlain(args[key]);
        break;
        // Pass-through
        case '!':
          break;
        // Escaped and placeholder
        case '%':
        default:
          args[key] = Drupal.theme('placeholder', args[key]);
          break;
      }
      str = str.replace(key, args[key]);
    }
  }
  return str;
};

/**
 * Format a string containing a count of items.
 *
 * This function ensures that the string is pluralized correctly. Since Drupal.t() is
 * called by this function, make sure not to pass already-localized strings to it.
 *
 * See the documentation of the server-side format_plural() function for further details.
 *
 * @param count
 *   The item count to display.
 * @param singular
 *   The string for the singular case. Please make sure it is clear this is
 *   singular, to ease translation (e.g. use "1 new comment" instead of "1 new").
 *   Do not use @count in the singular string.
 * @param plural
 *   The string for the plural case. Please make sure it is clear this is plural,
 *   to ease translation. Use @count in place of the item count, as in "@count
 *   new comments".
 * @param args
 *   An object of replacements pairs to make after translation. Incidences
 *   of any key in this array are replaced with the corresponding value.
 *   Based on the first character of the key, the value is escaped and/or themed:
 *    - !variable: inserted as is
 *    - @variable: escape plain text to HTML (Drupal.checkPlain)
 *    - %variable: escape text and theme as a placeholder for user-submitted
 *      content (checkPlain + Drupal.theme('placeholder'))
 *   Note that you do not need to include @count in this array.
 *   This replacement is done automatically for the plural case.
 * @return
 *   A translated string.
 */
Drupal.formatPlural = function(count, singular, plural, args) {
  var args = args || {};
  args['@count'] = count;
  // Determine the index of the plural form.
  var index = Drupal.locale.pluralFormula ? Drupal.locale.pluralFormula(args['@count']) : ((args['@count'] == 1) ? 0 : 1);

  if (index == 0) {
    return Drupal.t(singular, args);
  }
  else if (index == 1) {
    return Drupal.t(plural, args);
  }
  else {
    args['@count['+ index +']'] = args['@count'];
    delete args['@count'];
    return Drupal.t(plural.replace('@count', '@count['+ index +']'), args);
  }
};

/**
 * Generate the themed representation of a Drupal object.
 *
 * All requests for themed output must go through this function. It examines
 * the request and routes it to the appropriate theme function. If the current
 * theme does not provide an override function, the generic theme function is
 * called.
 *
 * For example, to retrieve the HTML that is output by theme_placeholder(text),
 * call Drupal.theme('placeholder', text).
 *
 * @param func
 *   The name of the theme function to call.
 * @param ...
 *   Additional arguments to pass along to the theme function.
 * @return
 *   Any data the theme function returns. This could be a plain HTML string,
 *   but also a complex object.
 */
Drupal.theme = function(func) {
  for (var i = 1, args = []; i < arguments.length; i++) {
    args.push(arguments[i]);
  }

  return (Drupal.theme[func] || Drupal.theme.prototype[func]).apply(this, args);
};

/**
 * Parse a JSON response.
 *
 * The result is either the JSON object, or an object with 'status' 0 and 'data' an error message.
 */
Drupal.parseJson = function (data) {
  if ((data.substring(0, 1) != '{') && (data.substring(0, 1) != '[')) {
    return { status: 0, data: data.length ? data : Drupal.t('Unspecified error') };
  }
  return eval('(' + data + ');');
};

/**
 * Freeze the current body height (as minimum height). Used to prevent
 * unnecessary upwards scrolling when doing DOM manipulations.
 */
Drupal.freezeHeight = function () {
  Drupal.unfreezeHeight();
  var div = document.createElement('div');
  $(div).css({
    position: 'absolute',
    top: '0px',
    left: '0px',
    width: '1px',
    height: $('body').css('height')
  }).attr('id', 'freeze-height');
  $('body').append(div);
};

/**
 * Unfreeze the body height
 */
Drupal.unfreezeHeight = function () {
  $('#freeze-height').remove();
};

/**
 * Wrapper around encodeURIComponent() which avoids Apache quirks (equivalent of
 * drupal_urlencode() in PHP). This function should only be used on paths, not
 * on query string arguments.
 */
Drupal.encodeURIComponent = function (item, uri) {
  uri = uri || location.href;
  item = encodeURIComponent(item).replace(/%2F/g, 'index.html');
  return (uri.indexOf('?q=') != -1) ? item : item.replace(/%26/g, '%2526').replace(/%23/g, '%2523').replace(/\/\//g, '/%252F');
};

/**
 * Get the text selection in a textarea.
 */
Drupal.getSelection = function (element) {
  if (typeof(element.selectionStart) != 'number' && document.selection) {
    // The current selection
    var range1 = document.selection.createRange();
    var range2 = range1.duplicate();
    // Select all text.
    range2.moveToElementText(element);
    // Now move 'dummy' end point to end point of original range.
    range2.setEndPoint('EndToEnd', range1);
    // Now we can calculate start and end points.
    var start = range2.text.length - range1.text.length;
    var end = start + range1.text.length;
    return { 'start': start, 'end': end };
  }
  return { 'start': element.selectionStart, 'end': element.selectionEnd };
};

/**
 * Build an error message from ahah response.
 */
Drupal.ahahError = function(xmlhttp, uri) {
  if (xmlhttp.status == 200) {
    if (jQuery.trim(xmlhttp.responseText)) {
      var message = Drupal.t("An error occurred. \n@uri\n@text", {'@uri': uri, '@text': xmlhttp.responseText });
    }
    else {
      var message = Drupal.t("An error occurred. \n@uri\n(no information available).", {'@uri': uri });
    }
  }
  else {
    var message = Drupal.t("An HTTP error @status occurred. \n@uri", {'@uri': uri, '@status': xmlhttp.status });
  }
  return message.replace(/\n/g, '<br />');
}

// Global Killswitch on the <html> element
$(document.documentElement).addClass('js');
// Attach all behaviors.
$(document).ready(function() {
  Drupal.attachBehaviors(this);
});

/**
 * The default themes.
 */
Drupal.theme.prototype = {

  /**
   * Formats text for emphasized display in a placeholder inside a sentence.
   *
   * @param str
   *   The text to format (plain-text).
   * @return
   *   The formatted text (html).
   */
  placeholder: function(str) {
    return '<em>' + Drupal.checkPlain(str) + '</em>';
  }
};
;
/* Code first created by Zewa,
 * fixed up by AlexisWilke so it works with any type of page
 */
/**
* Reply, Edit & Delete buttons for included comments.
*
* Store where the user was when clicking add comment or reply,
* and bring him or her back there afterwards.
*/
function insert_node_destination(link){
  var attr=jQuery(link).attr('href');
  var p=attr.indexOf('#');
  var anchor='';
  if(p!=-1){
    anchor=attr.substr(p);
    attr=attr.substr(0,p);
  }
  attr+=attr.indexOf('?')==-1?'?':'&';
  url=location.pathname.substring(1);
  jQuery(link).attr('href',attr+'destination='+url+anchor);
}
jQuery(document).ready(function() {
  jQuery("a[href*='comment/reply'],a[href*='comment/edit'],a[href*='comment/delete']").click(function(){insert_node_destination(this);});
});
;
/*!
 * jQuery Tools v1.2.7 - The missing UI library for the Web
 * 
 * overlay/overlay.js
 * scrollable/scrollable.js
 * tabs/tabs.js
 * tooltip/tooltip.js
 * 
 * NO COPYRIGHTS OR LICENSES. DO WHAT YOU LIKE.
 * 
 * http://flowplayer.org/tools/
 * 
 */
(function(a){a.tools=a.tools||{version:"v1.2.7"},a.tools.overlay={addEffect:function(a,b,d){c[a]=[b,d]},conf:{close:null,closeOnClick:!0,closeOnEsc:!0,closeSpeed:"fast",effect:"default",fixed:!a.browser.msie||a.browser.version>6,left:"center",load:!1,mask:null,oneInstance:!0,speed:"normal",target:null,top:"10%"}};var b=[],c={};a.tools.overlay.addEffect("default",function(b,c){var d=this.getConf(),e=a(window);d.fixed||(b.top+=e.scrollTop(),b.left+=e.scrollLeft()),b.position=d.fixed?"fixed":"absolute",this.getOverlay().css(b).fadeIn(d.speed,c)},function(a){this.getOverlay().fadeOut(this.getConf().closeSpeed,a)});function d(d,e){var f=this,g=d.add(f),h=a(window),i,j,k,l=a.tools.expose&&(e.mask||e.expose),m=Math.random().toString().slice(10);l&&(typeof l=="string"&&(l={color:l}),l.closeOnClick=l.closeOnEsc=!1);var n=e.target||d.attr("rel");j=n?a(n):null||d;if(!j.length)throw"Could not find Overlay: "+n;d&&d.index(j)==-1&&d.click(function(a){f.load(a);return a.preventDefault()}),a.extend(f,{load:function(d){if(f.isOpened())return f;var i=c[e.effect];if(!i)throw"Overlay: cannot find effect : \""+e.effect+"\"";e.oneInstance&&a.each(b,function(){this.close(d)}),d=d||a.Event(),d.type="onBeforeLoad",g.trigger(d);if(d.isDefaultPrevented())return f;k=!0,l&&a(j).expose(l);var n=e.top,o=e.left,p=j.outerWidth({margin:!0}),q=j.outerHeight({margin:!0});typeof n=="string"&&(n=n=="center"?Math.max((h.height()-q)/2,0):parseInt(n,10)/100*h.height()),o=="center"&&(o=Math.max((h.width()-p)/2,0)),i[0].call(f,{top:n,left:o},function(){k&&(d.type="onLoad",g.trigger(d))}),l&&e.closeOnClick&&a.mask.getMask().one("click",f.close),e.closeOnClick&&a(document).on("click."+m,function(b){a(b.target).parents(j).length||f.close(b)}),e.closeOnEsc&&a(document).on("keydown."+m,function(a){a.keyCode==27&&f.close(a)});return f},close:function(b){if(!f.isOpened())return f;b=b||a.Event(),b.type="onBeforeClose",g.trigger(b);if(!b.isDefaultPrevented()){k=!1,c[e.effect][1].call(f,function(){b.type="onClose",g.trigger(b)}),a(document).off("click."+m+" keydown."+m),l&&a.mask.close();return f}},getOverlay:function(){return j},getTrigger:function(){return d},getClosers:function(){return i},isOpened:function(){return k},getConf:function(){return e}}),a.each("onBeforeLoad,onStart,onLoad,onBeforeClose,onClose".split(","),function(b,c){a.isFunction(e[c])&&a(f).on(c,e[c]),f[c]=function(b){b&&a(f).on(c,b);return f}}),i=j.find(e.close||".close"),!i.length&&!e.close&&(i=a("<a class=\"close\"></a>"),j.prepend(i)),i.click(function(a){f.close(a)}),e.load&&f.load()}a.fn.overlay=function(c){var e=this.data("overlay");if(e)return e;a.isFunction(c)&&(c={onBeforeLoad:c}),c=a.extend(!0,{},a.tools.overlay.conf,c),this.each(function(){e=new d(a(this),c),b.push(e),a(this).data("overlay",e)});return c.api?e:this}})(jQuery);
(function(a){a.tools=a.tools||{version:"v1.2.7"},a.tools.scrollable={conf:{activeClass:"active",circular:!1,clonedClass:"cloned",disabledClass:"disabled",easing:"swing",initialIndex:0,item:"> *",items:".items",keyboard:!0,mousewheel:!1,next:".next",prev:".prev",size:1,speed:400,vertical:!1,touch:!0,wheelSpeed:0}};function b(a,b){var c=parseInt(a.css(b),10);if(c)return c;var d=a[0].currentStyle;return d&&d.width&&parseInt(d.width,10)}function c(b,c){var d=a(c);return d.length<2?d:b.parent().find(c)}var d;function e(b,e){var f=this,g=b.add(f),h=b.children(),i=0,j=e.vertical;d||(d=f),h.length>1&&(h=a(e.items,b)),e.size>1&&(e.circular=!1),a.extend(f,{getConf:function(){return e},getIndex:function(){return i},getSize:function(){return f.getItems().size()},getNaviButtons:function(){return n.add(o)},getRoot:function(){return b},getItemWrap:function(){return h},getItems:function(){return h.find(e.item).not("."+e.clonedClass)},move:function(a,b){return f.seekTo(i+a,b)},next:function(a){return f.move(e.size,a)},prev:function(a){return f.move(-e.size,a)},begin:function(a){return f.seekTo(0,a)},end:function(a){return f.seekTo(f.getSize()-1,a)},focus:function(){d=f;return f},addItem:function(b){b=a(b),e.circular?(h.children().last().before(b),h.children().first().replaceWith(b.clone().addClass(e.clonedClass))):(h.append(b),o.removeClass("disabled")),g.trigger("onAddItem",[b]);return f},seekTo:function(b,c,k){b.jquery||(b*=1);if(e.circular&&b===0&&i==-1&&c!==0)return f;if(!e.circular&&b<0||b>f.getSize()||b<-1)return f;var l=b;b.jquery?b=f.getItems().index(b):l=f.getItems().eq(b);var m=a.Event("onBeforeSeek");if(!k){g.trigger(m,[b,c]);if(m.isDefaultPrevented()||!l.length)return f}var n=j?{top:-l.position().top}:{left:-l.position().left};i=b,d=f,c===undefined&&(c=e.speed),h.animate(n,c,e.easing,k||function(){g.trigger("onSeek",[b])});return f}}),a.each(["onBeforeSeek","onSeek","onAddItem"],function(b,c){a.isFunction(e[c])&&a(f).on(c,e[c]),f[c]=function(b){b&&a(f).on(c,b);return f}});if(e.circular){var k=f.getItems().slice(-1).clone().prependTo(h),l=f.getItems().eq(1).clone().appendTo(h);k.add(l).addClass(e.clonedClass),f.onBeforeSeek(function(a,b,c){if(!a.isDefaultPrevented()){if(b==-1){f.seekTo(k,c,function(){f.end(0)});return a.preventDefault()}b==f.getSize()&&f.seekTo(l,c,function(){f.begin(0)})}});var m=b.parents().add(b).filter(function(){if(a(this).css("display")==="none")return!0});m.length?(m.show(),f.seekTo(0,0,function(){}),m.hide()):f.seekTo(0,0,function(){})}var n=c(b,e.prev).click(function(a){a.stopPropagation(),f.prev()}),o=c(b,e.next).click(function(a){a.stopPropagation(),f.next()});e.circular||(f.onBeforeSeek(function(a,b){setTimeout(function(){a.isDefaultPrevented()||(n.toggleClass(e.disabledClass,b<=0),o.toggleClass(e.disabledClass,b>=f.getSize()-1))},1)}),e.initialIndex||n.addClass(e.disabledClass)),f.getSize()<2&&n.add(o).addClass(e.disabledClass),e.mousewheel&&a.fn.mousewheel&&b.mousewheel(function(a,b){if(e.mousewheel){f.move(b<0?1:-1,e.wheelSpeed||50);return!1}});if(e.touch){var p={};h[0].ontouchstart=function(a){var b=a.touches[0];p.x=b.clientX,p.y=b.clientY},h[0].ontouchmove=function(a){if(a.touches.length==1&&!h.is(":animated")){var b=a.touches[0],c=p.x-b.clientX,d=p.y-b.clientY;f[j&&d>0||!j&&c>0?"next":"prev"](),a.preventDefault()}}}e.keyboard&&a(document).on("keydown.scrollable",function(b){if(!(!e.keyboard||b.altKey||b.ctrlKey||b.metaKey||a(b.target).is(":input"))){if(e.keyboard!="static"&&d!=f)return;var c=b.keyCode;if(j&&(c==38||c==40)){f.move(c==38?-1:1);return b.preventDefault()}if(!j&&(c==37||c==39)){f.move(c==37?-1:1);return b.preventDefault()}}}),e.initialIndex&&f.seekTo(e.initialIndex,0,function(){})}a.fn.scrollable=function(b){var c=this.data("scrollable");if(c)return c;b=a.extend({},a.tools.scrollable.conf,b),this.each(function(){c=new e(a(this),b),a(this).data("scrollable",c)});return b.api?c:this}})(jQuery);
(function(a){a.tools=a.tools||{version:"v1.2.7"},a.tools.tabs={conf:{tabs:"a",current:"current",onBeforeClick:null,onClick:null,effect:"default",initialEffect:!1,initialIndex:0,event:"click",rotate:!1,slideUpSpeed:400,slideDownSpeed:400,history:!1},addEffect:function(a,c){b[a]=c}};var b={"default":function(a,b){this.getPanes().hide().eq(a).show(),b.call()},fade:function(a,b){var c=this.getConf(),d=c.fadeOutSpeed,e=this.getPanes();d?e.fadeOut(d):e.hide(),e.eq(a).fadeIn(c.fadeInSpeed,b)},slide:function(a,b){var c=this.getConf();this.getPanes().slideUp(c.slideUpSpeed),this.getPanes().eq(a).slideDown(c.slideDownSpeed,b)},ajax:function(a,b){this.getPanes().eq(0).load(this.getTabs().eq(a).attr("href"),b)}},c,d;a.tools.tabs.addEffect("horizontal",function(b,e){if(!c){var f=this.getPanes().eq(b),g=this.getCurrentPane();d||(d=this.getPanes().eq(0).width()),c=!0,f.show(),g.animate({width:0},{step:function(a){f.css("width",d-a)},complete:function(){a(this).hide(),e.call(),c=!1}}),g.length||(e.call(),c=!1)}});function e(c,d,e){var f=this,g=c.add(this),h=c.find(e.tabs),i=d.jquery?d:c.children(d),j;h.length||(h=c.children()),i.length||(i=c.parent().find(d)),i.length||(i=a(d)),a.extend(this,{click:function(d,i){var k=h.eq(d),l=!c.data("tabs");typeof d=="string"&&d.replace("#","")&&(k=h.filter("[href*=\""+d.replace("#","")+"\"]"),d=Math.max(h.index(k),0));if(e.rotate){var m=h.length-1;if(d<0)return f.click(m,i);if(d>m)return f.click(0,i)}if(!k.length){if(j>=0)return f;d=e.initialIndex,k=h.eq(d)}if(d===j)return f;i=i||a.Event(),i.type="onBeforeClick",g.trigger(i,[d]);if(!i.isDefaultPrevented()){var n=l?e.initialEffect&&e.effect||"default":e.effect;b[n].call(f,d,function(){j=d,i.type="onClick",g.trigger(i,[d])}),h.removeClass(e.current),k.addClass(e.current);return f}},getConf:function(){return e},getTabs:function(){return h},getPanes:function(){return i},getCurrentPane:function(){return i.eq(j)},getCurrentTab:function(){return h.eq(j)},getIndex:function(){return j},next:function(){return f.click(j+1)},prev:function(){return f.click(j-1)},destroy:function(){h.off(e.event).removeClass(e.current),i.find("a[href^=\"#\"]").off("click.html");return f}}),a.each("onBeforeClick,onClick".split(","),function(b,c){a.isFunction(e[c])&&a(f).on(c,e[c]),f[c]=function(b){b&&a(f).on(c,b);return f}}),e.history&&a.fn.history&&(a.tools.history.init(h),e.event="history"),h.each(function(b){a(this).on(e.event,function(a){f.click(b,a);return a.preventDefault()})}),i.find("a[href^=\"#\"]").on("click.html",function(b){f.click(a(this).attr("href"),b)}),location.hash&&e.tabs=="a"&&c.find("[href=\""+location.hash+"\"]").length?f.click(location.hash):(e.initialIndex===0||e.initialIndex>0)&&f.click(e.initialIndex)}a.fn.tabs=function(b,c){var d=this.data("tabs");d&&(d.destroy(),this.removeData("tabs")),a.isFunction(c)&&(c={onBeforeClick:c}),c=a.extend({},a.tools.tabs.conf,c),this.each(function(){d=new e(a(this),b,c),a(this).data("tabs",d)});return c.api?d:this}})(jQuery);
(function(a){a.tools=a.tools||{version:"v1.2.7"},a.tools.tooltip={conf:{effect:"toggle",fadeOutSpeed:"fast",predelay:0,delay:30,opacity:1,tip:0,fadeIE:!1,position:["top","center"],offset:[0,0],relative:!1,cancelDefault:!0,events:{def:"mouseenter,mouseleave",input:"focus,blur",widget:"focus mouseenter,blur mouseleave",tooltip:"mouseenter,mouseleave"},layout:"<div/>",tipClass:"tooltip"},addEffect:function(a,c,d){b[a]=[c,d]}};var b={toggle:[function(a){var b=this.getConf(),c=this.getTip(),d=b.opacity;d<1&&c.css({opacity:d}),c.show(),a.call()},function(a){this.getTip().hide(),a.call()}],fade:[function(b){var c=this.getConf();!a.browser.msie||c.fadeIE?this.getTip().fadeTo(c.fadeInSpeed,c.opacity,b):(this.getTip().show(),b())},function(b){var c=this.getConf();!a.browser.msie||c.fadeIE?this.getTip().fadeOut(c.fadeOutSpeed,b):(this.getTip().hide(),b())}]};function c(b,c,d){var e=d.relative?b.position().top:b.offset().top,f=d.relative?b.position().left:b.offset().left,g=d.position[0];e-=c.outerHeight()-d.offset[0],f+=b.outerWidth()+d.offset[1],/iPad/i.test(navigator.userAgent)&&(e-=a(window).scrollTop());var h=c.outerHeight()+b.outerHeight();g=="center"&&(e+=h/2),g=="bottom"&&(e+=h),g=d.position[1];var i=c.outerWidth()+b.outerWidth();g=="center"&&(f-=i/2),g=="left"&&(f-=i);return{top:e,left:f}}function d(d,e){var f=this,g=d.add(f),h,i=0,j=0,k=d.attr("title"),l=d.attr("data-tooltip"),m=b[e.effect],n,o=d.is(":input"),p=o&&d.is(":checkbox, :radio, select, :button, :submit"),q=d.attr("type"),r=e.events[q]||e.events[o?p?"widget":"input":"def"];if(!m)throw"Nonexistent effect \""+e.effect+"\"";r=r.split(/,\s*/);if(r.length!=2)throw"Tooltip: bad events configuration for "+q;d.on(r[0],function(a){clearTimeout(i),e.predelay?j=setTimeout(function(){f.show(a)},e.predelay):f.show(a)}).on(r[1],function(a){clearTimeout(j),e.delay?i=setTimeout(function(){f.hide(a)},e.delay):f.hide(a)}),k&&e.cancelDefault&&(d.removeAttr("title"),d.data("title",k)),a.extend(f,{show:function(b){if(!h){l?h=a(l):e.tip?h=a(e.tip).eq(0):k?h=a(e.layout).addClass(e.tipClass).appendTo(document.body).hide().append(k):(h=d.next(),h.length||(h=d.parent().next()));if(!h.length)throw"Cannot find tooltip for "+d}if(f.isShown())return f;h.stop(!0,!0);var o=c(d,h,e);e.tip&&h.html(d.data("title")),b=a.Event(),b.type="onBeforeShow",g.trigger(b,[o]);if(b.isDefaultPrevented())return f;o=c(d,h,e),h.css({position:"absolute",top:o.top,left:o.left}),n=!0,m[0].call(f,function(){b.type="onShow",n="full",g.trigger(b)});var p=e.events.tooltip.split(/,\s*/);h.data("__set")||(h.off(p[0]).on(p[0],function(){clearTimeout(i),clearTimeout(j)}),p[1]&&!d.is("input:not(:checkbox, :radio), textarea")&&h.off(p[1]).on(p[1],function(a){a.relatedTarget!=d[0]&&d.trigger(r[1].split(" ")[0])}),e.tip||h.data("__set",!0));return f},hide:function(c){if(!h||!f.isShown())return f;c=a.Event(),c.type="onBeforeHide",g.trigger(c);if(!c.isDefaultPrevented()){n=!1,b[e.effect][1].call(f,function(){c.type="onHide",g.trigger(c)});return f}},isShown:function(a){return a?n=="full":n},getConf:function(){return e},getTip:function(){return h},getTrigger:function(){return d}}),a.each("onHide,onBeforeShow,onShow,onBeforeHide".split(","),function(b,c){a.isFunction(e[c])&&a(f).on(c,e[c]),f[c]=function(b){b&&a(f).on(c,b);return f}})}a.fn.tooltip=function(b){var c=this.data("tooltip");if(c)return c;b=a.extend(!0,{},a.tools.tooltip.conf,b),typeof b.position=="string"&&(b.position=b.position.split(/,?\s/)),this.each(function(){c=new d(a(this),b),a(this).data("tooltip",c)});return b.api?c:this}})(jQuery);
;
// $Id: captcha.js,v 1.2.2.3 2011/02/06 20:45:12 soxofaan Exp $

// Javascript behaviors for general CAPTCHA functionality.
Drupal.behaviors.captcha = function (context) {

  // Turn off autocompletion for the CAPTCHA response field.
  // We do it here with Javascript (instead of directly in the markup)
  // because this autocomplete attribute is not standard and
  // it would break (X)HTML compliance.
  $("#edit-captcha-response").attr("autocomplete", "off");

};


// JavaScript behaviors for the CAPTCHA admin page
Drupal.behaviors.captchaAdmin = function (context) {

	// Add onclick handler to checkbox for adding a CAPTCHA description
	// so that the textfields for the CAPTCHA description are hidden
	// when no description should be added.
	$("#edit-captcha-add-captcha-description").click(function() {
		if ($("#edit-captcha-add-captcha-description").is(":checked")) {
			// Show the CAPTCHA description textfield(s).
			$("#edit-captcha-description-wrapper").show("slow");
		}
		else {
			// Hide the CAPTCHA description textfield(s).
			$("#edit-captcha-description-wrapper").hide("slow");
		}
	});
	// Hide the CAPTCHA description textfields if option is disabled on page load.
	if (!$("#edit-captcha-add-captcha-description").is(":checked")) {
		$("#edit-captcha-description-wrapper").hide();
	}

};
;
(function() {
  /*jshint evil:true */
  var link = '';
  var deployment_id = '572D000000000c4';
  var org_id = '00D20000000Mrf2';
  var button_name = 'Everline';
  var buttonId = '573D000000000hA';
  var chat_id = 'livechat-form';
  var chat_running = true;
  var refresh_timeout = 4000;
  var button_removed = false;
  var xmlhttp;

  $(document).ready(function () {
	  $('#livechat-form').hide();
	  $("#livechat-media-wrapper").hide();
	  $("#kampylink").hide();
	  $('#k_slogan').hide();
	  $('#k_close_button').hide();
	  console.log('load 1');
	  chat_running = false;
	  
	  if(window.location.hostname == 'localhost'){
		  $('a.apply-now-btn').attr('href', 'https://test.everline.com:44300/Customer/Wizard');
		  $('a.login').attr('href', 'https://test.everline.com:44300/Account/Logon');
	  }
  });


  if (window.XMLHttpRequest) {
    xmlhttp = new XMLHttpRequest();
  } else {
    xmlhttp = new ActiveXObject('Microsoft.XMLHTTP');
  }

  xmlhttp.onreadystatechange = function () {
    if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
      eval(xmlhttp.responseText);
    }
  };

  function sendGaEvent(event_name, label_name) {
    if (typeof ga != 'undefined') {
      ga('send', 'event', {
        'eventCategory': 'Live Agent',
        'eventAction': event_name,
        'eventLabel': label_name
      });
    }
  }

  function hideButton() {
    if (!button_removed) {
      document.getElementById(chat_id).style.display = 'none';
    }
  }

  function removeButton() {
    $('.livechat-wrapper').remove();
    button_removed = true;
  }


  function handleChatButtonEvent(e) {
    if (e == liveagent.BUTTON_EVENT.BUTTON_AVAILABLE) {
      sendGaEvent('Available', button_name);
    }
    if (e == liveagent.BUTTON_EVENT.BUTTON_UNAVAILABLE) {
      sendGaEvent('Unavailable', button_name);
    }
  }

  function checkresult(response) {
    var available = false;
    for (var i = 0; i < response.message.results.length; i++) {
      var r = response.message.results[i];
      if (typeof r != 'undefined') {
        if ((typeof r.isAvailable != 'undefined') && (r.isAvailable === true)) {
          available = true;
        }
      }
    }
    if (available) {
      showButton();
    } else {
      hideButton();
    }
  }

  function showButton() {
    if (!button_removed) {
      if (document.getElementById(chat_id).style.display == 'none') {
        document.getElementById(chat_id).style.display = 'block';
      }
    }
  }

})();;
/**
 * @license 
 * jQuery Tools 1.2.5 Scrollable - New wave UI design
 * 
 * NO COPYRIGHTS OR LICENSES. DO WHAT YOU LIKE.
 * 
 * http://flowplayer.org/tools/scrollable.html
 *
 * Since: March 2008
 * Date:    Wed Sep 22 06:02:10 2010 +0000 
 */
(function($) { 

	// static constructs
	$.tools = $.tools || {version: '1.2.5'};
	
	$.tools.scrollable = {
		
		conf: {	
			activeClass: 'active',
			circular: false,
			clonedClass: 'cloned',
			disabledClass: 'disabled',
			easing: 'swing',
			initialIndex: 0,
			item: null,
			items: '.items',
			keyboard: true,
			mousewheel: true,
			next: '.next',   
			prev: '.prev', 
			speed: 400,
			vertical: false,
			touch: true,
			wheelSpeed: 0
		} 
	};
					
	// get hidden element's width or height even though it's hidden
	function dim(el, key) {
		var v = parseInt(el.css(key), 10);
		if (v) { return v; }
		var s = el[0].currentStyle; 
		return s && s.width && parseInt(s.width, 10);	
	}

	function find(root, query) { 
		var el = $(query);
		return el.length < 2 ? el : root.parent().find(query);
	}
	
	var current;		
	
	// constructor
	function Scrollable(root, conf) {   
		
		// current instance
		var self = this, 
			 fire = root.add(self),
			 itemWrap = root.children(),
			 index = 0,
			 vertical = conf.vertical;
				
		if (!current) { current = self; } 
		if (itemWrap.length > 1) { itemWrap = $(conf.items, root); }
		
		// methods
		$.extend(self, {
				
			getConf: function() {
				return conf;	
			},			
			
			getIndex: function() {
				return index;	
			}, 

			getSize: function() {
				return self.getItems().size();	
			},

			getNaviButtons: function() {
				return prev.add(next);	
			},
			
			getRoot: function() {
				return root;	
			},
			
			getItemWrap: function() {
				return itemWrap;	
			},
			
			getItems: function() {
				return itemWrap.children(conf.item).not("." + conf.clonedClass);	
			},
							
			move: function(offset, time) {
				return self.seekTo(index + offset, time);
			},
			
			next: function(time) {
				return self.move(1, time);	
			},
			
			prev: function(time) {
				return self.move(-1, time);	
			},
			
			begin: function(time) {
				return self.seekTo(0, time);	
			},
			
			end: function(time) {
				return self.seekTo(self.getSize() -1, time);	
			},	
			
			focus: function() {
				current = self;
				return self;
			},
			
			addItem: function(item) {
				item = $(item);
				
				if (!conf.circular)  {
					itemWrap.append(item);
				} else {
					itemWrap.children("." + conf.clonedClass + ":last").before(item);
					itemWrap.children("." + conf.clonedClass + ":first").replaceWith(item.clone().addClass(conf.clonedClass)); 						
				}
				
				fire.trigger("onAddItem", [item]);
				return self;
			},
			
			
			/* all seeking functions depend on this */		
			seekTo: function(i, time, fn) {	
				
				// ensure numeric index
				if (!i.jquery) { i *= 1; }
				
				// avoid seeking from end clone to the beginning
				if (conf.circular && i === 0 && index == -1 && time !== 0) { return self; }
				
				// check that index is sane				
				if (!conf.circular && i < 0 || i > self.getSize() || i < -1) { return self; }
				
				var item = i;
			
				if (i.jquery) {
					i = self.getItems().index(i);	
					
				} else {
					item = self.getItems().eq(i);
				}  
				
				// onBeforeSeek
				var e = $.Event("onBeforeSeek"); 
				if (!fn) {
					fire.trigger(e, [i, time]);				
					if (e.isDefaultPrevented() || !item.length) { return self; }			
				}  
	
				var props = vertical ? {top: -item.position().top} : {left: -item.position().left};  
				
				index = i;
				current = self;  
				if (time === undefined) { time = conf.speed; }   
				
				itemWrap.animate(props, time, conf.easing, fn || function() { 
					fire.trigger("onSeek", [i]);		
				});	 
				
				return self; 
			}					
			
		});
				
		// callbacks	
		$.each(['onBeforeSeek', 'onSeek', 'onAddItem'], function(i, name) {
				
			// configuration
			if ($.isFunction(conf[name])) { 
				$(self).bind(name, conf[name]); 
			}
			
			self[name] = function(fn) {
				if (fn) { $(self).bind(name, fn); }
				return self;
			};
		});  
		
		// circular loop
		if (conf.circular) {
			
			var cloned1 = self.getItems().slice(-1).clone().prependTo(itemWrap),
				 cloned2 = self.getItems().eq(1).clone().appendTo(itemWrap);
				
			cloned1.add(cloned2).addClass(conf.clonedClass);
			
			self.onBeforeSeek(function(e, i, time) {

				
				if (e.isDefaultPrevented()) { return; }
				
				/*
					1. animate to the clone without event triggering
					2. seek to correct position with 0 speed
				*/
				if (i == -1) {
					self.seekTo(cloned1, time, function()  {
						self.end(0);		
					});          
					return e.preventDefault();
					
				} else if (i == self.getSize()) {
					self.seekTo(cloned2, time, function()  {
						self.begin(0);		
					});	
				}
				
			});
			
			// seek over the cloned item
			self.seekTo(0, 0, function() {});
		}
		
		// next/prev buttons
		var prev = find(root, conf.prev).click(function() { self.prev(); }),
			 next = find(root, conf.next).click(function() { self.next(); });	
		
		if (!conf.circular && self.getSize() > 1) {
			
			self.onBeforeSeek(function(e, i) {
				setTimeout(function() {
					if (!e.isDefaultPrevented()) {
						prev.toggleClass(conf.disabledClass, i <= 0);
						next.toggleClass(conf.disabledClass, i >= self.getSize() -1);
					}
				}, 1);
			}); 
			
			if (!conf.initialIndex) {
				prev.addClass(conf.disabledClass);	
			}
		}
			
		// mousewheel support
		if (conf.mousewheel && $.fn.mousewheel) {
			root.mousewheel(function(e, delta)  {
				if (conf.mousewheel) {
					self.move(delta < 0 ? 1 : -1, conf.wheelSpeed || 50);
					return false;
				}
			});			
		}
		
		// touch event
		if (conf.touch) {
			var touch = {};
			
			itemWrap[0].ontouchstart = function(e) {
				var t = e.touches[0];
				touch.x = t.clientX;
				touch.y = t.clientY;
			};
			
			itemWrap[0].ontouchmove = function(e) {
				
				// only deal with one finger
				if (e.touches.length == 1 && !itemWrap.is(":animated")) {			
					var t = e.touches[0],
						 deltaX = touch.x - t.clientX,
						 deltaY = touch.y - t.clientY;
	
					self[vertical && deltaY > 0 || !vertical && deltaX > 0 ? 'next' : 'prev']();				
					e.preventDefault();
				}
			};
		}
		
		if (conf.keyboard)  {
			
			$(document).bind("keydown.scrollable", function(evt) {

				// skip certain conditions
				if (!conf.keyboard || evt.altKey || evt.ctrlKey || $(evt.target).is(":input")) { return; }
				
				// does this instance have focus?
				if (conf.keyboard != 'static' && current != self) { return; }
					
				var key = evt.keyCode;
			
				if (vertical && (key == 38 || key == 40)) {
					self.move(key == 38 ? -1 : 1);
					return evt.preventDefault();
				}
				
				if (!vertical && (key == 37 || key == 39)) {					
					self.move(key == 37 ? -1 : 1);
					return evt.preventDefault();
				}	  
				
			});  
		}
		
		// initial index
		if (conf.initialIndex) {
			self.seekTo(conf.initialIndex, 0, function() {});
		}
	} 

		
	// jQuery plugin implementation
	$.fn.scrollable = function(conf) { 
			
		// already constructed --> return API
		var el = this.data("scrollable");
		if (el) { return el; }		 

		conf = $.extend({}, $.tools.scrollable.conf, conf); 
		
		this.each(function() {			
			el = new Scrollable($(this), conf);
			$(this).data("scrollable", el);	
		});
		
		return conf.api ? el: this; 
		
	};
			
	
})(jQuery);
;
/*!
 * jQuery Tools v1.2.7 - The missing UI library for the Web
 *
 * tabs/tabs.js
 *
 * NO COPYRIGHTS OR LICENSES. DO WHAT YOU LIKE.
 *
 * http://flowplayer.org/tools/
 *
 */
(function(a){a.tools=a.tools||{version:"v1.2.7"},a.tools.tabs={conf:{tabs:"a",current:"current",onBeforeClick:null,onClick:null,effect:"default",initialEffect:!1,initialIndex:0,event:"click",rotate:!1,slideUpSpeed:400,slideDownSpeed:400,history:!1},addEffect:function(a,c){b[a]=c}};var b={"default":function(a,b){this.getPanes().hide().eq(a).show(),b.call()},fade:function(a,b){var c=this.getConf(),d=c.fadeOutSpeed,e=this.getPanes();d?e.fadeOut(d):e.hide(),e.eq(a).fadeIn(c.fadeInSpeed,b)},slide:function(a,b){var c=this.getConf();this.getPanes().slideUp(c.slideUpSpeed),this.getPanes().eq(a).slideDown(c.slideDownSpeed,b)},ajax:function(a,b){this.getPanes().eq(0).load(this.getTabs().eq(a).attr("href"),b)}},c,d;a.tools.tabs.addEffect("horizontal",function(b,e){if(!c){var f=this.getPanes().eq(b),g=this.getCurrentPane();d||(d=this.getPanes().eq(0).width()),c=!0,f.show(),g.animate({width:0},{step:function(a){f.css("width",d-a)},complete:function(){a(this).hide(),e.call(),c=!1}}),g.length||(e.call(),c=!1)}});function e(c,d,e){var f=this,g=c.add(this),h=c.find(e.tabs),i=d.jquery?d:c.children(d),j;h.length||(h=c.children()),i.length||(i=c.parent().find(d)),i.length||(i=a(d)),a.extend(this,{click:function(d,i){var k=h.eq(d),l=!c.data("tabs");typeof d=="string"&&d.replace("#","")&&(k=h.filter("[href*=\""+d.replace("#","")+"\"]"),d=Math.max(h.index(k),0));if(e.rotate){var m=h.length-1;if(d<0)return f.click(m,i);if(d>m)return f.click(0,i)}if(!k.length){if(j>=0)return f;d=e.initialIndex,k=h.eq(d)}if(d===j)return f;i=i||a.Event(),i.type="onBeforeClick",g.trigger(i,[d]);if(!i.isDefaultPrevented()){var n=l?e.initialEffect&&e.effect||"default":e.effect;b[n].call(f,d,function(){j=d,i.type="onClick",g.trigger(i,[d])}),h.removeClass(e.current),k.addClass(e.current);return f}},getConf:function(){return e},getTabs:function(){return h},getPanes:function(){return i},getCurrentPane:function(){return i.eq(j)},getCurrentTab:function(){return h.eq(j)},getIndex:function(){return j},next:function(){return f.click(j+1)},prev:function(){return f.click(j-1)},destroy:function(){h.off(e.event).removeClass(e.current),i.find("a[href^=\"#\"]").off("click.html");return f}}),a.each("onBeforeClick,onClick".split(","),function(b,c){a.isFunction(e[c])&&a(f).on(c,e[c]),f[c]=function(b){b&&a(f).on(c,b);return f}}),e.history&&a.fn.history&&(a.tools.history.init(h),e.event="history"),h.each(function(b){a(this).on(e.event,function(a){f.click(b,a);return a.preventDefault()})}),i.find("a[href^=\"#\"]").on("click.html",function(b){f.click(a(this).attr("href"),b)}),location.hash&&e.tabs=="a"&&c.find("[href=\""+location.hash+"\"]").length?f.click(location.hash):(e.initialIndex===0||e.initialIndex>0)&&f.click(e.initialIndex)}a.fn.tabs=function(b,c){var d=this.data("tabs");d&&(d.destroy(),this.removeData("tabs")),a.isFunction(c)&&(c={onBeforeClick:c}),c=a.extend({},a.tools.tabs.conf,c),this.each(function(){d=new e(a(this),b,c),a(this).data("tabs",d)});return c.api?d:this}})(jQuery);
;
/*
 * jQuery Easing v1.3 - http://gsgd.co.uk/sandbox/jquery/easing/
 *
 * Uses the built in easing capabilities added In jQuery 1.1
 * to offer multiple easing options
 *
 * TERMS OF USE - jQuery Easing
 * 
 * Open source under the BSD License. 
 * 
 * Copyright © 2008 George McGinley Smith
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of 
 * conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list 
 * of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * Neither the name of the author nor the names of contributors may be used to endorse 
 * or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 *  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 *  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 *  GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED 
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
 * OF THE POSSIBILITY OF SUCH DAMAGE. 
 *
*/

// t: current time, b: begInnIng value, c: change In value, d: duration
eval(function(p,a,c,k,e,r){e=function(c){return(c<a?'':e(parseInt(c/a)))+((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))};if(!''.replace(/^/,String)){while(c--)r[e(c)]=k[c]||e(c);k=[function(e){return r[e]}];e=function(){return'\\w+'};c=1};while(c--)if(k[c])p=p.replace(new RegExp('\\b'+e(c)+'\\b','g'),k[c]);return p}('h.i[\'1a\']=h.i[\'z\'];h.O(h.i,{y:\'D\',z:9(x,t,b,c,d){6 h.i[h.i.y](x,t,b,c,d)},17:9(x,t,b,c,d){6 c*(t/=d)*t+b},D:9(x,t,b,c,d){6-c*(t/=d)*(t-2)+b},13:9(x,t,b,c,d){e((t/=d/2)<1)6 c/2*t*t+b;6-c/2*((--t)*(t-2)-1)+b},X:9(x,t,b,c,d){6 c*(t/=d)*t*t+b},U:9(x,t,b,c,d){6 c*((t=t/d-1)*t*t+1)+b},R:9(x,t,b,c,d){e((t/=d/2)<1)6 c/2*t*t*t+b;6 c/2*((t-=2)*t*t+2)+b},N:9(x,t,b,c,d){6 c*(t/=d)*t*t*t+b},M:9(x,t,b,c,d){6-c*((t=t/d-1)*t*t*t-1)+b},L:9(x,t,b,c,d){e((t/=d/2)<1)6 c/2*t*t*t*t+b;6-c/2*((t-=2)*t*t*t-2)+b},K:9(x,t,b,c,d){6 c*(t/=d)*t*t*t*t+b},J:9(x,t,b,c,d){6 c*((t=t/d-1)*t*t*t*t+1)+b},I:9(x,t,b,c,d){e((t/=d/2)<1)6 c/2*t*t*t*t*t+b;6 c/2*((t-=2)*t*t*t*t+2)+b},G:9(x,t,b,c,d){6-c*8.C(t/d*(8.g/2))+c+b},15:9(x,t,b,c,d){6 c*8.n(t/d*(8.g/2))+b},12:9(x,t,b,c,d){6-c/2*(8.C(8.g*t/d)-1)+b},Z:9(x,t,b,c,d){6(t==0)?b:c*8.j(2,10*(t/d-1))+b},Y:9(x,t,b,c,d){6(t==d)?b+c:c*(-8.j(2,-10*t/d)+1)+b},W:9(x,t,b,c,d){e(t==0)6 b;e(t==d)6 b+c;e((t/=d/2)<1)6 c/2*8.j(2,10*(t-1))+b;6 c/2*(-8.j(2,-10*--t)+2)+b},V:9(x,t,b,c,d){6-c*(8.o(1-(t/=d)*t)-1)+b},S:9(x,t,b,c,d){6 c*8.o(1-(t=t/d-1)*t)+b},Q:9(x,t,b,c,d){e((t/=d/2)<1)6-c/2*(8.o(1-t*t)-1)+b;6 c/2*(8.o(1-(t-=2)*t)+1)+b},P:9(x,t,b,c,d){f s=1.l;f p=0;f a=c;e(t==0)6 b;e((t/=d)==1)6 b+c;e(!p)p=d*.3;e(a<8.w(c)){a=c;f s=p/4}m f s=p/(2*8.g)*8.r(c/a);6-(a*8.j(2,10*(t-=1))*8.n((t*d-s)*(2*8.g)/p))+b},H:9(x,t,b,c,d){f s=1.l;f p=0;f a=c;e(t==0)6 b;e((t/=d)==1)6 b+c;e(!p)p=d*.3;e(a<8.w(c)){a=c;f s=p/4}m f s=p/(2*8.g)*8.r(c/a);6 a*8.j(2,-10*t)*8.n((t*d-s)*(2*8.g)/p)+c+b},T:9(x,t,b,c,d){f s=1.l;f p=0;f a=c;e(t==0)6 b;e((t/=d/2)==2)6 b+c;e(!p)p=d*(.3*1.5);e(a<8.w(c)){a=c;f s=p/4}m f s=p/(2*8.g)*8.r(c/a);e(t<1)6-.5*(a*8.j(2,10*(t-=1))*8.n((t*d-s)*(2*8.g)/p))+b;6 a*8.j(2,-10*(t-=1))*8.n((t*d-s)*(2*8.g)/p)*.5+c+b},F:9(x,t,b,c,d,s){e(s==u)s=1.l;6 c*(t/=d)*t*((s+1)*t-s)+b},E:9(x,t,b,c,d,s){e(s==u)s=1.l;6 c*((t=t/d-1)*t*((s+1)*t+s)+1)+b},16:9(x,t,b,c,d,s){e(s==u)s=1.l;e((t/=d/2)<1)6 c/2*(t*t*(((s*=(1.B))+1)*t-s))+b;6 c/2*((t-=2)*t*(((s*=(1.B))+1)*t+s)+2)+b},A:9(x,t,b,c,d){6 c-h.i.v(x,d-t,0,c,d)+b},v:9(x,t,b,c,d){e((t/=d)<(1/2.k)){6 c*(7.q*t*t)+b}m e(t<(2/2.k)){6 c*(7.q*(t-=(1.5/2.k))*t+.k)+b}m e(t<(2.5/2.k)){6 c*(7.q*(t-=(2.14/2.k))*t+.11)+b}m{6 c*(7.q*(t-=(2.18/2.k))*t+.19)+b}},1b:9(x,t,b,c,d){e(t<d/2)6 h.i.A(x,t*2,0,c,d)*.5+b;6 h.i.v(x,t*2-d,0,c,d)*.5+c*.5+b}});',62,74,'||||||return||Math|function|||||if|var|PI|jQuery|easing|pow|75|70158|else|sin|sqrt||5625|asin|||undefined|easeOutBounce|abs||def|swing|easeInBounce|525|cos|easeOutQuad|easeOutBack|easeInBack|easeInSine|easeOutElastic|easeInOutQuint|easeOutQuint|easeInQuint|easeInOutQuart|easeOutQuart|easeInQuart|extend|easeInElastic|easeInOutCirc|easeInOutCubic|easeOutCirc|easeInOutElastic|easeOutCubic|easeInCirc|easeInOutExpo|easeInCubic|easeOutExpo|easeInExpo||9375|easeInOutSine|easeInOutQuad|25|easeOutSine|easeInOutBack|easeInQuad|625|984375|jswing|easeInOutBounce'.split('|'),0,{}))

/*
 *
 * TERMS OF USE - EASING EQUATIONS
 * 
 * Open source under the BSD License. 
 * 
 * Copyright © 2001 Robert Penner
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of 
 * conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list 
 * of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * Neither the name of the author nor the names of contributors may be used to endorse 
 * or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 *  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 *  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 *  GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED 
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
 * OF THE POSSIBILITY OF SUCH DAMAGE. 
 *
 */
;
/*
 * FancyBox - jQuery Plugin
 * Simple and fancy lightbox alternative
 *
 * Examples and documentation at: http://fancybox.net
 *
 * Copyright (c) 2008 - 2010 Janis Skarnelis
 * That said, it is hardly a one-person project. Many people have submitted bugs, code, and offered their advice freely. Their support is greatly appreciated.
 *
 * Version: 1.3.4 (11/11/2010)
 * Requires: jQuery v1.3+
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

;(function($) {
	var tmp, loading, overlay, wrap, outer, content, close, title, nav_left, nav_right,

		selectedIndex = 0, selectedOpts = {}, selectedArray = [], currentIndex = 0, currentOpts = {}, currentArray = [],

		ajaxLoader = null, imgPreloader = new Image(), imgRegExp = /\.(jpg|gif|png|bmp|jpeg)(.*)?$/i, swfRegExp = /[^\.]\.(swf)\s*$/i,

		loadingTimer, loadingFrame = 1,

		titleHeight = 0, titleStr = '', start_pos, final_pos, busy = false, fx = $.extend($('<div/>')[0], { prop: 0 }),

		isIE6 = $.browser.msie && $.browser.version < 7 && !window.XMLHttpRequest,

		/*
		 * Private methods 
		 */

		_abort = function() {
			loading.hide();

			imgPreloader.onerror = imgPreloader.onload = null;

			if (ajaxLoader) {
				ajaxLoader.abort();
			}

			tmp.empty();
		},

		_error = function() {
			if (false === selectedOpts.onError(selectedArray, selectedIndex, selectedOpts)) {
				loading.hide();
				busy = false;
				return;
			}

			selectedOpts.titleShow = false;

			selectedOpts.width = 'auto';
			selectedOpts.height = 'auto';

			tmp.html( '<p id="fancybox-error">The requested content cannot be loaded.<br />Please try again later.</p>' );

			_process_inline();
		},

		_start = function() {
			var obj = selectedArray[ selectedIndex ],
				href, 
				type, 
				title,
				str,
				emb,
				ret;

			_abort();

			selectedOpts = $.extend({}, $.fn.fancybox.defaults, (typeof $(obj).data('fancybox') == 'undefined' ? selectedOpts : $(obj).data('fancybox')));

			ret = selectedOpts.onStart(selectedArray, selectedIndex, selectedOpts);

			if (ret === false) {
				busy = false;
				return;
			} else if (typeof ret == 'object') {
				selectedOpts = $.extend(selectedOpts, ret);
			}

			title = selectedOpts.title || (obj.nodeName ? $(obj).attr('title') : obj.title) || '';

			if (obj.nodeName && !selectedOpts.orig) {
				selectedOpts.orig = $(obj).children("img:first").length ? $(obj).children("img:first") : $(obj);
			}

			if (title === '' && selectedOpts.orig && selectedOpts.titleFromAlt) {
				title = selectedOpts.orig.attr('alt');
			}

			href = selectedOpts.href || (obj.nodeName ? $(obj).attr('href') : obj.href) || null;

			if ((/^(?:javascript)/i).test(href) || href == '#') {
				href = null;
			}

			if (selectedOpts.type) {
				type = selectedOpts.type;

				if (!href) {
					href = selectedOpts.content;
				}

			} else if (selectedOpts.content) {
				type = 'html';

			} else if (href) {
				if (href.match(imgRegExp)) {
					type = 'image';

				} else if (href.match(swfRegExp)) {
					type = 'swf';

				} else if ($(obj).hasClass("iframe")) {
					type = 'iframe';

				} else if (href.indexOf("#") === 0) {
					type = 'inline';

				} else {
					type = 'ajax';
				}
			}

			if (!type) {
				_error();
				return;
			}

			if (type == 'inline') {
				obj	= href.substr(href.indexOf("#"));
				type = $(obj).length > 0 ? 'inline' : 'ajax';
			}

			selectedOpts.type = type;
			selectedOpts.href = href;
			selectedOpts.title = title;

			if (selectedOpts.autoDimensions) {
				if (selectedOpts.type == 'html' || selectedOpts.type == 'inline' || selectedOpts.type == 'ajax') {
					selectedOpts.width = 'auto';
					//if(!selectedOpts.height) {
						selectedOpts.height = 'auto';
					//}
				} else {
					selectedOpts.autoDimensions = false;	
				}
			}

			if (selectedOpts.modal) {
				selectedOpts.overlayShow = true;
				selectedOpts.hideOnOverlayClick = false;
				selectedOpts.hideOnContentClick = false;
				selectedOpts.enableEscapeButton = false;
				selectedOpts.showCloseButton = false;
			}

			selectedOpts.padding = parseInt(selectedOpts.padding, 10);
			selectedOpts.margin = parseInt(selectedOpts.margin, 10);

			tmp.css('padding', (selectedOpts.padding + selectedOpts.margin));

			$('.fancybox-inline-tmp').unbind('fancybox-cancel').bind('fancybox-change', function() {
				$(this).replaceWith(content.children());				
			});

			switch (type) {
				case 'html' :
					tmp.html( selectedOpts.content );
					_process_inline();
				break;

				case 'inline' :
					if ( $(obj).parent().is('#fancybox-content') === true) {
						busy = false;
						return;
					}

					$('<div class="fancybox-inline-tmp" />')
						.hide()
						.insertBefore( $(obj) )
						.bind('fancybox-cleanup', function() {
							$(this).replaceWith(content.children());
						}).bind('fancybox-cancel', function() {
							$(this).replaceWith(tmp.children());
						});

					$(obj).appendTo(tmp);

					_process_inline();
				break;

				case 'image':
					busy = false;

					$.fancybox.showActivity();

					imgPreloader = new Image();

					imgPreloader.onerror = function() {
						_error();
					};

					imgPreloader.onload = function() {
						busy = true;

						imgPreloader.onerror = imgPreloader.onload = null;

						_process_image();
					};

					imgPreloader.src = href;
				break;

				case 'swf':
					selectedOpts.scrolling = 'no';

					str = '<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" width="' + selectedOpts.width + '" height="' + selectedOpts.height + '"><param name="movie" value="' + href + '"></param>';
					emb = '';

					$.each(selectedOpts.swf, function(name, val) {
						str += '<param name="' + name + '" value="' + val + '"></param>';
						emb += ' ' + name + '="' + val + '"';
					});

					str += '<embed src="' + href + '" type="application/x-shockwave-flash" width="' + selectedOpts.width + '" height="' + selectedOpts.height + '"' + emb + '></embed></object>';

					tmp.html(str);

					_process_inline();
				break;

				case 'ajax':
					busy = false;

					$.fancybox.showActivity();

					selectedOpts.ajax.win = selectedOpts.ajax.success;

					ajaxLoader = $.ajax($.extend({}, selectedOpts.ajax, {
						url	: href+'?mod=1',
						data : selectedOpts.ajax.data || {},
						error : function(XMLHttpRequest, textStatus, errorThrown) {
							if ( XMLHttpRequest.status > 0 ) {
								_error();
							}
						},
						success : function(data, textStatus, XMLHttpRequest) {
							var o = typeof XMLHttpRequest == 'object' ? XMLHttpRequest : ajaxLoader;
							if (o.status == 200) {
								if ( typeof selectedOpts.ajax.win == 'function' ) {
									ret = selectedOpts.ajax.win(href, data, textStatus, XMLHttpRequest);

									if (ret === false) {
										loading.hide();
										return;
									} else if (typeof ret == 'string' || typeof ret == 'object') {
										data = ret;
									}
								}

								tmp.html( data );
								_process_inline();
							}
						}
					}));

				break;

				case 'iframe':
					_show();
				break;
			}
		},

		_process_inline = function() {

			var
				w = selectedOpts.width,
				h = selectedOpts.height;

			if (w.toString().indexOf('%') > -1) {
				w = parseInt( ($(window).width() - (selectedOpts.margin * 2)) * parseFloat(w) / 100, 10) + 'px';

			} else {
				w = w == 'auto' ? 'auto' : w + 'px';	
			}

			if (h.toString().indexOf('%') > -1) {
				h = parseInt( ($(window).height() - (selectedOpts.margin * 2)) * parseFloat(h) / 100, 10) + 'px';

			} else {
				h = h == 'auto' ? 'auto' : h + 'px';	
			}

			//console.log('height['+h+']');
			//console.log('width['+w+']');
			
			
			//create the content wrapper and find out the dimensions
			$('body').append('<div id="fb_tmp">' + tmp.html() + '</div>');
			var actual_height = $('#fb_tmp').css('height').replace('px','');
			var actual_width = $('#fb_tmp').css('width').replace('px','');
			$('#fb_tmp').remove();

            //console.log('actual height['+actual_height+']');
			//console.log('actual width['+actual_width+']');

			//set max height so popup doesnt need to scroll vertical with height auto
			var max_height = ($(window).height()/100)*75;			
			
			if(actual_height > max_height) {				
				h = max_height+'px';				
			} else {
				selectedOpts.scrolling = false;
			}
			
			tmp.wrapInner('<div style="width:' + w + ';height:' + h + ';overflow-x: hidden; overflow-y: ' + (selectedOpts.scrolling == 'auto' ? 'hidden' : (selectedOpts.scrolling == 'yes' ? 'scroll' : 'hidden')) + ';position:relative;"></div>');
    	    selectedOpts.width = tmp.width();
			selectedOpts.height = tmp.height();
						
			_show();
		},

		_process_image = function() {
			selectedOpts.width = imgPreloader.width;
			selectedOpts.height = imgPreloader.height;

			$("<img />").attr({
				'id' : 'fancybox-img',
				'src' : imgPreloader.src,
				'alt' : selectedOpts.title
			}).appendTo( tmp );

			_show();
		},

		_show = function() {
			var pos, equal;

			loading.hide();

			if (wrap.is(":visible") && false === currentOpts.onCleanup(currentArray, currentIndex, currentOpts)) {
				$.event.trigger('fancybox-cancel');

				busy = false;
				return;
			}

			busy = true;

			$(content.add( overlay )).unbind();

			$(window).unbind("resize.fb scroll.fb");
			$(document).unbind('keydown.fb');

			if (wrap.is(":visible") && currentOpts.titlePosition !== 'outside') {
				wrap.css('height', wrap.height());
			}

			currentArray = selectedArray;
			currentIndex = selectedIndex;
			currentOpts = selectedOpts;

			if (currentOpts.overlayShow) {
				overlay.css({
					'background-color' : currentOpts.overlayColor,
					'opacity' : currentOpts.overlayOpacity,
					'cursor' : currentOpts.hideOnOverlayClick ? 'pointer' : 'auto',
					'height' : $(document).height()
				});

				if (!overlay.is(':visible')) {
					if (isIE6) {
						$('select:not(#fancybox-tmp select)').filter(function() {
							return this.style.visibility !== 'hidden';
						}).css({'visibility' : 'hidden'}).one('fancybox-cleanup', function() {
							this.style.visibility = 'inherit';
						});
					}

					overlay.show();
				}

			} else {
				overlay.hide();
			}

			final_pos = _get_zoom_to();

			_process_title();

			if (wrap.is(":visible")) {
				$( close.add( nav_left ).add( nav_right ) ).hide();

				pos = wrap.position(),

				start_pos = {
					top	 : pos.top,
					left : pos.left,
					width : wrap.width(),
					height : wrap.height()
				};

				equal = (start_pos.width == final_pos.width && start_pos.height == final_pos.height);

				content.fadeTo(currentOpts.changeFade, 0.3, function() {
					var finish_resizing = function() {
                        
						content.html( tmp.contents() ).fadeTo(currentOpts.changeFade, 1, _finish);
					};

					$.event.trigger('fancybox-change');

					content
						.empty()
						.removeAttr('filter')
						.css({
							'border-width' : currentOpts.padding,
							'width'	: final_pos.width - currentOpts.padding * 2,
							'height' : selectedOpts.autoDimensions ? 'auto' : final_pos.height - titleHeight - currentOpts.padding * 2
						});
                    
					if (equal) {
						finish_resizing();

					} else {
						fx.prop = 0;

						$(fx).animate({prop: 1}, {
							 duration : currentOpts.changeSpeed,
							 easing : currentOpts.easingChange,
							 step : _draw,
							 complete : finish_resizing
						});
					}
				});

				return;
			}

			wrap.removeAttr("style");

			content.css('border-width', currentOpts.padding);

			if (currentOpts.transitionIn == 'elastic') {
				start_pos = _get_zoom_from();

				content.html( tmp.contents() );

				wrap.show();

				if (currentOpts.opacity) {
					final_pos.opacity = 0;
				}

				fx.prop = 0;

				$(fx).animate({prop: 1}, {
					 duration : currentOpts.speedIn,
					 easing : currentOpts.easingIn,
					 step : _draw,
					 complete : _finish
				});

				return;
			}

			if (currentOpts.titlePosition == 'inside' && titleHeight > 0) {	
				title.show();	
			}

			content
				.css({
					'width' : final_pos.width - currentOpts.padding * 2,
					'height' : selectedOpts.autoDimensions ? 'auto' : final_pos.height - titleHeight - currentOpts.padding * 2
				})
				.html( tmp.contents() );

			wrap
				.css(final_pos)
				.fadeIn( currentOpts.transitionIn == 'none' ? 0 : currentOpts.speedIn, _finish );
            
		},

		_format_title = function(title) {
			if (title && title.length) {
				if (currentOpts.titlePosition == 'float') {
					return '<table id="fancybox-title-float-wrap" cellpadding="0" cellspacing="0"><tr><td id="fancybox-title-float-left"></td><td id="fancybox-title-float-main">' + title + '</td><td id="fancybox-title-float-right"></td></tr></table>';
				}

				return '<div id="fancybox-title-' + currentOpts.titlePosition + '">' + title + '</div>';
			}

			return false;
		},

		_process_title = function() {
			titleStr = currentOpts.title || '';
			titleHeight = 0;

			title
				.empty()
				.removeAttr('style')
				.removeClass();

			if (currentOpts.titleShow === false) {
				title.hide();
				return;
			}

			titleStr = $.isFunction(currentOpts.titleFormat) ? currentOpts.titleFormat(titleStr, currentArray, currentIndex, currentOpts) : _format_title(titleStr);

			if (!titleStr || titleStr === '') {
				title.hide();
				return;
			}

			title
				.addClass('fancybox-title-' + currentOpts.titlePosition)
				.html( titleStr )
				.appendTo( 'body' )
				.show();

			switch (currentOpts.titlePosition) {
				case 'inside':
					title
						.css({
							'width' : final_pos.width - (currentOpts.padding * 2),
							'marginLeft' : currentOpts.padding,
							'marginRight' : currentOpts.padding
						});

					titleHeight = title.outerHeight(true);

					title.appendTo( outer );

					final_pos.height += titleHeight;
				break;

				case 'over':
					title
						.css({
							'marginLeft' : currentOpts.padding,
							'width'	: final_pos.width - (currentOpts.padding * 2),
							'bottom' : currentOpts.padding
						})
						.appendTo( outer );
				break;

				case 'float':
					title
						.css('left', parseInt((title.width() - final_pos.width - 40)/ 2, 10) * -1)
						.appendTo( wrap );
				break;

				default:
					title
						.css({
							'width' : final_pos.width - (currentOpts.padding * 2),
							'paddingLeft' : currentOpts.padding,
							'paddingRight' : currentOpts.padding
						})
						.appendTo( wrap );
				break;
			}

			title.hide();
		},

		_set_navigation = function() {
			if (currentOpts.enableEscapeButton || currentOpts.enableKeyboardNav) {
				$(document).bind('keydown.fb', function(e) {
					if (e.keyCode == 27 && currentOpts.enableEscapeButton) {
						e.preventDefault();
						$.fancybox.close();

					} else if ((e.keyCode == 37 || e.keyCode == 39) && currentOpts.enableKeyboardNav && e.target.tagName !== 'INPUT' && e.target.tagName !== 'TEXTAREA' && e.target.tagName !== 'SELECT') {
						e.preventDefault();
						$.fancybox[ e.keyCode == 37 ? 'prev' : 'next']();
					}
				});
			}

			if (!currentOpts.showNavArrows) { 
				nav_left.hide();
				nav_right.hide();
				return;
			}

			if ((currentOpts.cyclic && currentArray.length > 1) || currentIndex !== 0) {
				nav_left.show();
			}

			if ((currentOpts.cyclic && currentArray.length > 1) || currentIndex != (currentArray.length -1)) {
				nav_right.show();
			}
		},

		_finish = function () {
			if (!$.support.opacity) {
				content.get(0).style.removeAttribute('filter');
				wrap.get(0).style.removeAttribute('filter');
			}

			if (selectedOpts.autoDimensions) {
				content.css('height', 'auto');
			}

			wrap.css('height', 'auto');

			if (titleStr && titleStr.length) {
				title.show();
			}

			if (currentOpts.showCloseButton) {
				close.show();
			}

			_set_navigation();
	
			if (currentOpts.hideOnContentClick)	{
				content.bind('click', $.fancybox.close);
			}

			if (currentOpts.hideOnOverlayClick)	{
				overlay.bind('click', $.fancybox.close);
			}

			$(window).bind("resize.fb", $.fancybox.resize);

			if (currentOpts.centerOnScroll) {
				$(window).bind("scroll.fb", $.fancybox.center);
			}

			if (currentOpts.type == 'iframe') {
				/*$('<iframe id="fancybox-frame" name="fancybox-frame' + new Date().getTime() + '" frameborder="0" hspace="0" ' + ($.browser.msie ? 'allowtransparency="true""' : '') + ' scrolling="' + selectedOpts.scrolling + '" src="' + currentOpts.href + '"></iframe>').appendTo(content);*/
        if (selectedOpts && selectedOpts.showIframeLoading) {
					$.fancybox.showActivity();
				}
				$('<iframe id="fancybox-frame" name="fancybox-frame' + new Date().getTime() + '" frameborder="0" hspace="0" ' + ($.browser.msie ? 'allowtransparency="true""' : '') + ' scrolling="' + selectedOpts.scrolling + '" src="' + currentOpts.href + '"></iframe>').appendTo(content).load(function() {
					if (selectedOpts && selectedOpts.showIframeLoading) {
						$.fancybox.hideActivity();
					}
					if (selectedOpts && $.isFunction(selectedOpts.onIframeLoad)) {
						selectedOpts.onIframeLoad(selectedArray, selectedIndex, selectedOpts);
					}
				});
			}

			wrap.show();

			busy = false;

			$.fancybox.center();

			currentOpts.onComplete(currentArray, currentIndex, currentOpts);

			_preload_images();
		},

		_preload_images = function() {
			var href, 
				objNext;

			if ((currentArray.length -1) > currentIndex) {
				href = currentArray[ currentIndex + 1 ].href;

				if (typeof href !== 'undefined' && href.match(imgRegExp)) {
					objNext = new Image();
					objNext.src = href;
				}
			}

			if (currentIndex > 0) {
				href = currentArray[ currentIndex - 1 ].href;

				if (typeof href !== 'undefined' && href.match(imgRegExp)) {
					objNext = new Image();
					objNext.src = href;
				}
			}
		},

		_draw = function(pos) {
			var dim = {
				width : parseInt(start_pos.width + (final_pos.width - start_pos.width) * pos, 10),
				height : parseInt(start_pos.height + (final_pos.height - start_pos.height) * pos, 10),

				top : parseInt(start_pos.top + (final_pos.top - start_pos.top) * pos, 10),
				left : parseInt(start_pos.left + (final_pos.left - start_pos.left) * pos, 10)
			};

			if (typeof final_pos.opacity !== 'undefined') {
				dim.opacity = pos < 0.5 ? 0.5 : pos;
			}

			wrap.css(dim);

			content.css({
				'width' : dim.width - currentOpts.padding * 2,
				'height' : dim.height - (titleHeight * pos) - currentOpts.padding * 2
			});
		},

		_get_viewport = function() {
			return [
				$(window).width() - (currentOpts.margin * 2),
				$(window).height() - (currentOpts.margin * 2),
				$(document).scrollLeft() + currentOpts.margin,
				$(document).scrollTop() + currentOpts.margin
			];
		},

		_get_zoom_to = function () {
			var view = _get_viewport(),
				to = {},
				resize = currentOpts.autoScale,
				double_padding = currentOpts.padding * 2,
				ratio;

			if (currentOpts.width.toString().indexOf('%') > -1) {
				to.width = parseInt((view[0] * parseFloat(currentOpts.width)) / 100, 10);
			} else {
				to.width = currentOpts.width + double_padding;
			}

			if (currentOpts.height.toString().indexOf('%') > -1) {
				to.height = parseInt((view[1] * parseFloat(currentOpts.height)) / 100, 10);
			} else {
				to.height = currentOpts.height + double_padding;
			}

			if (resize && (to.width > view[0] || to.height > view[1])) {
				if (selectedOpts.type == 'image' || selectedOpts.type == 'swf') {
					ratio = (currentOpts.width ) / (currentOpts.height );

					if ((to.width ) > view[0]) {
						to.width = view[0];
						to.height = parseInt(((to.width - double_padding) / ratio) + double_padding, 10);
					}

					if ((to.height) > view[1]) {
						to.height = view[1];
						to.width = parseInt(((to.height - double_padding) * ratio) + double_padding, 10);
					}

				} else {
					to.width = Math.min(to.width, view[0]);
					to.height = Math.min(to.height, view[1]);
				}
			}

			to.top = parseInt(Math.max(view[3] - 20, view[3] + ((view[1] - to.height - 40) * 0.5)), 10);
			to.left = parseInt(Math.max(view[2] - 20, view[2] + ((view[0] - to.width - 40) * 0.5)), 10);

			return to;
		},

		_get_obj_pos = function(obj) {
			var pos = obj.offset();

			pos.top += parseInt( obj.css('paddingTop'), 10 ) || 0;
			pos.left += parseInt( obj.css('paddingLeft'), 10 ) || 0;

			pos.top += parseInt( obj.css('border-top-width'), 10 ) || 0;
			pos.left += parseInt( obj.css('border-left-width'), 10 ) || 0;

			pos.width = obj.width();
			pos.height = obj.height();

			return pos;
		},

		_get_zoom_from = function() {
			var orig = selectedOpts.orig ? $(selectedOpts.orig) : false,
				from = {},
				pos,
				view;

			if (orig && orig.length) {
				pos = _get_obj_pos(orig);

				from = {
					width : pos.width + (currentOpts.padding * 2),
					height : pos.height + (currentOpts.padding * 2),
					top	: pos.top - currentOpts.padding - 20,
					left : pos.left - currentOpts.padding - 20
				};

			} else {
				view = _get_viewport();

				from = {
					width : currentOpts.padding * 2,
					height : currentOpts.padding * 2,
					top	: parseInt(view[3] + view[1] * 0.5, 10),
					left : parseInt(view[2] + view[0] * 0.5, 10)
				};
			}

			return from;
		},

		_animate_loading = function() {
			if (!loading.is(':visible')){
				clearInterval(loadingTimer);
				return;
			}

			$('div', loading).css('top', (loadingFrame * -40) + 'px');

			loadingFrame = (loadingFrame + 1) % 12;
		};

	/*
	 * Public methods 
	 */

	$.fn.fancybox = function(options) {
		if (!$(this).length) {
			return this;
		}

		$(this)
			.data('fancybox', $.extend({}, options, ($.metadata ? $(this).metadata() : {})))
			.unbind('click.fb')
			.bind('click.fb',{e:$(this)},startmodalbox);

		return this;
	};
  
  //had to do a bit of work here to get clicktale trying to work for popups, the js command
  //were going to pass to clicktale is a manual "click" of the exact element that was clicked
  //initially - this should passthrough the this object correctly
  function startmodalbox(e) {
    
      var element_href = $(this).attr('href');
      var this_element = '.mod[href="' + element_href + '"]';      
   
      if(typeof ClickTaleExec=='function')
      ClickTaleExec("jQuery('" + this_element + "').click()");

      e.preventDefault();

      if (busy) {
        return;
      }

      busy = true;

      $(this).blur();

      selectedArray = [];
      selectedIndex = 0;

      var rel = $(this).attr('rel') || '';

      if (!rel || rel == '' || rel === 'nofollow') {
        selectedArray.push(this);

      } else {
        selectedArray = $("a[rel=" + rel + "], area[rel=" + rel + "]");
        selectedIndex = selectedArray.index( this );
      }

      _start();

      return;
    }
  

	$.fancybox = function(obj) {
		var opts;

		if (busy) {
			return;
		}

		busy = true;
		opts = typeof arguments[1] !== 'undefined' ? arguments[1] : {};

		selectedArray = [];
		selectedIndex = parseInt(opts.index, 10) || 0;

		if ($.isArray(obj)) {
			for (var i = 0, j = obj.length; i < j; i++) {
				if (typeof obj[i] == 'object') {
					$(obj[i]).data('fancybox', $.extend({}, opts, obj[i]));
				} else {
					obj[i] = $({}).data('fancybox', $.extend({content : obj[i]}, opts));
				}
			}

			selectedArray = jQuery.merge(selectedArray, obj);

		} else {
			if (typeof obj == 'object') {
				$(obj).data('fancybox', $.extend({}, opts, obj));
			} else {
				obj = $({}).data('fancybox', $.extend({content : obj}, opts));
			}

			selectedArray.push(obj);
		}

		if (selectedIndex > selectedArray.length || selectedIndex < 0) {
			selectedIndex = 0;
		}

		_start();
	};

	$.fancybox.showActivity = function() {
		clearInterval(loadingTimer);

		loading.show();
		//loadingTimer = setInterval(_animate_loading, 66);
	};

	$.fancybox.hideActivity = function() {
		loading.hide();
	};

	$.fancybox.next = function() {
		return $.fancybox.pos( currentIndex + 1);
	};

	$.fancybox.prev = function() {
		return $.fancybox.pos( currentIndex - 1);
	};

	$.fancybox.pos = function(pos) {
		if (busy) {
			return;
		}

		pos = parseInt(pos);

		selectedArray = currentArray;

		if (pos > -1 && pos < currentArray.length) {
			selectedIndex = pos;
			_start();

		} else if (currentOpts.cyclic && currentArray.length > 1) {
			selectedIndex = pos >= currentArray.length ? 0 : currentArray.length - 1;
			_start();
		}

		return;
	};

	$.fancybox.cancel = function() {
		if (busy) {
			return;
		}

		busy = true;

		$.event.trigger('fancybox-cancel');

		_abort();

		selectedOpts.onCancel(selectedArray, selectedIndex, selectedOpts);

		busy = false;
	};

	// Note: within an iframe use - parent.$.fancybox.close();
	$.fancybox.close = function() {
		if (busy || wrap.is(':hidden')) {
			return;
		}

		busy = true;

		if (currentOpts && false === currentOpts.onCleanup(currentArray, currentIndex, currentOpts)) {
			busy = false;
			return;
		}

		_abort();

		$(close.add( nav_left ).add( nav_right )).hide();

		$(content.add( overlay )).unbind();

		$(window).unbind("resize.fb scroll.fb");
		$(document).unbind('keydown.fb');

		content.find('iframe').attr('src', isIE6 && /^https/i.test(window.location.href || '') ? 'javascript:void(false)' : 'about:blank');

		if (currentOpts.titlePosition !== 'inside') {
			title.empty();
		}

		wrap.stop();

		function _cleanup() {
			overlay.fadeOut('fast');

			title.empty().hide();
			wrap.hide();

			$.event.trigger('fancybox-cleanup');

			content.empty();

			currentOpts.onClosed(currentArray, currentIndex, currentOpts);

			currentArray = selectedOpts	= [];
			currentIndex = selectedIndex = 0;
			currentOpts = selectedOpts	= {};

			busy = false;
		}

		if (currentOpts.transitionOut == 'elastic') {
			start_pos = _get_zoom_from();

			var pos = wrap.position();

			final_pos = {
				top	 : pos.top ,
				left : pos.left,
				width :	wrap.width(),
				height : wrap.height()
			};

			if (currentOpts.opacity) {
				final_pos.opacity = 1;
			}

			title.empty().hide();

			fx.prop = 1;
			
			$(fx).animate({ prop: 0 }, {
				 duration : currentOpts.speedOut,
				 easing : currentOpts.easingOut,
				 step : _draw,
				 complete : _cleanup
			});

		} else {
			wrap.fadeOut( currentOpts.transitionOut == 'none' ? 0 : currentOpts.speedOut, _cleanup);
		}
	};

	$.fancybox.resize = function() {
		if (overlay.is(':visible')) {
			overlay.css('height', $(document).height());
		}

		$.fancybox.center(true);
	};

	$.fancybox.center = function() {
		var view, align;

		if (busy) {
			return;	
		}

		align = arguments[0] === true ? 1 : 0;
		view = _get_viewport();

		if (!align && (wrap.width() > view[0] || wrap.height() > view[1])) {
			return;	
		}

		wrap
			.stop()
			.animate({
				'top' : parseInt(Math.max(view[3] - 20, view[3] + ((view[1] - content.height() - 40) * 0.5) - currentOpts.padding)),
				'left' : parseInt(Math.max(view[2] - 20, view[2] + ((view[0] - content.width() - 40) * 0.5) - currentOpts.padding))
			}, typeof arguments[0] == 'number' ? arguments[0] : 200);
	};

	$.fancybox.init = function() {
		if ($("#fancybox-wrap").length) {
			return;
		}

		$('body').append(
			tmp	= $('<div id="fancybox-tmp"></div>'),
			loading	= $('<div id="fancybox-loading"><div></div></div>'),
			overlay	= $('<div id="fancybox-overlay"></div>'),
			wrap = $('<div id="fancybox-wrap"></div>')
		);

		outer = $('<div id="fancybox-outer"></div>')
			.append('<div class="fancybox-bg" id="fancybox-bg-n"></div><div class="fancybox-bg" id="fancybox-bg-ne"></div><div class="fancybox-bg" id="fancybox-bg-e"></div><div class="fancybox-bg" id="fancybox-bg-se"></div><div class="fancybox-bg" id="fancybox-bg-s"></div><div class="fancybox-bg" id="fancybox-bg-sw"></div><div class="fancybox-bg" id="fancybox-bg-w"></div><div class="fancybox-bg" id="fancybox-bg-nw"></div>')
			.appendTo( wrap );

		outer.append(
			content = $('<div id="fancybox-content"></div>'),
			close = $('<a id="fancybox-close"></a>'),
			title = $('<div id="fancybox-title"></div>'),

			nav_left = $('<a href="javascript:;" id="fancybox-left"><span class="fancy-ico" id="fancybox-left-ico"></span></a>'),
			nav_right = $('<a href="javascript:;" id="fancybox-right"><span class="fancy-ico" id="fancybox-right-ico"></span></a>')
		);

		close.click($.fancybox.close);
		loading.click($.fancybox.cancel);

		nav_left.click(function(e) {
			e.preventDefault();
			$.fancybox.prev();
		});

		nav_right.click(function(e) {
			e.preventDefault();
			$.fancybox.next();
		});

		if ($.fn.mousewheel) {
			wrap.bind('mousewheel.fb', function(e, delta) {
				if (busy) {
					e.preventDefault();

				} else if ($(e.target).get(0).clientHeight == 0 || $(e.target).get(0).scrollHeight === $(e.target).get(0).clientHeight) {
					e.preventDefault();
					$.fancybox[ delta > 0 ? 'prev' : 'next']();
				}
			});
		}

		if (!$.support.opacity) {
			wrap.addClass('fancybox-ie');
		}

		if (isIE6) {
			loading.addClass('fancybox-ie6');
			wrap.addClass('fancybox-ie6');

			$('<iframe id="fancybox-hide-sel-frame" src="' + (/^https/i.test(window.location.href || '') ? 'javascript:void(false)' : 'about:blank' ) + '" scrolling="no" border="0" frameborder="0" tabindex="-1"></iframe>').prependTo(outer);
		}
	};

	$.fn.fancybox.defaults = {
		padding : 10,
		margin : 40,
		opacity : false,
		modal : false,
		cyclic : false,
		scrolling : 'auto',	// 'auto', 'yes' or 'no'

		width : 560,
		height : 340,

		autoScale : true,
		autoDimensions : true,
		centerOnScroll : false,

		ajax : {},
		swf : { wmode: 'transparent' },

		hideOnOverlayClick : true,
		hideOnContentClick : false,

		overlayShow : true,
		overlayOpacity : 0.7,
		overlayColor : '#777',

		titleShow : true,
		titlePosition : 'float', // 'float', 'outside', 'inside' or 'over'
		titleFormat : null,
		titleFromAlt : false,

		transitionIn : 'fade', // 'elastic', 'fade' or 'none'
		transitionOut : 'fade', // 'elastic', 'fade' or 'none'

		speedIn : 300,
		speedOut : 300,

		changeSpeed : 300,
		changeFade : 'fast',

		easingIn : 'swing',
		easingOut : 'swing',

		showCloseButton	 : true,
		showNavArrows : true,
    showIframeLoading : true,
		enableEscapeButton : true,
		enableKeyboardNav : true,

		onStart : function(){},
		onCancel : function(){},
		onComplete : function(){},
    onIframeLoad : function(){},
		onCleanup : function(){},
		onClosed : function(){},
		onError : function(){}
	};

	$(document).ready(function() {
		$.fancybox.init();
	});

})(jQuery);;
(function($){

	$.fn.alphanumeric = function(p) { 

		p = $.extend({
			ichars: "!@#$%^&*()+=[]\\\';,/{}|\":<>?~`.- _",
			nchars: "",
			allow: ""
		  }, p);	

		return this.each
			(
				function() 
				{

					if (p.nocaps) p.nchars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
					if (p.allcaps) p.nchars += "abcdefghijklmnopqrstuvwxyz";
					
					s = p.allow.split('');
					for ( i=0;i<s.length;i++) if (p.ichars.indexOf(s[i]) != -1) s[i] = "\\" + s[i];
					p.allow = s.join('|');
					
					var reg = new RegExp(p.allow,'gi');
					var ch = p.ichars + p.nchars;
					ch = ch.replace(reg,'');

					$(this).keypress
						(
							function (e)
								{
								
									if (!e.charCode) k = String.fromCharCode(e.which);
										else k = String.fromCharCode(e.charCode);
										
									if (ch.indexOf(k) != -1) e.preventDefault();
									if (e.ctrlKey&&k=='v') e.preventDefault();
									
								}
								
						);
						
					$(this).bind('contextmenu',function () {return false});
									
				}
			);

	};

	$.fn.numeric = function(p) {
	
		var az = "abcdefghijklmnopqrstuvwxyz";
		az += az.toUpperCase();

		p = $.extend({
			nchars: az
		  }, p);	
		  	
		return this.each (function()
			{
				$(this).alphanumeric(p);
			}
		);
			
	};
	
	$.fn.alpha = function(p) {

		var nm = "1234567890";

		p = $.extend({
			nchars: nm
		  }, p);	

		return this.each (function()
			{
				$(this).alphanumeric(p);
			}
		);
			
	};	

})(jQuery);
;
$(document).ready(function () {
    $().wonga('init');

    // valign for summary page
    $(function () {
        // vertical align function
        $.fn.vAlign = function () {
            return this.each(function (i) {
                var ah = $(this).height();
                var ph = $(this).parent().height();
                var mh = Math.ceil((ph - ah) / 2); //var mh = (ph - ah) / 2;
                $(this).css('margin-top', mh);
            });
        };

        //$('.summary-box-content .summary-text').vAlign();
        //$('.summary-box-content #cloud').vAlign();

    });

});;
jQuery.fn.supersleight = function(settings) {
	settings = jQuery.extend({
		imgs: true,
		backgrounds: true,
		shim: '/sites/all/themes/pizaz/images/x.gif',
		apply_positioning: true
	}, settings);
	
	return this.each(function(){
		if (jQuery.browser.msie && parseInt(jQuery.browser.version, 10) < 7 && parseInt(jQuery.browser.version, 10) > 4) {
			jQuery(this).find('*').andSelf().each(function(i,obj) {
				var self = jQuery(obj);
				// background pngs
				if (settings.backgrounds && self.css('background-image').match(/\.png/i) !== null) {
					var bg = self.css('background-image');
					var src = bg.substring(5,bg.length-2);
					var mode = (self.css('background-repeat') == 'no-repeat' ? 'crop' : 'scale');
					var styles = {
						'filter': "progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + src + "', sizingMethod='" + mode + "')",
						'background-image': 'url('+settings.shim+')'
					};
					self.css(styles);
				};
				// image elements
				if (settings.imgs && self.is('img[src$=png]')){
					var styles = {
						'width': self.width() + 'px',
						'height': self.height() + 'px',
						'filter': "progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + self.attr('src') + "', sizingMethod='scale')"
					};
					self.css(styles).attr('src', settings.shim);
				};
				// apply position to 'active' elements
				if (settings.apply_positioning && self.is('a, input') && (self.css('position') === '' || self.css('position') == 'static')){
					self.css('position', 'relative');
				};
			});
		};
	});
};;
//wonga core controller
(function($){

  $.fn.wonga = function(method){
    if ( methods[method] ) {
      return methods[method].apply( this, Array.prototype.slice.call( arguments, 1 ));
    } else if ( typeof method === 'object' || ! method ) {
      return methods.init.apply( this, arguments );
    }
  };


  var methods = {
    init : function(options) { //Main wonga jquery function called on document load on every page

      //TODO: EXTERNAL CALL MERGE WITH WONGA.SESSIONS.JS
      if (typeof wonga_session !== 'undefined') {
        wonga_session();
      }

      //TODO: EXTERNALL CALL MERGE or KILL JQUERY.DAYPLUSMIN.JS RE-CHECK DUPLICATE IN CALC##.JS
      $().dayplusmin('init');

      //TODO: STILL TO REVIEW
      if($.browser.msie && $.browser.version=='8.0')
      {
        //position fix ?
        $('#edit-submit').mousedown(function() {$(this).css('background-position', '1px 1px');});
        $('#edit-submit').mouseup(function() {$(this).css('background-position', '0px 0px');});
      }

      //TODO: REVIEW => VALIDATION?
      $('#wonga-sliders-form').attr('autocomplete','off');

      //TODO: REVIEW => GET RID OF ONCE ALL REGIONS USE SLIDERS2
      if( $('#edit-loan-amount').length > 0 ) {
          $('#edit-loan-amount').val( wonga_format_currency($('#edit-loan-amount').val(),true,0) );
      }

      //TODO: REVIEW => GET RID OF ONCE ALL REGIONS USE SLIDERS2
      $('#edit-loan-amount').name = Math.random();
      $('#edit-loan-duration').name = Math.random();

      //TODO: REVIEW => GET RID OF ONCE ALL REGIONS USE SLIDERS2
      $('#edit-loan-amount').focus( function() {
        $(this).parents('.form-input').css('background-position','0px -42px');
      });
      $('#edit-loan-amount').blur( function() {
        $(this).parents('.form-input').css('background-position','0px 0px');
      });
      $('#edit-loan-duration').focus( function() {
        $(this).parents('.form-input').css('background-position','0px -42px');
      });
      $('#edit-loan-duration').blur( function() {
        $(this).parents('.form-input').css('background-position','0px 0px');
      });

      //TODO: REVIEW => GET RID OF ONCE ALL REGIONS USE SLIDERS2
      $('#wonga-sliders-form').keypress( function(event) {
        if( event.keyCode == 13 && $('#edit-submit:focus').length != 1 ) return false;
      });


      //TODO: REVIEW => GET RID OF ONCE ALL REGIONS USE SLIDERS2 (pizaz/css/i18n & pizaz/images/##)
        // Focus on Apply now button to change button state, not working needs fix - JB
        //$('#edit-submit').focus( function() {
        //  $(this).css('background-image','url("/sites/all/themes/pizaz/images/buttons/promo_btn_left_hover.png")');
        //  $(this).parents('#button-edit-submit').find('.buttonEnding').css('background-image','url("/sites/all/themes/pizaz/images/buttons/promo_btn_right_hover.png")');
        //});
        //$('#edit-submit').blur( function() {
        //  $(this).css('background-image','url("/sites/all/themes/pizaz/images/buttons/promo_btn_left.png")');
        //  $(this).parents('#button-edit-submit').find('.buttonEnding').css('background-image','url("/sites/all/themes/pizaz/images/buttons/promo_btn_right.png")');
        //});

      //TODO: REVIEW => LOAN-APPROVE PAGE ?  MOVE TO AGREEMENTS PAGE JS (VALIDATION?)
      $('#accept').click( function() {
        $('#wonga-loan-approve-form').submit();
        return false;
      });

      //TODO: REVIEW => CHECK WITH ALEX HOW IT WORKS
      if (typeof $().initialise_jquery_validation === 'function') {
        $().initialise_jquery_validation(); //WONGA.VALIDATION.FORM.FIELDS.JS
      }

      //TODO: REVIEW => ie6 png fixes
      $().wonga('ie6_png_fix');

      //TODO: REVIEW WTF?
      //add multiple fieldsets
      //$().wonga('add_multiples');

      //TODO: REVIEW => WHY ITS DUPLICATED, WHAT DOES IT AFFECT -> MOVE TO AGREEMENT PAGES JS
      // scroll agreement to bottom of page
      $().wonga('agreement_scroll');

      //wonga log clear log
      $('#clear-log').click( function() {
        $.get('wonga-clear-log', function(data) {
          location.reload();
        });
      });

      //TODO: REVIEW => bind popup modals to all anchors with a class of mod -> ONCE FANCYBOX2 IS ENABLED.
      $().wonga('bind_modals');

      //TODO: REVIEW => hover bars -> WHAT IS THAT?
      $().wonga('hover_bars');

      //TODO: REVIEW => expand all request responses -> IN USE?
      $('.krumo-element').addClass('krumo-opened');

      //TODO: REVIEW => disable date drop downs with class=disabled -> IN USE? -> MOVE TO VALIDATE.JS
      $('.form-select.disabled').attr('disabled','disabled');

      //TODO: REVIEW => add tooltips -> POTENCIAL ISSUE WITH JQUERY.UI/TOOLS SWITCHING OFF.
      $().wonga('tool_tips');

      //TODO: REVIEW => control panel used to login -> POTENCIAL ISSUE WITH JQUERY.UI/TOOLS SWITCHING OFF.
      $().wonga('control_panel');

      //TODO: REVIEW =>  clear any prepopulated field with the class of clearme on focus -> IN USE?
      $('.clearme').one("focus", function() {
        $(this).val("");
      });

      //TODO: REVIEW => for add card add the ajax-clicked class so we know where to add -> MOVE TO AJAX.JS?
      $('.add-card-cell a').click( function() {
        $('#cards .ajax-response').addClass('ajax-clicked');
        return true;
      });

      //TODO: REVIEW => awards scroller in footer -> IN USE?
        //$("#awards").scrollable({circular: true, mousewheel: true}).navigator().autoscroll({
        //interval: 9000
        //});

      //TODO: REVIEW => inactive links should do nothing -> MOVE TO VALIDATE.JS
      $('.inactive').click( function() {
        return false;
      });

      //TODO: REVIEW => homepage survay -> POTENCIAL ISSUE WITH JQUERY.UI/TOOLS SWITCHING OFF.
      if (typeof $.fn.cycle !== 'undefined') {
        $('#survey').cycle({fx:'fade',timeout:  5000});
      }

    }, //END OF METHOD

    simple_ajax_links : function() { //call a links reponse via ajax, no popup

      $('.aj').click( function() {
        var href = $(this).attr('href');

        //display the ajax loader
        $('.ajax-loader').remove();
        $(this).append('<div class="ajax-loader"></div>');

        if( $(this).next().attr('class') == 'ajax-msg' ) {
          $(this).next().fadeOut('slow'); //TODO: POTENCIAL ISSUE WITH JQUERY.UI/TOOLS SWITCHING OFF.
        }

        $.ajax({
          url: href,
          context: $(this),
          success: function(data) {
            if( $(this).next().attr('class') == 'ajax-msg' ) {
              $(this).next().html(data)
              $(this).next().fadeIn('slow'); //TODO: POTENCIAL ISSUE WITH JQUERY.UI/TOOLS SWITCHING OFF.
            } else {
              $(this).after('<p class="ajax-msg" style="display: none;">'+data+'</p>');
              $(this).next('.ajax-msg').fadeIn('slow'); //TODO: POTENCIAL ISSUE WITH JQUERY.UI/TOOLS SWITCHING OFF.
            }
            $('.ajax-loader').hide();
          }
        });

      return false;
      });

    }, //END OF METHOD


    submit_once : function( ) { //all form input with class of form-submit should only submit once no matter how many times they are clicked

      $('.form-submit').click( function() {
          $(this).attr("disabled", "disabled");
      });

    }, //END OF METHOD

    ajax_submit_modal : function( ) { //when a form is submitted within a modal this handles the ajax request and response

      $('#fancybox-content .form-submit').click(function() {
         var href = 'validate/' + $(this).parents('form').attr('id').replace('-','_').replace('-form','');
          $.ajax({
            url: href+'?mod=1',
            context: $('#fancybox-content'),
            success: function(data){
              $(this).html(data);
            }
          });

          return false;
      });

    }, //END OF METHOD

    bind_close_fancybox : function() { //TODO: REVIEW => WHAT IS THAT? /WHY METHOD

      $('.fancy-close').click( function() {
        $.fancybox.close();
      });
      return false;

    }, //END OF METHOD

    card_conditionals : function() { //TODO: REVIEW => when maestro card show issue number, start date else dont -> VALIDATE.JS RULE?

      if($('#edit-card-type').length > 0 ) {
        $('#edit-card-type').change(function() {

          $('#edit-card-start-date-wrapper').hide();
          $('#edit-card-issue-wrapper').hide();

          if( $(this).val() == 'VisaDebit' || $(this).val() == 'MasterCardDebit' || $(this).val() == 'Maestro' ) {
            $('#edit-card-start-date-wrapper').show();
          }

          if( $(this).val() == 'Maestro' ) {
            $('#edit-card-issue-wrapper').show();
          }

        });
      }

    }, //END OF METHOD

    fancybox_options : function() {

      return {'titleShow'       : false,
              'autoScale'       : true,
              'autoDimensions'  : true,
              'margin'          : '50px',
              'scrolling'       : 'yes',
              'centerOnScroll'  : true,
              'onComplete'      : function() {
                //$().conditional_elements();
                $("#fancybox-wrap").easydrag();
                $("#fancybox-wrap").setHandler('h1');
                Drupal.attachBehaviors($('#fancybox-wrap'));

                //TODO I've disabled the line below because we're now calling Drupal.attachBehaviors -
                //need to confirm that commenting this out doesn't break anything - DGS
                //Drupal.Ajax.init();
                //
                //TODO - this should be called by Drupal.attachBehaviors - there's probably a problem with the js here - DGS
                // Function initialzation in validator_config_rules_and_messages.js
                if (typeof(Drupal.behaviors.wongaValidateAllForms) === "function") {
                  Drupal.behaviors.wongaValidateAllForms();
                }

                $().wonga('simple_ajax_links');
                $().wonga('bind_close_fancybox');

                $().wonga('card_conditionals');
                $().wonga('tool_tips');

              },
              'onClosed'        : function() {
              //remove the ajax response marker
              $('.ajax-clicked').removeClass('ajax-clicked');
              $('.ajax-delete').removeClass('ajax-delete');
              }
            }
    }, //END OF METHOD

    bind_modals : function() { //binds popup modals to all anchors with a class of mod
      var options = $().wonga('fancybox_options');
      $('.mod').fancybox( options );
      // Reload pages when the .mod link has a .reload-on-close class
      var options_reload_on_close = options;
      options_reload_on_close['onClosed'] = function() {
        parent.location.reload(true);
      };

      $('.mod.reload-on-close').fancybox( options_reload_on_close );

      //add iframe popup ability
      options['type'] = 'iframe';
      $('.mod-if').fancybox( options );

    }, //END OF METHOD

    ie6_png_fix : function() { //png fixes for ie6 allows use of transparent pngs

      $('#logo').supersleight();
      $('div.form-button-wrapper').supersleight();
      $('#edit-min-cash').supersleight();
      $('#awards').supersleight();

    }, //END OF METHOD

    add_multiples : function() { //TODO: WTF?

      $('#edit-add').click( function() {

        var director = $('#director').clone();
        director.attr('id','director-0');
        $('#edit-title-wrapper',director).attr('id','edit-title-wrapper-0')
        $('#edit-title',director).attr('id','edit-title-0')
        $('#edit-title-0',director).attr('name','title_0');

        $('#edit-first-name-wrapper',director).attr('id','edit-first-name-wrapper-0')
        $('#edit-first-name',director).attr('id','edit-first-name-0')
        $('#edit-first-name-0',director).attr('name','first_name_0');

        $('#edit-last-name-wrapper',director).attr('id','edit-last-name-wrapper-0')
        $('#edit-last-name',director).attr('id','edit-last-name-0')
        $('#edit-last-name-0',director).attr('name','last_name_0');

        $('#edit-email-wrapper',director).attr('id','edit-email-wrapper-0')
        $('#edit-email',director).attr('id','edit-email-0')
        $('#edit-email-0',director).attr('name','email_0');

        $('#edit-email-confirm-wrapper',director).attr('id','edit-email-confirm-wrapper-0')
        $('#edit-email-confirm',director).attr('id','edit-email-0')
        $('#edit-email-confirm-0',director).attr('name','email_confirm_0');

        $('#director').after(director);
        return false;

      });

    }, //END OF METHOD

    agreement_scroll : function() { //scrolls the agreement form to the bottom so the user can see the acccept button

      if($('#agreementscroll').length > 0)
      {
        var agreementdiv = document.getElementById('agreementscroll');

        if(Drupal.settings.ca_valid) {
          switch (Drupal.settings.ca_valid.province) {
            case 'ON' :agreementdiv.scrollTop = 600;break;
            case 'AB' :agreementdiv.scrollTop = 1700;break;
            case 'BC' :agreementdiv.scrollTop = 600;break;
          }
        }
      }

    }

}; //METHODS

})(jQuery);

if(typeof(console) === 'undefined') {
  var console = {}
  console.log = console.error = console.info = console.debug = console.warn = console.trace = console.dir = console.dirxml = console.group = console.groupEnd = console.time = console.timeEnd = console.assert = console.profile = function() {};
}

function changeFaq()
{
  var listQuestions = document.getElementById("listQuestions");
  var id = listQuestions.value;
  var containerAnswer = document.getElementById("containerAnswer");
  var divAnswer = document.getElementById("question_"+id);
  var s = divAnswer.innerHTML;
  containerAnswer.innerHTML = s;
}

window.onload = (function(){
{
  $("#seo-show-hide").click(function () {
    $("#seo-show-hide").fadeOut('slow', function() {
    // Animation complete.
    });
  });

}});


function zeroFill(ele, len) {

    var result = ele.toString();
    var pad = len - result.length;

    while(pad > 0) {
      result = '0' + result;
      pad--;
    }

    return result;
}

//TODO: COULD BE DUPLICATE, REMOVE ONCE ALL REGIONS USE SLIDERS2
function wonga_format_currency(amount, no_symbol, decs){
  var return_value = 0;

  if( Drupal.settings.wonga.country_code == 'UK-WB' ) {
    var tmp = $('<input />').val(amount);

    if( !decs ) {
      decs = 0;
    }

    if( no_symbol ) {
      return_value = tmp.formatCurrency({
        roundToDecimalPlace : decs,
        symbol : ''
      }).val();
    } else {
      return_value = tmp.formatCurrency({
        region : 'en-GB',
        roundToDecimalPlace : decs
      }).val();
    }

  }
  else {

    var i = parseFloat(amount);
    if(isNaN(i)) {
      i = 0.00;
    }
    var minus = (i < 0)?'-':'';
    i = Math.abs(i);
    i = parseInt((i + .005) * 100);
    i = i / 100;

    // If the resultant number is an integer, check if we need to add on any
    // decimal places:
    s = new String(i);
    if ((s.indexOf('.') < 0) && (decs > 0)) {
      s += '.' + Array(decs + 1).join("0");
    }
    // If somehow we ended up with a number such as 234.0, add another 0:'
    if((s.length > 2) && (s.indexOf('.') == (s.length - 2))) {
      s += '0';
    }

    //check the decimal seperator
    if(Drupal.settings.wonga.sliders_common != null){
      if(Drupal.settings.wonga.sliders_common.decimal != '.'){
        s = s.replace('.',Drupal.settings.wonga.sliders_common.decimal);
      }
    }

    s = minus + s;

    if( no_symbol ) {
      return_value = s;
    } else {
      if (Drupal.settings.wonga.currencyside=='left')
        return_value = Drupal.settings.wonga.currencysign+s;

      if (Drupal.settings.wonga.currencyside=='right')
        return_value = s+Drupal.settings.wonga.currencysign;
    }
  }

  return return_value;
}

/**
 * Drupal behavior to initialise the wongaTabs tabs.
 *
 * Triggers a wongaCoreTabs.loaded event.
 *
 * @param {Object} context
 */
Drupal.behaviors.secciTabs = function (context) {
  if ($(".wongaTabs ul", context).length) {
    var tabs = $(".wongaTabs ul", context).tabs(".panes > div").data("tabs");
    $(document).trigger('wongaCoreTabs.loaded', [tabs, context]);
  }
};
;
//generic javascript for wonga.com
(function($){


    var width = 0;
    var old_width = 0;
    var max_width = 0;
    var id = 1;
    var plus_limit = 7;
    var min_limit = 7;
    var pointers = [];

	var methods = {
		    init : function( options ) {

                //for each element with this class mark it up with day plus min stuff
                $('.dayplusmin').each( function() {
                    $(this).css('white-space','nowrap');

                    //add new markup
                    $(this).html('<div id="dpm-' + id + '" class=\"dpm-wrap\"><a class=\"dpm-min\"></a><div class=\"dpm-date\">'+$(this).html()+'</div><a class="dpm-plus"></a></div>');

                    //get the width of the date area, store max width so we can make all dpm-date elements same width
                    width = parseInt($('.dpm-date',this).css('width').replace('px.html'));
                    if( width > max_width ) {
                        max_width = width;
                    }

                    //setup the day pointer for this id
                    pointers[id] = (plus_limit + min_limit) / 2;
                    
                    //increment the id, we id each dpm-wrap element which contains the day plus add markup
                    id++;
                });

                //todo
                //reduce markup

                //set all date widths to the same
                $('.dpm-date').css('width',max_width+'px');

                max_width = max_width + 56; //width of wrapper including buttons and pading,
                // 15px + 15px width of buttons
                //padding on dpm-date 20px + borders 6px
                $('.dpm-wrap').css('width',max_width + 'px');

                $('.dpm-plus').click( function() {

                    //get the id for this dpm-wrap
                    var id = $(this).parents('.dpm-wrap').attr('id').replace('dpm-','');

                    //add a day, if we are within the limits
                    if( pointers[id] < (plus_limit + min_limit) ) {
                        $(this).prev('.dpm-date').dayplusmin('date_days',1);

                        //increment pointer
                        pointers[id]++;
                    }

                    console.log(pointers[id]);
                } );

                 $('.dpm-min').click( function() {

                    //get the id for this dpm-wrap
                    var id = $(this).parents('.dpm-wrap').attr('id').replace('dpm-','');

                    //minus a day, if we are within the limits
                    if( pointers[id] > 0 ) {
                        $(this).next('.dpm-date').dayplusmin('date_days',-1);

                        //increment pointer
                        pointers[id]--;
                    }

                    console.log(pointers[id]);
                } );

			},

            date_days : function(days) {

            var d = new Date($(this).html());
            var new_date = '';
            var t=Date.parse( $(this).html() );

            //add a day to the date
            d.setDate(d.getDate() + days);

            new_date += d.getDate();
            
            switch(d.getMonth()) {
                case 0:
                    new_date += ' Jan ';
                    break;
                case 1:
                    new_date += ' Feb ';
                    break;
                case 2:
                    new_date += ' March ';
                    break;
                case 3:
                    new_date += ' April ';
                    break;
                case 4:
                    new_date += ' May ';
                    break;
                case 5:
                    new_date += ' June ';
                    break;
                case 6:
                    new_date += ' July ';
                    break;
                case 7:
                    new_date += ' Aug ';
                    break;
                case 8:
                    new_date += ' Sept ';
                    break;
                case 9:
                    new_date += ' Oct ';
                    break;
                case 10:
                    new_date += ' Nov ';
                    break;
                case 11:
                    new_date += ' Dec ';
                    break;
            }


            new_date += d.getFullYear();
            $(this).html(new_date);
        }


    };

	$.fn.dayplusmin = function(method){

		// Method calling logic
	    if ( methods[method] ) {
	      return methods[ method ].apply( this, Array.prototype.slice.call( arguments, 1 ));
	    } else if ( typeof method === 'object' || ! method ) {
	      return methods.init.apply( this, arguments );
	    } else {
	      $.error( 'Method ' +  method + ' does not exist on jQuery.tooltip' );
	    }
	};

})(jQuery);
;
/**
* EasyDrag 1.5 - Drag & Drop jQuery Plug-in
*
* Thanks for the community that is helping the improvement
* of this little piece of code.
*
* For usage instructions please visit http://fromvega.com/scripts
*/

(function($){

	// to track if the mouse button is pressed
	var isMouseDown    = false;

	// to track the current element being dragged
	var currentElement = null;

	// callback holders
	var dropCallbacks = {};
	var dragCallbacks = {};
	
	// bubbling status
	var bubblings = {};

	// global position records
	var lastMouseX;
	var lastMouseY;
	var lastElemTop;
	var lastElemLeft;
	
	// track element dragStatus
	var dragStatus = {};	

	// if user is holding any handle or not
	var holdingHandler = false;

	// returns the mouse (cursor) current position
	$.getMousePosition = function(e){
		var posx = 0;
		var posy = 0;

		if (!e) var e = window.event;

		if (e.pageX || e.pageY) {
			posx = e.pageX;
			posy = e.pageY;
		}
		else if (e.clientX || e.clientY) {
			posx = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
			posy = e.clientY + document.body.scrollTop  + document.documentElement.scrollTop;
		}

		return { 'x': posx, 'y': posy };
	};

	// updates the position of the current element being dragged
	$.updatePosition = function(e) {
		var pos = $.getMousePosition(e);

		var spanX = (pos.x - lastMouseX);
		var spanY = (pos.y - lastMouseY);

		$(currentElement).css("top",  (lastElemTop + spanY));
		$(currentElement).css("left", (lastElemLeft + spanX));
	};

	// when the mouse is moved while the mouse button is pressed
	$(document).mousemove(function(e){
		if(isMouseDown && dragStatus[currentElement.id] != 'false'){
			// update the position and call the registered function
			$.updatePosition(e);
			if(dragCallbacks[currentElement.id] != undefined){
				dragCallbacks[currentElement.id](e, currentElement);
			}

			return false;
		}
	});

	// when the mouse button is released
	$(document).mouseup(function(e){
		if(isMouseDown && dragStatus[currentElement.id] != 'false'){
			isMouseDown = false;
			if(dropCallbacks[currentElement.id] != undefined){
				dropCallbacks[currentElement.id](e, currentElement);
			}

			return false;
		}
	});

	// register the function to be called while an element is being dragged
	$.fn.ondrag = function(callback){
		return this.each(function(){
			dragCallbacks[this.id] = callback;
		});
	};

	// register the function to be called when an element is dropped
	$.fn.ondrop = function(callback){
		return this.each(function(){
			dropCallbacks[this.id] = callback;
		});
	};
	
	// disable the dragging feature for the element
	$.fn.dragOff = function(){
		return this.each(function(){
			dragStatus[this.id] = 'off';
		});
	};
	
	// enable the dragging feature for the element
	$.fn.dragOn = function(){
		return this.each(function(){
			dragStatus[this.id] = 'on';
		});
	};
	
	// set a child element as a handler
	$.fn.setHandler = function(handlerId){
		return this.each(function(){
			var draggable = this;
			
			// enable event bubbling so the user can reach the handle
			bubblings[this.id] = true;
			
			// reset cursor style
			$(draggable).css("cursor", "");
			
			// set current drag status
			dragStatus[draggable.id] = "handler";

			// change handle cursor type
			$(handlerId).css("cursor", "move");	
			
			// bind event handler
			$(handlerId).mousedown(function(e){
				holdingHandler = true;
				$(draggable).trigger('mousedown', e);
			});
			
			// bind event handler
			$(handlerId).mouseup(function(e){
				holdingHandler = false;
			});
		});
	}

	// set an element as draggable - allowBubbling enables/disables event bubbling
	$.fn.easydrag = function(allowBubbling){

		return this.each(function(){

			// if no id is defined assign a unique one
			if(undefined == this.id || !this.id.length) this.id = "easydrag"+(new Date().getTime());
			
			// save event bubbling status
			bubblings[this.id] = allowBubbling ? true : false;

			// set dragStatus 
			dragStatus[this.id] = "on";
			
			// change the mouse pointer
			$(this).css("cursor", "move");

			// when an element receives a mouse press
			$(this).mousedown(function(e){
				
				// just when "on" or "handler"
				if((dragStatus[this.id] == "off") || (dragStatus[this.id] == "handler" && !holdingHandler))
					return bubblings[this.id];

				// set it as absolute positioned
				$(this).css("position", "absolute");

				// set z-index
				$(this).css("z-index", parseInt( new Date().getTime()/1000 ));

				// update track variables
				isMouseDown    = true;
				currentElement = this;

				// retrieve positioning properties
				var pos    = $.getMousePosition(e);
				lastMouseX = pos.x;
				lastMouseY = pos.y;

				lastElemTop  = this.offsetTop;
				lastElemLeft = this.offsetLeft;

				$.updatePosition(e);

				return bubblings[this.id];
			});
		});
	};

})(jQuery);;
(function(d){d.formatCurrency={};d.formatCurrency.regions=[];d.formatCurrency.regions[""]={symbol:"$",positiveFormat:"%s%n",negativeFormat:"(%s%n)",decimalSymbol:".",digitGroupSymbol:",",groupDigits:true};
d.fn.formatCurrency=function(e,f){if(arguments.length==1&&typeof e!=="string"){f=e;e=false}var g={name:"formatCurrency",colorize:false,region:"",global:true,roundToDecimalPlace:2,eventOnDecimalsEntered:false};
g=d.extend(g,d.formatCurrency.regions[""]);f=d.extend(g,f);if(f.region.length>0){f=d.extend(f,b(f.region))}f.regex=a(f);return this.each(function(){$this=d(this);
var o="0";o=$this[$this.is("input, select, textarea")?"val":"html"]();if(o.search("\\(")>=0){o="-"+o}if(o===""||(o==="-"&&f.roundToDecimalPlace===-1)){return
}if(isNaN(o)){o=o.replace(f.regex,"");if(o===""||(o==="-"&&f.roundToDecimalPlace===-1)){return}if(f.decimalSymbol!="."){o=o.replace(f.decimalSymbol,".")
}if(isNaN(o)){o="0"}}var m=String(o).split(".");var r=(o==Math.abs(o));var l=(m.length>1);var k=(l?m[1].toString():"0");var j=k;o=Math.abs(m[0]);
o=isNaN(o)?0:o;if(f.roundToDecimalPlace>=0){k=parseFloat("1."+k);k=k.toFixed(f.roundToDecimalPlace);if(k.substring(0,1)=="2"){o=Number(o)+1}k=k.substring(2)
}o=String(o);if(f.groupDigits){for(var n=0;n<Math.floor((o.length-(1+n))/3);n++){o=o.substring(0,o.length-(4*n+3))+f.digitGroupSymbol+o.substring(o.length-(4*n+3))
}}if((l&&f.roundToDecimalPlace==-1)||f.roundToDecimalPlace>0){o+=f.decimalSymbol+k}var q=r?f.positiveFormat:f.negativeFormat;var h=q.replace(/%s/g,f.symbol);
h=h.replace(/%n/g,o);var p=d([]);if(!e){p=$this}else{p=d(e)}p[p.is("input, select, textarea")?"val":"html"](h);if(l&&f.eventOnDecimalsEntered&&j.length>f.roundToDecimalPlace){p.trigger("decimalsEntered",j)
}if(f.colorize){p.css("color",r?"black":"red")}})};d.fn.toNumber=function(e){var f=d.extend({name:"toNumber",region:"",global:true},d.formatCurrency.regions[""]);
e=jQuery.extend(f,e);if(e.region.length>0){e=d.extend(e,b(e.region))}e.regex=a(e);return this.each(function(){var g=d(this).is("input, select, textarea")?"val":"html";
d(this)[g](d(this)[g]().replace("(","(-").replace(e.regex,""))})};d.fn.asNumber=function(f){var g=d.extend({name:"asNumber",region:"",parse:true,parseType:"Float",global:true},d.formatCurrency.regions[""]);
f=jQuery.extend(g,f);if(f.region.length>0){f=d.extend(f,b(f.region))}f.regex=a(f);f.parseType=c(f.parseType);var h=d(this).is("input, select, textarea")?"val":"html";
var e=d(this)[h]();e=e?e:"";e=e.replace("(","(-");e=e.replace(f.regex,"");if(!f.parse){return e}if(e.length==0){e="0"}if(f.decimalSymbol!="."){e=e.replace(f.decimalSymbol,".")
}return window["parse"+f.parseType](e)};function b(g){var f=d.formatCurrency.regions[g];if(f){return f}else{if(/(\w+)-(\w+)/g.test(g)){var e=g.replace(/(\w+)-(\w+)/g,"$1");
return d.formatCurrency.regions[e]}}return null}function c(e){switch(e.toLowerCase()){case"int":return"Int";case"float":return"Float";default:throw"invalid parseType"
}}function a(e){if(e.symbol===""){return new RegExp("[^\\d"+e.decimalSymbol+"-]","g")}else{var f=e.symbol.replace("$","\\$").replace(".","\\.");
return new RegExp(f+"|[^\\d"+e.decimalSymbol+"-]","g")}}})(jQuery);;
﻿//  This file is part of the jQuery formatCurrency Plugin.
//
//    The jQuery formatCurrency Plugin is free software: you can redistribute it
//    and/or modify it under the terms of the GNU General Public License as published 
//    by the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    The jQuery formatCurrency Plugin is distributed in the hope that it will
//    be useful, but WITHOUT ANY WARRANTY; without even the implied warranty 
//    of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License along with 
//    the jQuery formatCurrency Plugin.  If not, see <http://www.gnu.org/licenses/>.

(function($) {

	$.formatCurrency.regions['af-ZA'] = {
		symbol: 'R',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['am-ET'] = {
		symbol: 'ETB',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-AE'] = {
		symbol: 'د.إ.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-BH'] = {
		symbol: 'د.ب.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-DZ'] = {
		symbol: 'د.ج.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-EG'] = {
		symbol: 'ج.م.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-IQ'] = {
		symbol: 'د.ع.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-JO'] = {
		symbol: 'د.ا.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-KW'] = {
		symbol: 'د.ك.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-LB'] = {
		symbol: 'ل.ل.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-LY'] = {
		symbol: 'د.ل.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-MA'] = {
		symbol: 'د.م.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-OM'] = {
		symbol: 'ر.ع.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-QA'] = {
		symbol: 'ر.ق.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-SA'] = {
		symbol: 'ر.س.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-SY'] = {
		symbol: 'ل.س.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-TN'] = {
		symbol: 'د.ت.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ar-YE'] = {
		symbol: 'ر.ي.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['arn-CL'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['as-IN'] = {
		symbol: 'ট',
		positiveFormat: '%n%s',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['az-Cyrl-AZ'] = {
		symbol: 'ман.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['az-Latn-AZ'] = {
		symbol: 'man.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['ba-RU'] = {
		symbol: 'һ.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['be-BY'] = {
		symbol: 'р.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['bg-BG'] = {
		symbol: 'лв',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['bn-BD'] = {
		symbol: '৳',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['bn-IN'] = {
		symbol: 'টা',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['bo-CN'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['br-FR'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['bs-Cyrl-BA'] = {
		symbol: 'КМ',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['bs-Latn-BA'] = {
		symbol: 'KM',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['ca-ES'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['co-FR'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['cs-CZ'] = {
		symbol: 'Kč',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['cy-GB'] = {
		symbol: '£',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['da-DK'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['de-AT'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['de-CH'] = {
		symbol: 'SFr.',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: '\'',
		groupDigits: true
	};

	$.formatCurrency.regions['de-DE'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['de-LI'] = {
		symbol: 'CHF',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: '\'',
		groupDigits: true
	};

	$.formatCurrency.regions['de-LU'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['de'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['dsb-DE'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['dv-MV'] = {
		symbol: 'ރ.',
		positiveFormat: '%n %s',
		negativeFormat: '%n %s-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['el-GR'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['en-029'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-AU'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-BZ'] = {
		symbol: 'BZ$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-CA'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-GB'] = {
		symbol: '£',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-IE'] = {
		symbol: '€',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-IN'] = {
		symbol: 'Rs.',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-JM'] = {
		symbol: 'J$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-MY'] = {
		symbol: 'RM',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-NZ'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-PH'] = {
		symbol: 'Php',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-SG'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-TT'] = {
		symbol: 'TT$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-US'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-ZA'] = {
		symbol: 'R',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['en-ZW'] = {
		symbol: 'Z$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-AR'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-BO'] = {
		symbol: '$b',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-CL'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-CO'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-CR'] = {
		symbol: '₡',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-DO'] = {
		symbol: 'RD$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-EC'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-ES'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-GT'] = {
		symbol: 'Q',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-HN'] = {
		symbol: 'L.',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-MX'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-NI'] = {
		symbol: 'C$',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-PA'] = {
		symbol: 'B/.',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-PE'] = {
		symbol: 'S/.',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-PR'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-PY'] = {
		symbol: 'Gs',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-SV'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-US'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['es-UY'] = {
		symbol: '$U',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es-VE'] = {
		symbol: 'Bs',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['es'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['et-EE'] = {
		symbol: 'kr',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: '.',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['eu-ES'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['fa-IR'] = {
		symbol: 'ريال',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '/',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['fi-FI'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['fil-PH'] = {
		symbol: 'PhP',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['fo-FO'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['fr-BE'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['fr-CA'] = {
		symbol: '$',
		positiveFormat: '%n %s',
		negativeFormat: '(%n %s)',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['fr-CH'] = {
		symbol: 'SFr.',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: '\'',
		groupDigits: true
	};

	$.formatCurrency.regions['fr-FR'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['fr-LU'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['fr-MC'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['fr'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['fy-NL'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['ga-IE'] = {
		symbol: '€',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['gl-ES'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['gsw-FR'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['gu-IN'] = {
		symbol: 'રૂ',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ha-Latn-NG'] = {
		symbol: 'N',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['he-IL'] = {
		symbol: '₪',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['hi-IN'] = {
		symbol: 'रु',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['hr-BA'] = {
		symbol: 'KM',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['hr-HR'] = {
		symbol: 'kn',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['hsb-DE'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['hu-HU'] = {
		symbol: 'Ft',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['hy-AM'] = {
		symbol: 'դր.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['id-ID'] = {
		symbol: 'Rp',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['ig-NG'] = {
		symbol: 'N',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ii-CN'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['is-IS'] = {
		symbol: 'kr.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['it-CH'] = {
		symbol: 'SFr.',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: '\'',
		groupDigits: true
	};

	$.formatCurrency.regions['it-IT'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['it'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['iu-Cans-CA'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['iu-Latn-CA'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ja-JP'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ja'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ka-GE'] = {
		symbol: 'Lari',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['kk-KZ'] = {
		symbol: 'Т',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '-',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['kl-GL'] = {
		symbol: 'kr.',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['km-KH'] = {
		symbol: '៛',
		positiveFormat: '%n%s',
		negativeFormat: '-%n%s',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['kn-IN'] = {
		symbol: 'ರೂ',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ko-KR'] = {
		symbol: '₩',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['kok-IN'] = {
		symbol: 'रु',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ky-KG'] = {
		symbol: 'сом',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: '-',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['lb-LU'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['lo-LA'] = {
		symbol: '₭',
		positiveFormat: '%n%s',
		negativeFormat: '(%n%s)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['lt-LT'] = {
		symbol: 'Lt',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['lv-LV'] = {
		symbol: 'Ls',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['mi-NZ'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['mk-MK'] = {
		symbol: 'ден.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['ml-IN'] = {
		symbol: 'ക',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['mn-MN'] = {
		symbol: '₮',
		positiveFormat: '%n%s',
		negativeFormat: '-%n%s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['mn-Mong-CN'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['moh-CA'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['mr-IN'] = {
		symbol: 'रु',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ms-BN'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['ms-MY'] = {
		symbol: 'R',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['mt-MT'] = {
		symbol: 'Lm',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['nb-NO'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['ne-NP'] = {
		symbol: 'रु',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['nl-BE'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['nl-NL'] = {
		symbol: '€',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['nn-NO'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['nso-ZA'] = {
		symbol: 'R',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['oc-FR'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['or-IN'] = {
		symbol: 'ଟ',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['pa-IN'] = {
		symbol: 'ਰੁ',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['pl-PL'] = {
		symbol: 'zł',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['prs-AF'] = {
		symbol: '؋',
		positiveFormat: '%s%n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ps-AF'] = {
		symbol: '؋',
		positiveFormat: '%s%n',
		negativeFormat: '%s%n-',
		decimalSymbol: '٫',
		digitGroupSymbol: '٬',
		groupDigits: true
	};

	$.formatCurrency.regions['pt-BR'] = {
		symbol: 'R$',
		positiveFormat: '%s %n',
		negativeFormat: '-%s %n',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['pt-PT'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['qut-GT'] = {
		symbol: 'Q',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['quz-BO'] = {
		symbol: '$b',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['quz-EC'] = {
		symbol: '$',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['quz-PE'] = {
		symbol: 'S/.',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['rm-CH'] = {
		symbol: 'fr.',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: '\'',
		groupDigits: true
	};

	$.formatCurrency.regions['ro-RO'] = {
		symbol: 'lei',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['ru-RU'] = {
		symbol: 'р.',
		positiveFormat: '%n%s',
		negativeFormat: '-%n%s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['rw-RW'] = {
		symbol: 'RWF',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['sa-IN'] = {
		symbol: 'रु',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['sah-RU'] = {
		symbol: 'с.',
		positiveFormat: '%n%s',
		negativeFormat: '-%n%s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['se-FI'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['se-NO'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['se-SE'] = {
		symbol: 'kr',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['si-LK'] = {
		symbol: 'රු.',
		positiveFormat: '%s %n',
		negativeFormat: '(%s %n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['sk-SK'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['sl-SI'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sma-NO'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['sma-SE'] = {
		symbol: 'kr',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['smj-NO'] = {
		symbol: 'kr',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['smj-SE'] = {
		symbol: 'kr',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['smn-FI'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['sms-FI'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['sq-AL'] = {
		symbol: 'Lek',
		positiveFormat: '%n%s',
		negativeFormat: '-%n%s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sr-Cyrl-BA'] = {
		symbol: 'КМ',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sr-Cyrl-CS'] = {
		symbol: 'Дин.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sr-Latn-BA'] = {
		symbol: 'KM',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sr-Latn-CS'] = {
		symbol: 'Din.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sv-FI'] = {
		symbol: '€',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['sv-SE'] = {
		symbol: 'kr',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['sw-KE'] = {
		symbol: 'S',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['syr-SY'] = {
		symbol: 'ل.س.‏',
		positiveFormat: '%s %n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ta-IN'] = {
		symbol: 'ரூ',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['te-IN'] = {
		symbol: 'రూ',
		positiveFormat: '%s %n',
		negativeFormat: '%s -%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['tg-Cyrl-TJ'] = {
		symbol: 'т.р.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ';',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['th-TH'] = {
		symbol: '฿',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['tk-TM'] = {
		symbol: 'm.',
		positiveFormat: '%n%s',
		negativeFormat: '-%n%s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['tn-ZA'] = {
		symbol: 'R',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['tr-TR'] = {
		symbol: 'TL',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['tt-RU'] = {
		symbol: 'р.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['tzm-Latn-DZ'] = {
		symbol: 'DZD',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['ug-CN'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['uk-UA'] = {
		symbol: 'грн.',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['ur-PK'] = {
		symbol: 'Rs',
		positiveFormat: '%s%n',
		negativeFormat: '%s%n-',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['uz-Cyrl-UZ'] = {
		symbol: 'сўм',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['uz-Latn-UZ'] = {
		symbol: 'su\'m',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['vi-VN'] = {
		symbol: '₫',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: '.',
		groupDigits: true
	};

	$.formatCurrency.regions['wo-SN'] = {
		symbol: 'XOF',
		positiveFormat: '%n %s',
		negativeFormat: '-%n %s',
		decimalSymbol: ',',
		digitGroupSymbol: ' ',
		groupDigits: true
	};

	$.formatCurrency.regions['xh-ZA'] = {
		symbol: 'R',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['yo-NG'] = {
		symbol: 'N',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zh-CN'] = {
		symbol: '￥',
		positiveFormat: '%s%n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zh-HK'] = {
		symbol: 'HK$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zh-MO'] = {
		symbol: 'MOP',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zh-SG'] = {
		symbol: '$',
		positiveFormat: '%s%n',
		negativeFormat: '(%s%n)',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zh-TW'] = {
		symbol: 'NT$',
		positiveFormat: '%s%n',
		negativeFormat: '-%s%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zh'] = {
		symbol: '¥',
		positiveFormat: '%s%n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

	$.formatCurrency.regions['zu-ZA'] = {
		symbol: 'R',
		positiveFormat: '%s %n',
		negativeFormat: '%s-%n',
		decimalSymbol: '.',
		digitGroupSymbol: ',',
		groupDigits: true
	};

})(jQuery);
;
function base64_encode (data) {
    // Encodes string using MIME base64 algorithm  
    // 
    // version: 1109.2015
    // discuss at: http://phpjs.org/functions/base64_encode
    // +   original by: Tyler Akins (http://rumkin.com)
    // +   improved by: Bayron Guevara
    // +   improved by: Thunder.m
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   bugfixed by: Pellentesque Malesuada
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   improved by: Rafal Kukawski (http://kukawski.pl)
    // -    depends on: utf8_encode
    // *     example 1: base64_encode('Kevin van Zonneveld');
    // *     returns 1: 'S2V2aW4gdmFuIFpvbm5ldmVsZA=='
    // mozilla has this native
    // - but breaks in 2.0.0.12!
    //if (typeof this.window['atob'] == 'function') {
    //    return atob(data);
    //}
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
        ac = 0,
        enc = "",
        tmp_arr = [];
 
    if (!data) {
        return data;
    }
 
    data = this.utf8_encode(data + '');
 
    do { // pack three octets into four hexets
        o1 = data.charCodeAt(i++);
        o2 = data.charCodeAt(i++);
        o3 = data.charCodeAt(i++);
 
        bits = o1 << 16 | o2 << 8 | o3;
 
        h1 = bits >> 18 & 0x3f;
        h2 = bits >> 12 & 0x3f;
        h3 = bits >> 6 & 0x3f;
        h4 = bits & 0x3f;
 
        // use hexets to index into b64, and append result to encoded string
        tmp_arr[ac++] = b64.charAt(h1) + b64.charAt(h2) + b64.charAt(h3) + b64.charAt(h4);
    } while (i < data.length);
 
    enc = tmp_arr.join('');
    
    var r = data.length % 3;
    
    return (r ? enc.slice(0, r - 3) : enc) + '==='.slice(r || 3);
}

function base64_decode (data) {
    // Decodes string using MIME base64 algorithm  
    // 
    // version: 1109.2015
    // discuss at: http://phpjs.org/functions/base64_decode
    // +   original by: Tyler Akins (http://rumkin.com)
    // +   improved by: Thunder.m
    // +      input by: Aman Gupta
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   bugfixed by: Onno Marsman
    // +   bugfixed by: Pellentesque Malesuada
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +      input by: Brett Zamir (http://brett-zamir.me)
    // +   bugfixed by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // -    depends on: utf8_decode
    // *     example 1: base64_decode('S2V2aW4gdmFuIFpvbm5ldmVsZA==');
    // *     returns 1: 'Kevin van Zonneveld'
    // mozilla has this native
    // - but breaks in 2.0.0.12!
    //if (typeof this.window['btoa'] == 'function') {
    //    return btoa(data);
    //}
    var b64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
        ac = 0,
        dec = "",
        tmp_arr = [];
 
    if (!data) {
        return data;
    }
 
    data += '';
 
    do { // unpack four hexets into three octets using index points in b64
        h1 = b64.indexOf(data.charAt(i++));
        h2 = b64.indexOf(data.charAt(i++));
        h3 = b64.indexOf(data.charAt(i++));
        h4 = b64.indexOf(data.charAt(i++));
 
        bits = h1 << 18 | h2 << 12 | h3 << 6 | h4;
 
        o1 = bits >> 16 & 0xff;
        o2 = bits >> 8 & 0xff;
        o3 = bits & 0xff;
 
        if (h3 == 64) {
            tmp_arr[ac++] = String.fromCharCode(o1);
        } else if (h4 == 64) {
            tmp_arr[ac++] = String.fromCharCode(o1, o2);
        } else {
            tmp_arr[ac++] = String.fromCharCode(o1, o2, o3);
        }
    } while (i < data.length);
 
    dec = tmp_arr.join('');
    dec = this.utf8_decode(dec);
 
    return dec;
}


function utf8_encode (argString) {
    // Encodes an ISO-8859-1 string to UTF-8  
    // 
    // version: 1109.2015
    // discuss at: http://phpjs.org/functions/utf8_encode
    // +   original by: Webtoolkit.info (http://www.webtoolkit.info/)
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   improved by: sowberry
    // +    tweaked by: Jack
    // +   bugfixed by: Onno Marsman
    // +   improved by: Yves Sucaet
    // +   bugfixed by: Onno Marsman
    // +   bugfixed by: Ulrich
    // +   bugfixed by: Rafal Kukawski
    // *     example 1: utf8_encode('Kevin van Zonneveld');
    // *     returns 1: 'Kevin van Zonneveld'
    if (argString === null || typeof argString === "undefined") {
        return "";
    }
 
    var string = (argString + ''); // .replace(/\r\n/g, "\n").replace(/\r/g, "\n");
    var utftext = "",
        start, end, stringl = 0;
 
    start = end = 0;
    stringl = string.length;
    for (var n = 0; n < stringl; n++) {
        var c1 = string.charCodeAt(n);
        var enc = null;
 
        if (c1 < 128) {
            end++;
        } else if (c1 > 127 && c1 < 2048) {
            enc = String.fromCharCode((c1 >> 6) | 192) + String.fromCharCode((c1 & 63) | 128);
        } else {
            enc = String.fromCharCode((c1 >> 12) | 224) + String.fromCharCode(((c1 >> 6) & 63) | 128) + String.fromCharCode((c1 & 63) | 128);
        }
        if (enc !== null) {
            if (end > start) {
                utftext += string.slice(start, end);
            }
            utftext += enc;
            start = end = n + 1;
        }
    }
 
    if (end > start) {
        utftext += string.slice(start, stringl);
    }
 
    return utftext;
}

function utf8_decode (str_data) {
    // Converts a UTF-8 encoded string to ISO-8859-1  
    // 
    // version: 1109.2015
    // discuss at: http://phpjs.org/functions/utf8_decode
    // +   original by: Webtoolkit.info (http://www.webtoolkit.info/)
    // +      input by: Aman Gupta
    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // +   improved by: Norman "zEh" Fuchs
    // +   bugfixed by: hitwork
    // +   bugfixed by: Onno Marsman
    // +      input by: Brett Zamir (http://brett-zamir.me)
    // +   bugfixed by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
    // *     example 1: utf8_decode('Kevin van Zonneveld');
    // *     returns 1: 'Kevin van Zonneveld'
    var tmp_arr = [],
        i = 0,
        ac = 0,
        c1 = 0,
        c2 = 0,
        c3 = 0;
 
    str_data += '';
 
    while (i < str_data.length) {
        c1 = str_data.charCodeAt(i);
        if (c1 < 128) {
            tmp_arr[ac++] = String.fromCharCode(c1);
            i++;
        } else if (c1 > 191 && c1 < 224) {
            c2 = str_data.charCodeAt(i + 1);
            tmp_arr[ac++] = String.fromCharCode(((c1 & 31) << 6) | (c2 & 63));
            i += 2;
        } else {
            c2 = str_data.charCodeAt(i + 1);
            c3 = str_data.charCodeAt(i + 2);
            tmp_arr[ac++] = String.fromCharCode(((c1 & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
            i += 3;
        }
    }
 
    return tmp_arr.join('');
}

//this.start();

;
/*
 * $Id: rawdeflate.js,v 0.3 2009/03/01 19:05:05 dankogai Exp dankogai $
 *
 * Original:
 *   http://www.onicos.com/staff/iz/amuse/javascript/expert/deflate.txt
 */

(function(){

/* Copyright (C) 1999 Masanao Izumo <iz@onicos.co.jp>
 * Version: 1.0.1
 * LastModified: Dec 25 1999
 */

/* Interface:
 * data = zip_deflate(src);
 */

/* constant parameters */
var zip_WSIZE = 32768;		// Sliding Window size
var zip_STORED_BLOCK = 0;
var zip_STATIC_TREES = 1;
var zip_DYN_TREES    = 2;

/* for deflate */
var zip_DEFAULT_LEVEL = 6;
var zip_FULL_SEARCH = true;
var zip_INBUFSIZ = 32768;	// Input buffer size
var zip_INBUF_EXTRA = 64;	// Extra buffer
var zip_OUTBUFSIZ = 1024 * 8;
var zip_window_size = 2 * zip_WSIZE;
var zip_MIN_MATCH = 3;
var zip_MAX_MATCH = 258;
var zip_BITS = 16;
// for SMALL_MEM
var zip_LIT_BUFSIZE = 0x2000;
var zip_HASH_BITS = 13;
// for MEDIUM_MEM
// var zip_LIT_BUFSIZE = 0x4000;
// var zip_HASH_BITS = 14;
// for BIG_MEM
// var zip_LIT_BUFSIZE = 0x8000;
// var zip_HASH_BITS = 15;
if(zip_LIT_BUFSIZE > zip_INBUFSIZ)
    alert("error: zip_INBUFSIZ is too small");
if((zip_WSIZE<<1) > (1<<zip_BITS))
    alert("error: zip_WSIZE is too large");
if(zip_HASH_BITS > zip_BITS-1)
    alert("error: zip_HASH_BITS is too large");
if(zip_HASH_BITS < 8 || zip_MAX_MATCH != 258)
    alert("error: Code too clever");
var zip_DIST_BUFSIZE = zip_LIT_BUFSIZE;
var zip_HASH_SIZE = 1 << zip_HASH_BITS;
var zip_HASH_MASK = zip_HASH_SIZE - 1;
var zip_WMASK = zip_WSIZE - 1;
var zip_NIL = 0; // Tail of hash chains
var zip_TOO_FAR = 4096;
var zip_MIN_LOOKAHEAD = zip_MAX_MATCH + zip_MIN_MATCH + 1;
var zip_MAX_DIST = zip_WSIZE - zip_MIN_LOOKAHEAD;
var zip_SMALLEST = 1;
var zip_MAX_BITS = 15;
var zip_MAX_BL_BITS = 7;
var zip_LENGTH_CODES = 29;
var zip_LITERALS =256;
var zip_END_BLOCK = 256;
var zip_L_CODES = zip_LITERALS + 1 + zip_LENGTH_CODES;
var zip_D_CODES = 30;
var zip_BL_CODES = 19;
var zip_REP_3_6 = 16;
var zip_REPZ_3_10 = 17;
var zip_REPZ_11_138 = 18;
var zip_HEAP_SIZE = 2 * zip_L_CODES + 1;
var zip_H_SHIFT = parseInt((zip_HASH_BITS + zip_MIN_MATCH - 1) /
			   zip_MIN_MATCH);

/* variables */
var zip_free_queue;
var zip_qhead, zip_qtail;
var zip_initflag;
var zip_outbuf = null;
var zip_outcnt, zip_outoff;
var zip_complete;
var zip_window;
var zip_d_buf;
var zip_l_buf;
var zip_prev;
var zip_bi_buf;
var zip_bi_valid;
var zip_block_start;
var zip_ins_h;
var zip_hash_head;
var zip_prev_match;
var zip_match_available;
var zip_match_length;
var zip_prev_length;
var zip_strstart;
var zip_match_start;
var zip_eofile;
var zip_lookahead;
var zip_max_chain_length;
var zip_max_lazy_match;
var zip_compr_level;
var zip_good_match;
var zip_nice_match;
var zip_dyn_ltree;
var zip_dyn_dtree;
var zip_static_ltree;
var zip_static_dtree;
var zip_bl_tree;
var zip_l_desc;
var zip_d_desc;
var zip_bl_desc;
var zip_bl_count;
var zip_heap;
var zip_heap_len;
var zip_heap_max;
var zip_depth;
var zip_length_code;
var zip_dist_code;
var zip_base_length;
var zip_base_dist;
var zip_flag_buf;
var zip_last_lit;
var zip_last_dist;
var zip_last_flags;
var zip_flags;
var zip_flag_bit;
var zip_opt_len;
var zip_static_len;
var zip_deflate_data;
var zip_deflate_pos;

/* objects (deflate) */

var zip_DeflateCT = function() {
    this.fc = 0; // frequency count or bit string
    this.dl = 0; // father node in Huffman tree or length of bit string
}

var zip_DeflateTreeDesc = function() {
    this.dyn_tree = null;	// the dynamic tree
    this.static_tree = null;	// corresponding static tree or NULL
    this.extra_bits = null;	// extra bits for each code or NULL
    this.extra_base = 0;	// base index for extra_bits
    this.elems = 0;		// max number of elements in the tree
    this.max_length = 0;	// max bit length for the codes
    this.max_code = 0;		// largest code with non zero frequency
}

/* Values for max_lazy_match, good_match and max_chain_length, depending on
 * the desired pack level (0..9). The values given below have been tuned to
 * exclude worst case performance for pathological files. Better values may be
 * found for specific files.
 */
var zip_DeflateConfiguration = function(a, b, c, d) {
    this.good_length = a; // reduce lazy search above this match length
    this.max_lazy = b;    // do not perform lazy search above this match length
    this.nice_length = c; // quit search above this match length
    this.max_chain = d;
}

var zip_DeflateBuffer = function() {
    this.next = null;
    this.len = 0;
    this.ptr = new Array(zip_OUTBUFSIZ);
    this.off = 0;
}

/* constant tables */
var zip_extra_lbits = new Array(
    0,0,0,0,0,0,0,0,1,1,1,1,2,2,2,2,3,3,3,3,4,4,4,4,5,5,5,5,0);
var zip_extra_dbits = new Array(
    0,0,0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,9,9,10,10,11,11,12,12,13,13);
var zip_extra_blbits = new Array(
    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,3,7);
var zip_bl_order = new Array(
    16,17,18,0,8,7,9,6,10,5,11,4,12,3,13,2,14,1,15);
var zip_configuration_table = new Array(
	new zip_DeflateConfiguration(0,    0,   0,    0),
	new zip_DeflateConfiguration(4,    4,   8,    4),
	new zip_DeflateConfiguration(4,    5,  16,    8),
	new zip_DeflateConfiguration(4,    6,  32,   32),
	new zip_DeflateConfiguration(4,    4,  16,   16),
	new zip_DeflateConfiguration(8,   16,  32,   32),
	new zip_DeflateConfiguration(8,   16, 128,  128),
	new zip_DeflateConfiguration(8,   32, 128,  256),
	new zip_DeflateConfiguration(32, 128, 258, 1024),
	new zip_DeflateConfiguration(32, 258, 258, 4096));


/* routines (deflate) */

var zip_deflate_start = function(level) {
    var i;

    if(!level)
	level = zip_DEFAULT_LEVEL;
    else if(level < 1)
	level = 1;
    else if(level > 9)
	level = 9;

    zip_compr_level = level;
    zip_initflag = false;
    zip_eofile = false;
    if(zip_outbuf != null)
	return;

    zip_free_queue = zip_qhead = zip_qtail = null;
    zip_outbuf = new Array(zip_OUTBUFSIZ);
    zip_window = new Array(zip_window_size);
    zip_d_buf = new Array(zip_DIST_BUFSIZE);
    zip_l_buf = new Array(zip_INBUFSIZ + zip_INBUF_EXTRA);
    zip_prev = new Array(1 << zip_BITS);
    zip_dyn_ltree = new Array(zip_HEAP_SIZE);
    for(i = 0; i < zip_HEAP_SIZE; i++)
	zip_dyn_ltree[i] = new zip_DeflateCT();
    zip_dyn_dtree = new Array(2*zip_D_CODES+1);
    for(i = 0; i < 2*zip_D_CODES+1; i++)
	zip_dyn_dtree[i] = new zip_DeflateCT();
    zip_static_ltree = new Array(zip_L_CODES+2);
    for(i = 0; i < zip_L_CODES+2; i++)
	zip_static_ltree[i] = new zip_DeflateCT();
    zip_static_dtree = new Array(zip_D_CODES);
    for(i = 0; i < zip_D_CODES; i++)
	zip_static_dtree[i] = new zip_DeflateCT();
    zip_bl_tree = new Array(2*zip_BL_CODES+1);
    for(i = 0; i < 2*zip_BL_CODES+1; i++)
	zip_bl_tree[i] = new zip_DeflateCT();
    zip_l_desc = new zip_DeflateTreeDesc();
    zip_d_desc = new zip_DeflateTreeDesc();
    zip_bl_desc = new zip_DeflateTreeDesc();
    zip_bl_count = new Array(zip_MAX_BITS+1);
    zip_heap = new Array(2*zip_L_CODES+1);
    zip_depth = new Array(2*zip_L_CODES+1);
    zip_length_code = new Array(zip_MAX_MATCH-zip_MIN_MATCH+1);
    zip_dist_code = new Array(512);
    zip_base_length = new Array(zip_LENGTH_CODES);
    zip_base_dist = new Array(zip_D_CODES);
    zip_flag_buf = new Array(parseInt(zip_LIT_BUFSIZE / 8));
}

var zip_deflate_end = function() {
    zip_free_queue = zip_qhead = zip_qtail = null;
    zip_outbuf = null;
    zip_window = null;
    zip_d_buf = null;
    zip_l_buf = null;
    zip_prev = null;
    zip_dyn_ltree = null;
    zip_dyn_dtree = null;
    zip_static_ltree = null;
    zip_static_dtree = null;
    zip_bl_tree = null;
    zip_l_desc = null;
    zip_d_desc = null;
    zip_bl_desc = null;
    zip_bl_count = null;
    zip_heap = null;
    zip_depth = null;
    zip_length_code = null;
    zip_dist_code = null;
    zip_base_length = null;
    zip_base_dist = null;
    zip_flag_buf = null;
}

var zip_reuse_queue = function(p) {
    p.next = zip_free_queue;
    zip_free_queue = p;
}

var zip_new_queue = function() {
    var p;

    if(zip_free_queue != null)
    {
	p = zip_free_queue;
	zip_free_queue = zip_free_queue.next;
    }
    else
	p = new zip_DeflateBuffer();
    p.next = null;
    p.len = p.off = 0;

    return p;
}

var zip_head1 = function(i) {
    return zip_prev[zip_WSIZE + i];
}

var zip_head2 = function(i, val) {
    return zip_prev[zip_WSIZE + i] = val;
}

/* put_byte is used for the compressed output, put_ubyte for the
 * uncompressed output. However unlzw() uses window for its
 * suffix table instead of its output buffer, so it does not use put_ubyte
 * (to be cleaned up).
 */
var zip_put_byte = function(c) {
    zip_outbuf[zip_outoff + zip_outcnt++] = c;
    if(zip_outoff + zip_outcnt == zip_OUTBUFSIZ)
	zip_qoutbuf();
}

/* Output a 16 bit value, lsb first */
var zip_put_short = function(w) {
    w &= 0xffff;
    if(zip_outoff + zip_outcnt < zip_OUTBUFSIZ - 2) {
	zip_outbuf[zip_outoff + zip_outcnt++] = (w & 0xff);
	zip_outbuf[zip_outoff + zip_outcnt++] = (w >>> 8);
    } else {
	zip_put_byte(w & 0xff);
	zip_put_byte(w >>> 8);
    }
}

/* ==========================================================================
 * Insert string s in the dictionary and set match_head to the previous head
 * of the hash chain (the most recent string with same hash key). Return
 * the previous length of the hash chain.
 * IN  assertion: all calls to to INSERT_STRING are made with consecutive
 *    input characters and the first MIN_MATCH bytes of s are valid
 *    (except for the last MIN_MATCH-1 bytes of the input file).
 */
var zip_INSERT_STRING = function() {
    zip_ins_h = ((zip_ins_h << zip_H_SHIFT)
		 ^ (zip_window[zip_strstart + zip_MIN_MATCH - 1] & 0xff))
	& zip_HASH_MASK;
    zip_hash_head = zip_head1(zip_ins_h);
    zip_prev[zip_strstart & zip_WMASK] = zip_hash_head;
    zip_head2(zip_ins_h, zip_strstart);
}

/* Send a code of the given tree. c and tree must not have side effects */
var zip_SEND_CODE = function(c, tree) {
    zip_send_bits(tree[c].fc, tree[c].dl);
}

/* Mapping from a distance to a distance code. dist is the distance - 1 and
 * must not have side effects. dist_code[256] and dist_code[257] are never
 * used.
 */
var zip_D_CODE = function(dist) {
    return (dist < 256 ? zip_dist_code[dist]
	    : zip_dist_code[256 + (dist>>7)]) & 0xff;
}

/* ==========================================================================
 * Compares to subtrees, using the tree depth as tie breaker when
 * the subtrees have equal frequency. This minimizes the worst case length.
 */
var zip_SMALLER = function(tree, n, m) {
    return tree[n].fc < tree[m].fc ||
      (tree[n].fc == tree[m].fc && zip_depth[n] <= zip_depth[m]);
}

/* ==========================================================================
 * read string data
 */
var zip_read_buff = function(buff, offset, n) {
    var i;
    for(i = 0; i < n && zip_deflate_pos < zip_deflate_data.length; i++)
	buff[offset + i] =
	    zip_deflate_data.charCodeAt(zip_deflate_pos++) & 0xff;
    return i;
}

/* ==========================================================================
 * Initialize the "longest match" routines for a new file
 */
var zip_lm_init = function() {
    var j;

    /* Initialize the hash table. */
    for(j = 0; j < zip_HASH_SIZE; j++)
//	zip_head2(j, zip_NIL);
	zip_prev[zip_WSIZE + j] = 0;
    /* prev will be initialized on the fly */

    /* Set the default configuration parameters:
     */
    zip_max_lazy_match = zip_configuration_table[zip_compr_level].max_lazy;
    zip_good_match     = zip_configuration_table[zip_compr_level].good_length;
    if(!zip_FULL_SEARCH)
	zip_nice_match = zip_configuration_table[zip_compr_level].nice_length;
    zip_max_chain_length = zip_configuration_table[zip_compr_level].max_chain;

    zip_strstart = 0;
    zip_block_start = 0;

    zip_lookahead = zip_read_buff(zip_window, 0, 2 * zip_WSIZE);
    if(zip_lookahead <= 0) {
	zip_eofile = true;
	zip_lookahead = 0;
	return;
    }
    zip_eofile = false;
    /* Make sure that we always have enough lookahead. This is important
     * if input comes from a device such as a tty.
     */
    while(zip_lookahead < zip_MIN_LOOKAHEAD && !zip_eofile)
	zip_fill_window();

    /* If lookahead < MIN_MATCH, ins_h is garbage, but this is
     * not important since only literal bytes will be emitted.
     */
    zip_ins_h = 0;
    for(j = 0; j < zip_MIN_MATCH - 1; j++) {
//      UPDATE_HASH(ins_h, window[j]);
	zip_ins_h = ((zip_ins_h << zip_H_SHIFT) ^ (zip_window[j] & 0xff)) & zip_HASH_MASK;
    }
}

/* ==========================================================================
 * Set match_start to the longest match starting at the given string and
 * return its length. Matches shorter or equal to prev_length are discarded,
 * in which case the result is equal to prev_length and match_start is
 * garbage.
 * IN assertions: cur_match is the head of the hash chain for the current
 *   string (strstart) and its distance is <= MAX_DIST, and prev_length >= 1
 */
var zip_longest_match = function(cur_match) {
    var chain_length = zip_max_chain_length; // max hash chain length
    var scanp = zip_strstart; // current string
    var matchp;		// matched string
    var len;		// length of current match
    var best_len = zip_prev_length;	// best match length so far

    /* Stop when cur_match becomes <= limit. To simplify the code,
     * we prevent matches with the string of window index 0.
     */
    var limit = (zip_strstart > zip_MAX_DIST ? zip_strstart - zip_MAX_DIST : zip_NIL);

    var strendp = zip_strstart + zip_MAX_MATCH;
    var scan_end1 = zip_window[scanp + best_len - 1];
    var scan_end  = zip_window[scanp + best_len];

    /* Do not waste too much time if we already have a good match: */
    if(zip_prev_length >= zip_good_match)
	chain_length >>= 2;

//  Assert(encoder->strstart <= window_size-MIN_LOOKAHEAD, "insufficient lookahead");

    do {
//    Assert(cur_match < encoder->strstart, "no future");
	matchp = cur_match;

	/* Skip to next match if the match length cannot increase
	    * or if the match length is less than 2:
	*/
	if(zip_window[matchp + best_len]	!= scan_end  ||
	   zip_window[matchp + best_len - 1]	!= scan_end1 ||
	   zip_window[matchp]			!= zip_window[scanp] ||
	   zip_window[++matchp]			!= zip_window[scanp + 1]) {
	    continue;
	}

	/* The check at best_len-1 can be removed because it will be made
         * again later. (This heuristic is not always a win.)
         * It is not necessary to compare scan[2] and match[2] since they
         * are always equal when the other bytes match, given that
         * the hash keys are equal and that HASH_BITS >= 8.
         */
	scanp += 2;
	matchp++;

	/* We check for insufficient lookahead only every 8th comparison;
         * the 256th check will be made at strstart+258.
         */
	do {
	} while(zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		zip_window[++scanp] == zip_window[++matchp] &&
		scanp < strendp);

      len = zip_MAX_MATCH - (strendp - scanp);
      scanp = strendp - zip_MAX_MATCH;

      if(len > best_len) {
	  zip_match_start = cur_match;
	  best_len = len;
	  if(zip_FULL_SEARCH) {
	      if(len >= zip_MAX_MATCH) break;
	  } else {
	      if(len >= zip_nice_match) break;
	  }

	  scan_end1  = zip_window[scanp + best_len-1];
	  scan_end   = zip_window[scanp + best_len];
      }
    } while((cur_match = zip_prev[cur_match & zip_WMASK]) > limit
	    && --chain_length != 0);

    return best_len;
}

/* ==========================================================================
 * Fill the window when the lookahead becomes insufficient.
 * Updates strstart and lookahead, and sets eofile if end of input file.
 * IN assertion: lookahead < MIN_LOOKAHEAD && strstart + lookahead > 0
 * OUT assertions: at least one byte has been read, or eofile is set;
 *    file reads are performed for at least two bytes (required for the
 *    translate_eol option).
 */
var zip_fill_window = function() {
    var n, m;

    // Amount of free space at the end of the window.
    var more = zip_window_size - zip_lookahead - zip_strstart;

    /* If the window is almost full and there is insufficient lookahead,
     * move the upper half to the lower one to make room in the upper half.
     */
    if(more == -1) {
	/* Very unlikely, but possible on 16 bit machine if strstart == 0
         * and lookahead == 1 (input done one byte at time)
         */
	more--;
    } else if(zip_strstart >= zip_WSIZE + zip_MAX_DIST) {
	/* By the IN assertion, the window is not empty so we can't confuse
         * more == 0 with more == 64K on a 16 bit machine.
         */
//	Assert(window_size == (ulg)2*WSIZE, "no sliding with BIG_MEM");

//	System.arraycopy(window, WSIZE, window, 0, WSIZE);
	for(n = 0; n < zip_WSIZE; n++)
	    zip_window[n] = zip_window[n + zip_WSIZE];
      
	zip_match_start -= zip_WSIZE;
	zip_strstart    -= zip_WSIZE; /* we now have strstart >= MAX_DIST: */
	zip_block_start -= zip_WSIZE;

	for(n = 0; n < zip_HASH_SIZE; n++) {
	    m = zip_head1(n);
	    zip_head2(n, m >= zip_WSIZE ? m - zip_WSIZE : zip_NIL);
	}
	for(n = 0; n < zip_WSIZE; n++) {
	    /* If n is not on any hash chain, prev[n] is garbage but
	     * its value will never be used.
	     */
	    m = zip_prev[n];
	    zip_prev[n] = (m >= zip_WSIZE ? m - zip_WSIZE : zip_NIL);
	}
	more += zip_WSIZE;
    }
    // At this point, more >= 2
    if(!zip_eofile) {
	n = zip_read_buff(zip_window, zip_strstart + zip_lookahead, more);
	if(n <= 0)
	    zip_eofile = true;
	else
	    zip_lookahead += n;
    }
}

/* ==========================================================================
 * Processes a new input file and return its compressed length. This
 * function does not perform lazy evaluationof matches and inserts
 * new strings in the dictionary only for unmatched strings or for short
 * matches. It is used only for the fast compression options.
 */
var zip_deflate_fast = function() {
    while(zip_lookahead != 0 && zip_qhead == null) {
	var flush; // set if current block must be flushed

	/* Insert the string window[strstart .. strstart+2] in the
	 * dictionary, and set hash_head to the head of the hash chain:
	 */
	zip_INSERT_STRING();

	/* Find the longest match, discarding those <= prev_length.
	 * At this point we have always match_length < MIN_MATCH
	 */
	if(zip_hash_head != zip_NIL &&
	   zip_strstart - zip_hash_head <= zip_MAX_DIST) {
	    /* To simplify the code, we prevent matches with the string
	     * of window index 0 (in particular we have to avoid a match
	     * of the string with itself at the start of the input file).
	     */
	    zip_match_length = zip_longest_match(zip_hash_head);
	    /* longest_match() sets match_start */
	    if(zip_match_length > zip_lookahead)
		zip_match_length = zip_lookahead;
	}
	if(zip_match_length >= zip_MIN_MATCH) {
//	    check_match(strstart, match_start, match_length);

	    flush = zip_ct_tally(zip_strstart - zip_match_start,
				 zip_match_length - zip_MIN_MATCH);
	    zip_lookahead -= zip_match_length;

	    /* Insert new strings in the hash table only if the match length
	     * is not too large. This saves time but degrades compression.
	     */
	    if(zip_match_length <= zip_max_lazy_match) {
		zip_match_length--; // string at strstart already in hash table
		do {
		    zip_strstart++;
		    zip_INSERT_STRING();
		    /* strstart never exceeds WSIZE-MAX_MATCH, so there are
		     * always MIN_MATCH bytes ahead. If lookahead < MIN_MATCH
		     * these bytes are garbage, but it does not matter since
		     * the next lookahead bytes will be emitted as literals.
		     */
		} while(--zip_match_length != 0);
		zip_strstart++;
	    } else {
		zip_strstart += zip_match_length;
		zip_match_length = 0;
		zip_ins_h = zip_window[zip_strstart] & 0xff;
//		UPDATE_HASH(ins_h, window[strstart + 1]);
		zip_ins_h = ((zip_ins_h<<zip_H_SHIFT) ^ (zip_window[zip_strstart + 1] & 0xff)) & zip_HASH_MASK;

//#if MIN_MATCH != 3
//		Call UPDATE_HASH() MIN_MATCH-3 more times
//#endif

	    }
	} else {
	    /* No match, output a literal byte */
	    flush = zip_ct_tally(0, zip_window[zip_strstart] & 0xff);
	    zip_lookahead--;
	    zip_strstart++;
	}
	if(flush) {
	    zip_flush_block(0);
	    zip_block_start = zip_strstart;
	}

	/* Make sure that we always have enough lookahead, except
	 * at the end of the input file. We need MAX_MATCH bytes
	 * for the next match, plus MIN_MATCH bytes to insert the
	 * string following the next match.
	 */
	while(zip_lookahead < zip_MIN_LOOKAHEAD && !zip_eofile)
	    zip_fill_window();
    }
}

var zip_deflate_better = function() {
    /* Process the input block. */
    while(zip_lookahead != 0 && zip_qhead == null) {
	/* Insert the string window[strstart .. strstart+2] in the
	 * dictionary, and set hash_head to the head of the hash chain:
	 */
	zip_INSERT_STRING();

	/* Find the longest match, discarding those <= prev_length.
	 */
	zip_prev_length = zip_match_length;
	zip_prev_match = zip_match_start;
	zip_match_length = zip_MIN_MATCH - 1;

	if(zip_hash_head != zip_NIL &&
	   zip_prev_length < zip_max_lazy_match &&
	   zip_strstart - zip_hash_head <= zip_MAX_DIST) {
	    /* To simplify the code, we prevent matches with the string
	     * of window index 0 (in particular we have to avoid a match
	     * of the string with itself at the start of the input file).
	     */
	    zip_match_length = zip_longest_match(zip_hash_head);
	    /* longest_match() sets match_start */
	    if(zip_match_length > zip_lookahead)
		zip_match_length = zip_lookahead;

	    /* Ignore a length 3 match if it is too distant: */
	    if(zip_match_length == zip_MIN_MATCH &&
	       zip_strstart - zip_match_start > zip_TOO_FAR) {
		/* If prev_match is also MIN_MATCH, match_start is garbage
		 * but we will ignore the current match anyway.
		 */
		zip_match_length--;
	    }
	}
	/* If there was a match at the previous step and the current
	 * match is not better, output the previous match:
	 */
	if(zip_prev_length >= zip_MIN_MATCH &&
	   zip_match_length <= zip_prev_length) {
	    var flush; // set if current block must be flushed

//	    check_match(strstart - 1, prev_match, prev_length);
	    flush = zip_ct_tally(zip_strstart - 1 - zip_prev_match,
				 zip_prev_length - zip_MIN_MATCH);

	    /* Insert in hash table all strings up to the end of the match.
	     * strstart-1 and strstart are already inserted.
	     */
	    zip_lookahead -= zip_prev_length - 1;
	    zip_prev_length -= 2;
	    do {
		zip_strstart++;
		zip_INSERT_STRING();
		/* strstart never exceeds WSIZE-MAX_MATCH, so there are
		 * always MIN_MATCH bytes ahead. If lookahead < MIN_MATCH
		 * these bytes are garbage, but it does not matter since the
		 * next lookahead bytes will always be emitted as literals.
		 */
	    } while(--zip_prev_length != 0);
	    zip_match_available = 0;
	    zip_match_length = zip_MIN_MATCH - 1;
	    zip_strstart++;
	    if(flush) {
		zip_flush_block(0);
		zip_block_start = zip_strstart;
	    }
	} else if(zip_match_available != 0) {
	    /* If there was no match at the previous position, output a
	     * single literal. If there was a match but the current match
	     * is longer, truncate the previous match to a single literal.
	     */
	    if(zip_ct_tally(0, zip_window[zip_strstart - 1] & 0xff)) {
		zip_flush_block(0);
		zip_block_start = zip_strstart;
	    }
	    zip_strstart++;
	    zip_lookahead--;
	} else {
	    /* There is no previous match to compare with, wait for
	     * the next step to decide.
	     */
	    zip_match_available = 1;
	    zip_strstart++;
	    zip_lookahead--;
	}

	/* Make sure that we always have enough lookahead, except
	 * at the end of the input file. We need MAX_MATCH bytes
	 * for the next match, plus MIN_MATCH bytes to insert the
	 * string following the next match.
	 */
	while(zip_lookahead < zip_MIN_LOOKAHEAD && !zip_eofile)
	    zip_fill_window();
    }
}

var zip_init_deflate = function() {
    if(zip_eofile)
	return;
    zip_bi_buf = 0;
    zip_bi_valid = 0;
    zip_ct_init();
    zip_lm_init();

    zip_qhead = null;
    zip_outcnt = 0;
    zip_outoff = 0;

    if(zip_compr_level <= 3)
    {
	zip_prev_length = zip_MIN_MATCH - 1;
	zip_match_length = 0;
    }
    else
    {
	zip_match_length = zip_MIN_MATCH - 1;
	zip_match_available = 0;
    }

    zip_complete = false;
}

/* ==========================================================================
 * Same as above, but achieves better compression. We use a lazy
 * evaluation for matches: a match is finally adopted only if there is
 * no better match at the next window position.
 */
var zip_deflate_internal = function(buff, off, buff_size) {
    var n;

    if(!zip_initflag)
    {
	zip_init_deflate();
	zip_initflag = true;
	if(zip_lookahead == 0) { // empty
	    zip_complete = true;
	    return 0;
	}
    }

    if((n = zip_qcopy(buff, off, buff_size)) == buff_size)
	return buff_size;

    if(zip_complete)
	return n;

    if(zip_compr_level <= 3) // optimized for speed
	zip_deflate_fast();
    else
	zip_deflate_better();
    if(zip_lookahead == 0) {
	if(zip_match_available != 0)
	    zip_ct_tally(0, zip_window[zip_strstart - 1] & 0xff);
	zip_flush_block(1);
	zip_complete = true;
    }
    return n + zip_qcopy(buff, n + off, buff_size - n);
}

var zip_qcopy = function(buff, off, buff_size) {
    var n, i, j;

    n = 0;
    while(zip_qhead != null && n < buff_size)
    {
	i = buff_size - n;
	if(i > zip_qhead.len)
	    i = zip_qhead.len;
//      System.arraycopy(qhead.ptr, qhead.off, buff, off + n, i);
	for(j = 0; j < i; j++)
	    buff[off + n + j] = zip_qhead.ptr[zip_qhead.off + j];
	
	zip_qhead.off += i;
	zip_qhead.len -= i;
	n += i;
	if(zip_qhead.len == 0) {
	    var p;
	    p = zip_qhead;
	    zip_qhead = zip_qhead.next;
	    zip_reuse_queue(p);
	}
    }

    if(n == buff_size)
	return n;

    if(zip_outoff < zip_outcnt) {
	i = buff_size - n;
	if(i > zip_outcnt - zip_outoff)
	    i = zip_outcnt - zip_outoff;
	// System.arraycopy(outbuf, outoff, buff, off + n, i);
	for(j = 0; j < i; j++)
	    buff[off + n + j] = zip_outbuf[zip_outoff + j];
	zip_outoff += i;
	n += i;
	if(zip_outcnt == zip_outoff)
	    zip_outcnt = zip_outoff = 0;
    }
    return n;
}

/* ==========================================================================
 * Allocate the match buffer, initialize the various tables and save the
 * location of the internal file attribute (ascii/binary) and method
 * (DEFLATE/STORE).
 */
var zip_ct_init = function() {
    var n;	// iterates over tree elements
    var bits;	// bit counter
    var length;	// length value
    var code;	// code value
    var dist;	// distance index

    if(zip_static_dtree[0].dl != 0) return; // ct_init already called

    zip_l_desc.dyn_tree		= zip_dyn_ltree;
    zip_l_desc.static_tree	= zip_static_ltree;
    zip_l_desc.extra_bits	= zip_extra_lbits;
    zip_l_desc.extra_base	= zip_LITERALS + 1;
    zip_l_desc.elems		= zip_L_CODES;
    zip_l_desc.max_length	= zip_MAX_BITS;
    zip_l_desc.max_code		= 0;

    zip_d_desc.dyn_tree		= zip_dyn_dtree;
    zip_d_desc.static_tree	= zip_static_dtree;
    zip_d_desc.extra_bits	= zip_extra_dbits;
    zip_d_desc.extra_base	= 0;
    zip_d_desc.elems		= zip_D_CODES;
    zip_d_desc.max_length	= zip_MAX_BITS;
    zip_d_desc.max_code		= 0;

    zip_bl_desc.dyn_tree	= zip_bl_tree;
    zip_bl_desc.static_tree	= null;
    zip_bl_desc.extra_bits	= zip_extra_blbits;
    zip_bl_desc.extra_base	= 0;
    zip_bl_desc.elems		= zip_BL_CODES;
    zip_bl_desc.max_length	= zip_MAX_BL_BITS;
    zip_bl_desc.max_code	= 0;

    // Initialize the mapping length (0..255) -> length code (0..28)
    length = 0;
    for(code = 0; code < zip_LENGTH_CODES-1; code++) {
	zip_base_length[code] = length;
	for(n = 0; n < (1<<zip_extra_lbits[code]); n++)
	    zip_length_code[length++] = code;
    }
    // Assert (length == 256, "ct_init: length != 256");

    /* Note that the length 255 (match length 258) can be represented
     * in two different ways: code 284 + 5 bits or code 285, so we
     * overwrite length_code[255] to use the best encoding:
     */
    zip_length_code[length-1] = code;

    /* Initialize the mapping dist (0..32K) -> dist code (0..29) */
    dist = 0;
    for(code = 0 ; code < 16; code++) {
	zip_base_dist[code] = dist;
	for(n = 0; n < (1<<zip_extra_dbits[code]); n++) {
	    zip_dist_code[dist++] = code;
	}
    }
    // Assert (dist == 256, "ct_init: dist != 256");
    dist >>= 7; // from now on, all distances are divided by 128
    for( ; code < zip_D_CODES; code++) {
	zip_base_dist[code] = dist << 7;
	for(n = 0; n < (1<<(zip_extra_dbits[code]-7)); n++)
	    zip_dist_code[256 + dist++] = code;
    }
    // Assert (dist == 256, "ct_init: 256+dist != 512");

    // Construct the codes of the static literal tree
    for(bits = 0; bits <= zip_MAX_BITS; bits++)
	zip_bl_count[bits] = 0;
    n = 0;
    while(n <= 143) { zip_static_ltree[n++].dl = 8; zip_bl_count[8]++; }
    while(n <= 255) { zip_static_ltree[n++].dl = 9; zip_bl_count[9]++; }
    while(n <= 279) { zip_static_ltree[n++].dl = 7; zip_bl_count[7]++; }
    while(n <= 287) { zip_static_ltree[n++].dl = 8; zip_bl_count[8]++; }
    /* Codes 286 and 287 do not exist, but we must include them in the
     * tree construction to get a canonical Huffman tree (longest code
     * all ones)
     */
    zip_gen_codes(zip_static_ltree, zip_L_CODES + 1);

    /* The static distance tree is trivial: */
    for(n = 0; n < zip_D_CODES; n++) {
	zip_static_dtree[n].dl = 5;
	zip_static_dtree[n].fc = zip_bi_reverse(n, 5);
    }

    // Initialize the first block of the first file:
    zip_init_block();
}

/* ==========================================================================
 * Initialize a new block.
 */
var zip_init_block = function() {
    var n; // iterates over tree elements

    // Initialize the trees.
    for(n = 0; n < zip_L_CODES;  n++) zip_dyn_ltree[n].fc = 0;
    for(n = 0; n < zip_D_CODES;  n++) zip_dyn_dtree[n].fc = 0;
    for(n = 0; n < zip_BL_CODES; n++) zip_bl_tree[n].fc = 0;

    zip_dyn_ltree[zip_END_BLOCK].fc = 1;
    zip_opt_len = zip_static_len = 0;
    zip_last_lit = zip_last_dist = zip_last_flags = 0;
    zip_flags = 0;
    zip_flag_bit = 1;
}

/* ==========================================================================
 * Restore the heap property by moving down the tree starting at node k,
 * exchanging a node with the smallest of its two sons if necessary, stopping
 * when the heap property is re-established (each father smaller than its
 * two sons).
 */
var zip_pqdownheap = function(
    tree,	// the tree to restore
    k) {	// node to move down
    var v = zip_heap[k];
    var j = k << 1;	// left son of k

    while(j <= zip_heap_len) {
	// Set j to the smallest of the two sons:
	if(j < zip_heap_len &&
	   zip_SMALLER(tree, zip_heap[j + 1], zip_heap[j]))
	    j++;

	// Exit if v is smaller than both sons
	if(zip_SMALLER(tree, v, zip_heap[j]))
	    break;

	// Exchange v with the smallest son
	zip_heap[k] = zip_heap[j];
	k = j;

	// And continue down the tree, setting j to the left son of k
	j <<= 1;
    }
    zip_heap[k] = v;
}

/* ==========================================================================
 * Compute the optimal bit lengths for a tree and update the total bit length
 * for the current block.
 * IN assertion: the fields freq and dad are set, heap[heap_max] and
 *    above are the tree nodes sorted by increasing frequency.
 * OUT assertions: the field len is set to the optimal bit length, the
 *     array bl_count contains the frequencies for each bit length.
 *     The length opt_len is updated; static_len is also updated if stree is
 *     not null.
 */
var zip_gen_bitlen = function(desc) { // the tree descriptor
    var tree		= desc.dyn_tree;
    var extra		= desc.extra_bits;
    var base		= desc.extra_base;
    var max_code	= desc.max_code;
    var max_length	= desc.max_length;
    var stree		= desc.static_tree;
    var h;		// heap index
    var n, m;		// iterate over the tree elements
    var bits;		// bit length
    var xbits;		// extra bits
    var f;		// frequency
    var overflow = 0;	// number of elements with bit length too large

    for(bits = 0; bits <= zip_MAX_BITS; bits++)
	zip_bl_count[bits] = 0;

    /* In a first pass, compute the optimal bit lengths (which may
     * overflow in the case of the bit length tree).
     */
    tree[zip_heap[zip_heap_max]].dl = 0; // root of the heap

    for(h = zip_heap_max + 1; h < zip_HEAP_SIZE; h++) {
	n = zip_heap[h];
	bits = tree[tree[n].dl].dl + 1;
	if(bits > max_length) {
	    bits = max_length;
	    overflow++;
	}
	tree[n].dl = bits;
	// We overwrite tree[n].dl which is no longer needed

	if(n > max_code)
	    continue; // not a leaf node

	zip_bl_count[bits]++;
	xbits = 0;
	if(n >= base)
	    xbits = extra[n - base];
	f = tree[n].fc;
	zip_opt_len += f * (bits + xbits);
	if(stree != null)
	    zip_static_len += f * (stree[n].dl + xbits);
    }
    if(overflow == 0)
	return;

    // This happens for example on obj2 and pic of the Calgary corpus

    // Find the first bit length which could increase:
    do {
	bits = max_length - 1;
	while(zip_bl_count[bits] == 0)
	    bits--;
	zip_bl_count[bits]--;		// move one leaf down the tree
	zip_bl_count[bits + 1] += 2;	// move one overflow item as its brother
	zip_bl_count[max_length]--;
	/* The brother of the overflow item also moves one step up,
	 * but this does not affect bl_count[max_length]
	 */
	overflow -= 2;
    } while(overflow > 0);

    /* Now recompute all bit lengths, scanning in increasing frequency.
     * h is still equal to HEAP_SIZE. (It is simpler to reconstruct all
     * lengths instead of fixing only the wrong ones. This idea is taken
     * from 'ar' written by Haruhiko Okumura.)
     */
    for(bits = max_length; bits != 0; bits--) {
	n = zip_bl_count[bits];
	while(n != 0) {
	    m = zip_heap[--h];
	    if(m > max_code)
		continue;
	    if(tree[m].dl != bits) {
		zip_opt_len += (bits - tree[m].dl) * tree[m].fc;
		tree[m].fc = bits;
	    }
	    n--;
	}
    }
}

  /* ==========================================================================
   * Generate the codes for a given tree and bit counts (which need not be
   * optimal).
   * IN assertion: the array bl_count contains the bit length statistics for
   * the given tree and the field len is set for all tree elements.
   * OUT assertion: the field code is set for all tree elements of non
   *     zero code length.
   */
var zip_gen_codes = function(tree,	// the tree to decorate
		   max_code) {	// largest code with non zero frequency
    var next_code = new Array(zip_MAX_BITS+1); // next code value for each bit length
    var code = 0;		// running code value
    var bits;			// bit index
    var n;			// code index

    /* The distribution counts are first used to generate the code values
     * without bit reversal.
     */
    for(bits = 1; bits <= zip_MAX_BITS; bits++) {
	code = ((code + zip_bl_count[bits-1]) << 1);
	next_code[bits] = code;
    }

    /* Check that the bit counts in bl_count are consistent. The last code
     * must be all ones.
     */
//    Assert (code + encoder->bl_count[MAX_BITS]-1 == (1<<MAX_BITS)-1,
//	    "inconsistent bit counts");
//    Tracev((stderr,"\ngen_codes: max_code %d ", max_code));

    for(n = 0; n <= max_code; n++) {
	var len = tree[n].dl;
	if(len == 0)
	    continue;
	// Now reverse the bits
	tree[n].fc = zip_bi_reverse(next_code[len]++, len);

//      Tracec(tree != static_ltree, (stderr,"\nn %3d %c l %2d c %4x (%x) ",
//	  n, (isgraph(n) ? n : ' '), len, tree[n].fc, next_code[len]-1));
    }
}

/* ==========================================================================
 * Construct one Huffman tree and assigns the code bit strings and lengths.
 * Update the total bit length for the current block.
 * IN assertion: the field freq is set for all tree elements.
 * OUT assertions: the fields len and code are set to the optimal bit length
 *     and corresponding code. The length opt_len is updated; static_len is
 *     also updated if stree is not null. The field max_code is set.
 */
var zip_build_tree = function(desc) { // the tree descriptor
    var tree	= desc.dyn_tree;
    var stree	= desc.static_tree;
    var elems	= desc.elems;
    var n, m;		// iterate over heap elements
    var max_code = -1;	// largest code with non zero frequency
    var node = elems;	// next internal node of the tree

    /* Construct the initial heap, with least frequent element in
     * heap[SMALLEST]. The sons of heap[n] are heap[2*n] and heap[2*n+1].
     * heap[0] is not used.
     */
    zip_heap_len = 0;
    zip_heap_max = zip_HEAP_SIZE;

    for(n = 0; n < elems; n++) {
	if(tree[n].fc != 0) {
	    zip_heap[++zip_heap_len] = max_code = n;
	    zip_depth[n] = 0;
	} else
	    tree[n].dl = 0;
    }

    /* The pkzip format requires that at least one distance code exists,
     * and that at least one bit should be sent even if there is only one
     * possible code. So to avoid special checks later on we force at least
     * two codes of non zero frequency.
     */
    while(zip_heap_len < 2) {
	var xnew = zip_heap[++zip_heap_len] = (max_code < 2 ? ++max_code : 0);
	tree[xnew].fc = 1;
	zip_depth[xnew] = 0;
	zip_opt_len--;
	if(stree != null)
	    zip_static_len -= stree[xnew].dl;
	// new is 0 or 1 so it does not have extra bits
    }
    desc.max_code = max_code;

    /* The elements heap[heap_len/2+1 .. heap_len] are leaves of the tree,
     * establish sub-heaps of increasing lengths:
     */
    for(n = zip_heap_len >> 1; n >= 1; n--)
	zip_pqdownheap(tree, n);

    /* Construct the Huffman tree by repeatedly combining the least two
     * frequent nodes.
     */
    do {
	n = zip_heap[zip_SMALLEST];
	zip_heap[zip_SMALLEST] = zip_heap[zip_heap_len--];
	zip_pqdownheap(tree, zip_SMALLEST);

	m = zip_heap[zip_SMALLEST];  // m = node of next least frequency

	// keep the nodes sorted by frequency
	zip_heap[--zip_heap_max] = n;
	zip_heap[--zip_heap_max] = m;

	// Create a new node father of n and m
	tree[node].fc = tree[n].fc + tree[m].fc;
//	depth[node] = (char)(MAX(depth[n], depth[m]) + 1);
	if(zip_depth[n] > zip_depth[m] + 1)
	    zip_depth[node] = zip_depth[n];
	else
	    zip_depth[node] = zip_depth[m] + 1;
	tree[n].dl = tree[m].dl = node;

	// and insert the new node in the heap
	zip_heap[zip_SMALLEST] = node++;
	zip_pqdownheap(tree, zip_SMALLEST);

    } while(zip_heap_len >= 2);

    zip_heap[--zip_heap_max] = zip_heap[zip_SMALLEST];

    /* At this point, the fields freq and dad are set. We can now
     * generate the bit lengths.
     */
    zip_gen_bitlen(desc);

    // The field len is now set, we can generate the bit codes
    zip_gen_codes(tree, max_code);
}

/* ==========================================================================
 * Scan a literal or distance tree to determine the frequencies of the codes
 * in the bit length tree. Updates opt_len to take into account the repeat
 * counts. (The contribution of the bit length codes will be added later
 * during the construction of bl_tree.)
 */
var zip_scan_tree = function(tree,// the tree to be scanned
		       max_code) {  // and its largest code of non zero frequency
    var n;			// iterates over all tree elements
    var prevlen = -1;		// last emitted length
    var curlen;			// length of current code
    var nextlen = tree[0].dl;	// length of next code
    var count = 0;		// repeat count of the current code
    var max_count = 7;		// max repeat count
    var min_count = 4;		// min repeat count

    if(nextlen == 0) {
	max_count = 138;
	min_count = 3;
    }
    tree[max_code + 1].dl = 0xffff; // guard

    for(n = 0; n <= max_code; n++) {
	curlen = nextlen;
	nextlen = tree[n + 1].dl;
	if(++count < max_count && curlen == nextlen)
	    continue;
	else if(count < min_count)
	    zip_bl_tree[curlen].fc += count;
	else if(curlen != 0) {
	    if(curlen != prevlen)
		zip_bl_tree[curlen].fc++;
	    zip_bl_tree[zip_REP_3_6].fc++;
	} else if(count <= 10)
	    zip_bl_tree[zip_REPZ_3_10].fc++;
	else
	    zip_bl_tree[zip_REPZ_11_138].fc++;
	count = 0; prevlen = curlen;
	if(nextlen == 0) {
	    max_count = 138;
	    min_count = 3;
	} else if(curlen == nextlen) {
	    max_count = 6;
	    min_count = 3;
	} else {
	    max_count = 7;
	    min_count = 4;
	}
    }
}

  /* ==========================================================================
   * Send a literal or distance tree in compressed form, using the codes in
   * bl_tree.
   */
var zip_send_tree = function(tree, // the tree to be scanned
		   max_code) { // and its largest code of non zero frequency
    var n;			// iterates over all tree elements
    var prevlen = -1;		// last emitted length
    var curlen;			// length of current code
    var nextlen = tree[0].dl;	// length of next code
    var count = 0;		// repeat count of the current code
    var max_count = 7;		// max repeat count
    var min_count = 4;		// min repeat count

    /* tree[max_code+1].dl = -1; */  /* guard already set */
    if(nextlen == 0) {
      max_count = 138;
      min_count = 3;
    }

    for(n = 0; n <= max_code; n++) {
	curlen = nextlen;
	nextlen = tree[n+1].dl;
	if(++count < max_count && curlen == nextlen) {
	    continue;
	} else if(count < min_count) {
	    do { zip_SEND_CODE(curlen, zip_bl_tree); } while(--count != 0);
	} else if(curlen != 0) {
	    if(curlen != prevlen) {
		zip_SEND_CODE(curlen, zip_bl_tree);
		count--;
	    }
	    // Assert(count >= 3 && count <= 6, " 3_6?");
	    zip_SEND_CODE(zip_REP_3_6, zip_bl_tree);
	    zip_send_bits(count - 3, 2);
	} else if(count <= 10) {
	    zip_SEND_CODE(zip_REPZ_3_10, zip_bl_tree);
	    zip_send_bits(count-3, 3);
	} else {
	    zip_SEND_CODE(zip_REPZ_11_138, zip_bl_tree);
	    zip_send_bits(count-11, 7);
	}
	count = 0;
	prevlen = curlen;
	if(nextlen == 0) {
	    max_count = 138;
	    min_count = 3;
	} else if(curlen == nextlen) {
	    max_count = 6;
	    min_count = 3;
	} else {
	    max_count = 7;
	    min_count = 4;
	}
    }
}

/* ==========================================================================
 * Construct the Huffman tree for the bit lengths and return the index in
 * bl_order of the last bit length code to send.
 */
var zip_build_bl_tree = function() {
    var max_blindex;  // index of last bit length code of non zero freq

    // Determine the bit length frequencies for literal and distance trees
    zip_scan_tree(zip_dyn_ltree, zip_l_desc.max_code);
    zip_scan_tree(zip_dyn_dtree, zip_d_desc.max_code);

    // Build the bit length tree:
    zip_build_tree(zip_bl_desc);
    /* opt_len now includes the length of the tree representations, except
     * the lengths of the bit lengths codes and the 5+5+4 bits for the counts.
     */

    /* Determine the number of bit length codes to send. The pkzip format
     * requires that at least 4 bit length codes be sent. (appnote.txt says
     * 3 but the actual value used is 4.)
     */
    for(max_blindex = zip_BL_CODES-1; max_blindex >= 3; max_blindex--) {
	if(zip_bl_tree[zip_bl_order[max_blindex]].dl != 0) break;
    }
    /* Update opt_len to include the bit length tree and counts */
    zip_opt_len += 3*(max_blindex+1) + 5+5+4;
//    Tracev((stderr, "\ndyn trees: dyn %ld, stat %ld",
//	    encoder->opt_len, encoder->static_len));

    return max_blindex;
}

/* ==========================================================================
 * Send the header for a block using dynamic Huffman trees: the counts, the
 * lengths of the bit length codes, the literal tree and the distance tree.
 * IN assertion: lcodes >= 257, dcodes >= 1, blcodes >= 4.
 */
var zip_send_all_trees = function(lcodes, dcodes, blcodes) { // number of codes for each tree
    var rank; // index in bl_order

//    Assert (lcodes >= 257 && dcodes >= 1 && blcodes >= 4, "not enough codes");
//    Assert (lcodes <= L_CODES && dcodes <= D_CODES && blcodes <= BL_CODES,
//	    "too many codes");
//    Tracev((stderr, "\nbl counts: "));
    zip_send_bits(lcodes-257, 5); // not +255 as stated in appnote.txt
    zip_send_bits(dcodes-1,   5);
    zip_send_bits(blcodes-4,  4); // not -3 as stated in appnote.txt
    for(rank = 0; rank < blcodes; rank++) {
//      Tracev((stderr, "\nbl code %2d ", bl_order[rank]));
	zip_send_bits(zip_bl_tree[zip_bl_order[rank]].dl, 3);
    }

    // send the literal tree
    zip_send_tree(zip_dyn_ltree,lcodes-1);

    // send the distance tree
    zip_send_tree(zip_dyn_dtree,dcodes-1);
}

/* ==========================================================================
 * Determine the best encoding for the current block: dynamic trees, static
 * trees or store, and output the encoded block to the zip file.
 */
var zip_flush_block = function(eof) { // true if this is the last block for a file
    var opt_lenb, static_lenb; // opt_len and static_len in bytes
    var max_blindex;	// index of last bit length code of non zero freq
    var stored_len;	// length of input block

    stored_len = zip_strstart - zip_block_start;
    zip_flag_buf[zip_last_flags] = zip_flags; // Save the flags for the last 8 items

    // Construct the literal and distance trees
    zip_build_tree(zip_l_desc);
//    Tracev((stderr, "\nlit data: dyn %ld, stat %ld",
//	    encoder->opt_len, encoder->static_len));

    zip_build_tree(zip_d_desc);
//    Tracev((stderr, "\ndist data: dyn %ld, stat %ld",
//	    encoder->opt_len, encoder->static_len));
    /* At this point, opt_len and static_len are the total bit lengths of
     * the compressed block data, excluding the tree representations.
     */

    /* Build the bit length tree for the above two trees, and get the index
     * in bl_order of the last bit length code to send.
     */
    max_blindex = zip_build_bl_tree();

    // Determine the best encoding. Compute first the block length in bytes
    opt_lenb	= (zip_opt_len   +3+7)>>3;
    static_lenb = (zip_static_len+3+7)>>3;

//    Trace((stderr, "\nopt %lu(%lu) stat %lu(%lu) stored %lu lit %u dist %u ",
//	   opt_lenb, encoder->opt_len,
//	   static_lenb, encoder->static_len, stored_len,
//	   encoder->last_lit, encoder->last_dist));

    if(static_lenb <= opt_lenb)
	opt_lenb = static_lenb;
    if(stored_len + 4 <= opt_lenb // 4: two words for the lengths
       && zip_block_start >= 0) {
	var i;

	/* The test buf != NULL is only necessary if LIT_BUFSIZE > WSIZE.
	 * Otherwise we can't have processed more than WSIZE input bytes since
	 * the last block flush, because compression would have been
	 * successful. If LIT_BUFSIZE <= WSIZE, it is never too late to
	 * transform a block into a stored block.
	 */
	zip_send_bits((zip_STORED_BLOCK<<1)+eof, 3);  /* send block type */
	zip_bi_windup();		 /* align on byte boundary */
	zip_put_short(stored_len);
	zip_put_short(~stored_len);

      // copy block
/*
      p = &window[block_start];
      for(i = 0; i < stored_len; i++)
	put_byte(p[i]);
*/
	for(i = 0; i < stored_len; i++)
	    zip_put_byte(zip_window[zip_block_start + i]);

    } else if(static_lenb == opt_lenb) {
	zip_send_bits((zip_STATIC_TREES<<1)+eof, 3);
	zip_compress_block(zip_static_ltree, zip_static_dtree);
    } else {
	zip_send_bits((zip_DYN_TREES<<1)+eof, 3);
	zip_send_all_trees(zip_l_desc.max_code+1,
			   zip_d_desc.max_code+1,
			   max_blindex+1);
	zip_compress_block(zip_dyn_ltree, zip_dyn_dtree);
    }

    zip_init_block();

    if(eof != 0)
	zip_bi_windup();
}

/* ==========================================================================
 * Save the match info and tally the frequency counts. Return true if
 * the current block must be flushed.
 */
var zip_ct_tally = function(
	dist, // distance of matched string
	lc) { // match length-MIN_MATCH or unmatched char (if dist==0)
    zip_l_buf[zip_last_lit++] = lc;
    if(dist == 0) {
	// lc is the unmatched char
	zip_dyn_ltree[lc].fc++;
    } else {
	// Here, lc is the match length - MIN_MATCH
	dist--;		    // dist = match distance - 1
//      Assert((ush)dist < (ush)MAX_DIST &&
//	     (ush)lc <= (ush)(MAX_MATCH-MIN_MATCH) &&
//	     (ush)D_CODE(dist) < (ush)D_CODES,  "ct_tally: bad match");

	zip_dyn_ltree[zip_length_code[lc]+zip_LITERALS+1].fc++;
	zip_dyn_dtree[zip_D_CODE(dist)].fc++;

	zip_d_buf[zip_last_dist++] = dist;
	zip_flags |= zip_flag_bit;
    }
    zip_flag_bit <<= 1;

    // Output the flags if they fill a byte
    if((zip_last_lit & 7) == 0) {
	zip_flag_buf[zip_last_flags++] = zip_flags;
	zip_flags = 0;
	zip_flag_bit = 1;
    }
    // Try to guess if it is profitable to stop the current block here
    if(zip_compr_level > 2 && (zip_last_lit & 0xfff) == 0) {
	// Compute an upper bound for the compressed length
	var out_length = zip_last_lit * 8;
	var in_length = zip_strstart - zip_block_start;
	var dcode;

	for(dcode = 0; dcode < zip_D_CODES; dcode++) {
	    out_length += zip_dyn_dtree[dcode].fc * (5 + zip_extra_dbits[dcode]);
	}
	out_length >>= 3;
//      Trace((stderr,"\nlast_lit %u, last_dist %u, in %ld, out ~%ld(%ld%%) ",
//	     encoder->last_lit, encoder->last_dist, in_length, out_length,
//	     100L - out_length*100L/in_length));
	if(zip_last_dist < parseInt(zip_last_lit/2) &&
	   out_length < parseInt(in_length/2))
	    return true;
    }
    return (zip_last_lit == zip_LIT_BUFSIZE-1 ||
	    zip_last_dist == zip_DIST_BUFSIZE);
    /* We avoid equality with LIT_BUFSIZE because of wraparound at 64K
     * on 16 bit machines and because stored blocks are restricted to
     * 64K-1 bytes.
     */
}

  /* ==========================================================================
   * Send the block data compressed using the given Huffman trees
   */
var zip_compress_block = function(
	ltree,	// literal tree
	dtree) {	// distance tree
    var dist;		// distance of matched string
    var lc;		// match length or unmatched char (if dist == 0)
    var lx = 0;		// running index in l_buf
    var dx = 0;		// running index in d_buf
    var fx = 0;		// running index in flag_buf
    var flag = 0;	// current flags
    var code;		// the code to send
    var extra;		// number of extra bits to send

    if(zip_last_lit != 0) do {
	if((lx & 7) == 0)
	    flag = zip_flag_buf[fx++];
	lc = zip_l_buf[lx++] & 0xff;
	if((flag & 1) == 0) {
	    zip_SEND_CODE(lc, ltree); /* send a literal byte */
//	Tracecv(isgraph(lc), (stderr," '%c' ", lc));
	} else {
	    // Here, lc is the match length - MIN_MATCH
	    code = zip_length_code[lc];
	    zip_SEND_CODE(code+zip_LITERALS+1, ltree); // send the length code
	    extra = zip_extra_lbits[code];
	    if(extra != 0) {
		lc -= zip_base_length[code];
		zip_send_bits(lc, extra); // send the extra length bits
	    }
	    dist = zip_d_buf[dx++];
	    // Here, dist is the match distance - 1
	    code = zip_D_CODE(dist);
//	Assert (code < D_CODES, "bad d_code");

	    zip_SEND_CODE(code, dtree);	  // send the distance code
	    extra = zip_extra_dbits[code];
	    if(extra != 0) {
		dist -= zip_base_dist[code];
		zip_send_bits(dist, extra);   // send the extra distance bits
	    }
	} // literal or match pair ?
	flag >>= 1;
    } while(lx < zip_last_lit);

    zip_SEND_CODE(zip_END_BLOCK, ltree);
}

/* ==========================================================================
 * Send a value on a given number of bits.
 * IN assertion: length <= 16 and value fits in length bits.
 */
var zip_Buf_size = 16; // bit size of bi_buf
var zip_send_bits = function(
	value,	// value to send
	length) {	// number of bits
    /* If not enough room in bi_buf, use (valid) bits from bi_buf and
     * (16 - bi_valid) bits from value, leaving (width - (16-bi_valid))
     * unused bits in value.
     */
    if(zip_bi_valid > zip_Buf_size - length) {
	zip_bi_buf |= (value << zip_bi_valid);
	zip_put_short(zip_bi_buf);
	zip_bi_buf = (value >> (zip_Buf_size - zip_bi_valid));
	zip_bi_valid += length - zip_Buf_size;
    } else {
	zip_bi_buf |= value << zip_bi_valid;
	zip_bi_valid += length;
    }
}

/* ==========================================================================
 * Reverse the first len bits of a code, using straightforward code (a faster
 * method would use a table)
 * IN assertion: 1 <= len <= 15
 */
var zip_bi_reverse = function(
	code,	// the value to invert
	len) {	// its bit length
    var res = 0;
    do {
	res |= code & 1;
	code >>= 1;
	res <<= 1;
    } while(--len > 0);
    return res >> 1;
}

/* ==========================================================================
 * Write out any remaining bits in an incomplete byte.
 */
var zip_bi_windup = function() {
    if(zip_bi_valid > 8) {
	zip_put_short(zip_bi_buf);
    } else if(zip_bi_valid > 0) {
	zip_put_byte(zip_bi_buf);
    }
    zip_bi_buf = 0;
    zip_bi_valid = 0;
}

var zip_qoutbuf = function() {
    if(zip_outcnt != 0) {
	var q, i;
	q = zip_new_queue();
	if(zip_qhead == null)
	    zip_qhead = zip_qtail = q;
	else
	    zip_qtail = zip_qtail.next = q;
	q.len = zip_outcnt - zip_outoff;
//      System.arraycopy(zip_outbuf, zip_outoff, q.ptr, 0, q.len);
	for(i = 0; i < q.len; i++)
	    q.ptr[i] = zip_outbuf[zip_outoff + i];
	zip_outcnt = zip_outoff = 0;
    }
}

var zip_deflate = function(str, level) {
    var i, j;

    zip_deflate_data = str;
    zip_deflate_pos = 0;
    if(typeof level == "undefined")
	level = zip_DEFAULT_LEVEL;
    zip_deflate_start(level);

    var buff = new Array(1024);
    var aout = [];
    while((i = zip_deflate_internal(buff, 0, buff.length)) > 0) {
	var cbuf = new Array(i);
	for(j = 0; j < i; j++){
	    cbuf[j] = String.fromCharCode(buff[j]);
	}
	aout[aout.length] = cbuf.join("");
    }
    zip_deflate_data = null; // G.C.
    return aout.join("");
}

if (! window.RawDeflate) RawDeflate = {};
RawDeflate.deflate = zip_deflate;

})();

;

/*
 * $Id: rawinflate.js,v 0.2 2009/03/01 18:32:24 dankogai Exp $
 *
 * original:
 * http://www.onicos.com/staff/iz/amuse/javascript/expert/inflate.txt
 */

(function(){

/* Copyright (C) 1999 Masanao Izumo <iz@onicos.co.jp>
 * Version: 1.0.0.1
 * LastModified: Dec 25 1999
 */

/* Interface:
 * data = zip_inflate(src);
 */

/* constant parameters */
var zip_WSIZE = 32768;		// Sliding Window size
var zip_STORED_BLOCK = 0;
var zip_STATIC_TREES = 1;
var zip_DYN_TREES    = 2;

/* for inflate */
var zip_lbits = 9; 		// bits in base literal/length lookup table
var zip_dbits = 6; 		// bits in base distance lookup table
var zip_INBUFSIZ = 32768;	// Input buffer size
var zip_INBUF_EXTRA = 64;	// Extra buffer

/* variables (inflate) */
var zip_slide;
var zip_wp;			// current position in slide
var zip_fixed_tl = null;	// inflate static
var zip_fixed_td;		// inflate static
var zip_fixed_bl, fixed_bd;	// inflate static
var zip_bit_buf;		// bit buffer
var zip_bit_len;		// bits in bit buffer
var zip_method;
var zip_eof;
var zip_copy_leng;
var zip_copy_dist;
var zip_tl, zip_td;	// literal/length and distance decoder tables
var zip_bl, zip_bd;	// number of bits decoded by tl and td

var zip_inflate_data;
var zip_inflate_pos;


/* constant tables (inflate) */
var zip_MASK_BITS = new Array(
    0x0000,
    0x0001, 0x0003, 0x0007, 0x000f, 0x001f, 0x003f, 0x007f, 0x00ff,
    0x01ff, 0x03ff, 0x07ff, 0x0fff, 0x1fff, 0x3fff, 0x7fff, 0xffff);
// Tables for deflate from PKZIP's appnote.txt.
var zip_cplens = new Array( // Copy lengths for literal codes 257..285
    3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
    35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0);
/* note: see note #13 above about the 258 in this list. */
var zip_cplext = new Array( // Extra bits for literal codes 257..285
    0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
    3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0, 99, 99); // 99==invalid
var zip_cpdist = new Array( // Copy offsets for distance codes 0..29
    1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
    257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145,
    8193, 12289, 16385, 24577);
var zip_cpdext = new Array( // Extra bits for distance codes
    0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
    7, 7, 8, 8, 9, 9, 10, 10, 11, 11,
    12, 12, 13, 13);
var zip_border = new Array(  // Order of the bit length code lengths
    16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15);
/* objects (inflate) */

var zip_HuftList = function() {
    this.next = null;
    this.list = null;
}

var zip_HuftNode = function() {
    this.e = 0; // number of extra bits or operation
    this.b = 0; // number of bits in this code or subcode

    // union
    this.n = 0; // literal, length base, or distance base
    this.t = null; // (zip_HuftNode) pointer to next level of table
}

var zip_HuftBuild = function(b,	// code lengths in bits (all assumed <= BMAX)
		       n,	// number of codes (assumed <= N_MAX)
		       s,	// number of simple-valued codes (0..s-1)
		       d,	// list of base values for non-simple codes
		       e,	// list of extra bits for non-simple codes
		       mm	// maximum lookup bits
		   ) {
    this.BMAX = 16;   // maximum bit length of any code
    this.N_MAX = 288; // maximum number of codes in any set
    this.status = 0;	// 0: success, 1: incomplete table, 2: bad input
    this.root = null;	// (zip_HuftList) starting table
    this.m = 0;		// maximum lookup bits, returns actual

/* Given a list of code lengths and a maximum table size, make a set of
   tables to decode that set of codes.	Return zero on success, one if
   the given code set is incomplete (the tables are still built in this
   case), two if the input is invalid (all zero length codes or an
   oversubscribed set of lengths), and three if not enough memory.
   The code with value 256 is special, and the tables are constructed
   so that no bits beyond that code are fetched when that code is
   decoded. */
    {
	var a;			// counter for codes of length k
	var c = new Array(this.BMAX+1);	// bit length count table
	var el;			// length of EOB code (value 256)
	var f;			// i repeats in table every f entries
	var g;			// maximum code length
	var h;			// table level
	var i;			// counter, current code
	var j;			// counter
	var k;			// number of bits in current code
	var lx = new Array(this.BMAX+1);	// stack of bits per table
	var p;			// pointer into c[], b[], or v[]
	var pidx;		// index of p
	var q;			// (zip_HuftNode) points to current table
	var r = new zip_HuftNode(); // table entry for structure assignment
	var u = new Array(this.BMAX); // zip_HuftNode[BMAX][]  table stack
	var v = new Array(this.N_MAX); // values in order of bit length
	var w;
	var x = new Array(this.BMAX+1);// bit offsets, then code stack
	var xp;			// pointer into x or c
	var y;			// number of dummy codes added
	var z;			// number of entries in current table
	var o;
	var tail;		// (zip_HuftList)

	tail = this.root = null;
	for(i = 0; i < c.length; i++)
	    c[i] = 0;
	for(i = 0; i < lx.length; i++)
	    lx[i] = 0;
	for(i = 0; i < u.length; i++)
	    u[i] = null;
	for(i = 0; i < v.length; i++)
	    v[i] = 0;
	for(i = 0; i < x.length; i++)
	    x[i] = 0;

	// Generate counts for each bit length
	el = n > 256 ? b[256] : this.BMAX; // set length of EOB code, if any
	p = b; pidx = 0;
	i = n;
	do {
	    c[p[pidx]]++;	// assume all entries <= BMAX
	    pidx++;
	} while(--i > 0);
	if(c[0] == n) {	// null input--all zero length codes
	    this.root = null;
	    this.m = 0;
	    this.status = 0;
	    return;
	}

	// Find minimum and maximum length, bound *m by those
	for(j = 1; j <= this.BMAX; j++)
	    if(c[j] != 0)
		break;
	k = j;			// minimum code length
	if(mm < j)
	    mm = j;
	for(i = this.BMAX; i != 0; i--)
	    if(c[i] != 0)
		break;
	g = i;			// maximum code length
	if(mm > i)
	    mm = i;

	// Adjust last length count to fill out codes, if needed
	for(y = 1 << j; j < i; j++, y <<= 1)
	    if((y -= c[j]) < 0) {
		this.status = 2;	// bad input: more codes than bits
		this.m = mm;
		return;
	    }
	if((y -= c[i]) < 0) {
	    this.status = 2;
	    this.m = mm;
	    return;
	}
	c[i] += y;

	// Generate starting offsets into the value table for each length
	x[1] = j = 0;
	p = c;
	pidx = 1;
	xp = 2;
	while(--i > 0)		// note that i == g from above
	    x[xp++] = (j += p[pidx++]);

	// Make a table of values in order of bit lengths
	p = b; pidx = 0;
	i = 0;
	do {
	    if((j = p[pidx++]) != 0)
		v[x[j]++] = i;
	} while(++i < n);
	n = x[g];			// set n to length of v

	// Generate the Huffman codes and for each, make the table entries
	x[0] = i = 0;		// first Huffman code is zero
	p = v; pidx = 0;		// grab values in bit order
	h = -1;			// no tables yet--level -1
	w = lx[0] = 0;		// no bits decoded yet
	q = null;			// ditto
	z = 0;			// ditto

	// go through the bit lengths (k already is bits in shortest code)
	for(; k <= g; k++) {
	    a = c[k];
	    while(a-- > 0) {
		// here i is the Huffman code of length k bits for value p[pidx]
		// make tables up to required level
		while(k > w + lx[1 + h]) {
		    w += lx[1 + h]; // add bits already decoded
		    h++;

		    // compute minimum size table less than or equal to *m bits
		    z = (z = g - w) > mm ? mm : z; // upper limit
		    if((f = 1 << (j = k - w)) > a + 1) { // try a k-w bit table
			// too few codes for k-w bit table
			f -= a + 1;	// deduct codes from patterns left
			xp = k;
			while(++j < z) { // try smaller tables up to z bits
			    if((f <<= 1) <= c[++xp])
				break;	// enough codes to use up j bits
			    f -= c[xp];	// else deduct codes from patterns
			}
		    }
		    if(w + j > el && w < el)
			j = el - w;	// make EOB code end at table
		    z = 1 << j;	// table entries for j-bit table
		    lx[1 + h] = j; // set table size in stack

		    // allocate and link in new table
		    q = new Array(z);
		    for(o = 0; o < z; o++) {
			q[o] = new zip_HuftNode();
		    }

		    if(tail == null)
			tail = this.root = new zip_HuftList();
		    else
			tail = tail.next = new zip_HuftList();
		    tail.next = null;
		    tail.list = q;
		    u[h] = q;	// table starts after link

		    /* connect to last table, if there is one */
		    if(h > 0) {
			x[h] = i;		// save pattern for backing up
			r.b = lx[h];	// bits to dump before this table
			r.e = 16 + j;	// bits in this table
			r.t = q;		// pointer to this table
			j = (i & ((1 << w) - 1)) >> (w - lx[h]);
			u[h-1][j].e = r.e;
			u[h-1][j].b = r.b;
			u[h-1][j].n = r.n;
			u[h-1][j].t = r.t;
		    }
		}

		// set up table entry in r
		r.b = k - w;
		if(pidx >= n)
		    r.e = 99;		// out of values--invalid code
		else if(p[pidx] < s) {
		    r.e = (p[pidx] < 256 ? 16 : 15); // 256 is end-of-block code
		    r.n = p[pidx++];	// simple code is just the value
		} else {
		    r.e = e[p[pidx] - s];	// non-simple--look up in lists
		    r.n = d[p[pidx++] - s];
		}

		// fill code-like entries with r //
		f = 1 << (k - w);
		for(j = i >> w; j < z; j += f) {
		    q[j].e = r.e;
		    q[j].b = r.b;
		    q[j].n = r.n;
		    q[j].t = r.t;
		}

		// backwards increment the k-bit code i
		for(j = 1 << (k - 1); (i & j) != 0; j >>= 1)
		    i ^= j;
		i ^= j;

		// backup over finished tables
		while((i & ((1 << w) - 1)) != x[h]) {
		    w -= lx[h];		// don't need to update q
		    h--;
		}
	    }
	}

	/* return actual size of base table */
	this.m = lx[1];

	/* Return true (1) if we were given an incomplete table */
	this.status = ((y != 0 && g != 1) ? 1 : 0);
    } /* end of constructor */
}


/* routines (inflate) */

var zip_GET_BYTE = function() {
    if(zip_inflate_data.length == zip_inflate_pos)
	return -1;
    return zip_inflate_data.charCodeAt(zip_inflate_pos++) & 0xff;
}

var zip_NEEDBITS = function(n) {
    while(zip_bit_len < n) {
	zip_bit_buf |= zip_GET_BYTE() << zip_bit_len;
	zip_bit_len += 8;
    }
}

var zip_GETBITS = function(n) {
    return zip_bit_buf & zip_MASK_BITS[n];
}

var zip_DUMPBITS = function(n) {
    zip_bit_buf >>= n;
    zip_bit_len -= n;
}

var zip_inflate_codes = function(buff, off, size) {
    /* inflate (decompress) the codes in a deflated (compressed) block.
       Return an error code or zero if it all goes ok. */
    var e;		// table entry flag/number of extra bits
    var t;		// (zip_HuftNode) pointer to table entry
    var n;

    if(size == 0)
      return 0;

    // inflate the coded data
    n = 0;
    for(;;) {			// do until end of block
	zip_NEEDBITS(zip_bl);
	t = zip_tl.list[zip_GETBITS(zip_bl)];
	e = t.e;
	while(e > 16) {
	    if(e == 99)
		return -1;
	    zip_DUMPBITS(t.b);
	    e -= 16;
	    zip_NEEDBITS(e);
	    t = t.t[zip_GETBITS(e)];
	    e = t.e;
	}
	zip_DUMPBITS(t.b);

	if(e == 16) {		// then it's a literal
	    zip_wp &= zip_WSIZE - 1;
	    buff[off + n++] = zip_slide[zip_wp++] = t.n;
	    if(n == size)
		return size;
	    continue;
	}

	// exit if end of block
	if(e == 15)
	    break;

	// it's an EOB or a length

	// get length of block to copy
	zip_NEEDBITS(e);
	zip_copy_leng = t.n + zip_GETBITS(e);
	zip_DUMPBITS(e);

	// decode distance of block to copy
	zip_NEEDBITS(zip_bd);
	t = zip_td.list[zip_GETBITS(zip_bd)];
	e = t.e;

	while(e > 16) {
	    if(e == 99)
		return -1;
	    zip_DUMPBITS(t.b);
	    e -= 16;
	    zip_NEEDBITS(e);
	    t = t.t[zip_GETBITS(e)];
	    e = t.e;
	}
	zip_DUMPBITS(t.b);
	zip_NEEDBITS(e);
	zip_copy_dist = zip_wp - t.n - zip_GETBITS(e);
	zip_DUMPBITS(e);

	// do the copy
	while(zip_copy_leng > 0 && n < size) {
	    zip_copy_leng--;
	    zip_copy_dist &= zip_WSIZE - 1;
	    zip_wp &= zip_WSIZE - 1;
	    buff[off + n++] = zip_slide[zip_wp++]
		= zip_slide[zip_copy_dist++];
	}

	if(n == size)
	    return size;
    }

    zip_method = -1; // done
    return n;
}

var zip_inflate_stored = function(buff, off, size) {
    /* "decompress" an inflated type 0 (stored) block. */
    var n;

    // go to byte boundary
    n = zip_bit_len & 7;
    zip_DUMPBITS(n);

    // get the length and its complement
    zip_NEEDBITS(16);
    n = zip_GETBITS(16);
    zip_DUMPBITS(16);
    zip_NEEDBITS(16);
    if(n != ((~zip_bit_buf) & 0xffff))
	return -1;			// error in compressed data
    zip_DUMPBITS(16);

    // read and output the compressed data
    zip_copy_leng = n;

    n = 0;
    while(zip_copy_leng > 0 && n < size) {
	zip_copy_leng--;
	zip_wp &= zip_WSIZE - 1;
	zip_NEEDBITS(8);
	buff[off + n++] = zip_slide[zip_wp++] =
	    zip_GETBITS(8);
	zip_DUMPBITS(8);
    }

    if(zip_copy_leng == 0)
      zip_method = -1; // done
    return n;
}

var zip_inflate_fixed = function(buff, off, size) {
    /* decompress an inflated type 1 (fixed Huffman codes) block.  We should
       either replace this with a custom decoder, or at least precompute the
       Huffman tables. */

    // if first time, set up tables for fixed blocks
    if(zip_fixed_tl == null) {
	var i;			// temporary variable
	var l = new Array(288);	// length list for huft_build
	var h;	// zip_HuftBuild

	// literal table
	for(i = 0; i < 144; i++)
	    l[i] = 8;
	for(; i < 256; i++)
	    l[i] = 9;
	for(; i < 280; i++)
	    l[i] = 7;
	for(; i < 288; i++)	// make a complete, but wrong code set
	    l[i] = 8;
	zip_fixed_bl = 7;

	h = new zip_HuftBuild(l, 288, 257, zip_cplens, zip_cplext,
			      zip_fixed_bl);
	if(h.status != 0) {
	    alert("HufBuild error: "+h.status);
	    return -1;
	}
	zip_fixed_tl = h.root;
	zip_fixed_bl = h.m;

	// distance table
	for(i = 0; i < 30; i++)	// make an incomplete code set
	    l[i] = 5;
	zip_fixed_bd = 5;

	h = new zip_HuftBuild(l, 30, 0, zip_cpdist, zip_cpdext, zip_fixed_bd);
	if(h.status > 1) {
	    zip_fixed_tl = null;
	    alert("HufBuild error: "+h.status);
	    return -1;
	}
	zip_fixed_td = h.root;
	zip_fixed_bd = h.m;
    }

    zip_tl = zip_fixed_tl;
    zip_td = zip_fixed_td;
    zip_bl = zip_fixed_bl;
    zip_bd = zip_fixed_bd;
    return zip_inflate_codes(buff, off, size);
}

var zip_inflate_dynamic = function(buff, off, size) {
    // decompress an inflated type 2 (dynamic Huffman codes) block.
    var i;		// temporary variables
    var j;
    var l;		// last length
    var n;		// number of lengths to get
    var t;		// (zip_HuftNode) literal/length code table
    var nb;		// number of bit length codes
    var nl;		// number of literal/length codes
    var nd;		// number of distance codes
    var ll = new Array(286+30); // literal/length and distance code lengths
    var h;		// (zip_HuftBuild)

    for(i = 0; i < ll.length; i++)
	ll[i] = 0;

    // read in table lengths
    zip_NEEDBITS(5);
    nl = 257 + zip_GETBITS(5);	// number of literal/length codes
    zip_DUMPBITS(5);
    zip_NEEDBITS(5);
    nd = 1 + zip_GETBITS(5);	// number of distance codes
    zip_DUMPBITS(5);
    zip_NEEDBITS(4);
    nb = 4 + zip_GETBITS(4);	// number of bit length codes
    zip_DUMPBITS(4);
    if(nl > 286 || nd > 30)
      return -1;		// bad lengths

    // read in bit-length-code lengths
    for(j = 0; j < nb; j++)
    {
	zip_NEEDBITS(3);
	ll[zip_border[j]] = zip_GETBITS(3);
	zip_DUMPBITS(3);
    }
    for(; j < 19; j++)
	ll[zip_border[j]] = 0;

    // build decoding table for trees--single level, 7 bit lookup
    zip_bl = 7;
    h = new zip_HuftBuild(ll, 19, 19, null, null, zip_bl);
    if(h.status != 0)
	return -1;	// incomplete code set

    zip_tl = h.root;
    zip_bl = h.m;

    // read in literal and distance code lengths
    n = nl + nd;
    i = l = 0;
    while(i < n) {
	zip_NEEDBITS(zip_bl);
	t = zip_tl.list[zip_GETBITS(zip_bl)];
	j = t.b;
	zip_DUMPBITS(j);
	j = t.n;
	if(j < 16)		// length of code in bits (0..15)
	    ll[i++] = l = j;	// save last length in l
	else if(j == 16) {	// repeat last length 3 to 6 times
	    zip_NEEDBITS(2);
	    j = 3 + zip_GETBITS(2);
	    zip_DUMPBITS(2);
	    if(i + j > n)
		return -1;
	    while(j-- > 0)
		ll[i++] = l;
	} else if(j == 17) {	// 3 to 10 zero length codes
	    zip_NEEDBITS(3);
	    j = 3 + zip_GETBITS(3);
	    zip_DUMPBITS(3);
	    if(i + j > n)
		return -1;
	    while(j-- > 0)
		ll[i++] = 0;
	    l = 0;
	} else {		// j == 18: 11 to 138 zero length codes
	    zip_NEEDBITS(7);
	    j = 11 + zip_GETBITS(7);
	    zip_DUMPBITS(7);
	    if(i + j > n)
		return -1;
	    while(j-- > 0)
		ll[i++] = 0;
	    l = 0;
	}
    }

    // build the decoding tables for literal/length and distance codes
    zip_bl = zip_lbits;
    h = new zip_HuftBuild(ll, nl, 257, zip_cplens, zip_cplext, zip_bl);
    if(zip_bl == 0)	// no literals or lengths
	h.status = 1;
    if(h.status != 0) {
	if(h.status == 1)
	    ;// **incomplete literal tree**
	return -1;		// incomplete code set
    }
    zip_tl = h.root;
    zip_bl = h.m;

    for(i = 0; i < nd; i++)
	ll[i] = ll[i + nl];
    zip_bd = zip_dbits;
    h = new zip_HuftBuild(ll, nd, 0, zip_cpdist, zip_cpdext, zip_bd);
    zip_td = h.root;
    zip_bd = h.m;

    if(zip_bd == 0 && nl > 257) {   // lengths but no distances
	// **incomplete distance tree**
	return -1;
    }

    if(h.status == 1) {
	;// **incomplete distance tree**
    }
    if(h.status != 0)
	return -1;

    // decompress until an end-of-block code
    return zip_inflate_codes(buff, off, size);
}

var zip_inflate_start = function() {
    var i;

    if(zip_slide == null)
	zip_slide = new Array(2 * zip_WSIZE);
    zip_wp = 0;
    zip_bit_buf = 0;
    zip_bit_len = 0;
    zip_method = -1;
    zip_eof = false;
    zip_copy_leng = zip_copy_dist = 0;
    zip_tl = null;
}

var zip_inflate_internal = function(buff, off, size) {
    // decompress an inflated entry
    var n, i;

    n = 0;
    while(n < size) {
	if(zip_eof && zip_method == -1)
	    return n;

	if(zip_copy_leng > 0) {
	    if(zip_method != zip_STORED_BLOCK) {
		// STATIC_TREES or DYN_TREES
		while(zip_copy_leng > 0 && n < size) {
		    zip_copy_leng--;
		    zip_copy_dist &= zip_WSIZE - 1;
		    zip_wp &= zip_WSIZE - 1;
		    buff[off + n++] = zip_slide[zip_wp++] =
			zip_slide[zip_copy_dist++];
		}
	    } else {
		while(zip_copy_leng > 0 && n < size) {
		    zip_copy_leng--;
		    zip_wp &= zip_WSIZE - 1;
		    zip_NEEDBITS(8);
		    buff[off + n++] = zip_slide[zip_wp++] = zip_GETBITS(8);
		    zip_DUMPBITS(8);
		}
		if(zip_copy_leng == 0)
		    zip_method = -1; // done
	    }
	    if(n == size)
		return n;
	}

	if(zip_method == -1) {
	    if(zip_eof)
		break;

	    // read in last block bit
	    zip_NEEDBITS(1);
	    if(zip_GETBITS(1) != 0)
		zip_eof = true;
	    zip_DUMPBITS(1);

	    // read in block type
	    zip_NEEDBITS(2);
	    zip_method = zip_GETBITS(2);
	    zip_DUMPBITS(2);
	    zip_tl = null;
	    zip_copy_leng = 0;
	}

	switch(zip_method) {
	  case 0: // zip_STORED_BLOCK
	    i = zip_inflate_stored(buff, off + n, size - n);
	    break;

	  case 1: // zip_STATIC_TREES
	    if(zip_tl != null)
		i = zip_inflate_codes(buff, off + n, size - n);
	    else
		i = zip_inflate_fixed(buff, off + n, size - n);
	    break;

	  case 2: // zip_DYN_TREES
	    if(zip_tl != null)
		i = zip_inflate_codes(buff, off + n, size - n);
	    else
		i = zip_inflate_dynamic(buff, off + n, size - n);
	    break;

	  default: // error
	    i = -1;
	    break;
	}

	if(i == -1) {
	    if(zip_eof)
		return 0;
	    return -1;
	}
	n += i;
    }
    return n;
}

var zip_inflate = function(str) {
    var i, j;

    zip_inflate_start();
    zip_inflate_data = str;
    zip_inflate_pos = 0;

    var buff = new Array(1024);
    var aout = [];
    while((i = zip_inflate_internal(buff, 0, buff.length)) > 0) {
	var cbuf = new Array(i);
	for(j = 0; j < i; j++){
	    cbuf[j] = String.fromCharCode(buff[j]);
	}
	aout[aout.length] = cbuf.join("");
    }
    zip_inflate_data = null; // G.C.
    return aout.join("");
}

if (! window.RawDeflate) RawDeflate = {};
RawDeflate.inflate = zip_inflate;

})();
;
/* 
 * editprofile.js
 * This code was moved (and edited) from wonga's core.js into the my-account 
 * module and made more generic, all references to css were removed 
 * (as they need to sit in the themes)
 * 
 * What this code does: in the 'personal details' and 'payment details' section of 'my account', 
 * when you hover on an editable section, the class of the section changes (to allow for some
 * css customization) and the "Edit [icon]" appears.
 */

$(document).ready( function () {

function editprofile() { 
					

      $('.hover-bar.modal').click( function() {

        //so we know what to delete from page if removing content, and where was clicked
        $(this).addClass('ajax-delete');
        $(this).addClass('ajax-clicked');
        $('.ajax_update').remove();

        if( $('.hover-link',this).length > 0 ) {

        var options = $().wonga('fancybox_options');
        options['href'] = $('a.hover-link',this).attr('href');
        $.fancybox( options );

        }
      });

      $('.hover-bar.not-modal').click( function() {

        if( $('.hover-link',this).length > 0 ) {
          var options = $().wonga('fancybox_options');
          location.href = $('a.hover-link',this).attr('href');
        }

      });

      $('.hover-bar').hover(
          function() {
            if( $(this).find('.hover-link').length > 0 ) {								
              $(this).addClass('hover');

              //quick fix to not show set primary when on credit card								
              if( $('.card-type-visa-credit',this).length > 0 ) {
                      $('.hover-link-primary',this).css('visibility','hidden');
              }
            }
          },
          function() {	 
            if( $(this).find('.hover-link').length > 0 ) {
              $(this).removeClass('hover');
            }
          }
      );
        
    } 
    
    editprofile();
    
    });

Drupal.behaviors.wonga_my_account_edit_profile = function(context) {
  $editLink = $('#wonga-update-postal-address-form a.edit-address', context);

  var ShowAddressContainer = function() {
    $('#fancybox-loading').hide();
    $('#postcode-container').parent().hide();
    $('#button-edit-submit-lookup').hide();
    $('#address-container').parent().slideDown();
    $('#button-edit-submit').show();
  };
  
  var enableAddressEdit = function() {
    $("input").removeAttr('readonly').removeClass('readonly');
    $('#edit-flat').parent().parent().show();
    $('#edit-house-name').parent().parent().show();
    $('#edit-house-number').parent().parent().show();
    $('#edit-postcode-uk').parent().parent().show();
    $('#edit-street').parent().parent().show();
    $('#edit-town').parent().parent().show();
  };

  // We only want these events bound if the modal is showing
  if ($('#wonga-update-postal-address-form').length > 0) {
    //click on the not listed options
    $(document).on('address-not-listed', function(e) {
      ShowAddressContainer();
      enableAddressEdit();
      $editLink.hide();
    });

    $editLink.click(function(e) {
      e.preventDefault();
      enableAddressEdit();
      $editLink.slideUp(200);
    });
  }
};
;
/*
 * jQuery Form Plugin
 * version: 2.25 (08-APR-2009)
 * @requires jQuery v1.2.2 or later
 * @note This has been modified for ajax.module
 * Examples and documentation at: http://malsup.com/jquery/form/
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
eval(function(p,a,c,k,e,r){e=function(c){return(c<a?'':e(parseInt(c/a)))+((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))};if(!''.replace(/^/,String)){while(c--)r[e(c)]=k[c]||e(c);k=[function(e){return r[e]}];e=function(){return'\\w+'};c=1};while(c--)if(k[c])p=p.replace(new RegExp('\\b'+e(c)+'\\b','g'),k[c]);return p}(';(5($){$.B.1s=5(u){2(!4.G){R(\'1b: 2M 9 2N - 2O 2P 1t\');6 4}2(S u==\'5\')u={T:u};3 v=4.14(\'1c\')||1d.2Q.2R;v=(v.2S(/^([^#]+)/)||[])[1];v=v||\'\';u=$.1n({1e:v,H:4.14(\'1u\')||\'1Q\'},u||{});3 w={};4.L(\'C-1R-1S\',[4,u,w]);2(w.1T){R(\'1b: 9 1U 1o C-1R-1S L\');6 4}2(u.1v&&u.1v(4,u)===I){R(\'1b: 9 1f 1o 1v 1V\');6 4}3 a=4.1w(u.2T);2(u.J){u.O=u.J;K(3 n 1x u.J){2(u.J[n]2U 15){K(3 k 1x u.J[n])a.D({7:n,8:u.J[n][k]})}E a.D({7:n,8:u.J[n]})}}2(u.1y&&u.1y(a,4,u)===I){R(\'1b: 9 1f 1o 1y 1V\');6 4}4.L(\'C-9-1W\',[a,4,u,w]);2(w.1T){R(\'1b: 9 1U 1o C-9-1W L\');6 4}3 q=$.1z(a);2(u.H.2V()==\'1Q\'){u.1e+=(u.1e.2W(\'?\')>=0?\'&\':\'?\')+q;u.J=F}E u.J=q;3 x=4,V=[];2(u.2X)V.D(5(){x.1X()});2(u.2Y)V.D(5(){x.1Y()});2(!u.16&&u.17){3 y=u.T||5(){};V.D(5(a){$(u.17).2Z(a).P(y,1Z)})}E 2(u.T)V.D(u.T);u.T=5(a,b){K(3 i=0,M=V.G;i<M;i++)V[i].30(u,[a,b,x])};3 z=$(\'W:31\',4).18();3 A=I;K(3 j=0;j<z.G;j++)2(z[j])A=Q;2(u.20||A){2(u.21)$.32(u.21,1A);E 1A()}E $.33(u);4.L(\'C-9-34\',[4,u]);6 4;5 1A(){3 h=x[0];2($(\':W[7=9]\',h).G){35(\'36: 37 22 38 39 3a 3b "9".\');6}3 i=$.1n({},$.23,u);3 s=$.1n(Q,{},$.1n(Q,{},$.23),i);3 j=\'3c\'+(1B 3d().3e());3 k=$(\'<20 3f="\'+j+\'" 7="\'+j+\'" 24="25:26" />\');3 l=k[0];k.3g({3h:\'3i\',27:\'-28\',29:\'-28\'});3 m={1f:0,19:F,1g:F,3j:0,3k:\'n/a\',3l:5(){},2a:5(){},3m:5(){},3n:5(){4.1f=1;k.14(\'24\',\'25:26\')}};3 g=i.2b;2(g&&!$.1C++)$.1h.L("3o");2(g)$.1h.L("3p",[m,i]);2(s.2c&&s.2c(m,s)===I){s.2b&&$.1C--;6}2(m.1f)6;3 o=0;3 p=0;3 q=h.U;2(q){3 n=q.7;2(n&&!q.1i){u.O=u.O||{};u.O[n]=q.8;2(q.H=="X"){u.O[7+\'.x\']=h.Y;u.O[7+\'.y\']=h.Z}}}1j(5(){3 t=x.14(\'17\'),a=x.14(\'1c\');h.1k(\'17\',j);2(h.2d(\'1u\')!=\'2e\')h.1k(\'1u\',\'2e\');2(h.2d(\'1c\')!=i.1e)h.1k(\'1c\',i.1e);2(!u.3q){x.14({3r:\'2f/C-J\',3s:\'2f/C-J\'})}2(i.1D)1j(5(){p=Q;11()},i.1D);3 b=[];2g{2(u.O)K(3 n 1x u.O)b.D($(\'<W H="3t" 7="\'+n+\'" 8="\'+u.O[n]+\'" />\').2h(h)[0]);k.2h(\'1l\');l.2i?l.2i(\'2j\',11):l.3u(\'2k\',11,I);h.9()}3v{h.1k(\'1c\',a);t?h.1k(\'17\',t):x.3w(\'17\');$(b).2l()}},10);3 r=0;5 11(){2(o++)6;l.2m?l.2m(\'2j\',11):l.3x(\'2k\',11,I);3 c=Q;2g{2(p)3y\'1D\';3 d,N;N=l.2n?l.2n.2o:l.2p?l.2p:l.2o;2((N.1l==F||N.1l.2q==\'\')&&!r){r=1;o--;1j(11,2r);6}m.19=N.1l?N.1l.2q:F;m.1g=N.2s?N.2s:N;m.2a=5(a){3 b={\'3z-H\':i.16};6 b[a]};2(i.16==\'3A\'||i.16==\'3B\'){3 f=N.1E(\'1F\')[0];m.19=f?f.8:m.19}E 2(i.16==\'2t\'&&!m.1g&&m.19!=F){m.1g=2u(m.19)}d=$.3C(m,i.16)}3D(e){c=I;$.3E(i,m,\'2v\',e)}2(c){i.T(d,\'T\');2(g)$.1h.L("3F",[m,i])}2(g)$.1h.L("3G",[m,i]);2(g&&!--$.1C)$.1h.L("3H");2(i.2w)i.2w(m,c?\'T\':\'2v\');1j(5(){k.2l();m.1g=F},2r)};5 2u(s,a){2(1d.2x){a=1B 2x(\'3I.3J\');a.3K=\'I\';a.3L(s)}E a=(1B 3M()).3N(s,\'1G/2t\');6(a&&a.2y&&a.2y.1p!=\'3O\')?a:F}}};$.B.3P=5(c){6 4.2z().2A(\'9.C-1q\',5(){$(4).1s(c);6 I}).P(5(){$(":9,W:X",4).2A(\'2B.C-1q\',5(e){3 a=4.C;a.U=4;2(4.H==\'X\'){2(e.2C!=12){a.Y=e.2C;a.Z=e.3Q}E 2(S $.B.2D==\'5\'){3 b=$(4).2D();a.Y=e.2E-b.29;a.Z=e.2F-b.27}E{a.Y=e.2E-4.3R;a.Z=e.2F-4.3S}}1j(5(){a.U=a.Y=a.Z=F},10)})})};$.B.2z=5(){4.2G(\'9.C-1q\');6 4.P(5(){$(":9,W:X",4).2G(\'2B.C-1q\')})};$.B.1w=5(b){3 a=[];2(4.G==0)6 a;3 c=4[0];3 d=b?c.1E(\'*\'):c.22;2(!d)6 a;K(3 i=0,M=d.G;i<M;i++){3 e=d[i];3 n=e.7;2(!n)1H;2(b&&c.U&&e.H=="X"){2(!e.1i&&c.U==e)a.D({7:n+\'.x\',8:c.Y},{7:n+\'.y\',8:c.Z});1H}3 v=$.18(e,Q);2(v&&v.1r==15){K(3 j=0,2H=v.G;j<2H;j++)a.D({7:n,8:v[j]})}E 2(v!==F&&S v!=\'12\')a.D({7:n,8:v})}2(!b&&c.U){3 f=c.1E("W");K(3 i=0,M=f.G;i<M;i++){3 g=f[i];3 n=g.7;2(n&&!g.1i&&g.H=="X"&&c.U==g)a.D({7:n+\'.x\',8:c.Y},{7:n+\'.y\',8:c.Z})}}6 a};$.B.3T=5(a){6 $.1z(4.1w(a))};$.B.3U=5(b){3 a=[];4.P(5(){3 n=4.7;2(!n)6;3 v=$.18(4,b);2(v&&v.1r==15){K(3 i=0,M=v.G;i<M;i++)a.D({7:n,8:v[i]})}E 2(v!==F&&S v!=\'12\')a.D({7:4.7,8:v})});6 $.1z(a)};$.B.18=5(a){K(3 b=[],i=0,M=4.G;i<M;i++){3 c=4[i];3 v=$.18(c,a);2(v===F||S v==\'12\'||(v.1r==15&&!v.G))1H;v.1r==15?$.3V(b,v):b.D(v)}6 b};$.18=5(b,c){3 n=b.7,t=b.H,1a=b.1p.1I();2(S c==\'12\')c=Q;2(c&&(!n||b.1i||t==\'1m\'||t==\'3W\'||(t==\'1J\'||t==\'1K\')&&!b.1L||(t==\'9\'||t==\'X\')&&b.C&&b.C.U!=b||1a==\'13\'&&b.1M==-1))6 F;2(1a==\'13\'){3 d=b.1M;2(d<0)6 F;3 a=[],1N=b.3X;3 e=(t==\'13-2I\');3 f=(e?d+1:1N.G);K(3 i=(e?d:0);i<f;i++){3 g=1N[i];2(g.1t){3 v=g.8;2(!v)v=(g.1O&&g.1O[\'8\']&&!(g.1O[\'8\'].3Y))?g.1G:g.8;2(e)6 v;a.D(v)}}6 a}6 b.8};$.B.1Y=5(){6 4.P(5(){$(\'W,13,1F\',4).2J()})};$.B.2J=$.B.3Z=5(){6 4.P(5(){3 t=4.H,1a=4.1p.1I();2(t==\'1G\'||t==\'40\'||1a==\'1F\')4.8=\'\';E 2(t==\'1J\'||t==\'1K\')4.1L=I;E 2(1a==\'13\')4.1M=-1})};$.B.1X=5(){6 4.P(5(){2(S 4.1m==\'5\'||(S 4.1m==\'41\'&&!4.1m.42))4.1m()})};$.B.43=5(b){2(b==12)b=Q;6 4.P(5(){4.1i=!b})};$.B.2K=5(b){2(b==12)b=Q;6 4.P(5(){3 t=4.H;2(t==\'1J\'||t==\'1K\')4.1L=b;E 2(4.1p.1I()==\'2L\'){3 a=$(4).44(\'13\');2(b&&a[0]&&a[0].H==\'13-2I\'){a.45(\'2L\').2K(I)}4.1t=b}})};5 R(){2($.B.1s.46&&1d.1P&&1d.1P.R)1d.1P.R(\'[47.C] \'+15.48.49.4a(1Z,\'\'))}})(4b);',62,260,'||if|var|this|function|return|name|value|submit||||||||||||||||||||||||||||fn|form|push|else|null|length|type|false|data|for|trigger|max|doc|extraData|each|true|log|typeof|success|clk|callbacks|input|image|clk_x|clk_y||cb|undefined|select|attr|Array|dataType|target|a_fieldValue|responseText|tag|ajaxSubmit|action|window|url|aborted|responseXML|event|disabled|setTimeout|setAttribute|body|reset|extend|via|tagName|plugin|constructor|a_ajaxSubmit|selected|method|beforeSerialize|a_formToArray|in|beforeSubmit|param|fileUpload|new|active|timeout|getElementsByTagName|textarea|text|continue|toLowerCase|checkbox|radio|checked|selectedIndex|ops|attributes|console|GET|pre|serialize|veto|vetoed|callback|validate|a_resetForm|a_clearForm|arguments|iframe|closeKeepAlive|elements|ajaxSettings|src|about|blank|top|1000px|left|getResponseHeader|global|beforeSend|getAttribute|POST|multipart|try|appendTo|attachEvent|onload|load|remove|detachEvent|contentWindow|document|contentDocument|innerHTML|100|XMLDocument|xml|toXml|error|complete|ActiveXObject|documentElement|a_ajaxFormUnbind|bind|click|offsetX|offset|pageX|pageY|unbind|jmax|one|a_clearFields|a_selected|option|skipping|process|no|element|location|href|match|semantic|instanceof|toUpperCase|indexOf|resetForm|clearForm|html|apply|file|get|ajax|notify|alert|Error|Form|must|not|be|named|jqFormIO|Date|getTime|id|css|position|absolute|status|statusText|getAllResponseHeaders|setRequestHeader|abort|ajaxStart|ajaxSend|skipEncodingOverride|encoding|enctype|hidden|addEventListener|finally|removeAttr|removeEventListener|throw|content|json|script|httpData|catch|handleError|ajaxSuccess|ajaxComplete|ajaxStop|Microsoft|XMLDOM|async|loadXML|DOMParser|parseFromString|parsererror|a_ajaxForm|offsetY|offsetLeft|offsetTop|a_formSerialize|a_fieldSerialize|merge|button|options|specified|a_clearInputs|password|object|nodeType|a_enable|parent|find|debug|jquery|prototype|join|call|jQuery'.split('|'),0,{}));
/**
 * Automatic ajax validation
 *
 * @see http://drupal.org/project/ajax
 * @see irc://freenode.net/#drupy
 * @depends Drupal 6
 * @author brendoncrawford
 * @note This file uses a 79 character width limit.
 * 
 *
 */

Drupal.Ajax = new Object;

Drupal.Ajax.plugins = {};

Drupal.Ajax.firstRun = false;

/**
 * Init function.
 * This is being executed by Drupal behaviours.
 * See bottom of script.
 * 
 * @param {HTMLElement} context
 * @return {Bool}
 */
Drupal.Ajax.init = function(context) {
  
  var f, s;
  if (f = $('.ajax-form', context)) {
    if (!Drupal.Ajax.firstRun) {
      Drupal.Ajax.invoke('init');
      Drupal.Ajax.firstRun = true;
    }
    s = $('input[type="submit"]', f);
    s.click(function(){
      this.form.ajax_activator = $(this);
      return true;
    });
    f.each(function(){
      this.ajax_activator = null;
      $(this).submit(function(){
        if (this.ajax_activator === null) {
          this.ajax_activator = $('#edit-submit', this);
        }
        if (this.ajax_activator.hasClass('ajax-trigger')) {
          Drupal.Ajax.go($(this), this.ajax_activator);
          return false;
        }
        else {
          return true;
        }
      });
      return true;
    });
  }
  return true;
};

/**
 * Invokes plugins
 * 
 * @param {Object} formObj
 * @param {Object} submitter
 */
Drupal.Ajax.invoke = function(hook, args) {
  var plugin, r, ret;
  ret = true;
  for (plugin in Drupal.Ajax.plugins) {
    r = Drupal.Ajax.plugins[plugin](hook, args);
    if (r === false) {
      ret = false;
    }
  }
  return ret;
};

/**
 * Handles submission
 * 
 * @param {Object} submitter_
 * @return {Bool}
 */
Drupal.Ajax.go = function(formObj, submitter) {

	//dont submit if js validation returns false
	if(formObj.valid() == false )
	{
	return;
	}

  var submitterVal, submitterName, extraData;
  Drupal.Ajax.invoke('submit', {submitter:submitter});
  submitterVal = submitter.val();
  submitterName = submitter.attr('name');
  
	//add ajax loader to sumitter
	submitter.before('<div class="ajax-loader"></div>');
	
  extraData = {};
  extraData[submitterName] = submitterVal;
  extraData['drupal_ajax'] = '1';
	
  formObj.a_ajaxSubmit({
    extraData : extraData,
    beforeSubmit : function(data) {
      data[data.length] = {
        name : submitterName,
        value : submitterVal
      };
      data[data.length] = {
        name : 'drupal_ajax',
        value : '1'
      };
			submitter.attr('disabled', 'disabled');
      return true;
    },
    dataType : 'json',
    error: function (XMLHttpRequest, textStatus, errorThrown) {

			//log ajax error to watchdog here
			$.ajax({
					url: window.location.protocol + '//' + window.location.hostname+'/wonga-drupal-error/ajax popup failure/A popup failed to complete while requesting ['+formObj.context.action+']',
					success: function(data) {						
					}
			});

			//show generic error
			$('#fancybox-content').html('<h1>An error occured</h1><p>We have had a problem updating your details.  Please try again.</p>');
			      
      return true;
    },
    success: function(data){
      Drupal.Ajax.response(submitter, formObj, data);
      return true;
    }
  });
  return false;
};

/**
 * Handles scroller
 * 
 * @param {Object} submitter
 * @return {Bool}
 */
Drupal.Ajax.scroller = function(submitter) {
  var scroll_weight, box, found, timer;
  scroll_weight = 100;
  timer = window.setInterval(function() {
    box = submitter;
    found = false;
    // Watch for thickbox
    while (box.parentNode !== null &&
        Drupal.Ajax.invoke('scrollFind', {container:box})) {
      box = box.parentNode;
      // Document
      if (box === document) {
        if (box.documentElement.scrollTop &&
            box.documentElement.scrollTop > 0) {
          box.documentElement.scrollTop -= scroll_weight;
          found = true;
        }
      }
      // Body
      else if (box === document.body) {
        if (box.scrollTop &&
            box.scrollTop > 0) {
          box.scrollTop -= scroll_weight;
          found = true;
        }
      }
      // Window
      else if (box === window) {
        if ((window.pageYOffset && window.pageYOffset > 0) ||
            (window.scrollY && window.scrollY > 0)) {
          window.scrollBy(0, -scroll_weight);
          found = true;
        }
      }
      // Any other element
      else {
        if (box.scrollTop &&
            box.scrollTop > 0) {
          box.scrollTop -= scroll_weight;
          found = true;
        }
      }
    }
    // Check if completed
    if (!found) {
      window.clearInterval(timer);
    }
    return true;
  }, 100);
  return true;
};

/**
 * Handles messaging
 * 
 * @param {Object} formObj
 * @param {Object} submitter
 * @param {Object} data
 * @param {Object} options
 * @return {Bool}
 */
Drupal.Ajax.message = function(formObj, submitter, data, options) {
  var args;
  args = {
    formObj : formObj,
    submitter : submitter,
    data : data,
    options : options
  };
  if (Drupal.Ajax.invoke('message', args)) {
    Drupal.Ajax.writeMessage(args.formObj, args.submitter, args.options);
  }
  return true;
};

/**
 * Writes message
 * 
 * @param {Object} formObj
 * @param {Object} submitter
 * @param {Object} options
 * @return {Bool}
 */
Drupal.Ajax.writeMessage = function(formObj, submitter, options) {
  var i, _i, thisItem, log, errBox, h;
  if (options.action === 'notify') {
    // Cleanups
    $('.messages, .ajax-preview', formObj).remove();
    $('input, textarea').removeClass('error status warning required');
    // Preview
    if (options.type === 'preview') {
      log = $('<div>').addClass('ajax-preview');
      log.html(options.messages);
      formObj.prepend(log);
    }
    // Status, Error, Message
    else {
      log = $('<ul>');
      errBox = $(".messages." + options.type, formObj[0])
      for (i = 0, _i = options.messages.length; i < _i; i++) {
        thisItem = $('#' + options.messages[i].id, formObj[0])
        thisItem.addClass(options.type);
        if (options.messages[i].required) {
          thisItem.addClass('required');
        }
        log.append('<li>' + options.messages[i].value + '</li>');
      }
      if (errBox.length === 0) {
        errBox = $("<div class='messages " + options.type + "'>");
        formObj.prepend(errBox);
      }
      errBox.html(log);
    }
  }
  else if (options.action === 'clear') {
    $('.messages, .ajax-preview', formObj).remove();
  }
  Drupal.Ajax.scroller(submitter[0]);
  return true;
};

/**
 * Updates message containers
 * 
 * @param {Object} updaters
 * @return {Bool}
 */
Drupal.Ajax.updater = function(updaters) {
  var i, _i, elm;
  for (i = 0, _i = updaters.length; i < _i; i++) {
    elm = $(updaters[i].selector);
    // HTML:IN
    if (updaters[i].type === 'html_in') {
      elm.html(updaters[i].value);
    }
    // HTML:OUT
    else if (updaters[i].type === 'html_out') {
      elm.replaceWith(updaters[i].value);
    }
    // FIELD
    else if (updaters[i].type === 'field') {
      elm.val(updaters[i].value);
    }
    // REMOVE
    else if(updaters[i].type === 'remove') {
      elm.remove();
    }
  }
  return true;
};

/**
 * Handles data response
 * 
 * @param {Object} submitter
 * @param {Object} formObj
 * @param {Object} data
 * @return {Bool}
 */
Drupal.Ajax.response = function(submitter, formObj, data){

  var newSubmitter;
  data.local = {
    submitter : submitter,
    form : formObj
  };
  /**
   * Failure
   */
  if (data.status === false) {	  
		
    Drupal.Ajax.updater(data.updaters);
    Drupal.Ajax.message(formObj, submitter, data, {
      action : 'notify',
      messages : data.messages_error,
      type : 'error'
    });
		
		$('.ajax-loader').remove();
		submitter.removeAttr('disabled');
  }
  /**
   * Success
   */
  else {
  
    // Display preview
    if (data.preview !== null) {			
			Drupal.Ajax.updater(data.updaters);
      Drupal.Ajax.message(formObj, submitter, data, {
        action : 'notify',
        messages : decodeURIComponent(data.preview),
        type : 'preview'
      });
						
			$('.ajax-loader').remove();
			submitter.removeAttr('disabled');
    }
    // If no redirect, then simply show messages
    else if (data.redirect === null) {
      
      $.fancybox.close();
      //location.reload();
      $('.ajax-loader').remove(); 
			
      /*
      if (data.messages_status.length > 0) {
        Drupal.Ajax.message(formObj, submitter, data, {
          action : 'notify',
          messages : data.messages_status,
          type : 'status'
        });
      }
      if (data.messages_warning.length > 0) {
        Drupal.Ajax.message(formObj, submitter, data, {
          action : 'notify',
          messages : data.messages_warning,
          type : 'warning'
        });
      }
      if (data.messages_status.length === 0 &&
          data.messages_warning.length === 0) {
        Drupal.Ajax.message(formObj, submitter, data, {action:'clear'});
      }
      */
    }
    // Redirect
    else {    	
			
			var hostname = window.location.hostname;
			var port = window.location.port;
			
			if(port) {
				data.redirect = data.redirect.replace(hostname,hostname + ':' + port);
			}
		
      if (Drupal.Ajax.invoke('complete', data)) {
    	
			
    	//old code that just redirected
        //Drupal.Ajax.redirect( data.redirect );
        
        //get the contents via ajax and add the mod=1 so its simple theme
        //add contents from call back into the form 
				
        
        var redirect = '';
        if( data.redirect.indexOf('?') != -1 ) {
          redirect = data.redirect + '&mod=1';
        } else {
          redirect = data.redirect + '?mod=1';	
        }
        
    	$.ajax({
    	  url: redirect,
    	  context: $('#fancybox-content'),
    	  success: function(data){   		  
	    	$(this).html(data);
	    	
        if ($().account_val) {
          $().account_val(); // call js validation 
        }
				$().wonga('simple_ajax_links');
	    	Drupal.Ajax.init();
        $.fancybox.center();
				
				$("#fancybox-wrap").easydrag();
				$("#fancybox-wrap").setHandler('h1');																														
				
				$('.ajax-loader').remove();
				$().wonga('bind_close_fancybox');
				
    	  }
        }); //ajax call
      }
      else {
			  
        Drupal.Ajax.updater(data.updaters);
        if (data.messages_status.length === 0 &&
            data.messages_warning.length === 0) {        
          Drupal.Ajax.message(formObj, submitter, data, {action:'clear'});
        }
        else {        					
          Drupal.Ajax.message(formObj, submitter, data, {
            action : 'notify',
            messages : data.messages_status,
            type : 'status'
          });
        }
				
				$('.ajax-loader').remove();
				submitter.removeAttr('disabled');
				
      }
    }
  }
  return true;
};


/**
 * Redirects to appropriate page
 * 
 * @todo
 *   Some of this functionality should possibly hapen on
 *   the server instead of client.
 * @param {String} url
 */
Drupal.Ajax.redirect = function(url) {
  window.location.href = url;
};

Drupal.behaviors.Ajax = Drupal.Ajax.init;


;
(function($, Drupal) {
  Drupal.WongaFinance = Drupal.WongaFinance || {};
  Drupal.WongaFinance.Currency = Drupal.WongaFinance.Currency || {};

  /**
   * The type of objects containing settings for a currency formatting
   * operation.
   *
   * @typedef {object} FinanceCurrencyFormatSettings
   *
   * @property {string} decimalSymbol
   *  The string which should be used to separate the whole and fractional
   *  parts of a number. Defaults to a period ('.').
   *
   * @property {string} format
   *  A format string with which formatting should be performed. The special
   *  values '%s' and '%v' will be replaced with the currency symbol and
   *  formatted value respectively.
   *
   * @property {string} groupSeparator
   *  The string which should be used to group sets of three digits, as seen in
   *  larger numbers. Defaults to a comma (',').
   *
   * @property {number} precision
   *  The precision with which values should be formatted.
   *
   * @property {string} symbol
   *  The currency symbol to use.
   */

  /**
   * Formats a given value as a currency.
   *
   * @param {number} x
   *  The number to format.
   *
   * @param {FinanceCurrencyFormatSettings} settings
   *  A set of settings against which the formatting should be performed.
   *
   * @return {string}
   *  A formatted string.
   */
  Drupal.WongaFinance.Currency.format = function(x, settings) {
    var defaults = {
          decimalSymbol: '.',
          format: '%s%v',
          groupSeparator: ',',
          precision: 2,
          symbol: '£'
        },

        settings = $.extend({}, defaults, settings),
        groupDigits = function(n) {
          var s = n + '',
              parts = s.split('.'),
              whole = parts[0],
              fraction = parts.length > 1 ?
                settings.decimalSymbol + parts[1] : '',

              pattern = /(\d+)(\d{3})/;

          while (pattern.test(whole)) {
            whole = whole.replace(pattern, '$1' + settings.groupSeparator + '$2');
          }

          return whole + fraction;
        },

        formatted = groupDigits(
          (Math.ceil(x * 100) / 100).toFixed(settings.precision));
//        return 'aaa';

    return settings.format.
      replace('%s', settings.symbol).replace('%v', formatted);
  };
})(jQuery, Drupal);
;
(function($, Drupal) {
  Drupal.WongaFinance = Drupal.WongaFinance || {};
  Drupal.WongaFinance.SimpleInterest =
    Drupal.WongaFinance.SimpleInterest || {};

  /**
   * The type of objects returned by
   * Drupal.WongaFinance.SimpleInterest.calculate().
   *
   * @typedef {object} FinanceSimpleInterestCalculationResult
   *
   * @property {number} actualInterestRate
   *  The actual interest rate being charged (i.e., the rate of interest
   *  multipled by the term).
   *
   * @property {number} fees
   *  The total amount of flat fees that will be incurred.
   *
   * @property {number} feesAndInterest
   *  The total amount of flat fees and interest that will be incurred.
   *
   * @property {number} feeRate
   *  The rate with which flat fees were calculated.
   *
   * @property {number} interest
   *  The total amount of interest that will be incurred.
   *
   * @property {number} interestRate
   *  The rate at which interest was calculated.
   *
   * @property {number} principal
   *  The principal value (i.e., that upon which interest was charged).
   *
   * @property {number} repayment
   *  The amount that would be owed per repayment if payments were to be made
   *  once per unit of interest charged.
   *
   * @property {number} total
   *  The total amount owed (i.e., the sum of the principal, any flat fees and
   *  any interest).
   *
   * @property {number} units
   *  The number of units over which interest was charged.
   */

  /**
   * Calculates a set of costs and values associated with the application of
   * simple interest (i.e., where interest does not itself accrue interest).
   *
   * @param {number} principal
   *  The principal value (i.e., that upon which interest will be charged).
   *
   * @param {number} interestRate
   *  The interest rate, as a fractional decimal. For example, 0.05 would
   *  correspond to a 5% interest rate.
   *
   * @param {number} units
   *  The number of units for which interest will be charged. This might
   *  correspond to a number of weeks, months or years, for example.
   *
   * @param {number} feeRate
   *  An optional flat fee rate, as a fractional decimal. The default is 0
   *  (i.e., no flat fee).
   *
   * @return {FinanceSimpleInterestCalculationResult}
   *  An object containing the relevant costs and calculated values.
   */
  Drupal.WongaFinance.SimpleInterest.calculate = function(principal,
    interestRate, units, feeRate) {

    var actualInterestRate = interestRate * units * 100,
        feeRate = typeof feeRate === 'undefined' ? 0 : feeRate,
        fees = feeRate * principal,
        interest = interestRate * units * principal,
        feesAndInterest = parseFloat(fees) + parseFloat(interest),
        total = parseFloat(principal) + feesAndInterest,
        repayment = total / units;

    return {
      actualInterestRate: actualInterestRate.toPrecision(10),
      fees: fees.toPrecision(10),
      feesAndInterest: feesAndInterest,
      feeRate: feeRate,
      interest: interest.toPrecision(10),
      interestRate: interestRate,
      principal: principal,
      repayment: repayment,
      total: total,
      units: units
    };
  };
})(jQuery, Drupal);
;
(function($, Drupal) {
  Drupal.WongaSlidersWB = Drupal.WongaSlidersWB || {};

  Drupal.WongaSlidersWB.makeSummaryUpdater = function(settings) {
    var summarySelector = settings.parentSelector + ' .summary',
        overviewSelector = settings.parentSelector + ' .overview',

        $summary = $(summarySelector),
        $overview = $(overviewSelector);

    return function() {
      var sliders = Drupal.WongaSliders.sliders,
          amountSliderObjects = sliders[settings.amountSliderName],
          durationSliderObjects = sliders[settings.durationSliderName],
          product = settings.product;

      if (amountSliderObjects && durationSliderObjects && product) {
        var amountSlider = amountSliderObjects.model,
            durationSlider = durationSliderObjects.model,

            // Since the product's weekly interest rate and fee are returned
            // as whole percentages and not fractions (e.g. 3.00 instead of
            // 0.03), we must scale them before passing them off to the
            // calculate() function.
            calculation =
              Drupal.WongaFinance.SimpleInterest.calculate(
                amountSlider.getOffset(), product.WeeklyInterest / 100,
                durationSlider.getOffset(), product.Fee / 100),

            format = Drupal.WongaFinance.Currency.format;

        $summary.
          find('.amount').html(format(calculation.principal)).end().
          find('.fee').html(format(calculation.fees)).end().
          find('.interest').
            html(format(calculation.interest)).end().

          find('.interestRate').html(
            format(calculation.actualInterestRate,
              { symbol: '' })).end().

          find('.total').html(format(calculation.total));

        $overview.
          find('.length').html(calculation.units).end().
          find('.repayment').html(format(calculation.repayment));
      }
    };
  };
})(jQuery, Drupal);
;
(function($, Drupal) {
  Drupal.WongaSliders = Drupal.WongaSliders || {};
  Drupal.WongaSliders.Factories = Drupal.WongaSliders.Factories || {};

  Drupal.WongaSliders.Factories.everline = function(settings) {
    $.extend(settings, {
      attach: function(inputParent, elements) {
        var elementParent = elements.E.parentNode,
            elementSiblings = elementParent.children,
            firstSibling = elementSiblings[0],
            lastButOneSibling = elementSiblings[elementSiblings.length - 2];

        firstSibling.appendChild(elements.D);
        lastButOneSibling.appendChild(elements.I);

        $('.slider-cont', inputParent).append(elements.T);
      },

      buttonType: 'span',
      decrementButtonClassName: 'minus icon-minus',
      decrementButtonInnerHTML: '',
      handleClassName: 'ui-slider-handle ui-state-default ui-corner-all',
      incrementButtonClassName: 'plus icon-plus',
      incrementButtonInnerHTML: '',
      rangeClassName:
        'ui-slider-range ui-widget-header ui-corner-all ui-slider-range-min',

      trackClassName: 'slider ui-slider ui-slider-horizontal ' +
        'ui-widget ui-widget-content ui-corner-all',

      valueToString: function(value) {
        var valueString = value ?
              value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',') : '0';

        return settings.prefix + valueString;
      },

      stringToValue: function(string) {
        return parseInt(string.replace(/[£.,]/g, ''));
      }
    });

    var slider = new Slider(settings),
        view = new slider.View(settings);

    return {
      model: slider,
      view: view
    };
  };

  Drupal.behaviors.everlineSliders = function(context) {
    $("#edit-everline-loan-amount, #edit-loan-amount", context).numeric().blur(function() {
      $(this).trigger('change');
    });
    $("#edit-everline-loan-duration, #edit-loan-duration", context).numeric().blur(function() {
      $(this).trigger('change');
    });
    $('.header-sliders.sliders').not('.everline-sliders-processed').
      addClass('everline-sliders-processed').each(function() {
        var $document = $(document),
            $limit = $('.js-sole'),

            settings = Drupal.settings.everline,
            product = settings.product,
            threshold = settings.threshold,

            updateSettings = {
              amountSliderName: 'everline_loan_amount',
              durationSliderName: 'everline_loan_duration',
              parentSelector: $(this).selector,
              product: product
            },

            toggleLimit = function(e, model) {
              if (model.name === 'everline_loan_amount') {
                if (model.getOffset() > threshold) {
                  $limit.show();
                }
                else {
                  $limit.hide();
                }
              }
            },

            updateLoanSummaries =
              Drupal.WongaSlidersWB.makeSummaryUpdater(updateSettings);

        $document.on('wongaSliders.change', toggleLimit);
        $document.on('wongaSliders.change', updateLoanSummaries);
      });
  };
})(jQuery, Drupal);
;
(function($, Drupal) {
  Drupal.WongaSliders = Drupal.WongaSliders || {};
  Drupal.WongaSliders.sliders = Drupal.WongaSliders.sliders || {};

  /**
   * Updates the views of all sliders on the page.
   */
  Drupal.WongaSliders.updateAllViews = function() {
    $.each(Drupal.WongaSliders.sliders, function(name, objects) {
      if (objects.model) {
        objects.model.setOffset(objects.model.getOffset());
      }
    });
  };

  /**
   * Raises custom event with jQuery when a slider changes
   *
   * @param {Slider~model} model
   */
  Drupal.WongaSliders.raiseEventOnSetOffset = function(model) {
    $(document).trigger('wongaSliders.change', this);
  };
  
  /**
   * A Drupal behaviour for creating sliders for any FAPI form elements which
   * have settings in Drupal.settings.wongaSliders (created as a result of
   * #slider* attributes).
   */
  Drupal.behaviors.wongaSliders = function(context) {
    $('body').not('.wonga-sliders-processed').
      addClass('wonga-sliders-processed').each(function() {
        $.each(Drupal.settings.wongaSliders, function(name, slider) {
          Drupal.WongaSliders.sliders[name] =
            Drupal.WongaSliders.Factories[slider.type](slider.settings);
        });
      });
    // Binds window resize events to trigger all sliders to update.
    $(window).on('orientationchange resize', Drupal.WongaSliders.updateAllViews);
  };
})(jQuery, Drupal);
;
/*! Hammer.JS - v1.0.6dev - 2013-07-31
 * http://eightmedia.github.com/hammer.js
 *
 * Copyright (c) 2013 Jorik Tangelder <j.tangelder@gmail.com>;
 * Licensed under the MIT license */

(function(t,e){"use strict";function n(){if(!i.READY){i.event.determineEventTypes();for(var t in i.gestures)i.gestures.hasOwnProperty(t)&&i.detection.register(i.gestures[t]);i.event.onTouch(i.DOCUMENT,i.EVENT_MOVE,i.detection.detect),i.event.onTouch(i.DOCUMENT,i.EVENT_END,i.detection.detect),i.READY=!0}}var i=function(t,e){return new i.Instance(t,e||{})};i.defaults={stop_browser_behavior:{userSelect:"none",touchAction:"none",touchCallout:"none",contentZooming:"none",userDrag:"none",tapHighlightColor:"rgba(0,0,0,0)"}},i.HAS_POINTEREVENTS=t.navigator.pointerEnabled||t.navigator.msPointerEnabled,i.HAS_TOUCHEVENTS="ontouchstart"in t,i.MOBILE_REGEX=/mobile|tablet|ip(ad|hone|od)|android|silk/i,i.NO_MOUSEEVENTS=i.HAS_TOUCHEVENTS&&t.navigator.userAgent.match(i.MOBILE_REGEX),i.EVENT_TYPES={},i.DIRECTION_DOWN="down",i.DIRECTION_LEFT="left",i.DIRECTION_UP="up",i.DIRECTION_RIGHT="right",i.POINTER_MOUSE="mouse",i.POINTER_TOUCH="touch",i.POINTER_PEN="pen",i.EVENT_START="start",i.EVENT_MOVE="move",i.EVENT_END="end",i.DOCUMENT=t.document,i.plugins={},i.READY=!1,i.Instance=function(t,e){var r=this;return n(),this.element=t,this.enabled=!0,this.options=i.utils.extend(i.utils.extend({},i.defaults),e||{}),this.options.stop_browser_behavior&&i.utils.stopDefaultBrowserBehavior(this.element,this.options.stop_browser_behavior),i.event.onTouch(t,i.EVENT_START,function(t){r.enabled&&i.detection.startDetect(r,t)}),this},i.Instance.prototype={on:function(t,e){for(var n=t.split(" "),i=0;n.length>i;i++)this.element.addEventListener(n[i],e,!1);return this},off:function(t,e){for(var n=t.split(" "),i=0;n.length>i;i++)this.element.removeEventListener(n[i],e,!1);return this},trigger:function(t,e){var n=i.DOCUMENT.createEvent("Event");n.initEvent(t,!0,!0),n.gesture=e;var r=this.element;return i.utils.hasParent(e.target,r)&&(r=e.target),r.dispatchEvent(n),this},enable:function(t){return this.enabled=t,this}};var r=null,o=!1,s=!1;i.event={bindDom:function(t,e,n){for(var i=e.split(" "),r=0;i.length>r;r++)t.addEventListener(i[r],n,!1)},onTouch:function(t,e,n){var a=this;this.bindDom(t,i.EVENT_TYPES[e],function(c){var u=c.type.toLowerCase();if(!u.match(/mouse/)||!s){u.match(/touch/)||u.match(/pointerdown/)||u.match(/mouse/)&&1===c.which?o=!0:u.match(/mouse/)&&1!==c.which&&(o=!1),u.match(/touch|pointer/)&&(s=!0);var h=0;o&&(i.HAS_POINTEREVENTS&&e!=i.EVENT_END?h=i.PointerEvent.updatePointer(e,c):u.match(/touch/)?h=c.touches.length:s||(h=u.match(/up/)?0:1),h>0&&e==i.EVENT_END?e=i.EVENT_MOVE:h||(e=i.EVENT_END),(h||null===r)&&(r=c),n.call(i.detection,a.collectEventData(t,e,a.getTouchList(r,e),c)),i.HAS_POINTEREVENTS&&e==i.EVENT_END&&(h=i.PointerEvent.updatePointer(e,c))),h||(r=null,o=!1,s=!1,i.PointerEvent.reset())}})},determineEventTypes:function(){var t;t=i.HAS_POINTEREVENTS?i.PointerEvent.getEvents():i.NO_MOUSEEVENTS?["touchstart","touchmove","touchend touchcancel"]:["touchstart mousedown","touchmove mousemove","touchend touchcancel mouseup"],i.EVENT_TYPES[i.EVENT_START]=t[0],i.EVENT_TYPES[i.EVENT_MOVE]=t[1],i.EVENT_TYPES[i.EVENT_END]=t[2]},getTouchList:function(t){return i.HAS_POINTEREVENTS?i.PointerEvent.getTouchList():t.touches?t.touches:(t.indentifier=1,[t])},collectEventData:function(t,e,n,r){var o=i.POINTER_TOUCH;return(r.type.match(/mouse/)||i.PointerEvent.matchType(i.POINTER_MOUSE,r))&&(o=i.POINTER_MOUSE),{center:i.utils.getCenter(n),timeStamp:(new Date).getTime(),target:r.target,touches:n,eventType:e,pointerType:o,srcEvent:r,preventDefault:function(){this.srcEvent.preventManipulation&&this.srcEvent.preventManipulation(),this.srcEvent.preventDefault&&this.srcEvent.preventDefault()},stopPropagation:function(){this.srcEvent.stopPropagation()},stopDetect:function(){return i.detection.stopDetect()}}}},i.PointerEvent={pointers:{},getTouchList:function(){var t=this,e=[];return Object.keys(t.pointers).sort().forEach(function(n){e.push(t.pointers[n])}),e},updatePointer:function(t,e){return t==i.EVENT_END?this.pointers={}:(e.identifier=e.pointerId,this.pointers[e.pointerId]=e),Object.keys(this.pointers).length},matchType:function(t,e){if(!e.pointerType)return!1;var n={};return n[i.POINTER_MOUSE]=e.pointerType==e.MSPOINTER_TYPE_MOUSE||e.pointerType==i.POINTER_MOUSE,n[i.POINTER_TOUCH]=e.pointerType==e.MSPOINTER_TYPE_TOUCH||e.pointerType==i.POINTER_TOUCH,n[i.POINTER_PEN]=e.pointerType==e.MSPOINTER_TYPE_PEN||e.pointerType==i.POINTER_PEN,n[t]},getEvents:function(){return["pointerdown MSPointerDown","pointermove MSPointerMove","pointerup pointercancel MSPointerUp MSPointerCancel"]},reset:function(){this.pointers={}}},i.utils={extend:function(t,n,i){for(var r in n)t[r]!==e&&i||(t[r]=n[r]);return t},hasParent:function(t,e){for(;t;){if(t==e)return!0;t=t.parentNode}return!1},getCenter:function(t){for(var e=[],n=[],i=0,r=t.length;r>i;i++)e.push(t[i].pageX),n.push(t[i].pageY);return{pageX:(Math.min.apply(Math,e)+Math.max.apply(Math,e))/2,pageY:(Math.min.apply(Math,n)+Math.max.apply(Math,n))/2}},getVelocity:function(t,e,n){return{x:Math.abs(e/t)||0,y:Math.abs(n/t)||0}},getAngle:function(t,e){var n=e.pageY-t.pageY,i=e.pageX-t.pageX;return 180*Math.atan2(n,i)/Math.PI},getDirection:function(t,e){var n=Math.abs(t.pageX-e.pageX),r=Math.abs(t.pageY-e.pageY);return n>=r?t.pageX-e.pageX>0?i.DIRECTION_LEFT:i.DIRECTION_RIGHT:t.pageY-e.pageY>0?i.DIRECTION_UP:i.DIRECTION_DOWN},getDistance:function(t,e){var n=e.pageX-t.pageX,i=e.pageY-t.pageY;return Math.sqrt(n*n+i*i)},getScale:function(t,e){return t.length>=2&&e.length>=2?this.getDistance(e[0],e[1])/this.getDistance(t[0],t[1]):1},getRotation:function(t,e){return t.length>=2&&e.length>=2?this.getAngle(e[1],e[0])-this.getAngle(t[1],t[0]):0},isVertical:function(t){return t==i.DIRECTION_UP||t==i.DIRECTION_DOWN},stopDefaultBrowserBehavior:function(t,e){var n,i=["webkit","khtml","moz","Moz","ms","o",""];if(e&&t.style){for(var r=0;i.length>r;r++)for(var o in e)e.hasOwnProperty(o)&&(n=o,i[r]&&(n=i[r]+n.substring(0,1).toUpperCase()+n.substring(1)),t.style[n]=e[o]);"none"==e.userSelect&&(t.onselectstart=function(){return!1})}}},i.detection={gestures:[],current:null,previous:null,stopped:!1,startDetect:function(t,e){this.current||(this.stopped=!1,this.current={inst:t,startEvent:i.utils.extend({},e),lastEvent:!1,name:""},this.detect(e))},detect:function(t){if(this.current&&!this.stopped){t=this.extendEventData(t);for(var e=this.current.inst.options,n=0,r=this.gestures.length;r>n;n++){var o=this.gestures[n];if(!this.stopped&&e[o.name]!==!1&&o.handler.call(o,t,this.current.inst)===!1){this.stopDetect();break}}return this.current&&(this.current.lastEvent=t),t.eventType==i.EVENT_END&&!t.touches.length-1&&this.stopDetect(),t}},stopDetect:function(){this.previous=i.utils.extend({},this.current),this.current=null,this.stopped=!0},extendEventData:function(t){var e=this.current.startEvent;if(e&&(t.touches.length!=e.touches.length||t.touches===e.touches)){e.touches=[];for(var n=0,r=t.touches.length;r>n;n++)e.touches.push(i.utils.extend({},t.touches[n]))}var o=t.timeStamp-e.timeStamp,s=t.center.pageX-e.center.pageX,a=t.center.pageY-e.center.pageY,c=i.utils.getVelocity(o,s,a);return i.utils.extend(t,{deltaTime:o,deltaX:s,deltaY:a,velocityX:c.x,velocityY:c.y,distance:i.utils.getDistance(e.center,t.center),angle:i.utils.getAngle(e.center,t.center),direction:i.utils.getDirection(e.center,t.center),scale:i.utils.getScale(e.touches,t.touches),rotation:i.utils.getRotation(e.touches,t.touches),startEvent:e}),t},register:function(t){var n=t.defaults||{};return n[t.name]===e&&(n[t.name]=!0),i.utils.extend(i.defaults,n,!0),t.index=t.index||1e3,this.gestures.push(t),this.gestures.sort(function(t,e){return t.index<e.index?-1:t.index>e.index?1:0}),this.gestures}},i.gestures=i.gestures||{},i.gestures.Hold={name:"hold",index:10,defaults:{hold_timeout:500,hold_threshold:1},timer:null,handler:function(t,e){switch(t.eventType){case i.EVENT_START:clearTimeout(this.timer),i.detection.current.name=this.name,this.timer=setTimeout(function(){"hold"==i.detection.current.name&&e.trigger("hold",t)},e.options.hold_timeout);break;case i.EVENT_MOVE:t.distance>e.options.hold_threshold&&clearTimeout(this.timer);break;case i.EVENT_END:clearTimeout(this.timer)}}},i.gestures.Tap={name:"tap",index:100,defaults:{tap_max_touchtime:250,tap_max_distance:10,tap_always:!0,doubletap_distance:20,doubletap_interval:300},handler:function(t,e){if(t.eventType==i.EVENT_END){var n=i.detection.previous,r=!1;if(t.deltaTime>e.options.tap_max_touchtime||t.distance>e.options.tap_max_distance)return;n&&"tap"==n.name&&t.timeStamp-n.lastEvent.timeStamp<e.options.doubletap_interval&&t.distance<e.options.doubletap_distance&&(e.trigger("doubletap",t),r=!0),(!r||e.options.tap_always)&&(i.detection.current.name="tap",e.trigger(i.detection.current.name,t))}}},i.gestures.Swipe={name:"swipe",index:40,defaults:{swipe_max_touches:1,swipe_velocity:.7},handler:function(t,e){if(t.eventType==i.EVENT_END){if(e.options.swipe_max_touches>0&&t.touches.length>e.options.swipe_max_touches)return;(t.velocityX>e.options.swipe_velocity||t.velocityY>e.options.swipe_velocity)&&(e.trigger(this.name,t),e.trigger(this.name+t.direction,t))}}},i.gestures.Drag={name:"drag",index:50,defaults:{drag_min_distance:10,correct_for_drag_min_distance:!0,drag_max_touches:1,drag_block_horizontal:!1,drag_block_vertical:!1,drag_lock_to_axis:!1,drag_lock_min_distance:25},triggered:!1,handler:function(t,n){if(i.detection.current.name!=this.name&&this.triggered)return n.trigger(this.name+"end",t),this.triggered=!1,e;if(!(n.options.drag_max_touches>0&&t.touches.length>n.options.drag_max_touches))switch(t.eventType){case i.EVENT_START:this.triggered=!1;break;case i.EVENT_MOVE:if(t.distance<n.options.drag_min_distance&&i.detection.current.name!=this.name)return;if(i.detection.current.name!=this.name&&(i.detection.current.name=this.name,n.options.correct_for_drag_min_distance)){var r=Math.abs(n.options.drag_min_distance/t.distance);i.detection.current.startEvent.center.pageX+=t.deltaX*r,i.detection.current.startEvent.center.pageY+=t.deltaY*r,t=i.detection.extendEventData(t)}(i.detection.current.lastEvent.drag_locked_to_axis||n.options.drag_lock_to_axis&&n.options.drag_lock_min_distance<=t.distance)&&(t.drag_locked_to_axis=!0);var o=i.detection.current.lastEvent.direction;t.drag_locked_to_axis&&o!==t.direction&&(t.direction=i.utils.isVertical(o)?0>t.deltaY?i.DIRECTION_UP:i.DIRECTION_DOWN:0>t.deltaX?i.DIRECTION_LEFT:i.DIRECTION_RIGHT),this.triggered||(n.trigger(this.name+"start",t),this.triggered=!0),n.trigger(this.name,t),n.trigger(this.name+t.direction,t),(n.options.drag_block_vertical&&i.utils.isVertical(t.direction)||n.options.drag_block_horizontal&&!i.utils.isVertical(t.direction))&&t.preventDefault();break;case i.EVENT_END:this.triggered&&n.trigger(this.name+"end",t),this.triggered=!1}}},i.gestures.Transform={name:"transform",index:45,defaults:{transform_min_scale:.01,transform_min_rotation:1,transform_always_block:!1},triggered:!1,handler:function(t,n){if(i.detection.current.name!=this.name&&this.triggered)return n.trigger(this.name+"end",t),this.triggered=!1,e;if(!(2>t.touches.length))switch(n.options.transform_always_block&&t.preventDefault(),t.eventType){case i.EVENT_START:this.triggered=!1;break;case i.EVENT_MOVE:var r=Math.abs(1-t.scale),o=Math.abs(t.rotation);if(n.options.transform_min_scale>r&&n.options.transform_min_rotation>o)return;i.detection.current.name=this.name,this.triggered||(n.trigger(this.name+"start",t),this.triggered=!0),n.trigger(this.name,t),o>n.options.transform_min_rotation&&n.trigger("rotate",t),r>n.options.transform_min_scale&&(n.trigger("pinch",t),n.trigger("pinch"+(1>t.scale?"in":"out"),t));break;case i.EVENT_END:this.triggered&&n.trigger(this.name+"end",t),this.triggered=!1}}},i.gestures.Touch={name:"touch",index:-1/0,defaults:{prevent_default:!1,prevent_mouseevents:!1},handler:function(t,n){return n.options.prevent_mouseevents&&t.pointerType==i.POINTER_MOUSE?(t.stopDetect(),e):(n.options.prevent_default&&t.preventDefault(),t.eventType==i.EVENT_START&&n.trigger(this.name,t),e)}},i.gestures.Release={name:"release",index:1/0,handler:function(t,e){t.eventType==i.EVENT_END&&e.trigger(this.name,t)}},"function"==typeof define&&"object"==typeof define.amd&&define.amd?define(function(){return i}):"object"==typeof module&&"object"==typeof module.exports?module.exports=i:t.Hammer=i})(this);;
(function($, undefined) {
    'use strict';

    // no jQuery or Zepto!
    if($ === undefined) {
        return;
    }

    /**
     * bind dom events
     * this overwrites addEventListener
     * @param   {HTMLElement}   element
     * @param   {String}        eventTypes
     * @param   {Function}      handler
     */
    Hammer.event.bindDom = function(element, eventTypes, handler) {
        $(element).on(eventTypes, function(ev) {
            var data = ev.originalEvent || ev;

            // IE pageX fix
            if(data.pageX === undefined) {
                data.pageX = ev.pageX;
                data.pageY = ev.pageY;
            }

            // IE target fix
            if(!data.target) {
                data.target = ev.target;
            }

            // IE button fix
            if(data.which === undefined) {
                data.which = data.button;
            }

            // IE preventDefault
            if(!data.preventDefault) {
                data.preventDefault = ev.preventDefault;
            }

            // IE stopPropagation
            if(!data.stopPropagation) {
                data.stopPropagation = ev.stopPropagation;
            }

            handler.call(this, data);
        });
    };

    /**
     * the methods are called by the instance, but with the jquery plugin
     * we use the jquery event methods instead.
     * @this    {Hammer.Instance}
     * @return  {jQuery}
     */
    Hammer.Instance.prototype.on = function(types, handler) {
        return $(this.element).on(types, handler);
    };
    Hammer.Instance.prototype.off = function(types, handler) {
        return $(this.element).off(types, handler);
    };


    /**
     * trigger events
     * this is called by the gestures to trigger an event like 'tap'
     * @this    {Hammer.Instance}
     * @param   {String}    gesture
     * @param   {Object}    eventData
     * @return  {jQuery}
     */
    Hammer.Instance.prototype.trigger = function(gesture, eventData){
        var el = $(this.element);
        if(el.has(eventData.target).length) {
            el = $(eventData.target);
        }

        return el.trigger({
            type: gesture,
            gesture: eventData
        });
    };


    /**
     * jQuery plugin
     * create instance of Hammer and watch for gestures,
     * and when called again you can change the options
     * @param   {Object}    [options={}]
     * @return  {jQuery}
     */
    $.fn.hammer = function(options) {
        return this.each(function() {
            var el = $(this);
            var inst = el.data('hammer');
            // start new hammer instance
            if(!inst) {
                el.data('hammer', new Hammer(this, options || {}));
            }
            // change the options
            else if(inst && options) {
                Hammer.utils.extend(inst.options, options);
            }
        });
    };

})(window.jQuery || window.Zepto);;
/**
 * Creates a new slider model.
 *
 * @constructor
 *
 * @param {Slider~settings} settings
 *  An object of settings with which the new slider should be configured.
 */
function Slider(settings) {
  var model = this;

  /**
   * The identity function. Returns its argument.
   *
   * @private
   *
   * @param {*} x
   *  The value to return.
   *
   * @return
   *  The supplied value.
   */
  var identity = function(x) {
    return x;
  };

  /**
   * Combines the properties of a set of objects into a single object,
   * destructively overwriting the first argument.
   *
   * @param {object} o
   *  The object to extend with the properties of successive arguments.
   *
   * @param {...object} os
   *  A variable number of objects with which to extend the first argument.
   *
   * @return {object}
   *  The initial argument, extended with the fields of any additional
   *  arguments.
   */
  var extend = function() {
    var result = arguments[0];
    for (var i = 1; i < arguments.length; ++i) {
      var source = arguments[i];
      for (var property in source) {
        result[property] = source[property];
      }
    }

    return result;
  };

  /**
   * Ensures that a given set of keys correspond to settings with functional
   * values. If a setting is found to map to a string, the string is treated as
   * a function name and evaluated in order to produce the requisite function.
   *
   * @param {string[]} keys
   *  A list of settings which should have functional values.
   */
  var ensureFunctionSettings = function(keys) {
    for (var i = 0; i < keys.length; ++i) {
      var key = keys[i];
      if (typeof settings[key] === 'string') {
        settings[key] = eval(settings[key]);
      }
    }
  }

  /**
   * Ensures that a given set of keys correspond to settings with numeric
   * values.
   *
   * @param {string[]} keys
   *  A list of settings which should have numeric values.
   */
  var ensureNumericSettings = function(keys) {
    for (var i = 0; i < keys.length; ++i) {
      var key = keys[i];
      if (typeof settings[key] === 'string') {
        settings[key] = parseFloat(settings[key]);
      }
    }
  };

  /**
   * Clamps a given input to the range specified by the supplied minimum and
   * maximum values.
   *
   * @private
   *
   * @param {number} x
   *  The value to clamp.
   *
   * @param {number} minimum
   *  The minimum acceptable value.
   *
   * @param {number} maximum
   *  The maximum acceptable value.
   *
   * @return {number}
   *  A clamped value.
   */
  var clamp = function(x, minimum, maximum) {
    return Math.min(Math.max(x, minimum), maximum);
  };

  /**
   * An object of default settings with which new slider models may be
   * configured.
   *
   * @private
   * @type {Slider~settings}
   */
  var defaults = {
    defaultOffset: 1,
    maximum: 100,
    minimum: 1,
    observers: [],
    offsetToValue: identity,
    onSetOffset: function(model) { },
    valueToOffset: identity
  };

  /**
   * Initialises a new slider model using the provided settings.
   *
   * @private
   */
  var __construct = function() {
    ensureFunctionSettings([
      'offsetToValue',
      'onSetOffset',
      'valueToOffset'
    ]);

    ensureNumericSettings([
      'defaultOffset',
      'maximum',
      'minimum'
    ]);

    extend(model, defaults, settings);

    // Cache the range so that we don't have to repeatedly compute it.
    updateRange();
    model.setOffset(model.defaultOffset);
  };

  /**
   * Updates the cached copy of the slider's range.
   *
   * @private
   */
  var updateRange = function() {
    model.range = model.maximum - model.minimum;
  };

  /**
   * Adds an observer to the slider which will be notified whenever the
   * slider's offset is changed.
   *
   * @param {object} observer
   *  The object to be notified (through its update() method) when the slider's
   *  offset is changed.
   */
  model.addObserver = function(observer) {
    model.observers.push(observer);
  };

  /**
   * Returns the slider's current maximum offset.
   *
   * @method
   *
   * @return {number}
   *  The slider's current maximum offset.
   */
  model.getMaximum = function() {
    return model.maximum;
  };

  /**
   * Sets the slider's maximum offset.
   *
   * @method
   *
   * @param {number} maximum
   *  The new maximum offset. The slider's current offset will be clamped to
   *  the new range.
   *
   * @return {Slider}
   *  The slider model, suitable for method chaining.
   */
  model.setMaximum = function(maximum) {
    model.maximum = parseFloat(maximum);
    updateRange();

    return model.setOffset(model.offset);
  };

  /**
   * Returns the slider's current minimum offset.
   *
   * @method
   *
   * @return {number}
   *  The slider's current minimum offset.
   */
  model.getMinimum = function() {
    return model.minimum;
  };

  /**
   * Sets the slider's minimum offset.
   *
   * @method
   *
   * @param {number} minimum
   *  The new minimum offset. The slider's current offset will be clamped to
   *  the new range.
   *
   * @return {Slider}
   *  The slider model, suitable for method chaining.
   */
  model.setMinimum = function(minimum) {
    model.minimum = parseFloat(minimum);
    updateRange();

    return model.setOffset(model.offset);
  };

  /**
   * Returns the slider's current offset.
   *
   * @method
   *
   * @return {number}
   *  The slider's current offset.
   */
  model.getOffset = function() {
    return model.offset;
  };

  /**
   * Sets the slider's current offset.
   *
   * @method
   *
   * @param {number} offset
   *  The offset to set. This will be clamped to the slider's minimum and
   *  maximum values.
   *
   * @return {Slider}
   *  The slider model, suitable for method chaining.
   */
  model.setOffset = function(offset) {
    model.offset = clamp(Math.round(offset), model.minimum, model.maximum);

    notifyObservers();
    model.onSetOffset(model);

    return model;
  };

  /**
   * Notifies the slider's observers of a change in the slider's state.
   */
  var notifyObservers = function() {
    for (var i = 0; i < model.observers.length; ++i) {
      model.observers[i].update();
    }
  };

  /**
   * Returns the slider's range.
   *
   * @method
   *
   * @return {number}
   *  The slider's range.
   */
  model.getRange = function() {
    return model.range;
  };

  /**
   * Returns the slider's current value, as computed from the current offset.
   *
   * @method
   *
   * @return {?}
   *  The slider's current value, as computed from the current offset.
   */
  model.getValue = function() {
    return model.offsetToValue(model.offset);
  };

  /**
   * Sets the slider's current offset as computed from the supplied value.
   *
   * @method
   *
   * @param {?} value
   *  The value from which a new offset should be computed. Note that the
   *  resulting offset will be clamped to the slider's minimum and maximum
   *  values.
   *
   * @return {Slider}
   *  The slider model, suitable for method chaining.
   */
  model.setValue = function(value) {
    return model.setOffset(model.valueToOffset(value));
  };

  /**
   * Creates a new view of the enclosing slider model.
   *
   * @constructor
   *
   * @param {Slider~View~settings} settings
   *  An object of settings with which the new slider view should be configured.
   */
  model.View = function(settings) {
    var view = this;

    /**
     * An object of default settings with which new slider views may be
     * configured.
     *
     * @private
     * @type {Slider~View~settings}
     */
    var defaults = {
      buttonType: 'a',
      decrementButtonClassName: 'wonga-slider-button wonga-slider-decrement',
      decrementButtonInnerHTML: '-',
      handleClassName: 'wonga-slider-handle',
      handleType: 'a',
      handleId: settings.inputId + '-wonga-slider-handle',
      incrementButtonClassName: 'wonga-slider-button wonga-slider-increment',
      incrementButtonInnerHTML: '+',
      onAttach: function(view) { },
      rangeClassName: 'wonga-slider-range',
      rangeType: 'div',
      rangeId: settings.inputId + '-wonga-slider-range',
      replaceParent: false,
      snapFrom: function(offset) {
        return view.step * Math.round(offset / view.step);
      },

      step: 5,
      template: '<div class="wonga-slider-wrapper">{{D}}{{T}}{{I}}</div>',
      trackClassName: 'wonga-slider-track',
      trackId: settings.inputId + '-wonga-slider-track',
      trackType: 'div',
      valueToString: identity,
      stringToValue: identity
    };

    /**
     * Initialises a new slider view using the provided settings.
     *
     * @private
     */
    var __construct = function() {
      ensureFunctionSettings([
        'attach',
        'onAttach'
      ]);

      ensureNumericSettings([
        'step'
      ]);

      extend(view, defaults, settings);
      attach();

      model.addObserver(view);
      view.update();
    };


    /**
     * Returns the offset from the left of the window of the element passed in.
     *
     * @private
     *
     * @param {node} A dom element
     */
    var leftOffset = function(node) {
      if (typeof node.getBoundingClientRect !== 'undefined') {
        var pageLeft = window.pageXOffset || document.body.scrollLeft || 0,
            clientLeft = document.documentElement.clientLeft;

        left = node.getBoundingClientRect().left + pageLeft - clientLeft;
      }
      else {
        left = 0;
        while (node = node.offsetParent) {
          left += node.offsetLeft;
        }
      }

      return left;
    };

    /**
     * Creates the HTML elements necessary for the slider's view and attaches
     * them to the HTML element which is to be bound to the slider model.
     *
     * @private
     */
    var attach = function() {
      // Cache the created elements on the view object so that we don't have to
      // repeatedly find them in the document.
      view.track = createTrack();
      view.range = createRange();
      view.handle = createHandle();
      view.decrementButton = createButton(view.decrementButtonClassName,
        view.decrementButtonInnerHTML, decrement);

      view.incrementButton = createButton(view.incrementButtonClassName,
        view.incrementButtonInnerHTML, increment);

      view.input = document.getElementById(view.inputId);
      view.input.onchange = change;

      view.inputLabel = findLabel(view.input);
      view.inputParent = view.parentId ?
        document.getElementById(view.parentId) : view.input.parentNode;

      view.track.appendChild(view.range);
      view.track.appendChild(view.handle);

      var elements = {
            D: view.decrementButton,
            T: view.track,
            I: view.incrementButton,
            E: view.input,
            L: view.inputLabel
          };

      // Now we have an object containing the elements to be attached, we have
      // two choices. If an attach function has been provided, use that.
      // Otherwise, fall back to instantiating a template, for which a default
      // is always available.
      if (typeof view.attach !== 'undefined') {
        view.attach(view.inputParent, elements);
      }
      else {
        var nodes = instantiateTemplate(elements);

        if (view.replaceParent) {
          var replacingParent = view.inputParent.parentNode;
          for (var i = 0, n = nodes.length; i < n; ++i) {
            replacingParent.insertBefore(nodes[i], view.inputParent);
          }

          replacingParent.removeChild(view.inputParent);
        }
        else {
          for (var i = nodes.length - 1; i >= 0; --i) {
            view.inputParent.insertBefore(
              nodes[i], view.inputParent.children[1]);
          }
        }
      }

      // Fire an event once slider attachment is complete.
      view.onAttach(view);
    };

    /**
     * Locates the corresponding label element associated with a given input.
     *
     * @param {HTMLElement} input
     *  The input element for which a label element should be retrieved.
     *
     * @return {?HTMLElement}
     *  The label associated with the given input element, or null if no such
     *  label could be found.
     */
    var findLabel = (function() {
      // The findLabel function is built conditionally -- if the HTML5 'labels'
      // property is available, we use that to gain quick access to the input's
      // labels. Otherwise, we fall back to a one-off construction of a lookup
      // table which maps identifiers to their labels and use that.
      if (typeof document.createElement('input').labels !== 'undefined') {
        return function(input) {
          return input.labels.length > 0 ? input.labels[0] : null;
        };
      }
      else {
        var labels = document.getElementsByTagName('LABEL'),
            labelFor = {};

        for (var i = 0; i < labels.length; ++i) {
          label = labels[i];
          if (document.getElementById(label.getAttribute("for"))) {
            labelFor[label.getAttribute("for")] = label;
          }
        }

        return function(input) {
          return typeof labelFor[input.id] !== 'undefined' ?
            labelFor[input.id] : null;
        };
      }
    })();

    /**
     * Instantiates the view's template with a given set of elements, returning
     * the resulting DOM element.
     *
     * @param {object.<string, HTMLElement>} elements
     *  A mapping from template specifiers (such as 'D' [decrement button], 'T'
     *  [track]) to HTML elements which should be inserted when those
     *  specifiers are encountered.
     *
     * @return {HTMLElement}
     *  An HTML element representing the instantiated template.
     */
    var instantiateTemplate = function(elements) {
      var classFor = function(type) {
            return '__wonga_sliders_' + type;
          },

          replacer = function(match, type, offset, string) {
            return '<div class="' + classFor(type) + '"></div>';
          },

          templateHTML = view.template.replace(/{{([^}]+)}}/g, replacer),
          templateElement = document.createElement('div');

      // Trick the browser into compiling the HTML string into a DOM element by
      // setting it as the innerHTML of another, freshly created, element.
      templateElement.innerHTML = templateHTML;
      for (var type in elements) {
        var element = elements[type],
            targetClass = classFor(type);

            targets = typeof document.getElementsByClassName !== 'undefined' ?
              templateElement.getElementsByClassName(targetClass) :
              templateElement.querySelectorAll('.' + targetClass);

        if (targets.length > 0) {
          var target = targets[0];
          target.parentNode.replaceChild(element, target);
        }
      }

      // We clone the element's list of child nodes so that it won't change
      // due to DOM updates.
      var nodeList = templateElement.childNodes,
          nodeArray = [];

      for (var i = 0, n = nodeArray.length = nodeList.length; i < n; ++i) {
        nodeArray[i] = nodeList[i];
      }

      return nodeArray;
    };

    /**
     * Handles slider touch and drag events. This callback is used to handle
     * interactions with both the slider track and handle.
     *
     * @param {Hammer~Event} e
     *  The touch or drag event that was fired.
     */
    var onTouchDrag = function(e) {
      var touches = e.gesture.touches;

      // In IE8, the touches object may not always be defined, so we check its
      // existence before proceeding.
      if (touches) {
        var left = touches[0].pageX - leftOffset(view.track),
            scaleFactor = view.track.offsetWidth / model.getRange(),

            // In order to ensure that sliders whose minimum offset isn't zero
            // appear correctly, we must modify the offset used by the view to
            // compute the handle's position by the minimum possible value. Here,
            // we add the minimum value to account for the fact that the leftmost
            // position on the track might not represent zero. When we update the
            // view, the corresponding subtraction will be made to ensure that
            // the handle's offset is indeed zero.
            offset = (left / scaleFactor) + model.minimum;

        model.setOffset(view.snapFrom(offset, Slider.View.SnapType.DRAG));

        e.gesture.preventDefault();
      }
    };

    /**
     * Creates a new slider track.
     *
     * @private
     *
     * @return {HTMLElement}
     *  The created track.
     */
    var createTrack = function() {
      var track = document.createElement(view.trackType);

      track.className = view.trackClassName;
      track.id = view.trackId;

      // Set up the track to respond appropriately to touch events.
      Hammer(track).on('touch drag', onTouchDrag);

      return track;
    };

    /**
     * Creates a new slider range.
     *
     * @private
     *
     * @return {HTMLElement}
     *  The created range.
     */
    var createRange = function() {
      var range = document.createElement(view.rangeType);

      range.className = view.rangeClassName;
      range.id = view.rangeId;

      return range;
    };

    /**
     * Creates a new slider handle.
     *
     * @private
     *
     * @return {HTMLElement}
     *  The created handle.
     */
    var createHandle = function() {
      var handle = document.createElement(view.handleType);

      handle.className = view.handleClassName;
      handle.id = view.handleId;
      handle.style.position = 'absolute';

      // Set up the handle to respond appropriately to touch events.
      Hammer(handle).on('touch drag', onTouchDrag);

      return handle;
    };

    /**
     * Creates a new slider button with the given HTML class attribute, content
     * and click handler.
     *
     * @private
     *
     * @param {string} className
     *  The value of the HTML class attribute that should be assigned to the
     *  new button.
     *
     * @param {string} innerHTML
     *  The content that should be placed inside the new button.
     *
     * @param {function(e)} onClick
     *  The event handler callback to be fired when the new button is clicked.
     *
     * @return {HTMLElement}
     *  The created button.
     */
    var createButton = function(className, innerHTML, onClick) {
      var button = document.createElement(view.buttonType);

      button.className = className;
      button.innerHTML = innerHTML;
      Hammer(button).on('tap', onClick);

      return button;
    };

    /**
     * Change handler for the slider's input element.
     *
     * @private
     */
    var change = function() {
      model.setValue(view.stringToValue(view.input.value));
    };

    /**
     * Click handler for the slider's decrement button.
     *
     * @private
     */
    var decrement = function() {
      model.setOffset(view.snapFrom(
        model.offset - view.step, Slider.View.SnapType.DECREMENT));

      return false;
    };

    /**
     * Click handler for the slider's increment button.
     *
     * @private
     */
    var increment = function() {
      model.setOffset(view.snapFrom(
        model.offset + view.step, Slider.View.SnapType.INCREMENT));

      return false;
    };

    /**
     * Updates the slider's view.
     *
     * @private
     */
    view.update = function() {
      var offset = model.offset - model.minimum,
          scaleFactor = view.track.offsetWidth / model.getRange(),
          halfHandleWidth = view.handle.offsetWidth / 2,
          trackWidth = Math.floor(offset * scaleFactor),
          left = Math.round((view.track.offsetLeft + trackWidth) - halfHandleWidth);

      view.range.style.width = trackWidth + 'px';
      view.handle.style.left = left + 'px';
      view.input.value = view.valueToString(model.getValue());
    };

    __construct();
  };

  __construct();
}

Slider.View = Slider.View || {};

/**
 * The type of events which may cause a snap in a slider view.
 *
 * @readonly
 * @enum {number}
 */
Slider.View.SnapType = {
  /**
   * A drag event.
   */
  DRAG: 0,

  /**
   * A decrement event, triggered by clicking the relevant button.
   */
  DECREMENT: 1,

  /**
   * An increment event, triggered by clicking the relevant button.
   */
  INCREMENT: 2
};

/**
 * The type of settings objects that may be used to construct slider models.
 *
 * @typedef object Slider~settings
 *
 * @property {number} defaultOffset
 *  The slider's default (initial) offset.
 *
 * @property {number} maximum
 *  The slider's maximum offset.
 *
 * @property {number} minimum
 *  The slider's minimum offset
 *
 * @property {Slider~eventHandler} onSetOffset
 *  A callback to be invoked in response to changes in the slider's offset.
 *
 * @property {Slider~offsetValueConverter} offsetToValue
 *  The callback to be used for converting offsets to values. This should be a
 *  functional inverse of the callback supplied as the valueToOffset property.
 *
 * @property {Slider~offsetValueConverter} valueToOffset
 *  The callback to be used for converting values to offsets. This should be a
 *  functional inverse of the callback supplied as the offsetToValue property.
 */

/**
 * The type of slider model event handlers.
 *
 * @callback Slider~eventHandler
 *
 * @param {Slider} model
 *  The slider model which fired the event.
 */

/**
 * The type of callbacks used by slider models to convert between offsets and
 * values.
 *
 * @callback Slider~offsetValueConverter
 *
 * @param {number|?} offsetOrValue
 *  The offset of value to be converted.
 *
 * @return {number|?}
 *  Either the value corresponding to the supplied offset or the offset
 *  corresponding to the supplier value.
 */

/**
 * The type of settings objects that may be used to construct slider views.
 *
 * @typedef object Slider~View~settings
 *
 * @property {Slider~View~attacher} attach
 *  The callback to be used to attach a set of slider elements to the DOM.
 *  Takes precedence over the replaceParent/template properties, which
 *  simplify this operation in most cases.
 *
 * @property {string} buttonType
 *  The type of HTML element that should be created for slider buttons.
 *
 * @property {string} decrementButtonClassName
 *  The value of the HTML class attribute that should be assigned to the
 *  decrement button.
 *
 * @property {string} decrementButtonInnerHTML
 *  The HTML that be placed inside the decrement button.
 *
 * @property {string} handleClassName
 *  The value of the HTML class attribute that should be assigned to the
 *  slider handle.
 *
 * @property {string} handleId
 *  The HTML identifier that should be assigned to the slider handle.
 *
 * @property {string} handleType
 *  The type of HTML element that should be created for the slider handle.
 *
 * @property {string} incrementButtonClassName
 *  The value of the HTML class attribute that should be assigned to the
 *  increment button.
 *
 * @property {string} incrementButtonInnerHTML
 *  The HTML that be placed inside the increment button.
 *
 * @property {Slider~View~eventHandler} onAttach
 *  A callback to be invoked in response to the attachment of the slider view
 *  to an input element.
 *
 * @property {string} rangeClassName
 *  The value of the HTML class attribute that should be assigned to the
 *  slider range.
 *
 * @property {string} rangeId
 *  The HTML identifier that should be assigned to the slider range.
 *
 * @property {string} rangeType
 *  The type of HTML element that should be created for the slider range.
 *
 * @property {boolean} replaceParent
 *  Only to be used in conjunction with the template setting. True if and only
 *  if the parent element (as specified by parentId, for instance) should be
 *  replaced with the result of template instantiation (rather than
 *  supplemented).
 *
 * @property {Slider~View~snapConverter} snapFrom
 *  The callback to be used for snapping to a desired offset given an arbitrary
 *  position in the slider view.
 *
 * @property {number} step
 *  The value by which the increment and decrement buttons should increment and
 *  decrement the underlying model's offset respectively.
 *
 * @property {string} template
 *  A template string which should be used to produce slider markup. May
 *  contain the following variables:
 *
 *  * '{{D}}': The decrement button.
 *  * '{{I}}': The increment button.
 *  * '{{T}}': The slider track.
 *  * '{{E}}': The input element the slider is bound to.
 *  * '{{L}}': The label of the input element the slider is bound to, if
 *    available.
 *
 * @property {string} trackClassName
 *  The value of the HTML class attribute that should be assigned to the
 *  slider track.
 *
 * @property {string} trackId
 *  The HTML identifier that should be assigned to the slider track.
 *
 * @property {string} trackType
 *  The type of HTML element that should be created for the slider track.
 *
 * @property {Slider~View~valueStringifier} valueToString
 *  The callback to be used for converting slider values into strings suitable
 *  for insertion into the linked HTML element.
 */

/**
 * The type of slider view DOM attachment callbacks.
 *
 * @callback Slider~View~attacher
 *
 * @param {HTMLElement} inputParent
 *  The parent of the input element to which a slider view is being attached.
 *
 * @param {object.<string, HTMLElement} elements
 *  An object containing the following slider elements:
 *
 *  * D: The decrement button.
 *  * I: The increment button.
 *  * T: The slider track.
 *  * E: The input element the slider is bound to.
 *  * L: The label of the input element the slider is bound to, if available.
 */

/**
 * The type of slider view event handlers.
 *
 * @callback Slider~View~eventHandler
 *
 * @param {Slider~View} view
 *  The view which fired the event.
 */

/**
 * The type of callbacks used by slider views to snap to a desired offset given
 * an arbitrary position in the slider view.
 *
 * @callback Slider~View~snapConverter
 *
 * @param {number} offset
 *  The offset from which a new, snapped, offset should be computed.
 *
 * @param {Slider~View~SnapType} snapType
 *  The type of event that has caused the snap.
 *
 * @return {number}
 *  The snapped offset.
 */

/**
 * The type of callbacks used by slider views to convert slider values into
 * strings suitable for insertion into their linked HTML element.
 *
 * @callback Slider~View~valueStringifier
 *
 * @param {?} value
 *  The value to stringify.
 *
 * @return {string}
 *  The stringified value.
 */
;
/**
 * Autotab - jQuery plugin 1.1b
 * http://www.lousyllama.com/sandbox/jquery-autotab
 * 
 * Copyright (c) 2008 Matthew Miller
 * 
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 * 
 * Revised: 2008-09-10 16:55:08
 */

(function($) {
// Look for an element based on ID or name
var check_element = function(name) {
	var obj = null;
	var check_id = $('#' + name);
	var check_name = $('input[name=' + name + ']');

	if(check_id.length)
		obj = check_id;
	else if(check_name != undefined)
		obj = check_name;

	return obj;
};

/**
 * autotab_magic automatically establishes autotabbing with the
 * next and previous elements as provided by :input.
 * 
 * autotab_magic should called after applying filters, if used.
 * If any filters are applied after calling autotab_magic, then
 * Autotab may not protect against brute force typing.
 * 
 * @name	autotab_magic
 * @param	focus	Applies focus on the specified element
 * @example	$(':input').autotab_magic();
 */
$.fn.autotab_magic = function(focus) {
	for(var i = 0; i < this.length; i++)
	{
		var n = i + 1;
		var p = i - 1;

		if(i > 0 && n < this.length)
			$(this[i]).autotab({ target: $(this[n]), previous: $(this[p]) });
		else if(i > 0)
			$(this[i]).autotab({ previous: $(this[p]) });
		else
			$(this[i]).autotab({ target: $(this[n]) });

		// Set the focus on the specified element
		if(focus != null && (isNaN(focus) && focus == $(this[i]).attr('id')) || (!isNaN(focus) && focus == i))
			$(this[i]).focus();
	}
	return this;
};

/**
 * This will take any of the text that is typed and
 * format it according to the options specified.
 * 
 * Option values:
 *	format		text|number|alphanumeric|all|custom
 *	- Text			Allows all characters except numbers
 *	- Number		Allows only numbers
 *	- Alphanumeric	Allows only letters and numbers
 *	- All			Allows any and all characters
 *	- Custom		Allows developer to provide their own filter
 *
 *	uppercase	true|false
 *	- Converts a string to UPPERCASE
 * 
 *	lowercase	true|false
 *	- Converts a string to lowecase
 * 
 *	nospace		true|false
 *	- Remove spaces in the user input
 * 
 *	pattern		null|(regular expression)
 *	- Custom regular expression for the filter
 * 
 * @name	autotab_filter
 * @param	options		Can be a string, function or a list of options. If a string or
 *						function is passed, it will be assumed to be a format option.
 * @example	$('#number1, #number2, #number3').autotab_filter('number');
 * @example	$('#product_key').autotab_filter({ format: 'alphanumeric', nospace: true });
 * @example	$('#unique_id').autotab_filter({ format: 'custom', pattern: '[^0-9\.]' });
 */
$.fn.autotab_filter = function(options) {
	var defaults = {
		format: 'all',
		uppercase: false,
		lowercase: false,
		nospace: false,
		pattern: null
	};

	if(typeof options == 'string' || typeof options == 'function')
		defaults.format = options;
	else
		$.extend(defaults, options);

	for(var i = 0; i < this.length; i++)
	{
		$(this[i]).bind('keyup', function(e) {
			var val = this.value;

			switch(defaults.format)
			{
				case 'text':
					var pattern = new RegExp('[0-9]+', 'g');
					val = val.replace(pattern, '');
					break;

				case 'alpha':
					var pattern = new RegExp('[^a-zA-Z]+', 'g');
					val = val.replace(pattern, '');
					break;

				case 'number':
				case 'numeric':
					var pattern = new RegExp('[^0-9]+', 'g');
					val = val.replace(pattern, '');
					break;

				case 'alphanumeric':
					var pattern = new RegExp('[^0-9a-zA-Z]+', 'g');
					val = val.replace(pattern, '');
					break;

				case 'custom':
					var pattern = new RegExp(defaults.pattern, 'g');
					val = val.replace(pattern, '');
					break;

				case 'all':
				default:
					if(typeof defaults.format == 'function')
						var val = defaults.format(val);

					break;
			}

			if(defaults.nospace)
			{
				var pattern = new RegExp('[ ]+', 'g');
				val = val.replace(pattern, '');
			}

			if(defaults.uppercase)
				val = val.toUpperCase();

			if(defaults.lowercase)
				val = val.toLowerCase();

			if(val != this.value)
				this.value = val;
		});
	}
};

/**
 * Provides the autotabbing mechanism for the supplied element and passes
 * any formatting options to autotab_filter.
 * 
 * Refer to autotab_filter's description for a detailed explanation of
 * the options available.
 * 
 * @name	autotab
 * @param	options
 * @example	$('#phone').autotab({ format: 'number' });
 * @example	$('#username').autotab({ format: 'alphanumeric', target: 'password' });
 * @example	$('#password').autotab({ previous: 'username', target: 'confirm' });
 */
$.fn.autotab = function(options) {
	var defaults = {
		format: 'all',
		maxlength: 2147483647,
		uppercase: false,
		lowercase: false,
		nospace: false,
		target: null,
		previous: null,
		pattern: null
	};

	$.extend(defaults, options);

	// Sets targets to element based on the name or ID
	// passed if they are not currently objects
	if(typeof defaults.target == 'string')
		defaults.target = check_element(defaults.target);

	if(typeof defaults.previous == 'string')
		defaults.previous = check_element(defaults.previous);

	var maxlength = $(this).attr('maxlength');

	// defaults.maxlength has not changed and maxlength was specified
	if(defaults.maxlength == 2147483647 && maxlength != 2147483647)
		defaults.maxlength = maxlength;
	// defaults.maxlength overrides maxlength
	else if(defaults.maxlength > 0)
		$(this).attr('maxlength', defaults.maxlength)
	// defaults.maxlength and maxlength have not been specified
	// A target cannot be used since there is no defined maxlength
	else
		defaults.target = null;

	if(defaults.format != 'all')
		$(this).autotab_filter(defaults);

	// Go to the previous element when backspace
	// is pressed in an empty input field
	return $(this).bind('keydown', function(e) {
		if(e.which == 8 && this.value.length == 0 && defaults.previous)
			defaults.previous.focus().val(defaults.previous.val());
	}).bind('keyup', function(e) {
		/**
		 * Do not auto tab when the following keys are pressed
		 * 8:	Backspace
		 * 9:	Tab
		 * 16:	Shift
		 * 17:	Ctrl
		 * 18:	Alt
		 * 19:	Pause Break
		 * 20:	Caps Lock
		 * 27:	Esc
		 * 33:	Page Up
		 * 34:	Page Down
		 * 35:	End
		 * 36:	Home
		 * 37:	Left Arrow
		 * 38:	Up Arrow
		 * 39:	Right Arrow
		 * 40:	Down Arrow
		 * 45:	Insert
		 * 46:	Delete
		 * 144:	Num Lock
		 * 145:	Scroll Lock
		 */
		var keys = [8, 9, 16, 17, 18, 19, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45, 46, 144, 145];

		if(e.which != 8)
		{
			var val = $(this).val();

			if($.inArray(e.which, keys) == -1 && val.length == defaults.maxlength && defaults.target)
				defaults.target.focus();
		}
	});
};

})(jQuery);;
