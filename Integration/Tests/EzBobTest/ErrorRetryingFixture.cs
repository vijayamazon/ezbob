namespace EzBobTest
{
	using System;
	using EzBob.CommonLib;
	using NUnit.Framework;
	using Ezbob.Utils.Serialization;

	[TestFixture]
	public class ErrorRetryingFixture
	{
		[Test]
		public void Serialize()
		{
			var info = new ErrorRetryingInfo
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				}

			};

			Serialized.Serialize( @"d:/temp/ErrorRetryingInfo.xml", info );
		}

		[Test]
		public void WaitBeforeRetryController()
		{
			var info = new ErrorRetryingInfo
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				}

			};
			var retryingController = new WaitBeforeRetryController( new TestErrorRetryingWaiter( 60 ), info );

			retryingController.DoForAllTypesOfErrors( () => true );
		}

		[Test]
		public void WaitBeforeRetryControllerEnqabled()
		{
			var info = new ErrorRetryingInfo
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				}				
			};
			var retryingController = new WaitBeforeRetryController( new TestErrorRetryingWaiter( 60 ), info );

			int counter = 0;
			int maxCounter = 2;
			var rez = retryingController.DoForAllTypesOfErrors( () =>
				                                          {
															  if ( counter < maxCounter )
															  {
																  ++counter;
																  throw new TimeoutException();
															  }
															  else
															  {
																  return true;
															  }
				                                          });

			Assert.IsTrue( rez );
			Assert.AreEqual( counter, maxCounter );
		}

		[Test]
        //[ExpectedException(typeof(ArgumentException))]
		public void WaitBeforeRetryControllerEnqabled2()
		{
			var info = new ErrorRetryingInfo
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				},
			};
			var retryingController = new WaitBeforeRetryController( new TestErrorRetryingWaiter( 60 ), info );

			int counter = 0;
			int maxCounter = 2;
			bool rez = false;
			rez = retryingController.Do<bool>( () => {
						if ( counter < maxCounter ) {
							++counter;
							throw new TimeoutException();
						}

						throw new ArgumentException();
					}, ex => !(ex is ArgumentException));

			Assert.IsFalse( rez );
			Assert.AreEqual( counter, maxCounter );
		}

		[Test]
		//[ExpectedException(typeof(TimeoutException))]
		public void WaitBeforeRetryControllerDisabled()
		{
			var info = new ErrorRetryingInfo(false)
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				},
			};
			var retryingController = new WaitBeforeRetryController( new TestErrorRetryingWaiter( 60 ), info );

			int counter = 0;
			int maxCounter = 2;
			var rez = retryingController.DoForAllTypesOfErrors( () =>
			{
				if ( counter < maxCounter )
				{
					++counter;
					throw new TimeoutException();
				}
				else
				{
					return true;
				}
			} );

			Assert.AreEqual( counter, 1 );
		}

		[Test]
		public void WaitAndIncreaseWithoutLast()
		{
			var info = new ErrorRetryingInfo
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				},
			};
			var waiter = new TestErrorRetryingWaiter(60);
			var errRetrying = new WaitBeforeRetryHelper( waiter );

			WaitAndIncrease( info, errRetrying, waiter );

			bool needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsFalse( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 2 );
			Assert.AreEqual( errRetrying.CountRetrying, 3 );
			Assert.IsFalse( waiter.IsRegularWaiterRaised.HasValue );

			//------------------- end
			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsFalse( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 3 );
			Assert.AreEqual( errRetrying.CountRetrying, 0 );
			Assert.IsFalse( waiter.IsRegularWaiterRaised.HasValue );
		}

		[Test]
		public void WaitAndIncreaseWithLast()
		{
			var info = new ErrorRetryingInfo( true, 60, true)
			{
				Info = new[]
				{
					new ErrorRetryingItemInfo( 1, 3, 60),
					new ErrorRetryingItemInfo( 2, 3, 60),
					new ErrorRetryingItemInfo( 3, 3, 60),
				},
			};
			var waiter = new TestErrorRetryingWaiter(60);
			var errRetrying = new WaitBeforeRetryHelper( waiter );

			WaitAndIncrease( info, errRetrying, waiter );

			bool needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 2 );
			Assert.AreEqual( errRetrying.CountRetrying, 3 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue );
			Assert.IsFalse( waiter.IsRegularWaiterRaised.Value );

			//------------------- end
			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsFalse( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 3 );
			Assert.AreEqual( errRetrying.CountRetrying, 0 );			
			Assert.IsFalse( waiter.IsRegularWaiterRaised.HasValue );
		}

		private void WaitAndIncrease( ErrorRetryingInfo info, WaitBeforeRetryHelper errRetrying, TestErrorRetryingWaiter waiter )
		{

			bool needRetrying = false;
			Assert.AreEqual( errRetrying.CountIteration, 0 );
			Assert.AreEqual( errRetrying.CountRetrying, 0 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised == null );

			//var info = new ErrorRetryingInfo( 3, 3, 60 );

			//------------------- Iteration I
			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 0 );
			Assert.AreEqual( errRetrying.CountRetrying, 1 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && waiter.IsRegularWaiterRaised.Value );

			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 0 );
			Assert.AreEqual( errRetrying.CountRetrying, 2 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && waiter.IsRegularWaiterRaised.Value );

			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 0 );
			Assert.AreEqual( errRetrying.CountRetrying, 3 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && !waiter.IsRegularWaiterRaised.Value );

			//------------------- Iteration II
			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 1 );
			Assert.AreEqual( errRetrying.CountRetrying, 1 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && waiter.IsRegularWaiterRaised.Value );

			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 1 );
			Assert.AreEqual( errRetrying.CountRetrying, 2);
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && waiter.IsRegularWaiterRaised.Value );

			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 1 );
			Assert.AreEqual( errRetrying.CountRetrying, 3 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && !waiter.IsRegularWaiterRaised.Value );

			//------------------- Iteration I
			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 2 );
			Assert.AreEqual( errRetrying.CountRetrying, 1 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && waiter.IsRegularWaiterRaised.Value );

			needRetrying = errRetrying.IncreaseAndWait( info );

			Assert.IsTrue( needRetrying );
			Assert.AreEqual( errRetrying.CountIteration, 2 );
			Assert.AreEqual( errRetrying.CountRetrying, 2 );
			Assert.IsTrue( waiter.IsRegularWaiterRaised.HasValue && waiter.IsRegularWaiterRaised.Value );			
		}
	}

	internal class TestErrorRetryingWaiter : ErrorRetryingWaiterBase
	{
		private readonly int _RegularTimeoutInSeconds;

		public TestErrorRetryingWaiter(int regularTimeoutInSeconds)
		{
			_RegularTimeoutInSeconds = regularTimeoutInSeconds;
		}

		public override void Wait(double timeOutInMinutes)
		{
			IsRegularWaiterRaised = timeOutInMinutes == _RegularTimeoutInSeconds / 60d;
		}

		public override void Reset()
		{
			IsRegularWaiterRaised = null;
		}

		public bool? IsRegularWaiterRaised { get; private set; }
	}
}
