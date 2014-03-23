using System;
using System.Runtime.Serialization;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public class SubmitApp : CommandBase
	{
		public virtual string ParamsXml
		{
			get;
			set;
		}
		public virtual string Outlet
		{
			get;
			set;
		}
		public virtual AppStatus Status
		{
			get;
			set;
		}
		public override void Execute(IContext context)
		{
			try
			{
				if (!base.IsAppReady(this.Status))
				{
					this.Result = 1;
				}
				else
				{
					context.SubmitApplication(this.App, this.ParamsXml, this.Outlet, this.User, this.SecApp, this.ItemsToBeSigned, this.SignatureRequired);
					this.ErrorDescritpion = string.Empty;
					this.Result = 0;
				}
			}
			catch (System.Exception ex)
			{
				this.ErrorDescritpion = ex.Message;
				this.Result = -1;
			}
		}
		public override string GetName()
		{
			return string.Format("Submit ({0}, {1})", this.NodeName, this.Outlet);
		}
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("ParamsXml", this.ParamsXml);
			info.AddValue("Outlet", this.Outlet);
			info.AddValue("Status", (this.Status == null) ? -1 : this.Status.Id);
			base.GetObjectData(info, context);
		}
		public SubmitApp()
		{
		}
		protected SubmitApp(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			ISerializationHelper serializationHelper = (ISerializationHelper)context.Context;
			this.ParamsXml = info.GetString("ParamsXml");
			this.Outlet = info.GetString("Outlet");
			this.Status = serializationHelper.GetStatus(info.GetInt32("Status"));
		}
	}
}
