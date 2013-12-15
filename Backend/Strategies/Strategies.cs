namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Threading;
	using EzBob;
	using FraudChecker;
	using log4net;
	using System.Collections.Generic;
	using DbConnection;

	public class Strategies
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(Strategies));
		private readonly StrategiesMailer mailer = new StrategiesMailer();
		
		public void FraudChecker(int customerId)
		{
			var checker = new FraudDetectionChecker();
			checker.Check(customerId);
		}
		
		private object caisGenerationLock = new object();
		private int caisGenerationTriggerer = -1;
		public void CAISGenerate(int underwriterId)
		{
			lock (caisGenerationLock)
			{
				if (caisGenerationTriggerer != -1)
				{
					log.WarnFormat("A CAIS generation is already in progress. Triggered by Underwriter:{0}", caisGenerationTriggerer);
					return;
				}
				caisGenerationTriggerer = underwriterId;
			}
			
			// TODO: complete implementation - CAIS_NO_Upload

			lock (caisGenerationLock)
			{
				caisGenerationTriggerer = -1;
			}
		}

		public void CAISUpdate(int caisId)
		{
			// TODO: complete implementation - CAIS_NO_Upload
		}
		
		public void CustomerMarketPlaceAdded(int customerId, int marketplaceId)
		{
			DateTime startTime = DateTime.UtcNow;

			var requestId = RetrieveDataHelper.UpdateCustomerMarketplaceData(marketplaceId);

			while (!RetrieveDataHelper.IsRequestDone(requestId))
			{
				Thread.Sleep(1000); // TODO: make this configurable
			}
			var requestState = RetrieveDataHelper.GetRequestState(requestId);
			string errorCode = null;
			string errorMessage = null;
			bool tokenExpired = false;

			if (requestState == null || requestState.HasError())
			{
				DataTable dt = DbConnection.ExecuteSpReader("GetMarketplaceType");
				DataRow result = dt.Rows[0];
				string marketplaceType = result["Name"].ToString();

				if (requestState != null)
				{
					// TODO: make sure it contains the ebay error codes below such as token expired
					// What is the difference between errorCode & errorMessage?
					errorCode = requestState.ErorrInfo.Message; 
					errorMessage = requestState.ErorrInfo.Message;
				}

				string emailSubject, templateName;

				if (marketplaceType == "eBay" &&
					(errorCode == "16110" || errorCode == "931" || errorCode == "932" || errorCode == "16118" ||
					 errorCode == "16119" || errorCode == "17470"))
				{
					tokenExpired = true;
					emailSubject = "eBay token has expired";
					templateName = "Mandrill - Update MP Error Code";

					var variables = new Dictionary<string, string>
						{
							{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
							{"MPType", marketplaceType},
							{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
							{"ErrorMessage", errorMessage},
							{"ErrorCode", errorCode}
						};

					mailer.SendToEzbob(variables, templateName, emailSubject);
				}
				else
				{
					emailSubject = "ERROR occured when updating Customer Marketplace data";
					templateName = "Mandrill - UpdateCMP Error";

					// TODO: Remove ApplicationID from mandrill\mailchimp templates
					var variables = new Dictionary<string, string>
						{
							{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
							{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
							{"UpdateCMP_Error", errorMessage}
						};

					mailer.SendToEzbob(variables, templateName, emailSubject);
				}
			}

			DbConnection.ExecuteSpNonQuery("UpdateMPErrorMP",
				DbConnection.CreateParam("umi", marketplaceId),
				DbConnection.CreateParam("UpdateError", errorMessage),
				DbConnection.CreateParam("TokenExpired", tokenExpired));

			DbConnection.ExecuteSpNonQuery("InsertStrategyMarketPlaceUpdateTime",
				DbConnection.CreateParam("MarketPlaceId", marketplaceId),
				DbConnection.CreateParam("StartDate", startTime),
				DbConnection.CreateParam("EndDate", DateTime.UtcNow));

		}

		public void UpdateAllMarketplaces(int customerId)
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetCustomerMarketplaces", DbConnection.CreateParam("CustomerId", customerId));
			foreach (DataRow row in dt.Rows)
			{
				int marketplaceId = int.Parse(row["Id"].ToString());
				CustomerMarketPlaceAdded(customerId, marketplaceId);
			}
		}

		// main strategy - 1
		public void Evaluate(int customerId, NewCreditLineOption newCreditLineOption, int avoidAutomaticDescison, bool isUnderwriterForced = false)
		{
			var mainStrategy = new MainStrategy();
			mainStrategy.Evaluate(customerId, newCreditLineOption, avoidAutomaticDescison, isUnderwriterForced);
		}
		
		// main strategy - 2
		public void EvaluateWithIdHubCustomAddress(int customerId, int checkType, string houseNumber, string houseName, string street,
											string district, string town, string county, string postcode, string bankAccount, string sortCode, int avoidAutomaticDescison)
		{
			var mainStrategy = new MainStrategy();
			mainStrategy.EvaluateWithIdHubCustomAddress(customerId, checkType, houseNumber, houseName, street, district, town, county, postcode, bankAccount, sortCode, avoidAutomaticDescison);
		}
	}
}
