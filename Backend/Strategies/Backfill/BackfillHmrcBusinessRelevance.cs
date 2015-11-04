namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Strategies.VatReturn;
	using Ezbob.Database;

	public class BackfillHmrcBusinessRelevance : AStrategy {
		public override string Name {
			get { return "BackfillHmrcBusinessRelevance"; }
		} // Name

		public override void Execute() {
			DB.ExecuteNonQuery("UPDATE Business SET BelongsToCustomer = NULL", CommandSpecies.Text);

			DB.ForEachRowSafe(
				sr => { new UpdateHmrcBusinessRelevance(sr["Id"]).Execute(); },
				"SELECT Id FROM Customer ORDER BY Id",
				CommandSpecies.Text
			);
		} // Execute
	} // class BackfillHmrcBusinessRelevance
} // namespace
