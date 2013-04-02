using System;

namespace EzBob.CommonLib
{
	public interface IMarketplaceInfo
	{
		string DisplayName { get; }
		Guid InternalId { get; }
		string Description { get; }
	}
}
