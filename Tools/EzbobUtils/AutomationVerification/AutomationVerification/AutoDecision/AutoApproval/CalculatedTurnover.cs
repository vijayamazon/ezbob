namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using System.Collections.Generic;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using TTurnoverType = AutomationCalculator.Common.TurnoverType;

	public class CalculatedTurnover {
		public virtual decimal this[int monthCount] {
			get {
				decimal res = Math.Max(GetRelevantTurnover(monthCount), 0);

				this.log.Debug("turnover[{0}, '{1}'] = {2}", Grammar.Number(monthCount, "month"), TurnoverType, res);

				return res;
			} // get
		} // indexer

		public CalculatedTurnover(TTurnoverType? turnoverType, ASafeLog log) {
			this.log = log ?? new SafeLog();

			this.turnoverType = turnoverType;

			this.online = new SortedDictionary<int, OneValue>();
			this.hmrc = new SortedDictionary<int, decimal>();
			this.yodlee = new SortedDictionary<int, decimal>();
		} // constructor

		public virtual void Init() {
			switch (TurnoverType) {
			case TTurnoverType.HMRC:
				GetRelevantTurnover = GetHmrc;
				break;
			case TTurnoverType.Bank:
				GetRelevantTurnover = GetYodlee;
				break;
			case TTurnoverType.Online:
				GetRelevantTurnover = GetOnline;
				break;
			case null:
				GetRelevantTurnover = GetZero;
				break;
			default:
				throw new ArgumentOutOfRangeException("Unsupported turnover type: " + TurnoverType.Value, (Exception)null);
			} // switch
		} // Init

		public virtual void Add(TurnoverDbRow r) {
			r.WriteToLog(this.log);

			if (r.MpTypeID == Hmrc) {
				AddTo(this.hmrc, r, 3);
				AddTo(this.hmrc, r, 12);
			} else if (r.MpTypeID == Yodlee) {
				AddTo(this.yodlee, r, 3);
				AddTo(this.yodlee, r, 12);
			} else {
				Add(r, 1);
				Add(r, 3);
				Add(r, 6);
				Add(r, 12);
			} // if
		} // Add

		protected virtual Func<int, decimal> GetRelevantTurnover { get; set; }

		protected virtual TTurnoverType? TurnoverType {
			get { return this.turnoverType; }
			set { this.turnoverType = value; }
		} // TurnoverType

		protected virtual decimal GetHmrc(int monthCount) {
			return this.hmrc.ContainsKey(monthCount) ? this.hmrc[monthCount] : 0;
		} // GetHmrc

		protected virtual decimal GetOneOnline(int monthCount) {
			return this.online.ContainsKey(monthCount) ? this.online[monthCount].Value : 0;
		} // GetOneOnline

		protected virtual decimal GetOnline(int monthCount) {
			if (monthCount != 12)
				return GetOneOnline(monthCount);

			return PositiveMin(
				GetOneOnline(1) * 12,
				GetOneOnline(3) * 4,
				GetOneOnline(6) * 2,
				GetOneOnline(12)
				);
		} // GetOnline

		protected virtual decimal GetYodlee(int monthCount) {
			return this.yodlee.ContainsKey(monthCount) ? this.yodlee[monthCount] : 0;
		} // GetYodlee

		protected virtual decimal GetZero(int monthCount) {
			return 0;
		} // GetZero

		private class OneValue {
			public decimal Value {
				get { return Math.Max(this.ebay, this.payPal) + this.amazon; } // get
			} // Value

			public OneValue(TurnoverDbRow r) {
				this.ebay = 0;
				this.amazon = 0;
				this.payPal = 0;

				Add(r);
			} // constructor

			public void Add(TurnoverDbRow r) {
				if (r.MpTypeID == Ebay)
					this.ebay += r.Turnover;
				else if (r.MpTypeID == PayPal)
					this.payPal += r.Turnover;
				else if (r.MpTypeID == Amazon)
					this.amazon += r.Turnover;
			} // Add

			private decimal ebay;
			private decimal payPal;
			private decimal amazon;
		} // class OneValue

		private static void AddTo(SortedDictionary<int, decimal> dic, TurnoverDbRow r, int monthCount) {
			if (r.MonthCount > monthCount)
				return;

			if (dic.ContainsKey(monthCount))
				dic[monthCount] += r.Turnover;
			else
				dic[monthCount] = r.Turnover;
		} // AddTo

		private static decimal PositiveMin(params decimal[] args) {
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

		private void Add(TurnoverDbRow r, int monthCount) {
			if (r.MonthCount > monthCount)
				return;

			if (this.online.ContainsKey(monthCount))
				this.online[monthCount].Add(r);
			else
				this.online[monthCount] = new OneValue(r);
		} // Add

		private static readonly Guid Ebay = new Guid("A7120CB7-4C93-459B-9901-0E95E7281B59");
		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
		private static readonly Guid Amazon = new Guid("A4920125-411F-4BB9-A52D-27E8A00D0A3B");
		private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
		private static readonly Guid Yodlee = new Guid("107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF");

		private readonly ASafeLog log;
		private readonly SortedDictionary<int, OneValue> online;
		private readonly SortedDictionary<int, decimal> hmrc;
		private readonly SortedDictionary<int, decimal> yodlee;

		private TTurnoverType? turnoverType;
	} // class CalculatedTurnover
} // namespace
