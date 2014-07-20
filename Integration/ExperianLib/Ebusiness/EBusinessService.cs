namespace ExperianLib.Ebusiness {
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using ApplicationMng.Repository;
	using ConfigManager;
	using EBusiness;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Experian;
	using EzServiceAccessor;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class EBusinessService {
		#region public

		#region constructor

		public EBusinessService() {
			m_oRetryer = new SqlRetryer(oLog: ms_oLog);
			eSeriesUrl = CurrentValues.Instance.ExperianESeriesUrl;
			nonLimitedParser = new NonLimitedParser();

			var env = new Ezbob.Context.Environment(ms_oLog);
			m_oDB = new SqlConnection(env, ms_oLog);
		} // constructor

		#endregion constructor

		#region method TargetBusiness

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

				var response = MakeRequest("POST", "application/xml", requestXml);

				Utils.WriteLog(requestXml, response, ExperianServiceType.Targeting, customerId);

				return new TargetResults(response);
			}
			catch (Exception e) {
				ms_oLog.Error(e, "Target business failed.");
				throw;
			} // try
		} // TargetBusiness

		#endregion method TargetBusiness

		#region method GetLimitedBusinessData

		public LimitedResults GetLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			ms_oLog.Debug("Begin GetLimitedBusinessData {0} {1} {2} {3}", regNumber, customerId, checkInCacheOnly, forceCheck);

			var oRes = GetOneLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);

			if (oRes == null)
				return null;

			oRes.MaxBureauScore = oRes.BureauScore;

			ms_oLog.Info("Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}", oRes.BureauScore, oRes.MaxBureauScore, customerId, regNumber);

			foreach (string sOwnerRegNum in oRes.Owners) {
				var parentCompanyResult = GetOneLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly, forceCheck);

				if (parentCompanyResult == null)
					continue;

				if (parentCompanyResult.BureauScore > oRes.MaxBureauScore)
					oRes.MaxBureauScore = parentCompanyResult.BureauScore;

				ms_oLog.Info("Fetched BureauScore: {0} Calculated MaxBureauScore: {1} for customer: {2} regNum: {3}.", parentCompanyResult.BureauScore, oRes.MaxBureauScore, customerId, sOwnerRegNum);
			} // for each

			return oRes;
		} // GetLimitedBusinessData

		#endregion method GetLimitedBusinessData

		#region method GetNotLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			var oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);
			oRes.MaxBureauScore = oRes.BureauScore;
			ms_oLog.Info("Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}", oRes.BureauScore, oRes.MaxBureauScore, customerId, regNumber);
			return oRes;
		} // GetNotLimitedBusinessData

		#endregion method GetNotLimitedBusinessData

		#region method TargetCache

		public CompanyInfo TargetCache(int customerId, string refNumber) {
			return m_oRetryer.Retry(() => {
				var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ServiceLog>>();

				IQueryable<MP_ServiceLog> oCachedValues =
					repo.GetAll().Where(c => c.ServiceType == "ESeriesTargeting" && c.Customer.Id == customerId);

				foreach (var oVal in oCachedValues) {
					var targets = new TargetResults(oVal.ResponseData);

					foreach (var target in targets.Targets)
						if (target.BusRefNum == refNumber)
							return target;
				} // for each cached value

				return null;
			}, "EBusinessService.TargetCache(" + customerId + ", " + refNumber + ")");
		} // TargetCache

		#endregion method TargetCache

		#endregion public

		#region private

		#region method GetOneLimitedBusinessData

		private LimitedResults GetOneLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			try {
				ExperianLtd oExperianLtd = null;
				bool bCacheHit = false;

				if (forceCheck)
					oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);

				if (oExperianLtd == null) {
					oExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().CheckLtdCompanyCache(regNumber);

					if (oExperianLtd == null) {
						if (!checkInCacheOnly)
							oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);
					}
					else
						bCacheHit = true;
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

		#endregion method GetOneLimitedBusinessData

		#region method DownloadOneLimitedFromExperian

		private ExperianLtd DownloadOneLimitedFromExperian(string regNumber, int customerId) {
			string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

			string newResponse = MakeRequest("POST", "application/xml", requestXml);

			MP_ServiceLog oLogEntry = Utils.WriteLog(requestXml, newResponse, ExperianServiceType.LimitedData, customerId);

			ExperianLtd oExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().LoadExperianLtd(oLogEntry.Id);

			var res = new LimitedResults(oExperianLtd, false);

			try {
				if (res.Owners.Count > 0) {
					var repo = ObjectFactory.GetInstance<ExperianParentCompanyMapRepository>();

					foreach (var owner in res.Owners) {
						var map = new MP_ExperianParentCompanyMap {
							ExperianRefNum = regNumber,
							ExperianParentRefNum = owner
						};
						repo.SaveOrUpdate(map);
					} // for each owner
				} // if
			}
			catch (Exception ex) {
				ms_oLog.Error(ex, "Failed to save owners map for company {0}.", regNumber);
			} // try

			return oExperianLtd;
		} // DownloadOneLimitedFromExperian

		#endregion method DownloadOneLimitedFromExperian

		#region method CacheExpired

		private bool CacheExpired(DateTime? updateDate) {
			if (!updateDate.HasValue)
				return true;

			int cacheIsValidForDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;
			return (DateTime.UtcNow - updateDate.Value).TotalDays > cacheIsValidForDays;
		} // CacheNotExpired

		#endregion method CacheExpired

		#region method GetOneNotLimitedBusinessData

		private NonLimitedResults GetOneNotLimitedBusinessData(string refNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			try {
				DateTime? created = GetNonLimitedCreationTime(refNumber, customerId);

				if (forceCheck || (!checkInCacheOnly && CacheExpired(created))) {
					string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", refNumber);

					var newResponse = MakeRequest("POST", "application/xml", requestXml);

					MP_ServiceLog serviceLogEntry = Utils.WriteLog(requestXml, newResponse, ExperianServiceType.NonLimitedData, customerId);

					nonLimitedParser.ParseAndStore(customerId, newResponse, refNumber, serviceLogEntry.Id);

					return BuildResponseFromDb(customerId, refNumber);
				} // if

				if (created == null)
					return null;

				NonLimitedResults res = BuildResponseFromDb(customerId, refNumber);
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
				return new NonLimitedResults { Error = e.Message };
			} // try
		}

		private DateTime? GetNonLimitedCreationTime(string refNumber, int customerId) {
			DateTime? created = null;

			SafeReader sr = m_oDB.GetFirst(
				"GetNonLimitedCompanyCreationTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty)
				created = sr["Created"];

			return created;
		}

		private NonLimitedResults BuildResponseFromDb(int customerId, string refNumber) {
			var nonLimitedResults = new NonLimitedResults();

			SafeReader sr = m_oDB.GetFirst(
				"GetNonLimitedCompanyBasicDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty) {
				decimal creditLimit;

				DateTime? incorporationDate = sr["IncorporationDate"];
				string errors = sr["Errors"];
				string creditLimitStr = sr["CreditLimit"];

				if (!decimal.TryParse(creditLimitStr, out creditLimit))
					creditLimit = 0;

				string businessName = sr["BusinessName"];
				string address1 = sr["Address1"];
				string address2 = sr["Address2"];
				string address3 = sr["Address3"];
				string address4 = sr["Address4"];
				string address5 = sr["Address5"];
				string postcode = sr["Postcode"];
				int riskScore = sr["RiskScore"];

				nonLimitedResults.LastCheckDate = DateTime.UtcNow;
				nonLimitedResults.Error = errors;

				if (riskScore == 0)
					nonLimitedResults.Error += "Can't read RISKSCORE section from response!";
				else
					nonLimitedResults.BureauScore = riskScore;

				nonLimitedResults.CompanyNotFoundOnBureau = !string.IsNullOrEmpty(nonLimitedResults.Error);
				nonLimitedResults.CreditLimit = creditLimit;
				nonLimitedResults.CompanyName = businessName;
				nonLimitedResults.AddressLine1 = address1;
				nonLimitedResults.AddressLine2 = address2;
				nonLimitedResults.AddressLine3 = address3;
				nonLimitedResults.AddressLine4 = address4;
				nonLimitedResults.AddressLine5 = address5;
				nonLimitedResults.PostCode = postcode;
				nonLimitedResults.IncorporationDate = incorporationDate;
			} // if

			return nonLimitedResults;
		} // BuildResponseFromDb

		#endregion method GetOneNotLimitedBusinessData

		#region method MakeRequest

		private string MakeRequest(string method, string contentType, string post) {
			ms_oLog.Debug("Request URL: {0} with data: {1}", eSeriesUrl, post);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(eSeriesUrl);
			request.Method = method;
			request.AllowAutoRedirect = false;

			if (!string.IsNullOrEmpty(contentType))
				request.ContentType = contentType;

			request.ContentLength = post.Length;

			var stOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
			stOut.Write(post);
			stOut.Close();

			var resp = request.GetResponse();

			var sr = new StreamReader(resp.GetResponseStream());

			var respStr = sr.ReadToEnd();

			return respStr;
		} // MakeRequest

		#endregion method MakeRequest

		#region method GetResource

		private string GetResource(string resName, params object[] p) {
			using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName)) {
				if (s == null)
					return null;

				var data = new byte[s.Length];

				s.Read(data, 0, (int)s.Length);

				return string.Format(Encoding.UTF8.GetString(data), p);
			} // using
		} // GetResource

		#endregion method GetResource

		#region properties

		private readonly SqlRetryer m_oRetryer;
		private readonly NonLimitedParser nonLimitedParser;
		private readonly string eSeriesUrl;

		private readonly AConnection m_oDB;
		private static readonly SafeILog ms_oLog = new SafeILog(typeof(EBusinessService));

		#endregion properties

		#endregion private
	} // class EBusinessService
} // namespace
