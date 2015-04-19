﻿namespace Ezbob.Backend.Strategies.CallCreditStrategy {
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;
using CallCreditLib;
using Callcredit.CRBSB;
using Ezbob.Database;
using System.Xml.Serialization;
using Ezbob.Backend.ModelsWithDB;
using Ezbob.Logger;

	public class ParseCallCredit : AStrategy {

		public ParseCallCredit(CallCredit aa, long nServiceLogID)
        {
			Result = null;
			Intro = aa;
			m_nServiceLogID = nServiceLogID;
		}// constructor

		public override string Name {
			get { return "ParseCallCredit"; }
		}//name

		public override void Execute() {
			Log.Info("Parsing CallCredit for service log entry {0}...", m_nServiceLogID);
			try {
				//var loaded = Load();
				//var parsed = Parse(loaded);
				var oTbl = Save(Intro);

				if (oTbl != null)
					Result = oTbl;
			} 
			catch (Exception) {
				Log.Error("Parsing CallCredit for service log entry {0} failed.", m_nServiceLogID);
			}

			Log.Info("Parsing CallCredit for service log entry {0} complete.", m_nServiceLogID);
		}// Execute

		public CallCredit Result { get; private set; }
		public CallCredit Intro { get; set; }

		private readonly long m_nServiceLogID;

		private Tuple<CT_SearchResult, ServiceLog> Load() {
			SafeReader sr = DB.GetFirst(
				"LoadServiceLogEntry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@EntryID", m_nServiceLogID)
			);
			var serviceLog = sr.Fill<ServiceLog>();
			if (serviceLog.Id != m_nServiceLogID) {
				Log.Info("Parsing CallCredit for service log entry {0} failed: entry not found.", m_nServiceLogID);
				return null;
			} // if

			try{
				var outputRootSerializer = new XmlSerializer(typeof(CT_SearchResult));
				var outputRoot = (CT_SearchResult)outputRootSerializer.Deserialize(new StringReader(serviceLog.ResponseData));
				if (outputRoot == null) {
					Log.Alert("Parsing CallCredit Consumer for service log entry {0} failed root element is null.", m_nServiceLogID);
					return null;
				}
				return new Tuple<CT_SearchResult, ServiceLog>(outputRoot, serviceLog);
			} 
			catch (Exception e) {
				Log.Alert(e, "Parsing CallCredit for service log entry {0} failed.", m_nServiceLogID);
				return null;
			}
		}// Load

	   //	 private Tuple<CT_SearchResult, Servicelog> TestLoad()
	   //{
	   //	var outputRootSerializer = new XmlSerializer(typeof(CT_SearchResult), new XmlRootAttribute("xmlresponse"));
	   //	var outputRoot = (CT_SearchResult)outputRootSerializer.Deserialize(new StringReader(response));
	   //	return new Tuple<CT_SearchResult, Servicelog>(outputRoot, servicelog");
	   //}

		public CallCredit Parse(Tuple<CT_SearchResult, ServiceLog> oDoc) {
			if (oDoc == null || oDoc.Item1 == null)
				return null;

			Log.Info("Parsing CallCredit company data...");
			var builder = new CallCreditModelBuilder();
			return builder.Build(oDoc.Item1, oDoc.Item2.CustomerId, oDoc.Item2.DirectorId, oDoc.Item2.InsertDate, oDoc.Item2.Id);
		} // Parse

		public CallCredit Save(CallCredit data) {
			if (data == null)
				return null;

			Log.Info("Saving CallCredit data into DB...");

			var con = DB.GetPersistent();

			con.BeginTransaction();

			try {


				var arg = DB.CreateTableParameter<CallCredit>("Tbl", new List<CallCredit> { data });


				Log.Debug("\n\n\n\n\n\n\n\n\n\nSaveCallCredit - begin: {0}\n\n\n\n\n\n\n\n\n\n", data.Error.Length);

				long CallCreditID = DB.ExecuteScalar<long>(con, "SaveCallCredit", CommandSpecies.StoredProcedure, arg);

				Log.Debug("\n\n\n\n\n\n\n\n\n\nSaveCallCredit - end, new id = {0}\n\n\n\n\n\n\n\n\n\n", CallCreditID);

				if (data.ApplicantData.Any()) 
					SaveCallCreditData(data.ApplicantData, CallCreditID, con);
				
				if (data.Amendments.Any()) 
					SaveCallCreditAmendments(data.Amendments, CallCreditID, con);
				
				if (data.ApplicantAddresses.Any()) 
					SaveCallCreditApplicantAddresses(data.ApplicantAddresses, CallCreditID, con);
				
				if (data.ApplicantNames.Any()) 
					SaveCallCreditApplicantNames(data.ApplicantNames, CallCreditID, con);
				
				if (data.Email.Any()) 
					SaveCallCreditEmail(data.Email, CallCreditID, con);
				
				if (data.Telephone.Any()) 
					SaveCallCreditTelephone(data.Telephone, CallCreditID, con);
				
			} 


			catch (Exception ex) {
				Log.Warn(ex, "Failed to save CallCredit data");
				con.Rollback();
				return null;
			}

			con.Commit();
			Log.Info("Saving CallCredit data into DB complete");
			return data;
		} // Save

		private void SaveCallCreditAmendments(List<CallCreditAmendments> amendments, long callcreditid, ConnectionWrapper con) {

			foreach (var amend in amendments) {
				amend.CallCreditID = callcreditid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditAmendments", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditAmendments>("Tbl", amendments));
		}

		private void SaveCallCreditApplicantAddresses(List<CallCreditApplicantAddresses> appaddresses, long callcreditid, ConnectionWrapper con) {

			foreach (var apaddr in appaddresses) {
				apaddr.CallCreditID = callcreditid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditApplicantAddresses", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditApplicantAddresses>("Tbl", appaddresses));
		}

		private void SaveCallCreditApplicantNames(List<CallCreditApplicantNames> appnames, long callcreditid, ConnectionWrapper con) {

			foreach (var apname in appnames) {
				apname.CallCreditID = callcreditid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditApplicantNames", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditApplicantNames>("Tbl", appnames));
		}

		private void SaveCallCreditEmail(List<CallCreditEmail> emails, long callcreditid, ConnectionWrapper con) {

			foreach (var emailaddress in emails) {
				emailaddress.CallCreditID = callcreditid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditEmail", CommandSpecies.StoredProcedure,
				DB.CreateTableParameter<CallCreditEmail>("Tbl", emails));
		}

		private void SaveCallCreditTelephone(List<CallCreditTelephone> telephones, long callcreditid, ConnectionWrapper con) {

			foreach (var phone in telephones) {
				phone.CallCreditID = callcreditid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditTelephone", CommandSpecies.StoredProcedure,
				DB.CreateTableParameter<CallCreditTelephone>("Tbl", telephones));
		}

		private void SaveCallCreditData(List<CallCreditData> appdata, long callcreditid, ConnectionWrapper con) {

			foreach (var apdat in appdata) {
				apdat.CallCreditID = callcreditid;

				var CallCrediDataID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditData",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditData>("Tbl", new List<CallCreditData> { apdat }));

				if (apdat.Accs.Any()) 
					SaveCallCreditDataAccs(apdat.Accs, CallCrediDataID, con);
				
				if (apdat.AddressConfs.Any()) 
					SaveCallCreditDataAddressConfs(apdat.AddressConfs, CallCrediDataID, con);
				
				if (apdat.SummaryAddresses.Any()) 
					SaveCallCreditDataAddresses(apdat.SummaryAddresses, CallCrediDataID, con);
				
				if (apdat.AddressLinks.Any()) 
					SaveCallCreditDataAddressLinks(apdat.AddressLinks, CallCrediDataID, con);
				
				if (apdat.AliasLinks.Any())
					SaveCallCreditDataAliasLinks(apdat.AliasLinks, CallCrediDataID, con);
				
				if (apdat.AssociateLinks.Any()) 
					SaveCallCreditDataAssociateLinks(apdat.AssociateLinks, CallCrediDataID, con);
				
				if (apdat.CifasFiling.Any()) 
					SaveCallCreditDataCifasFiling(apdat.CifasFiling, CallCrediDataID, con);
				
				if (apdat.CifasPlusCases.Any()) 
					SaveCallCreditDataCifasPlusCases(apdat.CifasPlusCases, CallCrediDataID, con);
				
				if (apdat.CreditScores.Any()) 
					SaveCallCreditDataCreditScores(apdat.CreditScores, CallCrediDataID, con);
				
				if (apdat.Judgments.Any()) 
					SaveCallCreditDataJudgments(apdat.Judgments, CallCrediDataID, con);
				
				if (apdat.LinkAddresses.Any()) 
					SaveCallCreditDataLinkAddresses(apdat.LinkAddresses, CallCrediDataID, con);
				
				if (apdat.Nocs.Any()) 
					SaveCallCreditDataNocs(apdat.Nocs, CallCrediDataID, con);
				
				if (apdat.Rtr.Any()) 
					SaveCallCreditDataRtr(apdat.Rtr, CallCrediDataID, con);
				
				if (apdat.Searches.Any()) 
					SaveCallCreditDataSearches(apdat.Searches, CallCrediDataID, con);
				
				if (apdat.Tpd.Any()) 
					SaveCallCreditDataTpd(apdat.Tpd, CallCrediDataID, con);
			}
		} // SaveCallCreditData

		private void SaveCallCreditDataAddresses(List<CallCreditDataAddresses> sumaddresses, long callcreditdataid, ConnectionWrapper con) {

			foreach (var addr in sumaddresses) {
				addr.CallCreditDataID = callcreditdataid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataAddresses", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataAddresses>("Tbl", sumaddresses));
		}

		private void SaveCallCreditDataCreditScores(List<CallCreditDataCreditScores> creditscores, long callcreditdataid, ConnectionWrapper con) {

			foreach (var score in creditscores) {
				score.CallCreditDataID = callcreditdataid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataCreditScores", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataCreditScores>("Tbl", creditscores));
		}

		private void SaveCallCreditDataLinkAddresses(List<CallCreditDataLinkAddresses> linkaddresses, long callcreditdataid, ConnectionWrapper con) {

			foreach (var linkaddr in linkaddresses) {
				linkaddr.CallCreditDataID = callcreditdataid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataLinkAddresses", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataLinkAddresses>("Tbl", linkaddresses));
		}

		private void SaveCallCreditDataNocs(List<CallCreditDataNocs> nocs, long callcreditdataid, ConnectionWrapper con) {

			foreach (var notice in nocs) {
				notice.CallCreditDataID = callcreditdataid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataNocs", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataNocs>("Tbl", nocs));
		}

		private void SaveCallCreditDataSearches(List<CallCreditDataSearches> searches, long callcreditdataid, ConnectionWrapper con) {

			foreach (var seek in searches) {
				seek.CallCreditDataID = callcreditdataid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataSearches", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataSearches>("Tbl", searches));
		}

		private void SaveCallCreditDataAccs(List<CallCreditDataAccs> accsdata, long callcreditdataid, ConnectionWrapper con) {

			foreach (var acc in accsdata) {
				acc.CallCreditDataID = callcreditdataid;

				var CallCrediDataAccsID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataAccs",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataAccs>("Tbl", new List<CallCreditDataAccs> { acc }));

				foreach (var acchist in acc.AccHistory) {
					acchist.CallCreditDataAccsID = CallCrediDataAccsID;
				}

				if (acc.AccHistory.Any()) {
					var arg = DB.CreateTableParameter<CallCreditDataAccsHistory>("Tbl", acc.AccHistory);
					DB.ExecuteNonQuery(con, "SaveCallCreditDataAccsHistory", CommandSpecies.StoredProcedure, arg);
				} // if

				foreach (var accnotice in acc.AccNocs) {
					accnotice.CallCreditDataAccsID = CallCrediDataAccsID;
				}

				if (acc.AccNocs.Any())
					DB.ExecuteNonQuery(con, "SaveCallCreditDataAccsNocs", CommandSpecies.StoredProcedure, DB.CreateTableParameter<CallCreditDataAccsNocs>("Tbl", acc.AccNocs));
			}
		}

		private void SaveCallCreditDataAddressLinks(List<CallCreditDataAddressLinks> addresslinks, long callcreditdataid, ConnectionWrapper con) {

			foreach (var addrlink in addresslinks) {
				addrlink.CallCreditDataID = callcreditdataid;

				var CallCreditDataAddressLinksID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataAddressLinks",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataAddressLinks>("Tbl", new List<CallCreditDataAddressLinks> { addrlink }));

				foreach (var addrlinknotice in addrlink.AddressLinkNocs) {
					addrlinknotice.CallCreditDataAddressLinksID = CallCreditDataAddressLinksID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataAddressLinksNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataAddressLinksNocs>("Tbl", addrlink.AddressLinkNocs));
			}
		}

		private void SaveCallCreditDataAliasLinks(List<CallCreditDataAliasLinks> aliaslinks, long callcreditdataid, ConnectionWrapper con) {

			foreach (var aliaslink in aliaslinks) {
				aliaslink.CallCreditDataID = callcreditdataid;

				var CallCreditDataAliasLinksID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataAliasLinks",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataAliasLinks>("Tbl", new List<CallCreditDataAliasLinks> { aliaslink }));

				foreach (var aliaslinknotice in aliaslink.AliasLinkNocs) {
					aliaslinknotice.CallCreditDataAliasLinksID = CallCreditDataAliasLinksID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataAliasLinksNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataAliasLinksNocs>("Tbl", aliaslink.AliasLinkNocs));
			}
		}

		private void SaveCallCreditDataAssociateLinks(List<CallCreditDataAssociateLinks> associatelinks, long callcreditdataid, ConnectionWrapper con) {

			foreach (var asslink in associatelinks) {
				asslink.CallCreditDataID = callcreditdataid;

				var CallCreditDataAssociateLinksID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataAssociateLinks",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataAssociateLinks>("Tbl", new List<CallCreditDataAssociateLinks> { asslink }));

				foreach (var asslinknotice in asslink.AssociateLinkNocs) {
					asslinknotice.CallCreditDataAssociateLinksID = CallCreditDataAssociateLinksID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataAssociateLinksNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataAssociateLinksNocs>("Tbl", asslink.AssociateLinkNocs));
			}
		}

		private void SaveCallCreditDataCifasFiling(List<CallCreditDataCifasFiling> cifasfilingdata, long callcreditdataid, ConnectionWrapper con) {

			foreach (var cifasfiling in cifasfilingdata) {
				cifasfiling.CallCreditDataID = callcreditdataid;

				var CallCreditDataCifasFilingID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataCifasFiling",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataCifasFiling>("Tbl", new List<CallCreditDataCifasFiling> { cifasfiling }));

				foreach (var cifasfilingnotice in cifasfiling.CifasFilingNocs) {
					cifasfilingnotice.CallCreditDataCifasFilingID = CallCreditDataCifasFilingID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataCifasFilingNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataCifasFilingNocs>("Tbl", cifasfiling.CifasFilingNocs));
			}
		}

		private void SaveCallCreditDataJudgments(List<CallCreditDataJudgments> judgments, long callcreditdataid, ConnectionWrapper con) {

			foreach (var judgment in judgments) {
				judgment.CallCreditDataID = callcreditdataid;

				var CallCreditDataJudgmentsID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataJudgments",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataJudgments>("Tbl", new List<CallCreditDataJudgments> { judgment }));

				foreach (var judgmentnotice in judgment.JudgmentNocs) {
					judgmentnotice.CallCreditDataJudgmentsID = CallCreditDataJudgmentsID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataJudgmentsNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataJudgmentsNocs>("Tbl", judgment.JudgmentNocs));
			}
		}

		private void SaveCallCreditDataRtr(List<CallCreditDataRtr> rtreports, long callcreditdataid, ConnectionWrapper con) {

			foreach (var rtr in rtreports) {
				rtr.CallCreditDataID = callcreditdataid;

				var CallCreditDataRtrID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataRtr",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataRtr>("Tbl", new List<CallCreditDataRtr> { rtr }));

				foreach (var rtrnotice in rtr.RtrNocs) {
					rtrnotice.CallCreditDataRtrID = CallCreditDataRtrID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataRtrNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataRtrNocs>("Tbl", rtr.RtrNocs));
			}
		}

		private void SaveCallCreditDataAddressConfs(List<CallCreditDataAddressConfs> addconfs, long callcreditdataid, ConnectionWrapper con) {

			foreach (var addconf in addconfs) {
				addconf.CallCreditDataID = callcreditdataid;

				var CallCrediDataAddressConfsID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataAddressConfs",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataAddressConfs>("Tbl", new List<CallCreditDataAddressConfs> { addconf }));

				foreach (var resident in addconf.Residents) {
					resident.CallCreditDataAddressConfsID = CallCrediDataAddressConfsID;

					var CallCreditDataAddressConfsResidentsID = DB.ExecuteScalar<long>(
						con,
						"SaveCallCreditDataAddressConfsResidents",
						CommandSpecies.StoredProcedure,
						DB.CreateTableParameter<CallCreditDataAddressConfsResidents>("Tbl", new List<CallCreditDataAddressConfsResidents> { resident }));

					if (resident.ErHistory.Any())
						SaveCallCreditDataAddressConfsResidentsErHistory(resident.ErHistory, CallCreditDataAddressConfsResidentsID, con);

					if (resident.ResidentNocs.Any())
						SaveCallCreditDataAddressConfsResidentsNocs(resident.ResidentNocs, CallCreditDataAddressConfsResidentsID, con);
				}
			}
		}

		private void SaveCallCreditDataAddressConfsResidentsNocs(List<CallCreditDataAddressConfsResidentsNocs> residentnocs, long callcreditdataaddressconfsresidentsid, ConnectionWrapper con) {

			foreach (var resnotice in residentnocs) {
				resnotice.CallCreditDataAddressConfsResidentsID = callcreditdataaddressconfsresidentsid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataAddressConfsResidentsNocs", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataAddressConfsResidentsNocs>("Tbl", residentnocs));
		}

		private void SaveCallCreditDataAddressConfsResidentsErHistory(List<CallCreditDataAddressConfsResidentsErHistory> erhistorydata, long callcreditdataaddressconfsresidentsid, ConnectionWrapper con) {

			foreach (var erhist in erhistorydata) {
				erhist.CallCreditDataAddressConfsResidentsID = callcreditdataaddressconfsresidentsid;

				var CallCreditDataAddressConfsResidentsErHistoryID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataAddressConfsResidentsErHistory",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataAddressConfsResidentsErHistory>("Tbl", new List<CallCreditDataAddressConfsResidentsErHistory> { erhist }));

				foreach (var erhistnotice in erhist.ErHistoryNocs) {
					erhistnotice.CallCreditDataAddressConfsResidentsErHistoryID = CallCreditDataAddressConfsResidentsErHistoryID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataAddressConfsResidentsErHistoryNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataAddressConfsResidentsErHistoryNocs>("Tbl", erhist.ErHistoryNocs));
			}
		}

		private void SaveCallCreditDataCifasPlusCases(List<CallCreditDataCifasPlusCases> cifaspluscases, long callcreditdataid, ConnectionWrapper con) {

			foreach (var cifaspluscase in cifaspluscases) {
				cifaspluscase.CallCreditDataID = callcreditdataid;

				var CallCreditDataCifasPlusCasesID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataCifasPlusCases",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataCifasPlusCases>("Tbl", new List<CallCreditDataCifasPlusCases> { cifaspluscase }));

				if (cifaspluscase.Dmrs.Any())
					SaveCallCreditDataCifasPlusCasesDmrs(cifaspluscase.Dmrs, CallCreditDataCifasPlusCasesID, con);

				if (cifaspluscase.FilingReasons.Any())
					SaveCallCreditDataCifasPlusCasesFilingReasons(cifaspluscase.FilingReasons, CallCreditDataCifasPlusCasesID, con);

				if (cifaspluscase.CifasPlusCaseNocs.Any())
					SaveCallCreditDataCifasPlusCasesNocs(cifaspluscase.CifasPlusCaseNocs, CallCreditDataCifasPlusCasesID, con);

				if (cifaspluscase.Subjects.Any())
					SaveCallCreditDataCifasPlusCasesSubjects(cifaspluscase.Subjects, CallCreditDataCifasPlusCasesID, con);
			}
		} // CallCreditDataCifasPlusCases

		private void SaveCallCreditDataCifasPlusCasesDmrs(List<CallCreditDataCifasPlusCasesDmrs> cifaspluscasesdmrs, long callcreditdatacifaspluscasesid, ConnectionWrapper con) {

			foreach (var dmr in cifaspluscasesdmrs) {
				dmr.CallCreditDataCifasPlusCasesID = callcreditdatacifaspluscasesid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataCifasPlusCasesDmrs", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataCifasPlusCasesDmrs>("Tbl", cifaspluscasesdmrs));
		}

		private void SaveCallCreditDataCifasPlusCasesFilingReasons(List<CallCreditDataCifasPlusCasesFilingReasons> filingreasons, long callcreditdatacifaspluscasesid, ConnectionWrapper con) {

			foreach (var filingreason in filingreasons) {
				filingreason.CallCreditDataCifasPlusCasesID = callcreditdatacifaspluscasesid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataCifasPlusCasesFilingReasons", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataCifasPlusCasesFilingReasons>("Tbl", filingreasons));
		}

		private void SaveCallCreditDataCifasPlusCasesNocs(List<CallCreditDataCifasPlusCasesNocs> cifaspluscasesnocs, long callcreditdatacifaspluscasesid, ConnectionWrapper con) {

			foreach (var cifaspluscasenotice in cifaspluscasesnocs) {
				cifaspluscasenotice.CallCreditDataCifasPlusCasesID = callcreditdatacifaspluscasesid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataCifasPlusCasesNocs", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataCifasPlusCasesNocs>("Tbl", cifaspluscasesnocs));
		}

		private void SaveCallCreditDataCifasPlusCasesSubjects(List<CallCreditDataCifasPlusCasesSubjects> subjects, long callcreditdatacifaspluscasesid, ConnectionWrapper con) {

			foreach (var subject in subjects) {
				subject.CallCreditDataCifasPlusCasesID = callcreditdatacifaspluscasesid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataCifasPlusCasesSubjects", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataCifasPlusCasesSubjects>("Tbl", subjects));
		}

		private void SaveCallCreditDataTpd(List<CallCreditDataTpd> thirdpartydata, long callcreditdataid, ConnectionWrapper con) {

			foreach (var tpd in thirdpartydata) {
				tpd.CallCreditDataID = callcreditdataid;

				var CallCreditDataTpdID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataTpd",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataTpd>("Tbl", new List<CallCreditDataTpd> { tpd }));

				if (tpd.DecisionAlertIndividuals.Any())
					SaveCallCreditDataTpdDecisionAlertIndividuals(tpd.DecisionAlertIndividuals, CallCreditDataTpdID, con);

				if (tpd.ReviewAlertIndividuals.Any())
					SaveCallCreditDataTpdReviewAlertIndividuals(tpd.ReviewAlertIndividuals, CallCreditDataTpdID, con);

				if (tpd.DecisionCreditScores.Any())
					SaveCallCreditDataTpdDecisionCreditScores(tpd.DecisionCreditScores, CallCreditDataTpdID, con);

				if (tpd.HhoCreditScores.Any())
					SaveCallCreditDataTpdHhoCreditScores(tpd.HhoCreditScores, CallCreditDataTpdID, con);
			}
		} // CallCreditDataTpd

		private void SaveCallCreditDataTpdDecisionCreditScores(List<CallCreditDataTpdDecisionCreditScores> decalcreditscores, long callcreditdatatpdid, ConnectionWrapper con) {

			foreach (var decalscore in decalcreditscores) {
				decalscore.CallCreditDataTpdID = callcreditdatatpdid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataTpdDecisionCreditScores", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataTpdDecisionCreditScores>("Tbl", decalcreditscores));
		}

		private void SaveCallCreditDataTpdHhoCreditScores(List<CallCreditDataTpdHhoCreditScores> hhocreditscores, long callcreditdatatpdid, ConnectionWrapper con) {

			foreach (var hhoscore in hhocreditscores) {
				hhoscore.CallCreditDataTpdID = callcreditdatatpdid;
			}
			DB.ExecuteNonQuery(con, "SaveCallCreditDataTpdHhoCreditScores", CommandSpecies.StoredProcedure,
					   DB.CreateTableParameter<CallCreditDataTpdHhoCreditScores>("Tbl", hhocreditscores));
		}

		private void SaveCallCreditDataTpdDecisionAlertIndividuals(List<CallCreditDataTpdDecisionAlertIndividuals> decalertinds, long callcreditdatatpdid, ConnectionWrapper con) {

			foreach (var decalind in decalertinds) {
				decalind.CallCreditDataTpdID = callcreditdatatpdid;

				var CallCreditDataTpdDecisionAlertIndividualsID = DB.ExecuteScalar<long>(
					con,
					"SaveCCallCreditDataTpdReviewAlertIndividuals",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataTpdDecisionAlertIndividuals>("Tbl", new List<CallCreditDataTpdDecisionAlertIndividuals> { decalind }));

				foreach (var rtrnotice in decalind.DecisionAlertIndividualNocs) {
					rtrnotice.CallCreditDataTpdDecisionAlertIndividualsID = CallCreditDataTpdDecisionAlertIndividualsID;
				}
				DB.ExecuteNonQuery(con, "SaveCCallCreditDataTpdReviewAlertIndividualsNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataTpdDecisionAlertIndividualsNocs>("Tbl", decalind.DecisionAlertIndividualNocs));
			}
		}

		private void SaveCallCreditDataTpdReviewAlertIndividuals(List<CallCreditDataTpdReviewAlertIndividuals> revalertinds, long callcreditdatatpdid, ConnectionWrapper con) {

			foreach (var revalind in revalertinds) {
				revalind.CallCreditDataTpdID = callcreditdatatpdid;

				var CallCreditDataTpdReviewAlertIndividualsID = DB.ExecuteScalar<long>(
					con,
					"SaveCallCreditDataTpdReviewAlertIndividuals",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<CallCreditDataTpdReviewAlertIndividuals>("Tbl", new List<CallCreditDataTpdReviewAlertIndividuals> { revalind }));

				foreach (var rtrnotice in revalind.ReviewAlertIndividualNocs) {
					rtrnotice.CallCreditDataTpdReviewAlertIndividualsID = CallCreditDataTpdReviewAlertIndividualsID;
				}
				DB.ExecuteNonQuery(con, "SaveCallCreditDataTpdReviewAlertIndividualsNocs", CommandSpecies.StoredProcedure,
						   DB.CreateTableParameter<CallCreditDataTpdReviewAlertIndividualsNocs>("Tbl", revalind.ReviewAlertIndividualNocs));
			}
		}
	}
}

