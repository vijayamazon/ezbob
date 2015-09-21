namespace EzBobTest
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using CompanyFiles;
	using ConfigManager;
	using EKM;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using EzBob.AmazonServiceLib.Common;
	using EzBob.AmazonServiceLib.Config;
	using EzBob.AmazonServiceLib.Orders.Model;
	using EzBob.AmazonServiceLib.ServiceCalls;
	using Ezbob.Database;
	using Ezbob.Database.Pool;
	using Ezbob.RegistryScanner;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;
	using FreeAgent;
	using Sage;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.AmazonServiceLib;
	using EzBob.CommonLib;
	using EzBob.PayPal;
	using EzBob.PayPalDbLib.Models;
	using EzBob.eBayLib;
	using EzBob.eBayServiceLib;
	using NHibernate;
	using NUnit.Framework;
	using NHibernateWrapper.NHibernate;
	using StructureMap;
	using StructureMap.Pipeline;
	using log4net;
    using PayPoint;
    using YodleeLib.connector;
	using System.IO;
	using Ezbob.HmrcHarvester;
	using Ezbob.Logger;
	using EzBob.Models.Marketplaces.Builders;
	using Integration.ChannelGrabberFrontend;

	[TestFixture]
    public class TestRetrieveDataHelper
    {
        private DatabaseDataHelper _Helper;
		protected AConnection m_oDB;
		protected ASafeLog m_oLog;

        [SetUp]
        public void Init()
        {
			NHibernateManager.FluentAssemblies.Add(typeof(User).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(eBayDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(AmazonDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(PayPalDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(EkmDatabaseMarketPlace).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(DatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(YodleeDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(PayPointDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(FreeAgentDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(SageDatabaseMarketPlace).Assembly);
			NHibernateManager.FluentAssemblies.Add(typeof(CompanyFilesDatabaseMarketPlace).Assembly);
            Scanner.Register();
            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });

            _Helper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
	        
			var oLog4NetCfg = new Log4Net().Init();

			m_oLog = new ConsoleLog(new SafeILog(this));

			m_oDB = new SqlConnection(oLog4NetCfg.Environment, m_oLog);

			ConfigManager.CurrentValues.Init(m_oDB, m_oLog);
			DbConnectionPool.ReuseCount = CurrentValues.Instance.ConnectionPoolReuseCount;
			AConnection.UpdateConnectionPoolMaxSize(CurrentValues.Instance.ConnectionPoolMaxSize);
        }

		[Test]
		public void TestHmrcPdfThrasher() {
			var oLog = new SafeILog(LogManager.GetLogger(typeof(TestRetrieveDataHelper)));

			var p = new VatReturnPdfThrasher(true, oLog);

			foreach (string sFilePah in Directory.GetFiles(@"c:\ezbob\test-data\vat-return", "vat*.pdf")) {
				oLog.Msg("Processing file {0} started...", sFilePah);

				var smd = new SheafMetaData {
					BaseFileName = sFilePah,
					DataType = DataType.VatReturn,
					FileType = FileType.Pdf,
					Thrasher = p
				};

				ISeeds s = p.Run(smd, File.ReadAllBytes(sFilePah));

				oLog.Msg("Processing file {0} complete.", sFilePah);
			} // for each
		} // TestHmrcPdfThrasher

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
				var amazonSecurityInfo = new AmazonSecurityInfo(si.Id, null /*todo*/);
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
		public void CompanyFilesTest()
		{
			var marketPlace = ObjectFactory.GetInstance<CompanyFilesDatabaseMarketPlace>();
            var r = marketPlace.GetRetrieveDataHelper(_Helper) as CompanyFilesRetriveDataHelper;
			r.CustomerMarketplaceUpdateAction(18306);
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
		public void getEbayAccountInfo()
		{
			//var ebayConnectionInfo = ObjectFactory.GetInstance<IEbayMarketplaceTypeConnection>();
			//var _EbayConnectionInfo = eBayServiceHelper.CreateConnection( ebayConnectionInfo );
			//var mp = EBayRetriveDataHelper.GetDatabaseCustomerMarketPlace(18290);
			//var info = EBayRetriveDataHelper.CreateProviderCreationInfo(mp, _EbayConnectionInfo);
			//var account = new DataProviderGetAccount( info );
			//var data = account.GetAccount();
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

		[Test]
		public void regexTest()
		{
			var regex = new Regex("[^a-zA-Z -]");
			var surname = "O'Hare";
			surname = regex.Replace(surname, String.Empty);
			Assert.AreEqual("OHare", surname);
		}

		[Test]
		public void TestAmazonOrders() {
			var amazonTester = new AmazonTester();
			var elapsedTimeInfo = new ElapsedTimeInfo();
			var connectionInfo = ObjectFactory.GetInstance<AmazonMarketPlaceTypeConnection>();
			var connection = AmazonServiceConnectionFactory.CreateConnection(connectionInfo);
			var orders = amazonTester.GetOrders(18363, elapsedTimeInfo, connection, 7, false);
			amazonTester.DisplayOrders(elapsedTimeInfo, orders);
		}

		[Test]
		public  void TestMarketPlaceModelBuilder() {
			ISession session = ObjectFactory.GetInstance<ISession>();
			//var m = new MarketplaceModelBuilder(session);
			//var mp = session.Get<MP_CustomerMarketPlace>(18350); //sage not work
			//var mp = session.Get<MP_CustomerMarketPlace>(18289); //hmrc work 
			//var mp = session.Get<MP_CustomerMarketPlace>(18309); //ekm not work
			var mp = session.Get<MP_CustomerMarketPlace>(18362); //paypal not work

			IMarketplaceModelBuilder builder =
						ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString()) ??
						ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");

			//var ag = mp.Marketplace.GetAggregations(mp, null);
			var m = builder.Create(mp, null);
			
			Assert.True(m.PayPal != null);
			Assert.True(m.PayPal.PersonalInfo != null);
			//foreach (var a in ag) {
			//	Console.WriteLine("{0} {1} {2}", a.ParameterName, a.TimePeriod, a.Value);
			//}

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

	public class AmazonTester {
		public List<OrderItemTwo> GetOrders(int umi, ElapsedTimeInfo elapsedTimeInfo, AmazonServiceConnectionInfo _ConnectionInfo, int days, bool useReporting) {
			var session = ObjectFactory.GetInstance<ISession>();

			var marketplace = session.Get<MP_CustomerMarketPlace>(umi);

			var securityInfo = Serialized.Deserialize<AmazonSecurityInfo>(marketplace.SecurityData);

			var endDate = DateTime.UtcNow;
			var startDate = endDate.AddDays(-days);

			var errorRetryingInfo = new ErrorRetryingInfo((bool)CurrentValues.Instance.AmazonEnableRetrying, CurrentValues.Instance.AmazonMinorTimeoutInSeconds, CurrentValues.Instance.AmazonUseLastTimeOut);

			errorRetryingInfo.Info = new ErrorRetryingItemInfo[2];
			errorRetryingInfo.Info[0] = new ErrorRetryingItemInfo(CurrentValues.Instance.AmazonIterationSettings1Index, CurrentValues.Instance.AmazonIterationSettings1CountRequestsExpectError, CurrentValues.Instance.AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes);
			errorRetryingInfo.Info[1] = new ErrorRetryingItemInfo(CurrentValues.Instance.AmazonIterationSettings2Index, CurrentValues.Instance.AmazonIterationSettings2CountRequestsExpectError, CurrentValues.Instance.AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes);

			var amazonOrdersRequestInfo = new AmazonOrdersRequestInfo {
				StartDate = startDate,
				EndDate = endDate,
				MarketplaceId = securityInfo.MarketplaceId,
				MerchantId = securityInfo.MerchantId,
				ErrorRetryingInfo = errorRetryingInfo
			};

			List<OrderItemTwo> orders;

			if (useReporting) {
				var configurator = AmazonServiceConfigurationFactory.CreateServiceReportsConfigurator(_ConnectionInfo);

				orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo, umi,
					ElapsedDataMemberType.RetrieveDataFromExternalService,
					() => AmazonServiceReports.GetUserOrders(configurator, amazonOrdersRequestInfo, ActionAccessType.Full))
					.Select(OrderItemTwo.FromOrderItem)
					.ToList();
			} else {
				var counter = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo, umi,
					ElapsedDataMemberType.RetrieveDataFromExternalService,
					() => AmazonServiceHelper.GetListOrders(_ConnectionInfo, amazonOrdersRequestInfo, ActionAccessType.Full, null));
				//todo make it work again
				orders = new List<OrderItemTwo>();
			}

			return orders;
		}

		public void DisplayOrders(ElapsedTimeInfo elapsedTimeInfo, List<OrderItemTwo> orders) {
			foreach (var order in orders) {
				Console.WriteLine(order.ToString());
			}

			Console.WriteLine("RetrieveDataFromExternalService took {0} s", elapsedTimeInfo.GetValue(ElapsedDataMemberType.RetrieveDataFromExternalService));
			Console.WriteLine("Number of orders: {0}", orders.Count);
		}
	}

	public class OrderItemTwo {
		public override string ToString() {

			return string.Format("NumberOfItemsShipped: {0}, PurchaseDate: {1}, Total: {2}, OrderStatus: {3}, OrderId: {4}", NumberOfItemsShipped, PurchaseDate, Total, OrderStatus, OrderId);
		}

		public static OrderItemTwo FromOrderItem(AmazonOrderItem item) {
			return new OrderItemTwo() {
				OrderId = item.OrderId,
				OrderStatus = item.OrderStatus,
				Total = item.OrderTotal.Value,
				PurchaseDate = item.PurchaseDate,
				NumberOfItemsShipped = item.NumberOfItemsShipped
			};
		}

		public int? NumberOfItemsShipped { get; set; }

		public DateTime? PurchaseDate { get; set; }

		public double Total { get; set; }

		public AmazonOrdersList2ItemStatusType OrderStatus { get; set; }

		public string OrderId { get; set; }
	}
}
