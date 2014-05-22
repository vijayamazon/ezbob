namespace EZBob.DatabaseLib.Model.Database.Report {
	using ApplicationMng.Repository;
	using FluentNHibernate.Mapping;
	using Iesi.Collections.Generic;
	using NHibernate;

	[System.Serializable]
	public class DbReport {
		public virtual int Id { get; set; }
		public virtual string Type { get; set; }
		public virtual string Title { get; set; }
		public virtual string StoredProcedure { get; set; }
		public virtual string Header { get; set; }
		public virtual string Fields  { get; set; }
		public virtual ISet<ReportArguments> Arguments { get; set; }
	}
	
	public class ReportMap : ClassMap<DbReport>
	{
		public ReportMap()
		{
			Table("ReportScheduler");
			Id(x => x.Id);
			Map(x => x.Type);
			Map(x => x.Title);
			Map(x => x.StoredProcedure);
			Map(x => x.Header);
			Map(x => x.Fields);

			HasMany(x => x.Arguments).KeyColumn("ReportId");
		} 
	}

	public interface IReportRepository : IRepository<DbReport>
	{
	}

	public class ReportRepository : NHibernateRepositoryBase<DbReport>, IReportRepository
	{
		public ReportRepository(ISession session)
			: base(session)
		{
		}
	}
} // namespace
