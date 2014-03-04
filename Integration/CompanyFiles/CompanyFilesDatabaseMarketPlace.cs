using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace CompanyFiles
{
	public class CompanyFilesDatabaseMarketPlace : DatabaseMarketplaceBase<CompanyFilesDatabaseFunctionType>
	{
		public CompanyFilesDatabaseMarketPlace()
			: base(new CompanyFilesServiceInfo())
		{
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
		{
			return new CompanyFilesRetriveDataHelper(helper, this);
		}

		public override IDatabaseFunctionFactory<CompanyFilesDatabaseFunctionType> FunctionFactory
		{
			get { return new CompanyFilesDatabaseFunctionFactory(); }
		}
	}
}
