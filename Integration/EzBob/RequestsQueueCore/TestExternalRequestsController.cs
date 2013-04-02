using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EzBob.RequestsQueueCore.RequestInfos;
using NUnit.Framework;

namespace EzBob.RequestsQueueCore
{
	public class TestExternalRequestsController
	{
		[Test]
		public void Test1()
		{
			var c = new ExternalRequestsController();

			var mpInfo1 = new MarketplaceInfoTest( "Test1" );
			var mpInfo2 = new MarketplaceInfoTest( "Test2" );
			

			var mp1 = new DatabaseMarketplaceTest( mpInfo1 );
			var mp2 = new DatabaseMarketplaceTest( mpInfo2 );

			var requestData11 = RequestInfoFactory.CreateSingleRequest( mp1, ProcException, "Test Single - MP1 - ProcException" );
			var requestData12 = RequestInfoFactory.CreateSingleRequest( mp1, ProcWait, "Test Single - MP1 - ProcWait" );
			var requestData13 = RequestInfoFactory.CreateSingleRequest( mp1, ProcSucess, "Test Single - MP1 - ProcSucess" );
			var requestData21 = RequestInfoFactory.CreateSingleRequest( mp2, ProcException, "Test Single - MP2 - ProcException" );
			var requestData22 = RequestInfoFactory.CreateSingleRequest( mp2, ProcWait, "Test Single - MP2 - ProcWait" );
			var requestData23 = RequestInfoFactory.CreateSingleRequest( mp2, ProcSucess, "Test Single - MP2 - ProcSucess" );
			
			var r11 = c.CreateAndExecuteRequest( requestData11 );
			var r12 = c.CreateAndExecuteRequest( requestData12 );
			var r13 = c.CreateAndExecuteRequest( requestData13 );
			var r21 = c.CreateAndExecuteRequest( requestData21 );
			var r22 = c.CreateAndExecuteRequest( requestData22 );
			var r23 = c.CreateAndExecuteRequest( requestData23 );

			var r3 = c.CreateAndExecuteRequest( RequestInfoFactory.CreateCompositeRequest( new[] 
			                                                               	{ 
			                                                               		RequestInfoFactory.CreateSingleRequest( mp1, ProcException, "TestComposite - MP1 - ProcException" ),
			                                                               		RequestInfoFactory.CreateSingleRequest( mp1, ProcWait, "TestComposite - MP1 - ProcWait" ),
			                                                               		RequestInfoFactory.CreateSingleRequest( mp1, ProcSucess, "TestComposite - MP1 - ProcSucess" ),
			                                                               		RequestInfoFactory.CreateSingleRequest( mp2, ProcException, "TestComposite - MP2 - ProcException" ),
			                                                               		RequestInfoFactory.CreateSingleRequest( mp2, ProcWait, "TestComposite - MP2 - ProcWait" ),
			                                                               		RequestInfoFactory.CreateSingleRequest( mp2, ProcSucess, "TestComposite - MP2 - ProcSucess" ),
			                                                               	} ) );
			var states = c.WaitAll( new[] { r11, r12, r13, r21, r22, r23, r3 } );

			Assert.IsTrue( states.All( s => s.IsDone() ) );

			c.Exit();
		}

		private void ProcException()
		{
			Debug.WriteLine( string.Format( "[{0}] Execution: {1}", DateTime.Now, "ProcException" ) );
			throw new Exception("Test Error!");
		}

		private void ProcWait()
		{
			Debug.WriteLine( string.Format( "[{0}] {1}", DateTime.Now, "ProcWait - begin" ) );
			Thread.Sleep( 5 * 1000 );
			Debug.WriteLine( string.Format( "[{0}] {1}", DateTime.Now, "ProcWait - end" ) );
		}

		private void ProcSucess()
		{
			Debug.WriteLine( string.Format( "[{0}] Execution: {1}", DateTime.Now, "ProcSucess" ) );
		}
	}
}