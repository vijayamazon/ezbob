using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Controllers.Exceptions;
using NUnit.Framework;

namespace EzBob.Tests.LoanCreatorTests
{
    public class CustomerStateValidationFixture : LoanCreatorFixtureBase
    {
        private Customer _customer;

        public override void SetUp()
        {
            _customer = new Customer();
            base.SetUp();
        }

        [Test]
        public void customer_should_be_approved()
        {
            
            _customer.Status = Status.Rejected;
            _customer.PersonalInfo = new PersonalInfo() { FirstName = "John", Surname = "Tigers" };
            _customer.BankAccount = new BankAccount(){AccountNumber = "123123", SortCode = "121212", Type = BankAccountType.Personal};
            Assert.Throws<CustomerIsNotApprovedException>(() =>
            {
                _lc.ValidateCustomer(_customer);
            });
        }

        [Test]
        public void customer_should_have_personal_info()
        {
            
            _customer.PersonalInfo = null;
            Assert.Throws<CustomerIsNotFullyRegisteredException>(() =>
            {
                _lc.ValidateCustomer(_customer);
            });
        }

        [Test]
        public void customer_should_have_bank_account()
        {
            
            _customer.PersonalInfo = new PersonalInfo(){FirstName = "John", Surname = "Tigers"};
            _customer.BankAccount = null;
            Assert.Throws<CustomerIsNotFullyRegisteredException>(() =>
            {
                _lc.ValidateCustomer(_customer);
            });
        }
    }
}