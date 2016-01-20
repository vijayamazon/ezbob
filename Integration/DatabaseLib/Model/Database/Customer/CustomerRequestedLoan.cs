namespace EZBob.DatabaseLib.Model.Database
{
	using System;

	public class CustomerRequestedLoan
	{
		public virtual int Id { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual int CustomerId { get; set; }
		public virtual int? CustomerReasonId { get; set; }
		public virtual int? CustomerSourceOfRepaymentId { get; set; }
		public virtual double? Amount { get; set; }
		public virtual string OtherReason { get; set; }
		public virtual string OtherSourceOfRepayment { get; set; }
		public virtual int? Term { get; set; }
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mapping
{
	using FluentNHibernate.Mapping;
	using NHibernate.Type;

	public class CustomerRequestedLoanMap : ClassMap<CustomerRequestedLoan>
	{
		public CustomerRequestedLoanMap()
		{
			Table("CustomerRequestedLoan");
			Id(x => x.Id);
			Map(x => x.Created).CustomType<UtcDateTimeType>();
			Map(x => x.Amount);
			Map(x => x.OtherReason).Length(300);
			Map(x => x.OtherSourceOfRepayment).Length(300);
			Map(x => x.Term);
			Map(x => x.CustomerId);
			Map(x => x.CustomerReasonId, "ReasonId");
			Map(x => x.CustomerSourceOfRepaymentId, "SourceOfRepaymentId");
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface ICustomerRequestedLoanRepository : IRepository<CustomerRequestedLoan>
	{
	}

	public class CustomerRequestedLoanRepository : NHibernateRepositoryBase<CustomerRequestedLoan>, ICustomerRequestedLoanRepository
	{
		public CustomerRequestedLoanRepository(ISession session)
			: base(session)
		{
		}

	}
}
