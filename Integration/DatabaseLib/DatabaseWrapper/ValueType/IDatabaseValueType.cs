using System;

namespace EZBob.DatabaseLib.DatabaseWrapper.ValueType
{
	public enum DatabaseValueTypeEnum
	{
		String,
		Integer,
		Double,
		Xml,
		DateTime,
		Boolean
	}

	public interface IDatabaseValueType
	{
		DatabaseValueTypeEnum ValueType { get; }

		Guid InternalId { get; }

		string Description { get; }
		string Name { get;  }
		string DisplayName { get; }
	}

}
