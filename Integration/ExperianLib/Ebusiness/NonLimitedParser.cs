namespace ExperianLib.EBusiness 
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using log4net;

	public class NonLimitedParser 
	{
		private readonly AConnection db;
		private readonly SafeILog log;
		private XmlDocument xmlDoc;

		// DN10 fields
		private string businessName;
		private string address1;
		private string address2;
		private string address3;
		private string address4;
		private string address5;
		private string postcode;
		private List<string> sicCodes1992;
		private List<string> sicDescs1992;
		private string telephoneNumber;
		private string principalActivities;
		private DateTime? dateOwnershipCommenced;
		private DateTime? earliestKnownDate;
		private DateTime? incorporationDate;
		private DateTime? dateOwnershipCeased;
		private DateTime? lastUpdateDate;

		//DN11
		private List<DN11> bankruptcyDetails;

		//DN12
		private int? bankruptcyCountDuringOwnership;
		private int? ageOfMostRecentBankruptcyDuringOwnershipMonths;
		private int? associatedBankruptcyCountDuringOwnership;
		private int? ageOfMostRecentAssociatedBankruptcyDuringOwnershipMonths;

		//DN13
		private List<DN13> ccjDetails;

		//DN14
		private int? ageOfMostRecentJudgmentDuringOwnershipMonths;
		private int? totalJudgmentCountLast12Months;
		private int? totalJudgmentValueLast12Months;
		private int? totalJudgmentCountLast13To24Months;
		private int? totalJudgmentValueLast13To24Months;
		private int? valueOfMostRecentAssociatedJudgmentDuringOwnership;
		private int? totalAssociatedJudgmentCountLast12Months;
		private int? totalAssociatedJudgmentValueLast12Months;
		private int? totalAssociatedJudgmentCountLast13To24Months;
		private int? totalAssociatedJudgmentValueLast13To24Months;
		private int? totalJudgmentCountLast24Months;
		private int? totalAssociatedJudgmentCountLast24Months;
		private int? totalJudgmentValueLast24Months;
		private int? totalAssociatedJudgmentValueLast24Months;

		//DN17
		private List<DN17> previousSearches;

		//DN23
		private string supplierName;
		private string fraudCategory;
		private string fraudCategoryDesc;

		//DN26
		private int? numberOfAccountsPlacedForCollection;
		private int? valueOfAccountsPlacedForCollection;
		private int? numberOfAccountsPlacedForCollectionLast2Years;
		private int? averageDaysBeyondTermsFor0To100;
		private int? averageDaysBeyondTermsFor101To1000;
		private int? averageDaysBeyondTermsFor1001To10000;
		private int? averageDaysBeyondTermsForOver10000;
		private int? averageDaysBeyondTermsForLast3MonthsOfDataReturned;
		private int? averageDaysBeyondTermsForLast6MonthsOfDataReturned;
		private int? averageDaysBeyondTermsForLast12MonthsOfDataReturned;
		private int? currentAverageDebt;
		private int? averageDebtLast3Months;
		private int? averageDebtLast12Months;
		private List<CommonTerm> commonTerms; 

		//DN36
		private string telephoneNumberDN36;

		//DN40
		private List<Tuple<DateTime?, int?>> scoreHistory;
		private int? riskScore;

		//DN73
		private string searchType;
		private string searchTypeDesc;
		private int? commercialDelphiScore;
		private string creditRating;
		private string creditLimit;
		private decimal? probabilityOfDefaultScore;
		private string stabilityOdds;
		private string riskBand;
		private int? numberOfProprietorsSearched;
		private int? numberOfProprietorsFound;

		//Errors
		string errors = string.Empty;

		public NonLimitedParser()
		{
			log = new SafeILog(LogManager.GetLogger(typeof(NonLimitedParser)));
			var env = new Ezbob.Context.Environment(log);
			db = new SqlConnection(env, log);
		}

		private void Init()
		{
			sicCodes1992 = new List<string>();
			sicDescs1992 = new List<string>();

			bankruptcyDetails = new List<DN11>();
			ccjDetails = new List<DN13>();
			previousSearches = new List<DN17>();
			commonTerms = new List<CommonTerm>();
			scoreHistory = new List<Tuple<DateTime?, int?>>();
		}

		public void ParseAndStore(string xml, string refNumber, long serviceLogId, DateTime? insertDate = null)
		{
			Init();
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			ParseDN10();
			ParseDN11();
			ParseDN12();
			ParseDN13();
			ParseDN14();
			ParseDN17();
			ParseDN23();
			ParseDN26();
			ParseDN36();
			ParseDN40();
			ParseDN73();
			ParseErrors();

			SaveToDb(refNumber, serviceLogId, insertDate);
		}

		private void SaveToDb(string refNumber, long serviceLogId, DateTime? insertDate = null)
		{
			DataTable dt = db.ExecuteReader(
				"InsertNonLimitedResult",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber),
				new QueryParameter("ServiceLogId", serviceLogId),
				new QueryParameter("Created", insertDate.HasValue ? insertDate.Value : DateTime.UtcNow),
				new QueryParameter("BusinessName", businessName), 
				new QueryParameter("Address1", address1),
				new QueryParameter("Address2", address2),
				new QueryParameter("Address3", address3),
				new QueryParameter("Address4", address4),
				new QueryParameter("Address5", address5),
				new QueryParameter("Postcode", postcode),
				new QueryParameter("TelephoneNumber", telephoneNumber),
				new QueryParameter("PrincipalActivities", principalActivities),
				new QueryParameter("EarliestKnownDate", earliestKnownDate),
				new QueryParameter("DateOwnershipCommenced", dateOwnershipCommenced),
				new QueryParameter("IncorporationDate", incorporationDate),
				new QueryParameter("DateOwnershipCeased", dateOwnershipCeased),
				new QueryParameter("LastUpdateDate", lastUpdateDate),
				new QueryParameter("BankruptcyCountDuringOwnership", bankruptcyCountDuringOwnership),
				new QueryParameter("AgeOfMostRecentBankruptcyDuringOwnershipMonths", ageOfMostRecentBankruptcyDuringOwnershipMonths),
				new QueryParameter("AssociatedBankruptcyCountDuringOwnership", associatedBankruptcyCountDuringOwnership),
				new QueryParameter("AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths", ageOfMostRecentAssociatedBankruptcyDuringOwnershipMonths),
				new QueryParameter("AgeOfMostRecentJudgmentDuringOwnershipMonths", ageOfMostRecentJudgmentDuringOwnershipMonths),
				new QueryParameter("TotalJudgmentCountLast12Months", totalJudgmentCountLast12Months),
				new QueryParameter("TotalJudgmentValueLast12Months", totalJudgmentValueLast12Months),
				new QueryParameter("TotalJudgmentCountLast13To24Months", totalJudgmentCountLast13To24Months),
				new QueryParameter("TotalJudgmentValueLast13To24Months", totalJudgmentValueLast13To24Months),
				new QueryParameter("ValueOfMostRecentAssociatedJudgmentDuringOwnership", valueOfMostRecentAssociatedJudgmentDuringOwnership),
				new QueryParameter("TotalAssociatedJudgmentCountLast12Months", totalAssociatedJudgmentCountLast12Months),
				new QueryParameter("TotalAssociatedJudgmentValueLast12Months", totalAssociatedJudgmentValueLast12Months),
				new QueryParameter("TotalAssociatedJudgmentCountLast13To24Months", totalAssociatedJudgmentCountLast13To24Months),
				new QueryParameter("TotalAssociatedJudgmentValueLast13To24Months", totalAssociatedJudgmentValueLast13To24Months),
				new QueryParameter("TotalJudgmentCountLast24Months", totalJudgmentCountLast24Months),
				new QueryParameter("TotalAssociatedJudgmentCountLast24Months", totalAssociatedJudgmentCountLast24Months),
				new QueryParameter("TotalJudgmentValueLast24Months", totalJudgmentValueLast24Months),
				new QueryParameter("TotalAssociatedJudgmentValueLast24Months", totalAssociatedJudgmentValueLast24Months),
				new QueryParameter("SupplierName", supplierName),
				new QueryParameter("FraudCategory", fraudCategory),
				new QueryParameter("FraudCategoryDesc", fraudCategoryDesc),
				new QueryParameter("NumberOfAccountsPlacedForCollection", numberOfAccountsPlacedForCollection),
				new QueryParameter("ValueOfAccountsPlacedForCollection", valueOfAccountsPlacedForCollection),
				new QueryParameter("NumberOfAccountsPlacedForCollectionLast2Years", numberOfAccountsPlacedForCollectionLast2Years),
				new QueryParameter("AverageDaysBeyondTermsFor0To100", averageDaysBeyondTermsFor0To100),
				new QueryParameter("AverageDaysBeyondTermsFor101To1000", averageDaysBeyondTermsFor101To1000),
				new QueryParameter("AverageDaysBeyondTermsFor1001To10000", averageDaysBeyondTermsFor1001To10000),
				new QueryParameter("AverageDaysBeyondTermsForOver10000", averageDaysBeyondTermsForOver10000),
				new QueryParameter("AverageDaysBeyondTermsForLast3MonthsOfDataReturned", averageDaysBeyondTermsForLast3MonthsOfDataReturned),
				new QueryParameter("AverageDaysBeyondTermsForLast6MonthsOfDataReturned", averageDaysBeyondTermsForLast6MonthsOfDataReturned),
				new QueryParameter("AverageDaysBeyondTermsForLast12MonthsOfDataReturned", averageDaysBeyondTermsForLast12MonthsOfDataReturned),
				new QueryParameter("CurrentAverageDebt", currentAverageDebt),
				new QueryParameter("AverageDebtLast3Months", averageDebtLast3Months),
				new QueryParameter("AverageDebtLast12Months", averageDebtLast12Months),
				new QueryParameter("TelephoneNumberDN36", telephoneNumberDN36),
				new QueryParameter("RiskScore", riskScore),
				new QueryParameter("SearchType", searchType),
				new QueryParameter("SearchTypeDesc", searchTypeDesc),
				new QueryParameter("CommercialDelphiScore", commercialDelphiScore),
				new QueryParameter("CreditRating", creditRating),
				new QueryParameter("CreditLimit", creditLimit),
				new QueryParameter("ProbabilityOfDefaultScore", probabilityOfDefaultScore),
				new QueryParameter("StabilityOdds", stabilityOdds),
				new QueryParameter("RiskBand", riskBand),
				new QueryParameter("NumberOfProprietorsSearched", numberOfProprietorsSearched),
				new QueryParameter("NumberOfProprietorsFound", numberOfProprietorsFound),
				new QueryParameter("Errors", errors)
			);

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				int newId = sr["NewId"];

				foreach (Tuple<DateTime?, int?> historyData in scoreHistory)
				{
					db.ExecuteNonQuery(
						"InsertNonLimitedResultScoreHistory",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", newId),
						new QueryParameter("RiskScore", historyData.Item2),
						new QueryParameter("Date", historyData.Item1));
				}

				if (sicCodes1992.Count == sicDescs1992.Count)
				{
					for (int i = 0; i < sicCodes1992.Count; i++)
					{
						db.ExecuteNonQuery(
							"InsertNonLimitedResultSicCodes",
							CommandSpecies.StoredProcedure,
							new QueryParameter("ExperianNonLimitedResultId", newId),
							new QueryParameter("Code", sicCodes1992[i]),
							new QueryParameter("Description", sicDescs1992[i]));
					}
				}
				else
				{
					log.Alert("Illegal data parsed: num of sic codes and sic descriptions is not the same");
				}

				foreach (DN11 dn11 in bankruptcyDetails)
				{
					db.ExecuteNonQuery(
						"InsertNonLimitedResultBankruptcyDetails",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", newId),
						new QueryParameter("BankruptcyName", dn11.BankruptcyName),
						new QueryParameter("BankruptcyAddr1", dn11.BankruptcyAddr1),
						new QueryParameter("BankruptcyAddr2", dn11.BankruptcyAddr2),
						new QueryParameter("BankruptcyAddr3", dn11.BankruptcyAddr3),
						new QueryParameter("BankruptcyAddr4", dn11.BankruptcyAddr4),
						new QueryParameter("BankruptcyAddr5", dn11.BankruptcyAddr5),
						new QueryParameter("PostCode", dn11.PostCode),
						new QueryParameter("GazetteDate", dn11.GazetteDate),
						new QueryParameter("BankruptcyType", dn11.BankruptcyType),
						new QueryParameter("BankruptcyTypeDesc", dn11.BankruptcyTypeDesc)
					);
				}

				foreach (DN13 dn13 in ccjDetails)
				{
					DataTable ccjDetailsIdDataTable = db.ExecuteReader(
						"InsertNonLimitedResultCcjDetails",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", newId),
						new QueryParameter("RecordType", dn13.RecordType),
						new QueryParameter("RecordTypeFullName", dn13.RecordTypeFullName),
						new QueryParameter("JudgementDate", dn13.JudgementDate),
						new QueryParameter("SatisfactionFlag", dn13.SatisfactionFlag),
						new QueryParameter("SatisfactionFlagDesc", dn13.SatisfactionFlagDesc),
						new QueryParameter("SatisfactionDate", dn13.SatisfactionDate),
						new QueryParameter("JudgmentType", dn13.JudgmentType),
						new QueryParameter("JudgmentTypeDesc", dn13.JudgmentTypeDesc),
						new QueryParameter("JudgmentAmount", dn13.JudgmentAmount),
						new QueryParameter("Court", dn13.Court),
						new QueryParameter("CaseNumber", dn13.CaseNumber),
						new QueryParameter("NumberOfJudgmentNames", dn13.NumberOfJudgmentNames),
						new QueryParameter("NumberOfTradingNames", dn13.NumberOfTradingNames),
						new QueryParameter("LengthOfJudgmentName", dn13.LengthOfJudgmentName),
						new QueryParameter("LengthOfTradingName", dn13.LengthOfTradingName),
						new QueryParameter("LengthOfJudgmentAddress", dn13.LengthOfJudgmentAddress),
						new QueryParameter("JudgementAddr1", dn13.JudgementAddr1),
						new QueryParameter("JudgementAddr2", dn13.JudgementAddr2),
						new QueryParameter("JudgementAddr3", dn13.JudgementAddr3),
						new QueryParameter("JudgementAddr4", dn13.JudgementAddr4),
						new QueryParameter("JudgementAddr5", dn13.JudgementAddr5),
						new QueryParameter("PostCode", dn13.PostCode)
					);

					if (ccjDetailsIdDataTable.Rows.Count == 1)
					{
						var ccjDetailesSafeReader = new SafeReader(ccjDetailsIdDataTable.Rows[0]);
						int ccjDetailsNewId = ccjDetailesSafeReader["NewId"];
						foreach (string name in dn13.JudgmentRegisteredAgainst)
						{
							db.ExecuteNonQuery(
								"InsertNonLimitedResultCcjRegisteredAgainst",
								CommandSpecies.StoredProcedure,
								new QueryParameter("ExperianNonLimitedResultCcjDetailsId", ccjDetailsNewId),
								new QueryParameter("Name", name)
							);
						}

						foreach (TradingName tradingName in dn13.TradingNames)
						{
							db.ExecuteNonQuery(
								"InsertNonLimitedResultCcjTradingNames",
								CommandSpecies.StoredProcedure,
								new QueryParameter("ExperianNonLimitedResultCcjDetailsId", ccjDetailsNewId),
								new QueryParameter("Name", tradingName.Name),
								new QueryParameter("TradingIndicator", tradingName.TradingIndicator),
								new QueryParameter("TradingIndicatorDesc", tradingName.TradingIndicatorDesc)
							);
						}
					}
				}

				foreach (DN17 dn17 in previousSearches)
				{
					db.ExecuteNonQuery(
						"InsertNonLimitedResultPreviousSearches",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", newId),
						new QueryParameter("PreviousSearchDate", dn17.PreviousSearchDate),
						new QueryParameter("EnquiryType", dn17.EnquiryType),
						new QueryParameter("EnquiryTypeDesc", dn17.EnquiryTypeDesc),
						new QueryParameter("CreditRequired", dn17.CreditRequired)
					);
				}

				foreach (CommonTerm commonTerm in commonTerms)
				{
					db.ExecuteNonQuery(
						"InsertNonLimitedResultPaymentPerformanceDetails",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", newId),
						new QueryParameter("Code", commonTerm.Code),
						new QueryParameter("DaysBeyondTerms", commonTerm.DaysBeyondTerms)
					);
				}
			}
		}

		public int? GetScore()
		{
			return riskScore;
		}

		private void ParseErrors()
		{
			XmlNodeList errorMessagesNodes = xmlDoc.SelectNodes("//REQUEST/ERR1/MESSAGE");
			if (errorMessagesNodes != null)
			{
				foreach (XmlNode errorMessagesNode in errorMessagesNodes)
				{
					errors += errorMessagesNode.InnerText + Environment.NewLine;
				}
			}
		}

		private void ParseDN10()
		{
			XmlNodeList dn10Nodes = xmlDoc.SelectNodes("//DN10");
			if (dn10Nodes != null && dn10Nodes.Count == 1)
			{
				XmlNode dn10Node = dn10Nodes[0];
				businessName = GetString(dn10Node, "BUSINESSNAME");
				address1 = GetString(dn10Node, "BUSADDR1");
				address2 = GetString(dn10Node, "BUSADDR2");
				address3 = GetString(dn10Node, "BUSADDR3");
				address4 = GetString(dn10Node, "BUSADDR4");
				address5 = GetString(dn10Node, "BUSADDR5");
				postcode = GetString(dn10Node, "BUSPOSTCODE");
				telephoneNumber = GetString(dn10Node, "TELEPHONENUM");
				principalActivities = GetString(dn10Node, "PRINCIPALACTIVITIES");

				dateOwnershipCeased = GetDate(dn10Node, "DATEOWNSHPTERMD");
				lastUpdateDate = GetDate(dn10Node, "LATESTUPDATE");

				dateOwnershipCommenced = GetDate(dn10Node, "DATEOWNSHPCOMMD");
				earliestKnownDate = GetDate(dn10Node, "EARLIESTKNOWNDATE");
				incorporationDate = dateOwnershipCommenced ?? earliestKnownDate;

				XmlNodeList sicCodeNodes = dn10Node.SelectNodes("SICCODES");
				if (sicCodeNodes != null)
				{
					foreach (XmlNode sicCodeNode in sicCodeNodes)
					{
						XmlNode sicCode1992Node = sicCodeNode.SelectSingleNode("SICCODE1992");
						if (sicCode1992Node != null)
						{
							sicCodes1992.Add(sicCode1992Node.InnerText);
						}
					}
				}
				XmlNodeList sicDescNodes = dn10Node.SelectNodes("SICDESCS");
				if (sicDescNodes != null)
				{
					foreach (XmlNode sicDescNode in sicDescNodes)
					{
						XmlNode sicDesc1992Node = sicDescNode.SelectSingleNode("SICDESC1992");
						if (sicDesc1992Node != null)
						{
							sicDescs1992.Add(sicDesc1992Node.InnerText);
						}
					}
				}
			}
		}

		private void ParseDN11()
		{
			XmlNodeList dn11Nodes = xmlDoc.SelectNodes("//DN11");
			if (dn11Nodes != null)
			{
				foreach (XmlNode dn11Node in dn11Nodes)
				{
					var dn11 = new DN11();
					dn11.BankruptcyName = GetString(dn11Node, "BANKRUPTCYNAME");
					dn11.BankruptcyAddr1 = GetString(dn11Node, "BANKRUPTCYADDR1");
					dn11.BankruptcyAddr2 = GetString(dn11Node, "BANKRUPTCYADDR2");
					dn11.BankruptcyAddr3 = GetString(dn11Node, "BANKRUPTCYADDR3");
					dn11.BankruptcyAddr4 = GetString(dn11Node, "BANKRUPTCYADDR4");
					dn11.BankruptcyAddr5 = GetString(dn11Node, "BANKRUPTCYADDR5");
					dn11.PostCode = GetString(dn11Node, "BUSPOSTCODE");
					dn11.GazetteDate = GetDate(dn11Node, "GAZETTE");
					dn11.BankruptcyType = GetString(dn11Node, "BANKRUPTCYTYPE");
					switch (dn11.BankruptcyType)
					{
						case "BO":
							dn11.BankruptcyTypeDesc = "Bankruptcy Order";
							break;
						case "SEQ":
							dn11.BankruptcyTypeDesc = "Sequestration";
							break;
						case "OD":
							dn11.BankruptcyTypeDesc = "Order Of Discharge";
							break;
					}
					bankruptcyDetails.Add(dn11);
				}
			}
		}

		private void ParseDN12()
		{
			XmlNodeList dn12Nodes = xmlDoc.SelectNodes("//DN12");
			if (dn12Nodes != null && dn12Nodes.Count == 1)
			{
				XmlNode dn12Node = dn12Nodes[0];
				bankruptcyCountDuringOwnership = GetInt(dn12Node, "MBANKRUPTCYCOUNTOWNSHP");
				ageOfMostRecentBankruptcyDuringOwnershipMonths = GetInt(dn12Node, "MMOSTRECBANKRUPCY");
				associatedBankruptcyCountDuringOwnership = GetInt(dn12Node, "ABANKRUPTCYCOUNTOWNSHP");
				ageOfMostRecentAssociatedBankruptcyDuringOwnershipMonths = GetInt(dn12Node, "AMOSTRECBANKRUPCY"); // TODO: never exists in prod - remove it
			}
		}

		private void ParseDN13()
		{
			XmlNodeList dn13Nodes = xmlDoc.SelectNodes("//DN13");
			if (dn13Nodes != null)
			{
				foreach (XmlNode dn13Node in dn13Nodes)
				{
					var dn13 = new DN13();
					dn13.RecordType = GetString(dn13Node, "RECORDTYPE");
					switch (dn13.RecordType)
					{
						case "M":
							dn13.RecordTypeFullName = "Main";
							break;
						case "A":
							dn13.RecordTypeFullName = "Associated";
							break;
						case "P":
							dn13.RecordTypeFullName = "Pool";
							break;
					}
					dn13.JudgementDate = GetDate(dn13Node, "JUDGDATE");
					dn13.SatisfactionFlag = GetString(dn13Node, "SATFLAG");
					switch (dn13.SatisfactionFlag)
					{
						case "Y":
							dn13.SatisfactionFlagDesc = "Satisfied";
							break;
						case "N":
							dn13.SatisfactionFlagDesc = "Not satisfied";
							break;
					}
					dn13.SatisfactionDate = GetDate(dn13Node, "SATDATE");

					dn13.JudgmentType = GetString(dn13Node, "JUDGTYPE");
					switch (dn13.JudgmentType)
					{
						case "JG":
							dn13.JudgmentTypeDesc = "Judgment";
							break;
						case "SS":
							dn13.JudgmentTypeDesc = "Satisfied Judgment";
							break;
						case "DO":
							dn13.JudgmentTypeDesc = "Discovery Order";
							break;
						case "CU":
							dn13.JudgmentTypeDesc = "Certificate Of Unenforceability";
							break;
					}

					dn13.JudgmentAmount = GetInt(dn13Node, "JUDGAMOUNT");
					dn13.Court = GetString(dn13Node, "COURT");
					dn13.CaseNumber = GetString(dn13Node, "CASENUM");
					dn13.NumberOfJudgmentNames = GetString(dn13Node, "NUMJUDGNAMES");
					dn13.NumberOfTradingNames = GetString(dn13Node, "NUMTRADNAMES");
					dn13.LengthOfJudgmentName = GetString(dn13Node, "LENJUDGNAME");
					dn13.LengthOfTradingName = GetString(dn13Node, "LENTRADNAME");
					dn13.LengthOfJudgmentAddress = GetString(dn13Node, "LENJUDGADDR");
					dn13.LengthOfJudgmentAddress = GetString(dn13Node, "LENJUDGADDR");
					dn13.LengthOfJudgmentAddress = GetString(dn13Node, "LENJUDGADDR");
					dn13.LengthOfJudgmentAddress = GetString(dn13Node, "LENJUDGADDR");
					dn13.LengthOfJudgmentAddress = GetString(dn13Node, "LENJUDGADDR");
					dn13.LengthOfJudgmentAddress = GetString(dn13Node, "LENJUDGADDR");
					dn13.JudgementAddr1 = GetString(dn13Node, "JUDGADDR1");
					dn13.JudgementAddr2 = GetString(dn13Node, "JUDGADDR2");
					dn13.JudgementAddr3 = GetString(dn13Node, "JUDGADDR3");
					dn13.JudgementAddr4 = GetString(dn13Node, "JUDGADDR4");
					dn13.JudgementAddr5 = GetString(dn13Node, "JUDGADDR5");
					dn13.PostCode = GetString(dn13Node, "JUDGPOSTCODE");

					XmlNodeList judgmentRegisteredAgainstNodes = dn13Node.SelectNodes("JUDGDETS");
					if (judgmentRegisteredAgainstNodes != null)
					{
						foreach (XmlNode judgmentRegisteredAgainstNode in judgmentRegisteredAgainstNodes)
						{
							dn13.JudgmentRegisteredAgainst.Add(judgmentRegisteredAgainstNode.InnerText);
						}
					}
					XmlNodeList tradingNamesNodes = dn13Node.SelectNodes("TRADDETS");
					if (tradingNamesNodes != null)
					{
						foreach (XmlNode tradingNamesNode in tradingNamesNodes)
						{
							var tradingName = new TradingName();
							tradingName.Name = GetString(tradingNamesNode, "TRADNAME");
							tradingName.TradingIndicator = GetString(tradingNamesNode, "TRADINDICATOR");
							switch (tradingName.TradingIndicator)
							{
								case "F":
									tradingName.TradingIndicatorDesc = "Formerly trading as";
									break;
								case "P":
									tradingName.TradingIndicatorDesc = "Previously trading as";
									break;
								case "T":
									tradingName.TradingIndicatorDesc = "Trading as";
									break;
							}
							dn13.TradingNames.Add(tradingName);
						}
					}

					ccjDetails.Add(dn13);
				}
			}
		}

		private void ParseDN14()
		{
			XmlNodeList dn14Nodes = xmlDoc.SelectNodes("//DN14");
			if (dn14Nodes != null && dn14Nodes.Count == 1)
			{
				XmlNode dn14Node = dn14Nodes[0];
				ageOfMostRecentJudgmentDuringOwnershipMonths = GetInt(dn14Node, "MAGEMOSTRECJUDGSINCEOWNSHP");
				totalJudgmentCountLast12Months = GetInt(dn14Node, "MTOTJUDGCOUNTLST12MNTHS");
				totalJudgmentValueLast12Months = GetInt(dn14Node, "MTOTJUDGVALUELST12MNTHS");
				totalJudgmentCountLast13To24Months = GetInt(dn14Node, "MTOTJUDGCOUNTLST13TO24MNTHS");
				totalJudgmentValueLast13To24Months = GetInt(dn14Node, "MTOTJUDGVALUELST13TO24MNTHS");
				valueOfMostRecentAssociatedJudgmentDuringOwnership = GetInt(dn14Node, "AVALMOSTRECJUDGSINCEOWNSHP");
				totalAssociatedJudgmentCountLast12Months = GetInt(dn14Node, "ATOTJUDGCOUNTLST12MNTHS");
				totalAssociatedJudgmentValueLast12Months = GetInt(dn14Node, "ATOTJUDGVALUELST12MNTHS");
				totalAssociatedJudgmentCountLast13To24Months = GetInt(dn14Node, "ATOTJUDGCOUNTLST13TO24MNTHS");
				totalAssociatedJudgmentValueLast13To24Months = GetInt(dn14Node, "ATOTJUDGVALUELST13TO24MNTHS");
				totalJudgmentCountLast24Months = GetInt(dn14Node, "MTOTJUDGCOUNTLST24MNTHS");
				totalAssociatedJudgmentCountLast24Months = GetInt(dn14Node, "ATOTJUDGCOUNTLST24MNTHS");
				totalJudgmentValueLast24Months = GetInt(dn14Node, "MTOTJUDGVALUELST24MNTHS");
				totalAssociatedJudgmentValueLast24Months = GetInt(dn14Node, "ATOTJUDGVALUELST24MNTHS");
			}
		}

		private void ParseDN17()
		{
			XmlNodeList dn17Nodes = xmlDoc.SelectNodes("//DN17");
			if (dn17Nodes != null)
			{
				foreach (XmlNode dn17Node in dn17Nodes)
				{
					var dn17 = new DN17();
					dn17.PreviousSearchDate = GetDate(dn17Node, "PREVSEARCHDATE");
					dn17.EnquiryType = GetString(dn17Node, "ENQUIRYTYPE");
					switch (dn17.EnquiryType)
					{
						case "Z":
							dn17.EnquiryTypeDesc = "Business Confirmation";
							break;
						case "Y":
							dn17.EnquiryTypeDesc = "Business Profile";
							break;
						case "X":
							dn17.EnquiryTypeDesc = "Credit Profile";
							break;
						case "W":
							dn17.EnquiryTypeDesc = "Full Profile";
							break;
						case "D":
							dn17.EnquiryTypeDesc = "e-series Gold";
							break;
						case "E":
							dn17.EnquiryTypeDesc = "e-series Silver";
							break;
						case "F":
							dn17.EnquiryTypeDesc = "e-series Bronze";
							break;
						case "N":
							dn17.EnquiryTypeDesc = "Commercial Autoscore Application";
							break;
						case "O":
							dn17.EnquiryTypeDesc = "Commercial Autoscore Reprocess Application";
							break;
						case "6":
							dn17.EnquiryTypeDesc = "Written Report";
							break;
						case "C":
							dn17.EnquiryTypeDesc = "CPU Link Enquiry";
							break;
						case "Q":
							dn17.EnquiryTypeDesc = "Credit Card Report";
							break;
					}
					dn17.CreditRequired = GetString(dn17Node, "CREDITREQD");
					previousSearches.Add(dn17);
				}
			}
		}

		private void ParseDN23()
		{
			XmlNodeList dn23Nodes = xmlDoc.SelectNodes("//DN23");
			if (dn23Nodes != null && dn23Nodes.Count == 1)
			{
				XmlNode dn23Node = dn23Nodes[0];
				{
					fraudCategory = GetString(dn23Node, "FRAUDCATEGORY");
					switch (fraudCategory)
					{
						case "01":
							fraudCategoryDesc = "Providing a false name and a true address.";
							break;
						case "02":
							fraudCategoryDesc = "Providing or using the name and particulars of another person.";
							break;
						case "03":
							fraudCategoryDesc =
								"Providing or using a genuine name and address, but one or more material falsehoods in personal details followed by a serious misuse of the credit or other facility and/or non-payment.";
							break;
						case "04":
							fraudCategoryDesc =
								"Providing or using a genuine name and address, but one or more material falsehoods in personal details.";
							break;
						case "05":
							fraudCategoryDesc =
								"Disposal/selling on of goods obtained on credit and failing to settle the finance agreement.";
							break;
						case "06":
							fraudCategoryDesc = "Opening an account for the purpose of fraud.";
							break;
					}
					supplierName = GetString(dn23Node, "SUPPLIERNAME");
				}
			}
		}

		private void ParseDN26()
		{
			XmlNodeList dn26Nodes = xmlDoc.SelectNodes("//DN26");
			if (dn26Nodes != null && dn26Nodes.Count == 1)
			{
				XmlNode dn26Node = dn26Nodes[0];

				numberOfAccountsPlacedForCollection = GetInt(dn26Node, "NUMACCSPLACEDFORCOLLTN");
				valueOfAccountsPlacedForCollection = GetInt(dn26Node, "VALACCSPLACEDFORCOLLTN");
				numberOfAccountsPlacedForCollectionLast2Years = GetInt(dn26Node, "NUMACCSPLACEDFORCOLLTNLST2YRS");
				averageDaysBeyondTermsFor0To100 = GetInt(dn26Node, "AVDBT0-100");
				averageDaysBeyondTermsFor101To1000 = GetInt(dn26Node, "AVDBT101-1000");
				averageDaysBeyondTermsFor1001To10000 = GetInt(dn26Node, "AVDBT1001-10000");
				averageDaysBeyondTermsForOver10000 = GetInt(dn26Node, "AVDBTGREATERTHAN10000");
				averageDaysBeyondTermsForLast3MonthsOfDataReturned = GetInt(dn26Node, "AVDBTLST3MNTHSDATARTND");
				averageDaysBeyondTermsForLast6MonthsOfDataReturned = GetInt(dn26Node, "AVDBTLST6MNTHSDATARTND");
				averageDaysBeyondTermsForLast12MonthsOfDataReturned = GetInt(dn26Node, "AVDBTLST12MNTHSDATARTND");
				currentAverageDebt = GetInt(dn26Node, "CURRAVDEBT");
				averageDebtLast3Months = GetInt(dn26Node, "AVDEBTLST3MNTHS");
				averageDebtLast12Months = GetInt(dn26Node, "AVDEBTLST12MNTHS");

				XmlNodeList commonTermsNodes = dn26Node.SelectNodes("COMMONTERMS");
				if (commonTermsNodes != null)
				{
					foreach (XmlNode commonTermsNode in commonTermsNodes)
					{
						var commonTerm = new CommonTerm();
						commonTerm.Code = GetString(commonTermsNode, "COMMONTERMSCODE");
						commonTerm.DaysBeyondTerms = GetInt(commonTermsNode, "COMMONTERMSDBT");
						commonTerms.Add(commonTerm);
					}
				}
			}
		}

		private void ParseDN36()
		{
			XmlNodeList dn36Nodes = xmlDoc.SelectNodes("//DN36");
			if (dn36Nodes != null && dn36Nodes.Count == 1)
			{
				XmlNode dn36Node = dn36Nodes[0];
				telephoneNumberDN36 = GetString(dn36Node, "TELEPHONENUM");
			}
		}

		private void ParseDN40()
		{
			XmlNodeList dn40Nodes = xmlDoc.SelectNodes("//DN40");
			if (dn40Nodes != null && dn40Nodes.Count == 1)
			{
				XmlNode dn40Node = dn40Nodes[0];
				riskScore = GetInt(dn40Node, "RISKSCORE");

				XmlNodeList scoreHistoryNodes = dn40Node.SelectNodes("SCOREHISTORY");

				if (scoreHistoryNodes != null)
				{
					foreach (XmlNode scoreHistoryNode in scoreHistoryNodes)
					{
						DateTime? scoreHistoryDate = GetDate(scoreHistoryNode, "SCOREHISTORY_DATE");
						int? riskScoreHistory = GetInt(scoreHistoryNode, "SCOREHISTORY_RISKSCORE");
						if (scoreHistoryDate.HasValue)
						{
							scoreHistory.Add(new Tuple<DateTime?, int?>(scoreHistoryDate, riskScoreHistory));
						}
					}
				}
			}
		}

		private void ParseDN73()
		{
			XmlNodeList dn73Nodes = xmlDoc.SelectNodes("//DN73");
			if (dn73Nodes != null && dn73Nodes.Count == 1)
			{
				XmlNode dn73Node = dn73Nodes[0];
				searchType = GetString(dn73Node, "SEARCHTYPE");
				switch (searchType)
				{
					case "P":
						searchTypeDesc = "Proprietor only";
						break;
					case "B":
						searchTypeDesc = "Business only";
						break;
					case "J":
						searchTypeDesc = "Business & proprietor";
						break;
				}
				commercialDelphiScore = GetInt(dn73Node, "NLCDSCORE");
				creditRating = GetString(dn73Node, "CREDITRATING");
				creditLimit = GetString(dn73Node, "CREDITLIMIT");
				probabilityOfDefaultScore = GetDecimal(dn73Node, "PDSCORE");
				stabilityOdds = GetString(dn73Node, "STABILITYODDS");
				riskBand = GetString(dn73Node, "RISKBAND");
				numberOfProprietorsSearched = GetInt(dn73Node, "NUMPROPSSEARCHED");
				numberOfProprietorsFound = GetInt(dn73Node, "NUMPROPSFOUND");
			}
		}

		private DateTime? GetDate(XmlNode node, string tag)
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
					dayNode.InnerText.Trim().PadLeft(2, '0')
				);

				DateTime result;

				if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					return result;
				}
			}

			return null;
		}

		private int? GetInt(XmlNode element, string nodeName)
		{
			XmlNode node = element.SelectSingleNode(nodeName);

			if (node != null)
			{
				int result;
				if (int.TryParse(node.InnerText, out result))
				{
					return result;
				}

				return null;
			}

			return null;
		}

		private decimal? GetDecimal(XmlNode element, string nodeName)
		{
			XmlNode node = element.SelectSingleNode(nodeName);

			if (node != null)
			{
				decimal result;
				if (decimal.TryParse(node.InnerText, out result))
				{
					return result;
				}
				
				return null;
			}

			return null;
		}

		private string GetString(XmlNode element, string nodeName)
		{
			XmlNode node = element.SelectSingleNode(nodeName);

			if (node == null)
			{
				return null;
			}

			return node.InnerText;
		}

		private class DN11
		{
			public string BankruptcyName { get; set; }
			public string BankruptcyAddr1 { get; set; }
			public string BankruptcyAddr2 { get; set; }
			public string BankruptcyAddr3 { get; set; }
			public string BankruptcyAddr4 { get; set; }
			public string BankruptcyAddr5 { get; set; }
			public string PostCode { get; set; }
			public DateTime? GazetteDate { get; set; }
			public string BankruptcyType { get; set; }
			public string BankruptcyTypeDesc { get; set; }
		}

		private class DN17
		{
			public DateTime? PreviousSearchDate { get; set; }
			public string EnquiryType { get; set; }
			public string EnquiryTypeDesc { get; set; }
			public string CreditRequired { get; set; }
		}

		private class DN13
		{
			public DN13()
			{
				JudgmentRegisteredAgainst = new List<string>();
				TradingNames = new List<TradingName>();
			}

			public string RecordType { get; set; }
			public string RecordTypeFullName { get; set; }
			public DateTime? JudgementDate { get; set; }
			public string SatisfactionFlag { get; set; }
			public string SatisfactionFlagDesc { get; set; }
			public DateTime? SatisfactionDate { get; set; }
			public string JudgmentType { get; set; }
			public string JudgmentTypeDesc { get; set; }
			public int? JudgmentAmount { get; set; }
			public string Court { get; set; }
			public string CaseNumber { get; set; }
			public string NumberOfJudgmentNames { get; set; }
			public string NumberOfTradingNames { get; set; }
			public string LengthOfJudgmentName { get; set; }
			public string LengthOfTradingName { get; set; }
			public string LengthOfJudgmentAddress { get; set; }
			public string JudgementAddr1 { get; set; }
			public string JudgementAddr2 { get; set; }
			public string JudgementAddr3 { get; set; }
			public string JudgementAddr4 { get; set; }
			public string JudgementAddr5 { get; set; }
			public string PostCode { get; set; }
			public List<string> JudgmentRegisteredAgainst { get; set; }
			public List<TradingName> TradingNames { get; set; }
		}

		private class TradingName
		{
			public string Name { get; set; }
			public string TradingIndicator { get; set; }
			public string TradingIndicatorDesc { get; set; }
		}

		private class CommonTerm
		{
			public string Code { get; set; }
			public int? DaysBeyondTerms { get; set; }
		}
	}
}
