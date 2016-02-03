namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Exceptions.Keeper;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	using DBResponse = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Response;
	using DBEtlData = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.EtlData;
	using DBModelOutput = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.ModelOutput;

	using PublicEtlData = Ezbob.Integration.LogicalGlue.Engine.Interface.EtlData;
	using PublicModelOutput = Ezbob.Integration.LogicalGlue.Engine.Interface.ModelOutput;

	internal abstract class AInferenceLoaderBase : ATimedCustomerActionBase {
		protected AInferenceLoaderBase(
			AConnection db,
			ASafeLog log,
			long responseID,
			int customerID,
			DateTime now,
			int historyLength,
			bool includeTryOutData,
			decimal monthlyPayment
		) : base(db, log, customerID, now) {
			this.resultSet = new SortedDictionary<long, Inference>();
			this.models = new SortedDictionary<long, PublicModelOutput>();
			Results = new List<Inference>();

			this.responseID = responseID;
			this.historyLength = historyLength;
			this.includeTryOutData = includeTryOutData;
			this.monthlyPayment = monthlyPayment;
		} // constructor

		public List<Inference> Results { get; private set; }

		protected void Load() {
			if (Executed) {
				Log.Alert("Inference loader({0}) has already been executed.", this.argList);
				return;
			} // if

			LoadInference sp = CreateLoadInferenceProcedure();

			sp.ResponseID = this.responseID;
			sp.CustomerID = CustomerID;
			sp.Now = Now;
			sp.HistoryLength = this.historyLength;
			sp.IncludeTryOutData = this.includeTryOutData;
			sp.MonthlyPayment = this.monthlyPayment;

			this.argList = string.Format(
				"response ID: {0}, customer ID: {1}, now: '{2}', history len: {3}, try outs: '{4}', payment: {5}",
				sp.ResponseID,
				sp.CustomerID,
				NowStr,
				sp.HistoryLength,
				sp.IncludeTryOutData ? "yes" : "no",
				sp.MonthlyPayment
			);

			Executed = true;

			Log.Debug("Executing inference loader({0})...", this.argList);

			sp.ForEachRowSafe(ProcessInferenceRow);

			Log.Debug("Executing inference loader({0}) complete.", this.argList);
		} // Load

		protected virtual LoadInference CreateLoadInferenceProcedure() {
			return new LoadInference(DB, Log);
		} // CreateLoadInferenceProcedure

		private void ProcessInferenceRow(SafeReader sr) {
			string rowTypeName = sr["RowType"];

			RowTypes rowType;

			if (!Enum.TryParse(rowTypeName, false, out rowType))
				throw new KeeperAlert(Log, "Inference loader({1}): unknown row type '{0}'.", rowTypeName, this.argList);

			switch (rowType) {
			case RowTypes.Request:
				ProcessRequest(sr);
				break;

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

			case RowTypes.ETL:
				ProcessEtl(sr);
				break;

			default:
				throw OutOfRangeException("Inference loader({1}): unsupported row type '{0}'.", rowTypeName, this.argList);
			} // switch
		} // ProcessInferenceRow

		[StringFormatMethod("format")]
		private KeeperAlert OutOfRangeException(string format, params object[] args) {
			return new KeeperAlert(Log, new ArgumentOutOfRangeException(), format, args);
		} // OutOfRangeException

		private Inference ProcessRequest(SafeReader sr) {
			Guid? uniqueID = sr["UniqueID"];

			if (uniqueID == null)
				return null;

			var result = new Inference {
				UniqueID = uniqueID.Value,
				MonthlyRepayment = sr["MonthlyRepayment"],
				IsTryOut = sr["IsTryOut"],
			};

			Results.Add(result);

			return result;
		} // ProcessRequest

		private void ProcessResponse(SafeReader sr) {
			var dbResponse = sr.Fill<DBResponse>();

			var result = ProcessRequest(sr);

			if (result == null) {
				throw OutOfRangeException(
					"Inference loader({0}): response '{1}' has no unique request ID.",
					this.argList,
					dbResponse.ID
				);
			} // if

			result.ResponseID = dbResponse.ID;
			result.ReceivedTime = dbResponse.ReceivedTime;
			result.Bucket = dbResponse.BucketID == null ? (Bucket?)null : (Bucket)(int)dbResponse.BucketID;
			result.Reason = dbResponse.Reason;
			result.Outcome = dbResponse.Outcome;

			result.Error = new InferenceError {
				Message = dbResponse.ErrorMessage,
				ParsingExceptionMessage = dbResponse.ParsingExceptionMessage,
				ParsingExceptionType = dbResponse.ParsingExceptionType,
				TimeoutSource = dbResponse.TimeoutSourceID == null
					? (TimeoutSources?)null
					: (TimeoutSources)dbResponse.TimeoutSourceID.Value,
			};

			result.Status = new InferenceStatus {
				HasEquifaxData = dbResponse.HasEquifaxData,
				HttpStatus = (HttpStatusCode)dbResponse.HttpStatus,
				ResponseStatus = (HttpStatusCode)dbResponse.ResponseStatus,
			};

			this.resultSet[result.ResponseID] = result;

			Log.Debug("Inference loader({0}): loaded response (id: {1}).", this.argList, result.ResponseID);
		} // ProcessResponse

		private void ProcessEtl(SafeReader sr) {
			DBEtlData dbEtl = sr.Fill<DBEtlData>();

			if (!this.resultSet.ContainsKey(dbEtl.ResponseID)) {
				throw OutOfRangeException(
					"Inference loader({0}): ETL '{1}' should belong to unknown response '{2}'.",
					this.argList,
					dbEtl.ID,
					dbEtl.ResponseID
				);
			} // if

			EtlCode? code = null;

			if (dbEtl.EtlCodeID != null) {
				int[] x = (int[])Enum.GetValues(typeof(EtlCode));

				if (!x.Contains((int)dbEtl.EtlCodeID.Value)) {
					throw OutOfRangeException(
						"Inference loader({0}): ETL '{1}' has unsupported ETL code value of '{2}'.",
						this.argList,
						dbEtl.ID,
						dbEtl.EtlCodeID.Value
					);
				} // if

				code = (EtlCode)(int)dbEtl.EtlCodeID.Value;
			} // if

			this.resultSet[dbEtl.ResponseID].Etl = new PublicEtlData {
				Code = code,
				Message = dbEtl.Message,
			};
		} // ProcessEtl

		private void ProcessModelOutput(SafeReader sr) {
			DBModelOutput dbModel = sr.Fill<DBModelOutput>();

			if (!Enum.IsDefined(typeof(ModelNames), dbModel.ModelID)) {
				throw OutOfRangeException(
					"inference loader({1}): unsupported request type '{0}'.",
					dbModel.ModelID,
					this.argList
				);
			} // if

			if (!this.resultSet.ContainsKey(dbModel.ResponseID)) {
				throw OutOfRangeException(
					"Inference loader({0}): model output '{1}' should belong to unknown response '{2}'.",
					this.argList,
					dbModel.ID,
					dbModel.ResponseID
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
			this.resultSet[dbModel.ResponseID].ModelOutputs[requestType] = pubModel;

			Log.Debug(
				"Inference loader({0}): loaded model output (id: {1}, type: {2}).",
				this.argList,
				dbModel.ID,
				requestType
			);
		} // ProcessModelOutput

		private void ProcessOutputRatio(SafeReader sr) {
			OutputRatio ratio = sr.Fill<OutputRatio>();

			if (this.models.ContainsKey(ratio.ModelOutputID)) {
				this.models[ratio.ModelOutputID].Grade.OutputRatios[ratio.OutputClass] = ratio.Score;

				Log.Debug(
					"Inference loader({0}): loaded map output ratio ({1}: {2}).",
					this.argList,
					ratio.OutputClass,
					ratio.Score
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}): map output ratio '{1}' should belong to unknown model '{2}'.",
					this.argList,
					ratio.ID,
					ratio.ModelOutputID
				);
			} // if
		} // ProcessOutputRatio

		private void ProcessWarning(SafeReader sr) {
			DBTable.Warning warning = sr.Fill<DBTable.Warning>();

			if (this.models.ContainsKey(warning.ModelOutputID)) {
				this.models[warning.ModelOutputID].Error.Warnings.Add(new Engine.Interface.Warning {
					FeatureName = warning.FeatureName,
					MaxValue = warning.MaxValue,
					MinValue = warning.MinValue,
					Value = warning.Value,
				});

				Log.Debug(
					"Inference loader({0}): loaded warning ({1}: '{2}' should be between '{3}' and '{4}').",
					this.argList,
					warning.FeatureName,
					warning.Value,
					warning.MinValue,
					warning.MaxValue
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}): warning '{1}' should belong to unknown model '{2}'.",
					this.argList,
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
					"Inference loader({0}): loaded encoding failure ('{1}': '{2}'; unenc: '{3}', msg: '{4}').",
					this.argList,
					encodingFailure.ColumnName,
					encodingFailure.Reason,
					encodingFailure.UnencodedValue,
					encodingFailure.Message
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}): encoding failure '{1}' should belong to unknown model '{2}'.",
					this.argList,
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
					"Inference loader({0}): loaded missing column ({1}).",
					this.argList,
					column.ColumnName
				);
			} else {
				throw OutOfRangeException(
					"Inference loader({0}): missing column '{1}' should belong to unknown model '{2}'.",
					this.argList,
					column.ID,
					column.ModelOutputID
				);
			} // if
		} // ProcessMissingColumn

		private enum RowTypes {
			Request,
			Response,
			ModelOutput,
			OutputRatio,
			Warning,
			EncodingFailure,
			MissingColumn,
			ETL,
		} // enum RowTypes

		private readonly SortedDictionary<long, PublicModelOutput> models;
		private readonly SortedDictionary<long, Inference> resultSet;
		private readonly long responseID;
		private readonly int historyLength;
		private readonly bool includeTryOutData;
		private readonly decimal monthlyPayment;
		private string argList;
	} // class InferenceLoader
} // namespace
