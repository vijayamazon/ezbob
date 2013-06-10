namespace EzBobTest
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using System.Threading;
	using System.Xml;
	using EKM;
	using FreeAgent;
	using Integration.Volusion;
	using Integration.Play;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EzBob;
	using EzBob.AmazonLib;
	using EzBob.AmazonServiceLib;
	using EzBob.CommonLib;
	using EzBob.PayPal;
	using EzBob.PayPalDbLib.Models;
	using EzBob.PayPalServiceLib;
	using EzBob.eBayLib;
	using EzBob.eBayLib.Config;
	using EzBob.eBayServiceLib;
	using NHibernate;
	using NUnit.Framework;
	using Scorto.Configuration;
	using Scorto.Configuration.Loader;
	using Scorto.NHibernate;
	using Scorto.RegistryScanner;
	using StructureMap;
	using StructureMap.Pipeline;
	using log4net;
	using log4net.Config;
    using PayPoint;
    using YodleeLib.connector;

    [TestFixture]
    public class TestRetrieveDataHelper
    {
        private DatabaseDataHelper _Helper;

        [SetUp]
        public void Init()
        {
            EnvironmentConfigurationLoader.AppPathDummy = @"c:\EzBob\App\clients\Maven\maven.exe";
            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(eBayDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(AmazonDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(PayPalDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(EkmDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(VolusionDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(PlayDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(YodleeDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(PayPointDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(FreeAgentDatabaseMarketPlace).Assembly);
            Scanner.Register();
            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });

            XmlElement configurationElement = ConfigurationRoot.GetConfiguration().XmlElementLog;
            XmlConfigurator.Configure(configurationElement);

            _Helper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
        }

        [Test]
        public void PayPalSettingsStoredInConfig()
        {
            var payPalSettings = ObjectFactory.GetInstance<IPayPalMarketplaceSettings>();
            var a = payPalSettings.ErrorRetryingInfo;
        }

        [Test]
        public void eBaySettingsStoredInConfig()
        {
            var payPalSettings = ObjectFactory.GetInstance<IEbayMarketplaceSettings>();
            var a = payPalSettings.ErrorRetryingInfo;
        }


        [Test]
        public void GetAnalysisValuesByCustomerMarketPlace()
        {
            var data = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(1648);

        }

        [Test]
        public void UpdateCustomer()
        {
            var id = 35;
            UpdateCustomer(id);

        }

        [Test]
        public void UpdateVolusionMarketplace()
        {
            UpdateCustomerMarketplace(2143);
        }

        [Test]
        public void UpdatePlayMarketplace()
        {
            UpdateCustomerMarketplace(2143); // TODO: put real market place id here
        }

        [Test]
        public void UpdateCustomerMarketplace()
        {
			var umi = 6406;
            UpdateCustomerMarketplace(umi);

            /*var umis = new[] { 2408, 268, 2222 };

            //umis.AsParallel().ForAll( UpdateCustomerMarketplace );
		
            foreach (var umi in umis)
            {
                UpdateCustomerMarketplace( umi );
            }*/
        }

        [Test]
        public void UpdatePayPalAcctountInfo()
        {
            var umi = 1039;
            UpdatePayPalAcctountInfo(umi);
        }

        private void UpdatePayPalAcctountInfo(int umi)
        {
            PayPalRetriveDataHelper.UpdateAccountInfo(umi);
        }

        private void UpdateCustomer(int id)
        {
            var requestId = RetrieveDataHelper.UpdateCustomerData(id);

            while (!RetrieveDataHelper.IsRequestDone(requestId))
            {
                Thread.Sleep(1000);
            }
            var requestState = RetrieveDataHelper.GetRequestState(requestId);
            Assert.NotNull(requestState);
            Assert.IsTrue(requestState.IsDone());
            Assert.IsFalse(requestState.HasError());
        }

        private void UpdateCustomerMarketplace(int umi)
        {
            var requestId = RetrieveDataHelper.UpdateCustomerMarketplaceData(umi);

            while (!RetrieveDataHelper.IsRequestDone(requestId))
            {
                Thread.Sleep(1000);
            }
            var requestState = RetrieveDataHelper.GetRequestState(requestId);
            Assert.NotNull(requestState);
            Assert.IsTrue(requestState.IsDone());
            Assert.IsFalse(requestState.HasError());
        }

        [Test]
        public void GetMarketplaceSecurityInfo()
        {
            var umis = new[] 
			{
				//285
				//86
				239
			};

            var infos = umis.AsParallel().Select(RetrieveDataHelper.RetrieveCustomerSecurityInfo).ToList();

            Assert.IsTrue(infos.Count > 0);
            Assert.AreEqual(infos.Count, umis.Length);
        }

        [Test]
        public void GetAllCustomerMarketplaceSecurityData()
        {
            //var session = ObjectFactory.GetInstance<ISession>();
            var rep = ObjectFactory.GetInstance<CustomerMarketPlaceRepository>();

            var amazon = new AmazonServiceInfo();
            var all = rep.GetAll().Where(mp => mp.Marketplace.InternalId == amazon.InternalId).ToList();

            var listData = new ConcurrentBag<CustomerSecurityInfo>();

            all.AsParallel().ForAll(mpCustomerMarketPlace =>
                {
                    var sd = RetrieveDataHelper.RetrieveCustomerSecurityInfo(mpCustomerMarketPlace.Id) as AmazonSecurityInfo;

                    var customerSecurityInfo = new CustomerSecurityInfo
                    {
                        AmazonMarketplaceId = sd.MarketplaceId.First(),
                        MerchantId = sd.MerchantId,
                        MarketplaceName = mpCustomerMarketPlace.DisplayName,
                        MarketPlaceId = mpCustomerMarketPlace.Id
                    };
                    listData.Add(customerSecurityInfo);
                });

            var list = listData.ToArray();

            SerializeDataHelper.SerializeToFile(@"d:\temp\umi\umiList.xml", list);

        }

        [Test]
        public void SaveMarketplaceAmazon()
        {
            int customerId = 211;

            var databaseCustomer = GetCustomerInfo(customerId);

            var siList = new[]
				{
					//new { Id = "A1OXZLJTRHTZJ3", Name = "Real Amazon Account"},
					//new { Id = "A4G97F1RP09ZP", Name = "Retailers Online"},
					//new { Id = "A1CZA06GMVNMNX", Name ="AndysGreatFinds"}
					new { Id = "AY7B0VWAMNYUB", Name ="positivenoise"}
				};


            foreach (var si in siList)
            {
                var amazonSecurityInfo = new AmazonSecurityInfo(si.Id);
                amazonSecurityInfo.AddMarketplace("A1F83G8C2ARO7P");

                AmazonRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(databaseCustomer, amazonSecurityInfo, si.Name);
            }

        }

        [Test]
        public void SaveMarketplacePayPal()
        {
            int customerId = 1;

            var databaseCustomer = GetCustomerInfo(customerId);

            var siList = new[]
				{
					new { UserId = "amshowman99@gmail.com", Name = "amshowman99"},
					new { UserId = "a.milburn@ntlworld.com", Name = "a.milburn"},
					new { UserId = "paypal@madjungle.com", Name ="adjungle"}
				};


            foreach (var si in siList)
            {
                var secData = new PayPalSecurityData
                {
                    UserId = si.UserId
                };

                PayPalRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(databaseCustomer, secData, si.Name);
            }

        }

        private Customer GetCustomerInfo(int customerId)
        {
            return _Helper.GetCustomerInfo(customerId);
        }

        private AmazonRetriveDataHelper AmazonRetriveDataHelper
        {
            get
            {

                var marketPlace = ObjectFactory.GetInstance<AmazonDatabaseMarketPlace>();
                return marketPlace.GetRetrieveDataHelper(_Helper) as AmazonRetriveDataHelper;
            }
        }

        [Test]
        public void SaveMarketplaceEbay()
        {
            int customerId = 1;

            var tokens = new[]
				{
					"AgAAAA**AQAAAA**aAAAAA**/zT8Tw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wJkYGoD5KHogmdj6x9nY+seQ**kXgBAA**AAMAAA**ZtzWCT7y/CDuAWGaA082vd91NFpT+178ZI43eyd/xsxP+ugZ3XbSslbtCT01q7rzfueXYDYfh7+X09O4lt1DRFvLNmfZjTwsy2U13rLLJLAQoREyEOWerVyZUiBemb0YhSJItaV9NlioKMk4x7wXEnjVThOxfTHSW3KkKQYdwf2itfUM8FGRHrQGpLr+9MVaHAvHk7j6Ns1LV+/S75ylttXIk7m/e8E4qpFlMUqLdgl3xB0pIOc9mnNsQYhHBlNkYzzbi9/44SovH/e730UJGaNI3YiU7xVn6OeedFyUEiOx4cY/eN8Z7lN47ofNyY8j7aUEfvr364pGdVzgqUsBiTzTg8DwrsMcEwaK89eIdiHaaybaKX0W7HM6TwXO/iNgNzR9/RPhpk4qSDuJI2pqJdC+bJ3pL986o52puLcXAAo4X2QyxK/xM3MR79K0XFPgbQ5PlQld8J50VrnIhX62Jur3Ig/LDwga1pTaxYVEZC506nP9lMx0XG8QzfzS6BP5lfPpFXV+7GhlQ7LnQxQDiocaMEOqg9svYFXG4s47Y875Bg4b7kFXGVIQ3TKhOblDJE1ADyb0aZ9GoyUbabYdt3Q8rMi1X+44r9FGGR5UHNopUMM2zOgsVFFgRnkVnJDu/mKUXyZEbKiek8BeDHkv1El/2vOXhI5Ncf1/CMmHleCPAH5Sdo/Q7843WUM+MUd804PHu5bzZI4/pYgelS4AgXhW4wTgoZwGF6WUB7g8GDcQESAIBmRh1vLKPu25gWCN",
					"AgAAAA**AQAAAA**aAAAAA**CzQDUA**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFkIqnCJOFpQydj6x9nY+seQ**kXgBAA**AAMAAA**ahr1JPnT4xKHh6+OIWluq09/FMw3xh7idxmn23CRX1tOkG4F4aJVA1w1Pg0gSupMbufldol55jMP3JyCzKEY233PqLHg0DWQFAf2dZYqw0vK8u5izDCSXANUM66ceANzfO9Zft+ntKFG183jb7vM3l+r1vF1yO37YcD7zPTKppBH59Uq7QFz5VbvvpfwoPJVxrLYg5zOMhsLw9hDz3XpZBKrldeOmYTvCFUTqWpiEgaDz45Y2DRP+3PjBXr457lbkXtVSqYo77xOHzz5E1yL5S2lImd3Ryr3Iay3V68UU7n3JCYpXIXKJE4s9aZu5MEvWHanBl613+vxbX/Cwzib1SfWW6fztwmhpti4J/w2Im8QboJOuccpG+jKvKh+3fKNok5kgUm+9bFyaCqVMWrOMRkgeV46yfbof1rlREQiBZAYqnXbgMkWJR1caKARwkxg0M0vPwMv404g6l3DnxiyAsQvkbhM+wJgQxfyKPW5ZDO7qT4jdvfa+qO+4wSJLDu+ljltX7cKO8o9MuyfrsSk04a+EBi/ayw8eSMhioN6eyqMswySGr7ZHWrMiV0oItl1Hje6KB8hboOQ4SG4B5DwgcDL7VONcHewmqTCh5AN8Nv03YZd0wKa2jdjkL2OB6vlN4qjY3z12eZDLXesCSiO43a9aJuAqczYLkMw3O+nw4h9VNPHjcA00IQmR+PaNSuqLgA+I+S4GLb5MFGVU/MvwG3Dse+lvN8TujgWgmtrdrYKftQCZmiULQ0LszP1w60t",
					"AgAAAA**AQAAAA**aAAAAA**s04FUA**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wGkYWlC5KDpwWdj6x9nY+seQ**kXgBAA**AAMAAA**Z5sECiktbsYxtpGHEeMbkuCtbwRnETUFkqKFSuZ9skyVGGuwLCTPdapIBeFhWptgVUqj0rjds2NXZSk5QXBjmWwQe912xM8ypB0uFsBJ3ux5MRct6UmRqOU5SXfo0aCIqfXLgsx1qU7odZ42Bn/+m0c82rqLlLVA1+PrhMEloPelNJI6mqH7cFbKmIJl8QQ58vs/FjVbDoiU3o5rZwAyKKVLm7ZjM4Ry3tPFUN2zoel9ctS5iFjBe5PbPxZbO3oZIhCgyNiFAT8y9qUJ/ee2rYhu2ivVvisCWeo4ks5Q+6Plp3cQx2cmaxNMehSpm1u8VmsrLYGzoYEu5+DHKhd1Xk/r0IG4tdJLpRwCHLmRJ8NcIBj8HAp9LvgUyYgUh1Y+ofQtbrwxIl6v007Ng0K0Kcj0c4GY2EkdwtKoQUZbGtKxiiYFKabe4ktCrB0V8/s2zNqyhLL2gLmR9unmyeRwyxTKKz8Z2o8/7FKOGHhb7MDN+ATaQhy9MlP4c4TWQcZVQUm62bTRuFt0GUk1mja9nGwi5nGmTnzinGvaE4fFL1wxnBFCbleN8K6U0aUbNqlHiegImAFZu3VetQ7zhtcc52p1in5Q83kAXy25d0YXj2v16fcxdhIXYqf6hxD75bg5cTWrYefaM75tFK9RG+aJEwLRev5AzeCYn/ZWbS6Wq7/LMI90IF1U4Yd/Z4IQmMS/NkZJ4GCMfTHGekbWF7ZZXRPjvqXCsyGbETgANVJq2ozXYby2EcN4KmVslHVDMTin",
					"AgAAAA**AQAAAA**aAAAAA**Q1IuUA**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wJkYSiD5GCoAidj6x9nY+seQ**kXgBAA**AAMAAA**kPXyR8U7iT2Ljf4RgCjM06+VmvifC2biBQRfiAc8AQo5gcBK9wxW1h1J1YiYvaVpiNUnco+pfrBnhli33tyvsN+2u+fslclDnraqKMuf1WmRBoUvBrTDT1wfBEGdLy+U+3BW73imKeJ5PPP4dSrgjYO4xVuFIpY2xos8/MAx7q8YlS3sEgCGrxCCmEjIDGWh3TK5VAyZcFvC22frmTUwerFyAE9DwiTZPGrB0oTIJjwVIERxvRg6yNpks2qh7ne0b7iMYvZjDui/nCZR2+pEd1VTm8hMbyF8RTSqvRT74KQIyepP5VltwZXk5mumyQFv3aCWuFo2aYzJ3Sc7AB5tKigDMqOOEX9i08E7/J1pQ+WFwJpKNwIz4cx4lEyHq6uf5rcuNmBTkyIgiPZqZImv2EwfFSpXbTUAQx1t8gXHVRfzLwl7UdLuN1i6OYtB9ooQOzS10dF/C+ZPDHXf9e2nEpeUNLWeNTgcoRcv/MQgyA7LHzwSvMN8syK0t1Ns0iMpGtZA7xM3T63LZAwb5fYoh2F+HjiB6RIt1IrhSw9mW84NZdM4U6UPT3N4QuCzS9HQ+FkuTuMsh3HilxQkKzs6UOOXJVyGOBrVWo1rYLKkkmwDV9nfS894gWnLgTtWrn+gkkuxmGHMT35UnPhz0D18q8FvrZY5hm0WrGdWfxvYQbRNsgxujkSKJSWDfAJspr6M7GB4Y3Nhdbpx7Gm1fAH8w2Z4Slm494D96DWnu8tjJgbMAyo2XEeDCtkVEATyxCp1",
				};
            //string token = @"AgAAAA**AQAAAA**aAAAAA**O3k6Tw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wHkYKiDJOLqQmdj6x9nY+seQ**qWEBAA**AAMAAA**nRW2tLgiukfpQJQ6eiLxXbzXO0C7UHhoQMFnXihl+AWomDL6Tw+5IZ/7FZ/EWSjap1znb/GbXnNQxRXJOrmlcBNgdf0FKxvfBjJFq4XSr6X6tjdmIbZLKoX89VrwmRd7Gcf7PuvxQBM0Jncr5LTQ+/ycA2dHcWQ5VFSujWYfn0E6ixMeDtRdUTGhXzng5OOrgTQfZztde6erYLeO5YnaWzSVypoui5tVOBP5EYTHQxzQPOZ1GV0/w7ww5ar3aLnZ56UMfungRscgz5lN+kTo4/lgbgDsv/GUBvUUjb0ZsYEGGJWHUGFK8X7AN40KtcQW9q+TGcgz5QU1G70IGDy+etg3xa9pPmhRA6+ZGbp+cKw8wzZLja/iMz5jEfLlCngBytHSTy4/KRdyLraWpR2ZDNoc0HKjnlew6mDmsGejV6J1EPm/r22vIOzpiPJa4Ndu5kDRISv9FHuUt3fgNANbu0j6abMIAhhb6VS6pby0m5NOxx7SDilEf8iFIMgBJ5U5DV9mXvjmzFNLf+GD6okyJ8uH5MvxRLRBD/PW/ybN5MMRWqF1ZA2NkNCUPKciUNxtIkTrDPc7Nr4nksEoZh+cEPcL9lFVBU8emMjhwF7HIioJMiC7KsOeJ3uCISYZpRwFEPIYzC5Qm3JiuD71T1PBCTKzVfhfcx4XfQyb7HZuhPU6qwkqm1sjoId9mJBGP1+QS85fo9wE8sCeBfZOrfHw89nX2G7chz4/zI5pbomXUAn0Wuqb4wtvQ+fNMWM7ssIp";

            var customer = GetCustomerInfo(customerId);

            Assert.NotNull(customer);

            foreach (var token in tokens)
            {
                var data = new eBaySecurityInfo
                {
                    Token = token
                };

                var info = EBayRetriveDataHelper.GetCustomerUserInfo(data);
                var marketplaceName = info.UserID;

                EBayRetriveDataHelper.StoreOrUpdateCustomerSecurityInfo(customer, data, marketplaceName);
            }
        }

        [Test]
        public void Round()
        {
            int countOrders = 51;
            int countItems = 100;
            int rez = (int)Math.Round(countOrders / (double)countItems, MidpointRounding.ToEven);

            Assert.AreEqual(rez, 1);

            countOrders = 50;
            countItems = 100;
            rez = (int)Math.Round(countOrders / (double)countItems, MidpointRounding.ToEven);

            Assert.AreEqual(rez, 0);

            countOrders = 49;
            countItems = 100;
            rez = (int)Math.Round(countOrders / (double)countItems, MidpointRounding.ToEven);

            Assert.AreEqual(rez, 0);


            countOrders = 51;
            countItems = 100;
            rez = (int)Math.Round(countOrders / (double)countItems, MidpointRounding.AwayFromZero);

            Assert.AreEqual(rez, 1);

            countOrders = 50;
            countItems = 100;
            rez = (int)Math.Round(countOrders / (double)countItems, MidpointRounding.AwayFromZero);

            Assert.AreEqual(rez, 1);

            countOrders = 49;
            countItems = 100;
            rez = (int)Math.Round(countOrders / (double)countItems, MidpointRounding.AwayFromZero);

            Assert.AreEqual(rez, 0);



        }

        private eBayRetriveDataHelper EBayRetriveDataHelper
        {
            get
            {
                var marketPlace = ObjectFactory.GetInstance<eBayDatabaseMarketPlace>();

                return marketPlace.GetRetrieveDataHelper(_Helper) as eBayRetriveDataHelper;
            }
        }

        private PayPalRetriveDataHelper PayPalRetriveDataHelper
        {
            get
            {
                var marketPlace = ObjectFactory.GetInstance<PayPalDatabaseMarketPlace>();

                return marketPlace.GetRetrieveDataHelper(_Helper) as PayPalRetriveDataHelper;
            }
        }

        [Test]
        public void WriteLoggerHelper1()
        {
            var _Log = LogManager.GetLogger(typeof(TestRetrieveDataHelper));
            WriteLoggerHelper.Write("Test: Write to Default log", WriteLogType.Debug, _Log);
            WriteLoggerHelper.Write("Test: Write to Specific log", WriteLogType.Debug);
        }

    }

    internal class TestOrderItemData
    {
        public double Price;
        public int Count;

    }

    public class CustomerSecurityInfo
    {
        public int MarketPlaceId { get; set; }
        public string MerchantId { get; set; }
        public string AmazonMarketplaceId { get; set; }
        public string MarketplaceName { get; set; }
    }


}