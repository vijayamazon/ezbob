using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using System;

namespace PayPoint
{
    public enum PayPointDatabaseFunctionType
	{
        //test
        BLAH,
	}

    internal class PayPointDatabaseFunctionStorage : DatabaseFunctionStorage<PayPointDatabaseFunctionType>
    {
        private static PayPointDatabaseFunctionStorage _Instance;

        private PayPointDatabaseFunctionStorage()
            : base(new PayPointDatabaseFunctionTypeConverter())
        {
            //Stas test
            CreateFunctionAndAddToCollection(PayPointDatabaseFunctionType.BLAH, DatabaseValueTypeEnum.Integer, "{fa09ce65-d6a9-4656-b00c-5e635d2083c2}");
            
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
            //Stas test
            string displayName = string.Empty;
            string description = string.Empty;

            string name = type.ToString();

            switch (type)
            {
                case PayPointDatabaseFunctionType.BLAH:
                    displayName = "Num of Orders";
                    break;
                    
                default:
                    throw new NotImplementedException();
            }

            return new ConvertedTypeInfo(name, displayName, description);
		}
	}
}