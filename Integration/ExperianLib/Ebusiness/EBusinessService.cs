namespace ExperianLib.Ebusiness {
	using System;
	using System.Data;
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
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;
	using log4net;

	public class EBusinessService
	{
		#region public

		#region constructor

		public EBusinessService() {
			m_oRetryer = new SqlRetryer(oLog: new SafeILog(Log));
			eSeriesUrl = CurrentValues.Instance.ExperianESeriesUrl;
			experianDL97AccountsRepository = ObjectFactory.GetInstance<ExperianDL97AccountsRepository>();
			nonLimitedParser = new NonLimitedParser();

			log = new SafeILog(LogManager.GetLogger(typeof(NonLimitedParser)));
			var env = new Ezbob.Context.Environment(log);
			db = new SqlConnection(env, log);
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
				Log.Error(e);
				throw;
			} // try
		} // TargetBusiness

		#endregion method TargetBusiness

		#region method GetLimitedBusinessData

		public LimitedResults GetLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck)
		{
			Log.DebugFormat("Begin GetLimitedBusinessData {0} {1} {2} {3}", regNumber, customerId, checkInCacheOnly, forceCheck);
			var oRes = GetOneLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);
			oRes.MaxBureauScore = oRes.BureauScore;
			Log.InfoFormat("Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}", oRes.BureauScore, oRes.MaxBureauScore, customerId, regNumber);
			foreach (string sOwnerRegNum in oRes.Owners)
			{
				var parentCompanyResult = GetOneLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly, forceCheck);
				if (parentCompanyResult.BureauScore > oRes.MaxBureauScore)
				{
					oRes.MaxBureauScore = parentCompanyResult.BureauScore;
				}
				Log.InfoFormat("Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}", parentCompanyResult.BureauScore, oRes.MaxBureauScore, customerId, sOwnerRegNum);
			}
			return oRes;
		} // GetLimitedBusinessData

		#endregion method GetLimitedBusinessData

		#region method GetNotLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			var oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);
			oRes.MaxBureauScore = oRes.BureauScore;
			Log.InfoFormat("Fetched BureauScore:{0} Calculated MaxBureauScore:{1} for customer:{2} regNum:{3}", oRes.BureauScore, oRes.MaxBureauScore, customerId, regNumber);
			
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

		private bool CacheExpired(DateTime updateDate)
		{
			int cacheIsValidForDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;
			return (DateTime.UtcNow - updateDate).TotalDays > cacheIsValidForDays;
		} // CacheNotExpired

		#region method GetOneLimitedBusinessData

		private LimitedResults GetOneLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck)
		{
			try 
			{
				var response = CheckCache(regNumber);

				if (forceCheck || (!checkInCacheOnly && (response == null || CacheExpired(response.LastUpdateDate))))
				{
					string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

					var newResponse = MakeRequest("POST", "application/xml", requestXml);

					MP_ServiceLog oLogEntry = Utils.WriteLog(requestXml, newResponse, ExperianServiceType.LimitedData, customerId);

					var res = new LimitedResults(oLogEntry.Id, newResponse, DateTime.UtcNow) {CacheHit = false};
					AddToCache(regNumber, requestXml, res);

					return res;
				} // if

				if (response == null)
				{
					return null;
				}

				MakeSureDl97IsFilled(customerId, response);

				return new LimitedResults(0, response.JsonPacket, response.LastUpdateDate) { CacheHit = true };
			}
			catch (Exception e) {
				Log.Error(e);
				return new LimitedResults(e);
			} // try
		} // GetOneLimitedBusinessData

		#endregion method GetOneLimitedBusinessData

		private void MakeSureDl97IsFilled(int customerId, MP_ExperianDataCache cachedEntry)
		{
			Log.DebugFormat("Making sure company accounts (DL97) is filled for customer {0}", customerId);
			if (experianDL97AccountsRepository.GetAll().FirstOrDefault(x => x.CustomerId == customerId) == null)
			{
				Log.DebugFormat("Company accounts (DL97) don't exist for customer {0}. Will fill it now", customerId);
				// Customer doesn't have Dl97 entries - we should insert all the entries like the customer from the cache entry
				foreach (ExperianDL97Accounts accountEntry in experianDL97AccountsRepository.GetAll().Where(x => x.CustomerId == cachedEntry.CustomerId))
				{
					var newAccountEntry = new ExperianDL97Accounts
					{
						CustomerId = customerId,
						State = accountEntry.State,
						Type = accountEntry.Type,
						Status12Months = accountEntry.Status12Months,
						LastUpdated = accountEntry.LastUpdated,
						CompanyType = accountEntry.CompanyType,
						CurrentBalance = accountEntry.CurrentBalance,
						MonthsData = accountEntry.MonthsData,
						Status1To2 = accountEntry.Status1To2,
						Status3To9 = accountEntry.Status3To9
					};

					experianDL97AccountsRepository.SaveOrUpdate(newAccountEntry);
				}
			}
		}

		#region method GetOneNotLimitedBusinessData

		private NonLimitedResults GetOneNotLimitedBusinessData(string refNumber, int customerId, bool checkInCacheOnly, bool forceCheck)
		{
			try
			{
				DateTime? created = GetNonLimitedCreationTime(refNumber, customerId);
				if (forceCheck || (!checkInCacheOnly && (created == null || CacheExpired(created.Value))))
				{
					string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", refNumber);

					var newResponse = MakeRequest("POST", "application/xml", requestXml);

					MP_ServiceLog serviceLogEntry = Utils.WriteLog(requestXml, newResponse, ExperianServiceType.NonLimitedData, customerId);

					nonLimitedParser.ParseAndStore(customerId, newResponse, refNumber, serviceLogEntry.Id);

					return BuildResponseFromDb(customerId, refNumber);
				} // if

				if (created == null)
				{
					return null;
				}

				NonLimitedResults res = BuildResponseFromDb(customerId, refNumber);
				res.CacheHit = true;
				return res;
			}
			catch (Exception e) {
				Log.Error(e);
				return new NonLimitedResults {Error = e.Message};
			} // try
		}

		private DateTime? GetNonLimitedCreationTime(string refNumber, int customerId)
		{
			DateTime? created = null;
			DataTable dt = db.ExecuteReader(
				"GetNonLimitedCompanyCreationTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));
			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				created = sr["Created"];
			}
			return created;
		}

		private NonLimitedResults BuildResponseFromDb(int customerId, string refNumber)
		{
			DateTime? incorporationDate;
			string errors, businessName, address1, address2, address3, address4, address5, postcode;
			int riskScore;
			decimal creditLimit;
			var nonLimitedResults = new NonLimitedResults();

			DataTable dt = db.ExecuteReader(
				"GetNonLimitedCompanyBasicDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				incorporationDate = sr["IncorporationDate"];
				errors = sr["Errors"];
				string creditLimitStr = sr["CreditLimit"];
				if (!decimal.TryParse(creditLimitStr, out creditLimit))
				{
					creditLimit = 0;
				}
				businessName = sr["BusinessName"];
				address1 = sr["Address1"];
				address2 = sr["Address2"];
				address3 = sr["Address3"];
				address4 = sr["Address4"];
				address5 = sr["Address5"];
				postcode = sr["Postcode"];
				riskScore = sr["RiskScore"];

				nonLimitedResults.LastCheckDate = DateTime.UtcNow;
				nonLimitedResults.Error = errors;
				nonLimitedResults.CompanyNotFoundOnBureau = nonLimitedResults.IsError;
				if (riskScore == 0)
				{
					nonLimitedResults.Error += "Can't read RISKSCORE section from response!";
				}
				else
				{
					nonLimitedResults.BureauScore = riskScore;
				}

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
			}

			return nonLimitedResults;
		}

		#endregion method GetOneNotLimitedBusinessData

		#region method CheckCache

		private MP_ExperianDataCache CheckCache(string refNumber) {
			return m_oRetryer.Retry(() => {
				var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

				Log.InfoFormat("Checking cache for refNumber={0}...", refNumber);

				var cacheVal = repo.GetAll().FirstOrDefault(c => c.CompanyRefNumber == refNumber);

				if (cacheVal != null) {
					Log.InfoFormat("Returning data from cache for refNumber={0}", refNumber);
					return cacheVal;
				} // if

				Log.WarnFormat("Company data from cache for refNumber={0} was not found", refNumber);
				return null;
			}, "EBusinessService.CheckCache(" + refNumber + ")");
		} // CheckCache

		#endregion method CheckCache

		#region method AddToCache

		private void AddToCache(string refNum, string input, BusinessReturnData res)
		{
			m_oRetryer.Retry(() => {
				var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

				MP_ExperianDataCache cacheVal =
					repo.GetAll().FirstOrDefault(c => c.CompanyRefNumber == refNum)
					?? new MP_ExperianDataCache { CompanyRefNumber = refNum };

				cacheVal.LastUpdateDate = DateTime.UtcNow;
				cacheVal.JsonPacketInput = input;
				cacheVal.JsonPacket = res.OutputXml;
				cacheVal.ExperianScore = (int)res.BureauScore;
				cacheVal.ExperianMaxScore = (int)res.MaxBureauScore;
				
				repo.SaveOrUpdate(cacheVal);
			}, "EBusinessService.AddToCache(" + refNum + ")");
			try
			{
				
				if (res.GetType() == typeof (LimitedResults))
				{
					var t = res as LimitedResults;
					if (t != null && !t.Owners.Any()) return;
					var repo = ObjectFactory.GetInstance<ExperianParentCompanyMapRepository>();
					if (t != null)
					{
						foreach (var owner in t.Owners)
						{
							var map = new MP_ExperianParentCompanyMap
								{
									ExperianRefNum = refNum,
									ExperianParentRefNum = owner
								};
							repo.SaveOrUpdate(map);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.ErrorFormat("Failed to save owners map for company {0} \n{1}", refNum, ex);
			}
		} // AddToCache

		#endregion method AddToCache

		#region method MakeRequest

		private string MakeRequest(string method, string contentType, string post) {
			Log.DebugFormat("Request URL: {0} with data: {1}", eSeriesUrl, post);

			var request = (HttpWebRequest)WebRequest.Create(eSeriesUrl);
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

		private static readonly ILog Log = LogManager.GetLogger(typeof(EBusinessService));
		private readonly SqlRetryer m_oRetryer;
		private readonly ExperianDL97AccountsRepository experianDL97AccountsRepository;
		private readonly NonLimitedParser nonLimitedParser;
		private readonly string eSeriesUrl;

		#endregion properties

		private readonly AConnection db;
		private readonly SafeILog log;

		#endregion private
	} // class EBusinessService
} // namespace
