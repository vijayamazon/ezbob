namespace EzBob.Backend.Strategies.Experian {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetExperianConsumerScore : AStrategy {

		public GetExperianConsumerScore(int customerId, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			Score = 0;
			m_nCustomerId = customerId;
		} // constructor

		public override string Name {
			get { return "GetExperianConsumerScore"; }
		} // Name

		public override void Execute() {
			Score = DB.ExecuteScalar<int>("GetExperianConsumerScore",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", m_nCustomerId)
			);

		} // Execute

		public int Score { get; private set; }

		private readonly int m_nCustomerId;

	} // class LoadExperianConsumerData
} // namespace
