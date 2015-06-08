namespace CallCreditLib {
	using System;
	using System.Collections.Generic;
	using Callcredit.CRBSB;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;
	
	public partial class CallCreditModelBuilder {


		private string Errors { get; set; }
		private bool HasParsingError { get; set; }

		
		public CallCredit Build(CT_SearchResult response, int? customerId, int? directorId, DateTime? insertDate, long serviceLogId) {

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
			TryRead(() => result.LinkType = response.creditreport.linktype, "report link type", false);
			TryRead(() => result.ReportSearchID = response.creditreport.searchid, "report search ID", false);

			//response set
			TryRead(() => result.PayLoadData = XSerializer.Serialize(response.payload), "Client's own data", false);
			TryRead(() => result.YourReference = response.yourreference, "User reference as part of search definition", false);
			TryRead(() => result.Token = response.token, "Client's own data", false);
			//request details as part of response
			TryRead(() => result.SchemaVersionCR = response.creditrequest.schemaversion, "The version of the schema for credit request", false);
			TryRead(() => result.DataSetsCR = (int)response.creditrequest.datasets, "Search datasets required for credit request, values 0 to 511", false);
			TryRead(() => result.Score = Convert.ToBoolean(response.creditrequest.score), "Score request check");
			TryRead(() => result.Purpose = response.creditrequest.purpose, "Credit request search purpose");
			TryRead(() => result.CreditType = response.creditrequest.credittype, "Credit type");
			TryRead(() => result.BalorLim = (int)response.creditrequest.balorlim, "Balance or credit limit applied for", false);
			TryRead(() => result.Term = response.creditrequest.term, "Term of loan applied for", false);
			TryRead(() => result.Transient = Convert.ToBoolean(response.creditrequest.transient), "financial (transient) association check", false);
			TryRead(() => result.AutoSearch = Convert.ToBoolean(response.creditrequest.autosearch), "auto-searching of undeclared addresses check", false);
			TryRead(() => result.AutoSearchMaximum = (int)response.creditrequest.autosearchmaximum, "Maximum number of addresses to auto-search", false);
			//jobdetails
			TryRead(() => result.UniqueSearchID = response.jobdetails.searchid, "Unique identifier for Credit Reports", false);
			TryRead(() => result.CastInfo = response.jobdetails.cast, "CastInfo - Callcredit System Specific Information", false);
			TryRead(() => result.PSTV = response.jobdetails.pstv, "PSTV - Callcredit System Specific Information", false);
			TryRead(() => result.LS = response.jobdetails.ls, "LS - Callcredit System Specific Information", false);
			result.SearchDate = TryReadDate(() => response.jobdetails.searchdate, "Date and time that Search was carried out ", false);
			//linkrequest
			TryRead(() => result.SchemaVersionLR = response.linkrequest.schemaversion, "The version of the schema for link request", false);
			TryRead(() => result.DataSetsLR = (int)response.linkrequest.datasets, "Search datasets required for link request, values 0 to 511", false);
			TryRead(() => result.OrigSrchLRID = response.linkrequest.origsrchid, "Original report searchid", false);
			TryRead(() => result.NavLinkID = response.linkrequest.navlinkid, "Link navigation identifier of the link to be followed (either Address or Associate Link)", false);
			//subsequent equest
			TryRead(() => result.SchemaVersionSR = response.secondaryrequest.schemaversion, "The version of the schema for subsequent request", false);
			TryRead(() => result.DataSetsLR = (int)response.secondaryrequest.datasets, "Search datasets required for subsequent request, values 0 to 511", false);
			TryRead(() => result.OrigSrchSRID = response.secondaryrequest.origsrchid, "Original report searchid", false);

			//request applicant details as part of response
			result.Dob = TryReadDate(() => response.creditrequest.applicant[0].dob, "Applicant's date Of birth");
			TryRead(() => result.Hho = Convert.ToBoolean(response.creditrequest.applicant[0].hho), "Household override check", false);
			TryRead(() => result.TpOptOut = Convert.ToBoolean(response.creditrequest.applicant[0].tpoptout), "Third party data opt out check");
			
			//applicant demographics
			//person
			TryRead(() => result.CustomerStatus = response.creditrequest.applicant[0].applicantdemographics.person.customerstatus, "Customer Applicant Status Code ", false);
			TryRead(() => result.MaritalStatus = response.creditrequest.applicant[0].applicantdemographics.person.maritalstatus, "Applicant's Marital Status Code", false);
			TryRead(() => result.TotalDependents = (int)response.creditrequest.applicant[0].applicantdemographics.person.totaldependents, "Total number of dependent children", false);
			TryRead(() => result.LanguageVerbal = response.creditrequest.applicant[0].applicantdemographics.person.language, "Language Code", false);
			//Identity
			TryRead(() => result.Type1 = response.creditrequest.applicant[0].applicantdemographics.person.identity[0].type, "Identity Type Code 1", false);
			TryRead(() => result.Type2 = response.creditrequest.applicant[0].applicantdemographics.person.identity[1].type, "Identity Type Code 2", false);
			TryRead(() => result.Type3 = response.creditrequest.applicant[0].applicantdemographics.person.identity[2].type, "Identity Type Code 3", false);
			TryRead(() => result.Type4 = response.creditrequest.applicant[0].applicantdemographics.person.identity[3].type, "Identity Type Code 4", false);
			//Accommodation
			TryRead(() => result.AccommodationType = response.creditrequest.applicant[0].applicantdemographics.accommodation.type, "Accommodation Type Code", false);
			TryRead(() => result.PropertyValue = (int)response.creditrequest.applicant[0].applicantdemographics.accommodation.propertyvalue, "Estimate of how much the property is worth", false);
			TryRead(() => result.MortgageBalance = (int)response.creditrequest.applicant[0].applicantdemographics.accommodation.mortgagebalance, "The total amount of the mortgage still to be repaid", false);
			TryRead(() => result.MonthlyRental = (int)response.creditrequest.applicant[0].applicantdemographics.accommodation.monthlyrental, "The price that the applicant pays the home's owner for using the home", false);
			TryRead(() => result.ResidentialStatus = response.creditrequest.applicant[0].applicantdemographics.accommodation.residentialstatus, "Residential Status Code", false);
			//employment
			TryRead(() => result.Occupation = response.creditrequest.applicant[0].applicantdemographics.employment.occupation, "Occupation Code", false);
			TryRead(() => result.EmploymentStatus = response.creditrequest.applicant[0].applicantdemographics.employment.employmentstatus, "Employment Status Code", false);
			result.ExpiryDate = TryReadDate(() => response.creditrequest.applicant[0].applicantdemographics.employment.expirydate, "Employment Expiry Data - dependent on the Employment Status Code", false);
			TryRead(() => result.EmploymentRecency = response.creditrequest.applicant[0].applicantdemographics.employment.employmentrecency, "Employment Recency Code", false);
			TryRead(() => result.EmployerCategory = response.creditrequest.applicant[0].applicantdemographics.employment.employercategory, "Employer Category Code", false);
			TryRead(() => result.TimeAtCurrentEmployer = response.creditrequest.applicant[0].applicantdemographics.employment.timeatcurrentemployer, "Total number of months with the current employer", false);
			//account
			TryRead(() => result.SortCode = response.creditrequest.applicant[0].applicantdemographics.account.sortcode, "Bank sort code of main banking relationship", false);
			TryRead(() => result.AccountNumber = response.creditrequest.applicant[0].applicantdemographics.account.accountnumber, "Account number of main banking relationship", false);
			TryRead(() => result.TimeAtBank = response.creditrequest.applicant[0].applicantdemographics.account.timeatbank, "Total number of months with the current bank", false);
			TryRead(() => result.PaymentMethod = response.creditrequest.applicant[0].applicantdemographics.account.paymentmethod, "Account's Payment Method Code", false);
			TryRead(() => result.FinanceType = response.creditrequest.applicant[0].applicantdemographics.account.financetype, "Finance/Non Finance Type Code", false);
			//expenditure
			TryRead(() => result.TotalDebitCards = (int)response.creditrequest.applicant[0].applicantdemographics.expenditure.totaldebitcards, "Total number of cheque of debit cards linked directly to the customer's bank account", false);
			TryRead(() => result.TotalCreditCards = (int)response.creditrequest.applicant[0].applicantdemographics.expenditure.totalcreditcards, "Total number of credit or charge cards used to purchase goods and services on credit", false);
			TryRead(() => result.MonthlyUnsecuredAmount = (int)response.creditrequest.applicant[0].applicantdemographics.expenditure.monthlyunsecuredamount, "Monthly unsecured financial obligation owed to creditors", false);
			//main incom details
			TryRead(() => result.AmountPr = (int)response.creditrequest.applicant[0].applicantdemographics.income.primary.amount, "Amount of main income received according to frequency", false);
			TryRead(() => result.TypePr = response.creditrequest.applicant[0].applicantdemographics.income.primary.type, "Main income Type Code", false);
			TryRead(() => result.PaymentMethodPr = response.creditrequest.applicant[0].applicantdemographics.income.primary.paymentmethod, "Main income Payment Method Code", false);
			TryRead(() => result.FrequencyPr = response.creditrequest.applicant[0].applicantdemographics.income.primary.frequency, "Main income Frequency Code", false);
			//any additional incom details
			TryRead(() => result.AmountAd = (int)response.creditrequest.applicant[0].applicantdemographics.income.additional.amount, "Amount of additional income received according to frequency", false);
			TryRead(() => result.TypeAd = response.creditrequest.applicant[0].applicantdemographics.income.additional.type, "Additional income Type Code", false);
			TryRead(() => result.PaymentMethodAd = response.creditrequest.applicant[0].applicantdemographics.income.additional.paymentmethod, "Additional income Payment Method Code", false);
			TryRead(() => result.FrequencyAd = response.creditrequest.applicant[0].applicantdemographics.income.additional.frequency, "Additional income Frequency Code", false);
			
			CT_creditreport creditreport = response.creditreport;
			
			result.ApplicantData = BuildData(creditreport);
			
			result.Amendments = GetAmendments(response.secondaryrequest);
			result.ApplicantAddresses = GetApplicantAddresses(response.creditrequest);
			result.ApplicantNames = GetApplicantNames(response.creditrequest);
			result.Email = GetEmail(response.creditrequest.applicant[0].applicantdemographics);
			result.Telephone = GetTelephone(response.creditrequest.applicant[0].applicantdemographics);
			result.ApplicantData[0].Tpd = GetTpd(creditreport.applicant[0].tpd);

			const int maxErrorLength = 4000;
			result.Error = (!string.IsNullOrEmpty(Errors) && Errors.Length > maxErrorLength) ? Errors.Substring(0, maxErrorLength) : Errors;
			
			result.HasParsingError = HasParsingError;
			
			return result; //Object with searchdata

		}

		public List<CallCreditData> BuildData(CT_creditreport rep) {

			var Applicants = new List<CallCreditData>();

			//mapping path for the main applicant
			CT_creditreportApplicant app = rep.applicant[0];

			//mapping path for the associate(s)
			CT_creditreportApplicantOias oias = rep.applicant[0].oias;

			//number of associates intialized
			var oiastotal = 0;

			//number of associates assigned
			if (app.oias != null) {
				oiastotal = (int)app.oias.total;
			}

			for (int i = 0; i <= oiastotal; ++i) {

				CT_outputapplicant report;

				if (i == 0) 
					report = app;
				else
					report = oias.oia[i - 1];

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
					OiaID = i
				};

				//applicant attributes
				TryRead(() => data.ReportType = report.reporttype, "report type");
				TryRead(() => data.TpOptOut = Convert.ToBoolean(report.tpoptout), "3rd party data check");
				TryRead(() => data.AutoSearchMaxExceeded = Convert.ToBoolean(report.autosearchmaxexceeded), "address links limit check", false);
				TryRead(() => data.AgeFlag = (int)(report.ageflag), "report age flag", false);
				TryRead(() => data.ReporTitle = report.reporttitle, "report title", false);

				//applicant summary set
				//summary bais(bankruptcy and insolvency)
				TryRead(() => data.CurrentInsolvment = Convert.ToBoolean(report.summary.bais.currentlyinsolvent), "currently insolvent check", false);
				TryRead(() => data.Restricted = Convert.ToBoolean(report.summary.bais.restricted), "restriction check", false);
				TryRead(() => data.TotalDischarged = (int)report.summary.bais.totaldischarged, "restriction check", false);
				//summary bds(behavioral data)
				TryRead(() => data.TotalMinPayments12Month = (int)report.summary.bds.totalminpayments12months, "min pays last 12 months", false);
				TryRead(() => data.TotalMinPayments36Month = (int)report.summary.bds.totalminpayments36months, "min pays last 36 months", false);
				TryRead(() => data.TotalValueCashAdvances12Month = (int)report.summary.bds.totalvaluecashadvances12months, "Cash last 12 months", false);
				TryRead(() => data.TotalValueCashAdvances36Month = (int)report.summary.bds.totalvaluecashadvances36months, "Cash last 36 months", false);
				//Summary cifas
				TryRead(() => data.TotalCifas = (int)report.summary.cifas.totalcifas, "Cash last 36 months", false);
				//Summary ich(Impaired Credit History)
				TryRead(() => data.ImpairedCredit = Convert.ToBoolean(report.summary.ich.impairedcredit), "ich detected check", false);
				TryRead(() => data.Secured = Convert.ToBoolean(report.summary.ich.secured), "mortgage acc paid 3 to 24 months check", false);
				TryRead(() => data.Unsecured = Convert.ToBoolean(report.summary.ich.unsecured), "loan acc paid 3 to 24 months check", false);
				TryRead(() => data.Judgment = Convert.ToBoolean(report.summary.ich.judgment), "min 500 gbps judgment check", false);
				TryRead(() => data.Iva = Convert.ToBoolean(report.summary.ich.iva), " Individual Voluntary Arrangement check", false);
				TryRead(() => data.Boss = Convert.ToBoolean(report.summary.ich.boss), "Bankruptcy Order or Scottish Sequestration check", false);
				//Summary indebtedness of applicant
				TryRead(() => data.BalanceLimitRatioVolve = (long)report.summary.indebt.balancelimitratiorevolve, "% ratio of balance to limits for all account group 3 Accounts", false);
				TryRead(() => data.TotalBalancesActive = (long)report.summary.indebt.totalbalancesactive, "Total balances for all active Accounts", false);
				TryRead(() => data.TotalBalancesLoans = (long)report.summary.indebt.totalbalancesloans, "Total balances for all account group 1 accounts", false);
				TryRead(() => data.TotalBalancesMortgage = (long)report.summary.indebt.totalbalancesmortgages, "Total balances for all account group 2 accounts (Mortgage Accounts)", false);
				TryRead(() => data.TotalBalancesRevolve = (long)report.summary.indebt.totalbalancesrevolve, "Total balances for all account group 3 Accounts", false);
				TryRead(() => data.TotalLimitsRevolve = (long)report.summary.indebt.totallimitsrevolve, "Total limits for all active account group 3 Accounts", false);
				//Summary judgments
				TryRead(() => data.Total = (int)(report.summary.judgments.total), "Total number of Judgments", false);
				TryRead(() => data.TotalActive = (int)(report.summary.judgments.totalactive), "Total number of active Judgments", false);
				TryRead(() => data.Total36m = (int)(report.summary.judgments.total36m), "Total number of Judgments in last 3 years", false);
				TryRead(() => data.TotalSatisfied = (int)(report.summary.judgments.totalsatisfied), "Total number of satisfied Judgments", false);
				TryRead(() => data.TotalActiveAmount = (int)(report.summary.judgments.totalactiveamount), "Total amount of active Judgments", false);
				TryRead(() => data.TotalSatisfiedAmount = (int)(report.summary.judgments.totalsatisfiedamount), "Total amount of satisfied Judgments", false);
				//Summary links
				TryRead(() => data.TotalUndecAddresses = (int)(report.summary.links.totalundecaddresses), " total number of undeclared Address Links", false);
				TryRead(() => data.TotalUndecAddressesSearched = (int)(report.summary.links.totalundecaddressessearched), " total number of undeclared searched Address Links", false);
				TryRead(() => data.TotalUndecAddressesUnsearched = (int)(report.summary.links.totalundecaddressesunsearched), " total number of undeclared unsearched Address Links", false);
				TryRead(() => data.TotalUndecAliases = (int)(report.summary.links.totalundecaliases), " total number of undeclared Alias Links", false);
				TryRead(() => data.TotalUndecAssociates = (int)(report.summary.links.totalundecassociates), " total number of undeclared Associate Links", false);
				//summary rtr (Real Time Reporting)
				TryRead(() => data.HasUpdates = Convert.ToBoolean(report.summary.rtr.hasupdates), "MODA data check", false);
				//summary searches
				TryRead(() => data.TotalHomeCreditSearches3Months = (int)report.summary.searches.totalhomecreditsearches3months, "home credit searches over last 3 months", false);
				TryRead(() => data.TotalSearches3Months = (int)report.summary.searches.totalsearches3months, "total searches over last 3 months", false);
				TryRead(() => data.TotalSearches12Months = (int)report.summary.searches.totalsearches12months, "total searches over last 3 months", false);
				//summary shared accounts
				TryRead(() => data.TotalAccounts = (int)report.summary.share.totalaccounts, "total share accs", false);
				TryRead(() => data.TotalActiveAccs = (int)report.summary.share.totalactiveaccs, "total active share accs", false);
				TryRead(() => data.TotalSettledAccs = (int)report.summary.share.totalsettledaccs, "total settled share accs", false);
				TryRead(() => data.TotalOpened6Month = (int)report.summary.share.totalopened6months, "total share accs opened 6 months", false);
				TryRead(() => data.WorstPayStatus12Month = report.summary.share.worsepaystatus12months, "worst payment status last 12 months", false);
				TryRead(() => data.WorstPayStatus36Month = report.summary.share.worsepaystatus36months, "worst payment status last 36 months", false);
				TryRead(() => data.TotalDelinqs12Month = (int)report.summary.share.totaldelinqs12months, "delinquent share accs last 12 months", false);
				TryRead(() => data.TotalDefaults12Month = (int)report.summary.share.totaldefaults12months, "total share accs defalted last 12 months", false);
				TryRead(() => data.TotalDefaults36Month = (int)report.summary.share.totaldefaults36months, "total share accs defalted last 36 months", false);
				//summary address
				TryRead(() => data.MessageCode = (int)report.summary.summaryaddress.messagecode, "level of confirmation", false);
				TryRead(() => data.PafValid = Convert.ToBoolean(report.summary.summaryaddress.pafvalid), "Postcode Address File check", false);
				TryRead(() => data.RollingRoll = Convert.ToBoolean(report.summary.summaryaddress.rollingroll), "electoral roll check", false);
				//summary tpd (3rd party decisions)
				TryRead(() => data.AlertDecision = report.summary.tpd.alertdecision, "leve of alert decision data ", false);
				TryRead(() => data.AlertReview = report.summary.tpd.alertreview, "leve of alert review data ", false);
				TryRead(() => data.Hho = report.summary.tpd.hho, "leve of Household Override data ", false);
				//summary notices
				TryRead(() => data.NocFlag = Convert.ToBoolean(report.summary.notices.nocflag), "notice check", false);
				TryRead(() => data.TotalDisputes = (int)report.summary.notices.totaldisputes, "total notice of disputes", false);

				//demographics set
				TryRead(() => data.CameoUk = report.demographics.cameouk, "CAMEO UK Code", false);
				TryRead(() => data.CameoInvestor = report.demographics.cameoinvestor, "CAMEO Investor UK Code", false);
				TryRead(() => data.CameoIncome = report.demographics.cameoincome, "CAMEO Income Code", false);
				TryRead(() => data.CameoUnemployment = report.demographics.cameounemployment, "CAMEO Unemployment Code", false);
				TryRead(() => data.CameoProperty = report.demographics.cameoproperty, "CAMEO Property Code", false);
				TryRead(() => data.CameoFinance = report.demographics.cameofinance, "CAMEO Finance Code", false);
				//demographics family
				TryRead(() => data.CameoUkFam = report.demographics.family.cameoukfam, "Percentage of single adult households", false);
				TryRead(() => data.Ind_adult1 = Convert.ToInt32(report.demographics.family.ind_adult1), "Single Adult Index", false);
				//demographics household
				TryRead(() => data.Adult_1 = Convert.ToInt32(report.demographics.household.adult_1), "Percentage of single adult households", false);
				TryRead(() => data.Adults_2 = Convert.ToInt32(report.demographics.household.adults_2), "Percentage of two adult households", false);
				TryRead(() => data.Adult_3pl = Convert.ToInt32(report.demographics.household.adult_1), "Percentage of three or more adult households", false);
				//demographics age
				TryRead(() => data.Age0_17 = Convert.ToInt32(report.demographics.age.age0_17), "Age 0-17 index", false);
				TryRead(() => data.Age18_24 = Convert.ToInt32(report.demographics.age.age18_24), "Age 18-24 index", false);
				TryRead(() => data.Age25_34 = Convert.ToInt32(report.demographics.age.age25_34), "Age 25-34 index", false);
				TryRead(() => data.Age35_44 = Convert.ToInt32(report.demographics.age.age35_44), "Age 35-44 index", false);
				TryRead(() => data.Age45_54 = Convert.ToInt32(report.demographics.age.age45_54), "Age 45-54 index", false);
				TryRead(() => data.Age55_64 = Convert.ToInt32(report.demographics.age.age55_64), "Age 55-64 index", false);
				TryRead(() => data.Age65_74 = Convert.ToInt32(report.demographics.age.age65_74), "Age 65-74 index", false);
				TryRead(() => data.Age75pl = Convert.ToInt32(report.demographics.age.age75pl), "Age 75 plus index", false);
				//demographics economic
				TryRead(() => data.Unem_prob = (float)report.demographics.economic.unem_prob, "Average percentage unemployed", false);
				TryRead(() => data.Unem_index = Convert.ToInt32(report.demographics.economic.unem_index), "Index of economic inactivity", false);
				//demographics economicactivity
				TryRead(() => data.Wk_fem_ind = Convert.ToInt32(report.demographics.economicactivity.wkfem_ind), "Working females indices", false);
				TryRead(() => data.Stu_ind = Convert.ToInt32(report.demographics.economicactivity.stu_ind), "Students indices", false);
				TryRead(() => data.Sick_ind = Convert.ToInt32(report.demographics.economicactivity.sick_ind), "Residents with long-term illness indices", false);
				TryRead(() => data.Degree_ind = Convert.ToInt32(report.demographics.economicactivity.degree_ind), "Residents with degrees indices", false);
				//demographics socialclass
				TryRead(() => data.Ab_ind = Convert.ToInt32(report.demographics.socialclass.ab_ind), "Social Class AB Indices", false);
				TryRead(() => data.C1_ind = Convert.ToInt32(report.demographics.socialclass.c1_ind), "Social Class C1 Indices", false);
				TryRead(() => data.C2_ind = Convert.ToInt32(report.demographics.socialclass.c2_ind), "Social Class C2 Indices", false);
				TryRead(() => data.De_ind = Convert.ToInt32(report.demographics.socialclass.de_ind), "Social Class DE Indices", false);
				//demographics housing
				TryRead(() => data.Cameoukhsg = report.demographics.housing.cameoukhsg, "Housing Type descriptor ", false);
				TryRead(() => data.Cameoukten = report.demographics.housing.cameoukten, "Housing Tenure descriptor ", false);
				TryRead(() => data.Natprice = Convert.ToInt32(report.demographics.housing.natprice), "Index of property price against national average", false);
				TryRead(() => data.Regprice = Convert.ToInt32(report.demographics.housing.regprice), "Index of property price against regional average", false);
				//demographics propertyprice
				TryRead(() => data.D_index = Convert.ToInt32(report.demographics.propertyprice.d_index), "Index of detached price against national average", false);
				TryRead(() => data.D_r_index = Convert.ToInt32(report.demographics.propertyprice.d_r_index), "Index of detached price against regional average", false);
				TryRead(() => data.S_index = Convert.ToInt32(report.demographics.propertyprice.s_index), "Index of semi-detached price against national average", false);
				TryRead(() => data.S_r_index = Convert.ToInt32(report.demographics.propertyprice.s_r_index), "Index of semi-detached price against regional average", false);
				TryRead(() => data.T_index = Convert.ToInt32(report.demographics.propertyprice.t_index), "Index of terrace price against national average", false);
				TryRead(() => data.T_r_index = Convert.ToInt32(report.demographics.propertyprice.t_r_index), "Index of terrace price against regional average", false);
				TryRead(() => data.F_index = Convert.ToInt32(report.demographics.propertyprice.f_index), "Index of flat price against national average", false);
				TryRead(() => data.F_r_index = Convert.ToInt32(report.demographics.propertyprice.f_r_index), "Index of flat price against regional average", false);
				//demographics movement
				TryRead(() => data.Unem_prob = (float)report.demographics.movement.l_of_res, "Average length of residency", false);
				TryRead(() => data.Unem_index = Convert.ToInt32(report.demographics.movement.move_rate), "Movement rate (and %) from Electoral Roll historical activity", false);

				//demographics2006 set
				TryRead(() => data.CameoUk06 = report.demographics2006.cameouk, "D2006 CAMEO UK Code", false);
				TryRead(() => data.CameoUkg06 = report.demographics2006.cameoukg, "D2006 CAMEO UK Group", false);
				TryRead(() => data.CameoIncome06 = report.demographics2006.cameoincome, "D2006 Income Category", false);
				TryRead(() => data.CameoIncg06 = report.demographics2006.cameoincg, "D2006 Income Group", false);
				TryRead(() => data.CameoInvestor06 = report.demographics2006.cameoinvestor, "D2006 Investor", false);
				TryRead(() => data.CameoInvg06 = report.demographics2006.cameoinvg, "D2006 Investor group", false);
				TryRead(() => data.CameoProperty06 = report.demographics2006.cameoproperty, "D2006 Property", false);
				TryRead(() => data.CameoFinance06 = report.demographics2006.cameofinance, "D2006 Finance Code", false);
				TryRead(() => data.CameoFing06 = report.demographics2006.cameofing, "D2006 Finance Group", false);
				TryRead(() => data.CameoUnemploy06 = report.demographics2006.cameounemploy, "D2006 Unemployment Code", false);
				//demographics2006 age
				TryRead(() => data.AgeScore = (float)report.demographics2006.age.agescore, "D2006 Age score", false);
				TryRead(() => data.AgeBand = (int)report.demographics2006.age.ageband, "D2006 Age band, values 1-20", false);
				//demographics2006 tenure
				TryRead(() => data.TenureScore = (float)report.demographics2006.tenure.tenrscore, "D2006 tenure score", false);
				TryRead(() => data.TenureBand = (int)report.demographics2006.tenure.tenrband, "D2006 tenure band, values 1-20", false);
				//demographics2006 household composition
				TryRead(() => data.CompScore = (float)report.demographics2006.hhcomp.compscore, "D2006 Household composition score", false);
				TryRead(() => data.CompBand = (int)report.demographics2006.hhcomp.compband, "D2006 Household composition band, values 1-20", false);
				//demographics2006 economic
				TryRead(() => data.EconScore = (float)report.demographics2006.economic.econscore, "D2006 Economic activity score", false);
				TryRead(() => data.EconBand = (int)report.demographics2006.economic.econband, "D2006 Economic activity band, values 1-20", false);
				//demographics2006 lifestage
				TryRead(() => data.LifeScore = (float)report.demographics2006.lifestage.lifescore, "D2006 Lifestage score", false);
				TryRead(() => data.LifeBand = (int)report.demographics2006.lifestage.lifeband, "D2006 Lifestage band, values 1-20", false);
				//demographics2006 social
				TryRead(() => data.Dirhhld = (float)report.demographics2006.social.dirhhld, "D2006 Proportion of ‘Millionaire club’ households in the postcode", false);
				TryRead(() => data.Millhhld = (float)report.demographics2006.social.millhhld, "D2006 Proportion of company director households in the postcode ", false);
				TryRead(() => data.SocScore = (float)report.demographics2006.social.socscore, "D2006 Social class score", false);
				TryRead(() => data.SocBand = (int)report.demographics2006.social.socband, "D2006 Social class band, values 1-20", false);
				//demographics2006 occupation
				TryRead(() => data.OccScore = (float)report.demographics2006.occupation.occscore, "D2006 occupation score", false);
				TryRead(() => data.OccBand = (int)report.demographics2006.occupation.occband, "D2006 occupation band, values 1-20", false);
				//demographics2006 lifestage
				TryRead(() => data.MortScore = (float)report.demographics2006.mortgage.mortscore, "D2006 Mortgage and house size  score", false);
				TryRead(() => data.MortBand = (int)report.demographics2006.mortgage.mortband, "D2006 Mortgage and house size  band, values 1-20", false);
				//demographics2006 shareholding
				TryRead(() => data.HhldShare = (float)report.demographics2006.sharehld.hhldshare, "D2006 Proportion of households with shares in the postcode", false);
				TryRead(() => data.AvNumHold = (float)report.demographics2006.sharehld.avnumhold, "D2006 Average number of shareholders per share holding household", false);
				TryRead(() => data.AvNumShares = (float)report.demographics2006.sharehld.avnumshares, "D2006 Average number of shares per share holding household", false);
				TryRead(() => data.AvNumComps = (float)report.demographics2006.sharehld.avnumcomps, "D2006 Average number of companies invested in per share holding household ", false);
				TryRead(() => data.AvValShares = (float)report.demographics2006.sharehld.avvalshares, "D2006 Average values of shares per share holding household", false);
				//demographics2006 unemployment
				TryRead(() => data.UnemMalelt = (float)report.demographics2006.unemployment.unemmalelt, "D2006 Long term male unemployment", false);
				TryRead(() => data.Unem1824 = (float)report.demographics2006.unemployment.unem1824, "D2006 Unemployment among 18 to 24 year olds", false);
				TryRead(() => data.Unem2539 = (float)report.demographics2006.unemployment.unem2539, "D2006 Unemployment among 25 to 39 year olds ", false);
				TryRead(() => data.Unem40pl = (float)report.demographics2006.unemployment.unem40pl, "D2006 Unemployment among those aged 40 and over", false);
				TryRead(() => data.UnemScore = (float)report.demographics2006.unemployment.unemscore, "D2006 Unemployment score", false);
				TryRead(() => data.UnemBal = (int)report.demographics2006.social.socband, "D2006 Unemployment band, values 1 to 20", false);
				//demographics2006 unemployment rate
				TryRead(() => data.UnemRate = (float)report.demographics2006.unemprate.unemrate, "D2006 Unemployment Rate", false);
				TryRead(() => data.UnemDiff = (float)report.demographics2006.unemprate.unemdiff, "D2006 Unemployment Rate difference", false);
				TryRead(() => data.UnemInd = (int)report.demographics2006.unemprate.unemind, "D2006 Rate Index (against national average),", false);
				TryRead(() => data.Unemall = (float)report.demographics2006.unemprate.unemall, "D2006 Overall Unemployment Rating", false);
				TryRead(() => data.UnemallIndex = (int)report.demographics2006.unemprate.unemallindex, "D2006 Overall Unemployment Rating Index", false);
				//demographics2006 property
				TryRead(() => data.HousAge = report.demographics2006.property.houseage, "D2006 Average House Age", false);
				TryRead(() => data.HhldDensity = (float)report.demographics2006.property.hhlddensity, "D2006 Household Density", false);
				TryRead(() => data.CtaxBand = report.demographics2006.property.ctaxband, "D2006 Council Tax Band", false);
				TryRead(() => data.LocationType = (int)report.demographics2006.property.locationtype, "D2006 Postcode Location Type, values 1 to 5", false);
				TryRead(() => data.NatAvgHouse = (int)report.demographics2006.property.natavghouse, "D2006 National Average House Price, value 0 to 99999999", false);
				TryRead(() => data.HouseScore = (float)report.demographics2006.property.housescore, "D2006Housing Type Score", false);
				TryRead(() => data.HouseBand = (int)report.demographics2006.property.houseband, "D2006 Housing Type Band, value 1 to 20", false);
				TryRead(() => data.PriceDiff = (long)report.demographics2006.property.pricediff, "D2006 National Average House Price Difference, values -9999999 to 99999999", false);
				TryRead(() => data.PriceIndex = (int)report.demographics2006.property.priceindex, "D2006 House Price Index, values 0 to 99999", false);
				TryRead(() => data.Activity = (int)report.demographics2006.property.activity, "D2006 Level of Sales Activity Index, values 0 to 9999", false);
				TryRead(() => data.RegionalBand = (int)report.demographics2006.property.regionalband, "D2006 Regional Banded House Price, values 1 to 10 ", false);
				TryRead(() => data.AvgDetVal = (int)report.demographics2006.property.avgdetvalue, "D2006 Average Detached Property, values 0 to 9999999", false);
				TryRead(() => data.AvgDetIndex = (int)report.demographics2006.property.avgdetindex, "D2006 Detached Property Index, values 0 to 9999", false);
				TryRead(() => data.AvgSemiVal = (int)report.demographics2006.property.avgsemivalue, "D2006 Average Semi-Detached Property Value, values 0 to 9999999", false);
				TryRead(() => data.AvgSemiIndex = (int)report.demographics2006.property.avgsemiindex, "D2006 Semi-Detached Property Index, values 0 to 9999", false);
				TryRead(() => data.AvgTerrVal = (int)report.demographics2006.property.avgterrvalue, "D2006 Average Terraced Property Value, values 0 to 9999999", false);
				TryRead(() => data.AvgTerrIndex = (int)report.demographics2006.property.avgterrindex, "D2006 Terraced Property Index, values 0 to 9999", false);
				TryRead(() => data.AvgFlatVal = (int)report.demographics2006.property.avgflatvalue, "D2006 Average Flat Property Value, values 0 to 9999999", false);
				TryRead(() => data.AvgFlatIndex = (int)report.demographics2006.property.avgflatindex, "D2006 Flat Property Index, values 0 to 9999", false);
				TryRead(() => data.RegionCode = (int)report.demographics2006.property.regioncode, "D2006 Standard Region Code, values 1 to 17", false);
				//demographics2006 international
				TryRead(() => data.CameoIntl = report.demographics2006.international.cameointl, "D2006 CAMEO International Code", false);

				Applicants.Add(data);

			data.Accs = GetAccs(report, i);
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
			}

			return Applicants;
		}

		public List<CallCreditAmendments> GetAmendments(CT_amendsubsequent amendmentsubreq) {
			var Amends = new List<CallCreditAmendments>();
			TryRead(() => {
				foreach (var AmNd in amendmentsubreq.amendments) {
					var amendments = new CallCreditAmendments();

					var amend = AmNd;
					TryRead(() => amendments.AmendmentName = amend.amendmentname, "Name of node that the amendment applies to", false);
					TryRead(() => amendments.AmendmentType = amend.amendmenttype, "Type of amendment –update or insert", false);
					TryRead(() => amendments.Balorlim = (int)amend.balorlim, "Change to balance or credit limit ", false);
					TryRead(() => amendments.Term = amend.term, "Change to term of loan", false);
					TryRead(() => amendments.AbodeNo = amend.address.abodeno, "Abode name or number", false);
					TryRead(() => amendments.BuildingNo = amend.address.buildingno, "Building Number", false);
					TryRead(() => amendments.BuildingName = amend.address.buildingname, "Building Name", false);
					TryRead(() => amendments.Street1 = amend.address.street1, "Street1", false);
					TryRead(() => amendments.Street2 = amend.address.street2, "Street2", false);
					TryRead(() => amendments.Sublocality = amend.address.sublocality, "Sublocality", false);
					TryRead(() => amendments.Locality = amend.address.locality, "District or Locality", false);
					TryRead(() => amendments.PostTown = amend.address.posttown, "Town", false);
					TryRead(() => amendments.PostCode = amend.address.postcode, "Postcode", false);
					amendments.StartDate = TryReadDate(() => amend.address.startdate, "The date the applicant moved into the Residence ", false);
					amendments.EndDate = TryReadDate(() => amend.address.enddate, "The date the applicant moved out of the Residence ", false);
					TryRead(() => amendments.Duration = amend.address.duration, "Duration of residency", false);
					TryRead(() => amendments.Title = amend.name.title, "Title", false);
					TryRead(() => amendments.Forename = amend.name.forename, "Forename", false);
					TryRead(() => amendments.OtherNames = amend.name.othernames, "Middle names / initials ", false);
					TryRead(() => amendments.SurName = amend.name.surname, "Surname", false);
					TryRead(() => amendments.Suffix = amend.name.suffix, "Suffix", false);
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
					TryRead(() => appaddresses.AbodeNo = appadd.abodeno, "Abode name or number", false);
					TryRead(() => appaddresses.BuildingNo = appadd.buildingno, "Building Number");
					TryRead(() => appaddresses.BuildingName = appadd.buildingname, "Building Name", false);
					TryRead(() => appaddresses.Street1 = appadd.street1, "Street1", false);
					TryRead(() => appaddresses.Street2 = appadd.street2, "Street2", false);
					TryRead(() => appaddresses.SubLocality = appadd.sublocality, "Sublocality", false);
					TryRead(() => appaddresses.Locality = appadd.locality, "District or Locality", false);
					TryRead(() => appaddresses.PostTown = appadd.posttown, "Town", false);
					TryRead(() => appaddresses.PostCode = appadd.postcode, "Postcode");
					appaddresses.StartDate = TryReadDate(() => appadd.startdate, "The date the applicant moved into the Residence ", false);
					appaddresses.EndDate = TryReadDate(() => appadd.enddate, "The date the applicant moved out of the Residence ", false);
					TryRead(() => appaddresses.Duration = appadd.duration, "Duration of residency", false);
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

					TryRead(() => appnames.Title = apname.title, "Title", false);
					TryRead(() => appnames.Forename = apname.forename, "Forename");
					TryRead(() => appnames.OtherNames = apname.othernames, "Middle names / initials ", false);
					TryRead(() => appnames.Surname = apname.surname, "Surname");
					TryRead(() => appnames.Suffix = apname.suffix, "Suffix", false);
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
	}
}
