namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class BrokerLoadLeadDetails : AStrategy {

		public BrokerLoadLeadDetails(int leadID, string sContactEmail) {
			this.leadID = leadID;
			this.contactEmail = sContactEmail;
			Result = new BrokerLeadDataModel();
		} // constructor

		public override string Name {
			get { return "Broker load customer details"; }
		} // Name

		public override void Execute() {
            Result = DB.FillFirst<BrokerLeadDataModel>(
				"BrokerLoadLeadDetails",
				new QueryParameter("@LeadID", this.leadID),
				new QueryParameter("@ContactEmail", this.contactEmail)
			);

            if (Result == null) {
				Log.Warn("{0}: personal details not found for lead {1} (broker {2}).", Name, this.leadID, this.contactEmail);
			} // if
		} // Execute

        public BrokerLeadDataModel Result { get; private set; } // Result
        
		private readonly string contactEmail;
	    private readonly int leadID;
	} // class BrokerLoadLeadDetails

} // namespace Ezbob.Backend.Strategies.Broker
