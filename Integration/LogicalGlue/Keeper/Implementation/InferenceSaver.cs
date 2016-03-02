namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Harvester.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;

	internal class InferenceSaver : AActionBase {
		public InferenceSaver(
			AConnection db,
			ASafeLog log,
			long requestID,
			Response<Reply> response,
			BucketRepository bucketRepo,
			TimeoutSourceRepository timeoutSourceRepo,
			EtlCodeRepository etlCodeRepo
		) : base(db, log) {
			this.requestID = requestID;
			this.response = response;
			this.bucketRepo = bucketRepo;
			this.timeoutSourceRepo = timeoutSourceRepo;
			this.etlCodeRepo = etlCodeRepo;
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

			ConnectionWrapper con = DB.GetPersistent();

			con.BeginTransaction();

			try {
				new SaveRawResponse(this.requestID, this.response, DB, Log).ExecuteNonQuery(con);

				ResponseID = new SaveResponse(
					this.requestID,
					this.response,
					this.bucketRepo,
					this.timeoutSourceRepo,
					DB,
					Log
				).Execute(con);

				if (this.response.Parsed.HasInference()) {
					var map = new SortedDictionary<ModelNames, long>();

					var saveMo = new SaveModelOutput(ResponseID, this.response, DB, Log);

					if (saveMo.HasValidParameters()) {
						saveMo.ForEachRowSafe(con, sr => {
							long id = sr["ModelOutputID"];
							ModelNames name = (ModelNames)(int)(long)sr["ModelID"];

							map[name] = id;
						});

						var saveEf = new SaveEncodingFailure(map, this.response, DB, Log);
						if (saveEf.HasValidParameters()) // invalid if e.g. no failures
							saveEf.ExecuteNonQuery(con);

						var saveMi = new SaveMissingColumn(map, this.response, DB, Log);
						if (saveMi.HasValidParameters()) // invalid if e.g. no missing columns
							saveMi.ExecuteNonQuery(con);

						var saveOr = new SaveOutputRatio(map, this.response, DB, Log);
						if (saveOr.HasValidParameters()) // invalid if e.g. no output ratio
							saveOr.ExecuteNonQuery(con);

						var saveW = new SaveWarning(map, this.response, DB, Log);
						if (saveW.HasValidParameters()) // invalid if e.g. no output ratio
							saveW.ExecuteNonQuery(con);
					} // if
				} // if

				var saveEtl = new SaveEtlData(ResponseID, this.response, this.etlCodeRepo, DB, Log);
				if (saveEtl.HasValidParameters()) // invalid if e.g. no ETL data
					saveEtl.Execute(con);

				new SaveCustomerHistory(ResponseID, DB, Log).ExecuteNonQuery(con);

				con.Commit();
			} catch (Exception e) {
				con.Rollback();

				Log.Warn(
					"Executing inference saver({0}, '{1}') failed because of exception: '{2}'.",
					this.requestID,
					this.response.ToShortString(),
					e.Message
				);

				throw;
			} // try

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
		private readonly BucketRepository bucketRepo;
		private readonly TimeoutSourceRepository timeoutSourceRepo;
		private readonly EtlCodeRepository etlCodeRepo;
	} // class InferenceSaver
} // namespace
