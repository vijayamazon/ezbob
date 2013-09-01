using System;

namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_RtiTaxMonthEntry

	public class MP_RtiTaxMonthEntry {
		#region public

		public virtual int Id { get; set; }
		public virtual MP_RtiTaxMonthRecord Record { get; set; }
		public virtual DateTime DateStart { get; set; }
		public virtual DateTime DateEnd { get; set; }
		public virtual decimal AmountPaid { get; set; }
		public virtual decimal AmountDue { get; set; }
		public virtual string CurrencyCode { get; set; }

		#endregion public
	} // class MP_RtiTaxMonthEntry

	#endregion class MP_RtiTaxMonthEntry
} // namespace
