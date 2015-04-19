namespace CallCreditLib {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Xml.Linq;
	using System.Xml.Serialization;
	using Callcredit.CRBSB;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;
	
	

	public partial class CallCreditModelBuilder {

		// to ConfigurationVariables db table
		private static string password = "7UM9AXH2";
		private static string userName = "Ezbob SR API CTEST";
		private static string companyName = "Ezbob SR CTEST";

		CallcreditBsbAndCreditReport apiProxy ;
		UserInfo user ;
		private CT_SearchDefinition apiSD;
		private string Errors { get; set; }
		private bool HasParsingError { get; set; }

		public CallCreditModelBuilder() {
			try {
				apiProxy = InitializeApiProxy();
				user = InitializeUser();
				apiSD = InitializeApiRequest(user);

				Console.WriteLine(apiProxy.Url);
				Console.WriteLine(apiSD);

				//CT_SearchResult apiresult = new CT_SearchResult();
				//apiresult = apiProxy.Search07a(apiSD);
				//var builder = new CallCreditModelBuilder();
				//builder.Build(apiresult);
				//Console.WriteLine("====" + apiresult.creditrequest.applicant.Length);
				//Console.WriteLine("====" + apiresult.creditrequest.applicant[0].dob);

			} catch (Exception e) {
				Console.WriteLine(e);
				//throw;
			}
		}

		public CallCredit GetSearch07a() {

			Console.WriteLine("ssss");

			CT_SearchResult apiresult = new CT_SearchResult();
			apiresult = apiProxy.Search07a(apiSD);
			//XmlSerializer serializer = new XmlSerializer(typeof(CT_SearchResult));
			//TextWriter writer = new StreamWriter(@"C:\temp1\Xml.xml");
			//serializer.Serialize(writer, apiresult);
			CallCredit xx  = this.Build(apiresult);


			return xx;
		}


		
		public CallCredit Build(CT_SearchResult response, int? customerId = null, int? directorId = null, DateTime? insertDate = null, long serviceLogId = 1) {

			/*Console.WriteLine(response.token);
			return null;*/


			var result = new CallCredit {
				ApplicantData = new List<CallCreditData>(),
				Amendments = new List<CallCreditAmendments>(),
				ApplicantAddresses = new List<CallCreditApplicantAddresses>(),
				ApplicantNames = new List<CallCreditApplicantNames>(),
				Email = new List<CallCreditEmail>(),
				Telephone = new List<CallCreditTelephone>(),

				MP_ServiceLogId = serviceLogId,
				CustomerId = customerId,
				DirectorId = directorId,
				InsertDate =  insertDate ?? DateTime.UtcNow
			};
			

			if (response == null || response.creditreport == null) {
				result.HasCallCreditError = true;
				result.Error = "Creditreport is null";
				return result;
			}

			//report attributes
			TryRead(() => result.LinkType = response.creditreport.linktype, "report link type");
			TryRead(() => result.ReportSearchID = response.creditreport.searchid, "report search ID");

			//response set
			TryRead(() => result.PayLoadData = XSerializer.Serialize(response.payload), "Client's own data");
			TryRead(() => result.YourReference = response.yourreference, "User reference as part of search definition");
			TryRead(() => result.Token = response.token, "Client's own data");
			//request details as part of response
			TryRead(() => result.SchemaVersionCR = response.creditrequest.schemaversion, "The version of the schema for credit request");
			TryRead(() => result.DataSetsCR = (int)response.creditrequest.datasets, "Search datasets required for credit request, values 0 to 511");
			TryRead(() => result.Score = Convert.ToBoolean(response.creditrequest.score), "Score request check");
			TryRead(() => result.Purpose = response.creditrequest.purpose, "Credit request search purpose");
			TryRead(() => result.CreditType = response.creditrequest.credittype, "Credit type");
			TryRead(() => result.BalorLim = (int)response.creditrequest.balorlim, "Balance or credit limit applied for");
			TryRead(() => result.Term = response.creditrequest.term, "Term of loan applied for");
			TryRead(() => result.Transient = Convert.ToBoolean(response.creditrequest.transient), "financial (transient) association check");
			TryRead(() => result.AutoSearch = Convert.ToBoolean(response.creditrequest.autosearch), "auto-searching of undeclared addresses check");
			TryRead(() => result.AutoSearchMaximum = (int)response.creditrequest.autosearchmaximum, "Maximum number of addresses to auto-search");
			//jobdetails
			TryRead(() => result.UniqueSearchID = response.jobdetails.searchid, "Unique identifier for Credit Reports");
			TryRead(() => result.CastInfo = response.jobdetails.cast, "CastInfo - Callcredit System Specific Information");
			TryRead(() => result.PSTV = response.jobdetails.pstv, "PSTV - Callcredit System Specific Information");
			TryRead(() => result.LS = response.jobdetails.ls, "LS - Callcredit System Specific Information");
			TryRead(() => result.SearchDate = response.jobdetails.searchdate, "Date and time that Search was carried out ");
			//linkrequest
			TryRead(() => result.SchemaVersionLR = response.linkrequest.schemaversion, "The version of the schema for link request");
			TryRead(() => result.DataSetsLR = (int)response.linkrequest.datasets, "Search datasets required for link request, values 0 to 511");
			TryRead(() => result.OrigSrchLRID = response.linkrequest.origsrchid, "Original report searchid");
			TryRead(() => result.NavLinkID = response.linkrequest.navlinkid, "Link navigation identifier of the link to be followed (either Address or Associate Link)");
			//subsequent equest
			TryRead(() => result.SchemaVersionSR = response.secondaryrequest.schemaversion, "The version of the schema for subsequent request");
			TryRead(() => result.DataSetsLR = (int)response.secondaryrequest.datasets, "Search datasets required for subsequent request, values 0 to 511");
			TryRead(() => result.OrigSrchSRID = response.secondaryrequest.origsrchid, "Original report searchid");

			//request applicant details as part of response
			TryRead(() => result.Dob = response.creditrequest.applicant[0].dob, "Applicant's date Of birth");
			TryRead(() => result.Hho = Convert.ToBoolean(response.creditrequest.applicant[0].hho), "Household override check");
			TryRead(() => result.TpOptOut = Convert.ToBoolean(response.creditrequest.applicant[0].tpoptout), "Third party data opt out check");
			
			//applicant demographics
			//person
			TryRead(() => result.CustomerStatus = response.creditrequest.applicant[0].applicantdemographics.person.customerstatus, "Customer Applicant Status Code ");
			TryRead(() => result.MaritalStatus = response.creditrequest.applicant[0].applicantdemographics.person.maritalstatus, "Applicant's Marital Status Code");
			TryRead(() => result.TotalDependents = (int)response.creditrequest.applicant[0].applicantdemographics.person.totaldependents, "Total number of dependent children");
			TryRead(() => result.LanguageVerbal = response.creditrequest.applicant[0].applicantdemographics.person.language, "Language Code");
			//Identity
			TryRead(() => result.Type1 = response.creditrequest.applicant[0].applicantdemographics.person.identity[0].type, "Identity Type Code 1");
			TryRead(() => result.Type2 = response.creditrequest.applicant[0].applicantdemographics.person.identity[1].type, "Identity Type Code 2");
			TryRead(() => result.Type3 = response.creditrequest.applicant[0].applicantdemographics.person.identity[2].type, "Identity Type Code 3");
			TryRead(() => result.Type4 = response.creditrequest.applicant[0].applicantdemographics.person.identity[3].type, "Identity Type Code 4");
			//Accommodation
			TryRead(() => result.AccommodationType = response.creditrequest.applicant[0].applicantdemographics.accommodation.type, "Accommodation Type Code");
			TryRead(() => result.PropertyValue = (int)response.creditrequest.applicant[0].applicantdemographics.accommodation.propertyvalue, "Estimate of how much the property is worth");
			TryRead(() => result.MortgageBalance = (int)response.creditrequest.applicant[0].applicantdemographics.accommodation.mortgagebalance, "The total amount of the mortgage still to be repaid");
			TryRead(() => result.MonthlyRental = (int)response.creditrequest.applicant[0].applicantdemographics.accommodation.monthlyrental, "The price that the applicant pays the home's owner for using the home");
			TryRead(() => result.ResidentialStatus = response.creditrequest.applicant[0].applicantdemographics.accommodation.residentialstatus, "Residential Status Code");
			//employment
			TryRead(() => result.Occupation = response.creditrequest.applicant[0].applicantdemographics.employment.occupation, "Occupation Code");
			TryRead(() => result.EmploymentStatus = response.creditrequest.applicant[0].applicantdemographics.employment.employmentstatus, "Employment Status Code");
			TryRead(() => result.ExpiryDate = response.creditrequest.applicant[0].applicantdemographics.employment.expirydate, "Employment Expiry Data - dependent on the Employment Status Code");
			TryRead(() => result.EmploymentRecency = response.creditrequest.applicant[0].applicantdemographics.employment.employmentrecency, "Employment Recency Code");
			TryRead(() => result.EmployerCategory = response.creditrequest.applicant[0].applicantdemographics.employment.employercategory, "Employer Category Code");
			TryRead(() => result.TimeAtCurrentEmployer = response.creditrequest.applicant[0].applicantdemographics.employment.timeatcurrentemployer, "Total number of months with the current employer");
			//account
			TryRead(() => result.SortCode = response.creditrequest.applicant[0].applicantdemographics.account.sortcode, "Bank sort code of main banking relationship");
			TryRead(() => result.AccountNumber = response.creditrequest.applicant[0].applicantdemographics.account.accountnumber, "Account number of main banking relationship");
			TryRead(() => result.TimeAtBank = response.creditrequest.applicant[0].applicantdemographics.account.timeatbank, "Total number of months with the current bank");
			TryRead(() => result.PaymentMethod = response.creditrequest.applicant[0].applicantdemographics.account.paymentmethod, "Account's Payment Method Code");
			TryRead(() => result.FinanceType = response.creditrequest.applicant[0].applicantdemographics.account.financetype, "Finance/Non Finance Type Code");
			//expenditure
			TryRead(() => result.TotalDebitCards = (int)response.creditrequest.applicant[0].applicantdemographics.expenditure.totaldebitcards, "Total number of cheque of debit cards linked directly to the customer's bank account");
			TryRead(() => result.TotalCreditCards = (int)response.creditrequest.applicant[0].applicantdemographics.expenditure.totalcreditcards, "Total number of credit or charge cards used to purchase goods and services on credit");
			TryRead(() => result.MonthlyUnsecuredAmount = (int)response.creditrequest.applicant[0].applicantdemographics.expenditure.monthlyunsecuredamount, "Monthly unsecured financial obligation owed to creditors");
			//main incom details
			TryRead(() => result.AmountPr = (int)response.creditrequest.applicant[0].applicantdemographics.income.primary.amount, "Amount of main income received according to frequency");
			TryRead(() => result.TypePr = response.creditrequest.applicant[0].applicantdemographics.income.primary.type, "Main income Type Code");
			TryRead(() => result.PaymentMethodPr = response.creditrequest.applicant[0].applicantdemographics.income.primary.paymentmethod, "Main income Payment Method Code");
			TryRead(() => result.FrequencyPr = response.creditrequest.applicant[0].applicantdemographics.income.primary.frequency, "Main income Frequency Code");
			//any additional incom details
			TryRead(() => result.AmountAd = (int)response.creditrequest.applicant[0].applicantdemographics.income.additional.amount, "Amount of additional income received according to frequency");
			TryRead(() => result.TypeAd = response.creditrequest.applicant[0].applicantdemographics.income.additional.type, "Additional income Type Code");
			TryRead(() => result.PaymentMethodAd = response.creditrequest.applicant[0].applicantdemographics.income.additional.paymentmethod, "Additional income Payment Method Code");
			TryRead(() => result.FrequencyAd = response.creditrequest.applicant[0].applicantdemographics.income.additional.frequency, "Additional income Frequency Code");


			//mapping path for the main applicant
			CT_creditreportApplicant app = response.creditreport.applicant[0];
			//mapping path for the associate(s)
			CT_creditreportApplicantOias oias = response.creditreport.applicant[0].oias;

			//number of associates intialized
			var oiastotal = 0;

			if (response.creditreport.applicant[0].oias != null) {
				oiastotal = (int)response.creditreport.applicant[0].oias.total;
			}

			for (int i = 0; i <= oiastotal; ++i) {
				result.ApplicantData = (i == 0) ? BuildData(app, i) : BuildData(oias.oia[i], i);
			}

			result.Amendments = GetAmendments(response.secondaryrequest);
			//result.ApplicantAddresses = GetApplicantAddresses(response.creditrequest);
			result.ApplicantNames = GetApplicantNames(response.creditrequest);
			result.Email = GetEmail(response.creditrequest.applicant[0].applicantdemographics);
			result.Telephone = GetTelephone(response.creditrequest.applicant[0].applicantdemographics);
			result.ApplicantData[0].Tpd = GetTpd(app.tpd);
			result.Error = "";//Errors;
			result.HasParsingError = HasParsingError;
			
			return result; //Object with searchdata

		}

		public List<CallCreditData> BuildData(CT_outputapplicant report, int oiaid) {

			var Applicants = new List<CallCreditData>();

			var data = new CallCreditData {
				Accs = new List<CallCreditDataAccs>(),
				AddressConfs = new List<CallCreditDataAddressConfs>(),
				SummaryAddresses = new List<CallCreditDataAddresses>(),
				AddressLinks = new List<CallCreditDataAddressLinks>(),
				AliasLinks = new List<CallCreditDataAliasLinks>(),
				AssociateLinks = new List<CallCreditDataAssociateLinks>(),
				CifasFiling = new List<CallCreditDataCifasFiling>(),
				CifasPlusCases = new List<CallCreditDataCifasPlusCases>(),
				CreditScores = new List<CallCreditDataCreditScores>(),
				Judgments = new List<CallCreditDataJudgments>(),
				LinkAddresses = new List<CallCreditDataLinkAddresses>(),
				Nocs = new List<CallCreditDataNocs>(),
				Rtr = new List<CallCreditDataRtr>(),
				Searches = new List<CallCreditDataSearches>(),
				Tpd = new List<CallCreditDataTpd>(),
				OiaID = oiaid
			};

			//applicant attributes
			TryRead(() => data.ReportType = report.reporttype, "report type");
			TryRead(() => data.TpOptOut = Convert.ToBoolean(report.tpoptout), "3rd party data check");
			TryRead(() => data.AutoSearchMaxExceeded = Convert.ToBoolean(report.autosearchmaxexceeded), "address links limit check");
			TryRead(() => data.AgeFlag = (int)(report.ageflag), "report age flag");
			TryRead(() => data.ReporTitle = report.reporttitle, "report title");

			//applicant summary set
			//summary bais(bankruptcy and insolvency)
			TryRead(() => data.CurrentInsolvment = Convert.ToBoolean(report.summary.bais.currentlyinsolvent), "currently insolvent check");
			TryRead(() => data.Restricted = Convert.ToBoolean(report.summary.bais.restricted), "restriction check");
			TryRead(() => data.TotalDischarged = (int)report.summary.bais.totaldischarged, "restriction check");
			//summary bds(behavioral data)
			TryRead(() => data.TotalMinPayments12Month = (int)report.summary.bds.totalminpayments12months, "min pays last 12 months");
			TryRead(() => data.TotalMinPayments36Month = (int)report.summary.bds.totalminpayments36months, "min pays last 36 months");
			TryRead(() => data.TotalValueCashAdvances12Month = (int)report.summary.bds.totalvaluecashadvances12months, "Cash last 12 months");
			TryRead(() => data.TotalValueCashAdvances36Month = (int)report.summary.bds.totalvaluecashadvances36months, "Cash last 36 months");
			//Summary cifas
			TryRead(() => data.TotalCifas = (int)report.summary.cifas.totalcifas, "Cash last 36 months");
			//Summary ich(Impaired Credit History)
			TryRead(() => data.ImpairedCredit = Convert.ToBoolean(report.summary.ich.impairedcredit), "ich detected check");
			TryRead(() => data.Secured = Convert.ToBoolean(report.summary.ich.secured), "mortgage acc paid 3 to 24 months check");
			TryRead(() => data.Unsecured = Convert.ToBoolean(report.summary.ich.unsecured), "loan acc paid 3 to 24 months check");
			TryRead(() => data.Judgment = Convert.ToBoolean(report.summary.ich.judgment), "min 500 gbps judgment check");
			TryRead(() => data.Iva = Convert.ToBoolean(report.summary.ich.iva), " Individual Voluntary Arrangement check");
			TryRead(() => data.Boss = Convert.ToBoolean(report.summary.ich.boss), "Bankruptcy Order or Scottish Sequestration check");
			//Summary indebtedness of applicant
			TryRead(() => data.BalanceLimitRatioVolve = (long)report.summary.indebt.balancelimitratiorevolve, "% ratio of balance to limits for all account group 3 Accounts");
			TryRead(() => data.TotalBalancesActive = (long)report.summary.indebt.totalbalancesactive, "Total balances for all active Accounts");
			TryRead(() => data.TotalBalancesLoans = (long)report.summary.indebt.totalbalancesloans, "Total balances for all account group 1 accounts");
			TryRead(() => data.TotalBalancesMortgage = (long)report.summary.indebt.totalbalancesmortgages, "Total balances for all account group 2 accounts (Mortgage Accounts)");
			TryRead(() => data.TotalBalancesRevolve = (long)report.summary.indebt.totalbalancesrevolve, "Total balances for all account group 3 Accounts");
			TryRead(() => data.TotalLimitsRevolve = (long)report.summary.indebt.totallimitsrevolve, "Total limits for all active account group 3 Accounts");
			//Summary judgments
			TryRead(() => data.Total = (int)(report.summary.judgments.total), "Total number of Judgments");
			TryRead(() => data.TotalActive = (int)(report.summary.judgments.totalactive), "Total number of active Judgments");
			TryRead(() => data.Total36m = (int)(report.summary.judgments.total36m), "Total number of Judgments in last 3 years");
			TryRead(() => data.TotalSatisfied = (int)(report.summary.judgments.totalsatisfied), "Total number of satisfied Judgments");
			TryRead(() => data.TotalActiveAmount = (int)(report.summary.judgments.totalactiveamount), "Total amount of active Judgments");
			TryRead(() => data.TotalSatisfiedAmount = (int)(report.summary.judgments.totalsatisfiedamount), "Total amount of satisfied Judgments");
			//Summary links
			TryRead(() => data.TotalUndecAddresses = (int)(report.summary.links.totalundecaddresses), " total number of undeclared Address Links");
			TryRead(() => data.TotalUndecAddressesSearched = (int)(report.summary.links.totalundecaddressessearched), " total number of undeclared searched Address Links");
			TryRead(() => data.TotalUndecAddressesUnsearched = (int)(report.summary.links.totalundecaddressesunsearched), " total number of undeclared unsearched Address Links");
			TryRead(() => data.TotalUndecAliases = (int)(report.summary.links.totalundecaliases), " total number of undeclared Alias Links");
			TryRead(() => data.TotalUndecAssociates = (int)(report.summary.links.totalundecassociates), " total number of undeclared Associate Links");
			//summary rtr (Real Time Reporting)
			TryRead(() => data.HasUpdates = Convert.ToBoolean(report.summary.rtr.hasupdates), "MODA data check");
			//summary searches
			TryRead(() => data.TotalHomeCreditSearches3Months = (int)report.summary.searches.totalhomecreditsearches3months, "home credit searches over last 3 months");
			TryRead(() => data.TotalSearches3Months = (int)report.summary.searches.totalsearches3months, "total searches over last 3 months");
			TryRead(() => data.TotalSearches12Months = (int)report.summary.searches.totalsearches12months, "total searches over last 3 months");
			//summary shared accounts
			TryRead(() => data.TotalAccounts = (int)report.summary.share.totalaccounts, "total share accs");
			TryRead(() => data.TotalActiveAccs = (int)report.summary.share.totalactiveaccs, "total active share accs");
			TryRead(() => data.TotalSettledAccs = (int)report.summary.share.totalsettledaccs, "total settled share accs");
			TryRead(() => data.TotalOpened6Month = (int)report.summary.share.totalopened6months, "total share accs opened 6 months");
			TryRead(() => data.WorstPayStatus12Month = report.summary.share.worsepaystatus12months, "worst payment status last 12 months");
			TryRead(() => data.WorstPayStatus36Month = report.summary.share.worsepaystatus36months, "worst payment status last 36 months");
			TryRead(() => data.TotalDelinqs12Month = (int)report.summary.share.totaldelinqs12months, "delinquent share accs last 12 months");
			TryRead(() => data.TotalDefaults12Month = (int)report.summary.share.totaldefaults12months, "total share accs defalted last 12 months");
			TryRead(() => data.TotalDefaults36Month = (int)report.summary.share.totaldefaults36months, "total share accs defalted last 36 months");
			//summary address
			TryRead(() => data.MessageCode = (int)report.summary.summaryaddress.messagecode, "total share accs defalted last 36 months");
			TryRead(() => data.PafValid = Convert.ToBoolean(report.summary.summaryaddress.pafvalid), "Postcode Address File check");
			TryRead(() => data.RollingRoll = Convert.ToBoolean(report.summary.summaryaddress.rollingroll), "electoral roll check");
			//summary tpd (3rd party decisions)
			TryRead(() => data.AlertDecision = report.summary.tpd.alertdecision, "leve of alert decision data ");
			TryRead(() => data.AlertReview = report.summary.tpd.alertreview, "leve of alert review data ");
			TryRead(() => data.Hho = report.summary.tpd.hho, "leve of Household Override data ");
			//summary notices
			TryRead(() => data.NocFlag = Convert.ToBoolean(report.summary.notices.nocflag), "notice check");
			TryRead(() => data.TotalDisputes = (int)report.summary.notices.totaldisputes, "total notice of disputes");

			//demographics set
			TryRead(() => data.CameoUk = report.demographics.cameouk, "CAMEO UK Code");
			TryRead(() => data.CameoInvestor = report.demographics.cameoinvestor, "CAMEO Investor UK Code");
			TryRead(() => data.CameoIncome = report.demographics.cameoincome, "CAMEO Income Code");
			TryRead(() => data.CameoUnemployment = report.demographics.cameounemployment, "CAMEO Unemployment Code");
			TryRead(() => data.CameoProperty = report.demographics.cameoproperty, "CAMEO Property Code");
			TryRead(() => data.CameoFinance = report.demographics.cameofinance, "CAMEO Finance Code");
			//demographics family
			TryRead(() => data.CameoUkFam = report.demographics.family.cameoukfam, "Percentage of single adult households");
			TryRead(() => data.Ind_adult1 = Convert.ToInt32(report.demographics.family.ind_adult1), "Single Adult Index");
			//demographics household
			TryRead(() => data.Adult_1 = Convert.ToInt32(report.demographics.household.adult_1), "Percentage of single adult households");
			TryRead(() => data.Adults_2 = Convert.ToInt32(report.demographics.household.adults_2), "Percentage of two adult households");
			TryRead(() => data.Adult_3pl = Convert.ToInt32(report.demographics.household.adult_1), "Percentage of three or more adult households");
			//demographics age
			TryRead(() => data.Age0_17 = Convert.ToInt32(report.demographics.age.age0_17), "Age 0-17 index");
			TryRead(() => data.Age18_24 = Convert.ToInt32(report.demographics.age.age18_24), "Age 18-24 index");
			TryRead(() => data.Age25_34 = Convert.ToInt32(report.demographics.age.age25_34), "Age 25-34 index");
			TryRead(() => data.Age35_44 = Convert.ToInt32(report.demographics.age.age35_44), "Age 35-44 index");
			TryRead(() => data.Age45_54 = Convert.ToInt32(report.demographics.age.age45_54), "Age 45-54 index");
			TryRead(() => data.Age55_64 = Convert.ToInt32(report.demographics.age.age55_64), "Age 55-64 index");
			TryRead(() => data.Age65_74 = Convert.ToInt32(report.demographics.age.age65_74), "Age 65-74 index");
			TryRead(() => data.Age75pl = Convert.ToInt32(report.demographics.age.age75pl), "Age 75 plus index");
			//demographics economic
			TryRead(() => data.Unem_prob = (float)report.demographics.economic.unem_prob, "Average percentage unemployed");
			TryRead(() => data.Unem_index = Convert.ToInt32(report.demographics.economic.unem_index), "Index of economic inactivity");
			//demographics economicactivity
			TryRead(() => data.Wk_fem_ind = Convert.ToInt32(report.demographics.economicactivity.wkfem_ind), "Working females indices");
			TryRead(() => data.Stu_ind = Convert.ToInt32(report.demographics.economicactivity.stu_ind), "Students indices");
			TryRead(() => data.Sick_ind = Convert.ToInt32(report.demographics.economicactivity.sick_ind), "Residents with long-term illness indices");
			TryRead(() => data.Degree_ind = Convert.ToInt32(report.demographics.economicactivity.degree_ind), "Residents with degrees indices");
			//demographics socialclass
			TryRead(() => data.Ab_ind = Convert.ToInt32(report.demographics.socialclass.ab_ind), "Social Class AB Indices");
			TryRead(() => data.C1_ind = Convert.ToInt32(report.demographics.socialclass.c1_ind), "Social Class C1 Indices");
			TryRead(() => data.C2_ind = Convert.ToInt32(report.demographics.socialclass.c2_ind), "Social Class C2 Indices");
			TryRead(() => data.De_ind = Convert.ToInt32(report.demographics.socialclass.de_ind), "Social Class DE Indices");
			//demographics housing
			TryRead(() => data.Cameoukhsg = report.demographics.housing.cameoukhsg, "Housing Type descriptor ");
			TryRead(() => data.Cameoukten = report.demographics.housing.cameoukten, "Housing Tenure descriptor ");
			TryRead(() => data.Natprice = Convert.ToInt32(report.demographics.housing.natprice), "Index of property price against national average");
			TryRead(() => data.Regprice = Convert.ToInt32(report.demographics.housing.regprice), "Index of property price against regional average");
			//demographics propertyprice
			TryRead(() => data.D_index = Convert.ToInt32(report.demographics.propertyprice.d_index), "Index of detached price against national average");
			TryRead(() => data.D_r_index = Convert.ToInt32(report.demographics.propertyprice.d_r_index), "Index of detached price against regional average");
			TryRead(() => data.S_index = Convert.ToInt32(report.demographics.propertyprice.s_index), "Index of semi-detached price against national average");
			TryRead(() => data.S_r_index = Convert.ToInt32(report.demographics.propertyprice.s_r_index), "Index of semi-detached price against regional average");
			TryRead(() => data.T_index = Convert.ToInt32(report.demographics.propertyprice.t_index), "Index of terrace price against national average");
			TryRead(() => data.T_r_index = Convert.ToInt32(report.demographics.propertyprice.t_r_index), "Index of terrace price against regional average");
			TryRead(() => data.F_index = Convert.ToInt32(report.demographics.propertyprice.f_index), "Index of flat price against national average");
			TryRead(() => data.F_r_index = Convert.ToInt32(report.demographics.propertyprice.f_r_index), "Index of flat price against regional average");
			//demographics movement
			TryRead(() => data.Unem_prob = (float)report.demographics.movement.l_of_res, "Average length of residency");
			TryRead(() => data.Unem_index = Convert.ToInt32(report.demographics.movement.move_rate), "Movement rate (and %) from Electoral Roll historical activity");

			//demographics2006 set
			TryRead(() => data.CameoUk06 = report.demographics2006.cameouk, "D2006 CAMEO UK Code");
			TryRead(() => data.CameoUkg06 = report.demographics2006.cameoukg, "D2006 CAMEO UK Group");
			TryRead(() => data.CameoIncome06 = report.demographics2006.cameoincome, "D2006 Income Category");
			TryRead(() => data.CameoIncg06 = report.demographics2006.cameoincg, "D2006 Income Group");
			TryRead(() => data.CameoInvestor06 = report.demographics2006.cameoinvestor, "D2006 Investor");
			TryRead(() => data.CameoInvg06 = report.demographics2006.cameoinvg, "D2006 Investor group");
			TryRead(() => data.CameoProperty06 = report.demographics2006.cameoproperty, "D2006 Property");
			TryRead(() => data.CameoFinance06 = report.demographics2006.cameofinance, "D2006 Finance Code");
			TryRead(() => data.CameoFing06 = report.demographics2006.cameofing, "D2006 Finance Group");
			TryRead(() => data.CameoUnemploy06 = report.demographics2006.cameounemploy, "D2006 Unemployment Code");
			//demographics2006 age
			TryRead(() => data.AgeScore = (float)report.demographics2006.age.agescore, "D2006 Age score");
			TryRead(() => data.AgeBand = (int)report.demographics2006.age.ageband, "D2006 Age band, values 1-20");
			//demographics2006 tenure
			TryRead(() => data.TenureScore = (float)report.demographics2006.tenure.tenrscore, "D2006 tenure score");
			TryRead(() => data.TenureBand = (int)report.demographics2006.tenure.tenrband, "D2006 tenure band, values 1-20");
			//demographics2006 household composition
			TryRead(() => data.CompScore = (float)report.demographics2006.hhcomp.compscore, "D2006 Household composition score");
			TryRead(() => data.CompBand = (int)report.demographics2006.hhcomp.compband, "D2006 Household composition band, values 1-20");
			//demographics2006 economic
			TryRead(() => data.EconScore = (float)report.demographics2006.economic.econscore, "D2006 Economic activity score");
			TryRead(() => data.EconBand = (int)report.demographics2006.economic.econband, "D2006 Economic activity band, values 1-20");
			//demographics2006 lifestage
			TryRead(() => data.LifeScore = (float)report.demographics2006.lifestage.lifescore, "D2006 Lifestage score");
			TryRead(() => data.LifeBand = (int)report.demographics2006.lifestage.lifeband, "D2006 Lifestage band, values 1-20");
			//demographics2006 social
			TryRead(() => data.Dirhhld = (float)report.demographics2006.social.dirhhld, "D2006 Proportion of ‘Millionaire club’ households in the postcode");
			TryRead(() => data.Millhhld = (float)report.demographics2006.social.millhhld, "D2006 Proportion of company director households in the postcode ");
			TryRead(() => data.SocScore = (float)report.demographics2006.social.socscore, "D2006 Social class score");
			TryRead(() => data.SocBand = (int)report.demographics2006.social.socband, "D2006 Social class band, values 1-20");
			//demographics2006 occupation
			TryRead(() => data.OccScore = (float)report.demographics2006.occupation.occscore, "D2006 occupation score");
			TryRead(() => data.OccBand = (int)report.demographics2006.occupation.occband, "D2006 occupation band, values 1-20");
			//demographics2006 lifestage
			TryRead(() => data.MortScore = (float)report.demographics2006.mortgage.mortscore, "D2006 Mortgage and house size  score");
			TryRead(() => data.MortBand = (int)report.demographics2006.mortgage.mortband, "D2006 Mortgage and house size  band, values 1-20");
			//demographics2006 shareholding
			TryRead(() => data.HhldShare = (float)report.demographics2006.sharehld.hhldshare, "D2006 Proportion of households with shares in the postcode");
			TryRead(() => data.AvNumHold = (float)report.demographics2006.sharehld.avnumhold, "D2006 Average number of shareholders per share holding household");
			TryRead(() => data.AvNumShares = (float)report.demographics2006.sharehld.avnumshares, "D2006 Average number of shares per share holding household");
			TryRead(() => data.AvNumComps = (float)report.demographics2006.sharehld.avnumcomps, "D2006 Average number of companies invested in per share holding household ");
			TryRead(() => data.AvValShares = (float)report.demographics2006.sharehld.avvalshares, "D2006 Average values of shares per share holding household");
			//demographics2006 unemployment
			TryRead(() => data.UnemMalelt = (float)report.demographics2006.unemployment.unemmalelt, "D2006 Long term male unemployment");
			TryRead(() => data.Unem1824 = (float)report.demographics2006.unemployment.unem1824, "D2006 Unemployment among 18 to 24 year olds");
			TryRead(() => data.Unem2539 = (float)report.demographics2006.unemployment.unem2539, "D2006 Unemployment among 25 to 39 year olds ");
			TryRead(() => data.Unem40pl = (float)report.demographics2006.unemployment.unem40pl, "D2006 Unemployment among those aged 40 and over");
			TryRead(() => data.UnemScore = (float)report.demographics2006.unemployment.unemscore, "D2006 Unemployment score");
			TryRead(() => data.UnemBal = (int)report.demographics2006.social.socband, "D2006 Unemployment band, values 1 to 20");
			//demographics2006 unemployment rate
			TryRead(() => data.UnemRate = (float)report.demographics2006.unemprate.unemrate, "D2006 Unemployment Rate");
			TryRead(() => data.UnemDiff = (float)report.demographics2006.unemprate.unemdiff, "D2006 Unemployment Rate difference");
			TryRead(() => data.UnemInd = (int)report.demographics2006.unemprate.unemind, "D2006 Rate Index (against national average),");
			TryRead(() => data.Unemall = (float)report.demographics2006.unemprate.unemall, "D2006 Overall Unemployment Rating");
			TryRead(() => data.UnemallIndex = (int)report.demographics2006.unemprate.unemallindex, "D2006 Overall Unemployment Rating Index");
			//demographics2006 property
			TryRead(() => data.HousAge = report.demographics2006.property.houseage, "D2006 Average House Age");
			TryRead(() => data.HhldDensity = (float)report.demographics2006.property.hhlddensity, "D2006 Household Density");
			TryRead(() => data.CtaxBand = report.demographics2006.property.ctaxband, "D2006 Council Tax Band");
			TryRead(() => data.LocationType = (int)report.demographics2006.property.locationtype, "D2006 Postcode Location Type, values 1 to 5");
			TryRead(() => data.NatAvgHouse = (int)report.demographics2006.property.natavghouse, "D2006 National Average House Price, value 0 to 99999999");
			TryRead(() => data.HouseScore = (float)report.demographics2006.property.housescore, "D2006Housing Type Score");
			TryRead(() => data.HouseBand = (int)report.demographics2006.property.houseband, "D2006 Housing Type Band, value 1 to 20");
			TryRead(() => data.PriceDiff = (long)report.demographics2006.property.pricediff, "D2006 National Average House Price Difference, values -9999999 to 99999999");
			TryRead(() => data.PriceIndex = (int)report.demographics2006.property.priceindex, "D2006 House Price Index, values 0 to 99999");
			TryRead(() => data.Activity = (int)report.demographics2006.property.activity, "D2006 Level of Sales Activity Index, values 0 to 9999");
			TryRead(() => data.RegionalBand = (int)report.demographics2006.property.regionalband, "D2006 Regional Banded House Price, values 1 to 10 ");
			TryRead(() => data.AvgDetVal = (int)report.demographics2006.property.avgdetvalue, "D2006 Average Detached Property, values 0 to 9999999");
			TryRead(() => data.AvgDetIndex = (int)report.demographics2006.property.avgdetindex, "D2006 Detached Property Index, values 0 to 9999");
			TryRead(() => data.AvgSemiVal = (int)report.demographics2006.property.avgsemivalue, "D2006 Average Semi-Detached Property Value, values 0 to 9999999");
			TryRead(() => data.AvgSemiIndex = (int)report.demographics2006.property.avgsemiindex, "D2006 Semi-Detached Property Index, values 0 to 9999");
			TryRead(() => data.AvgTerrVal = (int)report.demographics2006.property.avgterrvalue, "D2006 Average Terraced Property Value, values 0 to 9999999");
			TryRead(() => data.AvgTerrIndex = (int)report.demographics2006.property.avgterrindex, "D2006 Terraced Property Index, values 0 to 9999");
			TryRead(() => data.AvgFlatVal = (int)report.demographics2006.property.avgflatvalue, "D2006 Average Flat Property Value, values 0 to 9999999");
			TryRead(() => data.AvgFlatIndex = (int)report.demographics2006.property.avgflatindex, "D2006 Flat Property Index, values 0 to 9999");
			TryRead(() => data.RegionCode = (int)report.demographics2006.property.regioncode, "D2006 Standard Region Code, values 1 to 17");
			//demographics2006 international
			TryRead(() => data.CameoIntl = report.demographics2006.international.cameointl, "D2006 CAMEO International Code");

			Applicants.Add(data);

			data.Accs = GetAccs(report, oiaid);
			data.AddressConfs = GetAddressConfs(report);
			data.SummaryAddresses = GetSummaryAddresses(report.summary);
			data.AddressLinks = GetAddressLinks(report.addresslinks);
			data.AliasLinks = GetAliasLinks(report);
			data.AssociateLinks = GetAssociateLinks(report);
			data.CifasFiling = GetCifasFiling(report.cifas);
			data.CifasPlusCases = GetCifasPlusCases(report.cifas);
			data.CreditScores = GetCreditScores(report);
			data.Judgments = GetJudgments(report);
			data.LinkAddresses = GetLinkAddresses(report.addresslinks);
			data.Nocs = GetNocs(report);
			data.Rtr = GetRtr(report);
			data.Searches = GetSearches(report);
			

			return Applicants;
		}

		public List<CallCreditAmendments> GetAmendments(CT_amendsubsequent amendmentsubreq) {
			var Amends = new List<CallCreditAmendments>();
			TryRead(() => {
				foreach (var AmNd in amendmentsubreq.amendments) {
					var amendments = new CallCreditAmendments();

					var amend = AmNd;
					TryRead(() => amendments.AmendmentName = amend.amendmentname, "Name of node that the amendment applies to");
					TryRead(() => amendments.AmendmentType = amend.amendmenttype, "Type of amendment –update or insert");
					TryRead(() => amendments.Balorlim = (int)amend.balorlim, "Change to balance or credit limit ");
					TryRead(() => amendments.Term = amend.term, "Change to term of loan");
					TryRead(() => amendments.AbodeNo = amend.address.abodeno, "Abode name or number");
					TryRead(() => amendments.BuildingNo = amend.address.buildingno, "Building Number");
					TryRead(() => amendments.BuildingName = amend.address.buildingname, "Building Name");
					TryRead(() => amendments.Street1 = amend.address.street1, "Street1");
					TryRead(() => amendments.Street2 = amend.address.street2, "Street2");
					TryRead(() => amendments.Sublocality = amend.address.sublocality, "Sublocality");
					TryRead(() => amendments.Locality = amend.address.locality, "District or Locality");
					TryRead(() => amendments.PostTown = amend.address.posttown, "Town");
					TryRead(() => amendments.PostCode = amend.address.postcode, "Postcode");
					TryRead(() => amendments.StartDate = amend.address.startdate, "The date the applicant moved into the Residence ");
					TryRead(() => amendments.EndDate = amend.address.enddate, "The date the applicant moved out of the Residence ");
					TryRead(() => amendments.Duration = amend.address.duration, "Duration of residency");
					TryRead(() => amendments.Title = amend.name.title, "Title");
					TryRead(() => amendments.Forename = amend.name.forename, "Forename");
					TryRead(() => amendments.OtherNames = amend.name.othernames, "Middle names / initials ");
					TryRead(() => amendments.SurName = amend.name.surname, "Surname");
					TryRead(() => amendments.Suffix = amend.name.suffix, "Suffix");
					Amends.Add(amendments);
				}
			}, "Amendments");

			return Amends;
		}
		
		public List<CallCreditApplicantAddresses> GetApplicantAddresses(CT_searchrequest request) {
			var Applicantddresses = new List<CallCreditApplicantAddresses>();
			TryRead(() => {
				foreach (var ApD in request.applicant[0].address) {
					var appaddresses = new CallCreditApplicantAddresses();

					var appadd = ApD;
					TryRead(() => appaddresses.AbodeNo = appadd.abodeno, "Abode name or number");
					TryRead(() => appaddresses.BuildingNo = appadd.buildingno, "Building Number");
					TryRead(() => appaddresses.BuildingName = appadd.buildingname, "Building Name");
					TryRead(() => appaddresses.Street1 = appadd.street1, "Street1");
					TryRead(() => appaddresses.Street2 = appadd.street2, "Street2");
					TryRead(() => appaddresses.SubLocality = appadd.sublocality, "Sublocality");
					TryRead(() => appaddresses.Locality = appadd.locality, "District or Locality");
					TryRead(() => appaddresses.PostTown = appadd.posttown, "Town");
					TryRead(() => appaddresses.PostCode = appadd.postcode, "Postcode");
					TryRead(() => appaddresses.StartDate = appadd.startdate, "The date the applicant moved into the Residence ");
					TryRead(() => appaddresses.EndDate = appadd.enddate, "The date the applicant moved out of the Residence ");
					TryRead(() => appaddresses.Duration = appadd.duration, "Duration of residency");
					Applicantddresses.Add(appaddresses);
				}
			}, "Applicant Addresses");

			return Applicantddresses;
		}


		public List<CallCreditApplicantNames> GetApplicantNames(CT_searchrequest request) {
			var ApplicantNames = new List<CallCreditApplicantNames>();
			TryRead(() => {
				foreach (var ApN in request.applicant[0].name) {
					var appnames = new CallCreditApplicantNames();

					var apname = ApN;

					TryRead(() => appnames.Title = apname.title, "Title");
					TryRead(() => appnames.Forename = apname.forename, "Forename");
					TryRead(() => appnames.OtherNames = apname.othernames, "Middle names / initials ");
					TryRead(() => appnames.Surname = apname.surname, "Surname");
					TryRead(() => appnames.Suffix = apname.suffix, "Suffix");
					ApplicantNames.Add(appnames);
				}
			}, "Applicant Names");

			return ApplicantNames;
		}

		
		private DateTime? TryReadDate(Func<DateTime> a, string key, bool isRequired = true) {
			try {
				DateTime d = a();
				return (d < DbSmallestDate) ? (DateTime?)null : d;
			} catch {
				if (isRequired) {
					HasParsingError = true;
					Errors += "Can not read value for: " + key + Environment.NewLine;
				} // if

				return null;
			} // try
		} // TryReadDate

		private static readonly DateTime DbSmallestDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private void TryRead(Action a, string key, bool isRequired = true) {
			try {
				a();
			} catch {
				if (isRequired) {
					HasParsingError = true;
					Errors += "Can not read value for: " + key + Environment.NewLine;
				}
			}
		}


		private static CallcreditBsbAndCreditReport InitializeApiProxy() {
			/* Create a new proxy object which represents the Callcredit API. */
			CallcreditBsbAndCreditReport apiProxy = new CallcreditBsbAndCreditReport();

			/* We can alter the proxy URL here, if necessary. */
			/* TODO: Select Appropriate URL - either Client Test Site or Live Site */
			//apiProxy.Url = "https://www.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx";	//Live Site URL
			apiProxy.Url = "https://ct.callcreditsecure.co.uk/Services/BSB/CRBSB7.asmx";	//Client Test Site URL

			/* Create a new callcreditheaders object and attach it to the proxy object. */
			/* TODO: Setup User Credentials (provided by Callcredit Professional Services) */
			callcreditheaders apiCredentials = new callcreditheaders();
			apiCredentials.company = companyName;
			apiCredentials.username = userName;
			apiCredentials.password = password;
			apiProxy.callcreditheadersValue = apiCredentials;

			return apiProxy;
		}

		private static UserInfo InitializeUser() {
			UserInfo user = new UserInfo();

			/*user.dob = new DateTime(1910, 01, 01);
			user.title = "MISS";
			user.forename = "JULIA";
			user.othernames = "";
			user.surname = "AUDI";
			user.buildingno = "1";
			user.street = "TOP GEAR LANE";
			user.postcode = "X9 9LF";*/

			user.dob = new DateTime(1960, 11, 05);
			user.title = "MR";
			user.forename = "OSCAR";
			user.othernames = "TEST-PERSON";
			user.surname = "MANX";
			user.buildingno = "606";
			user.street = "ALLEY CAT LANE";
			user.postcode = "X9 9AA";

			return user;
		}

		private static CT_SearchDefinition InitializeApiRequest(UserInfo user) {
			CT_SearchDefinition searchDef = new CT_SearchDefinition();

			CT_searchrequest srequest = new CT_searchrequest();

			srequest.purpose = "DS";
			srequest.score = 1;
			srequest.scoreSpecified = true;
			srequest.transient = 0;
			srequest.transientSpecified = true;
			srequest.schemaversion = "7.2";
			srequest.datasets = 511;
			//srequest.credittype = this.cboCreditType.SelectedValue.ToString();
			searchDef.creditrequest = srequest;

			/* Create a new request applicant object and attach it to the credit request object. */
			CT_searchapplicant apiApplicant = new CT_searchapplicant();
			apiApplicant.dob = user.dob;
			apiApplicant.dobSpecified = true;

			srequest.applicant = new CT_searchapplicant[] { apiApplicant };

			/* Create a new name object and attach it to the request applicant object. */
			CT_inputname apiName = new CT_inputname();
			apiName.title = user.title;
			apiName.forename = user.forename;
			apiName.othernames = user.othernames;
			apiName.surname = user.surname;

			apiApplicant.name = new CT_inputname[] { apiName };

			/* Create a new input current address object */
			CT_inputaddress apiInputCurrentAddress = new CT_inputaddress();
			apiInputCurrentAddress.buildingno = user.buildingno;
			apiInputCurrentAddress.street1 = user.street;
			apiInputCurrentAddress.postcode = user.postcode;

			apiApplicant.address = new CT_inputaddress[] { apiInputCurrentAddress };

			return searchDef;
		}
	
	}
}
