using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ezbob.HmrcHarvester {
	#region class RtiTaxYearRowData

	class RtiTaxYearRowData {
		#region public

		#region constructor

		public RtiTaxYearRowData(string sPeriod, string sAmountPaid, string sAmountDue) {
			var ci = new CultureInfo("en-GB", false);

			const string sOneMonthPattern = @"(\d+)[^ ]+ (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)";
			MatchCollection match = Regex.Matches(sPeriod, "^" + sOneMonthPattern + " - " + sOneMonthPattern + "$");

			if (match.Count != 1)
				throw new HarvesterException("Unexpected format in period cell.");

			GroupCollection grp = match[0].Groups;

			if (grp.Count != 5)
				throw new HarvesterException("Unexpected format in period cell.");

			DayStart = Convert.ToInt32(grp[1].Value);
			MonthStart = DateTime.ParseExact(grp[2].Value, "MMM", ci).Month;

			DayEnd = Convert.ToInt32(grp[3].Value);
			MonthEnd = DateTime.ParseExact(grp[4].Value, "MMM", ci).Month;

			AmountPaid = AThrasher.ParseGBP(sAmountPaid);
			AmountDue = AThrasher.ParseGBP(sAmountDue);
		} // constructor

		#endregion constructor

		public int DayStart { get; private set; }
		public int MonthStart { get; private set; }

		public int DayEnd { get; private set; }
		public int MonthEnd { get; private set; }

		public decimal AmountPaid { get; private set; }
		public decimal AmountDue { get; private set; }

		#endregion public

		#region protected
		#endregion protected

		#region private
		#endregion private
	} // class RtiTaxYearRowData

	#endregion class RtiTaxYearRowData
} // namespace Ezbob.HmrcHarvester
