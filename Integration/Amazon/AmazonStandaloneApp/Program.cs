namespace AmazonStandaloneApp
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.AmazonLib;
	using EzBob.AmazonServiceLib;
	using EzBob.AmazonServiceLib.Common;
	using EzBob.AmazonServiceLib.Config;
	using EzBob.AmazonServiceLib.Orders.Model;
	using EzBob.AmazonServiceLib.ServiceCalls;
	using EzBob.CommonLib;
	using NHibernate;
	using StandaloneInitializer;
	using StructureMap;
	using ConfigManager;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;
	using NHibernateWrapper.NHibernate;

	class Program
    {
        static void Main(string[] args)
        {
            StandaloneApp.Execute<App>(args);
        }

        public class App : StandaloneApp
        {
            public override void Run(string[] args)
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: StandaloneAmazonApp.exe <umi> <days> <isReporting>");
                    return;
                }

                Init();

                var connectionInfo = ObjectFactory.GetInstance<IAmazonMarketPlaceTypeConnection>();
                var connection = AmazonServiceConnectionFactory.CreateConnection(connectionInfo);

                int umi = int.Parse(args[0]);
                int days = int.Parse(args[1]);
                bool isReporting = int.Parse(args[2]) == 1;

                var elapsedTimeInfo = new ElapsedTimeInfo();

                var orders = GetOrders(umi, elapsedTimeInfo, connection, days, isReporting);

                DisplayOrders(elapsedTimeInfo, orders);
            }
        }

        private static List<OrderItemTwo> GetOrders(int umi, ElapsedTimeInfo elapsedTimeInfo, AmazonServiceConnectionInfo _ConnectionInfo, int days, bool useReporting)
        {
            var session = ObjectFactory.GetInstance<ISession>();

            var marketplace = session.Get<MP_CustomerMarketPlace>(umi);

            var securityInfo = Serialized.Deserialize<AmazonSecurityInfo>(marketplace.SecurityData);

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

			var errorRetryingInfo = new ErrorRetryingInfo((bool)CurrentValues.Instance.AmazonEnableRetrying, CurrentValues.Instance.AmazonMinorTimeoutInSeconds, CurrentValues.Instance.AmazonUseLastTimeOut);

			errorRetryingInfo.Info = new ErrorRetryingItemInfo[2];
			errorRetryingInfo.Info[0] = new ErrorRetryingItemInfo(CurrentValues.Instance.AmazonIterationSettings1Index, CurrentValues.Instance.AmazonIterationSettings1CountRequestsExpectError, CurrentValues.Instance.AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes);
			errorRetryingInfo.Info[1] = new ErrorRetryingItemInfo(CurrentValues.Instance.AmazonIterationSettings2Index, CurrentValues.Instance.AmazonIterationSettings2CountRequestsExpectError, CurrentValues.Instance.AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes);

            var amazonOrdersRequestInfo = new AmazonOrdersRequestInfo
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    MarketplaceId = securityInfo.MarketplaceId,
                    MerchantId = securityInfo.MerchantId,
					ErrorRetryingInfo = errorRetryingInfo
                };

            List<OrderItemTwo> orders;

            if (useReporting)
            {
                var configurator = AmazonServiceConfigurationFactory.CreateServiceReportsConfigurator(_ConnectionInfo);

	            orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
		            elapsedTimeInfo, umi,
		            ElapsedDataMemberType.RetrieveDataFromExternalService,
		            () => AmazonServiceReports.GetUserOrders(configurator, amazonOrdersRequestInfo, ActionAccessType.Full))
	                        .Select(OrderItemTwo.FromOrderItem)
	                        .ToList();
            }
            else
            {

                var ordersList = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,umi, 
					ElapsedDataMemberType.RetrieveDataFromExternalService,
                    () => AmazonServiceHelper.GetListOrders(_ConnectionInfo, amazonOrdersRequestInfo, ActionAccessType.Full, null));

				//todo make it work again
	            orders = new List<OrderItemTwo>();
            }

            return orders;
        }

        private static void DisplayOrders(ElapsedTimeInfo elapsedTimeInfo, List<OrderItemTwo> orders)
        {
            foreach (var order in orders)
            {
                Console.WriteLine(order.ToString());
            }

            Console.WriteLine("RetrieveDataFromExternalService took {0} s", elapsedTimeInfo.GetValue(ElapsedDataMemberType.RetrieveDataFromExternalService));
            Console.WriteLine("Number of orders: {0}", orders.Count);
        }

        public static void Init()
        {
            Bootstrap.Init();
            NHibernateManager.FluentAssemblies.Add(typeof(AmazonDatabaseMarketPlace).Assembly);
        }
    }
}
