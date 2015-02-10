(function($, Drupal) {
  Drupal.behaviors.everlineForms = function (context) {

    var elementFocused = function(e) {
      if (e.currentTarget.title !== '' && !$(e.currentTarget).parent().hasClass('error')) {
        var tip = document.createElement('div'),
            tipContent = e.currentTarget.title;

        tip.className = 'form-tip';
        tip.innerHTML = tipContent;

        this.formTip = tip;
        e.currentTarget.parentNode.appendChild(tip);
      }
    };

    var elementBlur = function(e) {
      if ($(e.currentTarget).parent().find('.form-tip').length > 0) {
        e.currentTarget.parentNode.removeChild(this.formTip);
      }
    };

    $(context).on('focus', 'input,textarea', $.proxy(elementFocused, this));
    $(context).on('blur', 'input,textarea', $.proxy(elementBlur, this));

    var clickDelegate = function(e) {
      e.preventDefault();

      var $this = $(this),
          $next = $this.next('input');

      if ($next.attr('disabled')) {
        return false;
      }
      else {
        $next.trigger('click').trigger('change');
      }
    };

    var changeDelegate = function(e) {
      $('[name="' + this.name + '"]').each(function() {
        var $this = $(this);
        if ($this.is(':checked')) {
          $this.prev('a').addClass('jqMultiChecked');
        }
        else {
          $this.prev('a').removeClass('jqMultiChecked');
        }
      });
    };

    //process  radio options
    var processRadios = function() {
      var $e = $(this),
          klass = $e.is(':checked') ? ' class="jqMultiChecked"' : '',
          rel = ' rel="' + $e.attr('name') + '"';

      // if this has already been made then unmake it first, since the values may
      // have changed. this will kill off the objects that listeners have been
      // attached to
      if ($e.parent('.jqMultiRadioWrapper').length > 0 ) {
         $e.parent().find("a").remove();
         $e.unwrap().removeClass("jqMultiHide");
      }

      $e.addClass('jqMultiHide').wrap('<div class="jqMultiRadioWrapper">').
        parent().on('click', 'a', clickDelegate).
        on('change', 'input[type="radio"]', changeDelegate).
        prepend('<a href="#"' + klass + rel + '><span></span></a>');
    };
    $(context).find('input:radio').addBack('input:radio').each(processRadios);

    //process  checkboxes
    var processCheckboxes = function() {
      var $e = $(this);
      // if this has already been made then unmake it first, since the values may
      // have changed. this will kill off the objects that listeners have been
      // attached to
      if ($e.parent('.jqMultiCheckboxWrapper').length > 0 ) {
         $e.parent().find("a").remove();
         $e.unwrap().removeClass("jqMultiHide");
      }
      $(this).addClass('jqMultiHide').wrap('<div class="jqMultiCheckboxWrapper" />').
        parent().delegate('a', 'click', clickDelegate).
        delegate('input', 'change', changeDelegate).
        prepend('<a href="#" rel="' + this.name + '"><span></span></a>');
    };
    $(context).find('input:checkbox').addBack('input:checkbox').each(processCheckboxes);

  }

  /**
   * A Drupal behaviour for adding  auto-tabbing functionality to card number
   * and sort code fields.
   */
  Drupal.behaviors.everlineAutotab = function(context) {
    $('input.wonga-fields-bank-card-part').autotab_magic();
    $('input.wonga-sortcode-part').autotab_magic();
  };

})(jQuery, Drupal);
