namespace EzBob.Backend.Strategies.Broker {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerLoadSmsCount : AStrategy {
		#region constructor

		public BrokerLoadSmsCount(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			MaxPerNumber = 0;
			MaxPerPage = 0;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Broker load SMS count"; }
		} // Name

		#endregion property Name

		#region property MaxPerNumber

		public int MaxPerNumber { get; private set; } // MaxPerNumber

		#endregion property MaxPerNumber

		#region property MaxPerPage

		public int MaxPerPage { get; private set; } // MaxPerPage

		#endregion property MaxPerPage

		#region method Execute

		public override void Execute() {
			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nValue = sr["Value"];

					switch ((string)sr["Name"]) {
					case "BrokerMaxPerNumber":
						MaxPerNumber = nValue;
						break;

					case "BrokerMaxPerPage":
						MaxPerPage = nValue;
						break;
					} // switch

					return ActionResult.Continue;
				},
				"BrokerLoadSmsCount",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		#endregion method Execute
	} // class BrokerLoadSmsCount
} // namespace EzBob.Backend.Strategies.Broker
