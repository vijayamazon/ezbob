namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using System;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;
	using NHibernate.Type;

	public class CustomerRelationFollowUp
	{
		public virtual int Id { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual DateTime DateAdded { get; set; }
		public virtual DateTime FollowUpDate { get; set; }
		public virtual string Comment { get; set; }
	}

	public class CustomerRelationFollowUpMap : ClassMap<CustomerRelationFollowUp>
	{
		public CustomerRelationFollowUpMap()
		{
			Table("CustomerRelationFollowUp");
			Id(x => x.Id);
			Map(x => x.DateAdded).CustomType<UtcDateTimeType>();
			Map(x => x.FollowUpDate).CustomType<UtcDateTimeType>();
			Map(x => x.Comment).Length(1000);
			Map(x => x.CustomerId);
		}
	}

	public interface ICustomerRelationFollowUpRepository : IRepository<CustomerRelationFollowUp>
	{
	}
	
	public class CustomerRelationFollowUpRepository : NHibernateRepositoryBase<CustomerRelationFollowUp>, ICustomerRelationFollowUpRepository
	{
		public CustomerRelationFollowUpRepository(ISession session)
			: base(session)
		{
		}

		public CustomerRelationFollowUp GetLastFollowUp(int customerId)
		{
			return GetAll().Where(x => x.CustomerId == customerId).OrderByDescending(x => x.DateAdded).FirstOrDefault();
		}
	}
}