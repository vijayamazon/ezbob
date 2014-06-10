using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.RegistryScanner;
	using NHibernate;
	using NHibernateWrapper.NHibernate;
	using NUnit.Framework;
	using StructureMap;
	using StructureMap.Pipeline;

	[TestFixture]
	class BaseTestFixture
	{
		protected AConnection _db;
		protected ConsoleLog _log;

		[SetUp]
		public void Init()
		{
			_log = new ConsoleLog();
			_db = new SqlConnection(new Ezbob.Context.Environment(), _log);

			Scanner.Register();
			ObjectFactory.Configure(x =>
			{
				x.For<ISession>().LifecycleIs(new ThreadLocalStorageLifecycle()).Use(ctx => NHibernateManager.SessionFactory.OpenSession());
				x.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			});
		}
	}
}
