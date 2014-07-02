namespace EzBob.Backend.Strategies {
	using System;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Exceptions;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public abstract class AStrategy {
		#region public

		public abstract string Name { get; }

		public abstract void Execute();

		public AConnection DB { get; private set; }
		public StrategyLog Log { get; private set; }

		#endregion public

		#region protected

		#region constructor

		protected AStrategy(AConnection oDB, ASafeLog oLog) {
			if (ReferenceEquals(oDB, null))
				throw new FailedToInitStrategyException(this, new ArgumentNullException("oDB", "DB connection is not specified for mail strategy."));

			DB = oDB;
			Log = new StrategyLog(this, oLog);
		} // constructor

		#endregion constructor

		#region method GetMpModelBuilder

		protected static IMarketplaceModelBuilder GetMpModelBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetMpModelBuilder

		#endregion method GetMpModelBuilder

		#region property DbHelper

		protected static DatabaseDataHelper DbHelper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // DbHelper

		#endregion property DbHelper

		#endregion protected
	} // class AStrategy
} // namespace EzBob.Backend.Strategies
