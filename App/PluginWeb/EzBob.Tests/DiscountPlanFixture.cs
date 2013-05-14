using EZBob.DatabaseLib.Model.Loans;
using NUnit.Framework;

namespace EzBob.Tests
{
    [TestFixture]
    public class DiscountPlanFixture
    {
        [Test]
        public void can_parse_discount_values()
        {
            var plan = new DiscountPlan();
            plan.ValuesStr = "+0,+0,+0,-10,-20,-30";

            Assert.That(plan.Discounts.Length, Is.EqualTo(6));
            CollectionAssert.AreEqual(new decimal[]{0, 0, 0, -10, -20, -30}, plan.Discounts);
        } 
    }
}