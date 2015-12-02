namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class InferenceSaver : AActionBase {
		public InferenceSaver(
			AConnection db,
			ASafeLog log,
			long requestID,
			Response<Reply> response
		) : base(db, log) {
			this.requestID = requestID;
			this.response = response;
			ResponseID = 0;
		} // constructor

		public InferenceSaver Execute() {
			if (this.requestID <= 0)
				throw new InferenceSaverWrongRequestIDAlert(this.requestID, Log);

			if (this.response == null)
				throw new InferenceSaverNoDataAlert(this.requestID, Log);

			if (Executed) {
				Log.Alert(
					"Inference saver({0}, '{1}') has already been executed.",
					this.requestID,
					this.response.ToShortString()
				);
				return this;
			} // if

			Executed = true;

			Log.Debug(
				"Executing inference saver({0}, '{1}')...",
				this.requestID,
				this.response.ToShortString()
			);

			ResponseID = new SaveInference(this.requestID, this.response, DB, Log).ExecuteScalar<long>();

			Log.Debug(
				"Executing inference saver({0}, '{1}') complete, response ID is {2}.",
				this.requestID,
				this.response.ToShortString(),
				ResponseID
			);

			return this;
		} // Execute

		public long ResponseID { get; private set; }

		private readonly long requestID;
		private readonly Response<Reply> response;
	} // class InferenceSaver
} // namespace
