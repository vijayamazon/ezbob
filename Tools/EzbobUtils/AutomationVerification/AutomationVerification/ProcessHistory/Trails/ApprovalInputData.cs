namespace AutomationCalculator.ProcessHistory.Trails {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoApproval;

	public class ApprovalInputData : ITrailInputData {
		#region public

		#region constructor

		public ApprovalInputData() {
		} // constructor

		#endregion constructor

		public DateTime DataAsOf { get; private set; }

		#region method Serialize

		public string Serialize() {
			return null;
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
			CalculatedTurnover oTurnover
		) {
			DataAsOf = oDataAsOf;

			// TODO
		} // Init

		#endregion method Init

		#endregion public

		#region protected
		#endregion protected

		#region private
		#endregion private

	} // class ApprovalInputData
} // namespace
