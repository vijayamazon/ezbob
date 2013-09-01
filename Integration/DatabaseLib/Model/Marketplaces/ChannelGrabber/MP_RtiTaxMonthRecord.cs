using System;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_RtiTaxMonthRecord

	public class MP_RtiTaxMonthRecord {
		#region public

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_RtiTaxMonthEntry> Entries { get; set; }

		public MP_RtiTaxMonthRecord() {
			Entries = new Iesi.Collections.Generic.HashedSet<MP_RtiTaxMonthEntry>();
		} // constructor

		#endregion public
	} // class MP_RtiTaxMonthRecord

	#endregion class MP_RtiTaxMonthRecord
} // namespace
