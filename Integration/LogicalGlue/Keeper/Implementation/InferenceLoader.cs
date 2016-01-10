namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	using DBResponse = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Response;
	using DBModelOutput = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.ModelOutput;
	using PublicModelOutput = Ezbob.Integration.LogicalGlue.Engine.Interface.ModelOutput;

	internal class InferenceLoader : ATimedCustomerActionBase {
		public InferenceLoader(
			AConnection db,
			ASafeLog log,
			int customerID,
			DateTime now,
			bool includeTryOutData,
			decimal monthlyPayment
		) : base(db, log, customerID, now) {
			this.responseID = 0;
			this.includeTryOutData = includeTryOutData;
			this.monthlyPayment = monthlyPayment;
			this.models = new SortedDictionary<long, PublicModelOutput>();

			Result = new Inference();
		} // constructor

		internal InferenceLoader(
			AConnection db,
			ASafeLog log,
			long responseID,
			int customerID
		) : base(db, log, customerID, DateTime.UtcNow) {
			this.responseID = responseID;
			this.includeTryOutData = false;
			this.monthlyPayment = 0;
			this.models = new SortedDictionary<long, PublicModelOutput>();

			Result = new Inference();
		} // constructor

		public Inference Result { get; private set; }

		public InferenceLoader Execute() {
			if (Executed) {
				Log.Alert("Inference loader({0}, '{1}') has already been executed.", CustomerID, NowStr);
				return this;
			} // if

			Executed = true;

			Log.Debug("Executing inference loader({0}, '{1}')...", CustomerID, NowStr);

			new LoadInference(DB, Log) {
				ResponseID = this.responseID,
				CustomerID = CustomerID,
				Now = Now,
				IncludeTryOutData = this.includeTryOutData,
				MonthlyPayment = this.monthlyPayment,
			}.ForEachRowSafe(ProcessInferenceRow);

			Log.Debug("Executing inference loader({0}, '{1}') complete.", CustomerID, NowStr);

			return this;
		} // Execute

		private void ProcessInferenceRow(SafeReader sr) {
			string rowTypeName = sr["RowType"];

			RowTypes rowType;

			if (!Enum.TryParse(rowTypeName, false, out rowType)) {
				throw new KeeperAlert(
					Log,
					"Inference loader({1}, '{2}'): unknown row type '{0}'.",
					rowTypeName,
					CustomerID,
					NowStr
				);
			} // if

			switch (rowType) {
			case RowTypes.Response:
				ProcessResponse(sr);
				break;

			case RowTypes.ModelOutput:
				ProcessModelOutput(sr);
				break;

			case RowTypes.OutputRatio:
				ProcessOutputRatio(sr);
				break;

			case RowTypes.Warning:
				ProcessWarning(sr);
				break;

			case RowTypes.EncodingFailure:
				ProcessEncodingFailure(sr);
				break;

			case RowTypes.MissingColumn:
				ProcessMissingColumn(sr);
				break;

			default:
				throw OutOfRangeException(
					"Inference loader({1}, '{2}'): unsupported row type '{0}'.",
					rowTypeName,
					CustomerID,
					NowStr
				);
			} // switch
		} // ProcessInferenceRow

		[StringFormatMethod("format")]
		private KeeperAlert OutOfRangeException(string format, params object[] args) {
			return new KeeperAlert(Log, new ArgumentOutOfRangeException(), format, args);
		} // OutOfRangeException

		private void ProcessResponse(SafeReader sr) {
			Result.UniqueID = sr["UniqueID"];
			Result.MonthlyRepayment = sr["MonthlyRepayment"];

			var dbResponse = sr.Fill<DBResponse>();

			Result.ResponseID = dbResponse.ID;
			Result.ReceivedTime = dbResponse.ReceivedTime;
			Result.Bucket = dbResponse.BucketID == null ? (Bucket?)null : (Bucket)(int)dbResponse.BucketID;

			Result.Error = new InferenceError {
				Message = dbResponse.ErrorMessage,
				ParsingExceptionMessage = dbResponse.ParsingExceptionMessage,
				ParsingExceptionType = dbResponse.ParsingExceptionType,
				TimeoutSource = dbResponse.TimeoutSourceID == null
					? (TimeoutSources?)null
					: (TimeoutSources)dbResponse.TimeoutSourceID.Value,
			};

			Result.Status = new InferenceStatus {
				HasEquifaxData = dbResponse.HasEquifaxData,
				HttpStatus = (HttpStatusCode)dbResponse.HttpStatus,
				ResponseStatus = (HttpStatusCode)dbResponse.ResponseStatus,
			};

			Log.Debug(
				"Inference loader({0}, '{1}'): loaded response (id: {2}).",
				CustomerID,
				NowStr,
				Result.ResponseID
			);
		} // ProcessResponse

		private void ProcessModelOutput(SafeReader sr) {
			DBModelOutput dbModel = sr.Fill<DBModelOutput>();

			if (!Enum.IsDefined(typeof(ModelNames), dbModel.ModelID)) {
				throw OutOfRangeException(
					"inference loader({1}, '{2}'): unsupported request type '{0}'.",
					dbModel.ModelID,
					CustomerID,
					NowStr
				);
			} // if

			var pubModel = new PublicModelOutput {
				Status = dbModel.Status,
				Grade = new Grade {
					DecodedResult = dbModel.InferenceResultDecoded,
					EncodedResult = dbModel.InferenceResultEncoded,
					Score = dbModel.Score,
				},
				Error = new ModelError {
					ErrorCode = dbModel.ErrorCode,
					Exception = dbModel.Exception,
					Uuid = dbModel.Uuid,
				},
			};

			ModelNames requestType = (ModelNames)dbModel.ModelID;

			this.models[dbModel.ID] = pubModel;
			Result.ModelOutputs[requestType] = pubModel;

			Log.Debug(
				"Inference loader({0}, '{1}'): loaded model output (id: {2}, type: {3}).",
				CustomerID,
				NowStr,
				dbModel.ID,
				requestType
			);
		} // ProcessModelOutput

		private void ProcessOutputRatio(SafeReader sr) {
			OutputRatio ratio = sr.Fill<OutputRatio>();

			if (this.models.ContainsKey(ratio.ModelOutputID)) {
				this.models[ratio.ModelOutputID].Grade.OutputRatios[ratio.OutputClass] = ratio.Score;

				Log.Debug(
					"Inference loader({0}, '{1}'): loaded map output ratio ({2}: {3}).",
					CustomerID,
					NowStr,
					ratio.OutputClass,
					ratio.Score
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}, '{1}'): map output ratio '{2}' should belong to unknown model '{3}'.",
					CustomerID,
					NowStr,
					ratio.ID,
					ratio.ModelOutputID
				);
			} // if
		} // ProcessOutputRatio

		private void ProcessWarning(SafeReader sr) {
			DBTable.Warning warning = sr.Fill<DBTable.Warning>();

			if (this.models.ContainsKey(warning.ModelOutputID)) {
				this.models[warning.ModelOutputID].Grade.Warnings.Add(new Engine.Interface.Warning {
					FeatureName = warning.FeatureName,
					MaxValue = warning.MaxValue,
					MinValue = warning.MinValue,
					Value = warning.Value,
				});

				Log.Debug(
					"Inference loader({0}, '{1}'): loaded warning ({2}: '{3}' should be between '{4}' and '{5}').",
					CustomerID,
					NowStr,
					warning.FeatureName,
					warning.Value,
					warning.MinValue,
					warning.MaxValue
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}, '{1}'): warning '{2}' should belong to unknown model '{3}'.",
					CustomerID,
					NowStr,
					warning.ID,
					warning.ModelOutputID
				);
			} // if
		} // ProcessWarning

		private void ProcessEncodingFailure(SafeReader sr) {
			DBTable.EncodingFailure encodingFailure = sr.Fill<DBTable.EncodingFailure>();

			if (this.models.ContainsKey(encodingFailure.ModelOutputID)) {
				this.models[encodingFailure.ModelOutputID].Error.EncodingFailures.Add(
					new Engine.Interface.EncodingFailure {
						ColumnName = encodingFailure.ColumnName,
						Message = encodingFailure.Message,
						Reason = encodingFailure.Reason,
						RowIndex = encodingFailure.RowIndex,
						UnencodedValue = encodingFailure.UnencodedValue,
					}
				);

				Log.Debug(
					"Inference loader({0}, '{1}'): loaded encoding failure ('{2}': '{3}'; unenc: '{4}', msg: '{5}').",
					CustomerID,
					NowStr,
					encodingFailure.ColumnName,
					encodingFailure.Reason,
					encodingFailure.UnencodedValue,
					encodingFailure.Message
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}, '{1}'): encoding failure '{2}' should belong to unknown model '{3}'.",
					CustomerID,
					NowStr,
					encodingFailure.ID,
					encodingFailure.ModelOutputID
				);
			} // if
		} // ProcessEncodingFailure

		private void ProcessMissingColumn(SafeReader sr) {
			MissingColumn column = sr.Fill<MissingColumn>();

			if (this.models.ContainsKey(column.ModelOutputID)) {
				this.models[column.ModelOutputID].Error.MissingColumns.Add(column.ColumnName);

				Log.Debug(
					"Inference loader({0}, '{1}'): loaded missing column ({2}).",
					CustomerID,
					NowStr,
					column.ColumnName
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}, '{1}'): missing column '{2}' should belong to unknown model '{3}'.",
					CustomerID,
					NowStr,
					column.ID,
					column.ModelOutputID
				);
			} // if
		} // ProcessMissingColumn

		private enum RowTypes {
			Response,
			ModelOutput,
			OutputRatio,
			Warning,
			EncodingFailure,
			MissingColumn,
		} // enum RowTypes

		private readonly SortedDictionary<long, PublicModelOutput> models;
		private readonly long responseID;
		private readonly bool includeTryOutData;
		private readonly decimal monthlyPayment;
	} // class InferenceLoader
} // namespace
