using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class ExperianConsentAgreementMap: ClassMap<ExperianConsentAgreement>
    {
        public ExperianConsentAgreementMap()
        {
            Table("ExperianConsentAgreement");
            Id(x => x.Id).GeneratedBy.Native().Column("Id");
            Map(x => x.Template).CustomType("StringClob");
            Map(x => x.CustomerId);
            Map(x => x.FilePath);
        }
    }
}
