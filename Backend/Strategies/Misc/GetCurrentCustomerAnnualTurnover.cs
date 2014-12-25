namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Database;

	public class GetCurrentCustomerAnnualTurnover : AStrategy {
		public override string Name {
			get {
				return "GetCurrentCustomerAnnualTurnover";
			}
		} // Name

		public decimal Turnover { get; private set; }

		public GetCurrentCustomerAnnualTurnover(int customerID) {
			this.customerID = customerID;
			Turnover = 0;
		} // constructor

		public override void Execute() {
			Turnover = DB.ExecuteScalar<decimal>(
				"GetCustomerAnnualTurnover",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerID)
				);
		} // Execute

		private readonly int customerID;
	} // class GetCurrentCustomerAnnualTurnover
} // namespace
