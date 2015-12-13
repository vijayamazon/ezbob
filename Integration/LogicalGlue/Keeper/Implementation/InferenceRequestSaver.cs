namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class InferenceRequestSaver : ACustomerActionBase {
		public InferenceRequestSaver(
			AConnection db,
			ASafeLog log,
			int customerID,
			int companyID,
			bool isTryOut,
			InferenceInput request
		) : base(db, log, customerID) {
			Result = 0;
			this.companyID = companyID;
			this.isTryOut = isTryOut;
			this.request = request;
		} // constructor

		public long Result { get; private set; }

		public InferenceRequestSaver Execute() {
			if (this.request == null)
				throw new InferenceRequestSaverNoDataAlert(CustomerID, null, Log);

			if (Executed) {
				Log.Alert(
					"Inference request saver({0}, '{1}') has already been executed.",
					CustomerID,
					this.request.ToShortString()
				);
				return this;
			} // if

			Executed = true;

			Log.Debug(
				"Executing inference request saver({0}, {1}, '{2}')...",
				CustomerID,
				this.companyID,
				this.request.ToShortString()
			);

			Result = new SaveInferenceRequest(
				CustomerID,
				this.companyID,
				this.isTryOut,
				this.request,
				DB,
				Log
			).ExecuteScalar<long>();

			Log.Debug(
				"Executing inference request saver({0}, {1}, '{2}') complete, saved request id is {3}.",
				CustomerID,
				this.companyID,
				this.request.ToShortString(),
				Result
			);

			return this;
		} // Execute

		private readonly int companyID;
		private readonly bool isTryOut;
		private readonly InferenceInput request;
	} // class InferenceRequestSaver
} // namespace
