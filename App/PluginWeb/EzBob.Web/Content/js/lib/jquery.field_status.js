/*

Copyright (c) 2013 EZBOB, http://www.ezbob.com/

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
	var oDefaults = {
		required: false,

		width: 16, // pixels (px)
		height: 16, // pixels (px)

		wait_delay: 1000, // milliseconds

		empty_status_name: 'empty',
		wait_status_name: 'wait',
		required_status_name: 'required',

		status_list: {
			// if "inline" exists and not empty it has priority over src
			empty: { src: 'empty.gif', is_empty: true, inline: 'data:image/gif;base64,R0lGODlhAQABAPcAAAAAAAAAVQAAqgAA/wAkAAAkVQAkqgAk/wBJAABJVQBJqgBJ/wBtAABtVQBtqgBt/wCSAACSVQCSqgCS/wC2AAC2VQC2qgC2/wDbAADbVQDbqgDb/wD/AAD/VQD/qgD//yQAACQAVSQAqiQA/yQkACQkVSQkqiQk/yRJACRJVSRJqiRJ/yRtACRtVSRtqiRt/ySSACSSVSSSqiSS/yS2ACS2VSS2qiS2/yTbACTbVSTbqiTb/yT/ACT/VST/qiT//0kAAEkAVUkAqkkA/0kkAEkkVUkkqkkk/0lJAElJVUlJqklJ/0ltAEltVUltqklt/0mSAEmSVUmSqkmS/0m2AEm2VUm2qkm2/0nbAEnbVUnbqknb/0n/AEn/VUn/qkn//20AAG0AVW0Aqm0A/20kAG0kVW0kqm0k/21JAG1JVW1Jqm1J/21tAG1tVW1tqm1t/22SAG2SVW2Sqm2S/222AG22VW22qm22/23bAG3bVW3bqm3b/23/AG3/VW3/qm3//5IAAJIAVZIAqpIA/5IkAJIkVZIkqpIk/5JJAJJJVZJJqpJJ/5JtAJJtVZJtqpJt/5KSAJKSVZKSqpKS/5K2AJK2VZK2qpK2/5LbAJLbVZLbqpLb/5L/AJL/VZL/qpL//7YAALYAVbYAqrYA/7YkALYkVbYkqrYk/7ZJALZJVbZJqrZJ/7ZtALZtVbZtqrZt/7aSALaSVbaSqraS/7a2ALa2Vba2qra2/7bbALbbVbbbqrbb/7b/ALb/Vbb/qrb//9sAANsAVdsAqtsA/9skANskVdskqtsk/9tJANtJVdtJqttJ/9ttANttVdttqttt/9uSANuSVduSqtuS/9u2ANu2Vdu2qtu2/9vbANvbVdvbqtvb/9v/ANv/Vdv/qtv///8AAP8AVf8Aqv8A//8kAP8kVf8kqv8k//9JAP9JVf9Jqv9J//9tAP9tVf9tqv9t//+SAP+SVf+Sqv+S//+2AP+2Vf+2qv+2///bAP/bVf/bqv/b////AP//Vf//qv///yH5BAEAAP8ALAAAAAABAAEAQAgEAP8FBAA7' },
			ok:       { src: 'ok.png',                          inline: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoV2luZG93cykiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MTQ3RDU2QzFBREI3MTFFMkIxRUVGODE5REFENjFBQjIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MTQ3RDU2QzJBREI3MTFFMkIxRUVGODE5REFENjFBQjIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDoxNDdENTZCRkFEQjcxMUUyQjFFRUY4MTlEQUQ2MUFCMiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDoxNDdENTZDMEFEQjcxMUUyQjFFRUY4MTlEQUQ2MUFCMiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PsM21N4AAAFMSURBVHjaYvz//z8DNQEjtQ1kqdmiSpEBLT63GYBmMAOZ8UB8iolSFwEN0wZSG4F4LhCbUGIgSG8uEB8GYm8gbgbilSxkGiYFxL1AHAHlVwK93gEOQzIMMwTi+UCsD+VXwwwjx0BXIF4IxJJQfg/QsDb0cIABZgKGBQHxWiTD1gNxBbaA5QLiLiDeD8S+OAwLA+JFQMwL5V8F4kwg/ovNwGAgLgViWyDeBLXZCEmNPxDPAWJuKP8zEKcB8UtcUb8BiCcjiQUA8R4gzgFid2gE8CLJg8LsGM6cArUxD4g/AHEtVFwQaskvIGZDUr8fmlzwJk4YqAPiBjR5ZMM+AXEREP8m1kAQaATiVhxq+4D4AjHZByN7AvE0NLFLQNxPbH7EBgpA+RLK/g+15BNRxRcOcVA4JUPzLCiyNhNdHuKR+wrEkUDMPqAlNkCAAQBjWUdFhUihWAAAAABJRU5ErkJggg==' },
			fail:     { src: 'fail.png',                        inline: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoV2luZG93cykiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MTg3OEQ2M0VBREI3MTFFMkI2NTNDNTQ4QzkyNDUwNDciIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MTg3OEQ2M0ZBREI3MTFFMkI2NTNDNTQ4QzkyNDUwNDciPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDoxODc4RDYzQ0FEQjcxMUUyQjY1M0M1NDhDOTI0NTA0NyIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDoxODc4RDYzREFEQjcxMUUyQjY1M0M1NDhDOTI0NTA0NyIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/Pjnxfe4AAAG2SURBVHjarJTPK0RRFMffm4WikIySsqLYKDtqyIQoi7GQlZqiKDVR48fSis2UZiFRiomFGEp+/AeUldRkYatEUayspnk+t86r6857r1fm1qfz3rnnfN+59917bMdxrEqOiFXhUSZ4Z9trsBImmbglSP5xqiW73FrWIjhQgnF9zoT5KYlVLLt+291DvpTGbGrf+oLhmOM8eFQWx9xAjebeJjalL7nDyGuAU5JbDLFOzLEhpsabuYcpuDaC2uAEkWoRi2LOodmI26K6DfVg68eGhFrMFQwYCUewAGcwZMzlEJt2X2zzHCLaKJX2GonPHtuilp5EsOgrKKJNsrT+gFOTg1ldzPdgE/SBScjyzS8qgSzMmGKBN4Xgb8y+h+AP7MR87mwk4BZ0Y/Y8YurUdshehxMkuBWTh6i43mEXSvLeBYfEVYW5y7Ui1i6uJxhhifPYCblBaoyp8xcoiJiNOYAecV1AHLGC7Kt6H4SCzM+ZjcSsMCtVqA1fV8+IfBo/61FE8+LKIDpZ1m3oGGnpHK+QCOo0Ws4qFKU7xZVPr/BFNQPoo4rLMP2QuAxmFO6h3vem/Gf8CjAAOAHkNSmHhw4AAAAASUVORK5CYII=' },
			required: { src: 'required.png', is_required: true, inline: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABIAAAARCAYAAADQWvz5AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoV2luZG93cykiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MUQxN0FDQzUzQkRBMTFFMjgwNTk5QjVEODUzQjE2RjMiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MUQxN0FDQzYzQkRBMTFFMjgwNTk5QjVEODUzQjE2RjMiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDoxRDE3QUNDMzNCREExMUUyODA1OTlCNUQ4NTNCMTZGMyIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDoxRDE3QUNDNDNCREExMUUyODA1OTlCNUQ4NTNCMTZGMyIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/Pkzuxz0AAAEWSURBVHjaYvy61o6BAHCD0rvwKWIhYAgzEHcDMSMQGwLxX1wKmQgYFA/EekCsC2UzkGMQNxA3I/FboGIkG1QCxFJIfEmoGMEwArE1gNgAijOwqC8FYl4gvgDFN4D4D0xzDRD7A7EOEHMQCDOQ14qR+D+A+AoQbwR5bQUQyxJhCDYA0iMDMgNk0B0gdgXid2QY9A6azu7AAvsyEHsB8WcSDPkE1XMZPdZOAnEAEH8nwhCQmkCoHqzRvw+InxNh0HOoWpzpiBka8ISALFQtToOkgZiVCINYoWpxGqSAxgcltqVQ/AefWlwGfQPiKUCsCsQxUAxiT4bKgYAiPoP4gLgJiOWBOBeIHyDJgdh5UDmQGgFkjQABBgBVzy6PTKZRGgAAAABJRU5ErkJggg==' },
			wait:     { src: 'wait.gif',     is_wait: true,     inline: 'data:image/gif;base64,R0lGODlhEAAQAPIAAP///wAAAMLCwkJCQgAAAGJiYoKCgpKSkiH/C05FVFNDQVBFMi4wAwEAAAAh/hpDcmVhdGVkIHdpdGggYWpheGxvYWQuaW5mbwAh+QQJCgAAACwAAAAAEAAQAAADMwi63P4wyklrE2MIOggZnAdOmGYJRbExwroUmcG2LmDEwnHQLVsYOd2mBzkYDAdKa+dIAAAh+QQJCgAAACwAAAAAEAAQAAADNAi63P5OjCEgG4QMu7DmikRxQlFUYDEZIGBMRVsaqHwctXXf7WEYB4Ag1xjihkMZsiUkKhIAIfkECQoAAAAsAAAAABAAEAAAAzYIujIjK8pByJDMlFYvBoVjHA70GU7xSUJhmKtwHPAKzLO9HMaoKwJZ7Rf8AYPDDzKpZBqfvwQAIfkECQoAAAAsAAAAABAAEAAAAzMIumIlK8oyhpHsnFZfhYumCYUhDAQxRIdhHBGqRoKw0R8DYlJd8z0fMDgsGo/IpHI5TAAAIfkECQoAAAAsAAAAABAAEAAAAzIIunInK0rnZBTwGPNMgQwmdsNgXGJUlIWEuR5oWUIpz8pAEAMe6TwfwyYsGo/IpFKSAAAh+QQJCgAAACwAAAAAEAAQAAADMwi6IMKQORfjdOe82p4wGccc4CEuQradylesojEMBgsUc2G7sDX3lQGBMLAJibufbSlKAAAh+QQJCgAAACwAAAAAEAAQAAADMgi63P7wCRHZnFVdmgHu2nFwlWCI3WGc3TSWhUFGxTAUkGCbtgENBMJAEJsxgMLWzpEAACH5BAkKAAAALAAAAAAQABAAAAMyCLrc/jDKSatlQtScKdceCAjDII7HcQ4EMTCpyrCuUBjCYRgHVtqlAiB1YhiCnlsRkAAAOwAAAAAAAAAAAA==' }
		},

		initial_status: '',

		class_name: ''
	}; // defaults

	var MY_NAME = 'field_status';

	var TwoStepTransition = function(oImg, sStatusName, oSettings) {
		var oTmpImg = ReplaceImage(oImg, oSettings.wait_status_name, oSettings);

		window.setTimeout(function () {
			ReplaceImage(oTmpImg, sStatusName, oSettings); 
		}, oSettings.wait_delay);
	}; // TwoStepTransition

	var ReplaceImage = function(oImg, sStatus, oSettings) {
		var oOldImg = $(oImg);

		var oNewImg = oSettings.status_list[sStatus].img
			.clone()
			.attr({
				class: oOldImg.attr('class'),
				width: oSettings.width,
				height: oSettings.height,
				id: oOldImg.attr('id')
			})
			.addClass(oSettings.class_name)
			.css('width', oSettings.width + 'px')
			.css('height', oSettings.height + 'px')
			.show()
			.data(MY_NAME, oOldImg.data(MY_NAME));

		oOldImg.replaceWith(oNewImg);

		return oNewImg;
	}; // ReplaceImage

	var oMethods = {
		init: function(options) {
			return this.each(function() {
				var oSettings = $.extend({}, oDefaults, options);

				var oContainer = $('<div />');
				$('body').append(oContainer);
				oContainer.hide();

				for (var i in oSettings.status_list) {
					var oData = oSettings.status_list[i];

					oData.img = $('<img />').attr('src', oData.inline || oData.src);
					oContainer.append(oData.img);

					if (oData.is_wait)
						oSettings.wait_status_name = i;

					if (oData.is_required)
						oSettings.required_status_name = i;

					if (oData.is_empty)
						oSettings.empty_status_name = i;
				} // for

				$(this).data(MY_NAME, oSettings);

				if (oSettings.initial_status) {
					ReplaceImage(this, oSettings.initial_status, oSettings);
					return;
				} // if

				if (oSettings.required)
					ReplaceImage(this, oSettings.required_status_name, oSettings);
				else
					ReplaceImage(this, oSettings.empty_status_name, oSettings);
			}); // each
		}, // init

		clear: function(bWithDelay) {
			var oImg = this;
			var oSettings = $(oImg).data(MY_NAME);

			if (!oSettings)
				return true;

			var sStatusName = oSettings.required ? oSettings.required_status_name : oSettings.empty_status_name;

			if (bWithDelay)
				TwoStepTransition(oImg, sStatusName, oSettings);
			else
				ReplaceImage(oImg, sStatusName, oSettings);

			return true;
		}, // clear

		set: function(sStatusName, bWithDelay) {
			return this.each(function() {
				if (!sStatusName)
					return true;

				sStatusName = (new String(sStatusName)).valueOf().toLowerCase();

				var oImg = this;
				var oSettings = $(oImg).data(MY_NAME);

				if (!oSettings)
					return true;

				if (!oSettings.status_list[sStatusName])
					return true;

				if (bWithDelay)
					TwoStepTransition(oImg, sStatusName, oSettings);
				else
					ReplaceImage(oImg, sStatusName, oSettings);

				return true;
			});
		} // set_status
	}; // methods

	$.fn.field_status = function(method) {
		if (oMethods[method])
			return oMethods[method].apply(this, Array.prototype.slice.call(arguments, 1));
		else if (typeof method === 'object' || !method)
			return oMethods.init.apply(this, arguments);
		else
			$.error('Method is not implemented: ' + method);
	}; // main plugin function
})(jQuery);
