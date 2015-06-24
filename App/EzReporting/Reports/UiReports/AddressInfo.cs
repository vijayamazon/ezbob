namespace Reports.UiReports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using MainAppReferences;

	public class AddressInfo {

		public AddressInfo(int nCustomerID, IDataRecord oRow) {
			CustomerID = nCustomerID;
			m_oAddressCount = new SortedDictionary<CustomerAddressType, int>();
			Add(oRow);
		} // constructor

		public int CustomerID { get; private set; }

		public int this[CustomerAddressType nType] { get {
			if (m_oAddressCount.ContainsKey(nType))
				return m_oAddressCount[nType];

			return 0;
		} } // indexer

		public void Add(IDataRecord oRow) {
			int nAddressType = Convert.ToInt32(oRow["AddressTypeID"]);

			if (Enum.IsDefined(typeof(CustomerAddressType), nAddressType)) {
				CustomerAddressType at = (CustomerAddressType)Enum.ToObject(typeof (CustomerAddressType), nAddressType);

				m_oAddressCount[at] = Convert.ToInt32(oRow["AddressCount"]);
			} // if
		} // Add

		public override string ToString() {
			List<string> oOut = m_oAddressCount.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)).ToList();
			return string.Format("{0}: [ {1} ]", CustomerID, string.Join(", ", oOut));
		} // ToString

		private readonly SortedDictionary<CustomerAddressType, int> m_oAddressCount;
	} // class AddressInfo
} // namespace
