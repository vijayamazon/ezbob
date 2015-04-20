namespace CallCreditLib {
	using System;
	using System.Collections.Generic;
	using Callcredit.CRBSB;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;

	public partial class CallCreditModelBuilder {


		public List<CallCreditEmail> GetEmail(CT_applicantdemographics appdem) {
			var EmailAddresses = new List<CallCreditEmail>();
			TryRead(() => {
				foreach (var EmL in appdem.contact.email) {
					var email = new CallCreditEmail();

					var emailaddress = EmL;

					TryRead(() => email.EmailType = emailaddress.type, "Email type code", false);
					TryRead(() => email.EmailAddress = emailaddress.address, "Email address", false);
					EmailAddresses.Add(email);
				}
			}, "Emails");

			return EmailAddresses;
		}


		public List<CallCreditTelephone> GetTelephone(CT_applicantdemographics appdem) {
			var TelephoneNumbers = new List<CallCreditTelephone>();
			TryRead(() => {
				foreach (var TlPh in appdem.contact.telephone) {
					var telephones = new CallCreditTelephone();

					var phonenumber = TlPh;

					TryRead(() => telephones.TelephoneType = phonenumber.type, "Telephone Type Code", false);
					TryRead(() => telephones.STD = phonenumber.std, "Telephone STD Code", false);
					TryRead(() => telephones.PhoneNumber = phonenumber.number, "Telephone number", false);
					TryRead(() => telephones.Extension = phonenumber.extension, "Telephone extension number", false);
					TelephoneNumbers.Add(telephones);
				}
			}, "Telephones");
			return TelephoneNumbers;
		}


		public List<CallCreditDataJudgments> GetJudgments(CT_outputapplicant applicant) {
			var Judgments = new List<CallCreditDataJudgments>();
			TryRead(() => {
				foreach (var Judg in applicant.judgments) {
					var judgment = new CallCreditDataJudgments {
						JudgmentNocs = new List<CallCreditDataJudgmentsNocs>()
					};

					var judgs = Judg;
					TryRead(() => judgment.NameDetails = judgs.name, "Name details as provided on the Judgment", false);
					judgment.Dob = TryReadDate(() => judgs.dob, "Date of birth as provided on the Judgment", false);
					TryRead(() => judgment.CourtName = judgs.courtname, "Court name", false);
					TryRead(() => judgment.CourtType = (int)judgs.courttype, "Court type", false);
					TryRead(() => judgment.CaseNumber = judgs.casenumber, "Case number", false);
					TryRead(() => judgment.Status = judgs.status, "Status of Judgment", false);
					TryRead(() => judgment.Amount = (int)judgs.amount, "Total amount of the Judgment", false);
					judgment.JudgmentDate = TryReadDate(() => judgs.judgmentdate, "Date of the Judgment", false);
					judgment.DateSatisfied = TryReadDate(() => judgs.datesatisfied, "Date that the Judgment was satisfied", false);
					TryRead(() => judgment.CurrentAddress = Convert.ToBoolean(judgs.address.current), "Current address check", false);
					TryRead(() => judgment.UnDeclaredAddressType = (int)judgs.address.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => judgment.AddressValue = judgs.address.Value, "Address value related to judgment", false);
					TryRead(() => {
						foreach (var JudgNoc in judgs.notice) {
							var judgmentnoc = new CallCreditDataJudgmentsNocs();

							var judgnotice = JudgNoc;
							TryRead(() => judgmentnoc.NoticeType = judgnotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => judgmentnoc.RefNum = judgnotice.refnum, "Notice Type (Notice Reference Number )", false);
							judgmentnoc.DateRaised = TryReadDate(() => judgnotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => judgmentnoc.Text = judgnotice.text, "Text for Notice of Correction", false);
							TryRead(() => judgmentnoc.NameDetails = judgnotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => judgmentnoc.CurrentAddress = Convert.ToBoolean(judgnotice.address.current), "Current address check", false);
							TryRead(() => judgmentnoc.UnDeclaredAddressType = (int)judgnotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => judgmentnoc.AddressValue = judgnotice.address.Value, "Address value related to judgment notice", false);
							judgment.JudgmentNocs.Add(judgmentnoc);
						}
					}, "Judgment Notices");

					Judgments.Add(judgment);

				}
			}, "Judgments");

			return Judgments;
		}
		
		
		public List<CallCreditDataCreditScores> GetCreditScores(CT_outputapplicant request) {
			var CreditScores = new List<CallCreditDataCreditScores>();
			TryRead(() => {
				foreach (var CrSc in request.creditscores) {
					var creditscores = new CallCreditDataCreditScores();

					var credscore = CrSc;

					TryRead(() => creditscores.score = (int)credscore.score.Value, "Score value");
					TryRead(() => creditscores.ScoreClass = (int)credscore.score.@class, "Class of scorecard used", false);
					TryRead(() => creditscores.Reason1 = (int)credscore.reasons[0], "reason 1 code for the credit score", false);
					TryRead(() => creditscores.Reason2 = (int)credscore.reasons[1], "reason 2 code for the credit score", false);
					TryRead(() => creditscores.Reason3 = (int)credscore.reasons[2], "reason 3 code for the credit score", false);
					TryRead(() => creditscores.Reason4 = (int)credscore.reasons[3], "reason 4 code for the credit score", false);
					CreditScores.Add(creditscores);
				}
			}, "Credit scores");

			return CreditScores;
		}

		public List<CallCreditDataSearches> GetSearches(CT_outputapplicant request) {
			var Searches = new List<CallCreditDataSearches>();
			TryRead(() => {
				foreach (var SrCh in request.searches) {
					var searches = new CallCreditDataSearches();

					var search = SrCh;

					TryRead(() => searches.SearchRef = search.searchref, "Unique Search reference identifier", false);
					TryRead(() => searches.SearchOrgType = search.searchorgtype, "Type of organisation carrying out the Search", false);
					TryRead(() => searches.SearchOrgName = search.searchorgname, "Name of (own only) organisation carrying out the Search", false);
					TryRead(() => searches.YourReference = search.yourreference, "Your reference supplied on the Search (own organisations only)", false);
					TryRead(() => searches.SearchUnitName = search.searchunitname, "Name of organisational unit carrying out the Search (own organisations only)", false);
					TryRead(() => searches.OwnSearch = Convert.ToBoolean(search.ownsearch), "User's own organisation check", false);
					TryRead(() => searches.SubsequentEnquiry = Convert.ToBoolean(search.subsequentenquiry), "Subsequent enquiry check", false);
					TryRead(() => searches.UserName = search.username, "Username of user carrying out the Search (own organisations only)", false);
					TryRead(() => searches.SearchPurpose = search.searchpurpose, "Search purpose", false);
					TryRead(() => searches.CreditType = search.credittype, "Credit type", false);
					TryRead(() => searches.Balance = (int)search.balance, "Balance or credit limit applied for", false);
					TryRead(() => searches.Term = (int)search.term, "Term of loan applied for", false);
					TryRead(() => searches.JointApplication = Convert.ToBoolean(search.jointapplication), "This search was a joint application check", false);
					searches.SearchDate = TryReadDate(() => search.searchdate, "Date of Search", false);
					TryRead(() => searches.NameDetailes = search.name, "Name details input for this Search", false);
					TryRead(() => searches.Dob = search.dob, "Applicant's date of birth", false);
					searches.StartDate = TryReadDate(() => search.startdate, "Move in date specified for this Search", false);
					searches.EndDate = TryReadDate(() => search.enddate, "Move out date specified for this Search", false);
					TryRead(() => searches.TpOptOut = Convert.ToBoolean(search.tpoptout), "Third party data is opted out check", false);
					TryRead(() => searches.Transient = Convert.ToBoolean(search.transient), "Transient association check", false);
					TryRead(() => searches.LinkType = search.linktype, "Link Report Type (Non, Address, Associate)", false);
					TryRead(() => searches.CurrentAddress = Convert.ToBoolean(search.address.current), "Current address check", false);
					TryRead(() => searches.UnDeclaredAddressType = (int)search.address.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => searches.AddressValue = search.address.Value, "Address value related to specific search", false);
					Searches.Add(searches);
				}
			}, "Searches");

			return Searches;
		}


		public List<CallCreditDataNocs> GetNocs(CT_outputapplicant request) {
			var Notices = new List<CallCreditDataNocs>();
			TryRead(() => {
				foreach (var NoC in request.nocs) {
					var notices = new CallCreditDataNocs();

					var nocs = NoC;

					TryRead(() => notices.NoticeType = nocs.type, "Notice Type (Correction or Dispute)", false);
					TryRead(() => notices.Refnum = nocs.refnum, "Notice Type (Notice Reference Number)", false);
					notices.DateRaised = TryReadDate(() => nocs.dateraised, "Date that the Notice was raised)", false);
					TryRead(() => notices.Text = nocs.text, "Text for Notice of Correction", false);
					TryRead(() => notices.NameDetails = nocs.name, "Name details as provided on the Notice of Correction)", false);
					TryRead(() => notices.CurrentAddress = Convert.ToBoolean(nocs.address.current), "Current address check", false);
					TryRead(() => notices.UnDeclaredAddressType = (int)nocs.address.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => notices.AddressValue = nocs.address.Value, "Address value related to an applicant", false);
					Notices.Add(notices);
				}
			}, "Notices");

			return Notices;
		}


		public List<CallCreditDataRtr> GetRtr(CT_outputapplicant applicant) {
			var Rtr = new List<CallCreditDataRtr>();
			TryRead(() => {
				foreach (var RtR in applicant.rtr) {
					var rtr = new CallCreditDataRtr {
						RtrNocs = new List<CallCreditDataRtrNocs>()
					};

					var rtreport = RtR;
					TryRead(() => rtr.HolderName = rtreport.holder.name, "Account holder's name", false);
					rtr.Dob = TryReadDate(() => rtreport.holder.dob, "Account holder's date of birth", false);
					TryRead(() => rtr.CurrentAddress = Convert.ToBoolean(rtreport.holder.address.current), "Current address check", false);
					TryRead(() => rtr.UnDeclaredAddressType = (int)rtreport.holder.address.undeclaredaddresstype, "Type of undeclared address", false);
					TryRead(() => rtr.AddressValue = rtreport.holder.address.Value, "Address value related to account holder", false);
					TryRead(() => rtr.Updated = rtreport.updated, "The time stamp of publication", false);
					TryRead(() => rtr.OrgTypeCode = rtreport.orgtypecode, "Organisation type code", false);
					TryRead(() => rtr.OrgName = rtreport.orgname, "Organisation name", false);
					TryRead(() => rtr.AccNum = rtreport.accnum, "Account number", false);
					TryRead(() => rtr.AccSuffix = Convert.ToString(rtreport.accsuffix), "Account suffix", false);
					TryRead(() => rtr.AccTypeCode = rtreport.acctypecode, "Account type code", false);
					TryRead(() => rtr.Balance = rtreport.balance, "Current balance on account ", false);
					TryRead(() => rtr.Limit = rtreport.limit, "Current credit limit on account ", false);
					rtr.StartDate = TryReadDate(() => rtreport.startdate, "Account start date", false);
					rtr.EndDate = TryReadDate(() => rtreport.enddate, "Account closed date", false);
					TryRead(() => rtr.AccStatusCode = rtreport.accstatuscode, "Account status code", false);
					TryRead(() => rtr.RepayFreqCode = rtreport.repayfreqcode, "Repayment frequency code", false);
					TryRead(() => rtr.NumOverdue = (int)rtreport.numoverdue, "Number of overdue payments", false);
					TryRead(() => rtr.Rollover = Convert.ToBoolean(rtreport.rollover), "Account is rolled over check", false);
					TryRead(() => rtr.CrediText = Convert.ToBoolean(rtreport.creditext), "Credit extension check", false);
					TryRead(() => rtr.ChangePay = Convert.ToBoolean(rtreport.changepay), "Change to payment terms check", false);
					TryRead(() => rtr.NextPayAmount = rtreport.nextpayamount, "Value of next payment due", false);
					TryRead(() => {
						foreach (var JudgNoc in rtreport.notice) {
							var rtrnoc = new CallCreditDataRtrNocs();

							var rtrnotice = JudgNoc;
							TryRead(() => rtrnoc.NoticeType = rtrnotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => rtrnoc.Refnum = rtrnotice.refnum, "Notice Type (Notice Reference Number)", false);
							rtrnoc.DateRaised = TryReadDate(() => rtrnotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => rtrnoc.Text = rtrnotice.text, "Text for Notice of Correction", false);
							TryRead(() => rtrnoc.NameDetails = rtrnotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => rtrnoc.CurrentAddress = Convert.ToBoolean(rtrnotice.address.current), "Current address check", false);
							TryRead(() => rtrnoc.UnDeclaredAddressType = (int)rtrnotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => rtrnoc.AddressValue = rtrnotice.address.Value, "Address value related to real time report", false);
							rtr.RtrNocs.Add(rtrnoc);
						}
					}, "Real time report Notices");

					Rtr.Add(rtr);

				}
			}, "Real time report(MODA)");

			return Rtr;
		}

		public List<CallCreditDataTpd> GetTpd(CT_outputthirdpartyalerts thirdparty) {
			var Tpd = new List<CallCreditDataTpd>();
			var tpdata = new CallCreditDataTpd {
				DecisionAlertIndividuals = new List<CallCreditDataTpdDecisionAlertIndividuals>(),
				DecisionCreditScores = new List<CallCreditDataTpdDecisionCreditScores>(),
				HhoCreditScores = new List<CallCreditDataTpdHhoCreditScores>(),
				ReviewAlertIndividuals = new List<CallCreditDataTpdReviewAlertIndividuals>()
			};

			//Third party data

			//alert decision attribute
			TryRead(() => tpdata.TotalD = (int)thirdparty.decision.total, "Total number of Individuals that meet Alert Decision criteria", false);
			//alert decision summary
			TryRead(() => tpdata.Total36mJudgmesntsD = (int)thirdparty.decision.summary.judgments.total36m, "Total number of Judgments in last 3 years (Alert Decision)", false);
			TryRead(() => tpdata.TotalJudgmesntsD = (int)thirdparty.decision.summary.judgments.total, "Total number of Judgments (Alert Decision)", false);
			TryRead(() => tpdata.TotalActiveAmountJudgmesntsD = (int)thirdparty.decision.summary.judgments.totalactiveamount, "Total amount of active Judgments (Alert Decision)", false);
			TryRead(() => tpdata.CurrentlyInsolventD = Convert.ToBoolean(thirdparty.decision.summary.bais.currentlyinsolvent), "Currently insolvent check(Alert Decision)", false);
			TryRead(() => tpdata.RestrictedD = Convert.ToBoolean(thirdparty.decision.summary.bais.restricted), "Existing restriction check (Alert Decision)", false);
			TryRead(() => tpdata.WorsePayStatus12mD = thirdparty.decision.summary.share.worsepaystatus12m, "Worst payment status on SHARE Accounts in the last 12 months (Alert Decision)", false);
			TryRead(() => tpdata.WorsePayStatus24mD = thirdparty.decision.summary.share.worsepaystatus24m, "Worst payment status on SHARE Accounts in the last 24 months (Alert Decision)", false);
			TryRead(() => tpdata.TotalDefaultsD = (int)thirdparty.decision.summary.share.totaldefaults, "Total number of SHARE Accounts in default (Alert Decision)", false);
			TryRead(() => tpdata.TotalDefaults12mD = (int)thirdparty.decision.summary.share.totaldefaults12m, "Total number of SHARE Accounts that have defaulted in the last 12 months (Alert Decision)", false);
			TryRead(() => tpdata.TotalSettledDefaultsD = (int)thirdparty.decision.summary.share.totalsettleddefaults, "Total number of SHARE Accounts that have been satisifed (Alert Decision)", false);
			TryRead(() => tpdata.TotalDefaultsAmountD = (int)thirdparty.decision.summary.share.totaldefaultsamount, "Total value of SHARE Accounts in default (Alert Decision)", false);
			TryRead(() => tpdata.TotalWriteoffsD = (int)thirdparty.decision.summary.share.totalwriteoffs, "Total number of SHARE Accounts that have been written off (Alert Decision)", false);
			TryRead(() => tpdata.TotalWriteoffsAmountD = (int)thirdparty.decision.summary.share.totalwriteoffsamount, "Total value of SHARE Accounts that have been written off (Alert Decision)", false);
			TryRead(() => tpdata.TotalDelinqsD = (int)thirdparty.decision.summary.share.totaldelinqs, "Total number of delinquent SHARE Accounts (Alert Decision)", false);
			TryRead(() => tpdata.TotalDelinqsAmountD = (int)thirdparty.decision.summary.share.totaldelinqsamount, "Total value of delinquent SHARE Accounts (Alert Decision)", false);
			//alert review attribute 
			TryRead(() => tpdata.TotalR = (int)thirdparty.review.total, "Total number of Individuals that meet Alert Review criteria", false);
			//alert review summary
			TryRead(() => tpdata.Total36mJudgmesntsR = (int)thirdparty.review.summary.judgments.total36m, "Total number of Judgments in last 3 years (Alert Review)", false);
			TryRead(() => tpdata.TotalJudgmesntsR = (int)thirdparty.review.summary.judgments.total, "Total number of Judgments (Alert Review)", false);
			TryRead(() => tpdata.TotalActiveAmountJudgmesntsR = (int)thirdparty.review.summary.judgments.totalactiveamount, "Total amount of active Judgments (Alert Review)", false);
			TryRead(() => tpdata.CurrentlyInsolventR = Convert.ToBoolean(thirdparty.review.summary.bais.currentlyinsolvent), "Currently insolvent check(Alert Review)", false);
			TryRead(() => tpdata.RestrictedR = Convert.ToBoolean(thirdparty.review.summary.bais.restricted), "Existing restriction check (Alert Review)", false);
			TryRead(() => tpdata.WorsePayStatus12mR = thirdparty.review.summary.share.worsepaystatus12m, "Worst payment status on SHARE Accounts in the last 12 months (Alert Review)", false);
			TryRead(() => tpdata.WorsePayStatus24mR = thirdparty.review.summary.share.worsepaystatus24m, "Worst payment status on SHARE Accounts in the last 24 months (Alert Review)", false);
			TryRead(() => tpdata.TotalDefaultsR = (int)thirdparty.review.summary.share.totaldefaults, "Total number of SHARE Accounts in default (Alert Review)", false);
			TryRead(() => tpdata.TotalDefaults12mR = (int)thirdparty.review.summary.share.totaldefaults12m, "Total number of SHARE Accounts that have defaulted in the last 12 months (Alert Review)", false);
			TryRead(() => tpdata.TotalSettledDefaultsR = (int)thirdparty.review.summary.share.totalsettleddefaults, "Total number of SHARE Accounts that have been satisifed (Alert Review)", false);
			TryRead(() => tpdata.TotalDefaultsAmountR = (int)thirdparty.review.summary.share.totaldefaultsamount, "Total value of SHARE Accounts in default (Alert Review)", false);
			TryRead(() => tpdata.TotalWriteoffsR = (int)thirdparty.review.summary.share.totalwriteoffs, "Total number of SHARE Accounts that have been written off (Alert Review)", false);
			TryRead(() => tpdata.TotalWriteoffsAmountR = (int)thirdparty.review.summary.share.totalwriteoffsamount, "Total value of SHARE Accounts that have been written off (Alert Review)", false);
			TryRead(() => tpdata.TotalDelinqsR = (int)thirdparty.review.summary.share.totaldelinqs, "Total number of delinquent SHARE Accounts (Alert Review)", false);
			TryRead(() => tpdata.TotalDelinqsAmountR = (int)thirdparty.review.summary.share.totaldelinqsamount, "Total value of delinquent SHARE Accounts (Alert Review)", false);
			//household override attributes
			TryRead(() => tpdata.TotalH = (int)thirdparty.hho.total, "Currently insolvent check(HHO)", false);
			TryRead(() => tpdata.ThinFile = Convert.ToBoolean(thirdparty.hho.thinfile), "Total number of Individuals that meet HHO criteria", false);
			//household override summary
			TryRead(() => tpdata.Total36mJudgmentsH = (int)thirdparty.hho.summary.judgments.total36m, "Total number of Judgments in last 3 years (HHO)", false);
			TryRead(() => tpdata.TotalJudgmentsH = (int)thirdparty.hho.summary.judgments.total, "Total number of Judgments (HHO)", false);
			TryRead(() => tpdata.TotalActiveAmountJudgmesntsH = (int)thirdparty.hho.summary.judgments.totalactiveamount, "Total amount of active Judgments (HHO)", false);
			TryRead(() => tpdata.TotalSatisfiedJudgmesntsH = (int)thirdparty.hho.summary.judgments.totalsatisfied, "Total amount of active Judgments (HHO)", false);
			TryRead(() => tpdata.TotalSatisfiedAmountJudgmesntsH = (int)thirdparty.hho.summary.judgments.totalsatisfiedamount, "Total amount of active Judgments (HHO)", false);
			TryRead(() => tpdata.CurrentlyInsolventH = Convert.ToBoolean(thirdparty.hho.summary.bais.currentlyinsolvent), "Currently insolvent check(HHO)", false);
			TryRead(() => tpdata.RestrictedH = Convert.ToBoolean(thirdparty.hho.summary.bais.restricted), "Existing restriction check (HHO)", false);
			TryRead(() => tpdata.TotalAccountsH = (int)thirdparty.hho.summary.share.totalaccounts, "Total number of SHARE Accounts that have been satisifed (HHO)", false);
			TryRead(() => tpdata.TotalActiveAccountsH = (int)thirdparty.hho.summary.share.totalactiveaccounts, "Total number of SHARE Accounts that have been satisifed (HHO)", false);
			TryRead(() => tpdata.TotalActiveAccountsAmountH = (int)thirdparty.hho.summary.share.totalactiveaccountsamount, "Total number of SHARE Accounts that have been satisifed (HHO)", false);
			TryRead(() => tpdata.TotalAccountsZerobalH = (int)thirdparty.hho.summary.share.totalaccountszerobal, "Total number of SHARE Accounts that have been satisifed (HHO)", false);
			TryRead(() => tpdata.WorsePayStatus12mH = thirdparty.hho.summary.share.worsepaystatus12m, "Worst payment status on SHARE Accounts in the last 12 months (HHO)", false);
			TryRead(() => tpdata.WorsePayStatus24mH = thirdparty.hho.summary.share.worsepaystatus24m, "Worst payment status on SHARE Accounts in the last 24 months (HHO)", false);
			TryRead(() => tpdata.TotalDefaultsH = (int)thirdparty.hho.summary.share.totaldefaults, "Total number of SHARE Accounts in default (HHO)", false);
			TryRead(() => tpdata.TotalDefaults12mH = (int)thirdparty.hho.summary.share.totaldefaults12m, "Total number of SHARE Accounts that have defaulted in the last 12 months (HHO)", false);
			TryRead(() => tpdata.TotalDefaultsAmountH = (int)thirdparty.hho.summary.share.totaldefaultsamount, "Total value of SHARE Accounts in default (HHO)", false);
			TryRead(() => tpdata.TotalWriteoffsH = (int)thirdparty.hho.summary.share.totalwriteoffs, "Total number of SHARE Accounts that have been written off (HHO)", false);
			TryRead(() => tpdata.TotalWriteoffsAmountH = (int)thirdparty.hho.summary.share.totalwriteoffsamount, "Total value of SHARE Accounts that have been written off (HHO)", false);
			TryRead(() => tpdata.TotalDelinqsH = (int)thirdparty.hho.summary.share.totaldelinqs, "Total number of delinquent SHARE Accounts (HHO)", false);
			TryRead(() => tpdata.TotalDelinqsAmountH = (int)thirdparty.hho.summary.share.totaldelinqsamount, "Total value of delinquent SHARE Accounts (HHO)", false);


			TryRead(() => {
				foreach (var TpdADI in thirdparty.decision.alertindividual) {
					var alertdecisionind = new CallCreditDataTpdDecisionAlertIndividuals() {
						DecisionAlertIndividualNocs = new List<CallCreditDataTpdDecisionAlertIndividualsNocs>()
					};

					var aldecind = TpdADI;
					TryRead(() => alertdecisionind.IndividualName = aldecind.name, "Name of Alert decision Individual", false);
					TryRead(() => {
						foreach (var TpdADINoc in aldecind.notice) {
							var alertdecisionindnoc = new CallCreditDataTpdDecisionAlertIndividualsNocs();

							var aldecindnotice = TpdADINoc;
							TryRead(() => alertdecisionindnoc.NoticeType = aldecindnotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => alertdecisionindnoc.Refnum = aldecindnotice.refnum, "Notice Type (Notice Reference Number)", false);
							alertdecisionindnoc.DateRaised = TryReadDate(() => aldecindnotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => alertdecisionindnoc.Text = aldecindnotice.text, "Text for Notice of Correction", false);
							TryRead(() => alertdecisionindnoc.NameDetails = aldecindnotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => alertdecisionindnoc.CurrentAddress = Convert.ToBoolean(aldecindnotice.address.current), "Current address check", false);
							TryRead(() => alertdecisionindnoc.UnDeclaredAddressType = (int)aldecindnotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => alertdecisionindnoc.AddressValue = aldecindnotice.address.Value, "Address value related to notice against an alert decision individual", false);
							alertdecisionind.DecisionAlertIndividualNocs.Add(alertdecisionindnoc);
						}
					}, "Alert decision individual Notices");

					tpdata.DecisionAlertIndividuals.Add(alertdecisionind);
				}
			}, "Alert decision individuals");


			TryRead(() => {
				foreach (var TpdARI in thirdparty.review.alertindividual) {
					var alertreviewind = new CallCreditDataTpdReviewAlertIndividuals() {
						ReviewAlertIndividualNocs = new List<CallCreditDataTpdReviewAlertIndividualsNocs>()
					};

					var alrevind = TpdARI;
					TryRead(() => alertreviewind.IndividualName = alrevind.name, "Name of Alert review Individual", false);
					TryRead(() => {
						foreach (var TpdARINoc in alrevind.notice) {
							var alertreviewindnoc = new CallCreditDataTpdReviewAlertIndividualsNocs();

							var alrevindnotice = TpdARINoc;
							TryRead(() => alertreviewindnoc.NoticeType = alrevindnotice.type, "Notice Type (Correction or Dispute)", false);
							TryRead(() => alertreviewindnoc.Refnum = alrevindnotice.refnum, "Notice Type (Notice Reference Number)", false);
							alertreviewindnoc.DateRaised = TryReadDate(() => alrevindnotice.dateraised, "Date that the Notice was raised)", false);
							TryRead(() => alertreviewindnoc.Text = alrevindnotice.text, "Text for Notice of Correction", false);
							TryRead(() => alertreviewindnoc.NameDetails = alrevindnotice.name, "Name details as provided on the Notice of Correction)", false);
							TryRead(() => alertreviewindnoc.CurrentAddress = Convert.ToBoolean(alrevindnotice.address.current), "Current address check", false);
							TryRead(() => alertreviewindnoc.UnDeclaredAddressType = (int)alrevindnotice.address.undeclaredaddresstype, "Type of undeclared address", false);
							TryRead(() => alertreviewindnoc.AddressValue = alrevindnotice.address.Value, "Address value related to notice against an alert review individual", false);
							alertreviewind.ReviewAlertIndividualNocs.Add(alertreviewindnoc);
						}
					}, "Alert review individual Notices");

					tpdata.ReviewAlertIndividuals.Add(alertreviewind);
				}
			}, "Alert review individuas");


			TryRead(() => {
				foreach (var CrScD in thirdparty.review.creditscores) {
					var creditscoresaldec = new CallCreditDataTpdDecisionCreditScores();

					var credscorealdec = CrScD;

					TryRead(() => creditscoresaldec.score = (int)credscorealdec.score.Value, "Score value (Alert Decision)", false);
					TryRead(() => creditscoresaldec.ScoreClass = (int)credscorealdec.score.@class, "Class of scorecard used (Alert Decision)", false);
					TryRead(() => creditscoresaldec.Reason1 = (int)credscorealdec.reasons[0], "reason 1 code for the credit score (Alert Decision)", false);
					TryRead(() => creditscoresaldec.Reason2 = (int)credscorealdec.reasons[1], "reason 2 code for the credit score (Alert Decision)", false);
					TryRead(() => creditscoresaldec.Reason3 = (int)credscorealdec.reasons[2], "reason 3 code for the credit score (Alert Decision)", false);
					TryRead(() => creditscoresaldec.Reason4 = (int)credscorealdec.reasons[3], "reason 4 code for the credit score (Alert Decision)", false);
					tpdata.DecisionCreditScores.Add(creditscoresaldec);
				}
			}, "Summarised Credit Scores for Alert Decision Individuals");

			TryRead(() => {
				foreach (var CrScH in thirdparty.hho.creditscores) {
					var creditscoreshho = new CallCreditDataTpdHhoCreditScores();

					var credscorealhho = CrScH;

					TryRead(() => creditscoreshho.score = (int)credscorealhho.score.Value, "Score value (HHO)", false);
					TryRead(() => creditscoreshho.ScoreClass = (int)credscorealhho.score.@class, "Class of scorecard used (HHO)", false);
					TryRead(() => creditscoreshho.Reason1 = (int)credscorealhho.reasons[0], "reason 1 code for the credit score (HHO)", false);
					TryRead(() => creditscoreshho.Reason2 = (int)credscorealhho.reasons[1], "reason 2 code for the credit score (HHO)", false);
					TryRead(() => creditscoreshho.Reason3 = (int)credscorealhho.reasons[2], "reason 3 code for the credit score (HHO)", false);
					TryRead(() => creditscoreshho.Reason4 = (int)credscorealhho.reasons[3], "reason 4 code for the credit score (HHO)", false);
					tpdata.HhoCreditScores.Add(creditscoreshho);
				}
			}, "Summarised Credit Scores for Household Override Individuals");

			Tpd.Add(tpdata);
			

			return Tpd;
		}

	}
}
