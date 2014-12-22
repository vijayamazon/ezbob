namespace Ezbob.Backend.Strategies.VatReturn {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Utils;

	public partial class CalculateVatReturnSummary : AStrategy {
		public CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			m_nCustomerMarketplaceID = nCustomerMarketplaceID;

			m_oSpLoadBusinesses = new LoadBusinessesForVatReturnSummary(m_nCustomerMarketplaceID, DB, Log);
			m_oSpLoadSummaryData = new LoadDataForVatReturnSummary(m_nCustomerMarketplaceID, DB, Log);
			m_oSpLoadRtiMonths = new LoadRtiMonthForVatReturnSummary(m_nCustomerMarketplaceID, DB, Log);

			m_oBusinessRegNums = new SortedDictionary<int, long>();
			m_oRegNumBusinesses = new SortedDictionary<long, TimedBusinessID>();

			m_oBusinessData = new SortedDictionary<long, BusinessData>();
			m_oRtiMonths = new SortedDictionary<DateTime, decimal>();

			Stopper = new Stopper();
		} // constructor

		public override string Name {
			get { return "Calculate VAT return summary"; }
		} // Name

		public override void Execute() {
			Guid oCalculationID = Guid.NewGuid();

			Stopper.Execute(ElapsedDataMemberType.RetrieveDataFromDatabase, () => {
				m_oSpLoadBusinesses.ForEachRowSafe((sr, bRowsetStart) => {
					m_oBusinessRegNums[sr["BusinessID"]] = sr["RegistrationNo"];
					return ActionResult.Continue;
				});

				m_oSpLoadSummaryData.ForEachResult<LoadDataForVatReturnSummary.ResultRow>(oRow => {
					if (oRow.BoxNum < 1)
						return ActionResult.Continue;

					if (!m_oBusinessRegNums.ContainsKey(oRow.BusinessID)) {
						Log.Warn("Registration # not found for business with id {0}.", oRow.BusinessID);
						return ActionResult.Continue;
					} // if

					long nRegNo = m_oBusinessRegNums[oRow.BusinessID];

					if (m_oRegNumBusinesses.ContainsKey(nRegNo))
						m_oRegNumBusinesses[nRegNo].Update(oRow.BusinessID, oRow.DateFrom.Date);
					else
						m_oRegNumBusinesses[nRegNo] = new TimedBusinessID { BusinessID = oRow.BusinessID, Since = oRow.DateFrom.Date, };

					if (m_oBusinessData.ContainsKey(nRegNo))
						m_oBusinessData[nRegNo].Add(oRow);
					else
						m_oBusinessData[nRegNo] = new BusinessData(oRow);

					return ActionResult.Continue;
				});

				m_oCurrentRtiMonthAction = SaveCustomerID;

				m_oSpLoadRtiMonths.ForEachResult<LoadRtiMonthForVatReturnSummary.ResultRow>(oRow => {
					m_oCurrentRtiMonthAction(oRow);
					return ActionResult.Continue;
				}); // for each
			}); // Stopper.Execute

			Log.Debug("Customer ID: {0}, marketplace ID: {1}", m_nCustomerID, m_nCustomerMarketplaceID);

			Log.Debug("Summary data - begin:");

			int nSavedCount = 0;

			foreach (KeyValuePair<long, BusinessData> pair in m_oBusinessData) {
				BusinessData oBusinessData = pair.Value;

				Stopper.Execute(ElapsedDataMemberType.AggregateData, () => oBusinessData.Calculate(m_nOneMonthSalary, m_oRtiMonths));

				Stopper.Execute(ElapsedDataMemberType.StoreAggregatedData, () => {
					// Business registration number stays with the company for its entire life
					// while name and address can change. In our DB BusinessID is bound to
					// company name, address, and registration number therefore in the following
					// assignment we look for the most updated business id associated with
					// company registration number;
					oBusinessData.BusinessID = m_oRegNumBusinesses[
						m_oBusinessRegNums[oBusinessData.BusinessID]
					].BusinessID;

					Log.Debug(oBusinessData);

					var oSp = new SaveVatReturnSummary(DB, Log) {
						CustomerID = m_nCustomerID,
						CustomerMarketplaceID = m_nCustomerMarketplaceID,
						CalculationID = oCalculationID,
						Totals = new[] { oBusinessData },
						Quarters = oBusinessData.QuartersToSave(),
					};

					oSp.ExecuteNonQuery();

					nSavedCount++;
				});
			} // for each

			if (nSavedCount == 0) {
				DB.ExecuteNonQuery(
					"DeleteOtherVatReturnSummary",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", m_nCustomerID),
					new QueryParameter("CustomerMarketplaceID", m_nCustomerMarketplaceID)
				);
			} // if

			Log.Debug("Summary data - end.");

			UpdateBusinessRelevanceAndTotals();
		} // Execute

		public Stopper Stopper { get; private set; } // Stopper

		private void SaveCustomerID(LoadRtiMonthForVatReturnSummary.ResultRow oRow) {
			m_nCustomerID = oRow.CustomerID;
			m_oCurrentRtiMonthAction = SaveSalary;
		} // SaveCustomerID

		private void SaveSalary(LoadRtiMonthForVatReturnSummary.ResultRow oRow) {
			if (oRow.RecordID < 0)
				m_nOneMonthSalary = oRow.AmountPaid; // TODO: currency conversion using oRow.CurrencyCode
			else
				m_oRtiMonths[oRow.DateStart] = oRow.AmountPaid; // TODO: currency conversion using oRow.CurrencyCode
		} // SaveSalary

		private Action<LoadRtiMonthForVatReturnSummary.ResultRow> m_oCurrentRtiMonthAction;
		private int m_nCustomerID;
		private readonly int m_nCustomerMarketplaceID;
		private readonly LoadDataForVatReturnSummary m_oSpLoadSummaryData;
		private readonly LoadRtiMonthForVatReturnSummary m_oSpLoadRtiMonths;
		private readonly LoadBusinessesForVatReturnSummary m_oSpLoadBusinesses;
		private readonly SortedDictionary<long, BusinessData> m_oBusinessData;
		private decimal? m_nOneMonthSalary;
		private readonly SortedDictionary<DateTime, decimal> m_oRtiMonths;
		private readonly SortedDictionary<int, long> m_oBusinessRegNums; 
		private readonly SortedDictionary<long, TimedBusinessID> m_oRegNumBusinesses; 

		private static bool IsZero(decimal? x) {
			if (x == null)
				return true;

			return Math.Abs(x.Value) < 0.0000001m;
		} // IsZero

		private static decimal? Div(decimal? x, decimal? y) {
			if ((x == null) || IsZero(y))
				return null;

			// ReSharper disable PossibleInvalidOperationException
			return x.Value / y.Value;
			// ReSharper restore PossibleInvalidOperationException
		} // Div
	} // class CalculateVatReturnSummary
} // namespace
