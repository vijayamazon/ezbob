namespace EzBob.Backend.Strategies {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class CalculateVatReturnSummary : AStrategy {
		#region public

		#region constructor

		public CalculateVatReturnSummary(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSpLoad = new LoadDataForVatReturnSummary(nCustomerID, DB, Log);
		}

		// constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Calculate VAT return summary"; }
		}

		// Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oBusinessData = new SortedDictionary<int, BusinessData>();

			m_oSpLoad.ForEachResult<LoadDataForVatReturnSummary.ResultRow>(Process);

			Log.Debug("Data read for calculation - begin:");

			foreach (KeyValuePair<int, BusinessData> pair in m_oBusinessData) {
				pair.Value.Calculate();
				Log.Debug(pair.Value);
			} // for each

			Log.Debug("Data read for calculation - end.");
		}

		// Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method Process

		private ActionResult Process(LoadDataForVatReturnSummary.ResultRow oRow) {
			if (oRow.BoxNum < 1)
				return ActionResult.Continue;

			if (m_oBusinessData.ContainsKey(oRow.BusinessID))
				m_oBusinessData[oRow.BusinessID].Add(oRow);
			else
				m_oBusinessData[oRow.BusinessID] = new BusinessData(oRow);

			return ActionResult.Continue;
		}

		// Process

		#endregion method Process

		#region fields

		private readonly LoadDataForVatReturnSummary m_oSpLoad;
		private SortedDictionary<int, BusinessData> m_oBusinessData;

		#endregion fields

		#region class LoadDataForVatReturnSummary

		private class LoadDataForVatReturnSummary : AStoredProcedure {
			#region constructor

			public LoadDataForVatReturnSummary(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
			}

			// constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return CustomerID > 0;
			}

			// HasValidParameters

			#endregion method HasValidParameters

			#region property CustomerID

			[UsedImplicitly]
			public int CustomerID { get; set; }

			#endregion property CustomerID

			#region class ResultRow

			public class ResultRow : AResultRow {
				#region DB output fields

				[UsedImplicitly]
				public decimal Amount { get; set; }

				[UsedImplicitly]
				public string CurrencyCode { get; set; }

				[UsedImplicitly]
				public string BoxName { get; set; }

				[UsedImplicitly]
				public DateTime DateFrom { get; set; }

				[UsedImplicitly]
				public DateTime DateTo { get; set; }

				[UsedImplicitly]
				public int BusinessID { get; set; }

				#endregion DB output fields

				#region property BoxNum

				public int BoxNum {
					get {
						if (m_nBoxNum.HasValue)
							return m_nBoxNum.Value;

						m_nBoxNum = 0;

						Match m = ms_oBoxNameRegEx.Match(BoxName);

						if (m.Success) {
							int nBoxNum;

							if (int.TryParse(m.Groups[1].Value, out nBoxNum))
								m_nBoxNum = nBoxNum;
						} // if

						return m_nBoxNum.Value;
					} // get
				}

				// BoxNum

				private int? m_nBoxNum;

				#endregion property BoxNum

				#region private

				private static readonly Regex ms_oBoxNameRegEx = new Regex(@"\(Box (\d+)\)$");

				#endregion private
			}

			// class ResultRow

			#endregion class ResultRow
		}

		// class LoadDataForVatReturnSummary

		#endregion class LoadDataForVatReturnSummary

		#region class BusinessDataOutput

		private class BusinessDataOutput {
			public decimal? PctOfAnnualRevenues { get; set; }
			public decimal? Revenues { get; set; }
			public decimal? Opex { get; set; }
			public decimal? TotalValueAdded { get; set; }
			public decimal? PctOfRevenues { get; set; }
			public decimal? Salaries { get; set; }
			public decimal? Tax { get; set; }
			public decimal? Ebida { get; set; }
			public decimal? PctOfAnnual { get; set; }
			public decimal? ActualLoanRepayment { get; set; }
			public decimal? FreeCashFlow { get; set; }

			protected void ToString(StringBuilder os, string sPrefix) {
				os.AppendFormat("\n{0}% of annual revenues: {1}", sPrefix, PctOfAnnualRevenues);
				os.AppendFormat("\n{0}Revenues: {1}", sPrefix, Revenues);
				os.AppendFormat("\n{0}Opex: {1}", sPrefix, Opex);
				os.AppendFormat("\n{0}Total value added: {1}", sPrefix, TotalValueAdded);
				os.AppendFormat("\n{0}% of revenues: {1}", sPrefix, PctOfRevenues);
				os.AppendFormat("\n{0}Salaries: {1}", sPrefix, Salaries);
				os.AppendFormat("\n{0}Tax: {1}", sPrefix, Tax);
				os.AppendFormat("\n{0}Ebida: {1}", sPrefix, Ebida);
				os.AppendFormat("\n{0}% of annual: {1}", sPrefix, PctOfAnnual);
				os.AppendFormat("\n{0}Actual loan repayment: {1}", sPrefix, ActualLoanRepayment);
				os.AppendFormat("\n{0}Free cash flow: {1}", sPrefix, FreeCashFlow);
			} // ToString
		} // class BusinessDataOutput

		#endregion class BusinessDataOutput

		#region class BusinessData

		private class BusinessData : BusinessDataOutput {
			#region constructor

			public BusinessData(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Periods = new SortedDictionary<DateTime, BusinessDataEntry>();

				BusinessID = oRaw.BusinessID;
				CurrencyCode = "GBP"; // TODO: some day...

				Add(oRaw);
			} // constructor

			#endregion constructor

			#region method Add

			public void Add(LoadDataForVatReturnSummary.ResultRow oRaw) {
				if (Periods.ContainsKey(oRaw.DateFrom))
					Periods[oRaw.DateFrom].Add(oRaw);
				else
					Periods[oRaw.DateFrom] = new BusinessDataEntry(oRaw);
			} // oEntry

			#endregion method Add

			#region method ToString

			public override string ToString() {
				var os = new StringBuilder();

				os.AppendFormat(
					"\n\n\tBusiness ID: {0}\n\tCurrency code: {1}\n\t{2}\n\n\t*** Quarters:\n\t{3}\n\n\t*** Box totals:\n\t\t{4}\n\n",
					BusinessID,
					CurrencyCode,
					string.Join("\n\t", Periods.Select(pair => pair.Value.ToString("\t\t"))),
					string.Join("\n\t", Quarters.Select(x => x.ToString("\t\t"))),
					string.Join("\n\t\t", BoxTotals.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)))
				);

				ToString(os, "\t");

				return os.ToString();
			} // ToString

			#endregion method ToString

			#region method Calculate

			public void Calculate() {
				Quarters = Periods.Values.ToArray();
				BoxTotals = new SortedDictionary<int, decimal>();

				// Start index for totals is for last four quarters only.
				int nStartIdx = (Quarters.Length <= 4) ? 0 : Quarters.Length - 4;

				for (int i = nStartIdx; i < Quarters.Length; i++) {
					BusinessDataEntry oEntry = Quarters[i];

					oEntry.ForEachBox((nBoxNum, nAmount) => {
						if (BoxTotals.ContainsKey(nBoxNum))
							BoxTotals[nBoxNum] += nAmount;
						else
							BoxTotals[nBoxNum] = nAmount;

						return false;
					});
				} // for each quarter

				decimal? nOneMonthSalary = 1m; // TODO: load real value from DB, personalInfoModel.get('CompanyEmployeeCountInfo');

				if (nOneMonthSalary.HasValue)
					for (int i = 0; i < Quarters.Length; i++)
						Quarters[i].SetSalary(nOneMonthSalary.Value);

				// TODO: if (!nOneMonthSalary.HasValue && RTI Tax Months data) {}

				PctOfAnnualRevenues = 1m;

				Revenues = BoxTotals.ContainsKey(6) ? BoxTotals[6] : 0;
				bool bRevenuesSum = IsZero(Revenues.Value);

				Opex = BoxTotals.ContainsKey(7) ? BoxTotals[7] : 0;

				TotalValueAdded = Revenues - Opex;

				PctOfRevenues = Div(TotalValueAdded, Revenues);

				// Start index for displaying is for up to last five quarters.
				nStartIdx = (Quarters.Length <= 5) ? 0 : Quarters.Length - 5;

				for (int i = nStartIdx; i < Quarters.Length; i++) {
					var oQuarter = Quarters[i];

					oQuarter.Revenues = oQuarter[6];

					if ((i >= Quarters.Length - 4) && bRevenuesSum)
						oQuarter.PctOfAnnualRevenues = oQuarter.Revenues / Revenues;
					else
						oQuarter.PctOfAnnualRevenues = null;

					oQuarter.Opex = oQuarter[7];

					oQuarter.TotalValueAdded = oQuarter[6] - oQuarter[7];

					oQuarter.PctOfRevenues = Div(oQuarter.TotalValueAdded, oQuarter.Revenues);
				} // for
			} // Calculate

			#endregion method Calculate

			public int BusinessID { get; private set; }
			public string CurrencyCode { get; private set; }
			public SortedDictionary<DateTime, BusinessDataEntry> Periods { get; private set; }

			public BusinessDataEntry[] Quarters { get; private set; }
			public SortedDictionary<int, decimal> BoxTotals { get; private set; }

			private static bool IsZero(decimal? x) {
				if (x == null)
					return true;

				return Math.Abs(x.Value) < 0.0000001m;
			} // IsZero

			private static decimal? Div(decimal? x, decimal? y) {
				if (IsZero(y) || (x == null))
					return null;

				return x.Value / y.Value;
			} // Div
		} // class BusinessData

		#endregion class BusinessData

		#region class BusinessDataEntry

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

			#region indexer

			public decimal this[int nBoxNum] {
				get { return Boxes.ContainsKey(nBoxNum) ? Boxes[nBoxNum] : 0; } // get
			} // indexer

			#endregion indexer

			#region method SetSalary

			public void SetSalary(decimal nOneMonthSalary) {
				int nMonthCount = DateFrom.Year == DateTo.Year
					? DateTo.Month - DateFrom.Month + 1
					: 13 - DateFrom.Month + DateTo.Month;

				Salary = nOneMonthSalary * nMonthCount;
			} // SetSalary

			#endregion method SetSalary

			#region method ToString

			public override string ToString() {
				return this.ToString("");
			} // ToString

			public string ToString(string sPrefix) {
				sPrefix = sPrefix ?? "";

				var os = new StringBuilder();

				os.AppendFormat(
					"{0} - {1}: salary {2}",
					DateFrom.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
					DateTo.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
					Salary
				);

				foreach (KeyValuePair<int, decimal> pair in Boxes)
					os.AppendFormat("\n{2}{0}: {1}", pair.Key, pair.Value, sPrefix);

				ToString(os, sPrefix);

				return os.ToString();
			} // ToString

			#endregion method ToString

			public DateTime DateFrom { get; private set; }
			public DateTime DateTo { get; private set; }
			public SortedDictionary<int, decimal> Boxes { get; private set; }
			public decimal Salary { get; set; }
		} // BusinessDataEntry

		#endregion class BusinessDataEntry

		#endregion private
	} // class CalculateVatReturnSummary
} // namespace EzBob.Backend.Strategies
