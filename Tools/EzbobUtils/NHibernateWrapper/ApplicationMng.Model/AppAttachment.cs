using System;
namespace ApplicationMng.Model
{
	public class AppAttachment
	{
		public virtual long Id
		{
			get;
			set;
		}
		public virtual long DetailId
		{
			get;
			set;
		}
		public virtual byte[] Document
		{
			get;
			set;
		}
		public AppAttachment(long detailId, byte[] document)
		{
			this.DetailId = detailId;
			this.Document = document;
		}
		public AppAttachment()
		{
		}
		public AppAttachment(long detailId)
		{
			this.DetailId = detailId;
		}
	}
}
