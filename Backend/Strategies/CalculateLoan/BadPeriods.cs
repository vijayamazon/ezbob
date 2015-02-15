namespace Ezbob.Backend.Strategies.CalculateLoan {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DbConstants;
	using Ezbob.ValueIntervals;

	internal class BadPeriods {
		public static bool IsBad(CollectionStatusNames status) {
			switch (status) {
			case CollectionStatusNames.Enabled:
			case CollectionStatusNames.Disabled:
			case CollectionStatusNames.Fraud:
			case CollectionStatusNames.FraudSuspect:
			case CollectionStatusNames.DebtManagement:
			case CollectionStatusNames.Risky:
			case CollectionStatusNames.DaysMissed1To14:
			case CollectionStatusNames.DaysMissed15To30:
			case CollectionStatusNames.DaysMissed31To45:
			case CollectionStatusNames.DaysMissed46To60:
			case CollectionStatusNames.DaysMissed61To90:
			case CollectionStatusNames.DaysMissed90Plus:
				return false;

			case CollectionStatusNames.Legal:
			case CollectionStatusNames.Default:
			case CollectionStatusNames.Bad:
			case CollectionStatusNames.WriteOff:
			case CollectionStatusNames.LegalClaimProcess:
			case CollectionStatusNames.LegalApplyForJudgment:
			case CollectionStatusNames.LegalCCJ:
			case CollectionStatusNames.LegalBailiff:
			case CollectionStatusNames.LegalChargingOrder:
			case CollectionStatusNames.CollectionTracing:
			case CollectionStatusNames.CollectionSiteVisit:
			case CollectionStatusNames.IVA_CVA:
			case CollectionStatusNames.Liquidation:
			case CollectionStatusNames.Insolvency:
			case CollectionStatusNames.Bankruptcy:
				return true;

			default:
				throw new ArgumentOutOfRangeException("status");
			} // switch
		} // IsBad

		public BadPeriods(DateTime firstChangeToBadTime) {
			this.badPeriods = new List<DateInterval>();

			this.extremumPoints = new List<ExtremumPoint>();

			Add(firstChangeToBadTime.AddYears(-200), false);
			Add(firstChangeToBadTime, true);
		} // constructor

		public bool IsLastKnownGood {
			get {
				return
					(this.extremumPoints.Count < 1) ||
					!this.extremumPoints[this.extremumPoints.Count - 1].IsNewStatusBad;
			} // get
		} // IsLastKnownGood

		// Status change times should be added in chronological order.
		public void Add(DateTime statusChangeTime, bool isNewStatusBad) {
			this.badPeriods.Clear();

			if ((this.extremumPoints.Count < 1) || (IsLastKnownGood != !isNewStatusBad))
				this.extremumPoints.Add(new ExtremumPoint(statusChangeTime.Date, isNewStatusBad));
		} // Add

		public bool Contains(DateTime aDate) {
			CreateIntervals();
			return this.badPeriods.Any(oPeriod => oPeriod.Contains(aDate));
		} // Contains

		public int Count {
			get {
				CreateIntervals();
				return this.badPeriods.Count;
			} // get
		} // Count

		public override string ToString() {
			CreateIntervals();

			var os = new StringBuilder();

			os.Append("Points:");

			foreach (ExtremumPoint p in this.extremumPoints) {
				os.AppendFormat(
					" ({0} {1})",
					p.ChangeTime.ToString("MMM dd yyyy", CultureInfo.InvariantCulture),
					p.IsNewStatusBad ? "bad" : "good"
				);
			} // for each

			os.Append(". Intervals:");

			foreach (var di in this.badPeriods)
				os.AppendFormat(" {0}", di);

			os.Append(".");

			return os.ToString();
		} // ToString

		private void CreateIntervals() {
			if (this.badPeriods.Count > 0)
				return;

			ExtremumPoint oCur = this.extremumPoints[0];

			for (int i = 1; i < this.extremumPoints.Count; i++) {
				ExtremumPoint oNext = this.extremumPoints[i];

				if (oCur.IsNewStatusBad)
					this.badPeriods.Add(new DateInterval(oCur.ChangeTime, oNext.ChangeTime));

				oCur = oNext;
			} // for each

			if (!IsLastKnownGood) {
				this.badPeriods.Add(new DateInterval(
					this.extremumPoints[this.extremumPoints.Count - 1].ChangeTime,
					DateTime.UtcNow.AddYears(1000)
				));
			} // if
		} // CreateIntervals

		private class ExtremumPoint {
			public ExtremumPoint(DateTime changeTime, bool isNewStatusBad) {
				ChangeTime = changeTime;
				IsNewStatusBad = isNewStatusBad;
			} // ExtremumPoint

			public DateTime ChangeTime { get; private set; }
			public bool IsNewStatusBad { get; private set; }
		} // class ExtremumPoint

		private readonly List<DateInterval> badPeriods;
		private readonly List<ExtremumPoint> extremumPoints;
	} // class BadPeriods
} // namespace
