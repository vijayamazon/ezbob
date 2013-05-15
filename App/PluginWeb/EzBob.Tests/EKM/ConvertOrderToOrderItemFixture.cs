using System;
using NUnit.Framework;

namespace EzBob.Tests.EKM
{
    [TestFixture]
    public class ConvertOrderToOrderItemFixture
    {
        [Test]
         public void can_parse_date_time()
         {
             var order = new global::EKM.API.Order()
                             {
                                 OrderDate = "19/02/2013 16:06:41",
                                 OrderDateISO = "2013-02-19T16:06:41"
                             };

             var item = order.ToEkmOrderItem();

             var expected = new DateTime(2013, 2, 19, 16, 6, 41);

             Assert.That(item.OrderDate, Is.EqualTo(expected));
             Assert.That(item.OrderDateIso, Is.EqualTo(expected));
         }
    }
}