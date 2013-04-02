using System;
using EZBob.DatabaseLib.Model.Database;
using NUnit.Framework;

namespace EzBob.Tests.OfferValidation
{
    [TestFixture]
    public class DateValidationFixture
    {
        private EZBob.DatabaseLib.Model.Database.Customer _customer;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _customer = new EZBob.DatabaseLib.Model.Database.Customer();
            _now = DateTime.UtcNow;
        }

        [Test]
        public void offer_start_null_is_not_valid()
        {
            _customer.OfferStart = null;
            _customer.OfferValidUntil = _now.AddHours(1);
            
            Assert.Throws<OfferExpiredException>(() => {
                    _customer.ValidateOfferDate(); 
            });
        }

        [Test]
        public void offer_valid_until_null_is_not_valid()
        {
            _customer.OfferStart = _now.AddHours(-1);
            _customer.OfferValidUntil = null;

            Assert.Throws<OfferExpiredException>(() => 
                {
                    _customer.ValidateOfferDate(); 
                });
        }

        [Test]
        public void expired_offer()
        {
            _customer.OfferStart = _now.AddHours(-1);
            _customer.OfferValidUntil = _now.AddHours(1);

            Assert.Throws<OfferExpiredException>(() => 
            {
                _customer.ValidateOfferDate(_now.AddHours(2));
            });
        }

        [Test]
        public void not_started_offer()
        {
            _customer.OfferStart = _now.AddHours(1);
            _customer.OfferValidUntil = _now.AddHours(1);

            Assert.Throws<OfferExpiredException>(() => 
            {
                _customer.ValidateOfferDate(_now.AddHours(-2));
            });
        }

        [Test]
        public void valid_offer()
        {
            _customer.OfferStart = _now.AddHours(-1);
            _customer.OfferValidUntil = _now.AddHours(1);

            Assert.DoesNotThrow(()=>
                {
                    _customer.ValidateOfferDate();
                });            
        }

        [Test]
        public void valid_offer_explicit()
        {
            _customer.OfferStart = _now.AddHours(-1);
            _customer.OfferValidUntil = _now.AddHours(1);

            Assert.DoesNotThrow(()=>
                {
                    _customer.ValidateOfferDate(_now);
                });            
        }
    }
}