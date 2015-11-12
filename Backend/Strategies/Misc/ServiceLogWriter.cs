namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using StructureMap;
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.CallCreditStrategy;
	using Ezbob.Backend.Strategies.CreditSafe;
	using JetBrains.Annotations;

	public class ServiceLogWriter : AStrategy {
		public ServiceLogWriter(WriteToLogPackage oPackage) {
			this.serviceLogID = 0;
			this.customerID = null;
			this.directorID = null;

			this.repoLog = ObjectFactory.GetInstance<ServiceLogRepository>();

			Package = oPackage;
		} // constructor

		public override string Name {
			get { return "ServiceLogWriter"; }
		} // Name

		public override void Execute() {
			if (Package == null || Package.In == null)
				throw new ArgumentNullException("Cannot save to MP_ServiceLog: no package specified.", (Exception)null);

			try {
				SaveServiceLogEntry();

				if (this.serviceLogID <= 0)
					throw new Exception("Failed to retrieve ID of new MP_ServiceLog entry.");

				switch (Package.In.ServiceType) {
				case ExperianServiceType.LimitedData:
					var parseExperianLtd = new ParseExperianLtd(this.serviceLogID);
					parseExperianLtd.Execute();
					Package.Out.ExperianLtd = parseExperianLtd.Result;
					break;

				case ExperianServiceType.Consumer:
					var stra = new ParseExperianConsumerData(this.serviceLogID);
					stra.Execute();
					Package.Out.ExperianConsumer = stra.Result;

					if (Package.Out.ExperianConsumer != null) {
						DB.ExecuteNonQuery(
							"UpdateConsumerScoreByServiceLogID",
							CommandSpecies.StoredProcedure,
							new QueryParameter("@ServiceLogID", this.serviceLogID),
							new QueryParameter("@BureauScore", Package.Out.ExperianConsumer.BureauScore)
						);
					} // if

					break;

				case ExperianServiceType.CreditSafeNonLtd:
					try {
						var parseCreditSafeNonLtdData = new ParseCreditSafeNonLtd(this.serviceLogID);
						parseCreditSafeNonLtdData.Execute();
					} catch (Exception e) {
						Log.Error(e, "CreditSafeLtd/NonLtd failed for unexpected reason.");
						throw;
					} // if
					break;

				case ExperianServiceType.CallCredit:
					var parseCallCredit = new ParseCallCredit(this.serviceLogID);
					parseCallCredit.Execute();
					break;
				} // switch

				var spSaveExperianHistoryEntry = new SpSaveExperianHistory(this);

				if (spSaveExperianHistoryEntry.HasValidParameters())
					spSaveExperianHistoryEntry.ExecuteNonQuery();

				Package.Out.ServiceLog = this.repoLog.GetById(this.serviceLogID);
			} catch (Exception e) {
				Log.Error(
					e,
					"Failed to save a '{0}' entry for customer id {1} of type {2} into MP_ServiceLog.",
					Package.In.ServiceType,
					Package.In.CustomerID,
					Package.In.ServiceType.DescriptionAttr()
				);
			} // try
		} // Execute

		public WriteToLogPackage Package { get; set; }

		private void SaveServiceLogEntry() {
			this.spSaveServiceLog = new SpSaveServiceLogEntry(this);

			Log.Debug("Input data was: {0}", this.spSaveServiceLog.RequestData);
			Log.Debug("Output data was: {0}", this.spSaveServiceLog.ResponseData);

			this.spSaveServiceLog.Execute();
		} // SaveServiceLogEntry

		private class SpSaveServiceLogEntry : AStoredProc {
			public SpSaveServiceLogEntry(ServiceLogWriter writer) : base(writer.DB, writer.Log) {
				this.writer = writer;

				var input = this.writer.Package.In;

				RequestData = input.Request;
				ResponseData = input.Response;
				ServiceType = input.ServiceType.DescriptionAttr();
				Firstname = input.Firstname;
				Surname = input.Surname;
				DateOfBirth = input.DateOfBirth;
				Postcode = input.PostCode;
				CompanyRefNum = input.CompanyRefNum;
				CustomerID = input.CustomerID > 0 ? input.CustomerID : (int?)null;
				DirectorID = input.DirectorID > 0 ? input.DirectorID : null;
			} // constructor

			public override bool HasValidParameters() {
				return true;
			} // HasValidParameters

			[UsedImplicitly]
			public int? CustomerID { get; set; }
			[UsedImplicitly]
			public int? DirectorID { get; set; }

			[UsedImplicitly]
			public DateTime InsertDate {
				get { return DateTime.UtcNow; }
				// ReSharper disable once ValueParameterNotUsed
				set { }
			} // InsertDate

			[UsedImplicitly]
			public string ServiceType { get; set; }
			[UsedImplicitly]
			public string RequestData { get; set; }
			[UsedImplicitly]
			public string ResponseData { get; set; }
			[UsedImplicitly]
			public string CompanyRefNum { get; set; }
			[UsedImplicitly]
			public string Firstname { get; set; }
			[UsedImplicitly]
			public string Surname { get; set; }
			[UsedImplicitly]
			public DateTime? DateOfBirth { get; set; }
			[UsedImplicitly]
			public string Postcode { get; set; }

			public void Execute() {
				SafeReader sr = GetFirst();

				if (sr.IsEmpty) {
					this.writer.serviceLogID = 0;
					this.writer.customerID = null;
					this.writer.directorID = null;
				} else {
					this.writer.serviceLogID = sr["ServiceLogID"];
					this.writer.customerID = sr["CustomerID"];
					this.writer.directorID = sr["DirectorID"];
				} // if
			} // Execute

			private readonly ServiceLogWriter writer;
		} // class SpSaveServiceLogEntry

		private class SpSaveExperianHistory : AStoredProcedure {
			public SpSaveExperianHistory(ServiceLogWriter writer) : base(writer.DB, writer.Log) {
				this.doSave = false;

				switch (writer.Package.In.ServiceType) {
				case ExperianServiceType.Consumer:
					if (writer.Package.Out.ExperianConsumer != null) {
						Score = writer.Package.Out.ExperianConsumer.BureauScore;
						CII = writer.Package.Out.ExperianConsumer.CII;
						CaisBalance = ExperianLib.Utils.GetConsumerCaisBalance(writer.Package.Out.ExperianConsumer.Cais);
					} // if

					this.doSave = true;
					break;

				case ExperianServiceType.LimitedData:
					Score = (writer.Package.Out.ExperianLtd == null)
						? -1
						: (writer.Package.Out.ExperianLtd.CommercialDelphiScore ?? -1);
					CaisBalance = ExperianLib.Utils.GetLimitedCaisBalance(writer.Package.Out.ExperianLtd);
					this.doSave = true;
					break;

				case ExperianServiceType.NonLimitedData:
					var strategyInstance = new GetCompanyDataForCreditBureau(writer.Package.Out.ServiceLog.CompanyRefNum);
					strategyInstance.Execute();

					var notLimitedBusinessData = new CompanyDataForCreditBureau {
						LastUpdate = strategyInstance.LastUpdate,
						Score = strategyInstance.Score,
						Errors = strategyInstance.Errors,
					};

					Score = notLimitedBusinessData.Score;
					this.doSave = true;
					break;
				} // switch

				if (!this.doSave)
					return;

				CustomerId = writer.customerID;
				DirectorId = writer.directorID;
				CompanyRefNum = writer.spSaveServiceLog.CompanyRefNum;
				ServiceLogId = writer.serviceLogID;
				InsertDate = writer.spSaveServiceLog.InsertDate;
				Type = writer.spSaveServiceLog.ServiceType;
			} // constructor

			public override bool HasValidParameters() {
				return this.doSave;
			} // HasValidParameters

			[UsedImplicitly]
			public long ServiceLogId { get; set; }
			[UsedImplicitly]
			public DateTime InsertDate { get; set; }
			[UsedImplicitly]
			public string Type { get; set; }
			[UsedImplicitly]
			public int? Score { get; set; }
			[UsedImplicitly]
			public int? CustomerId { get; set; }
			[UsedImplicitly]
			public int? DirectorId { get; set; }
			[UsedImplicitly]
			public string CompanyRefNum { get; set; }
			[UsedImplicitly]
			public decimal? CaisBalance { get; set; }
			[UsedImplicitly]
			public int? CII { get; set; }

			private readonly bool doSave;
		} // class SpSaveExperianHistory

		private long serviceLogID;
		private int? customerID;
		private int? directorID;
		private SpSaveServiceLogEntry spSaveServiceLog;

		private readonly ServiceLogRepository repoLog;
	} // class ServiceLogWriter
} // namespace
