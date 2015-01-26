namespace AutomationCalculator.Turnover {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using TTurnoverType = AutomationCalculator.Common.TurnoverType;

	public abstract class ACalculatedTurnoverBase {
		public abstract void Init();

		public abstract decimal this[int monthCount] { get; }

		public virtual void Add(TurnoverDbRow r) {
			r.WriteToLog(this.log);

			TurnoverTargetMetaData t = FindTarget(r);

			t.Add(r, 1, 3, 6, 12);

			if (OnNewDataAdded != null)
				OnNewDataAdded();
		} // Add

		protected delegate void NewDataAddedEventHandler();

		protected event NewDataAddedEventHandler OnNewDataAdded;

		protected abstract TurnoverTargetMetaData FindTarget(TurnoverDbRow r);

		protected ACalculatedTurnoverBase() {
			this.log = Library.Instance.Log;

			this.online = new SortedDictionary<int, AOneMonthValue>();
			this.hmrc = new SortedDictionary<int, AOneMonthValue>();
			this.yodlee = new SortedDictionary<int, AOneMonthValue>();
		} // constructor

		protected virtual decimal GetHmrc(int monthCount) {
			return this.hmrc.ContainsKey(monthCount) ? this.hmrc[monthCount].Amount : 0;
		} // GetHmrc

		protected virtual decimal GetOneOnline(int monthCount) {
			return this.online.ContainsKey(monthCount) ? this.online[monthCount].Amount : 0;
		} // GetOneOnline

		protected virtual decimal GetYodlee(int monthCount) {
			return this.yodlee.ContainsKey(monthCount) ? this.yodlee[monthCount].Amount : 0;
		} // GetYodlee

		protected virtual decimal GetZero(int monthCount) {
			return 0;
		} // GetZero

		protected static decimal PositiveMin(params decimal[] args) {
			decimal? res = null;

			for (int i = 0; i < args.Length; i++) {
				decimal cur = args[i];

				if (cur <= 0)
					continue;

				if (res == null) {
					res = cur;
					continue;
				} // if

				res = Math.Min(res.Value, cur);
			} // for

			return res ?? 0;
		} // PositiveMin

		protected readonly ASafeLog log;

		protected readonly SortedDictionary<int, AOneMonthValue> online;
		protected readonly SortedDictionary<int, AOneMonthValue> hmrc;
		protected readonly SortedDictionary<int, AOneMonthValue> yodlee;
	} // class CalculatedTurnover
} // namespace
