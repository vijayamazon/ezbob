namespace Ezbob.Backend.Strategies.MainStrategy
{
	using ConfigManager;
	using Ezbob.Backend.Models;
	using MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Staller
	{
		private readonly NewCreditLineOption newCreditLineOption;
		private readonly StrategiesMailer mailer;
		private readonly int customerId;
		private readonly AConnection db;
		private readonly ASafeLog log;

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

		public Staller(int customerId, NewCreditLineOption newCreditLineOption, StrategiesMailer mailer, AConnection db, ASafeLog log)
		{
			this.customerId = customerId;
			this.db = db;
			this.log = log;
			this.newCreditLineOption = newCreditLineOption;
			this.mailer = mailer;

			totalTimeToWaitForMarketplacesUpdate = CurrentValues.Instance.TotalTimeToWaitForMarketplacesUpdate;
			intervalWaitForMarketplacesUpdate = CurrentValues.Instance.IntervalWaitForMarketplacesUpdate;

			totalTimeToWaitForExperianCompanyCheck = CurrentValues.Instance.TotalTimeToWaitForExperianCompanyCheck;
			intervalWaitForExperianCompanyCheck = CurrentValues.Instance.IntervalWaitForExperianCompanyCheck;

			totalTimeToWaitForExperianConsumerCheck = CurrentValues.Instance.TotalTimeToWaitForExperianConsumerCheck;
			intervalWaitForExperianConsumerCheck = CurrentValues.Instance.IntervalWaitForExperianConsumerCheck;

			totalTimeToWaitForAmlCheck = CurrentValues.Instance.TotalTimeToWaitForAmlCheck;
			intervalWaitForAmlCheck = CurrentValues.Instance.IntervalWaitForAmlCheck;

			SafeReader sr = db.GetFirst(
				"GetMainStrategyStallerData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			if (!sr.IsEmpty)
			{
				string experianRefNum = sr["ExperianRefNum"];
				hasCompany = !string.IsNullOrEmpty(experianRefNum);
				wasMainStrategyExecutedBefore = sr["MainStrategyExecutedBefore"];
				typeOfBusiness = sr["TypeOfBusiness"];
				customerEmail = sr["CustomerEmail"];
			}
		}

		private bool WaitForUpdateToFinish(Func<bool> function, int totalSecondsToWait, int intervalBetweenCheck)
		{
			DateTime startWaitingTime = DateTime.UtcNow;

			while (true)
			{
				if (function())
				{
					return true;
				}

				if ((DateTime.UtcNow - startWaitingTime).TotalSeconds > totalSecondsToWait)
				{
					return false;
				}

				Thread.Sleep(intervalBetweenCheck);
			}
		}

		private bool MakeSureMpDataIsSufficient()
		{
			if (!WaitForMarketplacesToFinishUpdates())
			{
				log.Info("Waiting for marketplace data ended with error");

				return true;
			}

			return false;
		}

		private bool WaitForMarketplacesToFinishUpdates()
		{
			log.Info("Waiting for marketplace data");
			return WaitForUpdateToFinish(GetIsMarketPlacesUpdated, totalTimeToWaitForMarketplacesUpdate, intervalWaitForMarketplacesUpdate);
		}

		private bool GetIsMarketPlacesUpdated()
		{
			bool result = true;

			db.ForEachRowSafe(
				(sr, rowsetStart) =>
				{
					string lastStatus = sr["CurrentStatus"];

					if (lastStatus == "In progress")
					{
						result = false;
						return ActionResult.SkipAll;
					}

					return ActionResult.Continue;
				},
				"GetAllLastMarketplaceStatuses",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			return result;
		}

		private void WaitForExperianCompanyCheckIfNeeded()
		{
			if (!WaitForExperianCompanyCheckToFinishUpdates())
			{
				log.Info("No data exist from experian company check for customer:{0}.", customerId);
			}
		}

		private void WaitForExperianAmlCheckIfNeeded()
		{
			if (!WaitForAmlToFinishUpdates())
			{
				log.Info("No AML data exist for customer:{0}.", customerId);
			}
		}

		private void WaitForExperianConsumerCheckIfNeeded()
		{
			if (!WaitForExperianConsumerCheckToFinishUpdates())
			{
				log.Info("No data exist from experian consumer check for customer {0}.", customerId);
			}

			if (typeOfBusiness == "Entrepreneur")
			{
				return; // No consumer check for directors for Entrepreneurs
			}

			db.ForEachRowSafe((sr, bRowsetStart) =>
				{
					int directorId = sr["DirId"];
					string appDirName = sr["DirName"];
					string appDirSurname = sr["DirSurname"];

					if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
						return ActionResult.Continue;

					if (!WaitForExperianConsumerCheckToFinishUpdates(directorId))
					{
						log.Info("No data exist from experian consumer check for customer {0} director {1}.", customerId, directorId);
					}

					return ActionResult.Continue;
				},
				                "GetCustomerDirectorsForConsumerCheck",
				                CommandSpecies.StoredProcedure,
				                new QueryParameter("CustomerId", customerId)
				);
		}

		private bool WaitForExperianCompanyCheckToFinishUpdates()
		{
			log.Info("Waiting for experian company check");

			if (!hasCompany) // Customer has no company - so there is no need for waiting
			{
				return true;
			}

			return WaitForUpdateToFinish(GetIsExperianCompanyUpdated, totalTimeToWaitForExperianCompanyCheck, intervalWaitForExperianCompanyCheck);
		}

		private bool GetIsExperianCompanyUpdated()
		{
			return db.ExecuteScalar<bool>(
				"GetIsCompanyDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("Today", DateTime.Today)
			);
		}

		private bool WaitForExperianConsumerCheckToFinishUpdates(int? directorId = null)
		{
			log.Info("Waiting for experian consumer check");
			return WaitForUpdateToFinish(() => GetIsExperianConsumerUpdated(directorId), totalTimeToWaitForExperianConsumerCheck, intervalWaitForExperianConsumerCheck);
		}

		private bool GetIsExperianConsumerUpdated(int? directorId)
		{
			return db.ExecuteScalar<bool>(
				"GetIsConsumerDataUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DirectorId", directorId),
				new QueryParameter("Today", DateTime.Today)
			);
		}

		private bool WaitForAmlToFinishUpdates()
		{
			log.Info("Waiting for AML check");
			return WaitForUpdateToFinish(GetIsAmlUpdated, totalTimeToWaitForAmlCheck, intervalWaitForAmlCheck);
		}

		private bool GetIsAmlUpdated()
		{
			return db.ExecuteScalar<bool>("GetIsAmlUpdated",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));
		}

		public void Stall()
		{
			if (!wasMainStrategyExecutedBefore)
			{
				WaitForExperianConsumerCheckIfNeeded();
				WaitForExperianCompanyCheckIfNeeded();
				WaitForExperianAmlCheckIfNeeded();
			}

			bool endedWithError = MakeSureMpDataIsSufficient();

			// TODO: if nobody monitors these mails so there is no need for them
			if (endedWithError)
			{
				mailer.Send("Mandrill - No Information about shops", new Dictionary<string, string> {
						{"UserEmail", customerEmail},
						{"CustomerID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"ApplicationID", customerEmail}
					});
			}
		}
	}
}
