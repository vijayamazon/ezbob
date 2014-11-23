namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using AutomationCalculator.Common;
	using Newtonsoft.Json;

	public class ApprovalInputData : ITrailInputData {
		#region public

		#region constructor

		public ApprovalInputData() {
			LatePayments = new List<Payment>();
		} // constructor

		#endregion constructor

		#region method Serialize

		public string Serialize() {
			return JsonConvert.SerializeObject(this);
		} // Serialize

		#endregion method Serialize

		public DateTime DataAsOf { get; private set; }

		public Configuration Configuration { get; private set; }
		public MetaData MetaData { get; private set; }
		public int CustomerID { get { return m_oArguments.CustomerID; } } // CustomerID
		public decimal SystemCalculatedAmount { get { return m_oArguments.SystemCalculatedAmount; } } // SystemCalculatedAmount

		public string WorstStatuses { get; private set; }

		public int MarketplaceSeniority { get; private set; }
		public List<Payment> LatePayments { get; private set; }
		public decimal Turnover1M { get { return m_oTurnover[1]; } } // Turnover1M
		public decimal Turnover3M { get { return m_oTurnover[3]; } } // Turnover3M
		public decimal Turnover1Y { get { return m_oTurnover[12]; } } // Turnover1Y

		public decimal AvailableFunds { get; private set; }

		#region method FullInit

		public void FullInit(
			DateTime oDataAsOf,
			Configuration oCfg,
			Arguments oArgs,
			MetaData oMetaData,
			IEnumerable<string> oWorstStatuses,
			IEnumerable<Payment> oPayments,
			OriginationTime oOriginationTime,
			CalculatedTurnover oTurnover,
			AvailableFunds oFunds
		) {
			SetDataAsOf(oDataAsOf);
			SetConfiguration(oCfg);
			m_oArguments = oArgs;
			SetMetaData(oMetaData);
			SetWorstStatuses(oWorstStatuses);

			if (oPayments != null)
				LatePayments.AddRange(oPayments);

			SetSeniority(oOriginationTime.Seniority);

			SetTurnover(1, oTurnover[1]);
			SetTurnover(3, oTurnover[3]);
			SetTurnover(12, oTurnover[12]);

			SetAvailableFunds(oFunds.Available, oFunds.Reserved);
		} // FullInit

		#endregion method FullInit

		#region method AddLatePayment

		public void AddLatePayment(Payment oPayment) {
			LatePayments.Add(oPayment);
		} // AddLatePayment

		#endregion method AddLatePayment

		#region method SetWorstStatuses

		public void SetWorstStatuses(IEnumerable<string> oWorstStatuses) {
			WorstStatuses = string.Join(",", oWorstStatuses);
		} // SetWorstStatuses

		#endregion method SetWorstStatuses

		#region method SetSeniority

		public void SetSeniority(int v) {
			MarketplaceSeniority = v;
		} // SetSeniority

		#endregion method SetSeniority

		#region method SetArgs

		public void SetArgs(int nCustomerID, decimal nAmount) {
			m_oArguments = new Arguments(nCustomerID, nAmount);
		} // SetArgs

		#endregion method SetArgs

		#region method SetDataAsOf

		public void SetDataAsOf(DateTime v) {
			DataAsOf = v;
		} // SetDataAsOf

		#endregion method SetDataAsOf

		#region method SetAvailableFunds

		public void SetAvailableFunds(decimal nAvailable, decimal nReserved) {
			AvailableFunds = nAvailable - nReserved;
		} // SetAvailableFunds

		#endregion method SetAvailableFunds

		#region method SetTurnover

		public void SetTurnover(int nMonthCount, decimal nTurnover) {
			if (m_oTurnover == null)
				m_oTurnover = new SortedDictionary<int, decimal>();

			m_oTurnover[nMonthCount] = nTurnover;
		} // SetTurnover

		#endregion method SetTurnover

		#region method SetWorstStatuses

		public void SetWorstStatuses(string s) {
			WorstStatuses = s ?? string.Empty;
		} // SetWorstStatuses

		#endregion method SetWorstStatuses

		#region method SetConfiguration

		public void SetConfiguration(Configuration oCfg) {
			Configuration = oCfg;
		} // SetConfiguration

		#endregion method SetConfiguration

		#region method SetMetaData

		public void SetMetaData(MetaData oMetaData) {
			MetaData = oMetaData;
		} // SetMetaData

		#endregion method SetMetaData

		#endregion public

		#region private

		private Arguments m_oArguments;

		private SortedDictionary<int, decimal> m_oTurnover;

		#endregion private
	} // class ApprovalInputData
} // namespace
