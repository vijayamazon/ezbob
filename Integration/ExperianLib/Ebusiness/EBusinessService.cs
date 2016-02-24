namespace ExperianLib.Ebusiness {
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Lingvo;
	using EzServiceAccessor;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Repository;
	using StructureMap;
	using Ezbob.Backend.Models;

	public class EBusinessService {
		public EBusinessService(AConnection oDB) {
			this.retryer = new SqlRetryer(log: log);
			this.eSeriesUrl = CurrentValues.Instance.ExperianESeriesUrl;

			this.db = oDB;
		} // constructor

		public TargetResults TargetBusiness(
			string companyName,
			string postCode,
			int customerId,
			TargetResults.LegalStatus nFilter,
			string regNum = ""
		) {
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
			} catch (Exception e) {
				log.Error(e, "Target business failed.");
				throw;
			} // try
		} // TargetBusiness

		public LimitedResults GetLimitedBusinessData(
			string regNumber,
			int customerId,
			bool checkInCacheOnly,
			bool forceCheck
		) {
			log.Debug(
				"Begin GetLimitedBusinessData({0}, {1}, {2}, {3})...",
				regNumber,
				customerId,
				checkInCacheOnly,
				forceCheck
			);

			LimitedResults oRes = GetOneLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);

			if (oRes == null) {
				log.Debug(
					"End GetLimitedBusinessData({0}, {1}, {2}, {3}): result is null.",
					regNumber,
					customerId,
					checkInCacheOnly,
					forceCheck
				);
				return null;
			} // if

			oRes.MaxBureauScore = oRes.BureauScore;

			log.Info(
				"Fetched BureauScore: {0}, Calculated MaxBureauScore: {1} for customer: {2} and regNum: {3}.",
				oRes.BureauScore,
				oRes.MaxBureauScore,
				customerId,
				regNumber
			);

			log.Debug(
				"GetLimitedBusinessData({0}, {1}, {2}, {3}): traversing owners...",
				regNumber,
				customerId,
				checkInCacheOnly,
				forceCheck
			);

			foreach (string sOwnerRegNum in oRes.Owners) {
				log.Debug(
					"GetLimitedBusinessData({0}, {1}, {2}, {3}): current owner reg num is '{4}'.",
					regNumber, customerId, checkInCacheOnly, forceCheck, sOwnerRegNum
				);

				LimitedResults parentCompanyResult = GetOneLimitedBusinessData(
					sOwnerRegNum,
					customerId,
					checkInCacheOnly,
					forceCheck
				);

				if (parentCompanyResult == null)
					continue;

				if (parentCompanyResult.BureauScore > oRes.MaxBureauScore)
					oRes.MaxBureauScore = parentCompanyResult.BureauScore;

				log.Info(
					"Fetched BureauScore: {0}, Calculated MaxBureauScore: {1} for customer: {2} and regNum: {3}.",
					parentCompanyResult.BureauScore,
					oRes.MaxBureauScore,
					customerId,
					sOwnerRegNum
				);
			} // for each

			log.Debug(
				"GetLimitedBusinessData({0}, {1}, {2}, {3}): traversing owners complete.",
				regNumber,
				customerId,
				checkInCacheOnly,
				forceCheck
			);

			log.Debug("End GetLimitedBusinessData({0}, {1}, {2}, {3}).",
				regNumber,
				customerId,
				checkInCacheOnly,
				forceCheck
			);

			return oRes;
		} // GetLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(
			string regNumber,
			int customerId,
			bool checkInCacheOnly,
			bool forceCheck
		) {
			var oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);

			if (oRes == null) {
				log.Warn("Failed fetching non limited data. Customer:{0} RefNumber:{1}", customerId, regNumber);
				return null;
			} // if

			oRes.MaxBureauScore = oRes.BureauScore;

			log.Info(
				"Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}",
				oRes.BureauScore,
				oRes.MaxBureauScore,
				customerId,
				regNumber
			);

			return oRes;
		} // GetNotLimitedBusinessData

		public CompanyInfo TargetCache(int customerId, string refNumber) {
			return this.retryer.Retry(
				() => {
					var repo = ObjectFactory.GetInstance<ServiceLogRepository>();

					IQueryable<MP_ServiceLog> oCachedValues = repo.GetAll().Where(c =>
						c.Customer != null &&
						c.Customer.Id == customerId &&
						c.ServiceType == "ESeriesTargeting"
					);

					foreach (var oVal in oCachedValues) {
						var targets = new TargetResults(oVal.ResponseData);

						foreach (var target in targets.Targets) {
							if (target.BusRefNum == refNumber)
								return target;
						} // for each target
					} // for each cached value

					return null;
				},
				"EBusinessService.TargetCache(" + customerId + ", " + refNumber + ")"
			);
		} // TargetCache

		private LimitedResults GetOneLimitedBusinessData(
			string regNumber,
			int customerId,
			bool checkInCacheOnly,
			bool forceCheck
		) {
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

						log.Debug(
							"GetOneLimitedBusinessData({0}, {1}, {2}, {3}): no data found in cache.",
							regNumber,
							customerId,
							checkInCacheOnly,
							forceCheck
						);

						if (!checkInCacheOnly)
							oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);
					} else {
						bool cacheExpired = (DateTime.UtcNow - oExperianLtd.ReceivedTime).TotalDays >
							CurrentValues.Instance.UpdateCompanyDataPeriodDays;

						if (cacheExpired) {
							oExperianLtd = DownloadOneLimitedFromExperian(regNumber, customerId);

							if ((oExperianLtd == null) || (oExperianLtd.ID == 0)) {
								oExperianLtd = null;

								log.Debug(
									"GetOneLimitedBusinessData({0}, {1}, {2}, {3}): " +
									"no data downloaded nor data found in cache.",
									regNumber,
									customerId,
									checkInCacheOnly,
									forceCheck
								);
							} // if
						} else
							bCacheHit = true;
					} // if
				} // if

				log.Debug(
					"GetOneLimitedBusinessData({0}, {1}, {2}, {3}) = (cache hit: {4}):\n{5}",
					regNumber,
					customerId,
					checkInCacheOnly,
					forceCheck,
					bCacheHit,
					oExperianLtd == null ? "-- null --" : oExperianLtd.StringifyAll()
				);

				return oExperianLtd == null ? null : new LimitedResults(oExperianLtd, bCacheHit);
			} catch (Exception e) {
				log.Error(e,
					"Failed to get limited results for a company {0} and customer {1} (cache only: {2}, force: {3}).",
					regNumber,
					customerId,
					checkInCacheOnly ? "yes" : "no",
					forceCheck ? "yes" : "no"
				);
				return new LimitedResults(e);
			} // try
		} // GetOneLimitedBusinessData

		private ExperianLtd DownloadOneLimitedFromExperian(string regNumber, int customerId) {
			log.Debug("Downloading data from Experian for company {0} and customer {1}...", regNumber, customerId);

			string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

			string newResponse = MakeRequest(requestXml);

			var pkg = new WriteToLogPackage(
				requestXml,
				newResponse,
				ExperianServiceType.LimitedData,
				customerId,
				companyRefNum: regNumber
			);

			Utils.WriteLog(pkg);

			log.Debug(
				"Downloading data from Experian for company {0} and customer {1} complete.",
				regNumber,
				customerId
			);

			return pkg.Out.ExperianLtd;
		} // DownloadOneLimitedFromExperian

		private bool CacheExpired(DateTime? updateDate) {
			if (!updateDate.HasValue)
				return true;

			int cacheIsValidForDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;
			return (DateTime.UtcNow - updateDate.Value).TotalDays > cacheIsValidForDays;
		} // CacheNotExpired

		private NonLimitedResults GetOneNotLimitedBusinessData(
			string refNumber,
			int customerId,
			bool checkInCacheOnly,
			bool forceCheck
		) {
			try {
				DateTime? created = GetNonLimitedCreationTime(refNumber);

				if (forceCheck || (!checkInCacheOnly && CacheExpired(created))) {

					string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", refNumber);

					var newResponse = MakeRequest(requestXml);

					Utils.WriteLog(
						requestXml,
						newResponse,
						ExperianServiceType.NonLimitedData,
						customerId,
						companyRefNum: refNumber
					);

					return BuildResponseFromDb(refNumber);
				} // if

				if (created == null)
					return null;

				NonLimitedResults res = BuildResponseFromDb(refNumber);
				res.CacheHit = true;
				return res;
			} catch (Exception e) {
				log.Error(e,
					"Failed to get one non limited result for a company {0} and customer {1} (cache only: {2}, force: {3}).",
					refNumber, customerId,
					checkInCacheOnly ? "yes" : "no",
					forceCheck ? "yes" : "no"
				);

				return new NonLimitedResults(e);
			} // try
		} // GetOneNotLimitedBusinessData

		private DateTime? GetNonLimitedCreationTime(string refNumber) {
			DateTime? created = null;

			SafeReader sr = this.db.GetFirst(
				"GetNonLimitedCompanyCreationTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty)
				created = sr["Created"];

			return created;
		} // GetNonLimitedCreationTime

		private NonLimitedResults BuildResponseFromDb(string refNumber) {
			SafeReader sr = this.db.GetFirst(
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

			log.Debug("Request {2} to URL: {0} with data: {1}", this.eSeriesUrl, post, sRequestID);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.eSeriesUrl);
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
					log.Warn("Request {0}: result is empty because could not read from web response.", sRequestID);
					return string.Empty;
				} // if

				using (var sr = new StreamReader(oStream)) {
					string sResponse = sr.ReadToEnd();

					int nLen = Encoding.ASCII.GetByteCount(sResponse);

					log.Warn("Request {0}: result is {1} long.", sRequestID, Grammar.Number(nLen, "byte"));

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

		private readonly SqlRetryer retryer;
		private readonly string eSeriesUrl;
		private readonly AConnection db;

		private static readonly SafeILog log = new SafeILog(typeof(EBusinessService));
	} // class EBusinessService
} // namespace
