using System;

namespace EZBob.DatabaseLib.DatabaseWrapper.ValueType
{
	public static class DatabaseValueTypeFactory
	{
		public static IDatabaseValueType Create( DatabaseValueTypeEnum type )
		{
			switch (type)
			{
				case DatabaseValueTypeEnum.String:
					return DatabaseValueType.String;

				case DatabaseValueTypeEnum.DateTime:
					return DatabaseValueType.DateTime;

				case DatabaseValueTypeEnum.Double:
					return DatabaseValueType.Double;

				case DatabaseValueTypeEnum.Integer:
					return DatabaseValueType.Integer;

				case DatabaseValueTypeEnum.Xml:
					return DatabaseValueType.Xml;

				case DatabaseValueTypeEnum.Boolean:
					return DatabaseValueType.Boolean;

				default:
					throw new NotImplementedException();
			}
		}
	}
}