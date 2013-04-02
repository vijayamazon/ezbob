using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.Handle;
using EzBob.RequestsQueueCore.RequestStates;

namespace EzBob.RequestsQueueCore.RequestInfos
{
	class RequestDataComposite : RequestDataBase, IEnumerable<IRequestData>
	{
		public static IRequestData Create(IEnumerable<IRequestData> requestInfoList, string name)
		{
			return new RequestDataComposite( requestInfoList, name );
		}

		private readonly List<IRequestData> _List = new List<IRequestData>();

		private RequestDataComposite(IEnumerable<IRequestData> infos, string name)
			: base( name )
		{
			_List = infos.ToList();
		}

		public IEnumerator<IRequestData> GetEnumerator()
		{
			return _List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected override IRequestHandle CreateRequestInternal(RequestsManagerStorage storageManagers)
		{
			_List.ForEach( requestData => requestData.CreateRequest( storageManagers ) );
			return RequestHandle.Create( this );
		}

		public override IRequestState ExecuteRequest( RequestsManagerStorage storageManagers )
		{
			var statesList = _List.Select( requestData => requestData.ExecuteRequest( storageManagers ) );
			return RequestStateFactory.Create( statesList );
		}

		public override IRequestState GetRequestState(RequestsManagerStorage storageManagers)
		{
			var statesList = _List.Select( requestData => requestData.GetRequestState( storageManagers ) );
			return RequestStateFactory.Create( statesList );
		}

		public override IRequestState CancelRequest( RequestsManagerStorage storageManagers )
		{
			var statesList = _List.Select( requestData => requestData.CancelRequest( storageManagers ) );
			return RequestStateFactory.Create( statesList );
		}
		
		public override Task CreateTask(CancellationToken cancelationToken)
		{
			return new Task( () => _List.ForEach( requestData => requestData.CreateTask( cancelationToken ).Start() ), cancelationToken );
		}
	}
}