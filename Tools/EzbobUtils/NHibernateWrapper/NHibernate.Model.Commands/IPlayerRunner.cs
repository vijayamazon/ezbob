using ApplicationMng.Model.Commands;
using System;
namespace NHibernateWrapper.NHibernate.Model.Commands
{
	public interface IPlayerRunner
	{
		string Outparams
		{
			get;
			set;
		}
		void Execute(IContext context, CommandBase command);
	}
}
