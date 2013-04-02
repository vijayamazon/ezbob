using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;

namespace EzBob.Tests
{
    [TestFixture]
    public class LoanAddTransactionFixture
    {
        
        [Test]
        public void adding_transaction_to_loan_sets_references()
        {
            var loan = new Loan(){Id = 1};
            var tran = new PaypointTransaction(){Id = 2};
            loan.AddTransaction(tran);

            Assert.That(tran.Loan.Id, Is.EqualTo(loan.Id));
            Assert.That(loan.Transactions.Count, Is.EqualTo(1));
            Assert.That(loan.Transactions.First().Id, Is.EqualTo(tran.Id));
        }

        [Test]
        public void adding_transaction_to_loan_generates_refnumber()
        {
            var loan = new Loan() { RefNumber = "01141490002" };
            var tran = new PaypointTransaction();
            loan.AddTransaction(tran);

            Assert.That(tran.RefNumber.Length, Is.EqualTo(14));
            Assert.That(tran.RefNumber, Is.EqualTo("01141490002" + "001"));
        }
    }
}