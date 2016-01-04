namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	public class BrokerLoadLeadDetails : AStrategy {
		public BrokerLoadLeadDetails(int leadID, string sContactEmail, CustomerOriginEnum origin) {
			this.leadID = leadID;
			this.contactEmail = sContactEmail;
			this.origin = (int)origin;
			Result = new BrokerLeadDataModel();
		} // constructor

		public override string Name {
			get { return "Broker load customer details"; }
		} // Name

		public override void Execute() {
			Result = DB.FillFirst<BrokerLeadDataModel>(
				"BrokerLoadLeadDetails",
				new QueryParameter("@LeadID", this.leadID),
				new QueryParameter("@ContactEmail", this.contactEmail),
				new QueryParameter("@Origin", this.origin)
			);

			if (Result == null)
				Log.Warn("{0}: personal details not found for lead {1} (broker {2}).", Name, this.leadID, this.contactEmail);
		} // Execute

		public BrokerLeadDataModel Result { get; private set; } // Result

		private readonly string contactEmail;
		private readonly int leadID;
		private readonly int origin;
	} // class BrokerLoadLeadDetails
} // namespace Ezbob.Backend.Strategies.Broker
