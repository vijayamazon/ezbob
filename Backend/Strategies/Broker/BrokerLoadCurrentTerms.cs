﻿namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;

	public class BrokerLoadCurrentTerms : AStrategy {
		public BrokerLoadCurrentTerms() {
			ID = 0;
			Terms = "";
		} // constructor

		public override string Name {
			get { return "Broker load current terms"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					ID = sr["BrokerTermsID"];
					Terms = sr["BrokerTerms"];
					return ActionResult.SkipAll;
				},
				"BrokerLoadCurrentTerms",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		public string Terms { get; private set; } // Terms

		public int ID { get; private set; } // ID
	} // class BrokerLoadCurrentTerms
} // namespace Ezbob.Backend.Strategies.Broker
