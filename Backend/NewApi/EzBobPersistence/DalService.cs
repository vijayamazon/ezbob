namespace EzBobPersistence
{
    using EzBobCommon;
    using EzBobPersistence.Customer;
    using EzBobPersistence.Loan;

    public class DalService {

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        [Injected]
        public LoanQueries LoanQueries { get; set; }

        [Injected]
        public ConfigurationQueries ConfigurationQueries { get; set; }

    }
}
