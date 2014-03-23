using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Mappings
{
	public class AppAttachmentMap : ClassMap<AppAttachment>
	{
		public AppAttachmentMap()
		{
			base.Table("Application_Attachment");
			this.Id((AppAttachment x) => (object)x.Id, "AttachmentId").GeneratedBy.Native("SEQ_APP_ATTACHMENT");
			base.Map((AppAttachment x) => (object)x.DetailId, "DetailId");
			base.Map((AppAttachment x) => x.Document, "Value").Length(2147483647).LazyLoad();
		}
	}
}
