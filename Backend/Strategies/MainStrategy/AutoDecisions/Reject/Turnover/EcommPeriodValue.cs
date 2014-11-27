namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions.Reject.Turnover {
	using System;

	internal class EcommPeriodValue : IPeriodValue {
		#region public

		#region constructor

		public EcommPeriodValue() {
			m_nEbay = 0;
			m_nOther = 0;
			m_nPayPal = 0;
		} // constructor

		#endregion constructor

		#region method Add

		public void Add(Row r) {
			if (r.MpTypeInternalID == Ebay)
				m_nEbay += r.Turnover;
			else if (r.MpTypeInternalID == PayPal)
				m_nPayPal += r.Turnover;
			else if (!r.IsPaymentAccount)
				m_nOther += r.Turnover;
		} // Add

		#endregion method Add

		#region property Value

		public decimal Value {
			get { return Math.Max(m_nEbay, m_nPayPal) + m_nOther; } // get
		} // Value

		#endregion property Value

		#endregion public

		#region private

		private decimal m_nEbay;
		private decimal m_nPayPal;
		private decimal m_nOther;

		private static readonly Guid Ebay   = new Guid("A7120CB7-4C93-459B-9901-0E95E7281B59");
		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");

		#endregion private
	} // EcommPeriodValue
} // namespace
