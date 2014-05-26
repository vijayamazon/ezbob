namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Data;
	using System.IO;
	using System.Xml;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		#region public

		#region constructor

		public ExperianCompanyCheck(int nCustomerID, bool bForceCheck, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oExperianData = null;
			m_nCustomerID = nCustomerID;
			m_bForceCheck = bForceCheck;
			m_bFoundCompany = false;

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_bFoundCompany = true;

					string companyType = sr["CompanyType"];
					m_sExperianRefNum = sr["ExperianRefNum"];

					m_bIsLimited = companyType == "Limited" || companyType == "LLP";

					return ActionResult.SkipAll;
				},
				"GetCompanyData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if (!m_bFoundCompany)
				oLog.Info("Can't find company data for customer:{0}. The customer is probably an entrepreneur.");
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Info("Starting company check with parameters: IsLimited={0} ExperianRefNum={1}", m_bIsLimited ? "yes" : "no", m_sExperianRefNum);

			if (!m_bFoundCompany || (m_sExperianRefNum == "NotFound")) {
				Log.Info("Can't execute Experian company check for customer with no company");
				return;
			} // if

			string experianError = null;
			decimal experianBureauScore = 0;

			if (string.IsNullOrEmpty(m_sExperianRefNum))
				experianError = "RefNumber is empty";
			else {
				m_oExperianData = GetBusinessDataFromExperian();

				if (!m_oExperianData.IsError)
					experianBureauScore = m_oExperianData.BureauScore;
				else
					experianError = m_oExperianData.Error;
			} // if

			DB.ExecuteNonQuery(
				"UpdateExperianBusiness",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNumber", m_sExperianRefNum),
				new QueryParameter("ExperianError", experianError),
				new QueryParameter("ExperianScore", experianBureauScore),
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			UpdateAnalytics();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method GetBusinessDataFromExperian

		private BusinessReturnData GetBusinessDataFromExperian() {
			var service = new EBusinessService();

			if (m_bIsLimited)
				return service.GetLimitedBusinessData(m_sExperianRefNum, m_nCustomerID, false, m_bForceCheck);

			return service.GetNotLimitedBusinessData(m_sExperianRefNum, m_nCustomerID, false, m_bForceCheck);
		} // GetBusinessDataFromExperian

		#endregion method GetBusinessDataFromExperian

		#region method UpdateAnalytics

		private void UpdateAnalytics() {
			if ((m_oExperianData == null) || m_oExperianData.IsError)
				return;

			if (m_oExperianData.CacheHit)
			{
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
				{
					return;
				}
			}

			Log.Debug("Updating customer analytics for customer {0} and company '{1}'...", m_nCustomerID, m_sExperianRefNum);

			DataTable dt = DB.ExecuteReader("GetPersonalInfoForExperianCompanyCheck",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID));

			var sr = new SafeReader(dt.Rows[0]);
			string companyData = sr["CompanyData"];
			string experianRefNum = sr["ExperianRefNum"];
			string experianCompanyName = sr["ExperianCompanyName"];
			string typeOfBusinessStr = sr["typeOfBusiness"];

			TypeOfBusinessReduced typeOfBusiness = ((TypeOfBusiness)Enum.Parse(typeof(TypeOfBusiness), typeOfBusinessStr)).Reduce();
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
				
				tangibleEquity =  totalShareFund - inTngblAssets -debtorsDirLoans + credDirLoans + onClDirLoans;

				if (output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data.Count != 1)
				{
					ParsedDataItem parsedDataItemPrev = output.Dataset["Limited Company Financial Details IFRS & UK GAAP"].Data[1];

					decimal retainedEarnings = GetDecimalValueFromDataItem(parsedDataItem, "RetainedEarnings");
					decimal retainedEarningsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "RetainedEarnings");
					decimal fixedAssetsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "TngblAssets");

					adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev / 5;
				}
			}

			XmlDocument xmlDoc = GetXmlDocumentObject(companyData);
			string sic1980Code1, sic1980Desc1, sic1992Code1, sic1992Desc1;
			GetSicCodes(xmlDoc, out sic1980Code1, out sic1980Desc1, out sic1992Code1, out sic1992Desc1);
			
			int ageOfMostRecentCcj, numOfCcjsInLast24Months, sumOfCcjsInLast24Months;
			GetCcjs(xmlDoc, typeOfBusiness == TypeOfBusinessReduced.Limited, out ageOfMostRecentCcj, out numOfCcjsInLast24Months, out sumOfCcjsInLast24Months);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("Score", m_oExperianData.BureauScore),
				new QueryParameter("SuggestedAmount", m_oExperianData.CreditLimit),
				new QueryParameter("IncorporationDate", m_oExperianData.IncorporationDate),
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

			Log.Debug("Updating customer analytics for customer {0} and company '{1}' complete.", m_nCustomerID, m_sExperianRefNum);
		} // UpdateAnalytics

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

		private void GetCcjs(XmlDocument xmlDoc, bool isLimited, out int ageOfMostRecentCcj, out int numOfCcjsInLast24Months, out int sumOfCcjsInLast24Months)
		{
			if (isLimited)
			{
				XmlNodeList dl26Nodes = xmlDoc.SelectNodes("//DL26");
				if (dl26Nodes != null && dl26Nodes.Count == 1)
				{
					XmlNode dl26Node = dl26Nodes[0];
					ageOfMostRecentCcj = GetIntValueOrDefault(dl26Node, "AGEMOSTRECENTCCJ");
					numOfCcjsInLast24Months = GetIntValueOrDefault(dl26Node, "NUMCCJLAST12") + GetIntValueOrDefault(dl26Node, "NUMCCJ13TO24");
					sumOfCcjsInLast24Months = GetIntValueOrDefault(dl26Node, "VALCCJLAST12") + GetIntValueOrDefault(dl26Node, "VALCCJ13TO24");
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

		public void GetSicCodes(XmlDocument xmlDoc, out string sic1980Code1, out string sic1980Desc1, out string sic1992Code1, out string sic1992Desc1)
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

		#endregion method UpdateAnalytics
		
		private bool IsCustomerAlreadyInAnalytics()
		{
			DataTable dt = DB.ExecuteReader(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if (dt.Rows.Count == 1)
			{
				return true;
			}

			return false;
		}

		#region fields

		private readonly int m_nCustomerID;
		private bool m_bFoundCompany;
		private readonly bool m_bForceCheck;
		private bool m_bIsLimited;
		private string m_sExperianRefNum;
		private BusinessReturnData m_oExperianData;

		#endregion fields

		#endregion private
	} // class ExperianCompanyCheck
} // namespace
