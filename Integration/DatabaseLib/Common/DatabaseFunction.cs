using System;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib.Common
{
	public class DatabaseFunction<TEnum> : IDatabaseFunction
	{
		public DatabaseFunction( TEnum functionType, IDatabaseValueType valueType, Guid internalId, IDatabaseEnumTypeConverter<TEnum> databaseFunctionTypeConverterFactory )
		{
			FunctionValueType = valueType;
			InternalId = internalId;
			FunctionType = functionType;

			var info = databaseFunctionTypeConverterFactory.Convert( FunctionType );
			Name = info.Name;
			Description = info.Description;
			DisplayName = info.DisplayName;
		}

		public string Name { get; private set; }

		public string DisplayName { get; private set; }

		public string Description { get; set; }

		public IMarketplaceType DatabaseMarketplace { get; private set; }

		public IDatabaseValueType FunctionValueType { get; private set; }

		public Guid InternalId { get; private set; }

		public TEnum FunctionType { get; private set; }

	}
}