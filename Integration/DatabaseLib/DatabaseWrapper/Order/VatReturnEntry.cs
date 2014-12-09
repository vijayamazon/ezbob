using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;

namespace EZBob.DatabaseLib.DatabaseWrapper.Order {

	[Serializable]
	public class VatReturnEntry : AInternalOrderItem {

		public static int CompareForSort(VatReturnEntry a, VatReturnEntry b) {
			return a.DateFrom.CompareTo(b.DateFrom);
		} // CompareForSort

		public VatReturnEntry() {
			Data = new SortedDictionary<string, Coin>();
		} // constructor

		public override string NativeOrderId {
			get {
				if (m_sNativeOrderId == null) {
					m_sNativeOrderId = string.Format(
						"{0}-{1}-{2}-{3}-{4}",
						RegistrationNo,
						DateFrom.Ticks,
						DateTo.Ticks,
						Period,
						BusinessName
						);
				} // if

				return m_sNativeOrderId;
			} // get
			set {
				// nothing here
			} // set
		} // NativeOrderId

		private string m_sNativeOrderId;

		public virtual DateTime DateFrom { get; set; }
		public virtual DateTime DateTo { get; set; }
		public virtual DateTime DateDue { get; set; }
		public virtual string Period { get; set; }

		public virtual long RegistrationNo { get; set; }
		public virtual string BusinessName { get; set; }
		public virtual string[] BusinessAddress { get; set; }

		public virtual SortedDictionary<string, Coin> Data { get; private set; }

		public override DateTime RecordTime { get { return DateTo; }} // RecordTime

	} // class ChannelGrabberOrderItem
} // namespace
