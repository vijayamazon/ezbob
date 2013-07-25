using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzAnalyticsConsoleClient {
	#region class CountryData

	class CountryData {
		#region public

		#region constructor

		public CountryData(int nVisits = 0, int nVisitors = 0) {
			Visits = nVisits;
			Visitors = nVisitors;
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(int nVisits, int nVisitors) {
			Visits += nVisits;
			Visitors += nVisitors;
		} // Add

		#endregion method Add

		#region property Visits

		public int Visits { get; private set; }

		#endregion property Visits

		#region property Visitors

		public int Visitors { get; private set; }

		#endregion property Visitors

		#endregion public
	} // class CountryData

	#endregion class CountryData
} // namespace EzAnalyticsConsoleClient
