namespace Ezbob.Backend.Strategies.Broker {
	using Ezbob.Database;

	public class BrokerLoadSmsCount : AStrategy {

		public BrokerLoadSmsCount() {
			MaxPerNumber = 0;
			MaxPerPage = 0;
		} // constructor

		public override string Name {
			get { return "Broker load SMS count"; }
		} // Name

		public int MaxPerNumber { get; private set; } // MaxPerNumber

		public int MaxPerPage { get; private set; } // MaxPerPage

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

	} // class BrokerLoadSmsCount
} // namespace Ezbob.Backend.Strategies.Broker
