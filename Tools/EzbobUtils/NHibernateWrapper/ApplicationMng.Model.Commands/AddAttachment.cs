using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public class AddAttachment : CommandBase
	{
		public virtual int UserId
		{
			get;
			set;
		}
		public virtual string Description
		{
			get;
			set;
		}
		public virtual string DocType
		{
			get;
			set;
		}
		public virtual string FileName
		{
			get;
			set;
		}
		public virtual string AttachControlName
		{
			get;
			set;
		}
		public virtual byte[] Body
		{
			get;
			set;
		}
		public override void Execute(IContext context)
		{
			if (!base.IsAppReady(null))
			{
				this.Result = 1;
			}
			else
			{
				context.AddAttachment(this.FileName, this.DocType, this.Description, this.Body, this.App.Id, this.AttachControlName, this.UserId);
				this.Result = 0;
			}
		}
		public override string GetName()
		{
			return "AddAtachment";
		}
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Description", this.Description);
			info.AddValue("DocType", this.DocType);
			info.AddValue("FileName", this.FileName);
			info.AddValue("AttachControlName", this.AttachControlName);
			info.AddValue("Body", this.Body);
		}
		public AddAttachment()
		{
		}
		protected AddAttachment(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			this.Description = info.GetString("Description");
			this.DocType = info.GetString("DocType");
			this.FileName = info.GetString("FileName");
			this.AttachControlName = info.GetString("AttachControlName");
			this.Body = (byte[])info.GetValue("Body", typeof(byte[]));
		}
		public override CommandBase Clone()
		{
			this.Body.Length.ToString();
			return base.Clone();
		}
	}
}
