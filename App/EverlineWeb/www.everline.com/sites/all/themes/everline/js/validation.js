(function($, Drupal) {
  Drupal.Everline = Drupal.Everline || {};
  Drupal.Everline.Validation = Drupal.Everline.Validation || {};

  Drupal.Everline.Validation.getDefaultOptions = function() {
    return {
      errorClass: 'error-tip',
      errorElement: 'span',
      errorPlacement: function($error, $element) {
        $element.closest('.row').children('.col:last').append($error);
      },

      highlight: function(element, errorClass) {
        $(element).closest('.col').
          removeClass('success').addClass('error');
      },

      ignoreTitle: true,
      messages: {},
      rules: {},

      unhighlight: function(element, errorClass) {
        $(element).closest('.col').
          removeClass('error').addClass('success');
      }
    };
  };
})(jQuery, Drupal);
