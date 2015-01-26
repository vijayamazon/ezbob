namespace AutomationCalculator.AutoDecision.AutoApproval {
	using System;
	using AutomationCalculator.Turnover;
	using Ezbob.Utils.Lingvo;
	using TTurnoverType = AutomationCalculator.Common.TurnoverType;

	public class AutoApprovalTurnover : ACalculatedTurnoverBase {
		public virtual TTurnoverType? TurnoverType { get; set; }

		public override decimal this[int monthCount] {
			get {
				decimal res = Math.Max(GetRelevantTurnover(monthCount), 0);

				this.log.Debug("turnover[{0}, '{1}'] = {2}", Grammar.Number(monthCount, "month"), TurnoverType, res);

				return res;
			} // get
		} // indexer

		public override void Init() {
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

		protected virtual Func<int, decimal> GetRelevantTurnover { get; set; }

		protected override TurnoverTargetMetaData FindTarget(TurnoverDbRow r) {
			if (r.MpTypeID == MpType.Hmrc)
				return new TurnoverTargetMetaData(this.hmrc, () => new SimpleOneMonthValue());

			if (r.MpTypeID == MpType.Yodlee)
				return new TurnoverTargetMetaData(this.yodlee, () => new SimpleOneMonthValue());

			if (r.MpTypeID == MpType.Amazon)
				return new TurnoverTargetMetaData(this.online, () => new OnlineOneMonthValue());

			return null;
		} // FindTarget

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
	} // class AutoApprovalTurnover
} // namespace
