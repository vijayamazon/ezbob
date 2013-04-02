using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
   public  class PersonalInfoHistoryMap : ClassMap<PersonalInfoHistory> 
    {
       public PersonalInfoHistoryMap()
       {
           Table("PersonalInfoHistory");
           Id(x => x.Id).GeneratedBy.Native().Column("Id");
           Map(x => x.FieldName).Length(100);
           Map(x => x.OldValue).Length(100);
           Map(x => x.NewValue).Length(100);
           Map(x => x.AddressId);
           Map(x => x.DateModifed).CustomType<UtcDateTimeType>();
           References(x => x.Customer, "CustomerId").Not.Nullable();
       }
     }
}
