namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using ApplicationMng.Repository;
	using NHibernate;

	public class CRMStatuses
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public interface ICRMStatusesRepository : IRepository<CRMStatuses>
	{
	}

	public class CRMStatusesRepository : NHibernateRepositoryBase<CRMStatuses>, ICRMStatusesRepository
	{
		public CRMStatusesRepository(ISession session)
			: base(session)
		{
		}
	}
}