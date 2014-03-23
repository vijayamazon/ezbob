using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model
{
	public class PublicNameMap : ClassMap<PublicName>
	{
		public PublicNameMap()
		{
			base.LazyLoad();
			base.Cache.Region("VeryShort").ReadWrite();
			base.Table("Strategy_PublicName");
			this.Id((PublicName x) => (object)x.Id).GeneratedBy.Native().Column("PUBLICNAMEID");
			base.Map((PublicName x) => x.Name).Length(255);
			base.Map((PublicName x) => (object)x.IsStopped);
			base.Map((PublicName x) => (object)x.IsDeleted);
			base.Map((PublicName x) => (object)x.TerminationDate);
			base.HasMany<PublicNameStrategy>((PublicName x) => x.PublicNameStrategies).AsSet().KeyColumn("PUBLICID").Cascade.All().Inverse();
		}
	}
}
