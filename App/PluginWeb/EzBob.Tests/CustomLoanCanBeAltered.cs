using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class CustomLoanCanBeAltered
    {
        private Loan _loanOriginal;
        private LoanScheduleCalculator _calculator;
        private ChangeLoanDetailsModelBuilder _builder;
        private CashRequest _cashRequest;

        [SetUp]
        public void SetUp()
        {
            _loanOriginal = new Loan();
            _cashRequest = new CashRequest() {InterestRate = 0.06m, LoanType = new StandardLoanType()};
            _calculator = new LoanScheduleCalculator();
            _calculator.Calculate(1000, _loanOriginal, new DateTime(2012, 10, 21));

            _builder = new ChangeLoanDetailsModelBuilder();
        }

        [Test]
        public void template_is_empty()
        {
            var alowed = _builder.IsAmountChangingAllowed(_cashRequest);
            Assert.That(alowed, Is.True);
        }

        [Test]
        public void nothing_was_changed()
        {
            var model = _builder.BuildModel(_loanOriginal);
            _cashRequest.LoanTemplate = model.ToJSON();
            var alowed = _builder.IsAmountChangingAllowed(_cashRequest);
            Assert.That(alowed, Is.True);
        }

        [Test]
        public void installment_was_added()
        {
            var model = _builder.BuildModel(_loanOriginal);

            var first = model.Items[0];

            model.Items.Add(new SchedultItemModel() { Balance = first.Balance - 100, BalanceBeforeRepayment = first.Balance, Date = first.Date.AddDays(10)});

            _cashRequest.LoanTemplate = model.ToJSON();
            var alowed = _builder.IsAmountChangingAllowed(_cashRequest);
            Assert.That(alowed, Is.False);
        }
    }
}
