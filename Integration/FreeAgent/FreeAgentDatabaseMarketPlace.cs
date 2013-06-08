namespace FreeAgent
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class FreeAgentDatabaseMarketPlace : DatabaseMarketplaceBase<FreeAgentDatabaseFunctionType>
    {
		public FreeAgentDatabaseMarketPlace()
			: base(new FreeAgentServiceInfo())
        {
        }

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
        {
			return new FreeAgentRetrieveDataHelper(helper, this);
        }

		public override IDatabaseFunctionFactory<FreeAgentDatabaseFunctionType> FunctionFactory
        {
			get { return new FreeAgentDatabaseFunctionFactory(); }
        }
    }
}
