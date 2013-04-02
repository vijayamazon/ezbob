using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class ScoringResultMap : ClassMap<ScoringResult>
    {
        public ScoringResultMap()
        {
            Table("CustomerScoringResult");
            Id(x => x.Id).GeneratedBy.Native().Column("Id");
            Map(x => x.ACDescription, "AC_Descriptors");
            Map(x => x.ACParameters, "AC_Parameters");
            Map(x => x.Weights, "Result_Weights");
            Map(x => x.MAXPossiblePoints, "Result_MAXPossiblePoints");
            Map(x => x.CustomerId);
            Map(x => x.ScoreDate);
            Map(x => x.Medal);
            Map(x => x.ScorePoints);
            Map(x => x.ScoreResult);
        }
    }
}
