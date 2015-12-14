namespace Ezbob.Backend.Strategies {
    using System;
    using System.Reflection;
    using ConfigManager;
    using Ezbob.Backend.Strategies.Exceptions;
    using Ezbob.Backend.Strategies.NewLoan;
    using Ezbob.Database;
    using Ezbob.Utils;
    using EzBob.Models.Marketplaces.Builders;
    using EZBob.DatabaseLib;
    using EZBob.DatabaseLib.Model.Database;
    using StructureMap;
    using Newtonsoft.Json;

    public abstract class AStrategy {
		static AStrategy() {
			ms_oLock = new object();
			ms_bDefaultsAreReady = false;
		} // static constructor

        public bool IsNewLoanRunStrategy
        {
            get { return this is Inlstrategy && Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value); }
        }

        public abstract string Name { get; }

		public abstract void Execute();

		public class StrategyContext {
			public int? UserID { get; set; }
			public int? CustomerID { get; set; }

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"user ID: '{0}', customer ID: '{1}'",
					UserID.HasValue ? UserID.Value.ToString() : "-- null --",
					CustomerID.HasValue ? CustomerID.Value.ToString() : "-- null --"
				);
			} // ToString
		} // class Context

		public StrategyContext Context { get; private set; }

		public AConnection DB { get; private set; }
		public StrategyLog Log { get; private set; }

		protected AStrategy() {
			if (ReferenceEquals(Library.Instance.DB, null))
				throw new FailedToInitStrategyException(this, new Exception("DB connection is not specified for strategy."));

			DB = Library.Instance.DB;
			Log = new StrategyLog(this, Library.Instance.Log);
			Context = new StrategyContext();

			InitDefaults(); // should not be moved to static constructor
		} // constructor

		protected static IMarketplaceModelBuilder GetMpModelBuilder(MP_CustomerMarketPlace mp) {
			var builder = ObjectFactory.TryGetInstance<IMarketplaceModelBuilder>(mp.Marketplace.GetType().ToString());
			return builder ?? ObjectFactory.GetNamedInstance<IMarketplaceModelBuilder>("DEFAULT");
		} // GetMpModelBuilder

		protected static DatabaseDataHelper DbHelper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // DbHelper

		protected virtual string BrokerSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.BrokerSite);
			} // get
		} // BrokerSite

		protected virtual string UnderwriterSite {
			get {
				return RemoveLastSlash(CurrentValues.Instance.UnderwriterSite);
			} // get
		} // UnderwriterSite

		private string RemoveLastSlash(string sResult) {
			while (sResult.EndsWith("/"))
				sResult = sResult.Substring(0, sResult.Length - 1);

			return sResult;
		} // RemoveLastSlash

		private static void InitDefaults() {
			if (ms_bDefaultsAreReady)
				return;

			lock (ms_oLock) {
				if (ms_bDefaultsAreReady)
					return;

				CurrentValues.Instance
					.SetDefault(Variables.CustomerSite, "https://app.ezbob.com")
					.SetDefault(Variables.BrokerSite, "https://app.ezbob.com/Broker");

				ms_bDefaultsAreReady = true;
			} // lock
		} // InitDefaults

		private static readonly object ms_oLock;
		private static bool ms_bDefaultsAreReady;

        public void NL_AddLog(LogType logType,
                                string description,
                                object args,
                                object result,
                                string exception,
                                string stacktrace) 
        {
            var sevirity = GetLogSection(logType);

            var nlLog = new NL_Log() {
                Exception = exception,
                Description = description,
                Referrer = this.Name,
                TimeStamp = DateTime.Now,
                Result = JsonConvert.SerializeObject(result, MiscUtils.GetJsonDBFormat()),                
                Sevirity = sevirity,
                Stacktrace = stacktrace,
                Args = JsonConvert.SerializeObject(args, MiscUtils.GetJsonDBFormat()),
                UserID = this.Context.UserID,
                CustomerID = this.Context.CustomerID,                
            };

            var logId = DB.ExecuteScalar<long>("NL_AddLog",
                CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Log>("Tbl", nlLog));
        }

	    public static string GetLogSection(LogType logType) {
	        return logType.ToString();
	    }

	    public enum LogType {
	        DataExsistense,
            Error,
            Info,
            Debug
	    }

    } // class AStrategy
} // namespace Ezbob.Backend.Strategies
