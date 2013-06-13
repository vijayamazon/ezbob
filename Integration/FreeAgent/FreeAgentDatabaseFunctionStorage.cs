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
		TotalSumOfExpenses,
		SumOfPaidInvoices,
		SumOfOverdueInvoices,
		SumOfOpenInvoices,
		SumOfDraftInvoices
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
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.SumOfPaidInvoices, DatabaseValueTypeEnum.Double, "{5C05D954-4D77-4AD5-A037-73D86E924570}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.SumOfOverdueInvoices, DatabaseValueTypeEnum.Double, "{1E68EE0E-B5B9-4A90-BC84-7F968138A475}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.SumOfOpenInvoices, DatabaseValueTypeEnum.Double, "{C82D1469-9DC5-4E17-8B11-668D96D8455B}");
			CreateFunctionAndAddToCollection(FreeAgentDatabaseFunctionType.SumOfDraftInvoices, DatabaseValueTypeEnum.Double, "{2FDC394F-A138-4ABA-A7A0-CF77412EB2B3}");
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

				case FreeAgentDatabaseFunctionType.SumOfPaidInvoices:
					displayName = "Sum of paid invoices";
					break;

				case FreeAgentDatabaseFunctionType.SumOfOverdueInvoices:
					displayName = "Sum of overdue invoices";
					break;

				case FreeAgentDatabaseFunctionType.SumOfOpenInvoices:
					displayName = "Sum of open invoices";
					break;

				case FreeAgentDatabaseFunctionType.SumOfDraftInvoices:
					displayName = "Sum of draft invoices";
					break;

                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
