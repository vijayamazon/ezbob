using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.AmazonDbLib;
using EzBob.AmazonServiceLib;
using StructureMap;

namespace EzBob.AmazonLib
{
	public class AmazonDatabaseMarketPlace : DatabaseMarketplaceBase<AmazonDatabaseMarketPlace, AmazonDatabaseFunctionType>
	{
		/*static AmazonDatabaseMarketPlace()
		{
			InitDatabaseMarketplace<AmazonDatabaseMarketPlace>();				
		}*/

		public AmazonDatabaseMarketPlace() 
			: base(new AmazonServiceInfo())
		{
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
		{
			return new AmazonRetriveDataHelper( helper, this );
		}
		
		public override IDatabaseFunctionFactory<AmazonDatabaseFunctionType> FunctionFactory
		{
			get { return new AmazonDatabaseFunctionFactory(); }
		}

	}
}
