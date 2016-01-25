using FluentNHibernate.Mapping;
using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{
	public class CustomerSourceOfRepayment
	{
		public virtual int Id { get; set; }
		public virtual string SourceOfRepayment { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	public class CustomerSourceOfRepaymentMap : ClassMap<CustomerSourceOfRepayment>
	{
		public CustomerSourceOfRepaymentMap()
		{
			Table("CustomerSourceOfRepayment");
			Id(x => x.Id);
			Map(x => x.SourceOfRepayment).Length(300);
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	public interface ICustomerSourceOfRepaymentRepository : IRepository<CustomerSourceOfRepayment>
	{
	}

	public class CustomerSourceOfRepaymentRepository : NHibernateRepositoryBase<CustomerSourceOfRepayment>,
												 ICustomerSourceOfRepaymentRepository
	{
		public CustomerSourceOfRepaymentRepository(ISession session)
			: base(session)
		{
		}

	}
}
