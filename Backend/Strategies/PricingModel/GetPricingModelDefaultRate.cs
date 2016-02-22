namespace Ezbob.Backend.Strategies.PricingModel {
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;

	public class GetPricingModelDefaultRate : AStrategy {
		public GetPricingModelDefaultRate(int customerId, PricingModelModel model) {
			this.customerId = customerId;
			this.model = model;
		} // constructor

		public override string Name { get { return "Get pricing model default rate"; } }

		public override void Execute() {
			if (this.model == null) {
				Log.Alert("Cannot load default rates for a NULL model, customer id is {0}.", this.customerId);
				return;
			} // if

			var sr = DB.GetFirst(
				"GetOfferConsumerBusinessDefaultRates",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			if (sr.IsEmpty) {
				Log.Alert("No default rate data returned from DB for customer {0}.", this.customerId);
				return;
			} // if

			sr.Stuff(this.model);

			Log.Msg(
				"Pricing model default rates. Consumer: score {0}, default rate {1}. " +
				"Business: score {2}, default rate {3}. Grade: {4} by score {5} with probability of default {6}",
				this.model.ConsumerScore,
				this.model.ConsumerDefaultRate.ToString("P4"),
				this.model.CompanyScore,
				this.model.CompanyDefaultRate.ToString("P4"),
				this.model.Grade.HasValue ? this.model.Grade.Value.ToString() : "N/A",
				this.model.GradeScore.HasValue ? this.model.GradeScore.Value.ToString("G4") : "N/A",
				this.model.ProbabilityOfDefault.HasValue ? this.model.ProbabilityOfDefault.Value.ToString("P4") : "N/A"
			);
		} // Execute

		private readonly int customerId;
		private readonly PricingModelModel model;
	} // class GetPricingModelDefaultRate
} // namespace
