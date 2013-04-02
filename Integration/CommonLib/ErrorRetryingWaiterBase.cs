namespace EzBob.CommonLib
{
	public abstract class ErrorRetryingWaiterBase : IErrorRetryingWaiter
	{
		public abstract void Wait(double timeOutInMinutes);

		public virtual void Reset()
		{

		}

		public void AssignHelper(WaitBeforeRetryHelper helper)
		{
			Helper = helper;
		}

		protected WaitBeforeRetryHelper Helper { get; private set; }
	}
}