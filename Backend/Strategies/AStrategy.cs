namespace EzBob.Backend.Strategies {
	using System;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Exceptions;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public abstract class AStrategy {
		#region static constructor

		static AStrategy() {
			ms_oLock = new object();
			ms_bDefaultsAreReady = false;
		} // static constructor

		#endregion static constructor

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

			InitDefaults(); // should not be moved to static constructor
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

		#region property CustomerSite

		protected virtual string CustomerSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.CustomerSite);
			} // get
		} // CustomerSite

		#endregion property CustomerSite

		#region property BrokerSite

		protected virtual string BrokerSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.BrokerSite);
			} // get
		} // BrokerSite

		#endregion property BrokerSite

		#region property UnderwriterSite

		protected virtual string UnderwriterSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.UnderwriterSite);
			} // get
		} // UnderwriterSite

		#endregion property UnderwriterSite

		#endregion protected

		#region private

		#region method RemoveLastSlash

		private string RemoveLastSlash(string sResult) {
			while (sResult.EndsWith("/"))
				sResult = sResult.Substring(0, sResult.Length - 1);

			return sResult;
		} // RemoveLastSlash

		#endregion method RemoveLastSlash

		#region method InitDefaults

		private static void InitDefaults() {
			if (ms_bDefaultsAreReady)
				return;

			lock (ms_oLock) {
				if (ms_bDefaultsAreReady)
					return;

				CurrentValues.Instance
					.SetDefault(ConfigManager.Variables.CustomerSite, "https://app.ezbob.com")
					.SetDefault(ConfigManager.Variables.BrokerSite, "https://app.ezbob.com/Broker");

				ms_bDefaultsAreReady = true;
			} // lock
		} // InitDefaults

		#endregion method InitDefaults

		private static readonly object ms_oLock;
		private static bool ms_bDefaultsAreReady;

		#endregion private
	} // class AStrategy
} // namespace EzBob.Backend.Strategies
