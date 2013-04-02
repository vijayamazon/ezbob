using EzBob.Web.Areas.Customer.Controllers;
using NUnit.Framework;

namespace EzBob.Tests
{
    [TestFixture]
    public class CustomerNameValidatorFixture
    {
        private CustomerNameValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new CustomerNameValidator();
        }

        [Test]
        public void empty_name_is_good()
        {
            Assert.That(_validator.CheckCustomerName("", "John", "Black"), Is.True);
            Assert.That(_validator.CheckCustomerName(null, "John", "Black"), Is.True);
        }

        [Test]
        public void name1()
        {
            Assert.That(_validator.CheckCustomerName("John Black", "John", "Black"), Is.True);
        }

        [Test]
        public void different_cases()
        {
            Assert.That(_validator.CheckCustomerName("john black", "John", "Black"), Is.True);
            Assert.That(_validator.CheckCustomerName("JOHN BLACK", "John", "Black"), Is.True);
        }

        [Test]
        public void two_names()
        {
            Assert.That(_validator.CheckCustomerName("john john black", "John", "Black"), Is.False);
        }

        [Test]
        public void no_surname()
        {
            Assert.That(_validator.CheckCustomerName("john", "John", "Black"), Is.False);
        }

        [Test]
        public void spaces_between_name()
        {
            Assert.That(_validator.CheckCustomerName("John     Black", "John", "Black"), Is.True);
            Assert.That(_validator.CheckCustomerName(" John Black", "John", "Black"), Is.True);
            Assert.That(_validator.CheckCustomerName("John Black ", "John", "Black"), Is.True);
            Assert.That(_validator.CheckCustomerName("John\tBlack", "John", "Black"), Is.True);
        }

        [Test]
        public void names_does_not_match()
        {
            Assert.That(_validator.CheckCustomerName("Johny Black", "John", "Black"), Is.False);
        }
    }
}