namespace Sage
{
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
	using System;

	public enum SageDatabaseFunctionType
    {
        NumOfOrders, 
		TotalSumOfOrders
    }

	internal class SageDatabaseFunctionStorage : DatabaseFunctionStorage<SageDatabaseFunctionType>
    {
        private static SageDatabaseFunctionStorage _Instance;

		private SageDatabaseFunctionStorage()
			: base(new SageDatabaseFunctionTypeConverter())
		{
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{906D9705-4E2F-4AD8-9DBC-B73FB723B83E}");
			CreateFunctionAndAddToCollection(SageDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{2F80945F-025F-4B85-BF04-C940FF9E6629}");
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
				
                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
        }
    }
}
