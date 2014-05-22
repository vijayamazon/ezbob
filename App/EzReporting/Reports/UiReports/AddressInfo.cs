using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Reports {
	using MainAppReferences;

	#region class AddressInfo

	public class AddressInfo {
		#region public

		#region constructor

		public AddressInfo(int nCustomerID, IDataRecord oRow) {
			CustomerID = nCustomerID;
			m_oAddressCount = new SortedDictionary<CustomerAddressType, int>();
			Add(oRow);
		} // constructor

		#endregion constructor

		public int CustomerID { get; private set; }

		#region indexer

		public int this[CustomerAddressType nType] { get {
			if (m_oAddressCount.ContainsKey(nType))
				return m_oAddressCount[nType];

			return 0;
		} } // indexer

		#endregion indexer

		#region method Add

		public void Add(IDataRecord oRow) {
			int nAddressType = Convert.ToInt32(oRow["AddressTypeID"]);

			if (Enum.IsDefined(typeof(CustomerAddressType), nAddressType)) {
				CustomerAddressType at = (CustomerAddressType)Enum.ToObject(typeof (CustomerAddressType), nAddressType);

				m_oAddressCount[at] = Convert.ToInt32(oRow["AddressCount"]);
			} // if
		} // Add

		#endregion method Add

		#region method ToString

		public override string ToString() {
			List<string> oOut = m_oAddressCount.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value)).ToList();
			return string.Format("{0}: [ {1} ]", CustomerID, string.Join(", ", oOut));
		} // ToString

		#endregion method ToString

		#endregion public

		#region private

		private readonly SortedDictionary<CustomerAddressType, int> m_oAddressCount;

		#endregion private
	} // class AddressInfo

	#endregion class AddressInfo
} // namespace Reports
