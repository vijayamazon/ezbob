using System;
using System.Runtime.Serialization;
using System.Text;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public class CreateApp : CommandBase
	{
		private long _appId;
		public virtual string StrategyName
		{
			get;
			set;
		}
		public override void Execute(IContext context)
		{
			try
			{
				Application application = context.CreateApplication(this.StrategyName, this.User);
				this.App = application;
				this.Result = 0;
				this._appId = application.Id;
			}
			catch (System.Exception ex)
			{
				this.ErrorDescritpion = ex.Message;
			}
		}
		protected override void AppendCustomResult(System.Text.StringBuilder sb)
		{
			sb.AppendFormat("AppId: {0},", this._appId);
			if (this.App != null)
			{
				sb.AppendFormat("AppCnt: {0},", this.App.AppCounter);
			}
		}
		public override string GetName()
		{
			return "Create";
		}
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("StrategyName", this.StrategyName);
			base.GetObjectData(info, context);
		}
		public CreateApp()
		{
		}
		protected CreateApp(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			ISerializationHelper serializationHelper = (ISerializationHelper)context.Context;
			this.StrategyName = info.GetString("StrategyName");
		}
	}
}
