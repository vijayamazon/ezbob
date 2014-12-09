using ApplicationMng.Repository;
using FluentNHibernate.Mapping;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database
{

	public class RejectReason
	{
		public virtual int Id { get; set; }
		public virtual string Reason { get; set; }
	}

	public class RejectReasonMap : ClassMap<RejectReason>
	{
		public RejectReasonMap()
		{
			Id(x => x.Id);
			Map(x => x.Reason).Length(100);
		}
	}

	public interface IRejectReasonRepository : IRepository<RejectReason>
	{

	}

	public class RejectReasonRepository : NHibernateRepositoryBase<RejectReason>, IRejectReasonRepository
	{
		public RejectReasonRepository(ISession session)
			: base(session)
		{
		}
	}

}
