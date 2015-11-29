namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;

	class Keeper : IKeeper {
		public Keeper(AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log.Safe();

			if (this.db == null)
				throw new NoConnectionKeeperAlert(this.log);
		} // constructor

		public InferenceInput LoadInputData(int customerID, DateTime now) {
			try {
				return new InputDataLoader(this.db, this.log, customerID, now).Execute().Result;
			} catch (Exception e) {
				throw new InputDataLoaderAlert(customerID, now, e, this.log);
			} // try
		} // LoadInputData

		public long SaveInferenceRequest(int customerID, InferenceInput request) {
			try {
				return new InferenceRequestSaver(this.db, this.log, customerID, request).Execute().Result;
			} catch (Exception e) {
				throw new InferenceRequestSaverAlert(customerID, request, e, this.log);
			} // try
		} // SaveRequest

		public Inference LoadInference(int customerID, DateTime time) {
			try {
				return new InferenceLoader(this.db, this.log, customerID, time).Execute().Result;
			} catch (Exception e) {
				throw new InferenceLoaderAlert(customerID, time, e, this.log);
			} // try
		} // LoadInference

		public Inference SaveInference(int customerID, long requestID, Response<Reply> response) {
			return null; // TODO
		} // SaveInference

		public Configuration LoadConfiguration() {
			return new Configuration {
				CacheAcceptanceDays = CurrentValues.Instance.LogicalGlueCacheAcceptanceDays,
			};
		} // LoadConfiguration

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class Keeper
} // namespace
