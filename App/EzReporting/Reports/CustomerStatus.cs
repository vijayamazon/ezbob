namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;

	#region enum CustomerStatus

	public enum CustomerStatus {
		Enabled,
		Disabled,
		Fraud,
		Legal,
		Default,
		FraudSuspect,
		Risky,
		Bad,
		WriteOff,
		DebtManagement,
	} // enum CustomerStatus

	#endregion enum CustomerStatus

	#region class StringExt

	internal static class StringExt {
		public static CustomerStatus ParseCustomerStatus(this string sStatus) {
			return (CustomerStatus)Enum.Parse(typeof(CustomerStatus), (sStatus ?? string.Empty).Replace(" ", ""));
		} // ParseCustomerStatus
	} // class StringExt

	#endregion class StringExt

	#region class CustomerStatusChange

	internal class CustomerStatusChange {
		public CustomerStatus OldStatus { get; set; }
		public CustomerStatus NewStatus { get; set; }
		public DateTime ChangeDate { get; set; }
	} // class CustomerStatusChange

	#endregion class CustomerStatusChange

	#region class CustomerStatusHistory

	internal class CustomerStatusHistory {
		public CustomerStatusHistory(int? nCustomerID, DateTime? oDateEnd, AConnection oDB) {
			if (oDB == null)
				throw new NullReferenceException("No DB connection specified for loading CustomerStatusHistory.");

			m_oData = new SortedDictionary<int, List<CustomerStatusChange>>();
			m_oCurrent = new SortedDictionary<int, CustomerStatusChange>();

			oDB.ForEachRowSafe(
				LoadCurrent,
				"LoadCustomerStatus",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", nCustomerID)
			);

			oDB.ForEachRowSafe(
				LoadHistoryItem,
				"LoadCustomerStatusHistory",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", nCustomerID),
				new QueryParameter("DateEnd", oDateEnd)
			);
		} // constructor

		public CustomerStatusChange GetLast(int nCustomerID) {
			return m_oData.ContainsKey(nCustomerID)
				? m_oData[nCustomerID].Last()
				: GetCurrent(nCustomerID);
		} // GetLast

		public CustomerStatusChange GetCurrent(int nCustomerID) {
			return m_oCurrent[nCustomerID];
		} // GetLast

		private void LoadCurrent(SafeReader sr) {
			m_oCurrent[sr["CustomerID"]] = new CustomerStatusChange {
				ChangeDate = sr["SetDate"],
				OldStatus = CustomerStatus.Enabled,
				NewStatus = ((string)sr["Status"]).ParseCustomerStatus(),
			};
		} // LoadCurrent

		private void LoadHistoryItem(SafeReader sr) {
			int nCustomerID = sr["CustomerID"];

			var csc = new CustomerStatusChange {
				ChangeDate = sr["ChangeDate"],
				OldStatus = ((string)sr["OldStatus"]).ParseCustomerStatus(),
				NewStatus = ((string)sr["NewStatus"]).ParseCustomerStatus(),
			};

			if (m_oData.ContainsKey(nCustomerID))
				m_oData[nCustomerID].Add(csc);
			else
				m_oData[nCustomerID] = new List<CustomerStatusChange> { csc };
		} // LoadHistoryItem

		private readonly SortedDictionary<int, List<CustomerStatusChange>> m_oData;
		private readonly SortedDictionary<int, CustomerStatusChange> m_oCurrent;
	} // class CustomerStatusHistory

	#endregion class CustomerStatusHistory
} // namespace
