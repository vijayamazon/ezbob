var EzBob = EzBob || {};
EzBob.Underwriter = EzBob.Underwriter || {};

EzBob.Underwriter.AffordabilityView = Backbone.Marionette.ItemView.extend({
	template: '#affordability-template',

	initialize: function () {
		this.bindTo(this.model, 'change sync', this.render, this);
		BlockUi('on', this.$el);
		return this;
	}, // initialize

	serializeData: function () {
		return {
			affordability: this.model.get('Affordabilities'),
		};
	}, // serializeData

	onRender: function () {
		BlockUi('on', this.$el);

		this.fillRows();

		if (this.model.customerId)
			this.rotateTable();

		UnBlockUi(this.$el);
	}, // onRender

	fillRows: function() {
		var affordability = this.model.get('Affordabilities');

		if (!affordability || !affordability.length)
			return;

		var template = this.$el.find('tfoot').find('tr').first();

		var target = this.$el.find('tbody');
	    if (!affordability)
	        return false;

		for (var i = 0; i < affordability.length; i++) {
			var af = affordability[i];

			if (!af.TypeStr)
				continue;

			var data = {
				TypeStr: af.TypeStr,
				ErrorMsgs: af.ErrorMsgs,
				Annualized: af.IsAnnualized ? 'Annualized ' : '',
				DateFrom: EzBob.formatDate2(af.DateFrom),
				DateTo: EzBob.formatDate2(af.DateTo),
				Revenues: EzBob.formatPoundsNoDecimals(af.Revenues),
				Opex: EzBob.formatPoundsNoDecimals(af.Opex),
				ValueAdded: EzBob.formatPoundsNoDecimals(af.ValueAdded),
				Salaries: EzBob.formatPoundsNoDecimals(af.Salaries),
				Tax: EzBob.formatPoundsNoDecimals(af.Tax),
				Ebitda: EzBob.formatPoundsNoDecimals(af.Ebitda),
				LoanRepayment: EzBob.formatPoundsNoDecimals(af.LoanRepayment),
				FreeCashFlow: EzBob.formatPoundsNoDecimals(af.FreeCashFlow),
			};

			var newEl = template.clone();
			target.append(newEl);

			newEl.find('[data-field-name]').load_display_value({ data_source: data, });

			newEl.find('.affordability-trend-').removeClass('affordability-trend-').addClass('affordability-trend-' + i);

			newEl.find('.alert').addClass('alert-' + (af.FreeCashFlow < 0 ? 'danger' : 'success'));

			if (af.ErrorMsgs)
				newEl.find('.fa-exclamation-circle').data('title', af.ErrorMsgs);
			else
				newEl.find('.fa-exclamation-circle').remove();
		} // for i
	}, // fillRows

	rotateTable: function () {
		this.$el.find('#affordabilityTable').each(function () {
			var $this = $(this);
			var newrows = [];
			$this.find('tr').each(function () {
				var i = 0;
				$(this).find('td').each(function () {
					i++;
					if (newrows[i] === undefined) { newrows[i] = $('<tr></tr>'); }
					newrows[i].append($(this));
				});
			});
			$this.find("tr").remove();
			$.each(newrows, function () {
				$this.append(this);
			});
		});

		this.$el.find('#affordabilityTable tr:first-child td').each(function () {
			$(this).replaceWith('<th>' + $(this).html() + '</th>');
		});

		$($('#affordabilityTable tr')[2]).addClass('green-row');

		var that = this;

		if (!this.model.customerId)
			return;

		_.each(this.model.get('Affordabilities'), function (aford, i) {
			if (_.isFunction(aford))
				return;

			var trendSorted = _.sortBy(aford.TurnoverTrend, function (turnover) {
				return moment.utc(turnover.TheMonth);
			});

			var trendTurnover = _.pluck(trendSorted, 'Turnover').join(',');

			that.$el.find('.affordability-trend-' + i).attr('values', trendTurnover);

			that.$el.find('.affordability-trend-' + i).sparkline('html', {
				width: '100px',
				lineWidth: 1,
				spotRadius: 2,
				lineColor: '#cfcfcf',
				fillColor: 'transparent',
				spotColor: '#cfcfcf',
				maxSpotColor: '#cfcfcf',
				minSpotColor: '#cfcfcf',
				valueSpots: {
					':': '#cfcfcf'
				},
			});
		});
	}, // rotateTable
}); // EzBob.Underwriter.AffordabilityView
