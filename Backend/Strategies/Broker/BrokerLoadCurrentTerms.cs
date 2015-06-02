namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;

	public class BrokerLoadCurrentTerms : AStrategy {
		public BrokerLoadCurrentTerms(int originID) {
			ID = 0;
			Terms = "";
			this.originID = originID;
		} // constructor

		public override string Name {
			get { return "Broker load current terms"; }
		} // Name

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"BrokerLoadCurrentTerms",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@OriginID", this.originID)
			);

			if (!sr.IsEmpty) {
				ID = sr["BrokerTermsID"];
				Terms = sr["BrokerTerms"];
			} // if
		} // Execute

		public string Terms { get; private set; } // Terms

		public int ID { get; private set; } // ID

		private readonly int originID;
	} // class BrokerLoadCurrentTerms
} // namespace Ezbob.Backend.Strategies.Broker
