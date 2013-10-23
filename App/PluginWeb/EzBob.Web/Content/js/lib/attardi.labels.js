/*

Copyright (c) 2009 Stefano J. Attardi, http://attardi.org/

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
(function ($) {
	function toggleLabel() {
		return $(this).each(function() {
			var input = $(this);

			setTimeout(function () {
				var def = input.attr('title');

				var bIsEmpty = input.val() && (input.val() == input.attr('empty_value'));

				if (bIsEmpty || !input.val() || (input.val() == def)) {
					input.prev('span').css('visibility', '');
					if (def) {
						var dummy = $('<label></label>').text(def).css('visibility', 'hidden').appendTo('body');
						input.prev('span').css('margin-left', dummy.width() + 3 + 'px');
						dummy.remove();
					}
				} else {
					input.prev('span').css('visibility', 'hidden');
				}
			}, 0);
		});
	}; // toggleLabel

	var sSupportedControls = '.attardi-input input, .attardi-input textarea, .attardi-input select';

	$(document).on('cut keydown paste change', sSupportedControls, toggleLabel);

	$(document).on('focusin focus', sSupportedControls, function () {
		var self = $(this);
		self.prev('span').addClass('active');
		self.attardi_labels('toggle');
	});

	$(document).on('focusout blur', sSupportedControls, function () {
		var self = $(this);
		self.prev('span').removeClass('active');
		self.attardi_labels('toggle');
	});

	// set things up as soon as the DOM is ready
	$(function () {
		$(sSupportedControls).each(function () { toggleLabel.call(this); });
	});

	// do it again to detect Chrome autofill
	$(window).load(function () {
		setTimeout(function () {
			$(sSupportedControls).each(function () { toggleLabel.call(this); });
		}, 0);
	});

	var oMethods = {
		toggle: toggleLabel,

		supported_controls: function () { return sSupportedControls; }, // supported_controls

		toggle_all: function() {
			return $(this).each(function() {
				$(this).find(sSupportedControls).each(function() {
					toggleLabel.apply(this);
				});
			});
		} // toggle_all
	}; // oMethods

	$.fn.attardi_labels = function(method) {
		if (oMethods[method])
			return oMethods[method].apply(this, Array.prototype.slice.call(arguments, 1));
		else
			$.error('Method is not implemented: ' + method);
	}; // main plugin function
})(jQuery);
