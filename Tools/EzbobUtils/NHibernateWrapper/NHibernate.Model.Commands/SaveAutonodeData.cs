using ApplicationMng.Model.Commands;
namespace NHibernateWrapper.NHibernate.Model.Commands
{
	using StructureMap;
	using IContext = global::ApplicationMng.Model.Commands.IContext;

	[System.Serializable]
	public class SaveAutonodeData : CommandBase
	{
		private string _outlet;
		private IPlayerRunner _playerRunner;
		public virtual string Outlet
		{
			get
			{
				return this._outlet;
			}
			set
			{
				this._outlet = value;
			}
		}
		public virtual string Outparams
		{
			get
			{
				return this.PlayerRunner.Outparams;
			}
			set
			{
				this.PlayerRunner.Outparams = value;
			}
		}
		public virtual IPlayerRunner PlayerRunner
		{
			get
			{
				IPlayerRunner arg_19_0;
				if ((arg_19_0 = this._playerRunner) == null)
				{
					arg_19_0 = (this._playerRunner = ObjectFactory.GetInstance<IPlayerRunner>());
				}
				return arg_19_0;
			}
		}
		public override void Execute(IContext context)
		{
			try
			{
				if (!base.IsAppReady(null))
				{
					this.Result = 1;
				}
				else
				{
					this.PlayerRunner.Execute(context, this);
					this.Result = 0;
				}
			}
			catch (System.Exception ex)
			{
				this.ErrorDescritpion = ex.Message;
				this.Result = -1;
			}
		}
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("Outlet", this.Outlet);
			info.AddValue("Outparams", this.Outparams);
			base.GetObjectData(info, context);
		}
		public override string GetName()
		{
			return string.Format("Execute autonode ({0}, {1})", this.NodeName, this.Outlet);
		}
	}
}
