namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using Ezbob.Backend.Strategies.MainStrategy.Exceptions;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;

	internal class ValidateInput : AOneExitStep {
		public ValidateInput(
			string outerContextDescription,
			CustomerDetails customerDetails,
			long cashRequestID,
			CashRequestOriginator? cashRequestOriginator
		) : base(outerContextDescription) {
			this.customerDetails = customerDetails;
			this.cashRequestID = cashRequestID;
			this.cashRequestOriginator = cashRequestOriginator;
		} // constructor

		protected override void ExecuteStep() {
			this.customerDetails.Load();

			if (!this.customerDetails.IsValid) {
				throw new InvalidInputException(
					"Customer details were not found for id {0}.",
					this.customerDetails.RequestedID
				);
			} // if

			if (this.cashRequestID.LacksValue()) {
				if (this.cashRequestOriginator == null) { // Should never happen but just in case...
					throw new InvalidInputException(
						"Neither cash request id nor cash request originator specified for customer {0} " +
						"(cash request cannot be created).",
						this.customerDetails.RequestedID
					);
				} // if
			} else {
				bool isMatch = DB.ExecuteScalar<bool>(
					"ValidateCustomerAndCashRequest",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerDetails.RequestedID),
					new QueryParameter("@CashRequestID", this.cashRequestID)
				);

				if (!isMatch) {
					throw new InvalidInputException(
						"Cash request id {0} does not belong to customer {1}.",
						this.cashRequestID,
						this.customerDetails.RequestedID
					);
				} // if
			} // if
		} // ExecuteStep

		private readonly CustomerDetails customerDetails;
		private readonly long cashRequestID;
		private readonly CashRequestOriginator? cashRequestOriginator;
	} // class ValidateInput
} // namespace
