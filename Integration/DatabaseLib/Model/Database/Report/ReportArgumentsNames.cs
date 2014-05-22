namespace EZBob.DatabaseLib.Model.Database.Report {
	using FluentNHibernate.Mapping;
	
	[System.Serializable]
	public class ReportArgumentNames {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
	
	public class ReportArgumentNamesMap : ClassMap<ReportArgumentNames>
	{
		public ReportArgumentNamesMap()
		{
			Table("ReportArgumentNames");
			Id(x => x.Id);
			Map(x => x.Name);
		} 
	}

} // namespace
