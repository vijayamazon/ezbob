namespace Ezbob.Integration.LogicalGlue.Engine.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Engine;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;

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

		public Inference GetInference(int customerID, decimal monthlyPayment, bool isTryout, GetInferenceMode mode) {
			Log.Debug("Engine.GetInference({0}, {1}) started...", customerID, mode);

			Inference result = null;

			switch (mode) {
			case GetInferenceMode.CacheOnly:
				result = GetInference(customerID, Now, isTryout, monthlyPayment);
				break;

			case GetInferenceMode.DownloadIfOld:
				Inference cachedInference = GetInference(customerID, Now, isTryout, monthlyPayment);
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
				result = DownloadAndSave(customerID, monthlyPayment, isTryout);
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
		} // GetInference (standard, by actual data)

		public Inference GetInference(
			int customerID,
			DateTime time,
			bool includeTryOutData,
			decimal explicitMonthlyPayment
		) {
			Log.Debug(
				"Engine.GetInference({0}, {1}) started...",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

			Inference result = Keeper.LoadInference(customerID, time, includeTryOutData, explicitMonthlyPayment);

			Log.Debug(
				"Engine.GetInference({0}, {1}) complete.",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture)
			);

			return result;
		} // GetInference (historical, by date [and payment])

		public List<Inference> GetInferenceHistory(
			int customerID,
			DateTime time,
			bool includeTryOuts,
			int? maxHistoryLength = null
		) {
			Log.Debug(
				"Engine.GetInferenceHistory({0}, {1}, {2}, {3}) started...",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				includeTryOuts,
				maxHistoryLength == null ? "entire history" : Grammar.Number(maxHistoryLength.Value, "item")
			);

			List<Inference> result = Keeper.LoadInferenceHistory(
				customerID,
				time,
				includeTryOuts,
				maxHistoryLength
			);

			Log.Debug(
				"Engine.GetInferenceHistory({0}, {1}, {2}, {3}) complete.",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				includeTryOuts,
				maxHistoryLength == null ? "entire history" : Grammar.Number(maxHistoryLength.Value, "item")
			);

			return result;
		} // GetInferenceHistory

		public bool SetRequestIsTryOut(Guid requestID, bool isTryOut) {
			Log.Debug("Enginge.SetRequestIsTryOut({0}, {1}) started...", requestID, isTryOut);

			bool result = Keeper.SetRequestIsTryOut(requestID, isTryOut);

			Log.Debug("Enginge.SetRequestIsTryOut({0}, {1}) complete with result: {2}.", requestID, isTryOut, result);

			return result;
		} // SetRequestIsTryOut

		public decimal GetMonthlyRepayment(int customerID, DateTime now) {
			return GetMonthlyRepaymentData(customerID, now).MonthlyRepayment;
		} // GetMonthlyRepayment

		public MonthlyRepaymentData GetMonthlyRepaymentData(int customerID, DateTime now) {
			string nowStr = now.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

			Log.Debug("Engine.GetMonthlyRepaymentData({0}, {1}) started...", customerID, nowStr);

			InferenceInputPackage inputPkg = Keeper.LoadInputData(customerID, Now, true);

			Log.Debug("Engine.GetMonthlyRepaymentData({0}, {1}) retrieved input package.", customerID, nowStr);

			return new MonthlyRepaymentData {
				RequestedAmount = inputPkg.InferenceInput.RequestedAmount,
				RequestedTerm = inputPkg.InferenceInput.RequestedTerm,
				MonthlyRepayment = inputPkg.InferenceInput.MonthlyPayment ?? 0,
			};
		} // GetMonthlyRepaymentData

		public Inference GetInferenceIfExists(
			int customerID,
			DateTime time,
			bool includeTryOutData,
			decimal monthlyPayment
		) {
			Log.Debug(
				"Engine.GetInferenceIfExists({0}, {1}, {2}, {3}) started...",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				includeTryOutData,
				monthlyPayment
			);

			Inference result = Keeper.LoadInferenceIfExists(customerID, time, includeTryOutData, monthlyPayment);

			Log.Debug(
				"Engine.GetInference({0}, {1}, {2}, {3}) complete.",
				customerID,
				time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				includeTryOutData,
				monthlyPayment
			);

			return result;
			
		} // GetInferenceIfExists

		private Inference DownloadAndSave(int customerID, decimal explicitMonthlyPayment, bool isTryOut) {
			ModuleConfiguration cfg = Keeper.LoadModuleConfiguration();

			if (!cfg.RemoteRequestsEnabled) {
				Log.Debug(
					"Engine.DownloadAndSave({0}, {1}, {2}): not calling remote API - calls are disabled.",
					customerID,
					explicitMonthlyPayment,
					isTryOut
				);

				return null;
			} // if

			var action = new DownloadAndSaveAction(
				Harvester,
				Keeper,
				customerID,
				explicitMonthlyPayment,
				isTryOut,
				Now,
				Log
			);

			action.Execute();

			return action.Result;
		} // DownloadAndSave
	} // class Engine
} // namespace
