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
	using EzServiceAccessor;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;

	public class EBusinessService {
		#region public

		#region constructor

		public EBusinessService(AConnection oDB) {
			m_oRetryer = new SqlRetryer(oLog: ms_oLog);
			eSeriesUrl = CurrentValues.Instance.ExperianESeriesUrl;
			nonLimitedParser = new NonLimitedParser();

			m_oDB = oDB;
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

				string response = MakeRequest(requestXml);

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
			if (string.IsNullOrWhiteSpace(regNumber))
				return null;

			try {
				ExperianLtd oExperianLtd = null;
				bool bCacheHit = false;

				if (forceCheck)
					oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);

				if (oExperianLtd == null) {
					oExperianLtd = ObjectFactory.GetInstance<IEzServiceAccessor>().CheckLtdCompanyCache(regNumber);

					if ((oExperianLtd == null) || (oExperianLtd.ID == 0)) {
						oExperianLtd = null;

						ms_oLog.Debug(
							"GetOneLimitedBusinessData({0}, {1}, {2}, {3}): no data found in cache.",
							regNumber, customerId, checkInCacheOnly, forceCheck
						);

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
			ms_oLog.Debug("Downloading data from Experian for company {0} and customer {1}...", regNumber, customerId);

			string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

			string newResponse = MakeRequest(requestXml);

			var pkg = new WriteToLogPackage(requestXml, newResponse, ExperianServiceType.LimitedData, customerId);

			Utils.WriteLog(pkg);

			ms_oLog.Debug("Downloading data from Experian for company {0} and customer {1} complete.", regNumber, customerId);

			return pkg.Out.ExperianLtd;
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

					var newResponse = MakeRequest(requestXml);

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
				return new NonLimitedResults(e);
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

		#endregion method GetOneNotLimitedBusinessData

		#region method MakeRequest

		private string MakeRequest(string post) {
			ms_oLog.Debug("Request URL: {0} with data: {1}", eSeriesUrl, post);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(eSeriesUrl);
			request.Method = "POST";
			request.AllowAutoRedirect = false;
			request.ContentType = "application/xml";
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
