namespace Sage
{
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;

	public class SageDatabaseMarketPlace : DatabaseMarketplaceBase<SageDatabaseFunctionType>
    {
		public SageDatabaseMarketPlace()
			: base(new SageServiceInfo())
        {
        }

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper)
        {
			return new SageRetrieveDataHelper(helper, this);
        }

		public override IDatabaseFunctionFactory<SageDatabaseFunctionType> FunctionFactory
        {
			get { return new SageDatabaseFunctionFactory(); }
        }
    }
}
