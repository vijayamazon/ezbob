namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using ConfigManager;
	using MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Staller {
		public Staller(int customerId, StrategiesMailer mailer) {
			this.marketplaceUpdateStatus = null;

			this.customerID = customerId;
			this.mailer = mailer;

			this.totalTimeToWaitForMarketplacesUpdate = CurrentValues.Instance.TotalTimeToWaitForMarketplacesUpdate;
			this.intervalWaitForMarketplacesUpdate = CurrentValues.Instance.IntervalWaitForMarketplacesUpdate;

			this.totalTimeToWaitForExperianCompanyCheck = CurrentValues.Instance.TotalTimeToWaitForExperianCompanyCheck;
			this.intervalWaitForExperianCompanyCheck = CurrentValues.Instance.IntervalWaitForExperianCompanyCheck;

			this.totalTimeToWaitForExperianConsumerCheck = CurrentValues.Instance.TotalTimeToWaitForExperianConsumerCheck;
			this.intervalWaitForExperianConsumerCheck = CurrentValues.Instance.IntervalWaitForExperianConsumerCheck;

			this.totalTimeToWaitForAmlCheck = CurrentValues.Instance.TotalTimeToWaitForAmlCheck;
			this.intervalWaitForAmlCheck = CurrentValues.Instance.IntervalWaitForAmlCheck;

			SafeReader sr = DB.GetFirst(
				"GetMainStrategyStallerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);

			if (!sr.IsEmpty) {
				string experianRefNum = sr["ExperianRefNum"];

				this.hasCompany = !string.IsNullOrEmpty(experianRefNum);
				this.wasMainStrategyExecutedBefore = sr["MainStrategyExecutedBefore"];
				this.typeOfBusiness = sr["TypeOfBusiness"];
				this.customerEmail = sr["CustomerEmail"];
			} // if
		} // constructor

		public void Stall() {
			if (!this.wasMainStrategyExecutedBefore) {
				WaitForExperianConsumerCheck();

				if (this.hasCompany) {
					WaitForUpdateToFinish(
						GetIsExperianCompanyUpdated,
						this.totalTimeToWaitForExperianCompanyCheck,
						this.intervalWaitForExperianCompanyCheck
					);
				} // if

				WaitForUpdateToFinish(GetIsAmlUpdated, this.totalTimeToWaitForAmlCheck, this.intervalWaitForAmlCheck);
			} // if

			bool endedWithError = !WaitForUpdateToFinish(
				GetIsMarketPlacesUpdated,
				this.totalTimeToWaitForMarketplacesUpdate,
				this.intervalWaitForMarketplacesUpdate
			);

			if (endedWithError) { // TODO: if nobody monitors these mails so there is no need for them
				this.mailer.Send(
					"Mandrill - No Information about shops", new Dictionary<string, string> {
						{"UserEmail", this.customerEmail},
						{"CustomerID", this.customerID.ToString(Library.Instance.Culture)},
						{"ApplicationID", this.customerEmail}
					}
				);
			} // if
		} // Stall

		internal Staller SetMarketplaceUpdateStatus(MarketplaceUpdateStatus mpus) {
			this.marketplaceUpdateStatus = mpus;
			return this;
		} // SetMarketplaceUpdateStatus

		private bool GetIsMarketPlacesUpdated() {
			if (this.marketplaceUpdateStatus != null) {
				if (this.marketplaceUpdateStatus.HasPending) {
					Log.Debug("Customer {0}, mp are NOT up to date according to in-memory status.", this.customerID);
					return false;
				} // if
			} // if

			bool result = true;

			DB.ForEachRowSafe(
				sr => {
					int mpID = sr["Id"];
					string lastStatus = sr["CurrentStatus"];

					Log.Debug("Customer {0}, mp {1} status: {2}.", this.customerID, mpID, lastStatus);

					if (lastStatus == "In progress")
						result = false;
				},
				"GetAllLastMarketplaceStatuses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);

			Log.Debug("Customer {0}, mp are up to date: {1}.", this.customerID, result ? "yes" : "no");

			return result;
		} // GetIsMarketPlacesUpdated

		private void WaitForExperianConsumerCheck() {
			var directors = new List<int?> { null }; // The "null" is to wait for customer data.

			if (this.typeOfBusiness != "Entrepreneur") {
				DB.ForEachRowSafe(
					sr => {
						if (!string.IsNullOrEmpty(sr["DirName"]) && !string.IsNullOrEmpty(sr["DirSurname"]))
							directors.Add(sr["DirId"]);
					},
					"GetCustomerDirectorsForConsumerCheck",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.customerID)
				);
			} // if

			foreach (int? dirId in directors) {
				int? directorId = dirId; // This assignment to avoid "Access to foreach variable in closure".

				WaitForUpdateToFinish(
					() => GetIsExperianConsumerUpdated(directorId), 
					this.totalTimeToWaitForExperianConsumerCheck,
					this.intervalWaitForExperianConsumerCheck
				);
			} // for each
		} // WaitForExperianConsumerCheck

		private bool GetIsExperianCompanyUpdated() {
			return DB.ExecuteScalar<bool>(
				"GetIsCompanyDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID),
				new QueryParameter("Today", DateTime.Today)
			);
		} // GetIsExperianCompanyUpdated

		private bool GetIsExperianConsumerUpdated(int? directorId) {
			return DB.ExecuteScalar<bool>(
				"GetIsConsumerDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID),
				new QueryParameter("DirectorId", directorId),
				new QueryParameter("Today", DateTime.Today)
			);
		} // GetIsExperianConsumerUpdated

		private bool GetIsAmlUpdated() {
			return DB.ExecuteScalar<bool>(
				"GetIsAmlUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerID)
			);
		} // GetIsAmlUpdated

		private static bool WaitForUpdateToFinish(Func<bool> function, int totalSecondsToWait, int intervalBetweenCheck) {
			DateTime startWaitingTime = DateTime.UtcNow;

			while (true) {
				if (function())
					return true;

				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalSecondsToWait)
					return false;

				Thread.Sleep(intervalBetweenCheck);
			} // while
		} // WaitForUpdateToFinish

		private static ASafeLog Log {
			get { return Library.Instance.Log; }
		} // Log

		private static AConnection DB {
			get { return Library.Instance.DB; }
		} // DB

		private readonly StrategiesMailer mailer;
		private readonly int customerID;

		private readonly int totalTimeToWaitForMarketplacesUpdate;
		private readonly int intervalWaitForMarketplacesUpdate;
		private readonly int totalTimeToWaitForExperianCompanyCheck;
		private readonly int intervalWaitForExperianCompanyCheck;
		private readonly int totalTimeToWaitForExperianConsumerCheck;
		private readonly int intervalWaitForExperianConsumerCheck;
		private readonly int totalTimeToWaitForAmlCheck;
		private readonly int intervalWaitForAmlCheck;

		private readonly bool hasCompany;
		private readonly bool wasMainStrategyExecutedBefore;
		private readonly string typeOfBusiness;
		private readonly string customerEmail;

		private MarketplaceUpdateStatus marketplaceUpdateStatus;
	} // class Staller
} // namespace
