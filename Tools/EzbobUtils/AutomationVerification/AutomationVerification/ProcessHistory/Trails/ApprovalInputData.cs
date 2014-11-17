namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using Newtonsoft.Json;

	public class ApprovalInputData : ITrailInputData {
		#region public

		public DateTime DataAsOf { get; private set; }

		#region method Serialize

		public string Serialize() {
			return JsonConvert.SerializeObject(new {
				date_as_of = DataAsOf,
				configuration = m_oConfiguration,
				arguments = m_oArguments,
				meta_data = m_oMetaData,
				worst_statuses = m_sWorstStatuses,
				max_payment_delay = m_nMaxDelay,
				marketplace_origination_time = m_oSince,
				turnover_one_month = m_oTurnover[1],
				turnover_three_months = m_oTurnover[3],
				turnover_one_year = m_oTurnover[12],
				available_funds = m_nAvailableFunds,
			});
		} // Serialize

		#endregion method Serialize

		#region method Init

		public void Init(
			DateTime oDataAsOf,
			Configuration oCfg,
			Arguments oArgs,
			MetaData oMetaData,
			SortedSet<string> oWorstStatuses,
			List<Payment> oPayments,
			OriginationTime oOriginationTime,
			CalculatedTurnover oTurnover,
			AvailableFunds oFunds
		) {
			DataAsOf = oDataAsOf;
			m_oConfiguration = oCfg;
			m_oArguments = oArgs;
			m_oMetaData = oMetaData;
			m_sWorstStatuses = string.Join(",", oWorstStatuses);
			m_nMaxDelay = oPayments.Max(p => p.Delay);
			m_oSince = oOriginationTime.Since;
			m_oTurnover = oTurnover;
			m_nAvailableFunds = oFunds.Available - oFunds.Reserved;
		} // Init

		#endregion method Init

		#endregion public

		#region private

		private Configuration m_oConfiguration;
		private Arguments m_oArguments;
		private MetaData m_oMetaData;
		private string m_sWorstStatuses;
		private int m_nMaxDelay;
		private DateTime? m_oSince;
		private CalculatedTurnover m_oTurnover;
		private decimal m_nAvailableFunds;

		#endregion private
	} // class ApprovalInputData
} // namespace
