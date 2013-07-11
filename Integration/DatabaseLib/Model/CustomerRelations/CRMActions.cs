namespace EZBob.DatabaseLib.Model.CustomerRelations
{
	using ApplicationMng.Repository;
	using NHibernate;

	public class CRMActions
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public interface ICRMActionsRepository : IRepository<CRMActions>
	{
	}

	public class CRMActionsRepository : NHibernateRepositoryBase<CRMActions>, ICRMActionsRepository
	{
		public CRMActionsRepository(ISession session)
			: base(session)
		{
		}
	}
}