namespace Ezbob.Integration.LogicalGlue.Engine {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils;

	internal class DownloadAndSaveAction : ASimpleRetryer {
		public DownloadAndSaveAction(
			IHarvester harvester,
			IKeeper keeper,
			int customerID,
			decimal explicitMonthlyPayment,
			bool isTryOut,
			DateTime now,
			ASafeLog log
		) : base(3, 5000, log) {
			this.now = now;
			this.harvester = harvester;
			this.keeper = keeper;
			this.customerID = customerID;
			this.explicitMonthlyPayment = explicitMonthlyPayment;
			this.isTryOut = isTryOut;

			Result = null;
		} // constructor

		public Inference Result { get; private set; }

		protected override ActionOutcomes ActionToRetry(int attemptNumber) {
			Result = null;

			Log.Debug(
				"Engine.DownloadAndSave({0}, {1}, {2}, {3}) started...",
				attemptNumber, this.customerID, this.explicitMonthlyPayment, this.isTryOut
			);

			InferenceInputPackage inputPkg = this.keeper.LoadInputData(this.customerID, this.now, false);

			Log.Debug(
				"Engine.DownloadAndSave({0}, {1}, {2}, {3}) retrieved input package.",
				attemptNumber, this.customerID, this.explicitMonthlyPayment, this.isTryOut
			);

			if (this.explicitMonthlyPayment > 0)
				inputPkg.InferenceInput.MonthlyPayment = this.explicitMonthlyPayment;

			List<string> errors = inputPkg.InferenceInput.Validate();

			if (errors != null) {
				Log.Warn(
					"Cannot query Logical Glue for customer {0} due to errors in the input data:\n\t{1}",
					this.customerID,
					string.Join("\n\t", errors)
				);
				return ActionOutcomes.Fatal;
			} // if

			Log.Debug(
				"Engine.DownloadAndSave({0}, {1}, {2}, {3}) input package is valid.",
				attemptNumber, this.customerID, this.explicitMonthlyPayment, this.isTryOut
			);

			long requestID = this.keeper.SaveInferenceRequest(
				this.customerID,
				inputPkg.CompanyID,
				this.isTryOut,
				inputPkg.InferenceInput
			);

			Log.Debug(
				"Engine.DownloadAndSave({0}, {1}, {2}, {3}) input package has been stored.",
				attemptNumber, this.customerID, this.explicitMonthlyPayment, this.isTryOut
			);

			Response<Reply> reply = this.harvester.Infer(inputPkg.InferenceInput, this.keeper.LoadHarvesterConfiguration());

			Log.Debug(
				"Engine.DownloadAndSave({0}, {1}, {2}, {3}) reply received.",
				attemptNumber, this.customerID, this.explicitMonthlyPayment, this.isTryOut
			);

			Result = this.keeper.SaveInference(this.customerID, requestID, reply);

			Log.Debug(
				"Engine.DownloadAndSave({0}, {1}, {2}, {3}) complete.",
				attemptNumber, this.customerID, this.explicitMonthlyPayment, this.isTryOut
			);

			return (Result != null) && (Result.Status != null) && (Result.Status.HttpStatus == HttpStatusCode.OK)
				? ActionOutcomes.Done
				: ActionOutcomes.Retry;
		} // ActionToRetry

		/// <summary>
		/// Short action to retry description for logging purposes.
		/// </summary>
		protected override string ActionDescription {
			get { return "download from LG and save"; }
		} // ActionDescription

		private readonly IKeeper keeper;
		private readonly IHarvester harvester;
		private readonly int customerID;
		private readonly decimal explicitMonthlyPayment;
		private readonly bool isTryOut;
		private readonly DateTime now;
	} // class DownloadAndSaveAction
} // namespace
