using System;

namespace EzBob.CommonLib
{
	public interface ITrapForThrottling : IDisposable
	{
		T Execute<T>( Func<T> func, string actionName );
		void Execute( ActionInfo actionInfo );
		void Exit();
	}
}