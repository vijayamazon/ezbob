namespace EzBob.Models {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.ValueIntervals;

	public class HmrcManualOnePeriodDataModel {
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }

		public string Period { get; set; }
		public DateTime DueDate { get; set; }

		public Dictionary<int, decimal> BoxData { get; set; }

		public string[] Errors { get; private set; }

		public bool IsValid() {
			var oErrors = new List<string>();

			if (ToDate <= FromDate) {
				oErrors.Add("Period final date is less than period start date in " +
					FromDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + " and " +
					ToDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + "."
				);
			} // if

			if (string.IsNullOrWhiteSpace(Period))
				Period = ToDate.ToString("MM yy", CultureInfo.InvariantCulture);

			if (DueDate < ToDate)
				DueDate = ToDate.AddMonths(1).AddDays(7);

			if ((BoxData == null) || (BoxData.Count < 1))
				oErrors.Add("No box data specified.");

			Errors = oErrors.ToArray();
			return Errors.Length == 0;
		} // IsValid

	} // class HmrcManualOnePeriodDataModel

	public class HmrcManualDataModel {
		public int CustomerID { get; set; }
		public long RegNo { get; set; }
		public string BusinessName { get; set; }
		public string BusinessAddress { get; set; }

		public Dictionary<int, string> BoxNames { get; set; }

		public HmrcManualOnePeriodDataModel[] VatPeriods { get; set; }

		public string[] Errors { get; private set; }

		public bool IsValid() {
			var oErrors = new List<string>();

			if (CustomerID < 1)
				oErrors.Add("Customer ID not specified.");

			if (RegNo < 0)
				oErrors.Add("Invalid RegNo.");

			if (string.IsNullOrWhiteSpace(BusinessAddress))
				oErrors.Add("Business address not set.");

			if (string.IsNullOrWhiteSpace(BusinessName))
				oErrors.Add("Business name not set.");

			if ((BoxNames == null) || (BoxNames.Count == 0))
				oErrors.Add("No box name specified.");
			else {
				foreach (KeyValuePair<int, string> pair in BoxNames)
					if (string.IsNullOrWhiteSpace(pair.Value))
						oErrors.Add("Invalid name for box " + pair.Key + ".");
			} // if

			var oDates = new List<DateInterval>();

			if ((VatPeriods == null) || (VatPeriods.Length < 1))
				oErrors.Add("No period data specified.");
			else {
				foreach (HmrcManualOnePeriodDataModel p in VatPeriods)
					if (p.IsValid()) {
						var di = new DateInterval(p.FromDate, p.ToDate);

						bool bGoodToAdd = true;

						foreach (DateInterval oExisting in oDates) {
							if (oExisting.Intersects(di)) {
								oErrors.Add("Inconsistent date intervals: " + oExisting + " and " + di + ".");
								bGoodToAdd = false;
								break;
							} // if
						} // for each existing interval

						if (bGoodToAdd)
							oDates.Add(di);
					}
					else
						oErrors.AddRange(p.Errors);
			} // if

			string sSequenceError = oDates.SortWithoutCheckSequence();

			if (!string.IsNullOrWhiteSpace(sSequenceError))
				oErrors.Add(sSequenceError);

			Errors = oErrors.ToArray();
			return Errors.Length == 0;
		} // IsValid

	} // class HmrcManualDataModel

} // namespace EzBob.Models
