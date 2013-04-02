using System;

namespace EzBob.CommonLib
{
	public class WaitBeforeRetryController
	{
		private readonly IErrorRetryingWaiter _Waiter;
		private readonly ErrorRetryingInfo _ErrorRetryingInfo;

		public WaitBeforeRetryController( IErrorRetryingWaiter waiter, ErrorRetryingInfo errorRetryingInfo)
		{
			_Waiter = waiter;
			_ErrorRetryingInfo = errorRetryingInfo ;			
		}

		public T DoForAllTypesOfErrors<T>( Func<T> func )
		{
			return Do( func, ex => true );
		}

		public T Do<T>(Func<T> func, Func<Exception, bool> isNeedToContinueRetrying)
		{
			bool needRetry;
			T rez = default( T );
			WaitBeforeRetryHelper waitBeforeRetryHelper = null;
			do
			{
				try
				{
					needRetry = false;
					rez = func();
				}
				catch ( Exception ex )
				{
					if ( _ErrorRetryingInfo == null || !_ErrorRetryingInfo.EnableRetrying )
					{
						throw;
					}

					needRetry = isNeedToContinueRetrying( ex );

					if ( needRetry )
					{
						if (waitBeforeRetryHelper == null)
						{
							waitBeforeRetryHelper = new WaitBeforeRetryHelper(_Waiter);
						}

						needRetry = waitBeforeRetryHelper.IncreaseAndWait(_ErrorRetryingInfo);

						if (!needRetry)
						{
							throw;
						}
					}
					else					
					{
						throw;
					}
				}

			} while ( needRetry );

			return rez;
		}
	}
}