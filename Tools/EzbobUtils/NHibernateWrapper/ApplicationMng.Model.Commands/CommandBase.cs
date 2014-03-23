using ApplicationMng.Repository;
using log4net;
using Scorto.Configuration;
using StructureMap;
using System;
using System.Runtime.Serialization;
using System.Text;
namespace ApplicationMng.Model.Commands
{
	[System.Serializable]
	public abstract class CommandBase : ICommand, System.Runtime.Serialization.ISerializable, System.ICloneable
	{
		private readonly ILog log = LogManager.GetLogger(typeof(CommandBase));
		private IRepository<User> Users
		{
			get
			{
				return ObjectFactory.GetInstance<IRepository<User>>();
			}
		}
		public virtual int Id
		{
			get;
			set;
		}
		public virtual SecurityApplication SecApp
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual User User
		{
			get
			{
				return this.Users.Get(ConfigurationRoot.GetConfiguration().HostUserId);
			}
		}
		public virtual int Position
		{
			get;
			set;
		}
		public virtual int Result
		{
			get;
			set;
		}
		protected virtual string ErrorDescritpion
		{
			get;
			set;
		}
		public virtual bool SignatureRequired
		{
			get;
			set;
		}
		public virtual string OutletName
		{
			get;
			set;
		}
		public virtual string ItemsToBeSigned
		{
			get;
			set;
		}
		public virtual string NodeName
		{
			get;
			set;
		}
		public abstract void Execute(IContext context);
		protected CommandBase()
		{
			this.Result = -1;
		}
		public abstract string GetName();
		public virtual string GetJsonResult()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append("{");
			this.AppendCustomResult(stringBuilder);
			stringBuilder.AppendFormat("Result: {0},", this.Result);
			if (!string.IsNullOrEmpty(this.ErrorDescritpion))
			{
				stringBuilder.AppendFormat("Error: '{0}',", this.ErrorDescritpion);
			}
			if (this.App != null)
			{
				stringBuilder.AppendFormat("State: '{0}',", (int)this.App.State);
			}
			if (this.App != null && this.App.AdditionalData.Status != null)
			{
				stringBuilder.AppendFormat("Status: '{0} ({1})',", this.App.AdditionalData.Status.Name, this.App.AdditionalData.Status.Description);
			}
			if (this.App != null && this.App.ExecutionState != null && this.App.ExecutionState.CurrentNode != null)
			{
				stringBuilder.AppendFormat("Node: '{0} ({1})',", this.App.ExecutionState.Id, this.App.ExecutionState.CurrentNode.Name);
			}
			stringBuilder.Append("fake: 'fake'");
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}
		protected virtual void AppendCustomResult(System.Text.StringBuilder sb)
		{
		}
		protected bool IsAppReady(AppStatus status)
		{
			bool result;
			try
			{
				if (this.App.State == ApplicationStrategyState.Error)
				{
					throw new System.Exception(string.Format("Application {0} is in error state.", this.App.Id));
				}
				if (this.App.State != ApplicationStrategyState.NeedProcessByNode)
				{
					result = false;
				}
				else
				{
					if (status != null && (this.App.AdditionalData.Status == null || this.App.AdditionalData.Status != status))
					{
						result = false;
					}
					else
					{
						if (this.App.Locker != null)
						{
							result = false;
						}
						else
						{
							if (this.App.ExecutionState == null || this.App.ExecutionState.CurrentNode == null)
							{
								result = false;
							}
							else
							{
								result = true;
							}
						}
					}
				}
			}
			catch (System.Exception message)
			{
				this.log.Warn(message);
				throw;
			}
			return result;
		}
		public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("SecApp", (this.SecApp == null) ? -1 : this.SecApp.Id);
			info.AddValue("Position", this.Position);
			info.AddValue("SignatureRequired", this.SignatureRequired);
			info.AddValue("ItemsToBeSigned", this.ItemsToBeSigned);
			info.AddValue("OutletName", this.OutletName);
			info.AddValue("NodeName", this.NodeName);
		}
		protected CommandBase(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			this.Result = -1;
			ISerializationHelper serializationHelper = (ISerializationHelper)context.Context;
			this.SecApp = serializationHelper.GetSecApp(info.GetInt32("SecApp"));
			this.Position = info.GetInt32("Position");
			this.SignatureRequired = info.GetBoolean("SignatureRequired");
			this.ItemsToBeSigned = info.GetString("ItemsToBeSigned");
			this.OutletName = info.GetString("OutletName");
			this.NodeName = info.TryGetString("NodeName");
		}
		object System.ICloneable.Clone()
		{
			return this.Clone();
		}
		public virtual CommandBase Clone()
		{
			return (CommandBase)base.MemberwiseClone();
		}
	}
}
