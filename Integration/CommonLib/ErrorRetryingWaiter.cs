using System.Threading;

namespace EzBob.CommonLib
{
	public class ErrorRetryingWaiter : ErrorRetryingWaiterBase
	{
		public override void Wait(double timeOutInMinutes)
		{
			if ( timeOutInMinutes <= 0 )
			{
				return;
			}
			Thread.Sleep( (int)(timeOutInMinutes * 60 * 1000) );
		}
		
	}
}