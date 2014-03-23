using ApplicationMng.Model;
using FluentNHibernate.Mapping;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.NHibernate.Mappings
{
	public class NodeMap : ClassMap<ExportJournal>
	{
		public NodeMap()
		{
			base.LazyLoad();
			base.Table("Export_OperationJournal");
			this.Id((ExportJournal x) => (object)x.Id).GeneratedBy.Native("SEQ_Export_OperationJournal").Column("Id");
			base.References<User>((ExportJournal x) => x.User).Column("UserId").LazyLoad();
			base.Map((ExportJournal x) => (object)x.ActionDateTime);
			base.Map((ExportJournal x) => x.BinaryBody).Length(2147483647).LazyLoad();
			base.Map((ExportJournal x) => x.ContentName).Length(255);
			base.Map((ExportJournal x) => x.ContentType).Length(10);
			base.Map((ExportJournal x) => x.OperationType).Length(6);
			base.Map((ExportJournal x) => (object)x.JournalType);
		}
	}
}
