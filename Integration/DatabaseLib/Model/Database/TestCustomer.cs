using FluentNHibernate.Mapping;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{
	public class TestCustomer
	{
		public virtual int Id { get; set; }
		public virtual string Pattern { get; set; }

	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	public class TestCustomerMap : ClassMap<TestCustomer>
	{
		public TestCustomerMap()
		{
			Table("TestCustomer");
			Id(x => x.Id);
			Map(x => x.Pattern).Length(50);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public interface ITestCustomerRepository : IRepository<TestCustomer>
	{
		IEnumerable<string> GetAllPatterns();
	}

	public class TestCustomerRepository : NHibernateRepositoryBase<TestCustomer>,
	                                             ITestCustomerRepository
	{
		public TestCustomerRepository(ISession session)
			: base(session)
		{
		}

		public IEnumerable<string> GetAllPatterns()
		{
			return GetAll().Select(t => t.Pattern).ToList();
		}
	}
}
