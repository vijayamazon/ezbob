namespace EzBobPersistence.Loan
{
    using EzBobCommon;
    using EzBobModels.Customer;
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
        /// Upserts the customer requested loan.
        /// </summary>
        /// <param name="requestedLoan">The requested loan.</param>
        /// <returns>true - success, false - failure, null - was nothing to save in db</returns>
        Optional<int> UpsertCustomerRequestedLoan(CustomerRequestedLoan requestedLoan);

        /// <summary>
        /// Gets the customer's requested loan.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        Optional<CustomerRequestedLoan> GetCustomerRequestedLoan(int customerId);
    }
}
