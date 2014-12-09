namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	public enum AddressCurrency {
		Current,
		Previous
	} // enum AddressCurrency

	public class GetCustomerAddresses : AStoredProcedure {
		public GetCustomerAddresses(int nCustomerID, int? nDirectorID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			CustomerID = nCustomerID;
			DirectorID = nDirectorID;
		} // constructor

		public override bool HasValidParameters() {
			return CustomerID > 0;
		} // HasValidParameters

		public int CustomerID { get; set; }
		public int? DirectorID { get; set; }

		public class ResultRow : AResultRow {
			public ResultRow() {
				m_aryCurrent = new string[7];
				m_aryPrev = new string[7];
			} // constructor

			public string Line1 { get { return m_aryCurrent[1]; } set { m_aryCurrent[1] = value; } } // Line1
			public string Line2 { get { return m_aryCurrent[2]; } set { m_aryCurrent[2] = value; } } // Line2
			public string Line3 { get { return m_aryCurrent[3]; } set { m_aryCurrent[3] = value; } } // Line3
			public string Line4 { get { return m_aryCurrent[4]; } set { m_aryCurrent[4] = value; } } // Line4
			public string Line5 { get { return m_aryCurrent[5]; } set { m_aryCurrent[5] = value; } } // Line5
			public string Line6 { get { return m_aryCurrent[6]; } set { m_aryCurrent[6] = value; } } // Line6

			public string Line1Prev { get { return m_aryPrev[1]; } set { m_aryPrev[1] = value; } } // Line1Prev
			public string Line2Prev { get { return m_aryPrev[2]; } set { m_aryPrev[2] = value; } } // Line2Prev
			public string Line3Prev { get { return m_aryPrev[3]; } set { m_aryPrev[3] = value; } } // Line3Prev
			public string Line4Prev { get { return m_aryPrev[4]; } set { m_aryPrev[4] = value; } } // Line4Prev
			public string Line5Prev { get { return m_aryPrev[5]; } set { m_aryPrev[5] = value; } } // Line5Prev
			public string Line6Prev { get { return m_aryPrev[6]; } set { m_aryPrev[6] = value; } } // Line6Prev

			public string this[int nIdx, AddressCurrency oCurrency] {
				get {
					if ((nIdx < 1) || (nIdx > 6))
						throw new ArgumentOutOfRangeException("nIdx", "Unsupported value: " + nIdx.ToString());

					switch (oCurrency) {
					case AddressCurrency.Current:
							return m_aryCurrent[nIdx];

					case AddressCurrency.Previous:
							return m_aryPrev[nIdx];

					default:
						throw new ArgumentOutOfRangeException("oCurrency", "Unsupported value: " + oCurrency.ToString());
					} // switch
				} // get
			} // indexer

			public override string ToString() {
				return string.Format(
					"Line1='{0}' Line2='{1}' Line3='{2}' Line4='{3}' Line5='{4}' Line6='{5}' " +
					"PrevLine1='{6}' PrevLine2='{7}' PrevLine3='{8}' PrevLine4='{9}' PrevLine5='{10}' PrevLine6='{11}'",
					Line1, Line2, Line3, Line4, Line5, Line6,
					Line1Prev, Line2Prev, Line3Prev, Line4Prev, Line5Prev, Line6Prev
				);
			} // ToString

			private readonly string[] m_aryCurrent;
			private readonly string[] m_aryPrev;
		} // class ResultRow
	} // class GetCustomerAddresses
} // namespace
