﻿namespace EzBob.Backend.Strategies.Experian
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Xml;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using Ezbob.Logger;

	#region class ExperianUtils

	internal class ExperianParserForAnalytics
	{
		public ExperianParserForAnalytics(ASafeLog log, AConnection db)
		{
			this.log = log;
			this.db = db;
		}

		public void UpdateAnalytics(int customerId)
		{
			log.Debug("Updating customer analytics for customer {0}", customerId);

			DataTable dt = db.ExecuteReader("GetPersonalInfoForExperianCompanyCheck",
			                                CommandSpecies.StoredProcedure,
			                                new QueryParameter("CustomerId", customerId));

			var sr = new SafeReader(dt.Rows[0]);
			string companyData = sr["CompanyData"];
			string experianRefNum = sr["ExperianRefNum"];
			string experianCompanyName = sr["ExperianCompanyName"];
			string typeOfBusinessStr = sr["typeOfBusiness"];

			TypeOfBusinessReduced typeOfBusiness =
				((TypeOfBusiness) Enum.Parse(typeof (TypeOfBusiness), typeOfBusinessStr)).Reduce();
			ExperianParserOutput output = ExperianParserFacade.Invoke(
				experianRefNum,
				experianCompanyName,
				ExperianParserFacade.Target.Company,
				typeOfBusiness
				);

			decimal tangibleEquity = 0;
			decimal adjustedProfit = 0;
			if (output.Dataset.ContainsKey("Limited Company Financial Details IFRS & UK GAAP") &&
			    output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data != null &&
			    output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data.Count != 0)
			{
				ParsedDataItem parsedDataItem = output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data[0];
				decimal totalShareFund = GetDecimalValueFromDataItem(parsedDataItem, "TotalShareFund");
				decimal inTngblAssets = GetDecimalValueFromDataItem(parsedDataItem, "InTngblAssets");
				decimal debtorsDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "DebtorsDirLoans");
				decimal credDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "CredDirLoans");
				decimal onClDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "OnClDirLoans");

				tangibleEquity = totalShareFund - inTngblAssets - debtorsDirLoans + credDirLoans + onClDirLoans;

				if (output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data.Count != 1)
				{
					ParsedDataItem parsedDataItemPrev = output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data[1];

					decimal retainedEarnings = GetDecimalValueFromDataItem(parsedDataItem, "RetainedEarnings");
					decimal retainedEarningsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "RetainedEarnings");
					decimal fixedAssetsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "TngblAssets");

					adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev/5;
				}
			}

			XmlDocument xmlDoc = GetXmlDocumentObject(companyData);
			string sic1980Code1, sic1980Desc1, sic1992Code1, sic1992Desc1;
			GetSicCodes(xmlDoc, out sic1980Code1, out sic1980Desc1, out sic1992Code1, out sic1992Desc1);

			bool isLimited = typeOfBusiness == TypeOfBusinessReduced.Limited;
			int ageOfMostRecentCcj, numOfCcjsInLast24Months, sumOfCcjsInLast24Months;
			GetCcjs(xmlDoc, isLimited, out ageOfMostRecentCcj, out numOfCcjsInLast24Months, out sumOfCcjsInLast24Months);

			int score = GetScore(xmlDoc, isLimited);
			int creditLimit = GetCreditLimit(xmlDoc, isLimited);
			DateTime? incorporationDate = GetIncorporationDate(xmlDoc, isLimited);

			ParseExperianDl97Accounts(customerId, xmlDoc);

			db.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerId),
				new QueryParameter("Score", score),
				new QueryParameter("SuggestedAmount", creditLimit),
				new QueryParameter("IncorporationDate", incorporationDate),
				new QueryParameter("TangibleEquity", tangibleEquity),
				new QueryParameter("AdjustedProfit", adjustedProfit),
				new QueryParameter("Sic1980Code1", sic1980Code1),
				new QueryParameter("Sic1980Desc1", sic1980Desc1),
				new QueryParameter("Sic1992Code1", sic1992Code1),
				new QueryParameter("Sic1992Desc1", sic1992Desc1),
				new QueryParameter("AgeOfMostRecentCcj", ageOfMostRecentCcj),
				new QueryParameter("NumOfCcjsInLast24Months", numOfCcjsInLast24Months),
				new QueryParameter("SumOfCcjsInLast24Months", sumOfCcjsInLast24Months),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow));

			log.Debug("Updating customer analytics for customer {0} and company '{1}' complete.", customerId, experianRefNum);
		}

		private void ParseExperianDl97Accounts(int customerId, XmlDocument xmlDoc)
		{
			db.ExecuteNonQuery(
				"DeleteExperianDL97Accounts",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId));

			XmlNodeList dl97List = xmlDoc.SelectNodes("//DL97");
			if (dl97List != null)
			{
				foreach (XmlElement dl97 in dl97List)
				{
					XmlNode stateNode = dl97.SelectSingleNode("ACCTSTATE");
					XmlNode typeNode = dl97.SelectSingleNode("ACCTTYPE");
					XmlNode status12MonthsNode = dl97.SelectSingleNode("ACCTSTATUS12");
					XmlNode lastUpdatedYearNode = dl97.SelectSingleNode("CAISLASTUPDATED-YYYY");
					XmlNode lastUpdatedMonthNode = dl97.SelectSingleNode("CAISLASTUPDATED-MM");
					XmlNode lastUpdatedDayNode = dl97.SelectSingleNode("CAISLASTUPDATED-DD");
					XmlNode companyTypeNode = dl97.SelectSingleNode("COMPANYTYPE");
					XmlNode currentBalanceNode = dl97.SelectSingleNode("CURRBALANCE");
					XmlNode monthsDataNode = dl97.SelectSingleNode("MONTHSDATA");
					XmlNode status1To2Node = dl97.SelectSingleNode("STATUS1TO2");
					XmlNode status3To9Node = dl97.SelectSingleNode("STATUS3TO9");

					string state = stateNode != null ? stateNode.InnerText : string.Empty;
					string type = typeNode != null ? typeNode.InnerText : string.Empty;
					string status12Months = status12MonthsNode != null ? status12MonthsNode.InnerText : string.Empty;
					DateTime? lastUpdated = null;
					if (lastUpdatedYearNode != null && lastUpdatedMonthNode != null && lastUpdatedDayNode != null)
					{
						int year, month, day;
						if (int.TryParse(lastUpdatedYearNode.InnerText, out year) &&
							int.TryParse(lastUpdatedMonthNode.InnerText, out month) &&
							int.TryParse(lastUpdatedDayNode.InnerText, out day))
						{
							lastUpdated = new DateTime(year, month, day);
						}
					}
					string companyType = companyTypeNode != null ? companyTypeNode.InnerText : string.Empty;
					int currentBalance = 0;
					if (currentBalanceNode != null)
					{
						int.TryParse(currentBalanceNode.InnerText, out currentBalance);
					}
					int monthsData = 0;
					if (monthsDataNode != null)
					{
						int.TryParse(monthsDataNode.InnerText, out monthsData);
					}
					int status1To2 = 0;
					if (status1To2Node != null)
					{
						int.TryParse(status1To2Node.InnerText, out status1To2);
					}
					int status3To9 = 0;
					if (status3To9Node != null)
					{
						int.TryParse(status3To9Node.InnerText, out status3To9);
					}

					db.ExecuteNonQuery(
						"AddExperianDL97Accounts",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", customerId),
						new QueryParameter("State", state),
						new QueryParameter("Type", type),
						new QueryParameter("Status12Months", status12Months),
						new QueryParameter("LastUpdated", lastUpdated),
						new QueryParameter("CompanyType", companyType),
						new QueryParameter("CurrentBalance", currentBalance),
						new QueryParameter("MonthsData", monthsData),
						new QueryParameter("Status1To2", status1To2),
						new QueryParameter("Status3To9", status3To9));
				}
			}
		}

		private int GetScore(XmlDocument xmlDoc, bool isLimited)
		{
			if (isLimited)
			{
				XmlNodeList dl76Nodes = xmlDoc.SelectNodes("//DL76");
				if (dl76Nodes != null && dl76Nodes.Count == 1)
				{
					XmlNode dl76Node = dl76Nodes[0];
					return GetIntValueOrDefault(dl76Node, "RISKSCORE");
				}

				return 0;
			}

			// non-limited
			XmlNodeList dn40Nodes = xmlDoc.SelectNodes("//DN40");
			if (dn40Nodes != null && dn40Nodes.Count == 1)
			{
				XmlNode dn40Node = dn40Nodes[0];
				return GetIntValueOrDefault(dn40Node, "RISKSCORE");
			}

			return 0;
		}

		private DateTime? GetDateFromNode(XmlNode node, string tag)
		{
			XmlNode yearNode = node.SelectSingleNode(tag + "-YYYY");
			XmlNode monthNode = node.SelectSingleNode(tag + "-MM");
			XmlNode dayNode = node.SelectSingleNode(tag + "-DD");

			if (yearNode != null && monthNode != null && dayNode != null)
			{
				string dateStr = string.Format(
					"{0}-{1}-{2}",
					yearNode.InnerText.Trim().PadLeft(4, '0'),
					monthNode.InnerText.Trim().PadLeft(2, '0'),
					dayNode.InnerText.Trim().PadLeft(2, '0'));

				DateTime result;
				if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					return result;
				} // if
			}

			return null;
		}

		private DateTime? GetIncorporationDate(XmlDocument xmlDoc, bool isLimited)
		{
			if (isLimited)
			{
				XmlNodeList dl12Nodes = xmlDoc.SelectNodes("//DL12");
				if (dl12Nodes != null && dl12Nodes.Count == 1)
				{
					XmlNode dl12Node = dl12Nodes[0];
					return GetDateFromNode(dl12Node, "DATEINCORP");
				}
				return null;
			}

			// non-limited
			XmlNodeList dn10Nodes = xmlDoc.SelectNodes("//DN10");
			if (dn10Nodes != null && dn10Nodes.Count == 1)
			{
				XmlNode dn10Node = dn10Nodes[0];
				DateTime? res = GetDateFromNode(dn10Node, "DATEOWNSHPCOMMD");
				if (res != null)
				{
					return res;
				}

				return GetDateFromNode(dn10Node, "EARLIESTKNOWNDATE");
			}

			return null;
		}

		private int GetCreditLimit(XmlDocument xmlDoc, bool isLimited)
		{
			if (isLimited)
			{
				XmlNodeList dl78Nodes = xmlDoc.SelectNodes("//DL78");
				if (dl78Nodes != null && dl78Nodes.Count == 1)
				{
					XmlNode dl78Node = dl78Nodes[0];
					return GetIntValueOrDefault(dl78Node, "CREDITLIMIT");
				}

				return 0;
			}

			// non-limited
			XmlNodeList dn73Nodes = xmlDoc.SelectNodes("//DN73");
			if (dn73Nodes != null && dn73Nodes.Count == 1)
			{
				XmlNode dn73Node = dn73Nodes[0];
				return GetIntValueOrDefault(dn73Node, "CREDITLIMIT");
			}

			return 0;
		}

		private XmlDocument GetXmlDocumentObject(string responseXml)
		{
			var xmlDoc = new XmlDocument();
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(responseXml);
			writer.Flush();
			stream.Position = 0;
			xmlDoc.Load(stream);
			return xmlDoc;
		}

		private void GetCcjs(XmlDocument xmlDoc, bool isLimited, out int ageOfMostRecentCcj, out int numOfCcjsInLast24Months,
		                     out int sumOfCcjsInLast24Months)
		{
			if (isLimited)
			{
				XmlNodeList dl26Nodes = xmlDoc.SelectNodes("//DL26");
				if (dl26Nodes != null && dl26Nodes.Count == 1)
				{
					XmlNode dl26Node = dl26Nodes[0];
					ageOfMostRecentCcj = GetIntValueOrDefault(dl26Node, "AGEMOSTRECENTCCJ");
					numOfCcjsInLast24Months = GetIntValueOrDefault(dl26Node, "NUMCCJLAST12") +
					                          GetIntValueOrDefault(dl26Node, "NUMCCJ13TO24");
					sumOfCcjsInLast24Months = GetIntValueOrDefault(dl26Node, "VALCCJLAST12") +
					                          GetIntValueOrDefault(dl26Node, "VALCCJ13TO24");
					return;
				}
			}
			else
			{
				XmlNodeList dn14Nodes = xmlDoc.SelectNodes("//DN14");
				if (dn14Nodes != null && dn14Nodes.Count == 1)
				{
					XmlNode dn14Node = dn14Nodes[0];
					ageOfMostRecentCcj = GetIntValueOrDefault(dn14Node, "MAGEMOSTRECJUDGSINCEOWNSHP");
					numOfCcjsInLast24Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGCOUNTLST24MNTHS") +
					                          GetIntValueOrDefault(dn14Node, "ATOTJUDGCOUNTLST24MNTHS");
					sumOfCcjsInLast24Months = GetIntValueOrDefault(dn14Node, "MTOTJUDGVALUELST24MNTHS") +
					                          GetIntValueOrDefault(dn14Node, "ATOTJUDGVALUELST24MNTHS");
					return;
				}
			}

			ageOfMostRecentCcj = 0;
			numOfCcjsInLast24Months = 0;
			sumOfCcjsInLast24Months = 0;
		}

		private int GetIntValueOrDefault(XmlNode element, string nodeName)
		{
			XmlNode node = element.SelectSingleNode(nodeName);

			if (node != null)
			{
				int result;
				if (int.TryParse(node.InnerText, out result))
				{
					return result;
				}
			}

			return 0;
		}

		private void GetSicCodes(XmlDocument xmlDoc, out string sic1980Code1, out string sic1980Desc1, out string sic1992Code1,
		                         out string sic1992Desc1)
		{
			XmlNodeList dl13Nodes = xmlDoc.SelectNodes("//DL13");
			if (dl13Nodes != null && dl13Nodes.Count == 1)
			{
				XmlNode dl13Node = dl13Nodes[0];
				XmlNode sic1980Code1Node = dl13Node.SelectSingleNode("SIC1980CODE1");
				XmlNode sic1980Desc1Node = dl13Node.SelectSingleNode("SIC1980DESC1");
				XmlNode sic1992Code1Node = dl13Node.SelectSingleNode("SIC1992CODE1");
				XmlNode sic1992Desc1Node = dl13Node.SelectSingleNode("SIC1992DESC1");
				sic1980Code1 = sic1980Code1Node != null ? sic1980Code1Node.InnerText : string.Empty;
				sic1980Desc1 = sic1980Desc1Node != null ? sic1980Desc1Node.InnerText : string.Empty;
				sic1992Code1 = sic1992Code1Node != null ? sic1992Code1Node.InnerText : string.Empty;
				sic1992Desc1 = sic1992Desc1Node != null ? sic1992Desc1Node.InnerText : string.Empty;

				return;
			}
			sic1980Code1 = string.Empty;
			sic1980Desc1 = string.Empty;
			sic1992Code1 = string.Empty;
			sic1992Desc1 = string.Empty;
		}

		private decimal GetDecimalValueFromDataItem(ParsedDataItem parsedDataItem, string requiredValueName)
		{
			string strValue = parsedDataItem.Values[requiredValueName];
			if (strValue.Length > 0)
			{
				strValue = strValue.Substring(1); // Remove pound sign
			}

			decimal result;
			if (!decimal.TryParse(strValue, out result))
			{
				return 0;
			}

			return result;
		}

		private readonly ASafeLog log;
		private readonly AConnection db;

		#endregion private
	}
}
