using System;
using System.Threading;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.RequestInfos;

namespace EzBob.RequestsQueueCore.Handle
{
	class RequestHandle : IRequestHandle, IEquatable<RequestHandle>
	{
		public static IRequestHandle Create( IRequestData requestData )
		{
			return new RequestHandle( requestData );
		}

		private RequestHandle( IRequestData requestData )
		{
			RequestData = requestData;
			Id = Guid.NewGuid();
		}

		public bool Equals(IRequestHandle other)
		{
			return Equals( other as RequestHandle );
		}

		public override bool Equals( object obj )
		{
			return Equals( obj as IRequestHandle);
		}

		public Guid Id { get; private set; }

		public IRequestData RequestData { get; private set; }

		public bool Equals(RequestHandle other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return other.Id.Equals(Id);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}		
	}
}