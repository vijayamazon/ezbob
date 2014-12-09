namespace Ezbob.HmrcHarvester {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text.RegularExpressions;

	class RtiTaxYearRowData {

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

			AmountPaid = AThrasher.ParseGBP(NormaliseAmountStr(sAmountPaid));
			AmountDue = AThrasher.ParseGBP(NormaliseAmountStr(sAmountDue));
		} // constructor

		public int DayStart { get; private set; }
		public int MonthStart { get; private set; }

		public int DayEnd { get; private set; }
		public int MonthEnd { get; private set; }

		public decimal AmountPaid { get; private set; }
		public decimal AmountDue { get; private set; }

		private string NormaliseAmountStr(string sAmount) {
			if (string.IsNullOrWhiteSpace(sAmount))
				return "";

			sAmount = sAmount.Trim();

			int nPos = sAmount.Length - 1;

			while ((nPos >= 0) && ms_oLegalChars.Contains(sAmount[nPos]))
				nPos--;

			if (nPos < 0)
				return sAmount;

			return sAmount.Substring(nPos + 1);
		} // NormaliseAmountStr

		private static readonly SortedSet<char> ms_oLegalChars = new SortedSet<char> {
			Convert.ToChar(65533), Convert.ToChar(163), // pound sign characters
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'-', ',', '.',
		};

	} // class RtiTaxYearRowData
} // namespace Ezbob.HmrcHarvester
