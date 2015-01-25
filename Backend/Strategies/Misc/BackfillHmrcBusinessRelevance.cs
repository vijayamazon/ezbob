namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Strategies.VatReturn;
	using Ezbob.Database;

	public class BackfillHmrcBusinessRelevance : AStrategy {
		public override string Name {
			get { return "BackfillHmrcBusinessRelevance"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe(
				sr => { new UpdateHmrcBusinessRelevance(sr["Id"]).Execute(); },
				"SELECT Id FROM Customer",
				CommandSpecies.Text
			);
		} // Execute
	} // class BackfillHmrcBusinessRelevance
} // namespace
