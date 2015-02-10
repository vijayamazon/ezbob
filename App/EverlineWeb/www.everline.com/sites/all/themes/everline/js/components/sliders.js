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
