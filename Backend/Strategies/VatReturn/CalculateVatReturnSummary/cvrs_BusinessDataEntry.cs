namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class BusinessDataEntry : BusinessDataOutput {
			#region constructor

			public BusinessDataEntry(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Boxes = new SortedDictionary<int, decimal>();

				DateFrom = oRaw.DateFrom;
				DateTo = oRaw.DateTo;

				Add(oRaw);
			} // constructor

			#endregion constructor

			#region method Add

			public void Add(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Boxes[oRaw.BoxNum] = oRaw.Amount; // TODO: if oRaw.CurrencyCode is not GBP apply currency conversion.
			} // Add

			#endregion method Add

			#region method ForEachBox

			public void ForEachBox(Func<int, decimal, bool> oCallback) {
				if (oCallback == null)
					return;

				foreach (var pair in Boxes) {
					bool bStop = oCallback(pair.Key, pair.Value);

					if (bStop)
						break;
				} // for each
			} // ForEachBox

			#endregion method ForEachBox

			#region method BoxValue

			public decimal BoxValue(int nBoxNum) {
				return Boxes.ContainsKey(nBoxNum) ? Boxes[nBoxNum] : 0;
			} // BoxValue

			#endregion method BoxValue

			#region method ToString

			public override string ToString() {
				return ToString("");
			} // ToString

			public string ToString(string sPrefix) {
				sPrefix = sPrefix ?? "";

				var os = new StringBuilder();

				os.AppendFormat(
					"{0} - {1}",
					DateFrom.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
					DateTo.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)
				);

				foreach (KeyValuePair<int, decimal> pair in Boxes)
					os.AppendFormat("\n{2}{0}: {1}", pair.Key, pair.Value, sPrefix);

				ToString(os, sPrefix);

				return os.ToString();
			} // ToString

			#endregion method ToString

			#region method MonthCount

			public override int MonthCount() {
				return DateFrom.Year == DateTo.Year
					? DateTo.Month - DateFrom.Month + 1
					: 13 - DateFrom.Month + DateTo.Month;
			} // MonthCount

			#endregion method MonthCount

			#region method CalculateOutput

			public void CalculateOutput(bool bDoPctOfAnnualRevenues, decimal? nTotalRevenues) {
				Revenues = BoxValue(6);

				if (bDoPctOfAnnualRevenues && !IsZero(nTotalRevenues))
					PctOfAnnualRevenues = Revenues / nTotalRevenues;
				else
					PctOfAnnualRevenues = null;

				Opex = BoxValue(7);

				TotalValueAdded = BoxValue(6) - BoxValue(7);

				PctOfRevenues = Div(TotalValueAdded, Revenues);

				Tax = null; // TODO: some day...

				Ebida = TotalValueAdded - (Salaries ?? 0) - (Tax ?? 0);

				PctOfAnnual = Div(Ebida, Revenues);

				ActualLoanRepayment = 0; // TODO: some day...

				FreeCashFlow = Ebida - ActualLoanRepayment;
			} // CalculateOutput

			#endregion method CalculateOutput

			#region method SetSalary

			public void SetSalary(decimal? nOneMonthSalary, SortedDictionary<DateTime, decimal> oRtiSalary) {
				if (nOneMonthSalary.HasValue) {
					Salaries = nOneMonthSalary.Value * MonthCount();
					return;
				} // if

				if ((oRtiSalary == null) || (oRtiSalary.Count < 1))
					return;

				foreach (KeyValuePair<DateTime, decimal> pair in oRtiSalary) {
					if ((DateFrom <= pair.Key) && (pair.Key <= DateTo))
						AddSalary(pair.Value);
				} // for each
			} // SetSalary

			#endregion method SetSalary

			#region properties saved in DB

			[UsedImplicitly]
			public DateTime DateFrom { get; set; }

			[UsedImplicitly]
			public DateTime DateTo { get; set; }

			#endregion properties saved in DB

			#region private

			private SortedDictionary<int, decimal> Boxes { get; set; }

			#endregion private
		} // BusinessDataEntry
	} // class CalculateVatReturnSummary
} // namespace
