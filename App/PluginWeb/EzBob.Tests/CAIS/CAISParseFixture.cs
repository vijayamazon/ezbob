using System.IO;
using ExperianLib.CaisFile;
using NUnit.Framework;

namespace EzBob.Tests.CAIS
{
    [TestFixture]
    public class CAISParseFixture
    {
        private StreamReader _caisReader;

        [SetUp]
        public void SetUp()
        {
            var ms = new MemoryStream(cais.cais_big);
            _caisReader = new StreamReader(ms);
        }

        [Test]
        public void parse_big_file_line_by_line()
        {
            var b = new BusinessCaisFileData();
            b.ReadFromReader(_caisReader);
            Assert.That(b.Accounts.Count, Is.EqualTo(68));
            Assert.That(b.Accounts[0].AccountNumber, Is.EqualTo("01168863438"));
            Assert.That(b.Accounts[67].AccountNumber, Is.EqualTo("01168863164"));
            Assert.That(b.Header.CompanyPortfolioName, Is.EqualTo("Orange Money"));
            Assert.That(b.Trailer.TotalRecords, Is.EqualTo(b.Accounts.Count));
        }

        [Test]
        public void parse_big_file_at_once()
        {
            var b = new BusinessCaisFileData();
            b.ReadFromString(_caisReader.ReadToEnd());
            Assert.That(b.Accounts.Count, Is.EqualTo(68));
            Assert.That(b.Accounts[0].AccountNumber, Is.EqualTo("01168863438"));
            Assert.That(b.Accounts[67].AccountNumber, Is.EqualTo("01168863164"));
            Assert.That(b.Header.CompanyPortfolioName, Is.EqualTo("Orange Money"));
            Assert.That(b.Trailer.TotalRecords, Is.EqualTo(b.Accounts.Count));
        }

    }
}
