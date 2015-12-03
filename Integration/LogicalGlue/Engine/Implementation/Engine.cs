namespace Ezbob.Integration.LogicalGlue.Engine.Implementation {
	using System;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Engine;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;

	public class Engine : IEngine {
		public Engine(IKeeper keeper, IHarvester harvester, ASafeLog log) {
			Log = log.Safe();
			Now = DateTime.UtcNow;

			Keeper = keeper;
			Harvester = harvester;

			if (Keeper == null)
				throw new NoConnectionEngineAlert(Log);

			if (Harvester == null)
				throw new NoHarvesterEngineAlert(Log);
		} // constructor

		public DateTime Now { get; private set; }
		public IKeeper Keeper { get; private set; }
		public IHarvester Harvester { get; private set; }
		public ASafeLog Log { get; private set; }

		public Inference GetInference(int customerID, GetInferenceMode mode) {
			switch (mode) {
			case GetInferenceMode.CacheOnly:
				return GetInference(customerID, Now);

			case GetInferenceMode.DownloadIfOld:
				Inference cachedInference = GetInference(customerID, Now);
				ModuleConfiguration cfg = Keeper.LoadModuleConfiguration();

				if (cachedInference.IsUpToDate(Now, cfg.CacheAcceptanceDays))
					return cachedInference;

				goto case GetInferenceMode.ForceDownload; // !!! fall through !!!

			case GetInferenceMode.ForceDownload:
				return DownloadAndSave(customerID);

			default:
				throw new EngineAlert(
					Log,
					new ArgumentOutOfRangeException("mode"),
					"Failed to get customer {0} inference at mode {1}.",
					customerID,
					mode
				);
			} // switch
		} // GetInference

		public Inference GetInference(int customerID, DateTime time) {
			return Keeper.LoadInference(customerID, time);
		} // GetHistoricalInference

		private Inference DownloadAndSave(int customerID) {
			InferenceInputPackage inputPkg = Keeper.LoadInputData(customerID, Now);

			if (!inputPkg.InferenceInput.IsValid())
				throw new FailedToLoadInputDataAlert(Log, customerID, Now);

			long requestID = Keeper.SaveInferenceRequest(customerID, inputPkg.CompanyID, inputPkg.InferenceInput);

			Response<Reply> reply = Harvester.Infer(inputPkg.InferenceInput, Keeper.LoadHarvesterConfiguration());

			return Keeper.SaveInference(customerID, requestID, reply);
		} // DownloadAndSave
	} // class Engine
} // namespace
