namespace EzBob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using JetBrains.Annotations;

	public partial class CalculateVatReturnSummary : AStrategy {
		private class BusinessData : BusinessDataOutput {

			public BusinessData(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Periods = new SortedDictionary<DateTime, BusinessDataEntry>();

				BusinessID = oRaw.BusinessID;
				CurrencyCode = "GBP"; // TODO: some day...

				Add(oRaw);
			} // constructor

			public void Add(LoadDataForVatReturnSummary.ResultRow oRaw) {
				if (Periods.ContainsKey(oRaw.DateFrom))
					Periods[oRaw.DateFrom].Add(oRaw);
				else
					Periods[oRaw.DateFrom] = new BusinessDataEntry(oRaw);
			} // Add

			public override string ToString() {
				var os = new StringBuilder();

				os.AppendFormat(
					"\n\n\tBusiness ID: {0}\n" +
					"\tCurrency code: {1}\n" +
					"\t{2}\n\n" +
					"\t*** Quarters:\n" +
					"\t{3}\n\n" +
					"\t*** Box totals:\n" +
					"\t\t{4}\n\n" +
					"\t*** Annualized data:\n{5}\n",
					BusinessID,
					CurrencyCode,
					string.Join("\n\t", Periods.Select(pair => pair.Value.ToString("\t\t"))),
					string.Join("\n\t", Quarters.Select(x => x.ToString("\t\t"))),
					string.Join("\n\t\t", BoxTotals.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value))),
					Annualized.ToString("\t\t")
				);

				ToString(os, "\t");

				return os.ToString();
			} // ToString

			public void Calculate(decimal? nOneMonthSalary, SortedDictionary<DateTime, decimal> oRtiSalary) {
				Quarters = Periods.Values.ToArray();
				BoxTotals = new SortedDictionary<int, decimal>();

				for (int i = (Quarters.Length <= 4) ? 0 : Quarters.Length - 4; i < Quarters.Length; i++) {
					Quarters[i].ForEachBox((nBoxNum, nAmount) => {
						if (BoxTotals.ContainsKey(nBoxNum))
							BoxTotals[nBoxNum] += nAmount;
						else
							BoxTotals[nBoxNum] = nAmount;

						return false;
					});
				} // for each quarter

				foreach (var oQuarter in Quarters)
					oQuarter.SetSalary(nOneMonthSalary, oRtiSalary);

				for (int i = (Quarters.Length <= 4 ? 0 : Quarters.Length - 4); i < Quarters.Length; i++)
					AddSalary(Quarters[i].Salaries);

				CalculateOutput();

				for (int i = (Quarters.Length <= 5) ? 0 : Quarters.Length - 5; i < Quarters.Length; i++)
					Quarters[i].CalculateOutput((i >= Quarters.Length - 4), Revenues);

				CalculateAnnualized();
			} // Calculate

			[UsedImplicitly]
			public int BusinessID { get; set; }

			[UsedImplicitly]
			public string CurrencyCode { get; set; }

			// ReSharper disable ValueParameterNotUsed

			[UsedImplicitly]
			public decimal AnnualizedTurnover {
				get { return Annualized.Turnover ?? 0; }
				set { }
			} // AnnualizedTurnover

			[UsedImplicitly]
			public decimal AnnualizedValueAdded {
				get { return Annualized.ValueAdded ?? 0; }
				set { }
			} // AnnualizedValueAdded

			[UsedImplicitly]
			public decimal AnnualizedFreeCashFlow {
				get { return Annualized.FreeCashFlow ?? 0; }
				set { }
			} // AnnualizedFreeCashFlow

			// ReSharper restore ValueParameterNotUsed

			public IEnumerable<BusinessDataEntry> QuartersToSave() {
				if (Quarters.Length <= 5)
					return Quarters;

				var slice = new BusinessDataEntry[5];

				Array.Copy(Quarters, Quarters.Length - 5, slice, 0, 5);

				return slice;
			} // QuartersToSave

			private SortedDictionary<DateTime, BusinessDataEntry> Periods { get; set; }
			private BusinessDataEntry[] Quarters { get; set; }
			private SortedDictionary<int, decimal> BoxTotals { get; set; }

			private void CalculateOutput() {
				PctOfAnnualRevenues = 1m;

				Revenues = BoxTotals.ContainsKey(6) ? BoxTotals[6] : 0;

				Opex = BoxTotals.ContainsKey(7) ? BoxTotals[7] : 0;

				TotalValueAdded = Revenues - Opex;

				PctOfRevenues = Div(TotalValueAdded, Revenues);

				Tax = null; // TODO: some day...

				Ebida = TotalValueAdded - SalariesCalculated;

				PctOfAnnual = Div(Ebida, Revenues);

				ActualLoanRepayment = 0;

				FreeCashFlow = Ebida - ActualLoanRepayment;
			} // CalculateOutput

			private void CalculateAnnualized() {
				Annualized = new AnnualizedData();

				int nMonthCount = 0;

				for (int i = (Quarters.Length <= 4 ? 0 : Quarters.Length - 4); i < Quarters.Length; i++)
					nMonthCount += Quarters[i].MonthCount();

				if (nMonthCount == 0)
					return;

				if (nMonthCount == 12) {
					Annualized.Turnover = Revenues;
					Annualized.ValueAdded = TotalValueAdded;
					Annualized.FreeCashFlow = FreeCashFlow;
					return;
				} // if

				decimal nFactor = 12 / (decimal)nMonthCount;

				Annualized.Turnover = Revenues.HasValue ? Revenues.Value * nFactor : (decimal?)null;
				Annualized.ValueAdded = TotalValueAdded.HasValue ? TotalValueAdded.Value * nFactor : (decimal?)null;
				Annualized.FreeCashFlow = FreeCashFlow.HasValue ? FreeCashFlow.Value * nFactor : (decimal?)null;
			} // CalculateAnnualized

			private AnnualizedData Annualized { get; set; }

		} // class BusinessData
	} // class CalculateVatReturnSummary
} // namespace
