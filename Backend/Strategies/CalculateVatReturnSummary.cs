namespace EzBob.Backend.Strategies {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using JetBrains.Annotations;

	using RtiSalaryTable = System.Collections.Generic.SortedDictionary<System.DateTime, decimal>;

	public class CalculateVatReturnSummary : AStrategy {
		#region public

		#region constructor

		public CalculateVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerMarketplaceID = nCustomerMarketplaceID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Calculate VAT return summary"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSpLoadSummaryData = new LoadDataForVatReturnSummary(m_nCustomerMarketplaceID, DB, Log);
			m_oSpLoadRtiMonths = new LoadRtiMonthForVatReturnSummary(m_nCustomerMarketplaceID, DB, Log);

			m_oBusinessData = new SortedDictionary<int, BusinessData>();
			m_oRtiMonths = new RtiSalaryTable();

			m_oSpLoadSummaryData.ForEachResult<LoadDataForVatReturnSummary.ResultRow>(ProcessSummaryDataItem);

			m_oCurrentRtiMonthAction = SaveCustomerID;
			m_oSpLoadRtiMonths.ForEachResult<LoadRtiMonthForVatReturnSummary.ResultRow>(ProcessRtiMonthItem);

			Log.Debug("Customer ID: {0}", m_nCustomerID);

			Log.Debug("Summary data - begin:");

			foreach (KeyValuePair<int, BusinessData> pair in m_oBusinessData) {
				pair.Value.Calculate(m_nOneMonthSalary, m_oRtiMonths);

				Log.Debug(pair.Value);

				var oSp = new SaveVatReturnSummary(DB, Log) {
					CustomerID = m_nCustomerID,
					CustomerMarketplaceID = m_nCustomerMarketplaceID,
					Totals = new BusinessData[] { pair.Value },
					Quarters = pair.Value.QuartersToSave(),
				};

				oSp.ExecuteNonQuery();
			} // for each

			Log.Debug("Summary data - end.");
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method ProcessRtiMonthItem

		private ActionResult ProcessRtiMonthItem(LoadRtiMonthForVatReturnSummary.ResultRow oRow) {
			m_oCurrentRtiMonthAction(oRow);
			return ActionResult.Continue;
		} // ProcessRtiMonthItem

		#endregion method ProcessRtiMonthItem

		#region method SaveCustomerID

		private void SaveCustomerID(LoadRtiMonthForVatReturnSummary.ResultRow oRow) {
			m_nCustomerID = oRow.CustomerID;
			m_oCurrentRtiMonthAction = SaveSalary;
		} // SaveCustomerID

		#endregion method SaveCustomerID

		#region method SaveSalary

		private void SaveSalary(LoadRtiMonthForVatReturnSummary.ResultRow oRow) {
			if (oRow.RecordID < 0)
				m_nOneMonthSalary = oRow.AmountPaid; // TODO: currency conversion using oRow.CurrencyCode
			else
				m_oRtiMonths[oRow.DateStart] = oRow.AmountPaid; // TODO: currency conversion using oRow.CurrencyCode
		} // SaveSalary

		#endregion method SaveSalary

		#region method ProcessSummaryDataItem

		private ActionResult ProcessSummaryDataItem(LoadDataForVatReturnSummary.ResultRow oRow) {
			if (oRow.BoxNum < 1)
				return ActionResult.Continue;

			if (m_oBusinessData.ContainsKey(oRow.BusinessID))
				m_oBusinessData[oRow.BusinessID].Add(oRow);
			else
				m_oBusinessData[oRow.BusinessID] = new BusinessData(oRow);

			return ActionResult.Continue;
		} // ProcessSummaryDataItem

		#endregion method ProcessSummaryDataItem

		#region fields

		private Action<LoadRtiMonthForVatReturnSummary.ResultRow> m_oCurrentRtiMonthAction;
		private int m_nCustomerID;
		private readonly int m_nCustomerMarketplaceID;
		private LoadDataForVatReturnSummary m_oSpLoadSummaryData;
		private LoadRtiMonthForVatReturnSummary m_oSpLoadRtiMonths;
		private SortedDictionary<int, BusinessData> m_oBusinessData;
		private decimal? m_nOneMonthSalary;
		private RtiSalaryTable m_oRtiMonths;

		#endregion fields

		#region stored procedure classes

		#region class LoadRtiMonthForVatReturnSummary

		private class LoadRtiMonthForVatReturnSummary : AStoredProcedure {
			#region public

			#region constructor

			public LoadRtiMonthForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region properties

			public int CustomerMarketplaceID { get; set; }

			#endregion properties

			#region class ResultRow

			public class ResultRow : AResultRow {
				[UsedImplicitly]
				public int CustomerID { get; set; }

				[UsedImplicitly]
				public int RecordID { get; set; }

				[UsedImplicitly]
				public DateTime DateStart { get; set; }

				[UsedImplicitly]
				public decimal AmountPaid { get; set; }

				[UsedImplicitly]
				public string CurrencyCode { get; set; }
			} // class ResultRow

			#endregion class ResultRow

			#endregion public
		} // class LoadRtiMonthForVatReturnSummary

		#endregion class LoadRtiMonthForVatReturnSummary

		#region class LoadDataForVatReturnSummary

		private class LoadDataForVatReturnSummary : AStoredProcedure {
			#region constructor

			public LoadDataForVatReturnSummary(int nCustomerMarketplaceID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerMarketplaceID = nCustomerMarketplaceID;
			} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return CustomerMarketplaceID > 0;
			} // HasValidParameters

			#endregion method HasValidParameters

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

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
			} // class ResultRow

			#endregion class ResultRow
		} // class LoadDataForVatReturnSummary

		#endregion class LoadDataForVatReturnSummary

		#region class SaveVatReturnSummary

		private class SaveVatReturnSummary : AStoredProcedure {
			public SaveVatReturnSummary(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (CustomerMarketplaceID > 0);
			} // HasValidParameters

			[UsedImplicitly]
			public int CustomerID { get; set; }

			[UsedImplicitly]
			public int CustomerMarketplaceID { get; set; }

			[UsedImplicitly]
			public DateTime CreationDate {
				get { return DateTime.UtcNow; }
				set { }
			} // CreationDate

			[UsedImplicitly]
			public IEnumerable<BusinessData> Totals { get; set; }

			[UsedImplicitly]
			public IEnumerable<BusinessDataEntry> Quarters { get; set; }
		} // SaveVatReturnSummary

		#endregion class SaveVatReturnSummary

		#endregion stored procedure classes

		#region internal data classes

		#region class BusinessDataOutput

		private class BusinessDataOutput : ITraversable {
			#region properties saved in DB

			public virtual decimal? PctOfAnnualRevenues { get; set; }
			public virtual decimal? Revenues { get; set; }
			public virtual decimal? Opex { get; set; }
			public virtual decimal? TotalValueAdded { get; set; }
			public virtual decimal? PctOfRevenues { get; set; }
			public virtual decimal? Salaries { get; set; }
			public virtual decimal? Tax { get; set; }
			public virtual decimal? Ebida { get; set; }
			public virtual decimal? PctOfAnnual { get; set; }
			public virtual decimal? ActualLoanRepayment { get; set; }
			public virtual decimal? FreeCashFlow { get; set; }

			#region property SalariesMultiplier 

			public virtual decimal SalariesMultiplier {
				get {
					if (m_nSalariesMultiplier == null)
						m_nSalariesMultiplier = (decimal)CurrentValues.Instance.HmrcSalariesMultiplier;

					return m_nSalariesMultiplier.Value;
				} // get

				set {
					// for ITraversable
				} // set
			} // SalariesMultiplier

			private decimal? m_nSalariesMultiplier;

			#endregion property SalariesMultiplier 

			#endregion properties saved in DB

			#region property SalariesCalculated

			public virtual decimal SalariesCalculated {
				get { return (Salaries ?? 0) * SalariesMultiplier; }
			} // SalariesCalculated

			#endregion property SalariesCalculated

			#region method MonthCount

			public virtual int MonthCount() {
				return 1;
			} // MonthCount

			#endregion method MonthCount

			#region method AddSalary

			public virtual void AddSalary(decimal? nDelta) {
				if (nDelta == null)
					return;

				if (!Salaries.HasValue)
					Salaries = 0;

				Salaries += nDelta;
			} // AddSalary

			#endregion method AddSalary

			#region method ToString

			protected virtual void ToString(StringBuilder os, string sPrefix) {
				os.AppendFormat("\n{0}% of annual revenues: {1}", sPrefix, PctOfAnnualRevenues);
				os.AppendFormat("\n{0}Revenues: {1}", sPrefix, Revenues);
				os.AppendFormat("\n{0}Opex: {1}", sPrefix, Opex);
				os.AppendFormat("\n{0}Total value added: {1}", sPrefix, TotalValueAdded);
				os.AppendFormat("\n{0}% of revenues: {1}", sPrefix, PctOfRevenues);
				os.AppendFormat("\n{0}Salaries: {1}", sPrefix, Salaries);
				os.AppendFormat("\n{0}Salaries multiplier: {1}", sPrefix, SalariesMultiplier);
				os.AppendFormat("\n{0}Salaries calculated: {1}", sPrefix, SalariesCalculated);
				os.AppendFormat("\n{0}Tax: {1}", sPrefix, Tax);
				os.AppendFormat("\n{0}Ebida: {1}", sPrefix, Ebida);
				os.AppendFormat("\n{0}% of annual: {1}", sPrefix, PctOfAnnual);
				os.AppendFormat("\n{0}Actual loan repayment: {1}", sPrefix, ActualLoanRepayment);
				os.AppendFormat("\n{0}Free cash flow: {1}", sPrefix, FreeCashFlow);
			} // ToString

			#endregion method ToString
		} // class BusinessDataOutput

		#endregion class BusinessDataOutput

		#region class BusinessData

		private class BusinessData : BusinessDataOutput, IParametrisable {
			#region constructor

			public BusinessData(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Periods = new SortedDictionary<DateTime, BusinessDataEntry>();

				BusinessID = oRaw.BusinessID;
				CurrencyCode = "GBP"; // TODO: some day...

				Add(oRaw);
			} // constructor

			#endregion constructor

			#region method Add

			public virtual void Add(LoadDataForVatReturnSummary.ResultRow oRaw) {
				if (Periods.ContainsKey(oRaw.DateFrom))
					Periods[oRaw.DateFrom].Add(oRaw);
				else
					Periods[oRaw.DateFrom] = new BusinessDataEntry(oRaw);
			} // Add

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

			public virtual void Calculate(decimal? nOneMonthSalary, RtiSalaryTable oRtiSalary) {
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

				for (int i = 0; i < Quarters.Length; i++)
					Quarters[i].SetSalary(nOneMonthSalary, oRtiSalary);

				for (int i = (Quarters.Length <= 4 ? 0 : Quarters.Length - 4); i < Quarters.Length; i++)
					AddSalary(Quarters[i].Salaries);

				CalculateOutput();

				for (int i = (Quarters.Length <= 5) ? 0 : Quarters.Length - 5; i < Quarters.Length; i++)
					Quarters[i].CalculateOutput((i >= Quarters.Length - 4), Revenues);
			} // Calculate

			#endregion method Calculate

			#region properties saved in DB

			public int BusinessID { get; set; }

			public string CurrencyCode { get; set; }

			#endregion properties saved in DB

			#region method ToParameter

			public object[] ToParameter() {
				return new object[] {
					BusinessID,
					CurrencyCode,
					PctOfAnnualRevenues,
					Revenues,
					Opex,
					TotalValueAdded,
					PctOfRevenues,
					Salaries,
					Tax,
					Ebida,
					PctOfAnnual,
					ActualLoanRepayment,
					FreeCashFlow,
					SalariesMultiplier,
				};
			} // ToParameter

			#endregion method ToParameter

			#region method QuartersToSave

			public IEnumerable<BusinessDataEntry> QuartersToSave() {
				if (Quarters.Length <= 5)
					return Quarters;

				var slice = new BusinessDataEntry[5];

				Array.Copy(Quarters, Quarters.Length - 6, slice, 0, 5);

				return slice;
			} // QuartersToSave

			#endregion method QuartersToSave

			#region private

			#region properties

			private SortedDictionary<DateTime, BusinessDataEntry> Periods { get; set; }
			private BusinessDataEntry[] Quarters { get; set; }
			private SortedDictionary<int, decimal> BoxTotals { get; set; }

			#endregion properties

			#region method CalculateOutput

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

			#endregion method CalculateOutput

			#endregion private
		} // class BusinessData

		#endregion class BusinessData

		#region class BusinessDataEntry

		private class BusinessDataEntry : BusinessDataOutput, IParametrisable {
			#region constructor

			public BusinessDataEntry(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Boxes = new SortedDictionary<int, decimal>();

				DateFrom = oRaw.DateFrom;
				DateTo = oRaw.DateTo;

				Add(oRaw);
			} // constructor

			#endregion constructor

			#region method Add

			public virtual void Add(LoadDataForVatReturnSummary.ResultRow oRaw) {
				Boxes[oRaw.BoxNum] = oRaw.Amount; // TODO: if oRaw.CurrencyCode is not GBP apply currency conversion.
			} // Add

			#endregion method Add

			#region method ForEachBox

			public virtual void ForEachBox(Func<int, decimal, bool> oCallback) {
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

			public virtual string ToString(string sPrefix) {
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

				Ebida = (TotalValueAdded ?? 0) - (Salaries ?? 0) - (Tax ?? 0);

				PctOfAnnual = Div(Ebida, Revenues);

				ActualLoanRepayment = 0; // TODO: some day...

				FreeCashFlow = Ebida - ActualLoanRepayment;
			} // CalculateOutput

			#endregion method CalculateOutput

			#region method SetSalary

			public virtual void SetSalary(decimal? nOneMonthSalary, RtiSalaryTable oRtiSalary) {
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

			public virtual DateTime DateFrom { get; set; }
			public virtual DateTime DateTo { get; set; }

			#endregion properties saved in DB

			#region method ToParameter

			public object[] ToParameter() {
				return new object[] {
					DateFrom,
					DateTo,
					PctOfAnnualRevenues,
					Revenues,
					Opex,
					TotalValueAdded,
					PctOfRevenues,
					Salaries,
					Tax,
					Ebida,
					PctOfAnnual,
					ActualLoanRepayment,
					FreeCashFlow,
				};
			} // ToParameter

			#endregion method ToParameter

			#region private

			private SortedDictionary<int, decimal> Boxes { get; set; }

			#endregion private
		} // BusinessDataEntry

		#endregion class BusinessDataEntry

		#endregion internal data classes

		#region method IsZero

		private static bool IsZero(decimal? x) {
			if (x == null)
				return true;

			return Math.Abs(x.Value) < 0.0000001m;
		} // IsZero

		#endregion method IsZero

		#region method Div

		private static decimal? Div(decimal? x, decimal? y) {
			if ((x == null) || IsZero(y))
				return null;

			return x.Value / y.Value;
		} // Div

		#endregion method Div

		#endregion private
	} // class CalculateVatReturnSummary
} // namespace EzBob.Backend.Strategies
