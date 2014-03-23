using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class DataSourceMap : ClassMap<DataSource>
	{
		public DataSourceMap()
		{
			base.Cache.Region("LongTerm").ReadWrite();
			base.Table("DataSource_Sources");
			this.Id((DataSource x) => (object)x.Id).GeneratedBy.Native("SEQ_DATASOURCE_SOURCE");
			base.Map((DataSource x) => x.Name).Length(255);
			base.Map((DataSource x) => x.DisplayName).Length(255);
			base.Map((DataSource x) => x.Description).Length(500);
			base.Map((DataSource x) => x.Type).Length(20);
			base.Map((DataSource x) => (object)x.IsDeleted);
			base.Map((DataSource x) => (object)x.CreationDate);
			base.Map((DataSource x) => (object)x.TerminationDate);
			base.References<User>((DataSource x) => x.User, "UserId");
			base.Map((DataSource x) => x.Document).CustomType("StringClob").LazyLoad();
			base.Map((DataSource x) => x.SignedDocument, "SignedDocument").CustomType("StringClob").LazyLoad();
			base.Map((DataSource x) => x.SignedDocumentDelete, "SignedDocumentDelete").CustomType("StringClob").LazyLoad();
		}
	}
}
