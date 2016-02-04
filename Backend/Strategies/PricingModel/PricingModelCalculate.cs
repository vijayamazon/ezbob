namespace Ezbob.Backend.Strategies.PricingModel {
	using Exceptions;
	using Ezbob.Database;

	public class PricingModelCalculate : AStrategy {
		public PricingModelCalculate(int customerID, PricingModelModel model) {
			Model = model;
			this.customerID = customerID;
			this.pricingModelCalculator = new PricingModelCalculator(customerID, model);
		} // constructor

		public override string Name { get { return "Pricing model calculate"; } }

		public PricingModelModel Model { get; private set; }

		public override void Execute() {
			this.pricingModelCalculator.Execute();

			if (!string.IsNullOrEmpty(this.pricingModelCalculator.Error))
				throw new StrategyWarning(this, this.pricingModelCalculator.Error);

			Model = this.pricingModelCalculator.Model;

			SetCustomerOriginID();
		} // Execute

		private void SetCustomerOriginID() {
			Model.OriginID = DB.ExecuteScalar<int>(
				"GetCustomerOrigin",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);
		} // SetCustomerOriginID

		private readonly int customerID;
		private readonly PricingModelCalculator pricingModelCalculator;
	} // class PricingModelCalculate
} // namespace
