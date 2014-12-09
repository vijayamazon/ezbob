namespace Reports.EarnedInterest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Ezbob.ValueIntervals;

	public class BadPeriods {

		public static bool IsBad(CustomerStatus nStatus) {
			switch (nStatus) {
			case CustomerStatus.WriteOff:
			case CustomerStatus.Default:
			case CustomerStatus.Legal:
			case CustomerStatus.Bad:
				return true;

			case CustomerStatus.Enabled:
			case CustomerStatus.Disabled:
			case CustomerStatus.Fraud:
			case CustomerStatus.FraudSuspect:
			case CustomerStatus.Risky:
			case CustomerStatus.DebtManagement:
				return false;

			default:
				throw new ArgumentOutOfRangeException("nStatus");
			} // switch
		} // IsBad

		public BadPeriods(DateTime oChangeDate) {
			m_oBadPeriods = new List<DateInterval>();

			m_oExtremumPoints = new List<Tuple<DateTime, bool>>();

			Add(ms_oLongAgo, false);
			Add(oChangeDate, true);
		} // constructor

		public bool IsLastKnownGood {
			get {
				return (m_oExtremumPoints.Count < 1) || !m_oExtremumPoints[m_oExtremumPoints.Count - 1].Item2;
			} // get
		} // IsLastKnownGood

		public void Add(DateTime oChangeDate, bool bIsBad) {
			if ((m_oExtremumPoints.Count < 1) || (IsLastKnownGood != !bIsBad))
				m_oExtremumPoints.Add(new Tuple<DateTime, bool>(oChangeDate.Date, bIsBad));
		} // Add

		public bool Contains(DateTime oDate) {
			CreateIntervals();
			return m_oBadPeriods.Any(oPeriod => oPeriod.Contains(oDate));
		} // Contains

		public int Count {
			get {
				CreateIntervals();
				return m_oBadPeriods.Count;
			} // get
		} // Count

		public override string ToString() {
			CreateIntervals();

			var os = new StringBuilder();

			os.Append("Points:");

			foreach (var p in m_oExtremumPoints)
				os.AppendFormat(" ({0} {1})", p.Item1.ToString("MMM dd yyyy", CultureInfo.InvariantCulture), p.Item2 ? "bad" : "good");

			os.Append(". Intervals:");

			foreach (var di in m_oBadPeriods)
				os.AppendFormat(" {0}", di);

			os.Append(".");

			return os.ToString();
		} // ToString

		private void CreateIntervals() {
			if (m_oBadPeriods.Count > 0)
				return;

			Tuple<DateTime, bool> oCur = m_oExtremumPoints[0];

			for (int i = 1; i < m_oExtremumPoints.Count; i++) {
				Tuple<DateTime, bool> oNext = m_oExtremumPoints[i];

				if (oCur.Item2)
					m_oBadPeriods.Add(new DateInterval(oCur.Item1, oNext.Item1));

				oCur = oNext;
			} // for each

			if (!IsLastKnownGood)
				m_oBadPeriods.Add(new DateInterval(m_oExtremumPoints[m_oExtremumPoints.Count - 1].Item1, DateTime.UtcNow.AddYears(1000)));
		} // CreateIntervals

		private readonly List<DateInterval> m_oBadPeriods;
		private readonly List<Tuple<DateTime, bool>> m_oExtremumPoints;

		private static readonly DateTime ms_oLongAgo = new DateTime(1976, 7, 1, 0, 0, 0, DateTimeKind.Utc);

	} // class BadPeriods
} // namespace
