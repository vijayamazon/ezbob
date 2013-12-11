using System;
using System.Data;

namespace Reports {
	#region class AddressKey

	class AddressKey : IComparable<AddressKey> {
		#region public

		#region constructor

		public AddressKey(IDataRecord oRow) {
			CustomerID = Convert.ToInt32(oRow["CustomerID"]);
			AddressTypeID = Convert.ToInt32(oRow["AddressTypeID"]);
		} // constructor

		#endregion constructor

		#region Implementation of IComparable<in AddressKey>

		public int CompareTo(AddressKey other) {
			if (ReferenceEquals(other, null))
				return 1;

			var nCustomerCompareResult = CustomerID.CompareTo(other.CustomerID);

			return nCustomerCompareResult == 0 ? AddressTypeID.CompareTo(other.AddressTypeID) : nCustomerCompareResult;
		} // CompareTo

		#endregion Implementation of IComparable<in AddressInfo>

		public int CustomerID { get; private set; }
		public int AddressTypeID { get; private set; }

		#region method ToString

		public override string ToString() {
			return string.Format("{0}, {1}", CustomerID, AddressTypeID);
		} // ToString

		#endregion method ToString

		#endregion public
	} // class AddressInfo

	#endregion class AddressKey
} // namespace Reports
