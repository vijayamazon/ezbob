namespace ExperianLib.Ebusiness {
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using ConfigManager;
	using EBusiness;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using EzServiceAccessor;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using StructureMap;

	public class EBusinessService {

		public EBusinessService(AConnection oDB) {
			m_oRetryer = new SqlRetryer(oLog: ms_oLog);
			eSeriesUrl = CurrentValues.Instance.ExperianESeriesUrl;
			nonLimitedParser = new NonLimitedParser();

			m_oDB = oDB;
		} // constructor

		public TargetResults TargetBusiness(string companyName, string postCode, int customerId, TargetResults.LegalStatus nFilter, string regNum = "") {
			try {
				companyName = HttpUtility.HtmlEncode(companyName);
				string isLimited = nFilter != TargetResults.LegalStatus.NonLimited ? "Y" : "N";
				string isNonLimited = nFilter != TargetResults.LegalStatus.Limited ? "Y" : "N";

				string requestXml = GetResource(
					"ExperianLib.Ebusiness.TargetBusiness.xml",
					companyName,
					postCode,
					regNum,
					isNonLimited,
					isLimited
				);

				string response = MakeRequest(requestXml);

				Utils.WriteLog(requestXml, response, ExperianServiceType.Targeting, customerId, companyRefNum: regNum);

				return new TargetResults(response);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Target business failed.");
				throw;
			} // try
		} // TargetBusiness

		public LimitedResults GetLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			ms_oLog.Debug("Begin GetLimitedBusinessData({0}, {1}, {2}, {3})...", regNumber, customerId, checkInCacheOnly, forceCheck);

			LimitedResults oRes = GetOneLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);

			if (oRes == null) {
				ms_oLog.Debug("End GetLimitedBusinessData({0}, {1}, {2}, {3}): result is null.", regNumber, customerId, checkInCacheOnly, forceCheck);
				return null;
			} // if

			oRes.MaxBureauScore = oRes.BureauScore;

			ms_oLog.Info("Fetched BureauScore: {0}, Calculated MaxBureauScore: {1} for customer: {2} and regNum: {3}.", oRes.BureauScore, oRes.MaxBureauScore, customerId, regNumber);

			ms_oLog.Debug("GetLimitedBusinessData({0}, {1}, {2}, {3}): traversing owners...", regNumber, customerId, checkInCacheOnly, forceCheck);

			foreach (string sOwnerRegNum in oRes.Owners) {
				ms_oLog.Debug(
					"GetLimitedBusinessData({0}, {1}, {2}, {3}): current owner reg num is '{4}'.",
					regNumber, customerId, checkInCacheOnly, forceCheck, sOwnerRegNum
				);

				LimitedResults parentCompanyResult = GetOneLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly, forceCheck);

				if (parentCompanyResult == null)
					continue;

				if (parentCompanyResult.BureauScore > oRes.MaxBureauScore)
					oRes.MaxBureauScore = parentCompanyResult.BureauScore;

				ms_oLog.Info("Fetched BureauScore: {0}, Calculated MaxBureauScore: {1} for customer: {2} and regNum: {3}.", parentCompanyResult.BureauScore, oRes.MaxBureauScore, customerId, sOwnerRegNum);
			} // for each

			ms_oLog.Debug("GetLimitedBusinessData({0}, {1}, {2}, {3}): traversing owners complete.", regNumber, customerId, checkInCacheOnly, forceCheck);

			ms_oLog.Debug("End GetLimitedBusinessData({0}, {1}, {2}, {3}).", regNumber, customerId, checkInCacheOnly, forceCheck);

			return oRes;
		} // GetLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			var oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);
			if (oRes == null)
			{
				ms_oLog.Warn("Failed fetching non limited data. Customer:{0} RefNumber:{1}", customerId, regNumber);
				return null;
			}

			oRes.MaxBureauScore = oRes.BureauScore;
			ms_oLog.Info("Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}", oRes.BureauScore, oRes.MaxBureauScore, customerId, regNumber);
			return oRes;
		} // GetNotLimitedBusinessData

		public CompanyInfo TargetCache(int customerId, string refNumber) {
			return m_oRetryer.Retry(() => {
				var repo = ObjectFactory.GetInstance<ServiceLogRepository>();
				IQueryable<MP_ServiceLog> oCachedValues = repo.GetAll().Where(c => c.Customer != null && c.Customer.Id == customerId && c.ServiceType == "ESeriesTargeting");

				foreach (var oVal in oCachedValues)
				{
					var targets = new TargetResults(oVal.ResponseData);

					foreach (var target in targets.Targets)
						if (target.BusRefNum == refNumber)
							return target;
				} // for each cached value

				return null;
			}, "EBusinessService.TargetCache(" + customerId + ", " + refNumber + ")");
		} // TargetCache

		private LimitedResults GetOneLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			if (string.IsNullOrWhiteSpace(regNumber))
				return null;

			try {
				ExperianLtd oExperianLtd = null;
				bool bCacheHit = false;

				if (forceCheck)
					oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);

				if (oExperianLtd == null) {
					oExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().CheckLtdCompanyCache(1, regNumber);

					if ((oExperianLtd == null) || (oExperianLtd.ID == 0)) {
						oExperianLtd = null;

						ms_oLog.Debug(
							"GetOneLimitedBusinessData({0}, {1}, {2}, {3}): no data found in cache.",
							regNumber, customerId, checkInCacheOnly, forceCheck
						);

						if (!checkInCacheOnly)
							oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);
					}
					else {
						if ((DateTime.UtcNow - oExperianLtd.ReceivedTime).TotalDays > CurrentValues.Instance.UpdateCompanyDataPeriodDays) {
							oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);

							if ((oExperianLtd == null) || (oExperianLtd.ID == 0)) {
								oExperianLtd = null;

								ms_oLog.Debug(
									"GetOneLimitedBusinessData({0}, {1}, {2}, {3}): no data downloaded nor data found in cache.",
									regNumber, customerId, checkInCacheOnly, forceCheck
								);
							}
						}
						else
							bCacheHit = true;
					}
				} // if

				ms_oLog.Debug(
					"GetOneLimitedBusinessData({0}, {1}, {2}, {3}) = (cache hit: {4}):\n{5}",
					regNumber, customerId, checkInCacheOnly, forceCheck, bCacheHit,
					oExperianLtd == null ? "-- null --" : oExperianLtd.StringifyAll()
				);

				return oExperianLtd == null ? null : new LimitedResults(oExperianLtd, bCacheHit);
			}
			catch (Exception e) {
				ms_oLog.Error(e,
					"Failed to get limited results for a company {0} and customer {1} (cache only: {2}, force: {3}).",
					regNumber, customerId,
					checkInCacheOnly ? "yes" : "no",
					forceCheck ? "yes" : "no"
				);
				return new LimitedResults(e);
			} // try
		} // GetOneLimitedBusinessData

		private ExperianLtd DownloadOneLimitedFromExperian(string regNumber, int customerId) {
			ms_oLog.Debug("Downloading data from Experian for company {0} and customer {1}...", regNumber, customerId);

			string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

			string newResponse = MakeRequest(requestXml);

			var pkg = new WriteToLogPackage(requestXml, newResponse, ExperianServiceType.LimitedData, customerId, companyRefNum: regNumber);

			Utils.WriteLog(pkg);

			ms_oLog.Debug("Downloading data from Experian for company {0} and customer {1} complete.", regNumber, customerId);

			return pkg.Out.ExperianLtd;
		} // DownloadOneLimitedFromExperian

		private bool CacheExpired(DateTime? updateDate) {
			if (!updateDate.HasValue)
				return true;

			int cacheIsValidForDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;
			return (DateTime.UtcNow - updateDate.Value).TotalDays > cacheIsValidForDays;
		} // CacheNotExpired

		private NonLimitedResults GetOneNotLimitedBusinessData(string refNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			try {
				DateTime? created = GetNonLimitedCreationTime(refNumber);

				if (forceCheck || (!checkInCacheOnly && CacheExpired(created))) {
					string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", refNumber);

					//var newResponse = MakeRequest(requestXml);
					string newResponse =
						"<?xml version='1.0' standalone='yes' ?><GEODS  exp_cid='sIeG' > <REQUEST type='BINSIGHT' subtype='CALLBUR' EXP_ExperianRef='' success='Y' timestamp='Sat, 25 Oct 2014 at 9:36 PM' id='sIeG'><DK10 seq='00'><CICSREGION>CIXF    </CICSREGION><MFDATE-YYYY>2014</MFDATE-YYYY><MFDATE-MM>10</MFDATE-MM><MFDATE-DD>25</MFDATE-DD><TIME>213622</TIME></DK10><DK12 seq='00'><ACCESSITEMSCOUNT>008</ACCESSITEMSCOUNT><ACCESSITEMS><ITEMCODE>BTAR</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>BTPC</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>BTPH</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CDEL</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CDEN</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CDE6</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>SHNA</ITEMCODE></ACCESSITEMS><ACCESSITEMS><ITEMCODE>CD3L</ITEMCODE></ACCESSITEMS></DK12><DN10 seq='01'><NONLTDKEY>10487968</NONLTDKEY><EARLIESTKNOWNDATE-YYYY>2013</EARLIESTKNOWNDATE-YYYY><EARLIESTKNOWNDATE-MM>06</EARLIESTKNOWNDATE-MM><EARLIESTKNOWNDATE-DD>12</EARLIESTKNOWNDATE-DD><DATEOWNSHPCOMMD-YYYY>2009</DATEOWNSHPCOMMD-YYYY><DATEOWNSHPCOMMD-MM>09</DATEOWNSHPCOMMD-MM><DATEOWNSHPTERMD-YYYY>2014</DATEOWNSHPTERMD-YYYY><DATEOWNSHPTERMD-MM>10</DATEOWNSHPTERMD-MM><LATESTUPDATE-YYYY>2013</LATESTUPDATE-YYYY><LATESTUPDATE-MM>06</LATESTUPDATE-MM><LATESTUPDATE-DD>14</LATESTUPDATE-DD><NUMNOCS>00</NUMNOCS><LENBUSINESSDETAILS>050</LENBUSINESSDETAILS><LENPRINCIPLEACTIVITIES>01</LENPRINCIPLEACTIVITIES><NUM1992SICCODES>01</NUM1992SICCODES><LEN1992SICDESC>001</LEN1992SICDESC><BUSINESSNAME>MR PARIMAL PATEL</BUSINESSNAME><BUSADDR1>71 ACORN WALK</BUSADDR1><BUSADDR2>LONDON</BUSADDR2><BUSPOSTCODE>SE165DY</BUSPOSTCODE><SICCODES><SICCODE1992>0000 </SICCODE1992></SICCODES><SICDESCS></SICDESCS></DN10><DN12 seq='01'><NONLTDKEY>10487968</NONLTDKEY><DATEOWNSHPCOMMD-YYYY>2009</DATEOWNSHPCOMMD-YYYY><DATEOWNSHPCOMMD-MM>09</DATEOWNSHPCOMMD-MM><DATEOWNSHPTERMD-YYYY>2014</DATEOWNSHPTERMD-YYYY><DATEOWNSHPTERMD-MM>10</DATEOWNSHPTERMD-MM><MOWNSHPRANGE>N</MOWNSHPRANGE><MBANKRUPTCYCOUNTOWNSHP>0</MBANKRUPTCYCOUNTOWNSHP><MNOCFLAG>N</MNOCFLAG><AOWNSHPRANGE>N</AOWNSHPRANGE><ABANKRUPTCYCOUNTOWNSHP>0</ABANKRUPTCYCOUNTOWNSHP><ANOCFLAG>N</ANOCFLAG></DN12><DN14 seq='01'><NONLTDKEY>10487968</NONLTDKEY><DATEOWNSHPCOMMD-YYYY>2009</DATEOWNSHPCOMMD-YYYY><DATEOWNSHPCOMMD-MM>09</DATEOWNSHPCOMMD-MM><DATEOWNSHPTERMD-YYYY>2014</DATEOWNSHPTERMD-YYYY><DATEOWNSHPTERMD-MM>10</DATEOWNSHPTERMD-MM><MOWNSHPRANGE>N</MOWNSHPRANGE><MTOTJUDGCOUNT>0</MTOTJUDGCOUNT><MTOTJUDGVALUE>0</MTOTJUDGVALUE><MAGEMOSTRECJUDGSINCEOWNSHP>0</MAGEMOSTRECJUDGSINCEOWNSHP><MVALMOSTRECJUDGSINCEOWNSHP>0</MVALMOSTRECJUDGSINCEOWNSHP><MTOTJUDGCOUNTLST6MNTHS>0</MTOTJUDGCOUNTLST6MNTHS><MTOTJUDGVALUELST6MNTHS>0</MTOTJUDGVALUELST6MNTHS><MTOTJUDGCOUNTLST12MNTHS>0</MTOTJUDGCOUNTLST12MNTHS><MTOTJUDGVALUELST12MNTHS>0</MTOTJUDGVALUELST12MNTHS><MTOTJUDGCOUNTLST24MNTHS>0</MTOTJUDGCOUNTLST24MNTHS><MTOTJUDGVALUELST24MNTHS>0</MTOTJUDGVALUELST24MNTHS><MTOTJUDGCOUNTLST13TO24MNTHS>0</MTOTJUDGCOUNTLST13TO24MNTHS><MTOTJUDGVALUELST13TO24MNTHS>0</MTOTJUDGVALUELST13TO24MNTHS><MTOTJUDGCOUNTLST25TO36MNTHS>0</MTOTJUDGCOUNTLST25TO36MNTHS><MTOTJUDGVALUELST25TO36MNTHS>0</MTOTJUDGVALUELST25TO36MNTHS><MTOTJUDGCOUNTLST37TO48MNTHS>0</MTOTJUDGCOUNTLST37TO48MNTHS><MTOTJUDGVALUELST37TO48MNTHS>0</MTOTJUDGVALUELST37TO48MNTHS><MTOTJUDGCOUNTLST49TO60MNTHS>0</MTOTJUDGCOUNTLST49TO60MNTHS><MTOTJUDGVALUELST49TO60MNTHS>0</MTOTJUDGVALUELST49TO60MNTHS><MTOTJUDGCOUNTLST61TO72MNTHS>0</MTOTJUDGCOUNTLST61TO72MNTHS><MTOTJUDGVALUELST61TO72MNTHS>0</MTOTJUDGVALUELST61TO72MNTHS><MNOCFLAG>N</MNOCFLAG><AOWNSHPRNGE>N</AOWNSHPRNGE><ATOTJUDGCOUNT>0</ATOTJUDGCOUNT><ATOTJUDGVALUE>0</ATOTJUDGVALUE><AAGEMOSTRECJUDGSINCEOWNSHP>0</AAGEMOSTRECJUDGSINCEOWNSHP><AVALMOSTRECJUDGSINCEOWNSHP>0</AVALMOSTRECJUDGSINCEOWNSHP><ATOTJUDGCOUNTLST6MNTHS>0</ATOTJUDGCOUNTLST6MNTHS><ATOTJUDGVALUELST6MNTHS>0</ATOTJUDGVALUELST6MNTHS><ATOTJUDGCOUNTLST12MNTHS>0</ATOTJUDGCOUNTLST12MNTHS><ATOTJUDGVALUELST12MNTHS>0</ATOTJUDGVALUELST12MNTHS><ATOTJUDGCOUNTLST24MNTHS>0</ATOTJUDGCOUNTLST24MNTHS><ATOTJUDGVALUELST24MNTHS>0</ATOTJUDGVALUELST24MNTHS><ATOTJUDGCOUNTLST13TO24MNTHS>0</ATOTJUDGCOUNTLST13TO24MNTHS><ATOTJUDGVALUELST13TO24MNTHS>0</ATOTJUDGVALUELST13TO24MNTHS><ATOTJUDGCOUNTLST25TO36MNTHS>0</ATOTJUDGCOUNTLST25TO36MNTHS><ATOTJUDGVALUELST25TO36MNTHS>0</ATOTJUDGVALUELST25TO36MNTHS><ATOTJUDGCOUNTLST37TO48MNTHS>0</ATOTJUDGCOUNTLST37TO48MNTHS><ATOTJUDGVALUELST37TO48MNTHS>0</ATOTJUDGVALUELST37TO48MNTHS><ATOTJUDGCOUNTLST49TO60MNTHS>0</ATOTJUDGCOUNTLST49TO60MNTHS><ATOTJUDGVALUELST49TO60MNTHS>0</ATOTJUDGVALUELST49TO60MNTHS><ATOTJUDGCOUNTLST61TO72MNTHS>0</ATOTJUDGCOUNTLST61TO72MNTHS><ATOTJUDGVALUELST61TO72MNTHS>0</ATOTJUDGVALUELST61TO72MNTHS><ANOCFLAG>N</ANOCFLAG></DN14><DN16 seq='01'><NONLTDKEY>10487968</NONLTDKEY><DATEOWNSHPCOMMD-YYYY>2009</DATEOWNSHPCOMMD-YYYY><DATEOWNSHPCOMMD-MM>09</DATEOWNSHPCOMMD-MM><DATEOWNSHPTERMD-YYYY>2014</DATEOWNSHPTERMD-YYYY><DATEOWNSHPTERMD-MM>10</DATEOWNSHPTERMD-MM><MAINDATAOSOWNSHPRNGE>N</MAINDATAOSOWNSHPRNGE><MNUMCCLS>0</MNUMCCLS><MNOCFLAG>N</MNOCFLAG><ASSOCDATAOSOWNSHPRNGE>N</ASSOCDATAOSOWNSHPRNGE><ANUMCCLS>0</ANUMCCLS><ANOCFLAG>N</ANOCFLAG></DN16><DN18 seq='01'><NONLTDKEY>10487968</NONLTDKEY><DATEOWNSHPCOMMD-YYYY>2009</DATEOWNSHPCOMMD-YYYY><DATEOWNSHPCOMMD-MM>09</DATEOWNSHPCOMMD-MM><DATEOWNSHPTERMD-YYYY>2014</DATEOWNSHPTERMD-YYYY><DATEOWNSHPTERMD-MM>10</DATEOWNSHPTERMD-MM><DATAOSOWNSHPRNGE>N</DATAOSOWNSHPRNGE><NUMPREVSEARCHES3MTHS>0</NUMPREVSEARCHES3MTHS><NUMPREVSEARCHES6MTHS>0</NUMPREVSEARCHES6MTHS><NUMPREVSEARCHES12MTHS>0</NUMPREVSEARCHES12MTHS><NOCFLAG>N</NOCFLAG></DN18><DN25 seq='01'><NONLTDKEY>10487968</NONLTDKEY><DATEOWNSHPCOMMD-YYYY>2009</DATEOWNSHPCOMMD-YYYY><DATEOWNSHPCOMMD-MM>09</DATEOWNSHPCOMMD-MM><DATEOWNSHPTERMD-YYYY>2014</DATEOWNSHPTERMD-YYYY><DATEOWNSHPTERMD-MM>10</DATEOWNSHPTERMD-MM><MNUMMNTHSPPDATA>0</MNUMMNTHSPPDATA><MNUMOCCSCOMNTERMSDATA>0</MNUMOCCSCOMNTERMSDATA><MNOCFLAG>N</MNOCFLAG><ANOCFLAG>N</ANOCFLAG><MDATAOSOWNSHPRNGE>N</MDATAOSOWNSHPRNGE><ADATAOSOWNSHPRNGE>N</ADATAOSOWNSHPRNGE></DN25><DN36 seq='01'><NONLTDKEY>10487968</NONLTDKEY><NUMTELNUMBERS>0</NUMTELNUMBERS><NUMFAXNUMBERS>0</NUMFAXNUMBERS></DN36><DN40 seq='01'><NONLTDKEY>10487968</NONLTDKEY><RISKSCORE>028</RISKSCORE><RISKBAND>4</RISKBAND><STABILITYODDS>22:1      </STABILITYODDS><OVERRIDEIND>N</OVERRIDEIND><TEXTCOUNT>19</TEXTCOUNT><SCOREHISTORYCOUNT>6</SCOREHISTORYCOUNT><RISKBANDTEXT>Above Average Risk</RISKBANDTEXT><SCOREHISTORY><SCOREHISTORY_DATE-YYYY>2014</SCOREHISTORY_DATE-YYYY><SCOREHISTORY_DATE-MM>04</SCOREHISTORY_DATE-MM><SCOREHISTORY_DATE-DD>30</SCOREHISTORY_DATE-DD><SCOREHISTORY_OVERRIDEIND>N</SCOREHISTORY_OVERRIDEIND><SCOREHISTORY_RISKSCORE>30</SCOREHISTORY_RISKSCORE><SCOREHISTORY_CROVERRIDE>N</SCOREHISTORY_CROVERRIDE><SCOREHISTORY_CREDITRATING>000000000000500</SCOREHISTORY_CREDITRATING><SCOREHISTORY_CLOVERRIDE>N</SCOREHISTORY_CLOVERRIDE><SCOREHISTORY_CREDITLIMIT>000000000000750</SCOREHISTORY_CREDITLIMIT></SCOREHISTORY><SCOREHISTORY><SCOREHISTORY_DATE-YYYY>2014</SCOREHISTORY_DATE-YYYY><SCOREHISTORY_DATE-MM>05</SCOREHISTORY_DATE-MM><SCOREHISTORY_DATE-DD>31</SCOREHISTORY_DATE-DD><SCOREHISTORY_OVERRIDEIND>N</SCOREHISTORY_OVERRIDEIND><SCOREHISTORY_RISKSCORE>30</SCOREHISTORY_RISKSCORE><SCOREHISTORY_CROVERRIDE>N</SCOREHISTORY_CROVERRIDE><SCOREHISTORY_CREDITRATING>000000000000500</SCOREHISTORY_CREDITRATING><SCOREHISTORY_CLOVERRIDE>N</SCOREHISTORY_CLOVERRIDE><SCOREHISTORY_CREDITLIMIT>000000000000750</SCOREHISTORY_CREDITLIMIT></SCOREHISTORY><SCOREHISTORY><SCOREHISTORY_DATE-YYYY>2014</SCOREHISTORY_DATE-YYYY><SCOREHISTORY_DATE-MM>06</SCOREHISTORY_DATE-MM><SCOREHISTORY_DATE-DD>30</SCOREHISTORY_DATE-DD><SCOREHISTORY_OVERRIDEIND>N</SCOREHISTORY_OVERRIDEIND><SCOREHISTORY_RISKSCORE>30</SCOREHISTORY_RISKSCORE><SCOREHISTORY_CROVERRIDE>N</SCOREHISTORY_CROVERRIDE><SCOREHISTORY_CREDITRATING>000000000000500</SCOREHISTORY_CREDITRATING><SCOREHISTORY_CLOVERRIDE>N</SCOREHISTORY_CLOVERRIDE><SCOREHISTORY_CREDITLIMIT>000000000000750</SCOREHISTORY_CREDITLIMIT></SCOREHISTORY><SCOREHISTORY><SCOREHISTORY_DATE-YYYY>2014</SCOREHISTORY_DATE-YYYY><SCOREHISTORY_DATE-MM>07</SCOREHISTORY_DATE-MM><SCOREHISTORY_DATE-DD>31</SCOREHISTORY_DATE-DD><SCOREHISTORY_OVERRIDEIND>N</SCOREHISTORY_OVERRIDEIND><SCOREHISTORY_RISKSCORE>28</SCOREHISTORY_RISKSCORE><SCOREHISTORY_CROVERRIDE>N</SCOREHISTORY_CROVERRIDE><SCOREHISTORY_CREDITRATING>000000000000500</SCOREHISTORY_CREDITRATING><SCOREHISTORY_CLOVERRIDE>N</SCOREHISTORY_CLOVERRIDE><SCOREHISTORY_CREDITLIMIT>000000000000750</SCOREHISTORY_CREDITLIMIT></SCOREHISTORY><SCOREHISTORY><SCOREHISTORY_DATE-YYYY>2014</SCOREHISTORY_DATE-YYYY><SCOREHISTORY_DATE-MM>08</SCOREHISTORY_DATE-MM><SCOREHISTORY_DATE-DD>30</SCOREHISTORY_DATE-DD><SCOREHISTORY_OVERRIDEIND>N</SCOREHISTORY_OVERRIDEIND><SCOREHISTORY_RISKSCORE>28</SCOREHISTORY_RISKSCORE><SCOREHISTORY_CROVERRIDE>N</SCOREHISTORY_CROVERRIDE><SCOREHISTORY_CREDITRATING>000000000000500</SCOREHISTORY_CREDITRATING><SCOREHISTORY_CLOVERRIDE>N</SCOREHISTORY_CLOVERRIDE><SCOREHISTORY_CREDITLIMIT>000000000000750</SCOREHISTORY_CREDITLIMIT></SCOREHISTORY><SCOREHISTORY><SCOREHISTORY_DATE-YYYY>2014</SCOREHISTORY_DATE-YYYY><SCOREHISTORY_DATE-MM>09</SCOREHISTORY_DATE-MM><SCOREHISTORY_DATE-DD>30</SCOREHISTORY_DATE-DD><SCOREHISTORY_OVERRIDEIND>N</SCOREHISTORY_OVERRIDEIND><SCOREHISTORY_RISKSCORE>28</SCOREHISTORY_RISKSCORE><SCOREHISTORY_CROVERRIDE>N</SCOREHISTORY_CROVERRIDE><SCOREHISTORY_CREDITRATING>000000000000500</SCOREHISTORY_CREDITRATING><SCOREHISTORY_CLOVERRIDE>N</SCOREHISTORY_CLOVERRIDE><SCOREHISTORY_CREDITLIMIT>000000000000750</SCOREHISTORY_CREDITLIMIT></SCOREHISTORY></DN40><DN49 seq='01'><REGNUMBER>10487968</REGNUMBER><NOTECOUNT>0</NOTECOUNT><NOTELEN>43</NOTELEN></DN49><DN73 seq='01'><NONLTDKEY>10487968</NONLTDKEY><KEYSEGMENT>X</KEYSEGMENT><CONSENT>Y</CONSENT><SEARCHTYPE>B</SEARCHTYPE><NLCDSCORE>28</NLCDSCORE><CREDITRATING>00000500</CREDITRATING><CREDITLIMIT>00000750</CREDITLIMIT><PDSCORE>0.00</PDSCORE><STABILITYODDS>22:1      </STABILITYODDS><RISKBAND>4</RISKBAND><CDOVERRIDE>N</CDOVERRIDE><CROVERRIDE>N</CROVERRIDE><CLOVERRIDE>N</CLOVERRIDE><CTOVERRIDE>N</CTOVERRIDE><NUMPROPSSEARCHED>0</NUMPROPSSEARCHED><NUMPROPSFOUND>0</NUMPROPSFOUND><NUMPROPSNODATA>0</NUMPROPSNODATA><NUMADDRNOTTRACED>0</NUMADDRNOTTRACED></DN73><DN74 seq='01'><NONLTDKEY>10487968</NONLTDKEY><LENRISKTEXT>19</LENRISKTEXT><LENCREDITTEXT>495</LENCREDITTEXT><LENCONCLUDINGTEXT>165</LENCONCLUDINGTEXT><LENNOCTEXT>1</LENNOCTEXT><LENPOSSRELATEDTEXT>1</LENPOSSRELATEDTEXT><RISKTEXT>Above Average Risk</RISKTEXT><CREDITTEXT>Although the existence of the business can be confirmed, there is no record of the business size. Based on the length of ownership entered, the business appears to have been established for over 3 years under the current management. It should be noted that any information which may be held on the same business name and address before the date entered will be ignored for scoring purposes. As no proprietor details have been entered, the score has been calculated on the business details only.</CREDITTEXT><CONCLUDINGTEXT>An above average risk business; it should prove good for credit transactions to the limit assigned.  Dealings in excess of the limit may require careful monitoring.</CONCLUDINGTEXT></DN74><DK11 seq='01'><DEMOFLAG>N</DEMOFLAG><CAISFLAG>Y</CAISFLAG><PPERFUSER>N</PPERFUSER><CAISSOURCE>0721</CAISSOURCE><CIFASFLAG>N</CIFASFLAG></DK11><DM12 seq='00'><NOCCOUNT>0</NOCCOUNT></DM12><DK01 seq='01'><APPNAME>XML8255   </APPNAME><VERSIONMAJOR>0100</VERSIONMAJOR><VERSIONMINOR>0100</VERSIONMINOR></DK01><DK02 seq='01'><USERDEPT>0</USERDEPT><VER0101END>#</VER0101END></DK02><DK03 seq='01'><MESSAGETYPE>NLTD</MESSAGETYPE><MESSAGESUBTYPE>0006</MESSAGESUBTYPE><MESSAGEID>00        </MESSAGEID></DK03><DK04 seq='01'><FLAGCOUNT>1</FLAGCOUNT><FLAGVALUES><FLAGVALUE>YCON</FLAGVALUE></FLAGVALUES></DK04><DN01 seq='01'><NONLTDKEY>10487968</NONLTDKEY><OWNSHIPYRS>05</OWNSHIPYRS><OWNSHIPMTHS>01</OWNSHIPMTHS></DN01></REQUEST></GEODS>";

					var writelog = Utils.WriteLog(requestXml, newResponse, ExperianServiceType.NonLimitedData, customerId, companyRefNum: refNumber);

					nonLimitedParser.ParseAndStore(newResponse, refNumber, writelog.ServiceLog.Id);

					return BuildResponseFromDb(refNumber);
				} // if

				if (created == null)
					return null;

				NonLimitedResults res = BuildResponseFromDb(refNumber);
				res.CacheHit = true;
				return res;
			}
			catch (Exception e) {
				ms_oLog.Error(e,
					"Failed to get one non limited result for a company {0} and customer {1} (cache only: {2}, force: {3}).",
					refNumber, customerId,
					checkInCacheOnly ? "yes" : "no",
					forceCheck ? "yes" : "no"
				);
				return new NonLimitedResults(e);
			} // try
		}

		private DateTime? GetNonLimitedCreationTime(string refNumber) {
			DateTime? created = null;

			SafeReader sr = m_oDB.GetFirst(
				"GetNonLimitedCompanyCreationTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty)
				created = sr["Created"];

			return created;
		}

		private NonLimitedResults BuildResponseFromDb(string refNumber) {
			SafeReader sr = m_oDB.GetFirst(
				"GetNonLimitedCompanyBasicDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty) {
				decimal creditLimit;

				DateTime? incorporationDate = sr["IncorporationDate"];
				string errors = sr["Errors"];
				string creditLimitStr = sr["CreditLimit"];

				if (!decimal.TryParse(creditLimitStr, out creditLimit))
					creditLimit = 0;

				int riskScore = sr["RiskScore"];
				if (riskScore == 0)
					errors += "Can't read RISKSCORE section from response!";

				return new NonLimitedResults(errors, riskScore) {
					CreditLimit = creditLimit,
					CompanyName = sr["BusinessName"],
					AddressLine1 = sr["Address1"],
					AddressLine2 = sr["Address2"],
					AddressLine3 = sr["Address3"],
					AddressLine4 = sr["Address4"],
					AddressLine5 = sr["Address5"],
					PostCode = sr["Postcode"],
					IncorporationDate = incorporationDate
				};
			} // if

			return new NonLimitedResults("No data found.", 0);
		} // BuildResponseFromDb

		private string MakeRequest(string post) {
			string sRequestID = Guid.NewGuid().ToString("N");

			ms_oLog.Debug("Request {2} to URL: {0} with data: {1}", eSeriesUrl, post, sRequestID);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(eSeriesUrl);
			request.Method = "POST";
			request.AllowAutoRedirect = false;
			request.ContentType = "application/xml";
			request.ContentLength = post.Length;

			var stOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
			stOut.Write(post);
			stOut.Close();

			using (WebResponse resp = request.GetResponse()) {
				Stream oStream = resp.GetResponseStream();

				if (oStream == null) {
					ms_oLog.Warn("Request {0}: result is empty because could not read from web response.", sRequestID);
					return string.Empty;
				} // if

				using (var sr = new StreamReader(oStream)) {
					string sResponse = sr.ReadToEnd();

					int nLen = Encoding.ASCII.GetByteCount(sResponse);

					ms_oLog.Warn("Request {0}: result is {1} long.", sRequestID, Grammar.Number(nLen, "byte"));

					return sResponse;
				} // using reader
			} // using response
		} // MakeRequest

		private string GetResource(string resName, params object[] p) {
			using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName)) {
				if (s == null)
					return null;

				var data = new byte[s.Length];

				s.Read(data, 0, (int)s.Length);

				return string.Format(Encoding.UTF8.GetString(data), p);
			} // using
		} // GetResource

		private readonly SqlRetryer m_oRetryer;
		private readonly NonLimitedParser nonLimitedParser;
		private readonly string eSeriesUrl;

		private readonly AConnection m_oDB;
		private static readonly SafeILog ms_oLog = new SafeILog(typeof(EBusinessService));

	} // class EBusinessService
} // namespace
