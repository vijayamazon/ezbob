namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerIsBroker : AStrategy {

		public BrokerIsBroker(string sContactEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
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

} // namespace EzBob.Backend.Strategies.Broker
