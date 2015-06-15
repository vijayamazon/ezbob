namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Ezbob.Database;

	public enum CustomerStatus {
		Enabled,
		Default,
		WriteOff,
	} // enum CustomerStatus

	internal static class StringExt {
		public static CustomerStatus ParseCustomerStatus(this string status, bool isDefault) {
			if (string.Compare(status, CustomerStatus.WriteOff.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
				return CustomerStatus.WriteOff;

			return isDefault ? CustomerStatus.Default : CustomerStatus.Enabled;
		} // ParseCustomerStatus
	} // class StringExt

	public class CustomerStatusChange {
		public CustomerStatus OldStatus { get; set; }
		public CustomerStatus NewStatus { get; set; }
		public DateTime ChangeDate { get; set; }


		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(CustomerStatusChange);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t");
			}
			return sb.ToString();
		}
	} // class CustomerStatusChange

	public class CustomerStatusHistory {
		public class HistoryData {
			public HistoryData(CustomerStatusHistory oFullHistory) {
				Data = new SortedDictionary<int, List<CustomerStatusChange>>();
				WriteOffDate = new SortedDictionary<int, DateTime>();

				m_oFullHistory = oFullHistory;
			} // constructor

			public CustomerStatusChange GetLast(int nCustomerID) {
				return Data.ContainsKey(nCustomerID) ? Data[nCustomerID].Last() : m_oFullHistory.GetCurrent(nCustomerID);
			} // GetLast

			public DateTime? GetWriteOffDate(int nCustomerID) {
				return WriteOffDate.ContainsKey(nCustomerID) ? WriteOffDate[nCustomerID] : (DateTime?)null;
			} // GetWriteOffDate

			internal void Add(int nCustomerID, CustomerStatusChange csc) {
				if (Data.ContainsKey(nCustomerID))
					Data[nCustomerID].Add(csc);
				else
					Data[nCustomerID] = new List<CustomerStatusChange> { csc };

				if ((csc.NewStatus == CustomerStatus.WriteOff) && !WriteOffDate.ContainsKey(nCustomerID))
					WriteOffDate[nCustomerID] = csc.ChangeDate;
			} // Add

			public SortedDictionary<int, List<CustomerStatusChange>> Data { get; set; }

			private SortedDictionary<int, DateTime> WriteOffDate { get; set;}
			private readonly CustomerStatusHistory m_oFullHistory;

		} // class HistoryData

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

		public CustomerStatusChange GetCurrent(int nCustomerID) {
			return m_oCurrent[nCustomerID];
		} // GetLast

		public HistoryData Data { get; private set; }
		public HistoryData FullData { get; private set; }

		private void LoadCurrent(SafeReader sr) {
			m_oCurrent[sr["CustomerID"]] = new CustomerStatusChange {
				ChangeDate = sr["SetDate"],
				OldStatus = CustomerStatus.Enabled,
				NewStatus = ((string)sr["Status"]).ParseCustomerStatus(sr["IsDefault"]),
			};
		} // LoadCurrent

		private void LoadHistoryItem(SafeReader sr) {
			int nCustomerID = sr["CustomerID"];

			var csc = new CustomerStatusChange {
				ChangeDate = sr["ChangeDate"],
				OldStatus = ((string)sr["OldStatus"]).ParseCustomerStatus(sr["OldIsDefault"]),
				NewStatus = ((string)sr["NewStatus"]).ParseCustomerStatus(sr["NewIsDefault"]),
			};

			if (m_oDateEnd.HasValue && (csc.ChangeDate < m_oDateEnd.Value))
				Data.Add(nCustomerID, csc);

			FullData.Add(nCustomerID, csc);
		} // LoadHistoryItem

		private readonly SortedDictionary<int, CustomerStatusChange> m_oCurrent;
		private readonly DateTime? m_oDateEnd;
	} // class CustomerStatusHistory
} // namespace
