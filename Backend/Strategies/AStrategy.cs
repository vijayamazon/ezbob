﻿namespace Ezbob.Backend.Strategies {
	using System;
	using ConfigManager;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Exceptions;
	using Ezbob.Backend.Extensions;
	using EzBob.Models.Marketplaces.Builders;
	using Ezbob.Database;
	using StructureMap;

	public abstract class AStrategy {
		static AStrategy() {
			ms_oLock = new object();
			ms_bDefaultsAreReady = false;
		} // static constructor

		public abstract string Name { get; }

		public abstract void Execute();

		public class StrategyContext {
			public StrategyContext() {
				StartTime = DateTime.UtcNow;
			} // constructor

			public int? UserID { get; set; }
			public int? CustomerID { get; set; }
			public DateTime StartTime { get; private set; }

			public string Description {
				get {
					if (string.IsNullOrWhiteSpace(this.description))
						this.description = DefaultDescription;

					return this.description;
				} // get
				set {
					this.description = string.IsNullOrWhiteSpace(value) ? DefaultDescription : value.Trim();
				} // set
			} // Description

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>
			/// A string that represents the current object.
			/// </returns>
			public override string ToString() {
				return string.Format(
					"user ID: '{0}', customer ID: '{1}', started at '{2}'",
					UserID.HasValue ? UserID.Value.ToString() : "-- null --",
					CustomerID.HasValue ? CustomerID.Value.ToString() : "-- null --",
					StartTime.MomentStr()
				);
			} // ToString

			private const string DefaultDescription = "This strategy usually finishes almost instantly.";

			private string description;
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
					.SetDefault(ConfigManager.Variables.CustomerSite, "https://app.ezbob.com")
					.SetDefault(ConfigManager.Variables.BrokerSite, "https://app.ezbob.com/Broker");

				ms_bDefaultsAreReady = true;
			} // lock
		} // InitDefaults

		private static readonly object ms_oLock;
		private static bool ms_bDefaultsAreReady;
	} // class AStrategy
} // namespace Ezbob.Backend.Strategies
