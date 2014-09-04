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
(function ($) {
	$.fn.set_display_value = function (sValue, oSetText) {
		var bSetText = false;

		if (oSetText === true)
			bSetText = true;
		else if (typeof(oSetText) === 'string') {
			switch (oSetText.toLowerCase()) {
			case 'true':
			case 'text':
				bSetText = true;
				break;
			} // switch
		} // if

		$(this).each(function() {
			if (this.hasOwnProperty('value'))
				this.value = sValue;
			else {
				if (bSetText)
					$(this).text(sValue);
				else
					$(this).html(sValue);
			} // if
		});
	}; // main plugin function
})(jQuery);
