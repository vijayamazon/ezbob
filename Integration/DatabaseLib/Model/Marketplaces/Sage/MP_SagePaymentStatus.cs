namespace EZBob.DatabaseLib.Model.Marketplaces.Sage
{
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_SagePaymentStatus
	{
		public virtual int Id { get; set; }

		public virtual int SageId { get; set; }
		public virtual string name { get; set; }
	}

	public interface IMP_SagePaymentStatusRepository : IRepository<MP_SagePaymentStatus>
	{
	}

	public class MP_SagePaymentStatusRepository : NHibernateRepositoryBase<MP_SagePaymentStatus>, IMP_SagePaymentStatusRepository
	{
		public MP_SagePaymentStatusRepository(ISession session)
			: base(session)
		{
		}
	}
}