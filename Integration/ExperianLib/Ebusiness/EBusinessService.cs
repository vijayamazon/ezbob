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
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StructureMap;
	using log4net;

	public class EBusinessService {
		#region public

		#region constructor

		public EBusinessService() {
			m_oRetryer = new SqlRetryer(oLog: new SafeILog(Log));
			eSeriesUrl = CurrentValues.Instance.ExperianESeriesUrl;
			experianDL97AccountsRepository = ObjectFactory.GetInstance<ExperianDL97AccountsRepository>();
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
			foreach (string sOwnerRegNum in oRes.Owners)
			{
				var parentCompanyResult = GetOneLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly, forceCheck);
				if (parentCompanyResult.BureauScore > oRes.MaxBureauScore)
				{
					oRes.MaxBureauScore = parentCompanyResult.BureauScore;
				}
			}
			return oRes;
		} // GetLimitedBusinessData

		#endregion method GetLimitedBusinessData

		#region method GetNotLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			var oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);
			oRes.MaxBureauScore = oRes.BureauScore;

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

		private bool CacheExpired(MP_ExperianDataCache cacheEntry)
		{
			int cacheIsValidForDays = CurrentValues.Instance.UpdateCompanyDataPeriodDays;
			return (DateTime.UtcNow - cacheEntry.LastUpdateDate).TotalDays > cacheIsValidForDays;
		} // CacheNotExpired

		#region method GetOneLimitedBusinessData

		private LimitedResults GetOneLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck)
		{
			try 
			{
				var response = CheckCache(regNumber);

				if (forceCheck || (!checkInCacheOnly && (response == null || CacheExpired(response))))
				{
					string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

					var newResponse = MakeRequest("POST", "application/xml", requestXml);

					Utils.WriteLog(requestXml, newResponse, ExperianServiceType.LimitedData, customerId);

					var res = new LimitedResults(newResponse, DateTime.UtcNow) {CacheHit = false};
					AddToCache(regNumber, requestXml, res);

					return res;
				} // if

				if (response == null)
				{
					return null;
				}

				MakeSureDl97IsFilled(customerId, response);

				return new LimitedResults(response.JsonPacket, response.LastUpdateDate) { CacheHit = true };
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

		private NonLimitedResults GetOneNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck)
		{
			try 
			{
				var response = CheckCache(regNumber);

				if (forceCheck || (!checkInCacheOnly && (response == null || CacheExpired(response))))
				{
					string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", regNumber);

					var newResponse = MakeRequest("POST", "application/xml", requestXml);

					Utils.WriteLog(requestXml, newResponse, ExperianServiceType.NonLimitedData, customerId);

					var res = new NonLimitedResults(newResponse, DateTime.UtcNow) { CacheHit = false };
					AddToCache(regNumber, requestXml, res);

					return res;
				} // if

				if (response == null)
				{
					return null;
				}

				return new NonLimitedResults(response.JsonPacket, response.LastUpdateDate) { CacheHit = true };
			}
			catch (Exception e) {
				Log.Error(e);
				return new NonLimitedResults(e);
			} // try
		} // GetOneNotLimitedBusinessData

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
		private readonly string eSeriesUrl;

		#endregion properties

		#endregion private
	} // class EBusinessService
} // namespace
