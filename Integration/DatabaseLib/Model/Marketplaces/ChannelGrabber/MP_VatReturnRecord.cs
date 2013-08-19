using System;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_VatReturnRecord

	public class MP_VatReturnRecord {
		#region public

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }

		public virtual string Period { get; set; }
		public virtual DateTime DateFrom { get; set; }
		public virtual DateTime DateTo { get; set; }
		public virtual DateTime DateDue { get; set; }

		public virtual string RegistrationNo { get; set; }

		public virtual Business Business { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_VatReturnEntry> Entries { get; set; }

		public MP_VatReturnRecord() {
			Entries = new Iesi.Collections.Generic.HashedSet<MP_VatReturnEntry>();
		} // constructor

		#endregion public
	} // class MP_VatReturnRecord

	#endregion class MP_VatReturnRecord
} // namespace
