using System.Linq;
using NUnit.Framework;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EzBob.MongoDB
{
    public class MongoTest
    {
        private MongoClient _client;

        [TestFixtureSetUp]
        public void Config()
        {
            _client = new MongoClient("mongodb://localhost/");
        }

        [Test]
        public void Test()
        {
            var server = _client.GetServer();
            var db = server.GetDatabase("TestDB");
            var tests = db.GetCollection("TestCollection");
            var data = new TestData
            {
                name = "Test",
                type = 3,
                data = System.Text.Encoding.UTF8.GetBytes("Hello mongo")
            };
            tests.Insert(data);

            var a = db.GetCollection<TestData>("TestCollection").FindAll().ToList();
            var b = System.Text.Encoding.UTF8.GetString(a[0].data);
            db.Drop();
        }
    }

    public class TestData
    {
        public ObjectId _id;
        public string name;
        public double type;
        public byte[] data;
    }
}
