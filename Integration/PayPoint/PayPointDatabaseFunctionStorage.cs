namespace PayPoint
{
    using EZBob.DatabaseLib.Common;
    using EzBob.CommonLib;
    using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
    using System;

    public enum PayPointDatabaseFunctionType
	{
        NumOfOrders,
        //qqq define all aggregations wanted in paypoint
	}

    internal class PayPointDatabaseFunctionStorage : DatabaseFunctionStorage<PayPointDatabaseFunctionType>
    {
        private static PayPointDatabaseFunctionStorage _Instance;

        private PayPointDatabaseFunctionStorage()
            : base(new PayPointDatabaseFunctionTypeConverter())
        {
            // qqq - what is this guid used for? how should it be generated\kept?
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{fa09ce65-d6a9-4656-b00c-5e635d2083c2}");            
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
                    
                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}