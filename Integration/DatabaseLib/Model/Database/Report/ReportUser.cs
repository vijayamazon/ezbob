namespace EZBob.DatabaseLib.Model.Database.Report {
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	[System.Serializable]
	public class ReportUser {
		public virtual int Id { get; set; }
		public virtual string UserName { get; set; }
		public virtual string Name { get; set; }
		public virtual int UnderwriterId { get; set; }
	}
	
	public class ReportUserMap : ClassMap<ReportUser>
	{
		public ReportUserMap()
		{
			Table("ReportUsers");
			Id(u => u.Id);
			Map(u => u.UserName);
			Map(u => u.Name);
			Map(u => u.UnderwriterId);
		} 
	}

	public interface IReportUsersRepository : IRepository<ReportUser>
	{
	}

	public class ReportUsersRepository : NHibernateRepositoryBase<ReportUser>, IReportUsersRepository
	{
		public ReportUsersRepository(ISession session)
			: base(session)
		{
		}
	}
} // namespace
