namespace AutomationCalculator.Turnover {
	using System;
	using System.Collections.Generic;

	public class AutoRejectTurnover : ACalculatedTurnoverBase {
		public AutoRejectTurnover() {
			calculatedTurnovers = new SortedDictionary<int, decimal>();

			accounting = new SortedDictionary<int, AOneMonthValue>();

			OnNewDataAdded += () => calculatedTurnovers.Clear();
		} // constructor

		public override decimal this[int monthCount] {
			get {
				if (!calculatedTurnovers.ContainsKey(monthCount))
					Calculate(monthCount);

				return calculatedTurnovers[monthCount];
			} // get
		} // indexer

		public override void Init() {} // Init

		protected override TurnoverTargetMetaData FindTarget(TurnoverDbRow r) {
			if (r.MpTypeID == MpType.Hmrc)
				return new TurnoverTargetMetaData(this.hmrc, () => new SimpleOneMonthValue());

			if (r.MpTypeID == MpType.Yodlee)
				return new TurnoverTargetMetaData(this.yodlee, () => new SimpleOneMonthValue());

			return (r.IsPaymentAccount && (r.MpTypeID != MpType.PayPal))
				? new TurnoverTargetMetaData(this.accounting, () => new SimpleOneMonthValue())
				: new TurnoverTargetMetaData(this.online, () => new OnlineOneMonthValue());
		} // FindTarget

		protected readonly SortedDictionary<int, AOneMonthValue> accounting;

		protected virtual decimal GetAccounting(int monthCount) {
			return this.accounting.ContainsKey(monthCount) ? this.accounting[monthCount].Amount : 0;
		} // GetAccounting

		private void Calculate(int monthCount) {
			if (monthCount == 12) { // annual
				calculatedTurnovers[monthCount] = MultiMax(
					12 * GetHmrc(1),       4 * GetHmrc(3),       2 * GetHmrc(6),       GetHmrc(12),
					12 * GetYodlee(1),     4 * GetYodlee(3),     2 * GetYodlee(6),     GetYodlee(12),
					12 * GetAccounting(1), 4 * GetAccounting(3), 2 * GetAccounting(6), GetAccounting(12),
					12 * GetOneOnline(1),  4 * GetOneOnline(3),  2 * GetOneOnline(6),  GetOneOnline(12)
				);
			} else if (monthCount == 3) { // quarter
				calculatedTurnovers[monthCount] = MultiMax(
					GetHmrc(1),       GetHmrc(3),
					GetYodlee(1),     GetYodlee(3),
					GetAccounting(1), GetAccounting(3),
					GetOneOnline(1),  GetOneOnline(3)
				);
			} else { // just in case, currently (Jan 26 2015) not used.
				calculatedTurnovers[monthCount] = MultiMax(
					GetHmrc(monthCount),
					GetYodlee(monthCount),
					GetAccounting(monthCount),
					GetOneOnline(monthCount)
				);
			} // if
		} // Calculate

		protected static decimal MultiMax(params decimal[] args) {
			decimal? res = null;

			for (int i = 0; i < args.Length; i++) {
				decimal cur = args[i];

				if (res == null) {
					res = cur;
					continue;
				} // if

				res = Math.Max(res.Value, cur);
			} // for

			return res ?? 0;
		} // MultiMax

		private readonly SortedDictionary<int, decimal> calculatedTurnovers;
	} // class AutoRejectTurnover
} // namespace
