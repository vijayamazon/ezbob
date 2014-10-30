namespace EzBob.Backend.Strategies.MainStrategy
{
	using System.Collections.Generic;
	using Experian;
	using EzBob.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Misc;

	public class AdditionalStrategiesCaller
	{
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
		                                  AConnection db, ASafeLog log)
		{
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

		public List<string> Call()
		{
			var consumerCaisDetailWorstStatuses = new List<string>();
			var strat = new ExperianConsumerCheck(customerId, null, false, db, log);
			strat.Execute();

			// TODO: load from DB within dataGatherer.Gather()
			if (strat.Result != null && strat.Result.Cais != null)
			{
				foreach (var caisDetails in strat.Result.Cais)
				{
					consumerCaisDetailWorstStatuses.Add(caisDetails.WorstStatus);
				}
			}

			if (typeOfBusiness != "Entrepreneur")
			{
				db.ForEachRowSafe((sr, bRowsetStart) =>
				{
					int appDirId = sr["DirId"];
					string appDirName = sr["DirName"];
					string appDirSurname = sr["DirSurname"];

					if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
						return ActionResult.Continue;

					var directorExperianConsumerCheck = new ExperianConsumerCheck(customerId, appDirId, false, db, log);
					directorExperianConsumerCheck.Execute();

					// TODO: load from DB within dataGatherer.Gather()
					if (directorExperianConsumerCheck.Result != null && directorExperianConsumerCheck.Result.Cais != null)
					{
						foreach (var caisDetails in directorExperianConsumerCheck.Result.Cais)
						{
							consumerCaisDetailWorstStatuses.Add(caisDetails.WorstStatus);
						}
					}

					return ActionResult.Continue;
				},
									"GetCustomerDirectorsForConsumerCheck",
									CommandSpecies.StoredProcedure,
									new QueryParameter("CustomerId", customerId)
					);
			}

			if (wasMainStrategyExecutedBefore)
			{
				log.Info("Performing experian company check");
				var experianCompanyChecker = new ExperianCompanyCheck(customerId, false, db, log);
				experianCompanyChecker.Execute();
			}

			GetAml();
			GetBwa();

			GetZooplaData();

			return consumerCaisDetailWorstStatuses;
		}

		private void GetBwa()
		{
			if (ShouldRunBwa())
			{
				log.Info("Getting BWA for customer: {0}", customerId);
				var bwaChecker = new BwaChecker(customerId, db, log);
				bwaChecker.Execute();
			}
		}

		private bool ShouldRunBwa()
		{
			return appBankAccountType == "Personal" && bwaBusinessCheck == "1" && appSortCode != null && appAccountNumber != null;
		}

		private void GetAml()
		{
			if (wasMainStrategyExecutedBefore)
			{
				log.Info("Getting AML for customer: {0}", customerId);
				var amlChecker = new AmlChecker(customerId, db, log);
				amlChecker.Execute();
			}
		}

		private void GetZooplaData()
		{
			log.Info("Getting zoopla data for customer:{0}", customerId);
			strategyHelper.GetZooplaData(customerId);
		}
	}
}
