using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonLib;
using EzBob.AmazonServiceLib;
using EzBob.AmazonServiceLib.Common;
using EzBob.AmazonServiceLib.Config;
using EzBob.AmazonServiceLib.Orders.Model;
using EzBob.AmazonServiceLib.ServiceCalls;
using EzBob.CommonLib;
using NHibernate;
using Scorto.Configuration;
using Scorto.Configuration.Loader;
using Scorto.NHibernate;
using Scorto.RegistryScanner;
using StructureMap;
using StructureMap.Pipeline;
using log4net.Config;

namespace StandaloneAmazonApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("Usage: StandaloneAmazonApp.exe <umi> <days> <isReporting>");
                    return;
                }

                Init();

                var connectionInfo = ObjectFactory.GetInstance<IAmazonMarketPlaceTypeConnection>();

                var _ConnectionInfo = AmazonServiceConnectionFactory.CreateConnection(connectionInfo);
                var amazonSettings = ObjectFactory.GetInstance<IAmazonMarketplaceSettings>();

                int umi = int.Parse(args[0]);
                int days = int.Parse(args[1]);
                bool isReporting = int.Parse(args[2]) == 1;

                var elapsedTimeInfo = new ElapsedTimeInfo();

                var orders = GetOrders(umi, amazonSettings, elapsedTimeInfo, _ConnectionInfo, days, isReporting);

                DisplayOrders(elapsedTimeInfo, orders);
            }
            finally
            {
                Console.WriteLine("Finished at {0}", DateTime.Now);
                Console.ReadLine();
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
            EnvironmentConfigurationLoader.AppPathDummy = @"c:\alexbo\src\App\clients\Maven\maven.exe";
            //EnvironmentConfigurationLoader.AppPathDummy = @"c:\EzBob\App\clients\Maven\maven.exe";
            NHibernateManager.FluentAssemblies.Add(typeof(ApplicationMng.Model.Application).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(Customer).Assembly);
            NHibernateManager.FluentAssemblies.Add(typeof(AmazonDatabaseMarketPlace).Assembly);

            Scanner.Register();

            //ObjectFactory.Configure(x => x.AddRegistry<EzBobRegistry>());

            ObjectFactory.Configure(x =>
            {
                x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
                x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
            });

            var cfg = ConfigurationRoot.GetConfiguration();

            XmlElement configurationElement = cfg.XmlElementLog;
            XmlConfigurator.Configure(configurationElement);

        }
    }
}
