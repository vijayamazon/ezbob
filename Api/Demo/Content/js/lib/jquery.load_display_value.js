/*

Copyright (c) 2014 EZBOB, http://www.ezbob.com/

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
(function($) {
	$.fn.load_display_value = function(options) {
		var oDefaults = {
			data_source: null,
			data_field_attr: null,
			callback: null,
			realFieldName: null,
			set_text: false,
			fieldNameSetText: null,
		};

		var oArgs = $.extend({}, oDefaults, options);

		if (!oArgs.data_source)
			return;

		if (!oArgs.data_field_attr)
			oArgs.data_field_attr = 'data-field-name';

		$(this).each(function() {
			var oElm = $(this);

			var sFieldName = oElm.attr(oArgs.data_field_attr);
			var sRealFieldName = sFieldName;

			if (oArgs.realFieldName)
				sRealFieldName = oArgs.realFieldName(sFieldName);

			var bSetText = oArgs.set_text;

			if (oArgs.fieldNameSetText)
				bSetText = oArgs.fieldNameSetText(sFieldName);

			if (oArgs.callback)
				oElm.set_display_value(oArgs.callback(sFieldName, oArgs.data_source[sRealFieldName]), bSetText);
			else
				oElm.set_display_value(oArgs.data_source[sRealFieldName] || '', bSetText);
		});
	}; // main plugin function
})(jQuery);
