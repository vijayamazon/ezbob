namespace EchoSignLib {
	using System;

	internal enum PersonType {
		Other,
		Customer,
		Director,
		ExperianDirector,
	} // enum PersonType

	internal abstract class AAddressable : Address {
		public int ID { get; set; }

		public string RowType {
			get { return m_sRowType; }

			set {
				m_sRowType = value;

				PersonType pt;
				PersonType = Enum.TryParse(m_sRowType, true, out pt) ? pt : PersonType.Other;
			} // set
		} // RowType

		private string m_sRowType;

		public PersonType PersonType { get; private set; } // PersonType

		protected AAddressable() {
			PersonType = PersonType.Other;
		} // constructor
	} // class AAddressable
} // namespace
