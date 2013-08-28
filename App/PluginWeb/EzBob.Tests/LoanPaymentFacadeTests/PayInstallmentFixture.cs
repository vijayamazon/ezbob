using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using Moq;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    [TestFixture]
    public class PayInstallmentFixture
    {
        [Test]
        public void saves_loan_history()
        {
            LoanHistory historyItem = null;
            var history = new Mock<ILoanHistoryRepository>();
            history.Setup(x => x.Save(It.IsAny<LoanHistory>())).Callback<LoanHistory>(i =>
            {
                historyItem = i;
            });
            var facade = new LoanPaymentFacade(history.Object, null);

            var loan = new Loan() { Id = 1, Status = LoanStatus.Live };
            var calculator = new LoanScheduleCalculator();
            var startDate = new DateTime(2012, 1, 1);
            calculator.Calculate(3000, loan, startDate);

            var installment = loan.Schedule[0];
            var oldInstallment = installment.Clone();

            facade.PayInstallment(installment, 100, "w3", "", installment.Date);

            Assert.That(historyItem.ExpectedAmountDue, Is.EqualTo(oldInstallment.AmountDue));
        }
    }
}