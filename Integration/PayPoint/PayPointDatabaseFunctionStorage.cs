namespace PayPoint
{
    using EZBob.DatabaseLib.Common;
    using EzBob.CommonLib;
    using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
    using System;

    public enum PayPointDatabaseFunctionType
	{
        NumOfOrders, // All the orders
        SumOfAuthorisedOrders, // Orders with Status=Authorised
        OrdersAverage, // SumOfAuthorisedOrders / number of orders with Status=Authorised
        NumOfFailures, // Orders with Status!=Authorised
        CancellationRate, // (number of orders with Status=Authorised) * 100 / NumOfOrders
        CancellationValue // (Sum of orders with Status!=Authorised) / (Sum of all orders)
	}

    internal class PayPointDatabaseFunctionStorage : DatabaseFunctionStorage<PayPointDatabaseFunctionType>
    {
        private static PayPointDatabaseFunctionStorage _Instance;

        private PayPointDatabaseFunctionStorage()
            : base(new PayPointDatabaseFunctionTypeConverter())
        {
            // Can be converted to DatabaseValueTypeEnum.Decimal after adding the decimal column to DB
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{D2E0F96C-D98C-4230-8A8A-05BAF06B8829}");
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.SumOfAuthorisedOrders, DatabaseValueTypeEnum.Double, "{1F206986-DE58-433A-A175-22DE3045904E}");
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.OrdersAverage, DatabaseValueTypeEnum.Double, "{62F56269-9F0E-4F4B-B244-F74F9A4625D0}");
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.NumOfFailures, DatabaseValueTypeEnum.Integer, "{0ED4E59E-AC1C-48B1-BEC0-C4CB3282D769}");
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.CancellationRate, DatabaseValueTypeEnum.Double, "{122B5C33-F1AD-4510-B437-7F05FF5F304B}");
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.CancellationValue, DatabaseValueTypeEnum.Double, "{015EC706-E3B3-4252-B27E-28638DF330A8}");
        }

        public static PayPointDatabaseFunctionStorage Instance
        {
            get
            {
                return _Instance ?? (_Instance = new PayPointDatabaseFunctionStorage());
            }
        }
    }

	internal class PayPointDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<PayPointDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(PayPointDatabaseFunctionType type)
		{
            string displayName = string.Empty;
            string description = string.Empty;

            string name = type.ToString();

            switch (type)
            {
                case PayPointDatabaseFunctionType.NumOfOrders:
                    displayName = "Num of Orders";
                    break;
                case PayPointDatabaseFunctionType.SumOfAuthorisedOrders:
                    displayName = "Sum of Orders";
                    break;
                case PayPointDatabaseFunctionType.OrdersAverage:
                    displayName = "Average Order";
                    break;
                case PayPointDatabaseFunctionType.NumOfFailures:
                    displayName = "Num of Failures";
                    break;
                case PayPointDatabaseFunctionType.CancellationRate:
                    displayName = "Cancellation Rate";
                    break;
                case PayPointDatabaseFunctionType.CancellationValue:
                    displayName = "Cancellation Value";
                    break;
                    
                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}