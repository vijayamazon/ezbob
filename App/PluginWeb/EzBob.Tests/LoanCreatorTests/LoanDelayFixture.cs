using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.Areas.Customer.Controllers;
using NUnit.Framework;

namespace EzBob.Tests.LoanCreatorTests
{
    public class LoanDelayFixture : LoanCreatorFixtureBase
    {
        private Customer _customer;

        public override void SetUp()
        {
            _customer = new Customer();
            base.SetUp();
        }

        [Test]
        public void no_loan_no_delay()
        {
            Assert.DoesNotThrow(() =>
            {
                _lc.ValidateLoanDelay(_customer, DateTime.UtcNow);
            });
        }

        [Test]
        public void should_throw_if_within_delay_one_loan()
        {
            var loanStart = new DateTime(2013, 1, 1);
            
            _customer.Loans.Add(new Loan { Date = loanStart });
            
            Assert.Throws<LoanDelayViolationException>(() =>
            {
                _lc.ValidateLoanDelay(_customer, loanStart.AddSeconds(20));
            });
        }

        [Test]
        public void should_throw_if_within_delay_three_loans()
        {
            var loanStart = new DateTime(2013, 1, 1);
            
            _customer.Loans.Add(new Loan { Date = loanStart });
            _customer.Loans.Add(new Loan { Date = loanStart.AddHours(-1) });
            _customer.Loans.Add(new Loan { Date = loanStart.AddHours(-2) });
            _customer.Loans.Add(new Loan { Date = loanStart.AddHours(-3) });
            
            Assert.Throws<LoanDelayViolationException>(() =>
            {
                _lc.ValidateLoanDelay(_customer, loanStart.AddSeconds(20));
            });
        }

        [Test]
        public void should_pass_if_outside_delay_one_loan()
        {
            var loanStart = new DateTime(2013, 1, 1);
            
            _customer.Loans.Add(new Loan { Date = loanStart });
            
            Assert.DoesNotThrow(() =>
            {
                _lc.ValidateLoanDelay(_customer, loanStart.AddSeconds(80));
            });
        }

        [Test]
        public void should_pass_if_outside_delay_three_loans()
        {
            var loanStart = new DateTime(2013, 1, 1);
            
            _customer.Loans.Add(new Loan { Date = loanStart.AddHours(-3) });
            _customer.Loans.Add(new Loan { Date = loanStart });
            _customer.Loans.Add(new Loan { Date = loanStart.AddHours(-1) });
            _customer.Loans.Add(new Loan { Date = loanStart.AddHours(-2) });

            Assert.DoesNotThrow(() =>
            {
                _lc.ValidateLoanDelay(_customer, loanStart.AddSeconds(80));
            });
        }
    }
}