using System;
using System.Collections.Generic;
using EZBob.DatabaseLib;

namespace EzBob.RequestsQueueCore.RequestStates
{
	public class RequestErorrInfo
	{
		public RequestErorrInfo( Exception exception )			
		{
			CompositeException = new CompositeServiceRequestException( new[] { exception } );			
		}

		public RequestErorrInfo( IEnumerable<Exception> exceptions )
		{
			CompositeException = new CompositeServiceRequestException( exceptions );
			
		}

		public string Message
		{
			get  { return CompositeException.Message; }
		}

		public CompositeServiceRequestException CompositeException { get; private set; }

		public string MessageWithCallStack
		{
			get { return CompositeException.MessageWithCallStack; }
		}
	}
}