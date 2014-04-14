namespace EzBob.Tests.Terapeak
{
	using TeraPeakServiceLib;
	using TeraPeakServiceLib.Stub;
	using NUnit.Framework;

    [TestFixture]
    public class CategoriesFixture
    {
        [Test]
        [Ignore]
        public void request_data()
        {
            //var requestInfo = CreateRequestInfo("headboardsdirect");
            var requestInfo = TerapeakRequestInfoBuilder.CreateRequestInfo("megastar4", 10);
            var data = TeraPeakService.SearchBySeller(requestInfo);
            Assert.That(data.Count, Is.GreaterThan(0));
        }
    }
}