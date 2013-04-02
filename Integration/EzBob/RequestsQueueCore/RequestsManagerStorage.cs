using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace EzBob.RequestsQueueCore
{
	public class RequestsManagerStorage
	{
		private readonly Dictionary<Guid, ExternalRequestsManager> _Managers = new Dictionary<Guid, ExternalRequestsManager>(new ConcurrentDictionary<Guid, ExternalRequestsManager>());
		private readonly object _Locker = new object();
		
		public void Exit()
		{
			_Managers.Values.ToList().ForEach( m => m.Exit() );

			_Managers.Clear();
		}

		public ExternalRequestsManager GetManager( IDatabaseMarketplace marketplace )
		{
			lock ( _Locker )
			{
				ExternalRequestsManager externalRequestsManager;

				var internalId = marketplace.InternalId;

				if ( !_Managers.ContainsKey( internalId ) )
				{
					externalRequestsManager = CreateManager( marketplace );
					_Managers.Add( internalId, externalRequestsManager );
				}
				else
				{
					externalRequestsManager = _Managers[internalId];
				}

				return externalRequestsManager;
			}
			
		}

		private ExternalRequestsManager CreateManager( IDatabaseMarketplace marketplace )
		{
			return new ExternalRequestsManager( marketplace );
		}

	}
}