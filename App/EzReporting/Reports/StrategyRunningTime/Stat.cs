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
			row.Add(Count);

			row.Add(Min);

			if (Count > 0)
				row.Add(MinTime);
			else
				row.Add(null);

			row.Add(Average);

			row.Add(Median);

			row.Add(Max);

			if (Count > 0)
				row.Add(MaxTime);
			else
				row.Add(null);
		} // ToRow

		#endregion public
	} // class Stat
} // namespace
