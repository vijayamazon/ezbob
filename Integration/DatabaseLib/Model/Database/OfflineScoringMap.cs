namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;

	public class OfflineScoringMap : ClassMap<OfflineScoring>
    {
		public OfflineScoringMap()
        {
			Table("OfflineScoring");
			Id(x => x.Id).GeneratedBy.Native().Column("Id");
			Map(x => x.IsActive);
			Map(x => x.CustomerId);
            Map(x => x.Medal);
            Map(x => x.TotalScoreNormalized);
        }
    }
}
