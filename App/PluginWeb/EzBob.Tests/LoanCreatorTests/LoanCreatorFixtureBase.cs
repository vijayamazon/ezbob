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
}