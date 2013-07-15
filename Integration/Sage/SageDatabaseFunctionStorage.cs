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
		TotalSumOfPurchaseInvoices,
        NumOfExpenditures,
		TotalSumOfExpenditures,
		TotalSumOfPaidSalesInvoices,
		TotalSumOfUnpaidSalesInvoices,
		TotalSumOfPartiallyPaidSalesInvoices,
		TotalSumOfPaidPurchaseInvoices,
		TotalSumOfUnpaidPurchaseInvoices,
		TotalSumOfPartiallyPaidPurchaseInvoices
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
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.NumOfExpenditures, DatabaseValueTypeEnum.Integer, "{28B6E7E4-537E-4502-92E2-61F8E0B6B1CA}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfExpenditures, DatabaseValueTypeEnum.Double, "{21D7AD60-2E2F-41B5-9666-6CCABC56343A}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfPaidSalesInvoices, DatabaseValueTypeEnum.Double, "{EAFC6364-C34F-4209-BAC2-C8D8D0323EB9}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfUnpaidSalesInvoices, DatabaseValueTypeEnum.Double, "{92A81E0F-A2A0-42ED-9B70-B4167616032D}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfPartiallyPaidSalesInvoices, DatabaseValueTypeEnum.Double, "{1198F887-E0CE-4968-9B5D-010DE4188EAD}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfPaidPurchaseInvoices, DatabaseValueTypeEnum.Double, "{B04965CF-BECC-46CC-8B26-7C97DFC330D4}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfUnpaidPurchaseInvoices, DatabaseValueTypeEnum.Double, "{EDE52DCF-3C08-478C-A7C6-4764501C27F7}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfPartiallyPaidPurchaseInvoices, DatabaseValueTypeEnum.Double, "{E785E542-81D2-4B52-8C95-8AD10E1BC5C1}");
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

				case SageDatabaseFunctionType.NumOfExpenditures:
					displayName = "Num of Expenditures";
					break;

				case SageDatabaseFunctionType.TotalSumOfExpenditures:
					displayName = "Total Sum of Expenditures";
					break;

				case SageDatabaseFunctionType.TotalSumOfPaidSalesInvoices:
					displayName = "Total Sum of Paid Sales Invoices";
					break;

				case SageDatabaseFunctionType.TotalSumOfUnpaidSalesInvoices:
					displayName = "Total Sum of Unpaid Sales Invoices";
					break;

				case SageDatabaseFunctionType.TotalSumOfPartiallyPaidSalesInvoices:
					displayName = "Total Sum of Partially Paid Sales Invoices";
					break;

				case SageDatabaseFunctionType.TotalSumOfPaidPurchaseInvoices:
					displayName = "Total Sum of Paid Purchase Invoices";
					break;

				case SageDatabaseFunctionType.TotalSumOfUnpaidPurchaseInvoices:
					displayName = "Total Sum of Unpaid Purchase Invoices";
					break;

				case SageDatabaseFunctionType.TotalSumOfPartiallyPaidPurchaseInvoices:
					displayName = "Total Sum of Partially Paid Purchase Invoices";
					break;
				
                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
