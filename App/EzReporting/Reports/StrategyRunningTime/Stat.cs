namespace Reports.StrategyRunningTime {
	using System;
	using System.Collections.Generic;

	class Stat {
		#region public

		#region constructor

		public Stat() {
			Min = double.MaxValue;
			Max = double.MinValue;
			Average = 0;
			Median = 0;
			Count = 0;
		} // constructor

		#endregion constructor

		public int Count { get; private set; }
		public double Min { get; private set; }
		public double Max { get; private set; }
		public double Average { get; private set; }
		public double Median { get; set; }
		public double Pct75 { get; set; }
		public double Pct90 { get; set; }

		public DateTime MinTime { get; private set; }
		public DateTime MaxTime { get; private set; }

		public void Append(double nValue, DateTime oTime) {
			if (nValue < Min) {
				Min = nValue;
				MinTime = oTime;
			} // if

			if (nValue > Max) {
				Max = nValue;
				MaxTime = oTime;
			} // if

			Average += nValue;

			Count++;
		} // Append

		public void SetAverage() {
			if (Count > 0)
				Average /= Count;
		} // SetAverage

		public void ToRow(List<object> row) {
			if (Count > 0) {
				row.Add(Count);
				row.Add(Min);
				row.Add(MinTime);
				row.Add(Average);
				row.Add(Median);
				row.Add(Pct75);
				row.Add(Pct90);
				row.Add(Max);
				row.Add(MaxTime);
			}
			else {
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
				row.Add(DBNull.Value);
			} // if
		} // ToRow

		#endregion public
	} // class Stat
} // namespace
