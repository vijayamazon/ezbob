using System;
namespace ApplicationMng.Model.Commands
{
	public interface ICommand
	{
		void Execute(IContext context);
	}
}
