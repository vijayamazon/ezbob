namespace Ezbob.Integration.LogicalGlue.Keeper {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Interface;
	using Ezbob.Integration.LogicalGlue.Keeper.DBTable;
	using Ezbob.Logger;

	internal class InferenceLoader {
		public InferenceLoader(AConnection db, ASafeLog log, int customerID, DateTime time) {
			this.db = db;
			this.log = log;
			this.customerID = customerID;
			this.time = time;
			this.timeStr = this.time.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture);

			this.models = new SortedDictionary<long, ModelOutput>();
			this.executed = false;

			Result = new Inference();
		} // constructor

		public Inference Result { get; private set; }

		public InferenceLoader Execute() {
			if (this.executed) {
				this.log.Alert(
					"Inference loader({0}, '{1}') has already been executed.",
					this.customerID,
					this.timeStr
				);

				return this;
			} // if

			this.executed = true;

			this.log.Debug("Executing inference loader({0}, '{1}')...", this.customerID, this.timeStr);

			this.db.ForEachRowSafe(
				ProcessInferenceRow,
				"LogicalGlueLoadInference",
				new QueryParameter("@CustomerID", this.customerID),
				new QueryParameter("@Now", this.time)
			);

			this.log.Debug(
				"Executing inference loader({0}, '{1}') complete.",
				this.customerID,
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
					this.customerID,
					this.timeStr
				);

				this.log.Alert("{0}", err);
				
				throw new Exception(err);
			} // if

			switch (rowType) {
			case RowTypes.Response:
				Response response = sr.Fill<Response>();

				if (!Enum.IsDefined(typeof(RequestType), response.RequestTypeID)) {
					err = string.Format(
						"inference loader({1}, '{2}'): unsupported request type '{0}'.",
						response.RequestTypeID,
						this.customerID,
						this.timeStr
					);

					this.log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				RequestType requestType = (RequestType)response.RequestTypeID;

				var model = new ModelOutput();

				this.models[response.ResponseID] = model;
				Result[requestType] = model;

				model.Status = response.Status;

				model.Grade.Score = response.Score;
				model.Grade.EncodedResult = response.InferenceResultEncoded;
				model.Grade.DecodedResult = response.InferenceResultDecoded;

				model.Error.ErrorCode = response.ErrorCode;
				model.Error.Exception = response.Exception;
				model.Error.Uuid = response.Uuid;

				this.log.Debug(
					"Inference loader({0}, '{1}'): loaded response (id: {2}, type: {3}).",
					this.customerID,
					this.timeStr,
					response.ResponseID,
					requestType
				);

				break;

			case RowTypes.MapOutputRatio:
				MapOutputRatio ratio = sr.Fill<MapOutputRatio>();

				if (this.models.ContainsKey(ratio.ResponseID)) {
					this.models[ratio.ResponseID].Grade.MapOutputRatios[ratio.OutputClass] = ratio.Score;

					this.log.Debug(
						"Inference loader({0}, '{1}'): loaded map output ratio ({2}: {3}).",
						this.customerID,
						this.timeStr,
						ratio.OutputClass,
						ratio.Score
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): map output ratio '{2}' should belong to unknown response '{3}'.",
						this.customerID,
						this.timeStr,
						ratio.OutputRatioID,
						ratio.ResponseID
					);

					this.log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			case RowTypes.Warning:
				DBTable.Warning warning = sr.Fill<DBTable.Warning>();

				if (this.models.ContainsKey(warning.ResponseID)) {
					this.models[warning.ResponseID].Grade.Warnings.Add(new Interface.Warning {
						FeatureName = warning.FeatureName,
						MaxValue = warning.MaxValue,
						MinValue = warning.MinValue,
						Value = warning.Value,
					});

					this.log.Debug(
						"Inference loader({0}, '{1}'): loaded warning ({2}: {3}).",
						this.customerID,
						this.timeStr,
						warning.FeatureName,
						warning.Value
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): warning '{2}' should belong to unknown response '{3}'.",
						this.customerID,
						this.timeStr,
						warning.WarningID,
						warning.ResponseID
					);

					this.log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			case RowTypes.EncodingFailure:
				DBTable.EncodingFailure encodingFailure = sr.Fill<DBTable.EncodingFailure>();

				if (this.models.ContainsKey(encodingFailure.ResponseID)) {
					this.models[encodingFailure.ResponseID].Error.EncodingFailures.Add(new Interface.EncodingFailure {
						ColumnName = encodingFailure.ColumnName,
						Message = encodingFailure.Message,
						Reason = encodingFailure.Reason,
						RowIndex = encodingFailure.RowIndex,
						UnencodedValue = encodingFailure.UnencodedValue,
					});

					this.log.Debug(
						"Inference loader({0}, '{1}'): loaded encoding failure ({2}: {3}).",
						this.customerID,
						this.timeStr,
						encodingFailure.ColumnName,
						encodingFailure.Reason
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): encoding failure '{2}' should belong to unknown response '{3}'.",
						this.customerID,
						this.timeStr,
						encodingFailure.FailureID,
						encodingFailure.ResponseID
					);

					this.log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			case RowTypes.MissingColumn:
				MissingColumn column = sr.Fill<MissingColumn>();

				if (this.models.ContainsKey(column.ResponseID)) {
					this.models[column.ResponseID].Error.MissingColumns.Add(column.ColumnName);

					this.log.Debug(
						"Inference loader({0}, '{1}'): loaded missing column ({2}).",
						this.customerID,
						this.timeStr,
						column.ColumnName
					);
				} else {
					err = string.Format(
						"Inference loader({0}, '{1}'): missing column '{2}' should belong to unknown response '{3}'.",
						this.customerID,
						this.timeStr,
						column.MissingColumnID,
						column.ResponseID
					);

					this.log.Alert("{0}", err);

					throw new ArgumentOutOfRangeException(err, (Exception)null);
				} // if

				break;

			default:
				err = string.Format(
					"Inference loader({1}, '{2}'): unsupported row type '{0}'.",
					rowTypeName,
					this.customerID,
					this.timeStr
				);

				this.log.Alert("{0}", err);

				throw new ArgumentOutOfRangeException(err, (Exception)null);
			} // switch
		} // ProcessInferenceRow

		private enum RowTypes {
			Response,
			MapOutputRatio,
			Warning,
			EncodingFailure,
			MissingColumn,
		} // enum RowTypes

		private bool executed;

		private readonly SortedDictionary<long, ModelOutput> models;

		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly int customerID;
		private readonly DateTime time;
		private readonly string timeStr;
	} // class InferenceLoader
} // namespace
