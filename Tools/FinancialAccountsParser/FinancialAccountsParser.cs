namespace FinancialAccountsParser
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml;
	using Ezbob.Context;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class FinancialAccountsParser
	{
		private readonly AConnection db;
		private readonly SafeILog log;

		public FinancialAccountsParser()
		{
			log = new SafeILog(LogManager.GetLogger(typeof (FinancialAccountsParser)));
			db = new SqlConnection(new Ezbob.Context.Environment(Name.Production), log);
		}

		public void Execute()
		{
			DataTable dt = db.ExecuteReader("GetUnprocessedServiceLogEntries", CommandSpecies.StoredProcedure);
			log.Info("Fetched {0} entries", dt.Rows.Count);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				int serviceLogId = sr["Id"];
				int customerId = sr["CustomerId"];
				string response = sr["ResponseData"];

				HandleResponse(response, serviceLogId, customerId);
			}
		}

		private void HandleResponse(string response, int serviceLogId, int customerId)
		{
			var xmlDoc = new XmlDocument();

			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(response.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
			writer.Flush();
			stream.Position = 0;
			xmlDoc.Load(stream);

			XmlNodeList caisDetailsList = xmlDoc.SelectNodes("//CAISDetails");
			if (caisDetailsList != null)
			{
				log.Info("Found {0} financial account blocks for customer {1}");
				var financialAccounts = new List<FinancialAccount>();
				bool hadError = false;
				foreach (XmlElement currentCaisDetails in caisDetailsList)
				{
					try
					{
						FinancialAccount financialAccount = HandleOneCaisDetailsBlock(currentCaisDetails, serviceLogId, customerId);
						financialAccounts.Add(financialAccount);
					}
					catch (Exception ex)
					{
						hadError = true;
						log.Error("Exception while parsing data for ServiceLogId:{0}. No data will be created. The exception:{1}", serviceLogId, ex);
					}
				}

				if (!hadError && financialAccounts.Count > 0)
				{
					UpdateFinancialAccounts(financialAccounts, serviceLogId, customerId);
				}
			}
		}

		private void UpdateFinancialAccounts(List<FinancialAccount> financialAccounts, int serviceLogId, int customerId)
		{
			db.ExecuteNonQuery("DeletePreviousFinancialAccounts", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			foreach (FinancialAccount financialAccount in financialAccounts)
			{
				db.ExecuteNonQuery("InsertFinancialAccount", CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("ServiceLogId", serviceLogId),
					new QueryParameter("StartDate", financialAccount.StartDate), //qqq - consider null for all dates
					new QueryParameter("AccountStatus", financialAccount.AccountStatus),
					new QueryParameter("DateType", financialAccount.DateType),
					new QueryParameter("SettlementOrDefaultDate", financialAccount.SettlementOrDefaultDate),
					new QueryParameter("LastUpdateDate", financialAccount.LastUpdateDate),
					new QueryParameter("StatusCode1", financialAccount.StatusCodes[0]),
					new QueryParameter("StatusCode2", financialAccount.StatusCodes[1]),
					new QueryParameter("StatusCode3", financialAccount.StatusCodes[2]),
					new QueryParameter("StatusCode4", financialAccount.StatusCodes[3]),
					new QueryParameter("StatusCode5", financialAccount.StatusCodes[4]),
					new QueryParameter("StatusCode6", financialAccount.StatusCodes[5]),
					new QueryParameter("StatusCode7", financialAccount.StatusCodes[6]),
					new QueryParameter("StatusCode8", financialAccount.StatusCodes[7]),
					new QueryParameter("StatusCode9", financialAccount.StatusCodes[8]),
					new QueryParameter("StatusCode10", financialAccount.StatusCodes[9]),
					new QueryParameter("StatusCode11", financialAccount.StatusCodes[10]),
					new QueryParameter("StatusCode12", financialAccount.StatusCodes[11]),
					new QueryParameter("CreditLimit", financialAccount.CreditLimit),
					new QueryParameter("Balance", financialAccount.Balance),
					new QueryParameter("CurrentDefaultBalance", financialAccount.CurrentDefaultBalance),
					new QueryParameter("Status1To2", financialAccount.Status1To2),
					new QueryParameter("StatusTo3", financialAccount.StatusTo3),
					new QueryParameter("WorstStatus", financialAccount.WorstStatus),
					new QueryParameter("AccountType", financialAccount.AccountType));
			}
		}

		private DateTime? ReadDateFromNode(XmlNode node)
		{
			if (node == null)
			{
				return null;
			}

			XmlNode yearNode = node.SelectSingleNode("CCYY");
			XmlNode monthNode = node.SelectSingleNode("MM");
			XmlNode dayNode = node.SelectSingleNode("DD");

			if (yearNode == null || monthNode == null || dayNode == null)
			{
				return null;
			}

			string yearStr = yearNode.InnerText;
			string monthStr = monthNode.InnerText;
			string dayStr = dayNode.InnerText;

			int month, year, day;
			if (!int.TryParse(yearStr, out year) || !int.TryParse(monthStr, out month) || !int.TryParse(dayStr, out day))
			{
				return null;
			}

			return new DateTime(year, month, day);
		}

		private int? ReadAmountFromNode(XmlNode node)
		{
			if (node == null)
			{
				return null;
			}

			XmlNode amountNode = node.SelectSingleNode("Amount");
			if (amountNode != null)
			{
				string amountStr = amountNode.InnerText;
				string amountDigitsStr = Regex.Replace(amountStr, "[^.0-9]", string.Empty);
				int amountCreditLimit;
				if (int.TryParse(amountDigitsStr, out amountCreditLimit))
				{
					return amountCreditLimit;
				}
			}

			return null;
		}

		private FinancialAccount HandleOneCaisDetailsBlock(XmlElement currentCaisDetails, int serviceLogId, int customerId)
		{
			var result = new FinancialAccount();

			result.StartDate = ReadDateFromNode(currentCaisDetails.SelectSingleNode("CAISAccStartDate"));
			
			XmlNode accountStatusCodeNode = currentCaisDetails.SelectSingleNode("AccountStatus");
			string accountStatusCode = null;
			if (accountStatusCodeNode != null)
			{
				accountStatusCode = accountStatusCodeNode.InnerText;
			}
			string dateType;
			result.AccountStatus = GetAccountStatusString(accountStatusCode, out dateType);
			result.DateType = dateType;
			
			if (accountStatusCode == "F" || accountStatusCode == "S")
			{
				result.SettlementOrDefaultDate = ReadDateFromNode(currentCaisDetails.SelectSingleNode("SettlementDate"));
			}

			result.LastUpdateDate = ReadDateFromNode(currentCaisDetails.SelectSingleNode("LastUpdatedDate"));
			
			XmlNode accountStatusCodesNode = currentCaisDetails.SelectSingleNode("AccountStatusCodes");
			string statusCodes = string.Empty;
			if (accountStatusCodesNode != null)
			{
				statusCodes = accountStatusCodesNode.InnerText;
			}

			for (int i = 0; i < 12; i++)
			{
				result.StatusCodes.Add(statusCodes.Length > i ? statusCodes.Substring(i, 1) : string.Empty);
			}

			result.CreditLimit = ReadAmountFromNode(currentCaisDetails.SelectSingleNode("CreditLimit"));
			result.Balance = ReadAmountFromNode(currentCaisDetails.SelectSingleNode("Balance"));
			result.CurrentDefaultBalance = ReadAmountFromNode(currentCaisDetails.SelectSingleNode("CurrentDefBalance"));

			XmlNode status1To2Node = currentCaisDetails.SelectSingleNode("Status1To2");
			if (status1To2Node != null)
			{
				int status1To2;
				if (int.TryParse(status1To2Node.InnerText, out status1To2))
				{
					result.Status1To2 = status1To2;
				}
			}

			XmlNode statusTo3Node = currentCaisDetails.SelectSingleNode("StatusTo3");
			if (statusTo3Node != null)
			{
				int statusTo3;
				if (int.TryParse(statusTo3Node.InnerText, out statusTo3))
				{
					result.StatusTo3 = statusTo3;
				}
			}

			XmlNode worstStatusNode = currentCaisDetails.SelectSingleNode("WorstStatus");
			if (worstStatusNode != null)
			{
				result.WorstStatus = worstStatusNode.InnerText;
			}

			XmlNode accountTypeNode = currentCaisDetails.SelectSingleNode("AccountType");
			if (accountTypeNode != null)
			{
				result.AccountType = accountTypeNode.InnerText;
			}

			return result;
		}

		private string GetAccountStatusString(string status, out string dateType)
		{
			switch (status)
			{
				case "D":
					dateType = "Delinquent Date";
					return "Delinquent";
				case "F":
					dateType = "Default Date";
					return "Default";
				case "S":
					dateType = "Settlement Date";
					return "Settled";
				default: // "A" or unexpected
					dateType = "Last Update Date";
					return "Active";
			}
		}
	}
}
