using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.eBayDbLib;
using EzBob.eBayServiceLib;
using StructureMap;

namespace EzBob.eBayLib
{
	public class eBayDatabaseMarketPlace : DatabaseMarketplaceBase<eBayDatabaseMarketPlace, eBayDatabaseFunctionType>
	{
		/*static eBayDatabaseMarketPlace()
		{
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			var mp = new eBayDatabaseMarketPlace();
			helper.InitDatabaseMarketPlace( mp );
		}*/

		public eBayDatabaseMarketPlace()
			:base(new eBayServiceInfo())
		{
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
		{
			return new eBayRetriveDataHelper( helper, this );
		}

		public override IDatabaseFunctionFactory<eBayDatabaseFunctionType> FunctionFactory
		{
			get { return new eBayDatabaseFunctionFactory(); }
		}
	}
}
