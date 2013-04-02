using System;
using System.Diagnostics;
using log4net;

namespace EzBob.CommonLib.TrapForThrottlingLogic
{
	abstract class TrapForThrottlingBase : ITrapForThrottling
	{
		private static readonly ILog _Log = LogManager.GetLogger( typeof( TrapForThrottlingBase ) );

		protected string Name { get; private set; }

		protected TrapForThrottlingBase(string name)
		{
			Name = name;
		}

		public abstract T Execute<T>( Func<T> func, string actionName );

		public abstract void Execute(ActionInfo actionInfo);

		public abstract void Exit();
		
		public virtual void Dispose()
		{
			Exit();
		}

		protected void WriteToLog( string message, WriteLogType messageType = WriteLogType.Debug, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}