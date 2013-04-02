using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.DatabaseWrapper.Functions;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;

namespace EZBob.DatabaseLib.Common
{
	public abstract class DatabaseFunctionStorage<TEnum>
	{
		private readonly IDatabaseEnumTypeConverter<TEnum> _DatabaseFunctionTypeConverter;
		
		private readonly Dictionary<TEnum, IDatabaseFunction> _Funcs = new Dictionary<TEnum, IDatabaseFunction>();

		protected DatabaseFunctionStorage( IDatabaseEnumTypeConverter<TEnum> databaseFunctionTypeConverter )
		{
			_DatabaseFunctionTypeConverter = databaseFunctionTypeConverter;
		}

		public IDatabaseFunction GetFunction( TEnum type )
		{
			if ( !_Funcs.ContainsKey( type ) )
			{
				throw new NotImplementedException();
			}
			return _Funcs[type];
		}

		protected void CreateFunctionAndAddToCollection( TEnum functionType, DatabaseValueTypeEnum functionReturnValue, string guidId )
		{
			var func = CreateFunction( functionType, functionReturnValue, guidId );

			_Funcs.Add( functionType, func );
		}

		private IDatabaseFunction CreateFunction( TEnum functionType, DatabaseValueTypeEnum functionReturnValue, string guidId )
		{
			return new DatabaseFunction<TEnum>( functionType, DatabaseValueTypeFactory.Create( functionReturnValue ), new Guid( guidId ), _DatabaseFunctionTypeConverter );
		}

		public IDatabaseFunction GetFunctionById( Guid id )
		{
			return _Funcs.Values.FirstOrDefault( fv => fv.InternalId.Equals( id ) );
		}

	}
}
