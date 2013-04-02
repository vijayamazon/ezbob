namespace EzBob.CommonLib
{
	public interface IErrorRetryingWaiter
	{
		void Wait(double timeOutInMinutes);
		void Reset();
		void AssignHelper( WaitBeforeRetryHelper helper );
	}
}