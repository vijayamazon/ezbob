namespace Ezbob.Backend.Strategies.MainStrategy {
	using Experian;
	using EzBob.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Misc;

	public class AdditionalStrategiesCaller {
		private readonly int customerId;
		private readonly AConnection db;
		private readonly ASafeLog log;
		private readonly bool wasMainStrategyExecutedBefore;
		private readonly string typeOfBusiness;
		private readonly string bwaBusinessCheck;
		private readonly string appBankAccountType;
		private readonly string appAccountNumber;
		private readonly string appSortCode;
		private readonly StrategyHelper strategyHelper = new StrategyHelper();

		public AdditionalStrategiesCaller(int customerId,
										  bool wasMainStrategyExecutedBefore, string typeOfBusiness, string bwaBusinessCheck,
										  string appBankAccountType, string appAccountNumber, string appSortCode,
										  AConnection db, ASafeLog log) {
			this.customerId = customerId;
			this.db = db;
			this.log = log;
			this.wasMainStrategyExecutedBefore = wasMainStrategyExecutedBefore;
			this.typeOfBusiness = typeOfBusiness;
			this.bwaBusinessCheck = bwaBusinessCheck;
			this.appBankAccountType = appBankAccountType;
			this.appAccountNumber = appAccountNumber;
			this.appSortCode = appSortCode;
		}

		public void Call() {
			var strat = new ExperianConsumerCheck(customerId, null, false);
			strat.Execute();

			if (typeOfBusiness != "Entrepreneur") {
				db.ForEachRowSafe((sr, bRowsetStart) => {
					int appDirId = sr["DirId"];
					string appDirName = sr["DirName"];
					string appDirSurname = sr["DirSurname"];

					if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
						return ActionResult.Continue;

					var directorExperianConsumerCheck = new ExperianConsumerCheck(customerId, appDirId, false);
					directorExperianConsumerCheck.Execute();

					return ActionResult.Continue;
				},
								  "GetCustomerDirectorsForConsumerCheck",
								  CommandSpecies.StoredProcedure,
								  new QueryParameter("CustomerId", customerId)
					);
			}

			if (wasMainStrategyExecutedBefore) {
				log.Info("Performing experian company check");
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, false);
				experianCompanyChecker.Execute();
			}

			GetAml();
			GetBwa();

			GetZooplaData();
		}

		private void GetBwa() {
			if (ShouldRunBwa()) {
				log.Info("Getting BWA for customer: {0}", customerId);
				var bwaChecker = new BwaChecker(customerId);
				bwaChecker.Execute();
			}
		}

		private bool ShouldRunBwa() {
			return appBankAccountType == "Personal" && bwaBusinessCheck == "1" && appSortCode != null && appAccountNumber != null;
		}

		private void GetAml() {
			if (wasMainStrategyExecutedBefore) {
				log.Info("Getting AML for customer: {0}", customerId);
				var amlChecker = new AmlChecker(customerId);
				amlChecker.Execute();
			}
		}

		private void GetZooplaData() {
			log.Info("Getting zoopla data for customer:{0}", customerId);
			strategyHelper.GetZooplaData(customerId);
		}
	}
}
