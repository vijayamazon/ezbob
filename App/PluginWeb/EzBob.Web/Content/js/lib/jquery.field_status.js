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
(function ($) {
	var oDefaults = {
		required: false,

		width: 16, // pixels (px)
		height: 16, // pixels (px)

		wait_delay: 500, // milliseconds
		always_with_delay: true,

		empty_status_name: 'empty',
		wait_status_name: 'wait',
		required_status_name: 'required',

		status_list: {
			// inline: base64 encoded image
			// src: path to image (like <img src="...">)
			// is_empty: "empty" status
			// is_required: "required" status
			// is wait: "wait" status

			// if "inline" exists and not empty it has priority over "src"
			// "src" is also used as image identifier. If it is empty status name is used as image identifier.

			empty: { src: '', is_empty: true, inline: 'data:image/gif;base64,R0lGODlhAQABAPcAAAAAAAAAVQAAqgAA/wAkAAAkVQAkqgAk/wBJAABJVQBJqgBJ/wBtAABtVQBtqgBt/wCSAACSVQCSqgCS/wC2AAC2VQC2qgC2/wDbAADbVQDbqgDb/wD/AAD/VQD/qgD//yQAACQAVSQAqiQA/yQkACQkVSQkqiQk/yRJACRJVSRJqiRJ/yRtACRtVSRtqiRt/ySSACSSVSSSqiSS/yS2ACS2VSS2qiS2/yTbACTbVSTbqiTb/yT/ACT/VST/qiT//0kAAEkAVUkAqkkA/0kkAEkkVUkkqkkk/0lJAElJVUlJqklJ/0ltAEltVUltqklt/0mSAEmSVUmSqkmS/0m2AEm2VUm2qkm2/0nbAEnbVUnbqknb/0n/AEn/VUn/qkn//20AAG0AVW0Aqm0A/20kAG0kVW0kqm0k/21JAG1JVW1Jqm1J/21tAG1tVW1tqm1t/22SAG2SVW2Sqm2S/222AG22VW22qm22/23bAG3bVW3bqm3b/23/AG3/VW3/qm3//5IAAJIAVZIAqpIA/5IkAJIkVZIkqpIk/5JJAJJJVZJJqpJJ/5JtAJJtVZJtqpJt/5KSAJKSVZKSqpKS/5K2AJK2VZK2qpK2/5LbAJLbVZLbqpLb/5L/AJL/VZL/qpL//7YAALYAVbYAqrYA/7YkALYkVbYkqrYk/7ZJALZJVbZJqrZJ/7ZtALZtVbZtqrZt/7aSALaSVbaSqraS/7a2ALa2Vba2qra2/7bbALbbVbbbqrbb/7b/ALb/Vbb/qrb//9sAANsAVdsAqtsA/9skANskVdskqtsk/9tJANtJVdtJqttJ/9ttANttVdttqttt/9uSANuSVduSqtuS/9u2ANu2Vdu2qtu2/9vbANvbVdvbqtvb/9v/ANv/Vdv/qtv///8AAP8AVf8Aqv8A//8kAP8kVf8kqv8k//9JAP9JVf9Jqv9J//9tAP9tVf9tqv9t//+SAP+SVf+Sqv+S//+2AP+2Vf+2qv+2///bAP/bVf/bqv/b////AP//Vf//qv///yH5BAEAAP8ALAAAAAABAAEAQAgEAP8FBAA7' },
			ok: { src: '', inline: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoV2luZG93cykiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MTQ3RDU2QzFBREI3MTFFMkIxRUVGODE5REFENjFBQjIiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MTQ3RDU2QzJBREI3MTFFMkIxRUVGODE5REFENjFBQjIiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDoxNDdENTZCRkFEQjcxMUUyQjFFRUY4MTlEQUQ2MUFCMiIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDoxNDdENTZDMEFEQjcxMUUyQjFFRUY4MTlEQUQ2MUFCMiIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PsM21N4AAAFMSURBVHjaYvz//z8DNQEjtQ1kqdmiSpEBLT63GYBmMAOZ8UB8iolSFwEN0wZSG4F4LhCbUGIgSG8uEB8GYm8gbgbilSxkGiYFxL1AHAHlVwK93gEOQzIMMwTi+UCsD+VXwwwjx0BXIF4IxJJQfg/QsDb0cIABZgKGBQHxWiTD1gNxBbaA5QLiLiDeD8S+OAwLA+JFQMwL5V8F4kwg/ovNwGAgLgViWyDeBLXZCEmNPxDPAWJuKP8zEKcB8UtcUb8BiCcjiQUA8R4gzgFid2gE8CLJg8LsGM6cArUxD4g/AHEtVFwQaskvIGZDUr8fmlzwJk4YqAPiBjR5ZMM+AXEREP8m1kAQaATiVhxq+4D4AjHZByN7AvE0NLFLQNxPbH7EBgpA+RLK/g+15BNRxRcOcVA4JUPzLCiyNhNdHuKR+wrEkUDMPqAlNkCAAQBjWUdFhUihWAAAAABJRU5ErkJggg==' },
			fail: { src: '', inline: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAYAAACNiR0NAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMy1jMDExIDY2LjE0NTY2MSwgMjAxMi8wMi8wNi0xNDo1NjoyNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNiAoV2luZG93cykiIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MTg3OEQ2M0VBREI3MTFFMkI2NTNDNTQ4QzkyNDUwNDciIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MTg3OEQ2M0ZBREI3MTFFMkI2NTNDNTQ4QzkyNDUwNDciPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDoxODc4RDYzQ0FEQjcxMUUyQjY1M0M1NDhDOTI0NTA0NyIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDoxODc4RDYzREFEQjcxMUUyQjY1M0M1NDhDOTI0NTA0NyIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/Pjnxfe4AAAG2SURBVHjarJTPK0RRFMffm4WikIySsqLYKDtqyIQoi7GQlZqiKDVR48fSis2UZiFRiomFGEp+/AeUldRkYatEUayspnk+t86r6857r1fm1qfz3rnnfN+59917bMdxrEqOiFXhUSZ4Z9trsBImmbglSP5xqiW73FrWIjhQgnF9zoT5KYlVLLt+291DvpTGbGrf+oLhmOM8eFQWx9xAjebeJjalL7nDyGuAU5JbDLFOzLEhpsabuYcpuDaC2uAEkWoRi2LOodmI26K6DfVg68eGhFrMFQwYCUewAGcwZMzlEJt2X2zzHCLaKJX2GonPHtuilp5EsOgrKKJNsrT+gFOTg1ldzPdgE/SBScjyzS8qgSzMmGKBN4Xgb8y+h+AP7MR87mwk4BZ0Y/Y8YurUdshehxMkuBWTh6i43mEXSvLeBYfEVYW5y7Ui1i6uJxhhifPYCblBaoyp8xcoiJiNOYAecV1AHLGC7Kt6H4SCzM+ZjcSsMCtVqA1fV8+IfBo/61FE8+LKIDpZ1m3oGGnpHK+QCOo0Ws4qFKU7xZVPr/BFNQPoo4rLMP2QuAxmFO6h3vem/Gf8CjAAOAHkNSmHhw4AAAAASUVORK5CYII=' },
			required: { src: '', is_required: true, inline: 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAadEVYdFNvZnR3YXJlAFBhaW50Lk5FVCB2My41LjEwMPRyoQAAAI9JREFUOE+lz1ENgDAMBNDzMAcYwAAKUIADHMwBFtCCByygAQulowtbwi2h4eOF9eiaFSLyyzsENhVfeUMpgDF/04BZBTU8/xvsAPT3ETiyNMTq3NhSChsS88VTTaqrm5lS2JN3taj0irVubClF2tcG2O62RqibGRp60NCDhh409KChBw09aOhBQw8afie4ADxuz75W+un9AAAAAElFTkSuQmCC' },
			wait: { src: '', is_wait: true, inline: 'data:image/gif;base64,R0lGODlhEAAQAPIAAP///wAAAMLCwkJCQgAAAGJiYoKCgpKSkiH/C05FVFNDQVBFMi4wAwEAAAAh/hpDcmVhdGVkIHdpdGggYWpheGxvYWQuaW5mbwAh+QQJCgAAACwAAAAAEAAQAAADMwi63P4wyklrE2MIOggZnAdOmGYJRbExwroUmcG2LmDEwnHQLVsYOd2mBzkYDAdKa+dIAAAh+QQJCgAAACwAAAAAEAAQAAADNAi63P5OjCEgG4QMu7DmikRxQlFUYDEZIGBMRVsaqHwctXXf7WEYB4Ag1xjihkMZsiUkKhIAIfkECQoAAAAsAAAAABAAEAAAAzYIujIjK8pByJDMlFYvBoVjHA70GU7xSUJhmKtwHPAKzLO9HMaoKwJZ7Rf8AYPDDzKpZBqfvwQAIfkECQoAAAAsAAAAABAAEAAAAzMIumIlK8oyhpHsnFZfhYumCYUhDAQxRIdhHBGqRoKw0R8DYlJd8z0fMDgsGo/IpHI5TAAAIfkECQoAAAAsAAAAABAAEAAAAzIIunInK0rnZBTwGPNMgQwmdsNgXGJUlIWEuR5oWUIpz8pAEAMe6TwfwyYsGo/IpFKSAAAh+QQJCgAAACwAAAAAEAAQAAADMwi6IMKQORfjdOe82p4wGccc4CEuQradylesojEMBgsUc2G7sDX3lQGBMLAJibufbSlKAAAh+QQJCgAAACwAAAAAEAAQAAADMgi63P7wCRHZnFVdmgHu2nFwlWCI3WGc3TSWhUFGxTAUkGCbtgENBMJAEJsxgMLWzpEAACH5BAkKAAAALAAAAAAQABAAAAMyCLrc/jDKSatlQtScKdceCAjDII7HcQ4EMTCpyrCuUBjCYRgHVtqlAiB1YhiCnlsRkAAAOwAAAAAAAAAAAA==' }
		},

		initial_status: '',

		class_name: ''
	}; // defaults

	var MY_NAME = 'field_status';
	var MY_ID = 'A382C616C905462BBAA78345FB1CFA94';

	function OneStepTransition(oImg, sStatus, oSettings) {
		var oOldImg = $(oImg);

		var oNewImg = oSettings.status_list[sStatus].img
			.clone()
			.attr({
				'class': oOldImg.attr('class'),
				height: oSettings.height,
				id: oOldImg.attr('id'),
				width: oSettings.width
			})
			.addClass(oSettings.class_name)
			.css('height', oSettings.height + 'px')
			.css('width', oSettings.width + 'px')
			.data(MY_NAME, oOldImg.data(MY_NAME));

		if (oOldImg.css('display'))
			oNewImg.css('display', oOldImg.css('display'));

		if (oOldImg.css('visibility'))
			oNewImg.css('visibility', oOldImg.css('visibility'));

		oOldImg.replaceWith(oNewImg);

		return oNewImg;
	}; // OneStepTransition

	function TwoStepTransition(oImg, sStatusName, oSettings) {
		var oTmpImg = OneStepTransition(oImg, oSettings.wait_status_name, oSettings);

		window.setTimeout(function () {
			OneStepTransition(oTmpImg, sStatusName, oSettings);
		}, oSettings.wait_delay);
	}; // TwoStepTransition

	function DoTransition(bWithDelay, oImg, sStatusName, oSettings) {
		if (oSettings.always_with_delay || bWithDelay)
			TwoStepTransition(oImg, sStatusName, oSettings);
		else
			OneStepTransition(oImg, sStatusName, oSettings);
	}; // DoTransition

	// Delay modes:
	// 0: without delay unless always_with_delay is set.
	// 1: with delay.
	// 2: without delay. Ignore always_with_delay if it is set.
	function DelayMode(nDelayMode) {
		if ((nDelayMode === null) || (typeof nDelayMode === "undefined"))
			return 0;

		if (nDelayMode === parseInt(nDelayMode)) {
			switch (nDelayMode) {
			case 0:
			case 1:
				return nDelayMode;

			default:
				return 2;
			} // switch
		} // if

		switch (nDelayMode) {
		case '0':
			return 0;

		case '1':
			return 1;

		default:
			return 2;
		} // switch
	}; // DelayMode

	var oMethods = {
		init: function (options) {
			return this.each(function () {
				var oSettings = $.extend({}, oDefaults, options);

				var oContainer = $('div#' + MY_ID + '.' + MY_ID);

				if (oContainer.length == 0) {
					oContainer = $('<div />').attr('id', MY_ID).addClass(MY_ID);
					$('body').append(oContainer);
					oContainer.hide();
				} // if

				for (var i in oSettings.status_list) {
					var oData = oSettings.status_list[i];

					var sImgID = oData.src || i;

					var oImg = null;

					oContainer.find('img').each(function () {
						var oCurImg = $(this);

						if (oCurImg.attr(MY_NAME + '_image_id') == sImgID) {
							oImg = oCurImg;
							return false;
						} // if

						return true;
					});

					if (oImg == null) {
						var oAttr = {};
						oAttr.src = oData.inline || oData.src;
						oAttr[MY_NAME + '_image_id'] = sImgID;

						oData.img = $('<img />').attr(oAttr);
						oContainer.append(oData.img);
					} else
						oData.img = oImg;

					if (oData.is_wait)
						oSettings.wait_status_name = i;

					if (oData.is_required)
						oSettings.required_status_name = i;

					if (oData.is_empty)
						oSettings.empty_status_name = i;
				} // for

				$(this).data(MY_NAME, oSettings);

				if (oSettings.initial_status) {
					OneStepTransition(this, oSettings.initial_status, oSettings);
					return;
				} // if

				if (oSettings.required)
					OneStepTransition(this, oSettings.required_status_name, oSettings);
				else
					OneStepTransition(this, oSettings.empty_status_name, oSettings);
			}); // each
		}, // init

		clear: function (nDelayMode) {
			return this.each(function () {
				var oImg = this;
				var oSettings = $(oImg).data(MY_NAME);

				if (!oSettings)
					return true;

				var sStatusName = oSettings.required ? oSettings.required_status_name : oSettings.empty_status_name;

				nDelayMode = DelayMode(nDelayMode);
				if (2 == nDelayMode)
					OneStepTransition(oImg, sStatusName, oSettings);
				else
					DoTransition(nDelayMode, oImg, sStatusName, oSettings);

				return true;
			}); // each
		}, // clear

		set: function (sStatusName, nDelayMode) {
			if (!sStatusName)
				return this;

			return this.each(function () {
				sStatusName = (new String(sStatusName)).valueOf().toLowerCase();

				var oImg = this;
				var oSettings = $(oImg).data(MY_NAME);

				if (!oSettings)
					return true;

				if (!oSettings.status_list[sStatusName])
					return true;

				nDelayMode = DelayMode(nDelayMode);
				if (2 == nDelayMode)
					OneStepTransition(oImg, sStatusName, oSettings);
				else
					DoTransition(nDelayMode, oImg, sStatusName, oSettings);

				return true;
			}); // each
		}, // set_status

		getStatus: function () {
			var oImg = this;
			return oImg.attr(MY_NAME + '_image_id');
		}
	}; // methods

	$.fn.field_status = function (method) {
		if (oMethods[method])
			return oMethods[method].apply(this, Array.prototype.slice.call(arguments, 1));
		else if (typeof method === 'object' || !method)
			return oMethods.init.apply(this, arguments);
		else
			$.error('Method is not implemented: ' + method);
	}; // main plugin function
})(jQuery);
