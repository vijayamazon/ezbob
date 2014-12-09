using System;
using System.Linq;
using EZBob.DatabaseLib.Model;
using NUnit.Framework;

namespace EzBob.Tests.CustomerTests
{
    [TestFixture]
    public class TryAddPayPointCardTests
    {
        [Test]
        public void adds_new_credit_card_after_callback()
        {
            var customer = new EZBob.DatabaseLib.Model.Database.Customer();
            var card = customer.TryAddPayPointCard("0f22b1c-e8ec-4299-83c2-ed9f9443abf2", "3917", "1014", null);

            Assert.That(customer.PayPointCards.Count, Is.EqualTo(1));
            Assert.That(card.ExpireDate.Value.Year, Is.EqualTo(2014));
            Assert.That(card.ExpireDate.Value.Month, Is.EqualTo(10));
            Assert.That(card.CardNo, Is.EqualTo("3917"));
            Assert.That(card.TransactionId, Is.EqualTo("0f22b1c-e8ec-4299-83c2-ed9f9443abf2"));
        }

        [Test]
        public void adds_new_test_credit_card_after_callback()
        {
            var customer = new EZBob.DatabaseLib.Model.Database.Customer();
            var card = customer.TryAddPayPointCard("0f22b1c-e8ec-4299-83c2-ed9f9443abf2", null, null, null);

            Assert.That(customer.PayPointCards.Count, Is.EqualTo(1));
            Assert.That(card.ExpireDate.HasValue, Is.False);
            Assert.That(card.CardNo, Is.Null);
            Assert.That(card.TransactionId, Is.EqualTo("0f22b1c-e8ec-4299-83c2-ed9f9443abf2"));
        }

        [Test]
        [Ignore]
        public void if_new_card_expire_date_and_cardno_equal_to_existing_overwrite_it()
        {
            var customer = new EZBob.DatabaseLib.Model.Database.Customer();

            var payPointCard = new PayPointCard()
                                   {
                                       CardNo = "1234",
                                       Customer = customer,
                                       DateAdded = new DateTime(2012, 10, 10),
                                       ExpireDate = new DateTime(2014, 10, 1),
                                       Id = 1,
                                       TransactionId = "0f22b1c-e8ec-4299-83c2-ed9f9443abf3"
                                   };
            customer.PayPointCards.Add(payPointCard);

            var card = customer.TryAddPayPointCard("0f22b1c-e8ec-4299-83c2-ed9f9443abf2", "1234", "1014", null);

            Assert.That(customer.PayPointCards.Count, Is.EqualTo(1));
            Assert.That(customer.PayPointCards.First().TransactionId, Is.EqualTo("0f22b1c-e8ec-4299-83c2-ed9f9443abf2"));
        }

        [Test]
        public void tes_card_does_not_override()
        {
            var customer = new EZBob.DatabaseLib.Model.Database.Customer();

            var payPointCard = new PayPointCard()
                                   {
                                       Customer = customer,
                                       DateAdded = new DateTime(2012, 10, 10),
                                       Id = 1,
                                       TransactionId = "0f22b1c-e8ec-4299-83c2-ed9f9443abf3"
                                   };
            customer.PayPointCards.Add(payPointCard);

            var card = customer.TryAddPayPointCard("0f22b1c-e8ec-4299-83c2-ed9f9443abf2", null, null, null);

            Assert.That(customer.PayPointCards.Count, Is.EqualTo(2));
        }
    }
}
