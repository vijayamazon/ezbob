namespace EzBobPersistence.Loan
{
    using EzBobModels;
    using EzBobModels.Loan;
    using EzBobModels.Loan.Enums;

    public interface ILoanQueries {
        /// <summary>
        /// Gets the loan source.
        /// </summary>
        /// <param name="loanKind">Kind of the loan.</param>
        /// <returns></returns>
        LoanSource GetLoanSource(LoanKind loanKind);

        /// <summary>
        /// Saves the customer requested loan.
        /// </summary>
        /// <param name="requestedLoan">The requested loan.</param>
        /// <returns>true - success, false - failure, null - was nothing to save in db</returns>
        bool? SaveCustomerRequestedLoan(CustomerRequestedLoan requestedLoan);
    }
}
