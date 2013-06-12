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
		NumOfExpenses,
		TotalSumOfExpenses
    }

	internal class FreeAgentDatabaseFunctionStorage : DatabaseFunctionStorage<FreeAgentDatabaseFunctionType>
    {
        private static FreeAgentDatabaseFunctionStorage _Instance;

		private FreeAgentDatabaseFunctionStorage()
			: base(new FreeAgentDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{04AD442A-9405-4F4E-9CC4-375B15A9B212}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{3C0A5BB6-87D0-4F78-AFF3-B41D001A7074}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.NumOfExpenses, DatabaseValueTypeEnum.Integer, "{F6CC0CB1-99A1-4012-9493-849F142DD5A8}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.TotalSumOfExpenses, DatabaseValueTypeEnum.Double, "{C28A3C87-CDDF-45DC-8DE3-7E2F74FE01FF}");
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

				case FreeAgentDatabaseFunctionType.NumOfExpenses:
					displayName = "Num of Expenses";
					break;

				case FreeAgentDatabaseFunctionType.TotalSumOfExpenses:
					displayName = "Total Sum of Expenses";
					break;

                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
