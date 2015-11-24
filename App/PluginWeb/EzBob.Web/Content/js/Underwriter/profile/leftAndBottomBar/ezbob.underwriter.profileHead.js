var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ProfileHeadView = Backbone.Marionette.ItemView.extend({
	template: '#profile-head-template',

	initialize: function(options) {
		this.loanModel = options.loanModel;
		this.personalModel = options.personalModel;
		this.medalModel = options.medalModel;
		this.parentView = options.parentView;

		this.bindTo(this.model, "change sync", this.render, this);
		this.bindTo(this.loanModel, "change sync", this.render, this);
		this.bindTo(this.medalModel, "change sync", this.render, this);
		this.bindTo(this.personalModel, "change sync", this.personalModelChanged, this);
	},

	serializeData: function() {
		return {
			m: this.model.toJSON(),
			loan: this.loanModel.toJSON(),
			medal: this.medalModel.get('Score'),
		};
	},

	events: {
		'click a.collapseall': "collapseAll",
		'click #OfferEditBtn': 'editOfferClick',
		'click #RecalculateMedalBtn': 'recalculateMedalClick'
	},

	ui: {
		editOfferDiv: '.editOfferDiv',
		automationOffer: '.ez-automation-offer'
	},

	recalculateMedalClick: function() {
		var that = this;
		BlockUi();

		$.post(window.gRootPath + 'Underwriter/Medal/RecalculateMedal', {
			customerId: this.model.id
		}).always(function() {
			that.model.fetch();
			that.medalModel.fetch().always(function() {
				UnBlockUi();
			});
		});
	},

	editOfferClick: function() {
		var isHidden = this.ui.editOfferDiv.hasClass('hide');

		this.ui.editOfferDiv.toggleClass('hide', !isHidden);

		$.cookie('editOfferVisible', isHidden);
		$('.profile-content').css({ 'margin-top': (this.$el.height() + 10) + 'px', });
	},

	collapseAll: function() {
		var that = this;
		var btn = this.$el.find('a.collapseall');
		btn.children("i").addClass("anim-turn180");
		btn.parents(".box").children(".box-content").slideToggle(500, function() {
			if ($(this).is(":hidden")) {
				btn.children("i").attr("class", "fa fa-chevron-down");
				that.$el.find(".box-title-collapse").show();
				that.$el.find('#RecalculateMedalBtn').text('Re');
				$.cookie('collapseAll', true);
			} else {
				btn.children("i").attr("class", "fa fa-chevron-up");
				that.$el.find(".box-title-collapse").hide();
				that.$el.find('#RecalculateMedalBtn').text('Recalculate');
				$.cookie('collapseAll', false);
			}
			$(".profile-content").css({ "margin-top": (that.$el.height() + 10) + "px" });
			return false;
		});
		return false;
	},

	personalModelChanged: function(e, a) {
		if (e && a && this.medalModel) {
			this.medalModel.fetch();
		}

		if (e && a && this.model) {
			this.model.fetch();
		}
	},

	onRender: function() {
		if (this.loanInfoView) {
			this.loanInfoView.undelegateEvents();

			var el = $(this.loanInfoView.el);
			
			el.removeData().unbind();

			this.loanInfoView.remove();
			Backbone.Marionette.ItemView.prototype.remove.call(this.loanInfoView);

			el.empty();
		}

		this.loanInfoView = new EzBob.Underwriter.LoanInfoView({
			el: this.ui.editOfferDiv,
			model: this.loanModel,
			personalInfo: this.personalModel,
			parentView: this.parentView
		});

		// if (!this.loanInfoView.$el.hasClass('editOfferDiv')) {
			// this.loanInfoView.$el = this.ui.editOfferDiv; // this.$el.find('.editOfferDiv');
			// this.loanInfoView.delegateEvents();
		// } // if

		this.loanInfoView.render();

		var controlButtons = this.$el.find("#controlButtons");
		this.controlButtonsView = new EzBob.Underwriter.ControlButtonsView(
            {
            	el: controlButtons,
            	model: new Backbone.Model({ customerId: this.model.get("Id") })
            });
		this.controlButtonsView.render();

		this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });

		if (this.loanModel && this.loanModel.get('AutomationOfferModel') && this.loanModel.get('AutomationOfferModel').Amount === 0) {
		    this.ui.automationOffer.hide();
		}

		if (this.personalModel) {
			this.changeDecisionButtonsState(this.personalModel.get("Editable"));
		}

		if (this.model.get('Alerts') !== void 0) {
			if (this.model.get('Alerts').length === 0) {
				$('#customer-label-span').removeClass('label-warning').removeClass('label-important').addClass('label-success');
			} else {
				if (_.some(this.model.get('Alerts'), function(alert) {
                  return alert.AlertType === 'danger';
				})) {
					$('#customer-label-span').removeClass('label-success').removeClass('label-warning').addClass('label-important');
				} else {
					$('#customer-label-span').removeClass('label-success').removeClass('label-important').addClass('label-warning');
				}
			}
		}
		this.$el.find('[data-toggle="tooltip"]').tooltip({
			html: true,
			'placement': 'bottom'
		});

		var medalHistory = this.medalModel.get('History');
		if (medalHistory) {
			var histData = [];
			//var medalLabels = [];
			_.each(medalHistory.MedalHistories, function(hist, i) {
				histData.push([i + 1, hist.Result * 100, hist.Medal, EzBob.formatDate3(hist.Date), hist.MedalType + (hist.Error ? " <span class='red_cell'>*</span>" : "") + "</p>"]);
			});
			if (histData.length > 0) {
				this.medalHistoryGraph = $.jqplot('medalHistory', [histData], {
					title: '',
					axes: {
						yaxis: {
							labelRenderer: $.jqplot.CanvasAxisLabelRenderer,
							min: 0, //score between 0% and 100%
							max: 100,
							tickOptions: {
								show: false,
								formatString: '%.1f %'
							},
							rendererOptions: { drawBaseline: false }
						},
						xaxis: {
							min: 0,
							max: medalHistory.MedalHistories.length + 1,
							tickOptions: { show: false },
							rendererOptions: { drawBaseline: false }
						}
					},
					grid: {
						drawGridLines: false,
						background: 'transparent',
						borderWidth: 0,
						borderColor: 'transparent',
						shadow: false
					},
					seriesDefaults: {
						shadow: false,
						color: "#a7a7a7",
						markerOptions: {
							color: "#a7a7a7",
							style: "filledCircle",
							size: 7,
							shadow: false
						},
					},
					highlighter: {
						show: true,
						showTooltip: true,
						showMarker: false,
						tooltipAxes: 'y',
						yvalues: 4,
						formatString:
						    '<table class="jqplot-highlighter"> \
                                    <tr><td>Score: </td><td>%s</td></tr> \
                                    <tr><td>Medal: </td><td>%s</td></tr> \
                                    <tr><td>Date: </td><td>%s</td></tr> \
                                    <tr><td>Medal Type: </td><td>%s</td></tr> \
                                </table>'
					},
					canvasOverlay: {
						show: true,
						objects: [
						    { horizontalLine: { name: '', y: 0, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
						    { horizontalLine: { name: 'silver', y: 40, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
						    { horizontalLine: { name: 'gold', y: 62, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
						    { horizontalLine: { name: 'platinum', y: 84, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } },
						    { horizontalLine: { name: 'diamond', y: 100, lineWidth: 1, xOffset: 0, color: '#a7a7a7', shadow: false } }
						]
					}
				});
			}
			this.medalBarGraph = $.jqplot('medalBar', [[40], [22], [22], [16]], {
				stackSeries: true,
				captureRightClick: true,
				seriesDefaults: {
					renderer: $.jqplot.BarRenderer,
					rendererOptions: {
						// Put a 30 pixel margin between bars.
						barMargin: 30,
					},
					shadow: false,
				},
				seriesColors: ['silver', '#D4AF37', '#E5E4E2', '#39A3E1'],
				axes: {
					xaxis: {
						renderer: $.jqplot.CategoryAxisRenderer,
						ticks: [''],
						tickOptions: { show: false },
						rendererOptions: { drawBaseline: false }
					},
					yaxis: {
						min: 0,
						max: 100,
						padMin: 0,
						tickOptions: { show: false },
						rendererOptions: { drawBaseline: false }
					}
				},
				grid: {
					drawGridLines: false,
					background: 'transparent',
					borderWidth: 0,
					borderColor: 'transparent',
					shadow: false
				},
			});

		}
		var medal = this.medalModel.get('Score');
		if (medal) {
			var fillColor = 'black';
			var medalToUse = medal.Medal;
			var resultToUse = medal.Result;

			switch (medalToUse) {
				case 'Silver':
					fillColor = 'silver'; break;
				case 'Gold':
					fillColor = '#D4AF37'; break;
				case 'Platinum':
					fillColor = '#E5E4E2'; break;
				case 'Diamond':
					fillColor = '#39A3E1'; break;
			}

			this.drawDonut('medalCanvas', fillColor, resultToUse, false);
		}

		var offer = this.loanModel.get('OfferedCreditLine') || 0;
		this.drawDonut("offer-donut", "#00ab5d", offer / (EzBob.Config.ManagerMaxLoan || 120000), true);
		var period = this.loanModel.get('RepaymentPeriod') || 0;
		this.drawDi('period-di', "#00ab5d", period / 12);

		if ($.cookie('editOfferVisible') == "true") {
			this.ui.editOfferDiv.removeClass('hide');
			$(".profile-content").css({ "margin-top": this.$el.height() + "px" });
		}
		if ($.cookie('collapseAll') == "true") {
			this.collapseAll();
		}
	},

	changeDecisionButtonsState: function(isHideAll) {
		var creditResult = this.personalModel.get('CreditResult');

		if (isHideAll)
			this.$el.find('#SuspendBtn, #SignatureBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn').hide();

		var inProgress = this.$el.find('#MainStrategyIsInProgress');

		if (creditResult === '')
			inProgress.show();
		else
			inProgress.hide();

		switch (creditResult) {
			case '':
				this.$el.find('#ReturnBtn').hide();
				this.$el.find('#RejectBtn').hide();
				this.$el.find('#ApproveBtn').hide();
				this.$el.find('#SuspendBtn').hide();
				this.$el.find('#SignatureBtn').hide();
				this.$el.find('#EscalateBtn').hide();
				this.$el.find('#newCreditLineButtonId').addClass('disabled');
				break;

			case 'WaitingForDecision':
				this.$el.find('#ReturnBtn').hide();
				this.$el.find('#RejectBtn').show();
				this.$el.find('#ApproveBtn').show();
				this.$el.find('#SuspendBtn').show();
				this.$el.find('#SignatureBtn').show();
				this.$el.find('#newCreditLineButtonId').addClass('disabled');
				//if (!escalatedFlag) this.$el.find('#EscalateBtn').show();
				break;

			case 'Rejected':
			case 'Approved':
			case 'Late':
				this.$el.find('#ReturnBtn').hide();
				this.$el.find('#RejectBtn').hide();
				this.$el.find('#ApproveBtn').hide();
				this.$el.find('#SuspendBtn').hide();
				this.$el.find('#SignatureBtn').hide();
				this.$el.find('#EscalateBtn').hide();
				this.$el.find('#newCreditLineButtonId').removeClass('disabled');
				break;

			case 'Escalated':
				this.$el.find('#ReturnBtn').hide();
				this.$el.find('#RejectBtn').show();
				this.$el.find('#ApproveBtn').show();
				this.$el.find('#SuspendBtn').show();
				this.$el.find('#SignatureBtn').show();
				this.$el.find('#EscalateBtn').hide();
				this.$el.find('#newCreditLineButtonId').addClass('disabled');
				break;

			case 'ApprovedPending':
				this.$el.find('#ReturnBtn').show();
				this.$el.find('#RejectBtn').hide();
				this.$el.find('#ApproveBtn').hide();
				this.$el.find('#SuspendBtn').hide();
				this.$el.find('#SignatureBtn').hide();
				this.$el.find('#EscalateBtn').hide();
				this.$el.find('#newCreditLineButtonId').addClass('disabled');
				break;
		} // switch

		if (this.personalModel.get('UserStatus') === 'Registered') {
			this.$el.find('#ReturnBtn').hide();
			this.$el.find('#RejectBtn').hide();
			this.$el.find('#ApproveBtn').hide();
			this.$el.find('#SuspendBtn').hide();
			this.$el.find('#SignatureBtn').hide();
			this.$el.find('#EscalateBtn').hide();
		} // if

		if (!this.personalModel.get('IsCustomerInEnabledStatus')) {
			this.$el.find(
				'#SuspendBtn, #SignatureBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn, #newCreditLineButtonId'
			).addClass('disabled');
		} // if
	}, // changeDecisionButtonsState

	drawDonut: function(canvasId, fillColor, fillPercent, isClock) {
		var canvas = document.getElementById(canvasId);
		if (!canvas) return false;
		var context = canvas.getContext('2d');
		var x = canvas.width / 2;
		var y = canvas.height / 2;
		var radius = isClock ? 35 : 40;
		var startAngle = 1 * Math.PI;
		var endAngle = (isClock ? 2 : 3) * Math.PI;
		var lineWidth = isClock ? 25 : 15;
		var endEngleData = Math.PI * (1 + fillPercent * (isClock ? 1 : 2));
		context.beginPath();
		context.arc(x, y, radius, startAngle, endAngle, false);
		context.lineWidth = lineWidth;
		context.strokeStyle = '#ebebeb';
		context.stroke();
		context.beginPath();
		context.arc(x, y, radius, startAngle, endEngleData, false);
		context.strokeStyle = fillColor;
		context.lineWidth = lineWidth;
		context.stroke();

		if (isClock) {
			context.beginPath();
			context.moveTo(
                x - (radius + lineWidth / 2) * Math.cos(endEngleData - Math.PI),
                y - (radius + lineWidth / 2) * Math.sin(endEngleData - Math.PI)
            );
			context.lineTo(x, y);
			context.strokeStyle = '#000000';
			context.lineWidth = 2;
			context.stroke();
			context.beginPath();
			context.arc(x, y, 5, startAngle, 3 * Math.PI, false);
			context.lineWidth = 5;
			context.strokeStyle = '#ebebeb';
			context.stroke();
		}

		return true;
	},
	drawDi: function(canvasId, fillColor, fillPercent) {
		var canvas = document.getElementById(canvasId);
		if (!canvas) return false;
		var context = canvas.getContext('2d');
		var x = canvas.width / 2;
		var y = canvas.height;
		context.beginPath();
		context.moveTo(x, y);
		context.lineTo(x, 0);
		context.strokeStyle = '#ebebeb';
		context.lineWidth = 15;
		context.stroke();

		context.beginPath();
		context.moveTo(x, y - y * fillPercent);
		context.lineTo(x, y);
		context.strokeStyle = fillColor;
		context.lineWidth = 15;
		context.stroke();
		return true;
	}
});
