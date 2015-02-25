namespace Ezbob.Backend.Strategies.CalculateLoan.Helpers {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using Ezbob.ValueIntervals;

	public class BadPeriods {
		public BadPeriods() {
			this.badPeriods = new List<DateInterval>();
			this.extremumPoints = new List<ExtremumPoint>();
			Add(DateTime.MinValue, false);
		} // constructor

		public void Clear() {
			this.badPeriods.Clear();
			this.extremumPoints.Clear();
		} // Clear

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

		public void DeepCloneFrom(BadPeriods source = null) {
			if (source == null)
				return;

			bool first = true;

			foreach (ExtremumPoint point in source.extremumPoints) {
				if (first) {
					first = false;
					continue;
				} // if

				this.extremumPoints.Add(point.DeepClone());
			} // for each
		} // DeepCloneFrom

		public override string ToString() {
			CreateIntervals();

			var os = new StringBuilder();

			os.Append("Points:");

			foreach (ExtremumPoint p in this.extremumPoints)
				os.AppendFormat(" ({1} on {0})", p.ChangeTime.DateStr(), p.IsNewStatusBad ? "bad" : "good");

			os.Append(". ");

			if (this.badPeriods.Count > 0) {
				os.Append("Intervals:");

				foreach (DateInterval di in this.badPeriods)
					os.AppendFormat(" {0}", di);

				os.Append(".");
			} else
				os.Append("No bad periods.");

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

			public ExtremumPoint DeepClone() {
				return new ExtremumPoint(ChangeTime, IsNewStatusBad);
			} // DeepClone
		} // class ExtremumPoint

		private readonly List<DateInterval> badPeriods;
		private readonly List<ExtremumPoint> extremumPoints;

		private static CultureInfo Culture {
			get { return Library.Instance.Culture; }
		} // Culture
	} // class BadPeriods
} // namespace
