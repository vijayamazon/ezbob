namespace Ezbob.Integration.LogicalGlue.Engine.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
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
			Log.Debug("Engine.GetInference({0}, {1}) started...", customerID, mode);

			Inference result = null;

			switch (mode) {
			case GetInferenceMode.CacheOnly:
				result = GetInference(customerID, Now);
				break;

			case GetInferenceMode.DownloadIfOld:
				Inference cachedInference = GetInference(customerID, Now);
				ModuleConfiguration cfg = Keeper.LoadModuleConfiguration();

				if (cachedInference.IsUpToDate(Now, cfg.CacheAcceptanceDays)) {
					Log.Debug(
						"Engine.GetInference({0}, {1}): returning cached inference with ResponseID = {2}.",
						customerID,
						mode,
						cachedInference.ResponseID
					);

					result = cachedInference;
					break;
				} // if

				goto case GetInferenceMode.ForceDownload; // !!! fall through !!!

			case GetInferenceMode.ForceDownload:
				result = DownloadAndSave(customerID);
				break;

			default:
				throw new EngineAlert(
					Log,
					new ArgumentOutOfRangeException("mode"),
					"Failed to get customer {0} inference at mode {1}.",
					customerID,
					mode
				);
			} // switch

			Log.Debug("Engine.GetInference({0}, {1}) complete.", customerID, mode);

			return result;
		} // GetInference

		public Inference GetInference(int customerID, DateTime time) {
			Log.Debug(
				"Engine.GetInference({0}, {1}) started...",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

			Inference result = Keeper.LoadInference(customerID, time);

			Log.Debug(
				"Engine.GetInference({0}, {1}) complete.",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

			return result;
		} // GetHistoricalInference

		private Inference DownloadAndSave(int customerID) {
			Log.Debug("Engine.DownloadAndSave({0}) started...", customerID);

			InferenceInputPackage inputPkg = Keeper.LoadInputData(customerID, Now);

			Log.Debug("Engine.DownloadAndSave({0}) retrieved input package.", customerID);

			List<string> errors = inputPkg.InferenceInput.Validate();

			if (errors != null)
				throw new FailedToLoadInputDataAlert(Log, customerID, Now, errors);

			Log.Debug("Engine.DownloadAndSave({0}) input package is valid.", customerID);

			long requestID = Keeper.SaveInferenceRequest(customerID, inputPkg.CompanyID, inputPkg.InferenceInput);

			Log.Debug("Engine.DownloadAndSave({0}) input package is persisted.", customerID);

			Response<Reply> reply = Harvester.Infer(inputPkg.InferenceInput, Keeper.LoadHarvesterConfiguration());

			Log.Debug("Engine.DownloadAndSave({0}) reply received.", customerID);

			Inference result = Keeper.SaveInference(customerID, requestID, reply);

			Log.Debug("Engine.DownloadAndSave({0}) complete.", customerID);

			return result;
		} // DownloadAndSave
	} // class Engine
} // namespace
