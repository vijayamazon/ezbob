using System;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {
	public class RtiTaxMonthEntry : AInternalOrderItem {
		#region public

		#region method CompareForSort

		public static int CompareForSort(RtiTaxMonthEntry a, RtiTaxMonthEntry b) {
			return string.Compare(a.NativeOrderId, b.NativeOrderId, StringComparison.Ordinal);
		} // CompareForSort

		#endregion method CompareForSort

		#region property NativeOrderId

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

		#endregion property NativeOrderId

		public virtual DateTime DateStart { get; set; }
		public virtual DateTime DateEnd { get; set; }

		public virtual Coin AmountPaid { get; set; }
		public virtual Coin AmountDue { get; set; }

		public virtual DateTime FetchTime { get; set; }

		public override DateTime RecordTime { get { return DateStart; }} // RecordTime

		#endregion public
	} // class ChannelGrabberOrderItem
} // namespace
