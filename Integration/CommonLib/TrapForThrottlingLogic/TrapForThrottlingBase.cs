namespace EzBob.CommonLib.TrapForThrottlingLogic
{
	using System;
	using System.Diagnostics;

	abstract class TrapForThrottlingBase : ITrapForThrottling
	{
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