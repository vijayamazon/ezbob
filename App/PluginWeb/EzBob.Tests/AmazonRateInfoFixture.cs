using EzBob.AmazonServiceLib.UserInfo;
using NUnit.Framework;

namespace EzBob.Tests
{
    [TestFixture]
    public class AmazonRateInfoFixture
    {
        [Test]
        public void can_rate()
        {
            var x = AmazonRateInfo.GetUserRatingInfo("A1OXZLJTRHTZJ3");

            Assert.That(x.Rating, Is.GreaterThan(0));
            Assert.That(x.Name, Is.EqualTo("Contigoit"));
        }

		[Test]
        [Ignore]
		public void can_rate2()
		{
			var x = AmazonRateInfo.GetUserRatingInfo( "A2KLM32RICPAA2" );
		}

		[Test]
		public void IsUserCorrect()
		{
			string amazonMerchantId = "A2KLM32RICPAA2";
			var isUserCorrect = AmazonRateInfo.IsUserCorrect( new AmazonUserInfo { MerchantId = amazonMerchantId } );

			Assert.That( isUserCorrect, Is.False );
		}
    }
}