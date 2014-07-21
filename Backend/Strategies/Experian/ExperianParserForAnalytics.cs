namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Globalization;
	using System.Xml;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using Ezbob.Logger;

	internal class ExperianParserForAnalytics {
		#region constructor

		public ExperianParserForAnalytics(int nCustomerID, AConnection oDB, ASafeLog oLog) {
			m_nCustomerID = nCustomerID;
			m_oLog = oLog;
			m_oDB = oDB;
		} // constructor

		#endregion constructor

		#region method UpdateAnalytics

		public void UpdateAnalytics(decimal nMaxScore) {
			m_oLog.Debug("Updating customer analytics for customer {0}...", m_nCustomerID);

			string companyData = string.Empty;
			string experianRefNum = string.Empty;
			string experianCompanyName = string.Empty;
			string typeOfBusinessStr = string.Empty;

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					companyData = sr["CompanyData"];
					experianRefNum = sr["ExperianRefNum"];
					experianCompanyName = sr["ExperianCompanyName"];
					typeOfBusinessStr = sr["typeOfBusiness"];
					return ActionResult.SkipAll;
				},
				"GetPersonalInfoForExperianCompanyCheck",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			TypeOfBusinessReduced typeOfBusiness =
				((TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), typeOfBusinessStr)).Reduce();

			ExperianParserOutput output = ExperianParserFacade.Invoke(
				experianRefNum,
				experianCompanyName,
				ExperianParserFacade.Target.Company,
				typeOfBusiness
			);

			decimal tangibleEquity = 0;
			decimal adjustedProfit = 0;

			const string sKey = "Limited Company Financial Details IFRS & UK GAAP";

			ParsedData oParsedData = output.Dataset.ContainsKey(sKey) ?  output.Dataset[sKey] : null;

			if ((oParsedData != null) && (oParsedData.Data != null) && (oParsedData.Data.Count > 0)) {
				ParsedDataItem parsedDataItem = oParsedData.Data[0];

				decimal totalShareFund = GetDecimalValueFromDataItem(parsedDataItem, "TotalShareFund");
				decimal inTngblAssets = GetDecimalValueFromDataItem(parsedDataItem, "InTngblAssets");
				decimal debtorsDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "DebtorsDirLoans");
				decimal credDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "CredDirLoans");
				decimal onClDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "OnClDirLoans");

				tangibleEquity = totalShareFund - inTngblAssets - debtorsDirLoans + credDirLoans + onClDirLoans;

				if (oParsedData.Data.Count > 1) {
					ParsedDataItem parsedDataItemPrev = oParsedData.Data[1];

					decimal retainedEarnings = GetDecimalValueFromDataItem(parsedDataItem, "RetainedEarnings");
					decimal retainedEarningsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "RetainedEarnings");
					decimal fixedAssetsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "TngblAssets");

					adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev / 5;
				} // if
			} // if

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(companyData);

			string sic1980Code1, sic1980Desc1, sic1992Code1, sic1992Desc1;
			GetSicCodes(xmlDoc, out sic1980Code1, out sic1980Desc1, out sic1992Code1, out sic1992Desc1);

			int ageOfMostRecentCcj, numOfCcjsInLast24Months, sumOfCcjsInLast24Months;
			GetCcjs(xmlDoc, out ageOfMostRecentCcj, out numOfCcjsInLast24Months, out sumOfCcjsInLast24Months);

			int score = GetScore(xmlDoc);

			int creditLimit = GetCreditLimit(xmlDoc);
			DateTime? incorporationDate = GetIncorporationDate(xmlDoc);

			m_oLog.Info("Inserting to analytics Experian Score: {0} MaxScore: {1}.", score, nMaxScore);

			m_oDB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("Score", score),
				new QueryParameter("MaxScore", (int)nMaxScore),
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
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
			);

			m_oLog.Debug("Updating customer analytics for customer {0} and company '{1}' complete.", m_nCustomerID, experianRefNum);
		} // UpdateAnalytics

		#endregion method UpdateAnalytics

		private int GetScore(XmlDocument xmlDoc) {
			XmlNodeList dl76Nodes = xmlDoc.SelectNodes("//DL76");

			if (dl76Nodes != null && dl76Nodes.Count == 1) {
				XmlNode dl76Node = dl76Nodes[0];
				return GetIntValueOrDefault(dl76Node, "RISKSCORE");
			} // if

			return 0;
		} // GetScore

		private DateTime? GetDateFromNode(XmlNode node, string tag) {
			XmlNode yearNode = node.SelectSingleNode(tag + "-YYYY");
			XmlNode monthNode = node.SelectSingleNode(tag + "-MM");
			XmlNode dayNode = node.SelectSingleNode(tag + "-DD");

			if (yearNode != null && monthNode != null && dayNode != null) {
				string dateStr = string.Format(
					"{0}-{1}-{2}",
					yearNode.InnerText.Trim().PadLeft(4, '0'),
					monthNode.InnerText.Trim().PadLeft(2, '0'),
					dayNode.InnerText.Trim().PadLeft(2, '0')
				);

				DateTime result;

				if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
					return result;
			} // if

			return null;
		} // GetDateFromNode

		private DateTime? GetIncorporationDate(XmlDocument xmlDoc) {
			XmlNodeList dl12Nodes = xmlDoc.SelectNodes("//DL12");

			if (dl12Nodes != null && dl12Nodes.Count == 1) {
				XmlNode dl12Node = dl12Nodes[0];
				return GetDateFromNode(dl12Node, "DATEINCORP");
			} // if

			return null;
		} // GetIncorporationDate

		private int GetCreditLimit(XmlDocument xmlDoc) {
			XmlNodeList dl78Nodes = xmlDoc.SelectNodes("//DL78");

			if (dl78Nodes != null && dl78Nodes.Count == 1) {
				XmlNode dl78Node = dl78Nodes[0];
				return GetIntValueOrDefault(dl78Node, "CREDITLIMIT");
			} // if

			return 0;
		} // GetCreditLimit

		private void GetCcjs(
			XmlDocument xmlDoc,
			out int ageOfMostRecentCcj,
			out int numOfCcjsInLast24Months,
			out int sumOfCcjsInLast24Months
		) {
			XmlNodeList dl26Nodes = xmlDoc.SelectNodes("//DL26");

			if (dl26Nodes != null && dl26Nodes.Count == 1) {
				XmlNode dl26Node = dl26Nodes[0];

				ageOfMostRecentCcj = GetIntValueOrDefault(dl26Node, "AGEMOSTRECENTCCJ");

				numOfCcjsInLast24Months =
					GetIntValueOrDefault(dl26Node, "NUMCCJLAST12") +
					GetIntValueOrDefault(dl26Node, "NUMCCJ13TO24");

				sumOfCcjsInLast24Months =
					GetIntValueOrDefault(dl26Node, "VALCCJLAST12") +
					GetIntValueOrDefault(dl26Node, "VALCCJ13TO24");

				return;
			} // if

			ageOfMostRecentCcj = 0;
			numOfCcjsInLast24Months = 0;
			sumOfCcjsInLast24Months = 0;
		} // GetCcjs

		private int GetIntValueOrDefault(XmlNode element, string nodeName) {
			XmlNode node = element.SelectSingleNode(nodeName);

			if (node != null) {
				int result;

				if (int.TryParse(node.InnerText, out result))
					return result;
			} // if

			return 0;
		} // GetIntValueOrDefault

		private void GetSicCodes(
			XmlDocument xmlDoc,
			out string sic1980Code1,
			out string sic1980Desc1,
			out string sic1992Code1,
			out string sic1992Desc1
		) {
			XmlNodeList dl13Nodes = xmlDoc.SelectNodes("//DL13");

			if (dl13Nodes != null && dl13Nodes.Count == 1) {
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
			} // if

			sic1980Code1 = string.Empty;
			sic1980Desc1 = string.Empty;
			sic1992Code1 = string.Empty;
			sic1992Desc1 = string.Empty;
		} // GetSicCodes

		private decimal GetDecimalValueFromDataItem(ParsedDataItem parsedDataItem, string requiredValueName) {
			if (!parsedDataItem.Values.ContainsKey(requiredValueName))
				return 0;

			string sValue = parsedDataItem.Values[requiredValueName];

			decimal result;

			if (Transformation.ParseMoney(sValue, out result))
				return result;

			if (decimal.TryParse(sValue, out result))
				return result;

			return 0;
		} // GetDecimalValueFromDataItem

		private readonly ASafeLog m_oLog;
		private readonly AConnection m_oDB;
		private readonly int m_nCustomerID;
	} // class ExperianParserForAnalytics
} // namespace
