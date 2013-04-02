using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;

namespace EKM
{
	public enum EKMDatabaseFunctionType
	{
	}

    internal class EKMDatabaseFunctionStorage : DatabaseFunctionStorage<EKMDatabaseFunctionType>
    {
        private static EKMDatabaseFunctionStorage _Instance;

        private EKMDatabaseFunctionStorage()
            : base(new EKMDatabaseFunctionTypeConverter())
        {
        }

        public static EKMDatabaseFunctionStorage Instance
        {
            get
            {
                return _Instance ?? (_Instance = new EKMDatabaseFunctionStorage());
            }
        }
    }

	internal class EKMDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<EKMDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(EKMDatabaseFunctionType type)
		{
			throw new System.NotImplementedException();
		}
	}
}