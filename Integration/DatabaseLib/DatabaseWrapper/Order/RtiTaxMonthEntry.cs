namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	using System;
	using EZBob.DatabaseLib.Common;

	[Serializable]
	public class RtiTaxMonthEntry : AInternalOrderItem {

		public static int CompareForSort(RtiTaxMonthEntry a, RtiTaxMonthEntry b) {
			return string.Compare(a.NativeOrderId, b.NativeOrderId, StringComparison.Ordinal);
		} // CompareForSort

		public override string NativeOrderId {
			get {
				if (m_sNativeOrderId == null)
					m_sNativeOrderId = string.Format("{0}", DateStart.Year * 100 + DateStart.Month);

				return m_sNativeOrderId;
			} // get
			set {
				// nothing here
			} // set
		} // NativeOrderId

		private string m_sNativeOrderId;

		public virtual DateTime DateStart { get; set; }
		public virtual DateTime DateEnd { get; set; }

		public virtual Coin AmountPaid { get; set; }
		public virtual Coin AmountDue { get; set; }

		public virtual DateTime FetchTime { get; set; }

		public override DateTime RecordTime { get { return DateStart; }} // RecordTime

	} // class ChannelGrabberOrderItem
} // namespace
