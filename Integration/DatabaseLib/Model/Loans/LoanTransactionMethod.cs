namespace EZBob.DatabaseLib.Model.Database.Loans {
	public class LoanTransactionMethod {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual int DisplaySort { get; set; }

		public const string Default = "Unknown";

		public static LoanTransactionMethod GetDefault() {
			return new LoanTransactionMethod {
				Id = 0,
				Name = Default,
				DisplaySort = 0,
			};
		} // GetDefault
	} // class LoanTransactionMethod
} // namespace EZBob.DatabaseLib.Model.Database.Loans
