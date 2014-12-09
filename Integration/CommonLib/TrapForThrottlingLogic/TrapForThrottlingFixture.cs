using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace EzBob.CommonLib.TrapForThrottlingLogic
{
	[TestFixture]
	class TrapForThrottlingFixture
	{
		[Test]
		public void TrapForThrottlingRunInterval()
		{
			var t1 = new TrapForThrottlingSimpleWait( "test", 1, RequestQuoteTimePeriodType.PerSecond );
			Assert.AreEqual( 1, t1.RunWithIntervalInSeconds.TotalSeconds );
			var t2 = new TrapForThrottlingSimpleWait( "test", 6, RequestQuoteTimePeriodType.PerMinute );
			Assert.AreEqual( 10, t2.RunWithIntervalInSeconds.TotalSeconds );

			var t3 = new TrapForThrottlingSimpleWait( "test", 10, RequestQuoteTimePeriodType.PerSecond );
			Assert.AreEqual( 100, t3.RunWithIntervalInSeconds.TotalMilliseconds );
			var t4 = new TrapForThrottlingSimpleWait( "test", 60, RequestQuoteTimePeriodType.PerMinute );
			Assert.AreEqual( 1, t4.RunWithIntervalInSeconds.TotalSeconds );
		}

		[Test]
		public void TrapForThrottlingSimple()
		{
			var t = new TrapForThrottlingSimpleWait( "test", 6, RequestQuoteTimePeriodType.PerSecond );
			//var t = new TrapForThrottlingSimpleWait( "test", 6, CommonLib.TrapForThrottlingSimpleWait.RequestQuoteTimePeriodType.PerMinute );

			var tasks = new List<Task>();

			Debug.WriteLine( string.Format( "[{0}] Start", DateTime.Now ) );

			for (int i = 0; i < 10; i++)
			{
				var name = string.Format( "Action #{0}", i + 1 );

				tasks.Add( Task.Factory.StartNew( () => t.Execute( new ActionInfo(name)
																	{
																		Action = () => { },
																		Access = ActionAccessType.Full,																		
																	} ) ) );
			}

			Task.WaitAll( tasks.ToArray() );
			Debug.WriteLine( string.Format( "[{0}] Finish", DateTime.Now ) );
		}

		[Test]
		public void TestMethod()
		{		
			int requestQuota = 15;
			int countRequests = (int)(requestQuota/2d * 1.4);
			var tasks = new List<Task>();
			using ( var t = TrapForThrottlingController.Create( "test", requestQuota, 5 ) )
			{
				var localTrapForThrottling = t;
				for (int i = 0; i < countRequests; i++)
				{
					var counter = i + 1;
					tasks.Add(new Task(() => localTrapForThrottling.Execute(CreateAction(string.Format("Action #{0}", counter + 1)))));
					tasks.Add(new Task(() => localTrapForThrottling.Execute(CreateAction(string.Format("Limit Action #{0}", counter + 1), ActionAccessType.Limit))));
					//Thread.Sleep( 10 * 1000 );
				}
				//Thread.Sleep( requestQuota * 90 * 1000 );
				tasks.Add(new Task(() => localTrapForThrottling.Execute(CreateAction(string.Format("Action #{0}", countRequests + 1)))));

				//Thread.Sleep( requestQuota / 3 * 90 * 1000 );
				tasks.Add(new Task(() => localTrapForThrottling.Execute(CreateAction(string.Format("Action #{0}", countRequests + 2)))));
				//Thread.Sleep( 5 * 60 * 1000 );
				tasks.ForEach( t1 => t1.Start() );
				tasks.ForEach( t1 => t1.Wait() );
				TrapForThrottlingController.Exit();
			}
		}

		[Test]
		public void TestMethod2()
		{
			for (int i = 0; i < 10; i++)
			{
				var t = TrapForThrottlingController.Create( string.Format("test {0}", i + 1), 15 );
				t.Execute( CreateAction() );
			}

			var tShort = TrapForThrottlingController.Create( "test short", 15, 1 );
			tShort.Execute( CreateAction() );
			Thread.Sleep( 5000 );
			TrapForThrottlingController.Exit();
		}

		private ActionInfo CreateAction(string name = "Action",  ActionAccessType accessType = ActionAccessType.Full )
		{
			return new ActionInfo(name)
				{					
					Action = () => { },
					Access = accessType
				};
		}

	}
}
