namespace Ezbob.Backend.Strategies.MainStrategy {
	using ConfigManager;
	using MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using Ezbob.Database;

	public class Staller {
		public Staller(int customerId, StrategiesMailer mailer) {
			this.customerId = customerId;
			this.mailer = mailer;

			this.db = Library.Instance.DB;

			this.totalTimeToWaitForMarketplacesUpdate = CurrentValues.Instance.TotalTimeToWaitForMarketplacesUpdate;
			this.intervalWaitForMarketplacesUpdate = CurrentValues.Instance.IntervalWaitForMarketplacesUpdate;

			this.totalTimeToWaitForExperianCompanyCheck = CurrentValues.Instance.TotalTimeToWaitForExperianCompanyCheck;
			this.intervalWaitForExperianCompanyCheck = CurrentValues.Instance.IntervalWaitForExperianCompanyCheck;

			this.totalTimeToWaitForExperianConsumerCheck = CurrentValues.Instance.TotalTimeToWaitForExperianConsumerCheck;
			this.intervalWaitForExperianConsumerCheck = CurrentValues.Instance.IntervalWaitForExperianConsumerCheck;

			this.totalTimeToWaitForAmlCheck = CurrentValues.Instance.TotalTimeToWaitForAmlCheck;
			this.intervalWaitForAmlCheck = CurrentValues.Instance.IntervalWaitForAmlCheck;

			SafeReader sr = this.db.GetFirst(
				"GetMainStrategyStallerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
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
						{"CustomerID", this.customerId.ToString(Library.Instance.Culture)},
						{"ApplicationID", this.customerEmail}
					}
				);
			} // if
		} // Stall

		private bool GetIsMarketPlacesUpdated() {
			bool result = true;

			this.db.ForEachRowSafe(
				(sr, rowsetStart) => {
					string lastStatus = sr["CurrentStatus"];

					if (lastStatus == "In progress") {
						result = false;
						return ActionResult.SkipAll;
					} // if

					return ActionResult.Continue;
				},
				"GetAllLastMarketplaceStatuses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			return result;
		} // GetIsMarketPlacesUpdated

		private void WaitForExperianConsumerCheck() {
			var directors = new List<int?> { null }; // The "null" is to wait for customer data.

			if (typeOfBusiness != "Entrepreneur") {
				this.db.ForEachRowSafe(
					sr => {
						if (!string.IsNullOrEmpty(sr["DirName"]) && !string.IsNullOrEmpty(sr["DirSurname"]))
							directors.Add(sr["DirId"]);
					},
					"GetCustomerDirectorsForConsumerCheck",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.customerId)
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
			return db.ExecuteScalar<bool>(
				"GetIsCompanyDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("Today", DateTime.Today)
			);
		} // GetIsExperianCompanyUpdated

		private bool GetIsExperianConsumerUpdated(int? directorId) {
			return db.ExecuteScalar<bool>(
				"GetIsConsumerDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId),
				new QueryParameter("DirectorId", directorId),
				new QueryParameter("Today", DateTime.Today)
			);
		} // GetIsExperianConsumerUpdated

		private bool GetIsAmlUpdated() {
			return db.ExecuteScalar<bool>(
				"GetIsAmlUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
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

		private readonly StrategiesMailer mailer;
		private readonly int customerId;
		private readonly AConnection db;

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
	} // class Staller
} // namespace
