using System;
using System.Threading;
using System.Threading.Tasks;
using EzBob.RequestsQueueCore.RequestInfos;

namespace EzBob.RequestsQueueCore.Handle
{
	public interface IRequestHandle : IEquatable<IRequestHandle>
	{
		Guid Id { get; }
		IRequestData RequestData { get;  }
	}
}