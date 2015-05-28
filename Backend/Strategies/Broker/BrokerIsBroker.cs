namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;

	public class BrokerIsBroker : AStrategy {
		public BrokerIsBroker(string sContactEmail, int uiOrigin) {
			this.contactEmail = (sContactEmail ?? string.Empty).Trim();
			this.uiOrigin = uiOrigin;
			IsBroker = false;
		} // constructor

		public virtual bool IsBroker { get; private set; }

		public override string Name {
			get { return "Broker: check is broker"; }
		} // Name

		public override void Execute() {
			if (string.IsNullOrWhiteSpace(this.contactEmail)) {
				IsBroker = false;
				return;
			} // if

			IsBroker = 0 != DB.ExecuteScalar<int>(
				"BrokerIsBroker",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@ContactEmail", this.contactEmail),
				new QueryParameter("@UiOrigin", this.uiOrigin)
			);
		} // Execute

		private readonly string contactEmail;
		private readonly int uiOrigin;
	} // class BrokerIsBroker
} // namespace Ezbob.Backend.Strategies.Broker
