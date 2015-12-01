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

		public InferenceInputPackage LoadInputData(int customerID, DateTime now) {
			try {
				var loader = new InputDataLoader(this.db, this.log, customerID, now).Execute();
				return new InferenceInputPackage(loader.Result, loader.CompanyID);
			} catch (Exception e) {
				throw new InputDataLoaderAlert(customerID, now, e, this.log);
			} // try
		} // LoadInputData

		public long SaveInferenceRequest(int customerID, int companyID, InferenceInput request) {
			try {
				return new InferenceRequestSaver(this.db, this.log, customerID, companyID, request).Execute().Result;
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
			try {
				return new InferenceSaver(this.db, this.log, customerID, requestID, response).Execute().Result;
			} catch (Exception e) {
				throw new InferenceSaverAlert(customerID, requestID, e, this.log);
			} // try
		} // SaveInference

		public ModuleConfiguration LoadModuleConfiguration() {
			return new ModuleConfiguration {
				CacheAcceptanceDays = CurrentValues.Instance.LogicalGlueCacheAcceptanceDays,
			};
		} // LoadModuleConfiguration

		public HarvesterConfiguration LoadHarvesterConfiguration() {
			return new HarvesterConfiguration {
				HostName = CurrentValues.Instance.LogicalGlueHostName,
				NewCustomerRequestPath = CurrentValues.Instance.LogicalGlueNewCustomerRequestPath,
				OldCustomerRequestPath = CurrentValues.Instance.LogicalGlueOldCustomerRequestPath,
			};
		} // LoadHarvesterConfiguration

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class Keeper
} // namespace
