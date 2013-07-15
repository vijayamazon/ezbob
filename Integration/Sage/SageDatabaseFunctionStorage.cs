namespace Sage
{
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using System;

	public enum SageDatabaseFunctionType
	{
		NumOfOrders,
		TotalSumOfOrders,
        NumOfIncomes,
		TotalSumOfIncomes,
		NumOfPurchaseInvoices,
		TotalSumOfPurchaseInvoices
    }

	internal class SageDatabaseFunctionStorage : DatabaseFunctionStorage<SageDatabaseFunctionType>
    {
        private static SageDatabaseFunctionStorage _Instance;

		private SageDatabaseFunctionStorage()
			: base(new SageDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{906D9705-4E2F-4AD8-9DBC-B73FB723B83E}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{2F80945F-025F-4B85-BF04-C940FF9E6629}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.NumOfIncomes, DatabaseValueTypeEnum.Integer, "{3C367949-00AA-4A7F-8658-D66CE24A4E52}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfIncomes, DatabaseValueTypeEnum.Double, "{391AFF11-CB8B-4DC9-A7B3-55D0FA92C3FC}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.NumOfPurchaseInvoices, DatabaseValueTypeEnum.Integer, "{BAE950C5-9D78-42E1-8A2E-23EBD2669472}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfPurchaseInvoices, DatabaseValueTypeEnum.Double, "{315039AE-3366-4309-AEBA-A1DA20BBE3C3}");
        }

		public static SageDatabaseFunctionStorage Instance
        {
            get
            {
				return _Instance ?? (_Instance = new SageDatabaseFunctionStorage());
            }
        }
    }

	internal class SageDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<SageDatabaseFunctionType>
    {
		public ConvertedTypeInfo Convert(SageDatabaseFunctionType type)
        {
            string displayName;
            string description = string.Empty;

            string name = type.ToString();

            switch (type)
			{
				case SageDatabaseFunctionType.NumOfOrders:
					displayName = "Num of Orders";
					break;

				case SageDatabaseFunctionType.TotalSumOfOrders:
					displayName = "Total Sum of Orders";
					break;

				case SageDatabaseFunctionType.NumOfIncomes:
					displayName = "Num of Incomes";
					break;

				case SageDatabaseFunctionType.TotalSumOfIncomes:
					displayName = "Total Sum of Incomes";
					break;

				case SageDatabaseFunctionType.NumOfPurchaseInvoices:
					displayName = "Num of Purchase Invoices";
					break;

				case SageDatabaseFunctionType.TotalSumOfPurchaseInvoices:
					displayName = "Total Sum of Purchase Invoices";
					break;
				
                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
