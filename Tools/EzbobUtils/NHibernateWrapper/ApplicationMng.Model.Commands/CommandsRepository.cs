using ApplicationMng.Repository;
using NHibernate;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Model.Commands
{
	public class CommandsRepository : NHibernateRepositoryBase<CommandBase>
	{
		public CommandsRepository(ISession session) : base(session)
		{
		}
		public new void SaveOrUpdate(CommandBase cmd)
		{
			using (ITransaction transaction = this._session.BeginTransaction())
			{
				this._session.SaveOrUpdate(cmd);
				transaction.Commit();
			}
		}
		public void SaveOrUpdate(System.Collections.Generic.IEnumerable<CommandBase> cmds)
		{
			using (ITransaction transaction = this._session.BeginTransaction())
			{
				foreach (CommandBase current in cmds)
				{
					this._session.SaveOrUpdate(current);
				}
				transaction.Commit();
			}
		}
		public void SaveOrUpdate(CommandsList cmds)
		{
			using (ITransaction transaction = this._session.BeginTransaction())
			{
				this._session.SaveOrUpdate(cmds);
				transaction.Commit();
			}
		}
		public System.Collections.Generic.IList<CommandBase> GetCommands()
		{
			return this._session.CreateCriteria(typeof(CommandBase)).List<CommandBase>();
		}
		public System.Collections.Generic.IList<CommandBase> GetCommands(string guid)
		{
			System.Collections.Generic.IList<CommandBase> commands = null;
			this.EnsureTransaction(() => commands = this._session.Get<CommandsList>(guid).Commands);
			return commands;
		}
		public CommandsList GetCommandsList(string guid)
		{
			return this._session.Get<CommandsList>(guid);
		}
		public System.Collections.Generic.IList<CommandsList> GetCommandsLists()
		{
			return this._session.CreateCriteria(typeof(CommandsList)).List<CommandsList>();
		}
	}
}
