using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public class SaveApp : CommandBase
	{
		public virtual string ParamsXml
		{
			get;
			set;
		}
		public virtual AppStatus Status
		{
			get;
			set;
		}
		public virtual string ControlName
		{
			get;
			set;
		}
		public virtual string FormName
		{
			get;
			set;
		}
		public override void Execute(IContext context)
		{
			if (!base.IsAppReady(this.Status))
			{
				this.Result = 1;
			}
			else
			{
				context.SaveApplication(this.App, this.ParamsXml, this.User, this.SecApp, this.SignatureRequired, this.OutletName, this.ControlName, this.ItemsToBeSigned);
				this.Result = 0;
			}
		}
		public override string GetName()
		{
			return "Save";
		}
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("ParamsXml", this.ParamsXml);
			info.AddValue("Status", (this.Status == null) ? -1 : this.Status.Id);
			base.GetObjectData(info, context);
		}
		public SaveApp()
		{
		}
		protected SaveApp(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			ISerializationHelper serializationHelper = (ISerializationHelper)context.Context;
			this.ParamsXml = info.GetString("ParamsXml");
			this.Status = serializationHelper.GetStatus(info.GetInt32("Status"));
		}
	}
}
