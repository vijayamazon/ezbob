namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using System;
	using ApplicationMng.Repository;
	using NHibernate;

	public class MP_FreeAgentUsers
	{
		public virtual int Id { get; set; }

		public virtual MP_FreeAgentRequest Request { get; set; }

		public virtual string url { get; set; }
		public virtual string first_name { get; set; }
		public virtual string last_name { get; set; }
		public virtual string email { get; set; }
		public virtual string role { get; set; }
		public virtual int permission_level { get; set; }
		public virtual decimal opening_mileage { get; set; }
		public virtual DateTime updated_at { get; set; }
		public virtual DateTime created_at { get; set; }
	}

	public interface IMP_FreeAgentUsersRepository : IRepository<MP_FreeAgentUsers>
	{
	}

	public class MP_FreeAgentUsersRepository : NHibernateRepositoryBase<MP_FreeAgentUsers>, IMP_FreeAgentUsersRepository
	{
		public MP_FreeAgentUsersRepository(ISession session)
			: base(session)
		{
		}
	}
}