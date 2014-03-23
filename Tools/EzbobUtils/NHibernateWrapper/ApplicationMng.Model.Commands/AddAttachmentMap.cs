using FluentNHibernate.Mapping;
using System;
namespace ApplicationMng.Model.Commands
{
	public class AddAttachmentMap : SubclassMap<AddAttachment>
	{
		public AddAttachmentMap()
		{
			base.Map((AddAttachment x) => x.Description, "AttachDescription");
			base.Map((AddAttachment x) => x.DocType, "AttachDocType");
			base.Map((AddAttachment x) => x.FileName, "AttachFileName");
			base.Map((AddAttachment x) => x.Body, "AttachBody").Length(2147483647);
			base.Map((AddAttachment x) => x.AttachControlName, "AttachControlName");
		}
	}
}
