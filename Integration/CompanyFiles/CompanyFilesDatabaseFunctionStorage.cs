namespace CompanyFiles
{
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;

	public enum CompanyFilesDatabaseFunctionType
	{
	}

	internal class CompanyFilesDatabaseFunctionStorage : DatabaseFunctionStorage<CompanyFilesDatabaseFunctionType>
	{
		private static CompanyFilesDatabaseFunctionStorage _Instance;

		private CompanyFilesDatabaseFunctionStorage()
			: base(new CompanyFilesDatabaseFunctionTypeConverter())
		{

		}

		public static CompanyFilesDatabaseFunctionStorage Instance
		{
			get
			{
				return _Instance ?? (_Instance = new CompanyFilesDatabaseFunctionStorage());
			}
		}
	}

	internal class CompanyFilesDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<CompanyFilesDatabaseFunctionType>
	{
		public ConvertedTypeInfo Convert(CompanyFilesDatabaseFunctionType type)
		{
			return new ConvertedTypeInfo();
		}
	}
}
