using System;
using System.Threading;
using System.Threading.Tasks;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestInfos
{
	internal class RequestDataSingle : RequestDataBase
	{	
		private readonly IMarketplaceType _Marketplace;
		private readonly Action _Action;

		public static IRequestData Create(IMarketplaceType marketplace, Action action, string name)
		{
			return new RequestDataSingle( marketplace, action, name );
		}

		private RequestDataSingle(IMarketplaceType marketplace, Action action, string name)
			:base(name)
		{
			_Action = action;
			_Marketplace = marketplace;
		}

		protected override IRequestHandle CreateRequestInternal(RequestsManagerStorage storageManagers)
		{
			var manager = storageManagers.GetManager( _Marketplace );
			return manager.CreateRequest( this );
		}

		public override IRequestState ExecuteRequest(RequestsManagerStorage storageManagers)
		{
			var manager = storageManagers.GetManager( _Marketplace );
			return manager.ExecuteRequest( Handle );
		}

		public override IRequestState GetRequestState(RequestsManagerStorage storageManagers)
		{
			var manager = storageManagers.GetManager( _Marketplace );
			return manager.GetState( Handle );
		}

		public override IRequestState CancelRequest( RequestsManagerStorage storageManagers )
		{
			var manager = storageManagers.GetManager( _Marketplace );
			return manager.CancelRequest( Handle );
		}
		
		public override Task CreateTask(CancellationToken cancelationToken)
		{
			return new Task( _Action, cancelationToken );
		}		
	}
}