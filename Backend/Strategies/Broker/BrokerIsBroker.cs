namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;

	public class BrokerIsBroker : AStrategy {
		public BrokerIsBroker(string sContactEmail) {
			m_sContactEmail = (sContactEmail ?? string.Empty).Trim();
			IsBroker = false;
		} // constructor

		public virtual bool IsBroker { get; private set; }

		public override string Name {
			get { return "Broker: check is broker"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(m_sContactEmail)) {
				IsBroker = false;
				return;
			} // if

			IsBroker = 0 != DB.ExecuteScalar<int>(
				"BrokerIsBroker",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@ContactEmail", m_sContactEmail)
			);
		} // Execute

		private readonly string m_sContactEmail;
	} // class BrokerIsBroker
} // namespace Ezbob.Backend.Strategies.Broker
