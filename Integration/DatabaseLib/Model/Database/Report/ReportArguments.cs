namespace EZBob.DatabaseLib.Model.Database.Report {
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using NHibernate;

	[System.Serializable]
	public class ReportArguments {
		public virtual int Id { get; set; }
		public virtual DbReport Report { get; set; }
		public virtual ReportArgumentNames ReportArgument { get; set; }
	}
	
	public class ReportArgumentsMap : ClassMap<ReportArguments>
	{
		public ReportArgumentsMap()
		{
			Table("ReportArguments");
			Id(x => x.Id);
			References(x => x.Report, "ReportId");
			References(x => x.ReportArgument, "ReportArgumentNameId");
		} 
	}

	public interface IReportArgumentsRepository : IRepository<ReportArguments>
	{
	}

	public class ReportArgumentsRepository : NHibernateRepositoryBase<ReportArguments>, IReportArgumentsRepository
	{
		public ReportArgumentsRepository(ISession session)
			: base(session)
		{
		}
	}
} // namespace
