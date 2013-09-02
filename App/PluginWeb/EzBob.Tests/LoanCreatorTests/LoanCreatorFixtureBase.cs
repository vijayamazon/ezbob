using System;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Controllers;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using Moq;
using NHibernate;
using NUnit.Framework;
using PaymentServices.PacNet;
using ZohoCRM;

namespace EzBob.Tests.LoanCreatorTests
{
    [TestFixture]
    public class LoanCreatorFixtureBase
    {
        protected LoanCreator _lc;
        protected LoanBuilder _loanBuilder;
        protected ChangeLoanDetailsModelBuilder _loanDetailsModelBuilder;

        [SetUp]
        public void Init()
		{
			var loanHistoryRepository = new Mock<ILoanHistoryRepository>();

            var pacnetService = new Mock<IPacnetService>();
            pacnetService.Setup(x => x.SendMoney(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
             .Returns(new PacnetReturnData());

            var appCreator = new Mock<IAppCreator>();
            var crm = new Mock<IZohoFacade>();
            var agreementsGenerator = new Mock<IAgreementsGenerator>();
            var context = new Mock<IEzbobWorkplaceContext>();
            _loanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
            _loanBuilder = new LoanBuilder(_loanDetailsModelBuilder);

            _lc = new LoanCreator(loanHistoryRepository.Object, pacnetService.Object, appCreator.Object, crm.Object, agreementsGenerator.Object, context.Object, _loanBuilder, new AvailableFundsValidatorFake());
            SetUp();
        }

        public virtual void SetUp(){}
    }

    public class AvailableFundsValidatorFake : AvailableFundsValidator
    {
        public AvailableFundsValidatorFake() : base(null, null, null)
        {
        }

        public override void VerifyAvailableFunds(decimal transfered)
        {
        }
    }

    public class CustomerStatusesRepositoryStub : ICustomerStatusesRepository
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public CustomerStatuses Get(object id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CustomerStatuses> GetAll()
        {
            return (new[] {new CustomerStatuses() {Id = 0, Name = "Enabled"}}).AsQueryable();
        }

        public object Save(CustomerStatuses val)
        {
            throw new NotImplementedException();
        }

        public void SaveOrUpdate(CustomerStatuses val)
        {
            throw new NotImplementedException();
        }

        public void Update(CustomerStatuses val)
        {
            throw new NotImplementedException();
        }

        public CustomerStatuses Merge(CustomerStatuses val)
        {
            throw new NotImplementedException();
        }

        public void Delete(CustomerStatuses val)
        {
            throw new NotImplementedException();
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void EnsureTransaction(Action<ITransaction> action)
        {
            throw new NotImplementedException();
        }

        public void EnsureTransaction(Action action)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public CustomerStatuses Load(object id)
        {
            throw new NotImplementedException();
        }

        public bool GetIsEnabled(int id)
        {
            return id == 0;
        }
    }

}