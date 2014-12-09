using System;
using EZBob.DatabaseLib.Model.Database;
using NUnit.Framework;
using PostcodeAnywhere;

namespace EzBob.Tests.PostcodeAnywhereTests
{
    [TestFixture]
    public class CheckFixture
    {
        private SortCodeChecker _checker;

        [SetUp]
        public void SetUp()
        {
            _checker = new SortCodeChecker(3);
        }

        [Test]
        [Ignore]
        public void check_real_sort_code()
        {
            var customer = new Customer();
            var card = _checker.Check(customer, "70119768", "203716", "personal");
            Assert.That(card.BankBIC, Is.EqualTo("BARCGB21"));
        }

        [Test]
        [Ignore]
        public void return_null_if_too_many_errors()
        {
            var customer = new Customer();
            customer.BankAccountValidationInvalidAttempts = 10;
            var card = _checker.Check(customer, "70119768", "206716", "personal");
            Assert.That(card, Is.Null);
        }

        [Test]
        [Ignore]
        public void check_real_sort_code_DOES_NOT_increases_counter()
        {
            var customer = new Customer();
            var card = _checker.Check(customer, "70119768", "203716", "personal");
            Assert.That(customer.BankAccountValidationInvalidAttempts, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void check_fake_sort_code_increases_counter()
        {
            var customer = new Customer();
            try
            {
                var card = _checker.Check(customer, "70189768", "203713", "personal");
            }
            catch (Exception)
            {
            }
            Assert.That(customer.BankAccountValidationInvalidAttempts, Is.EqualTo(1));
        }
    }
}
