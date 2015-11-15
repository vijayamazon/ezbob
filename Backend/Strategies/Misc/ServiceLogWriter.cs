namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using StructureMap;
	using System;
	using Ezbob.Backend.Extensions;
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

				switch (Package.In.ServiceType) {
				case ExperianServiceType.LimitedData:
					DoLimited();
					break;

				case ExperianServiceType.Consumer:
					DoConsumer();
					break;

				case ExperianServiceType.CreditSafeNonLtd:
					DoCreditSafeNonLtd();
					break;

				case ExperianServiceType.CallCredit:
					DoCallCredit();
					break;
				} // switch

				SaveExperianHistory();

				// This operation must be done after all the saving SPs are completed
				// so that nhibernate picks up all the data from the DB.
				Package.Out.ServiceLog = this.repoLog.GetById(this.serviceLogID);

				if (IsSavedEntryValid())
				LogNewServiceLogEntry();
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

		private void DoLimited() {
			var parseExperianLtd = new ParseExperianLtd(this.serviceLogID);

			parseExperianLtd.Execute();

			Package.Out.ExperianLtd = parseExperianLtd.Result;
		} // DoLimited

		private void DoConsumer() {
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
		} // DoConsumer

		private void DoCreditSafeNonLtd() {
			var parser = new ParseCreditSafeNonLtd(this.serviceLogID);
			parser.Execute();
		} // DoCreditSafeNonLtd

		private void DoCallCredit() {
			var parser = new ParseCallCredit(this.serviceLogID);
			parser.Execute();
		} // DoCallCredit

		private void SaveExperianHistory() {
			var spSaveExperianHistoryEntry = new SpSaveExperianHistory(this);

			if (spSaveExperianHistoryEntry.HasValidParameters())
				spSaveExperianHistoryEntry.ExecuteNonQuery();
		} // SaveExperianHistory

		private bool IsSavedEntryValid() {
			if ((Package.Out.ServiceLog != null) && (Package.Out.ServiceLog.Id == this.serviceLogID))
				return true;

			Log.Alert("Failed to retrieve from DB MP_ServiceLog entry with id {0}.", this.serviceLogID);
			return false;
		} // IsSavedEntryValid

		private void SaveServiceLogEntry() {
			this.spSaveServiceLog = new SpSaveServiceLogEntry(this);

			Log.Debug("Input data was: {0}", this.spSaveServiceLog.RequestData);
			Log.Debug("Output data was: {0}", this.spSaveServiceLog.ResponseData);

			this.spSaveServiceLog.Execute();

			if (this.serviceLogID <= 0)
				throw new Exception("Failed to save/retrieve ID of new MP_ServiceLog entry.");
		} // SaveServiceLogEntry

		private void LogNewServiceLogEntry() {
			Log.Debug(
				"New MP_ServiceLog entry:" +
				"\n\tid            = {0}" +
				"\n\ttype          = {1}" +
				"\n\tinsert time   = {2}" +
				"\n\tcustomer id   = {3}" +
				"\n\tfirst name    = {4}" +
				"\n\tlast name     = {5}" +
				"\n\tpostcode      = {6}" +
				"\n\tcompany ref # = {7}" +
				"\n\trequest       = {8}" +
				"\n\tresponse      = {9}" +
				"\n",
				Package.Out.ServiceLog.Id,
				Package.Out.ServiceLog.ServiceType,
				Package.Out.ServiceLog.InsertDate.MomentStr(),
				Package.Out.ServiceLog.Customer.Id,
				Package.Out.ServiceLog.Firstname,
				Package.Out.ServiceLog.Surname,
				Package.Out.ServiceLog.Postcode,
				Package.Out.ServiceLog.CompanyRefNum,
				FormatRequestResponse(Package.Out.ServiceLog.RequestData),
				FormatRequestResponse(Package.Out.ServiceLog.ResponseData)
			);
		} // LogNewServiceLogEntry

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

		private static string FormatRequestResponse(string input) {
			const int substLen = 50;

			string res = (input ?? string.Empty);

			string dots = (res.Length > substLen) ? "..." : string.Empty;

			return "'" +
				res.TrimStart().Substring(0, substLen).Replace("\r", string.Empty).Replace("\n", string.Empty) +
				"'" +
				dots;
		} // FormatRequestResponse
	} // class ServiceLogWriter
} // namespace
