namespace EzBob.Web.Code {
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Loans;
	using EzBob.Web.Code.Agreements;
	using EzBob.Web.Infrastructure;
	using NHibernate;
	using PaymentServices.PacNet;

	public class LoanCreatorNoChecks : LoanCreator {
		public LoanCreatorNoChecks(
			ILoanHistoryRepository loanHistoryRepository,
			IPacnetService pacnetService,
			IAgreementsGenerator agreementsGenerator,
			IEzbobWorkplaceContext context,
			LoanBuilder loanBuilder,
			AvailableFundsValidator availableFundsValidator,
			ISession session
		) : base(loanHistoryRepository, pacnetService, agreementsGenerator, context, loanBuilder, availableFundsValidator, session) {}

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
