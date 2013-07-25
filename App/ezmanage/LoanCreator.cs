using System;
using ApplicationMng.Model;
using ApplicationMng.Repository;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using PaymentServices.PacNet;
using ZohoCRM;

namespace ezmanage
{
    public class LoanCreatorNoChecks : LoanCreator
    {
        private readonly EzContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUsersRepository _users;

        public LoanCreatorNoChecks(ILoanHistoryRepository loanHistoryRepository, IPacnetService pacnetService,
                                   IAppCreator appCreator, IZohoFacade crm, IAgreementsGenerator agreementsGenerator,
                                   EzContext context, ICustomerRepository customerRepository, IUsersRepository users,
                                   LoanBuilder loanBuilder, AvailableFundsValidator validator)
            : base(loanHistoryRepository, pacnetService, appCreator, crm, agreementsGenerator, context, loanBuilder, validator)
        {
            _context = context;
            _customerRepository = customerRepository;
            _users = users;
        }

        public void CreateLoan(int id, decimal amount, DateTime date)
        {
            _customerRepository.EnsureTransaction(() =>
                                                      {
                                                          Customer customer = _customerRepository.Get(id);
                                                          User user = _users.Get(id);

                                                          _context.Customer = customer;
                                                          _context.User = user;
                                                          CreateLoan(customer, amount, null, date);
                                                      });
        }

        public override void ValidateCustomer(Customer cus)
        {
        }

        public override void ValidateLoanDelay(Customer customer, DateTime now, TimeSpan period)
        {
        }

        public override void ValidateOffer(Customer cus)
        {
        }
    }
}