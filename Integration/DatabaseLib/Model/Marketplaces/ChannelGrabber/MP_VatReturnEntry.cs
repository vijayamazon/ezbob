namespace EZBob.DatabaseLib.Model.Database {
	#region class MP_VatReturnEntry

	public class MP_VatReturnEntry {
		#region public

		public virtual int Id { get; set; }
		public virtual MP_VatReturnRecord Record { get; set; }
		public virtual MP_VatReturnEntryName Name { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual string Currency { get; set; }

		#endregion public
	} // class MP_VatReturnEntry

	#endregion class MP_VatReturnEntry
} // namespace
