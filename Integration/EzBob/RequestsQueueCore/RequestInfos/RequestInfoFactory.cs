using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.RequestsQueueCore.Handle;

namespace EzBob.RequestsQueueCore.RequestInfos
{
	internal static class RequestInfoFactory
	{
		public static IRequestData CreateSingleRequest( IDatabaseMarketplace marketplace, Action action, string name = "RequestDataSingle" )
		{
			return RequestDataSingle.Create(marketplace, action, name);
		}

		public static IRequestData CreateCompositeRequest( IEnumerable<IRequestData> requestInfoList, string name = "RequestDataComposite" )
		{
			return RequestDataComposite.Create( requestInfoList, name );
		}
	}
}