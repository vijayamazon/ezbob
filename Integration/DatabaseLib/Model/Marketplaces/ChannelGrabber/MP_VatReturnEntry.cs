namespace EZBob.DatabaseLib.Model.Database {

	public class MP_VatReturnEntry {

		public virtual int Id { get; set; }
		public virtual MP_VatReturnRecord Record { get; set; }
		public virtual MP_VatReturnEntryName Name { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual string CurrencyCode { get; set; }

	} // class MP_VatReturnEntry

} // namespace
