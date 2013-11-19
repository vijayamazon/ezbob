namespace EKM
{
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using System;

    public enum EkmDatabaseFunctionType
    {
        NumOfOrders,
        NumOfCancelledOrders,
        NumOfOtherOrders,

		TotalSumOfOrders,
		TotalSumOfOrdersAnnualized,
        TotalSumOfCancelledOrders,
        TotalSumOfOtherOrders,

        AverageSumOfOrder,
        AverageSumOfCancelledOrder,
        AverageSumOfOtherOrder,

        CancellationRate, 
    }

    internal class EkmDatabaseFunctionStorage : DatabaseFunctionStorage<EkmDatabaseFunctionType>
    {
        private static EkmDatabaseFunctionStorage _Instance;

        private EkmDatabaseFunctionStorage()
            : base(new EkmDatabaseFunctionTypeConverter())
        {
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{FA09CE65-D6A9-4656-B00C-5E635D2083C2}");
			CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{8687DABC-C092-4296-92FF-42612A59157A}");
			CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.TotalSumOfOrdersAnnualized, DatabaseValueTypeEnum.Double, "{9289D308-C204-4865-AB45-B21A83658034}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.AverageSumOfOrder, DatabaseValueTypeEnum.Double, "{86A7E774-B48B-44C1-8DB1-F3212E5063F2}");

            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.NumOfCancelledOrders, DatabaseValueTypeEnum.Integer, "{DFAE1DC2-F8C6-4C71-A357-0C8A5FE3B23D}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.TotalSumOfCancelledOrders, DatabaseValueTypeEnum.Double, "{7AB97148-DB4C-43A2-98E2-88995431F523}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.AverageSumOfCancelledOrder, DatabaseValueTypeEnum.Double, "{D8E14F67-DCE1-4831-962D-5CB3BC49B1CF}");

            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.NumOfOtherOrders, DatabaseValueTypeEnum.Integer, "{9F52623C-B16B-4316-9244-9C5EDD0F2347}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.TotalSumOfOtherOrders, DatabaseValueTypeEnum.Double, "{A4F01792-2F97-469E-BE00-08E7E6BAE232}");
            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.AverageSumOfOtherOrder, DatabaseValueTypeEnum.Double, "{19CF2A6E-5975-48AE-A4C4-A25DDF127C2E}");

            CreateFunctionAndAddToCollection(EkmDatabaseFunctionType.CancellationRate, DatabaseValueTypeEnum.Double, "{C286A0ED-D143-4B27-9598-B9148DF0A49E}");
        }

        public static EkmDatabaseFunctionStorage Instance
        {
            get
            {
                return _Instance ?? (_Instance = new EkmDatabaseFunctionStorage());
            }
        }
    }

    internal class EkmDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<EkmDatabaseFunctionType>
    {
        public ConvertedTypeInfo Convert(EkmDatabaseFunctionType type)
        {
            string displayName;
            string description = string.Empty;

            string name = type.ToString();

            switch (type)
            {
                case EkmDatabaseFunctionType.NumOfOrders:
                    displayName = "Num of Orders";
                    break;

                case EkmDatabaseFunctionType.AverageSumOfOrder:
                    displayName = "Average Sum of Order";
					break;

				case EkmDatabaseFunctionType.TotalSumOfOrders:
					displayName = "Total Sum of Orders";
					break;

				case EkmDatabaseFunctionType.TotalSumOfOrdersAnnualized:
					displayName = "Total Sum of Orders Annualized";
					break;

                case EkmDatabaseFunctionType.NumOfCancelledOrders:
                    displayName = "Num of Cancelled Orders";
                    break;

                case EkmDatabaseFunctionType.AverageSumOfCancelledOrder:
                    displayName = "Average Sum of Cancelled Order";
                    break;

                case EkmDatabaseFunctionType.TotalSumOfCancelledOrders:
                    displayName = "Total Sum of Cancelled Orders";
                    break;

                case EkmDatabaseFunctionType.NumOfOtherOrders:
                    displayName = "Num of Other Orders";
                    break;

                case EkmDatabaseFunctionType.AverageSumOfOtherOrder:
                    displayName = "Average Sum of Other Order";
                    break;

                case EkmDatabaseFunctionType.TotalSumOfOtherOrders:
                    displayName = "Total Sum of Other Orders";
                    break;

                case EkmDatabaseFunctionType.CancellationRate:
                    displayName = "Cancellation Rate";
                    break;

                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
