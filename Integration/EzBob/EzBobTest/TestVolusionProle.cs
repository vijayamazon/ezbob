using System.Collections.Generic;
using System.Xml;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EzBob.AmazonLib;
using EzBob.CommonLib;
using EzBob.PayPal;
using EzBob.eBayLib;
using Integration.Volusion;
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

namespace Integration.ChannelGrabberAPI {
	#region class TestVolusionProle

	[TestFixture]
	public class TestVolusionProle {
		#region public

		private ILog m_oLog;
		private Customer m_oCustomer;
		private string m_sShopUrl;
		private string m_sShopUserName;
		private DatabaseDataHelper _Helper;

		#region method Init

		[SetUp]
		public void Init() {
			StdInit();

			m_oLog = LogManager.GetLogger(this.GetType());

			m_oCustomer = new Customer{
				Id = 1102,
				Name = "alexbo+02@ezbob.com"
			};

			m_sShopUrl = "http://wxjyr.stlas.servertrust.com";

			m_sShopUserName = "kerenl@ezbob.com";
		} // Init

		public void StdInit()
		{
			EnvironmentConfigurationLoader.AppPathDummy = @"c:\EzBob\App\clients\Maven\maven.exe";
			NHibernateManager.FluentAssemblies.Add( typeof( ApplicationMng.Model.Application ).Assembly );
			NHibernateManager.FluentAssemblies.Add( typeof( Customer ).Assembly );
			NHibernateManager.FluentAssemblies.Add(typeof (VolusionDatabaseMarketPlace).Assembly);
			Scanner.Register();
			ObjectFactory.Configure( x =>
			{
				x.For<ISession>().LifecycleIs( new ThreadLocalStorageLifecycle() ).Use( ctx => NHibernateManager.SessionFactory.OpenSession() );
				x.For<ISessionFactory>().Use( () => NHibernateManager.SessionFactory );
			} );

			XmlElement configurationElement = ConfigurationRoot.GetConfiguration().XmlElementLog;
			XmlConfigurator.Configure( configurationElement );

			_Helper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;
		}

		#endregion method Init

		#region method TestGetOrders

		[Test]
		public void TestGetOrders() {
			var oApi = new VolusionProle(m_oLog, m_oCustomer);

			List<ChannelGrabberOrder> lst = oApi.GetOrders(new VolusionAccountData {
				endpoint = m_sShopUrl,
				username = m_sShopUserName
			});

			Assert.AreNotEqual(lst.Count, 0);
		} // TestGetOrders

		#endregion method TestGetOrders

		#endregion public

		#region protected
		#endregion protected

		#region private
		#endregion private
	} // class TestVolusionProle

	#endregion class TestVolusionProle
} // namespace Integration.ChannelGrabberAPI
