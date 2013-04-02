using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;

namespace EZBob.DatabaseLib.DatabaseWrapper.Functions
{
	public interface IDatabaseFunction
	{
		string Name { get; }
		string DisplayName { get; }
		string Description { get; set; }
		IDatabaseMarketplace DatabaseMarketplace { get; }
		IDatabaseValueType FunctionValueType { get; }
		Guid InternalId { get; }
	}
}
