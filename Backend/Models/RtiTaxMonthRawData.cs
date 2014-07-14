// ReSharper disable ValueParameterNotUsed
namespace Ezbob.Backend.Models {
	using System;
	using System.Globalization;
	using System.Runtime.Serialization;
	using Utils;

	[DataContract]
	public class RtiTaxMonthRawData : IComparable<RtiTaxMonthRawData> {
		#region constructor

		public RtiTaxMonthRawData() {
			AmountPaid = new Coin();
			AmountDue = new Coin();
		} // constructor

		#endregion constructor

		#region method CompareTo

		public int CompareTo(RtiTaxMonthRawData a) {
			return a.DateCode.CompareTo(a.DateCode);
		} // CompareTo

		#endregion method CompareTo

		[DataMember]
		[NonTraversable]
		public Coin AmountPaid { get; set; }

		[DataMember]
		[NonTraversable]
		public Coin AmountDue { get; set; }

		#region property DateCode

		[NonTraversable]
		public int DateCode {
			get {
				if (m_nDateCode == null)
					m_nDateCode = GetDateCode();

				return m_nDateCode.Value;
			} // get
			set { m_nDateCode = value; }
		} // DateCode

		private int? m_nDateCode;

		#endregion property DateCode

		#region DB properties

		#region property DateStart

		[DataMember]
		public DateTime DateStart {
			get { return m_oDateStart; }
			set {
				m_oDateStart = value;
				DateCode = GetDateCode();
			}
		} // DateStart

		private DateTime m_oDateStart;

		#endregion property DateStart

		[DataMember]
		public DateTime DateEnd { get; set; }

		public decimal PaidAmount {
			get { return AmountPaid.Amount; }
			set { AmountPaid.Amount = value; }
		} // PaidAmount

		public string PaidCurrencyCode {
			get { return AmountPaid.CurrencyCode; }
			set { AmountPaid.CurrencyCode = value; }
		} // PaidCurrencyCode

		public decimal DueAmount {
			get { return AmountDue.Amount; }
			set { AmountDue.Amount = value; }
		} // DueAmount

		public string DueCurrencyCode {
			get { return AmountDue.CurrencyCode; }
			set { AmountDue.CurrencyCode = value; }
		} // DueCurrencyCode

		#endregion DB properties

		#region method ToString

		public override string ToString() {
			return string.Format("{0} - {1}: {2} {3}, {4} {5}",
				DateStart.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				DateEnd.ToString("MMM d yyyy", CultureInfo.InvariantCulture),
				PaidAmount, PaidCurrencyCode,
				DueAmount, DueCurrencyCode
			);
		} // ToString

		#endregion method ToString

		#region method GetDateCode

		private int GetDateCode() {
			return DateStart.Year * 100 + DateStart.Month;
		} // GetDateCode

		#endregion method GetDateCode
	} // class ChannelGrabberOrderItem
} // namespace
// ReSharper restore ValueParameterNotUsed
