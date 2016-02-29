namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;

	class Keeper : IKeeper {
		public Keeper(AConnection db, ASafeLog log) {
			this.db = db;
			this.log = log.Safe();

			if (this.db == null)
				throw new NoConnectionKeeperAlert(this.log);
		} // constructor

		public InferenceInputPackage LoadInputData(int customerID, DateTime now, bool loadMonthlyRepaymentOnly) {
			try {
				var loader = new InputDataLoader(this.db, this.log, customerID, now, loadMonthlyRepaymentOnly).Execute();
				return new InferenceInputPackage(loader.Result, loader.CompanyID);
			} catch (Exception e) {
				throw new InputDataLoaderAlert(customerID, now, e, this.log);
			} // try
		} // LoadInputData

		public long SaveInferenceRequest(int customerID, int companyID, bool isTryOut, InferenceInput request) {
			try {
				return new InferenceRequestSaver(
					this.db,
					this.log,
					customerID,
					companyID,
					isTryOut,
					request
				).Execute().Result;
			} catch (Exception e) {
				throw new InferenceRequestSaverAlert(customerID, request, e, this.log);
			} // try
		} // SaveRequest

		public Inference LoadInference(int customerID, DateTime time, bool includeTryOutData, decimal monthlyPayment) {
			try {
				return new InferenceLoader(
					this.db,
					this.log,
					customerID,
					time,
					includeTryOutData,
					monthlyPayment,
					LoadBuckets()
				).Execute().Result;
			} catch (Exception e) {
				throw new InferenceLoaderAlert(customerID, time, e, this.log);
			} // try
		} // LoadInference

		public Inference SaveInference(int customerID, long requestID, Response<Reply> response) {
			try {
				long responseID = new InferenceSaver(this.db, this.log, requestID, response, LoadBuckets())
					.Execute()
					.ResponseID;

				return new InferenceLoader(this.db, this.log, responseID, customerID, LoadBuckets()).Execute().Result;
			} catch (Exception e) {
				throw new InferenceSaverAlert(customerID, requestID, e, this.log);
			} // try
		} // SaveInference

		public ModuleConfiguration LoadModuleConfiguration() {
			return new ModuleConfiguration {
				CacheAcceptanceDays = CurrentValues.Instance.LogicalGlueCacheAcceptanceDays,
				RemoteRequestsEnabled = CurrentValues.Instance.LogicalGlueEnabled,
			};
		} // LoadModuleConfiguration

		public HarvesterConfiguration LoadHarvesterConfiguration() {
			return new HarvesterConfiguration {
				HostName = CurrentValues.Instance.LogicalGlueHostName,
				NewCustomerRequestPath = CurrentValues.Instance.LogicalGlueNewCustomerRequestPath,
				OldCustomerRequestPath = CurrentValues.Instance.LogicalGlueOldCustomerRequestPath,
				UserName = CurrentValues.Instance.LogicalGlueUserName,
				Password = CurrentValues.Instance.LogicalGluePassword,
				AuthorizationScheme = CurrentValues.Instance.LogicalGlueAuthorizationScheme,
			};
		} // LoadHarvesterConfiguration

		public List<Inference> LoadInferenceHistory(
			int customerID,
			DateTime time,
			bool includeTryOuts,
			int? maxHistoryLength
		) {
			try {
				return new InferenceHistoryLoader(
					this.db,
					this.log,
					customerID,
					time,
					includeTryOuts,
					Math.Max(maxHistoryLength ?? 0, 0),
					LoadBuckets()
				).Execute().Results;
			} catch (Exception e) {
				throw new InferenceLoaderAlert(customerID, time, e, this.log);
			} // try
		} // LoadInferenceHistory

		public bool SetRequestIsTryOut(Guid requestID, bool isTryOut) {
			try {
				var sp = new SetRequestIsTryOut(this.db, this.log) {
					RequestUniqueID = requestID,
					NewIsTryOutStatus = isTryOut,
				};

				sp.ExecuteNonQuery();

				return sp.RequestID > 0;
			} catch (Exception e) {
				throw new SetIsTryOutStatusAlert(requestID, isTryOut, e, this.log);
			} // try
		} // SetRequestIsTryOut

		public Inference LoadInferenceIfExists(
			int customerID,
			DateTime time,
			bool includeTryOutData,
			decimal monthlyPayment
		) {
			try {
				return new InferenceLoadAttempter(
					this.db,
					this.log,
					customerID,
					time,
					includeTryOutData,
					monthlyPayment,
					LoadBuckets()
				).Execute().Result;
			} catch (Exception e) {
				throw new InferenceLoaderAlert(customerID, time, e, this.log);
			} // try
		} // LoadInferenceIfExists

		public Bucket FindBucket(int bucketID) {
			return LoadBuckets().Find(bucketID);
		} // FindBucket

		public Bucket FindBucket(string bucket) {
			return LoadBuckets().Find(bucket);
		} // FindBucket

		private BucketRepository LoadBuckets() {
			var repo = new BucketRepository(this.db, this.log);
			repo.Load();
			return repo;
		} // LoadBuckets

		private readonly AConnection db;
		private readonly ASafeLog log;
	} // class Keeper
} // namespace
