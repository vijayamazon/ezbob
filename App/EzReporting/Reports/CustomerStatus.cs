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

	public class CustomerStatusChange {
		public CustomerStatus OldStatus { get; set; }
		public CustomerStatus NewStatus { get; set; }
		public DateTime ChangeDate { get; set; }
	} // class CustomerStatusChange

	#endregion class CustomerStatusChange

	#region class CustomerStatusHistory

	public class CustomerStatusHistory {
		#region public

		#region class HistoryData

		public class HistoryData {
			#region constructor

			public HistoryData(CustomerStatusHistory oFullHistory) {
				Data = new SortedDictionary<int, List<CustomerStatusChange>>();
				WriteOffDate = new SortedDictionary<int, DateTime>();

				m_oFullHistory = oFullHistory;
			} // constructor

			#endregion constructor

			#region method GetLast

			public CustomerStatusChange GetLast(int nCustomerID) {
				return Data.ContainsKey(nCustomerID) ? Data[nCustomerID].Last() : m_oFullHistory.GetCurrent(nCustomerID);
			} // GetLast

			#endregion method GetLast

			#region method GetWriteOffDate

			public DateTime? GetWriteOffDate(int nCustomerID) {
				return WriteOffDate.ContainsKey(nCustomerID) ? WriteOffDate[nCustomerID] : (DateTime?)null;
			} // GetWriteOffDate

			#endregion method GetWriteOffDate

			#region method Add

			internal void Add(int nCustomerID, CustomerStatusChange csc) {
				if (Data.ContainsKey(nCustomerID))
					Data[nCustomerID].Add(csc);
				else
					Data[nCustomerID] = new List<CustomerStatusChange> { csc };

				if ((csc.NewStatus == CustomerStatus.WriteOff) && !WriteOffDate.ContainsKey(nCustomerID))
					WriteOffDate[nCustomerID] = csc.ChangeDate;
			} // Add

			#endregion method Add

			public SortedDictionary<int, List<CustomerStatusChange>> Data { get; set; }

			#region private

			private SortedDictionary<int, DateTime> WriteOffDate { get; set;}
			private readonly CustomerStatusHistory m_oFullHistory;

			#endregion private
		} // class HistoryData

		#endregion class HistoryData

		#region constructor

		public CustomerStatusHistory(int? nCustomerID, DateTime? oDateEnd, AConnection oDB) {
			if (oDB == null)
				throw new NullReferenceException("No DB connection specified for loading CustomerStatusHistory.");

			Data = new HistoryData(this);
			FullData = new HistoryData(this);

			m_oCurrent = new SortedDictionary<int, CustomerStatusChange>();

			m_oDateEnd = oDateEnd;

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
				new QueryParameter("DateEnd")
			);
		} // constructor

		#endregion constructor

		#region method GetCurrent

		public CustomerStatusChange GetCurrent(int nCustomerID) {
			return m_oCurrent[nCustomerID];
		} // GetLast

		#endregion method GetCurrent

		public HistoryData Data { get; private set; }
		public HistoryData FullData { get; private set; }

		#endregion public

		#region private

		#region method LoadCurrent

		private void LoadCurrent(SafeReader sr) {
			m_oCurrent[sr["CustomerID"]] = new CustomerStatusChange {
				ChangeDate = sr["SetDate"],
				OldStatus = CustomerStatus.Enabled,
				NewStatus = ((string)sr["Status"]).ParseCustomerStatus(),
			};
		} // LoadCurrent

		#endregion method LoadCurrent

		#region method LoadHistoryItem

		private void LoadHistoryItem(SafeReader sr) {
			int nCustomerID = sr["CustomerID"];

			var csc = new CustomerStatusChange {
				ChangeDate = sr["ChangeDate"],
				OldStatus = ((string)sr["OldStatus"]).ParseCustomerStatus(),
				NewStatus = ((string)sr["NewStatus"]).ParseCustomerStatus(),
			};

			if (m_oDateEnd.HasValue && (csc.ChangeDate < m_oDateEnd.Value))
				Data.Add(nCustomerID, csc);

			FullData.Add(nCustomerID, csc);
		} // LoadHistoryItem

		#endregion method LoadHistoryItem

		private readonly SortedDictionary<int, CustomerStatusChange> m_oCurrent;
		private readonly DateTime? m_oDateEnd;

		#endregion private
	} // class CustomerStatusHistory

	#endregion class CustomerStatusHistory
} // namespace
