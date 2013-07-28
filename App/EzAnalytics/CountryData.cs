namespace EzAnalyticsConsoleClient {
	#region class CountryData

	internal class CountryData {
		#region public

		#region constructor

		public CountryData(int nVisitors, int nNew) {
			Visitors = nVisitors;
			New = nNew;
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(int nVisitors, int nNew) {
			Visitors += nVisitors;
			New += nNew;
		} // Add

		#endregion method Add

		#region property Visitors

		public int Visitors { get; private set; }

		#endregion property Visitors

		#region property New

		public int New { get; private set; }

		#endregion property New

		#region property Returning

		public int Returning { get { return Visitors - New; } }

		#endregion property Returning

		#endregion public
	} // class CountryData

	#endregion class CountryData
} // namespace EzAnalyticsConsoleClient
