namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadCurrentTerms : AStrategy {
		#region public

		#region constructor

		public BrokerLoadCurrentTerms(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			ID = 0;
			Terms = "";
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load current terms"; }
		} // Name

		#endregion property Name

		#region method Execute

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

		#endregion method Execute

		#region property Terms

		public string Terms { get; private set; } // Terms

		#endregion property Terms

		#region property ID

		public int ID { get; private set; } // ID

		#endregion property TID

		#endregion public

		#region private

		#endregion private
	} // class BrokerLoadCurrentTerms
} // namespace EzBob.Backend.Strategies.Broker
