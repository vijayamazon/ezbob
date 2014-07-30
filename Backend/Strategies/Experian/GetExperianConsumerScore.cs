namespace EzBob.Backend.Strategies.Experian {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetExperianConsumerScore : AStrategy {
		#region public

		#region constructor

		public GetExperianConsumerScore(int customerId, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			Score = 0;
			m_nCustomerId = customerId;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "GetExperianConsumerScore"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Score = DB.ExecuteScalar<int>("GetExperianConsumerScore",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", m_nCustomerId)
			);
			
		} // Execute

		#endregion method Execute

		#region property Result

		public int Score { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		private readonly int m_nCustomerId;
		
		#endregion private
	} // class LoadExperianConsumerData
} // namespace
