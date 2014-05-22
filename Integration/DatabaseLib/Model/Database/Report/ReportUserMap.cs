namespace EZBob.DatabaseLib.Model.Database.Report {
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	[System.Serializable]
	public class ReportUsersMap {
		public virtual int Id { get; set; }
		public virtual DbReport Report { get; set; }
		public virtual ReportUser ReportUser { get; set; }
		
	}
	
	public class ReportUsersMapMap : ClassMap<ReportUsersMap>
	{
		public ReportUsersMapMap()
		{
			Table("ReportsUsersMap");
			Id(u => u.Id);
			References(x => x.Report, "ReportID");
			References(x => x.ReportUser, "UserID");
		} 
	}

	public interface IReportUsersMapsRepository : IRepository<ReportUsersMap>
	{
		List<DbReport> GetAllUnderwriterReports(int uwId);
	}

	public class ReportUsersMapsRepository : NHibernateRepositoryBase<ReportUsersMap>, IReportUsersMapsRepository
	{
		public ReportUsersMapsRepository(ISession session)
			: base(session)
		{
			
		}

		public List<DbReport> GetAllUnderwriterReports(int uwId)
		{
			return GetAll().Where(x => x.ReportUser.UnderwriterId == uwId).Select(x => x.Report).ToList();
		}
	}
} // namespace
