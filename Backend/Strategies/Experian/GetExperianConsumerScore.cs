namespace Ezbob.Backend.Strategies.Experian {
	using Ezbob.Database;

	public class GetExperianConsumerScore : AStrategy {
		public GetExperianConsumerScore(int customerID) {
			Score = 0;
			this.customerID = customerID;
		} // constructor

		public override string Name {
			get { return "GetExperianConsumerScore"; }
		} // Name

		public override void Execute() {
			Score = DB.ExecuteScalar<int>(
				"GetExperianConsumerScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);
		} // Execute

		public int Score { get; private set; }
		private readonly int customerID;
	} // class LoadExperianConsumerData
} // namespace
