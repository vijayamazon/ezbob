namespace FullCustomerModelGreps {
	using System.Collections.Generic;

	class Stat {

		public Stat(double nValue) {
			Min = double.MaxValue;
			Max = double.MinValue;

			Average = 0;
			Median = 0;
			Pct75 = 0;
			Pct90 = 0;
			Count = 0;

			m_oData = new SortedDictionary<double, int>();

			Append(nValue);
		} // constructor

		public int Count { get; private set; }
		public double Min { get; private set; }
		public double Max { get; private set; }
		public double Average { get; private set; }
		public double Median { get; set; }
		public double Pct75 { get; set; }
		public double Pct90 { get; set; }

		public void Append(double nValue) {
			if (nValue < Min)
				Min = nValue;

			if (nValue > Max)
				Max = nValue;

			Average += nValue;

			Count++;

			if (m_oData.ContainsKey(nValue))
				m_oData[nValue]++;
			else
				m_oData[nValue] = 1;
		} // Append

		public void SetAverage() {
			if (Count > 0)
				Average /= Count;

			Median = GetPercentile(50);
			Pct75 = GetPercentile(75);
			Pct90 = GetPercentile(90);
		} // SetAverage

		public override string ToString() {
			return string.Join(";", Count, Min, Average, Median, Pct75, Pct90, Max);
		} // ToString

		private readonly SortedDictionary<double, int> m_oData;

		private double GetPercentile(int nPercentile) {
			double nMedian = 0;

			int nReachCount = (int)((double)Count * (double)nPercentile / 100.0);

			int nCurCount = 0;

			foreach (var pair in m_oData) {
				nCurCount += pair.Value;

				if (nCurCount >= nReachCount) {
					nMedian = pair.Key;
					break;
				} // if
			} // for each

			return nMedian;
		} // GetPercentile

	} // class Stat
} // namespace
