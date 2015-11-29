namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable;
	using Ezbob.Logger;

	using DBModelOutput = Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.ModelOutput;
	using PublicModelOutput = Ezbob.Integration.LogicalGlue.Engine.Interface.ModelOutput;

	internal class InferenceLoader : AActionBase {
		public InferenceLoader(AConnection db, ASafeLog log, int customerID, DateTime time) : base(db, log, customerID) {
			this.time = time;
			this.timeStr = this.time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

			this.models = new SortedDictionary<long, PublicModelOutput>();
			this.executed = false;

			Result = new Inference();
		} // constructor

		public Inference Result { get; private set; }

		public InferenceLoader Execute() {
			if (this.executed) {
				Log.Alert(
					"Inference loader({0}, '{1}') has already been executed.",
					CustomerID,
					this.timeStr
				);

				return this;
			} // if

			this.executed = true;

			Log.Debug("Executing inference loader({0}, '{1}')...", CustomerID, this.timeStr);

			DB.ForEachRowSafe(
				ProcessInferenceRow,
				"LogicalGlueLoadInference",
				new QueryParameter("@CustomerID", CustomerID),
				new QueryParameter("@Now", this.time)
			);

			Log.Debug(
				"Executing inference loader({0}, '{1}') complete.",
				CustomerID,
				this.timeStr
			);

			return this;
		} // Execute

		private void ProcessInferenceRow(SafeReader sr) {
			string err;

			string rowTypeName = sr["RowType"];

			RowTypes rowType;

			if (!Enum.TryParse(rowTypeName, false, out rowType)) {
				err = string.Format(
					"Inference loader({1}, '{2}'): unknown row type '{0}'.",
					rowTypeName,
					CustomerID,
					this.timeStr
				);

				Log.Alert("{0}", err);
				
				throw new Exception(err);
			} // if

			switch (rowType) {
			case RowTypes.Response:
				sr.Fill(Result);

				Log.Debug(
					"Inference loader({0}, '{1}'): loaded response (id: {2}).",
					CustomerID,
					this.timeStr,
					sr["ResponseID"]
				);

				break;

			case RowTypes.ModelOutput:
				DBModelOutput dbModel = sr.Fill<DBModelOutput>();

				if (!Enum.IsDefined(typeof(RequestType), dbModel.RequestTypeID)) {
					err = string.Format(
						"inference loader({1}, '{2}'): unsupported request type '{0}'.",
						dbModel.RequestTypeID,
						CustomerID,
						this.timeStr
					);

					Log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				var pubModel = new PublicModelOutput {
					Status = dbModel.Status,
					Grade = new Grade {
						DecodedResult = dbModel.InferenceResultDecoded,
						EncodedResult = dbModel.InferenceResultEncoded,
						Score = dbModel.Score,
					},
					Error = new Error {
						ErrorCode = dbModel.ErrorCode,
						Exception = dbModel.Exception,
						Uuid = dbModel.Uuid,
					},
				};

				RequestType requestType = (RequestType)dbModel.RequestTypeID;

				this.models[dbModel.ResponseID] = pubModel;
				Result.ModelOutputs[requestType] = pubModel;

				Log.Debug(
					"Inference loader({0}, '{1}'): loaded model output (id: {2}, type: {3}).",
					CustomerID,
					this.timeStr,
					sr["ModelOutputID"],
					requestType
				);

				break;

			case RowTypes.OutputRatio:
				OutputRatio ratio = sr.Fill<OutputRatio>();

				if (this.models.ContainsKey(ratio.ModelOutputID)) {
					this.models[ratio.ModelOutputID].Grade.OutputRatios[ratio.OutputClass] = ratio.Score;

					Log.Debug(
						"Inference loader({0}, '{1}'): loaded map output ratio ({2}: {3}).",
						CustomerID,
						this.timeStr,
						ratio.OutputClass,
						ratio.Score
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): map output ratio '{2}' should belong to unknown response '{3}'.",
						CustomerID,
						this.timeStr,
						sr["OutputRatioID"],
						ratio.ModelOutputID
					);

					Log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			case RowTypes.Warning:
				DBTable.Warning warning = sr.Fill<DBTable.Warning>();

				if (this.models.ContainsKey(warning.ModelOutputID)) {
					this.models[warning.ModelOutputID].Grade.Warnings.Add(new Engine.Interface.Warning {
						FeatureName = warning.FeatureName,
						MaxValue = warning.MaxValue,
						MinValue = warning.MinValue,
						Value = warning.Value,
					});

					Log.Debug(
						"Inference loader({0}, '{1}'): loaded warning ({2}: {3}).",
						CustomerID,
						this.timeStr,
						warning.FeatureName,
						warning.Value
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): warning '{2}' should belong to unknown response '{3}'.",
						CustomerID,
						this.timeStr,
						sr["WarningID"],
						warning.ModelOutputID
					);

					Log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			case RowTypes.EncodingFailure:
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
						"Inference loader({0}, '{1}'): loaded encoding failure ({2}: {3}).",
						CustomerID,
						this.timeStr,
						encodingFailure.ColumnName,
						encodingFailure.Reason
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): encoding failure '{2}' should belong to unknown response '{3}'.",
						CustomerID,
						this.timeStr,
						sr["FailureID"],
						encodingFailure.ModelOutputID
					);

					Log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			case RowTypes.MissingColumn:
				MissingColumn column = sr.Fill<MissingColumn>();

				if (this.models.ContainsKey(column.ModelOutputID)) {
					this.models[column.ModelOutputID].Error.MissingColumns.Add(column.ColumnName);

					Log.Debug(
						"Inference loader({0}, '{1}'): loaded missing column ({2}).",
						CustomerID,
						this.timeStr,
						column.ColumnName
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): missing column '{2}' should belong to unknown response '{3}'.",
						CustomerID,
						this.timeStr,
						sr["MissingColumnID"],
						column.ModelOutputID
					);

					Log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			default:
				err = string.Format(
					"Inference loader({1}, '{2}'): unsupported row type '{0}'.",
					rowTypeName,
					CustomerID,
					this.timeStr
				);

				Log.Alert("{0}", err);

				throw new ArgumentOutOfRangeException(err, (Exception)null);
			} // switch
		} // ProcessInferenceRow

		private enum RowTypes {
			Response,
			ModelOutput,
			OutputRatio,
			Warning,
			EncodingFailure,
			MissingColumn,
		} // enum RowTypes

		private bool executed;

		private readonly SortedDictionary<long, PublicModelOutput> models;

		private readonly DateTime time;
		private readonly string timeStr;
	} // class InferenceLoader
} // namespace
