namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Serialization;
	using ExperianLib;
	using Ezbob.Backend.ModelsWithDB;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Backend.ModelsWithDB.Experian;

	public class ParseExperianConsumerData : AStrategy {
		public ParseExperianConsumerData(long nServiceLogID) {
			Result = null;
			this.serviceLogID = nServiceLogID;
		} // constructor

		public override string Name {
			get { return "ParseExperianConsumerData"; }
		} // Name

		public override void Execute() {
			Log.Info("Parsing Experian Consumer for service log entry {0}...", this.serviceLogID);
			try {
				var oTbl = Save(Parse(Load()));

				if (oTbl != null)
					Result = oTbl;
			} catch (Exception) {
				Log.Error("Parsing Experian Consumer for service log entry {0} failed.", this.serviceLogID);
			} // try

			Log.Info("Parsing Experian Consumer for service log entry {0} complete.", this.serviceLogID);
		} // Execute

		public ExperianConsumerData Result { get; private set; }

		private Tuple<OutputRoot, ServiceLog> Load() {
			SafeReader sr = DB.GetFirst(
				"LoadServiceLogEntry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@EntryID", this.serviceLogID)
			);

			var serviceLog = sr.Fill<ServiceLog>();

			if (serviceLog.Id != this.serviceLogID) {
				Log.Info("Parsing Experian Consumer for service log entry {0} failed: entry not found.", this.serviceLogID);
				return null;
			} // if

			if (string.IsNullOrWhiteSpace(serviceLog.ResponseData)) {
				Log.Alert(
					"Parsing Experian Consumer for service log entry {0} failed: response data is empty.",
					this.serviceLogID
				);

				return null;
			} // if

			try {
				var outputRootSerializer = new XmlSerializer(typeof(OutputRoot));

				var outputRoot = (OutputRoot)outputRootSerializer.Deserialize(new StringReader(serviceLog.ResponseData));

				if (outputRoot == null) {
					Log.Alert(
						"Parsing Experian Consumer for service log entry {0} failed root element is null.",
						this.serviceLogID
					);

					return null;
				} // if

				return new Tuple<OutputRoot, ServiceLog>(outputRoot, serviceLog);
			} catch (Exception e) {
				Log.Alert(e, "Parsing Experian Consumer for service log entry {0} failed.", this.serviceLogID);
				return null;
			} // try
		} // Load

		private ExperianConsumerData Parse(Tuple<OutputRoot, ServiceLog> oDoc) {
			if (oDoc == null)
				return null;

			if ((oDoc.Item1 == null))
				return null;

			Log.Info("Parsing Experian company data...");

			var builder = new ConsumerExperianModelBuilder();

			return builder.Build(
				oDoc.Item1,
				oDoc.Item2.CustomerId,
				oDoc.Item2.DirectorId,
				oDoc.Item2.InsertDate,
				oDoc.Item2.Id
			);
		} // Parse

		private ExperianConsumerData Save(ExperianConsumerData data) {
			if (data == null) {
				Log.Info(
					"Not saving Experian consumer data into DB for service log id {0}: data is null.",
					this.serviceLogID
				);

				return null;
			} // if

			Log.Info("Saving Experian consumer data into DB for service log id {0}...", this.serviceLogID);

			var con = DB.GetPersistent();

			con.BeginTransaction();

			try {
				var id = DB.ExecuteScalar<long>(
					con,
					"SaveExperianConsumerData",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<ExperianConsumerData>("Tbl", new List<ExperianConsumerData> { data })
				);

				foreach (var cais in data.Cais) {
					cais.ExperianConsumerDataId = id;

					var caisId = DB.ExecuteScalar<long>(
						con,
						"SaveExperianConsumerDataCais",
						CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<ExperianConsumerDataCais>("Tbl", new List<ExperianConsumerDataCais> { cais })
					);

					if (cais.CardHistories.Any()) {
						foreach (var card in cais.CardHistories)
							card.ExperianConsumerDataCaisId = caisId;

						DB.ExecuteNonQuery(
							con,
							"SaveExperianConsumerDataCaisCardHistory",
							CommandSpecies.StoredProcedure,
							DB.CreateTableParameter<ExperianConsumerDataCaisCardHistory>("Tbl", cais.CardHistories)
						);
					} // if

					if (cais.AccountBalances.Any()) {
						foreach (var account in cais.AccountBalances)
							account.ExperianConsumerDataCaisId = caisId;

						DB.ExecuteNonQuery(
							con,
							"SaveExperianConsumerDataCaisBalance",
							CommandSpecies.StoredProcedure,
							DB.CreateTableParameter<ExperianConsumerDataCaisBalance>("Tbl", cais.AccountBalances)
						);
					} // if
				} // for each

				foreach (var applicant in data.Applicants)
					applicant.ExperianConsumerDataId = id;

				DB.ExecuteNonQuery(
					con,
					"SaveExperianConsumerDataApplicant",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<ExperianConsumerDataApplicant>("Tbl", data.Applicants)
				);

				foreach (var location in data.Locations)
					location.ExperianConsumerDataId = id;

				DB.ExecuteNonQuery(
					con,
					"SaveExperianConsumerDataLocation",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<ExperianConsumerDataLocation>("Tbl", data.Locations)
				);

				foreach (var residency in data.Residencies)
					residency.ExperianConsumerDataId = id;

				DB.ExecuteNonQuery(
					con,
					"SaveExperianConsumerDataResidency",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<ExperianConsumerDataResidency>("Tbl", data.Residencies)
				);

				foreach (var noc in data.Nocs)
					noc.ExperianConsumerDataId = id;

				DB.ExecuteNonQuery(
					con,
					"SaveExperianConsumerDataNoc",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<ExperianConsumerDataNoc>("Tbl", data.Nocs)
				);
			} catch (Exception ex) {
				Log.Warn(ex, "Failed to save experian consumer");
				con.Rollback();
				return null;
			} // try

			con.Commit();

			Log.Info("Saving Experian consumer data into DB for service log ID {0} complete.", this.serviceLogID);

			return data;
		} // Save

		private readonly long serviceLogID;
	} // class ParseExperianConsumerData
} // namespace
