namespace Ezbob.Integration.LogicalGlue.Engine.Implementation {
	using System;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Engine;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;

	public class Engine : IEngine {
		public Engine(IKeeper keeper, IHarvester harvester, ASafeLog log) {
			this.log = log.Safe();
			this.now = DateTime.UtcNow;

			this.keeper = keeper;
			this.harvester = harvester;

			if (this.keeper == null)
				throw new NoConnectionEngineAlert(this.log);

			if (this.harvester == null)
				throw new NoHarvesterEngineAlert(this.log);
		} // constructor

		public Inference GetInference(int customerID, GetInferenceMode mode) {
			switch (mode) {
			case GetInferenceMode.CacheOnly:
				return GetInference(customerID, this.now);

			case GetInferenceMode.DownloadIfOld:
				Inference cachedInference = GetInference(customerID, this.now);
				Configuration cfg = this.keeper.LoadConfiguration();

				if (cachedInference.IsUpToDate(this.now, cfg.CacheAcceptanceDays))
					return cachedInference;

				goto case GetInferenceMode.ForceDownload; // !!! fall through !!!

			case GetInferenceMode.ForceDownload:
				return DownloadAndSave(customerID);

			default:
				throw new EngineAlert(
					this.log,
					new ArgumentOutOfRangeException("mode"),
					"Failed to get customer {0} inference at mode {1}.",
					customerID,
					mode
				);
			} // switch
		} // GetInference

		public Inference GetInference(int customerID, DateTime time) {
			return this.keeper.LoadInference(customerID, time);
		} // GetHistoricalInference

		private Inference DownloadAndSave(int customerID) {
			InferenceInput inputData = this.keeper.LoadInputData(customerID, this.now);

			if (!inputData.IsValid())
				throw new FailedToLoadInputDataAlert(this.log, customerID, this.now);

			long requestID = this.keeper.SaveInferenceRequest(customerID, inputData);

			Response<Reply> reply = this.harvester.Infer(inputData);

			return this.keeper.SaveInference(customerID, requestID, reply);
		} // DownloadAndSave

		private readonly DateTime now;
		private readonly IKeeper keeper;
		private readonly IHarvester harvester;
		private readonly ASafeLog log;
	} // class Engine
} // namespace
