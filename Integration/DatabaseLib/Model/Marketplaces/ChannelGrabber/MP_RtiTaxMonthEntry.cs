using System;

namespace EZBob.DatabaseLib.Model.Database {

	public class MP_RtiTaxMonthEntry {

		public virtual int Id { get; set; }
		public virtual MP_RtiTaxMonthRecord Record { get; set; }
		public virtual DateTime DateStart { get; set; }
		public virtual DateTime DateEnd { get; set; }
		public virtual decimal AmountPaid { get; set; }
		public virtual decimal AmountDue { get; set; }
		public virtual string CurrencyCode { get; set; }

	} // class MP_RtiTaxMonthEntry

} // namespace
