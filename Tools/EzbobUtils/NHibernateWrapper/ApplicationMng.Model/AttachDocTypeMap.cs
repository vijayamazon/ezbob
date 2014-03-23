using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model
{
	public class AttachDocTypeMap : ClassMap<AttachDocType>
	{
		public AttachDocTypeMap()
		{
			base.Cache.ReadWrite().Region("LongTerm");
			this.Id((AttachDocType x) => (object)x.Id).Column("AttachmentTypeId").GeneratedBy.Native("SEQ_APP_ATTACHMENT");
			base.Map((AttachDocType x) => x.Name, "AttachmentType").Length(512);
			base.Map((AttachDocType x) => x.Group, "AttachmentGroup").Length(512);
			base.Table("App_Attach_DocType");
		}
	}
}
