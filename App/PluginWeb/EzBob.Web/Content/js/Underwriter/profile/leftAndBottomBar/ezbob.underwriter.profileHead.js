var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.ProfileHeadView = Backbone.Marionette.ItemView.extend({
	template: '#profile-head-template',

	initialize: function(options) {
		this.loanModel = options.loanModel;
		this.medalModel = options.medalModel;
		this.parentView = options.parentView;

		this.bindTo(this.model, 'change sync', this.render, this);
		this.bindTo(this.loanModel, 'change sync', this.render, this);
		this.bindTo(this.medalModel, 'change sync', this.renderMedal, this);
		
	},

	serializeData: function() {
		return {
			m: this.model.toJSON(),
			loan: this.loanModel.toJSON()
		};
	},

	events: {
		'click a.collapseall': 'collapseAll',
		'click #OfferEditBtn': 'editOfferClick',
	},

	ui: {
		editOfferDiv: '.editOfferDiv',
		automationOffer: '.ez-automation-offer'
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
		btn.children('i').addClass('anim-turn180');
		btn.parents('.box').children('.box-content').slideToggle(500, function() {
			if ($(this).is(":hidden")) {
				btn.children("i").attr('class', 'fa fa-chevron-down');
				that.$el.find(".box-title-collapse").show();
				that.$el.find('#RecalculateMedalBtn').text('Re');
				$.cookie('collapseAll', true);
			} else {
				btn.children('i').attr('class', 'fa fa-chevron-up');
				that.$el.find('.box-title-collapse').hide();
				that.$el.find('#RecalculateMedalBtn').text('Recalculate');
				$.cookie('collapseAll', false);
			}
			$(".profile-content").css({ "margin-top": (that.$el.height() + 10) + "px" });
			return false;
		});
		return false;
	},

	onRender: function () {
		this.loanInfoView = new EzBob.Underwriter.LoanInfoView({
			el: this.ui.editOfferDiv,
			model: this.loanModel,
			parentView: this.parentView,
			medalModel: this.medalModel
		});
		
		this.loanInfoView.render();

		var controlButtons = this.$el.find("#controlButtons");
		this.controlButtonsView = new EzBob.Underwriter.ControlButtonsView(
            {
            	el: controlButtons,
            	model: new Backbone.Model({ customerId: this.model.get('Id') })
            });
		this.controlButtonsView.render();

		this.$el.find('a[data-bug-type]').tooltip({ title: 'Report bug' });

		if (this.loanModel && this.loanModel.get('AutomationOfferModel') && this.loanModel.get('AutomationOfferModel').Amount === 0) {
		    this.ui.automationOffer.hide();
		}
		if (this.loanModel) {
			this.changeDecisionButtonsState(this.loanModel.get('Editable'));
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

		this.medalModel.trigger('change');

		this.$el.find('[data-toggle="tooltip"]').tooltip({
			html: true,
			'placement': 'bottom'
		});

		var offer = this.loanModel.get('OfferedCreditLine') || 0;
		EzBob.drawDonut("offer-donut", '#00ab5d', offer / (EzBob.Config.ManagerMaxLoan || 120000), true);
		var period = this.loanModel.get('RepaymentPeriod') || 0;
		EzBob.drawDi('period-di', '#00ab5d', period / 12);

		if ($.cookie('editOfferVisible') == 'true') {
			this.ui.editOfferDiv.removeClass('hide');
			$(".profile-content").css({ "margin-top": this.$el.height() + 'px' });
		}
		if ($.cookie('collapseAll') == 'true') {
			this.collapseAll();
		}
	},

	renderMedal: function () {
		this.profileHeadMedalView = new EzBob.Underwriter.ProfileHeadMedalView({ el: this.$el.find('#medal-wrapper'), model: this.medalModel });
		this.profileHeadMedalView.render();
	},
	changeDecisionButtonsState: function(isHideAll) {
		var creditResult = this.loanModel.get('CreditResult');
		var isWizardComplete = this.loanModel.get('SystemDecision') !== 'Registered';
		var isEnabled = this.loanModel.get('IsCustomerInEnabledStatus');
		if (isHideAll)
			this.$el.find('#SuspendBtn, #SignatureBtn, #RejectBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn').hide();

		this.$el.find('#MainStrategyIsInProgress').toggle(!!(isWizardComplete && (creditResult === '')));

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

		if (!isWizardComplete) {
			this.$el.find('#ReturnBtn').hide();
			this.$el.find('#RejectBtn').hide();
			this.$el.find('#ApproveBtn').hide();
			this.$el.find('#SuspendBtn').hide();
			this.$el.find('#SignatureBtn').hide();
			this.$el.find('#EscalateBtn').hide();
		} // if

		if (!isEnabled) {
			this.$el.find(
				'#SuspendBtn, #SignatureBtn, #ApproveBtn, #EscalateBtn, #ReturnBtn, #newCreditLineButtonId'
			).addClass('disabled');
		} // if
	}, // changeDecisionButtonsState
});

EzBob.Underwriter.ProfileHeadMedalView = Backbone.Marionette.ItemView.extend({
	template: '#profile-head-medal-template',

	initialize: function () {
		this.bindTo(this.model, 'change sync', this.render, this);
	},

	events: {
		'click #RecalculateMedalBtn': 'recalculateMedalClick'
	},

	recalculateMedalClick: function () {
		var that = this;
		BlockUi();

		$.post(window.gRootPath + 'Underwriter/Medal/RecalculateMedal', {
			customerId: this.model.get('Id')
		}).always(function () {
			that.model.fetch().always(function () {;
				UnBlockUi();
			});
		});
	},

	serializeData: function () {
		return {
			medal: this.model.get('Score') || {},
			logicalGlue: this.model.get('LogicalGlue') || {},
			m: { Id: this.model.get('Id') }
		};
	},

	onRender: function () {
		this.$el.find('[data-toggle="tooltip"]').tooltip({
			html: true,
			'placement': 'bottom'
		});
		var self = this;
		this.$el.find('[data-toggle="tab"]').click(function (el) {
			self.$el.find('#RecalculateMedalBtn').toggle($(this).attr('href') == '#medal');
		});

		var medalHistory = this.model.get('History');
		if (medalHistory) {
			var histData = [];
			//var medalLabels = [];
			_.each(medalHistory.MedalHistories, function (hist, i) {
				histData.push([i + 1, hist.Result * 100, hist.Medal, EzBob.formatDate3(hist.Date), hist.MedalType + (hist.Error ? ' <span class=\'red_cell\'>*</span>' : '') + '</p>']);
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
						color: '#a7a7a7',
						markerOptions: {
							color: '#a7a7a7',
							style: 'filledCircle',
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
		var medal = this.model.get('Score');
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

			EzBob.drawDonut('medalCanvas', fillColor, resultToUse, false);
		}
	}
});