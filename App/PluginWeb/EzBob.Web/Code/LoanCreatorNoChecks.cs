namespace EzBob.Web.Code {
    using System;
    using EzBob.Web.Infrastructure;
    using EZBob.DatabaseLib.Model.Database;
    using NHibernate;
    using PaymentServices.PacNet;

    public class LoanCreatorNoChecks : LoanCreator {
		public LoanCreatorNoChecks(
			IPacnetService pacnetService,
			IAgreementsGenerator agreementsGenerator,
			IEzbobWorkplaceContext context,
			LoanBuilder loanBuilder,
			ISession session
		) : base(pacnetService, agreementsGenerator, context, loanBuilder, session) {}

		public override void ValidateCustomer(Customer cus) {
		}

		public override void ValidateLoanDelay(Customer customer, DateTime now, TimeSpan period) {
		}

		public override void ValidateOffer(Customer cus) {
		}

		public override void VerifyAvailableFunds(decimal transfered) {
		}
	} // class LoanCreatorNoChecks
} // namespace
