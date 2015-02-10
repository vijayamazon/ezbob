(function($, Drupal) {
  Drupal.CTools = Drupal.CTools || {};
  Drupal.CTools.Theme = Drupal.CTools.Theme || {};

  /**
   * Returns a function for generating HTML identifiers prefixed with the given
   * string.
   *
   * @param {string} prefix
   *  The string with which arguments to the returned function should be
   *  concatenated.
   *
   * @return {function(string): string}
   *  A function for generating HTML identifiers prefixed with the given
   *  string.
   */
  Drupal.CTools.Theme.makePrefixedIdMaker = function(prefix) {
    return function(id) {
      return prefix + '-' + id;
    };
  };

  /**
   * Converts an HTML snake_case name to a hyphen-case identifier.
   *
   * @param {string} name
   *  The name to convert.
   *
   * @return {string}
   *  The converted identifier.
   */
  Drupal.CTools.Theme.nameToId = function(name) {
    return name.replace(/(\[|\]\[|_)/g, '-').replace(/\]$/g, '');
  };

  /**
   * Converts an HTML snake_case name to an edit-prefixed-hyphen-case
   * identifier.
   *
   * @param {string} name
   *  The name to convert.
   *
   * @return {string}
   *  The converted identifier.
   */
  Drupal.CTools.Theme.nameToEditId = function(name) {
    return 'edit-' + Drupal.CTools.Theme.nameToId(name);
  };

  /**
   * Creates a message element whose markup matches that which would be
   * generated statically by the associated theme function.
   *
   * @param {CToolsMessageSettings} settings
   *  An object describing the message element which should be created.
   *
   * @return {CToolsMessage}
   *  An object containing the created message elements.
   */
  Drupal.CTools.Theme.themeMessage = (function() {
    var typeClasses = {
      tip: 'tip '
    };

    return function(settings) {
      var typeClass = typeof typeClasses[settings.type] === 'undefined' ?
            '' : typeClasses[settings.type],

          klass = typeClass + settings.messageClass,
          $p = $('<p />', {
            'id': settings.messageId,
            'class': 'row ' + klass
          });

      return {
        $message: $p,
        $wrapper: $p
      };
    };
  })();

  /**
   * Creates a wrapped form element whose markup matches that which would be
   * generated statically by the associated theme function.
   *
   * @param {CToolsFormElementSettings} settings
   *  An object describing the form element which should be created.
   *
   * @return {CToolsFormElement}
   *  An object containing the created form elements.
   */
  Drupal.CTools.Theme.themeFormElement = function(settings) {
    var $div = $('<div />', {
          'class': 'row-wrapper'
        }),

        $p = $('<p />', {
          'id': settings.elementId,
          'class': 'row ' + settings.elementClass
        }),

        $label = $('<label />', {
          'for': settings.inputId,
          'class': 'col span_4 ' + settings.labelClass
        }).text(settings.labelText),

        $span = $('<span />', {
          'id': settings.itemId,
          'class': 'col span_8 ' + settings.itemClass
        }),

        $br = $('<br />', {
          'class': 'clearfix'
        });

    settings.$input.attr('id', settings.inputId);
    settings.$input.attr('name', settings.inputName);
    settings.$input.addClass(settings.inputClass);

    $span.append(settings.$input);

    $p.append($label);
    $p.append($span);
    $p.append($br);

    $div.append($p);

    // <select> elements must be further wrapped so that they can be styled
    // correctly.
    if (settings.$input[0].tagName === 'SELECT') {
      settings.$input.wrap('<span class="select-wrapper"/>');
    }

    return {
      $element: $div,
      $item: $p,
      $labelWrapper: $label,
      $label: $label,
      $input: settings.$input,
      $error: $div
    };
  };

  /**
   * Returns the closest "element" wrapper element to a given object.
   *
   * @param {jQuery} $e
   *  The element for which a closest "element" wrapper element should be
   *  retrieved.
   *
   * @return {jQuery}
   *  The closest "element" wrapper element to the given object.
   */
  Drupal.CTools.Theme.closestElement = function($e) {
    return $e.closest('.row-wrapper');
  };

  /**
   * Returns the closest "item" wrapper element to a given object.
   *
   * @param {jQuery} $e
   *  The element for which a closest "item" wrapper element should be
   *  retrieved.
   *
   * @return {jQuery}
   *  The closest "item" wrapper element to the given object.
   */
  Drupal.CTools.Theme.closestItem = function($e) {
    return $e.closest('.row');
  };
})(jQuery, Drupal);
