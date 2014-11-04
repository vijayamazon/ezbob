namespace EZBob.DatabaseLib.Model.Database
{
	using FluentNHibernate.Mapping;

	public class MedalCalculationsMap : ClassMap<MedalCalculations>
    {
		public MedalCalculationsMap()
        {
			Table("MedalCalculations");
			Id(x => x.Id).GeneratedBy.Native().Column("Id");
			Map(x => x.IsActive);
			Map(x => x.CustomerId);
            Map(x => x.Medal);
			Map(x => x.TotalScoreNormalized);
			Map(x => x.Error).Length(500);
        }
    }
}
