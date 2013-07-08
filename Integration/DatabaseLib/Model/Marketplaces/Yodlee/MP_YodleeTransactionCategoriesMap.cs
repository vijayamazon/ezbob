using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
    public class MP_YodleeTransactionCategoriesMap : ClassMap<MP_YodleeTransactionCategories>
    {
        public MP_YodleeTransactionCategoriesMap()
        {
            Table("MP_YodleeTransactionCategories");
            Id(x => x.CategoryId).Length(3);
            Map(x => x.Name).Length(300);
            Map(x => x.Type).Length(300);
        }
    }
}