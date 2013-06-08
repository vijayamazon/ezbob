namespace FreeAgent
{
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using System;

	public enum FreeAgentDatabaseFunctionType
    {
        NumOfOrders,
        TotalSumOfOrders,
    }

	internal class FreeAgentDatabaseFunctionStorage : DatabaseFunctionStorage<FreeAgentDatabaseFunctionType>
    {
        private static FreeAgentDatabaseFunctionStorage _Instance;

		private FreeAgentDatabaseFunctionStorage()
			: base(new FreeAgentDatabaseFunctionTypeConverter())
        {
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{04AD442A-9405-4F4E-9CC4-375B15A9B212}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{3C0A5BB6-87D0-4F78-AFF3-B41D001A7074}");
        }

		public static FreeAgentDatabaseFunctionStorage Instance
        {
            get
            {
				return _Instance ?? (_Instance = new FreeAgentDatabaseFunctionStorage());
            }
        }
    }

	internal class FreeAgentDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<FreeAgentDatabaseFunctionType>
    {
		public ConvertedTypeInfo Convert(FreeAgentDatabaseFunctionType type)
        {
            string displayName;
            string description = string.Empty;

            string name = type.ToString();

            switch (type)
            {
				case FreeAgentDatabaseFunctionType.NumOfOrders:
                    displayName = "Num of Orders";
                    break;

				case FreeAgentDatabaseFunctionType.TotalSumOfOrders:
                    displayName = "Total Sum of Orders";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
