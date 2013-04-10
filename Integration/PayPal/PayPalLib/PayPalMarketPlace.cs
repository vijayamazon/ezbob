using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EzBob.PayPalDbLib;
using EzBob.PayPalServiceLib;
using StructureMap;

namespace EzBob.PayPal
{
	public class PayPalDatabaseMarketPlace : DatabaseMarketplaceBase<PayPalDatabaseFunctionType>
	{
		/*static PayPalDatabaseMarketPlace()
		{
			var helper = ObjectFactory.GetInstance<DatabaseDataHelper>();
			var mp = new PayPalDatabaseMarketPlace();
			helper.InitDatabaseMarketPlace( mp );
		}*/

		public PayPalDatabaseMarketPlace()
			: base( new PayPalServiceInfo() )
		{
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
		{
			return new PayPalRetriveDataHelper( helper, this );
		}

		public override IDatabaseFunctionFactory<PayPalDatabaseFunctionType> FunctionFactory
		{
			get { return new PayPalDatabaseFunctionFactory(); }
		}
	}
}
