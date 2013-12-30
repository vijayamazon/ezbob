namespace EzBob.Tests.LoanCreatorTests
{
	using EZBob.DatabaseLib.Model.Loans;
	using Models;
	using Web.Code;
	using Web.Code.Agreements;
	using Web.Code.ApplicationCreator;
	using Web.Infrastructure;
	using Moq;
	using NUnit.Framework;
	using PaymentServices.PacNet;

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
            var agreementsGenerator = new Mock<IAgreementsGenerator>();
            var context = new Mock<IEzbobWorkplaceContext>();
            _loanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
            _loanBuilder = new LoanBuilder(_loanDetailsModelBuilder);

            _lc = new LoanCreator(loanHistoryRepository.Object, pacnetService.Object, appCreator.Object, agreementsGenerator.Object, context.Object, _loanBuilder, new AvailableFundsValidatorFake(), null);
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