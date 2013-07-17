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
using Scorto.NHibernate;
using StandaloneInitializer;
using StructureMap;

namespace AmazonStandaloneApp
{
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
                var amazonSettings = ObjectFactory.GetInstance<IAmazonMarketplaceSettings>();

                int umi = int.Parse(args[0]);
                int days = int.Parse(args[1]);
                bool isReporting = int.Parse(args[2]) == 1;

                var elapsedTimeInfo = new ElapsedTimeInfo();

                var orders = GetOrders(umi, amazonSettings, elapsedTimeInfo, connection, days, isReporting);

                DisplayOrders(elapsedTimeInfo, orders);
            }
        }


        private static List<OrderItemTwo> GetOrders(int umi, IAmazonMarketplaceSettings amazonSettings,
                                                   ElapsedTimeInfo elapsedTimeInfo, AmazonServiceConnectionInfo _ConnectionInfo, int days, bool useReporting)
        {
            var session = ObjectFactory.GetInstance<ISession>();

            var marketplace = session.Get<MP_CustomerMarketPlace>(umi);

            var securityInfo = SerializeDataHelper.DeserializeType<AmazonSecurityInfo>(marketplace.SecurityData);

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var amazonOrdersRequestInfo = new AmazonOrdersRequestInfo
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    MarketplaceId = securityInfo.MarketplaceId,
                    MerchantId = securityInfo.MerchantId,
                    ErrorRetryingInfo = amazonSettings.ErrorRetryingInfo
                };


            List<OrderItemTwo> orders;

            if (useReporting)
            {
                var configurator = AmazonServiceConfigurationFactory.CreateServiceReportsConfigurator(_ConnectionInfo);

                orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                                                                                            ElapsedDataMemberType.RetrieveDataFromExternalService,
                                                                                                            () => AmazonServiceReports.GetUserOrders(configurator, amazonOrdersRequestInfo, ActionAccessType.Full))
                                                                                                            .Select(o => OrderItemTwo.FromOrderItem(o))
                                                                                                            .ToList();
            }
            else
            {
                orders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
                                                                                            ElapsedDataMemberType.RetrieveDataFromExternalService,
                                                                                            () => AmazonServiceHelper.GetListOrders(_ConnectionInfo, amazonOrdersRequestInfo, ActionAccessType.Full))
                                                                                            .Select(o => OrderItemTwo.FromOrderItem2(o))
                                                                                            .ToList();
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
