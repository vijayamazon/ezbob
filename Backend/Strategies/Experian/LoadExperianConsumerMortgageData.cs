namespace Ezbob.Backend.Strategies.Experian {
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;

	public class LoadExperianConsumerMortgageData : AStrategy {
		public LoadExperianConsumerMortgageData(int customerId) {
			Result = new ExperianConsumerMortgagesData();
			m_nCustomerId = customerId;
		}

		public override void Execute() {
			Result = DB.FillFirst<ExperianConsumerMortgagesData>(
					"LoadExperianConsumerMortgagesData",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", m_nCustomerId)
				);
		}

		public override string Name {
			get { return "LoadExperianConsumerMortgageData"; }
		}

		public ExperianConsumerMortgagesData Result { get; private set; }
		private readonly int m_nCustomerId;
	}
}
