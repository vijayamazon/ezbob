using System;
using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using StructureMap;

namespace EZBob.DatabaseLib.DatabaseWrapper.ValueType
{
	public class DatabaseValueType : IDatabaseValueType
	{
		static DatabaseValueType()
		{
            var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			helper.InitValueTypes();
		}

		public static readonly IDatabaseValueType String	= new DatabaseValueType( DatabaseValueTypeEnum.String, new Guid( "{8F1988BD-2E11-4F86-9FEB-296FA70DA2CC}" ) );
		public static readonly IDatabaseValueType DateTime	= new DatabaseValueType( DatabaseValueTypeEnum.DateTime, new Guid( "{1BA61EEA-982D-4A26-A3D2-9CA66DB9CA17}" ) );
		public static readonly IDatabaseValueType Double	= new DatabaseValueType( DatabaseValueTypeEnum.Double, new Guid( "{97594E98-6B09-46AB-83ED-618678B327BE}" ) );
		public static readonly IDatabaseValueType Integer	= new DatabaseValueType( DatabaseValueTypeEnum.Integer, new Guid( "{A35FA704-C79E-4AA1-AB4C-A47B0005A2DE}" ) );
		public static readonly IDatabaseValueType Xml		= new DatabaseValueType( DatabaseValueTypeEnum.Xml, new Guid( "{88AA9E5A-1537-4FC3-9708-6EBB8372BE62}" ) );
		public static readonly IDatabaseValueType Boolean	= new DatabaseValueType( DatabaseValueTypeEnum.Boolean, new Guid( "{4C2A925B-A452-4703-AB90-B50A80292738}" ) );

		private DatabaseValueType( DatabaseValueTypeEnum databaseValueTypeEnum, Guid internalId )
		{
			ValueType = databaseValueTypeEnum;
			InternalId = internalId;

			var info = new DatabaseAnalysisFunctionTimePeriodTypeConverter().Convert( databaseValueTypeEnum );
			Name = info.Name;
			DisplayName = info.DisplayName;
			Description = info.Description;
		}

		public DatabaseValueTypeEnum ValueType { get; private set; }

		public Guid InternalId { get; private set; }

		public string Description { get; private set; }

		public string Name { get; private set; }

		public string DisplayName { get; private set; }

		public override string ToString()
		{
			return string.Format( "{0}", Name );
		}

	}

	internal class DatabaseAnalysisFunctionTimePeriodTypeConverter : IDatabaseEnumTypeConverter<DatabaseValueTypeEnum>
	{
		public ConvertedTypeInfo Convert(DatabaseValueTypeEnum type)
		{
			string name = string.Empty;
			string displayName = string.Empty;
			string description = string.Empty;

			switch ( type )
			{
				case DatabaseValueTypeEnum.String:
					name = "String";
					break;

				case DatabaseValueTypeEnum.DateTime:
					name = "DateTime";
					break;

				case DatabaseValueTypeEnum.Double:
					name = "Double";
					break;

				case DatabaseValueTypeEnum.Integer:
					name = "Integer";
					break;

				case DatabaseValueTypeEnum.Xml:
					name = "Xml";
					break;

				case DatabaseValueTypeEnum.Boolean:
					name = "Boolean";
					break;

				default:
					throw new NotImplementedException();
			}

			return new ConvertedTypeInfo( name, displayName, description );
		}
	}
}