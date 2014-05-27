﻿namespace ExperianLib.Ebusiness {
	using System;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Reflection;
	using System.Text;
	using System.Web;
	using System.Xml;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model;
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
			configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
			eSeriesUrl = configurationVariablesRepository.GetByName("ExperianESeriesUrl");
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

			foreach (string sOwnerRegNum in oRes.Owners)
				GetOneLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly, forceCheck);

			return oRes;
		} // GetLimitedBusinessData

		#endregion method GetLimitedBusinessData

		#region method GetNotLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly, bool forceCheck) {
			var oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly, forceCheck);

			foreach (string sOwnerRegNum in oRes.Owners)
				GetOneNotLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly, forceCheck);

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
			int cacheIsValidForDays = configurationVariablesRepository.GetByNameAsInt("UpdateCompanyDataPeriodDays");
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

					AddToCache(regNumber, requestXml, newResponse);

					return new LimitedResults(newResponse, DateTime.UtcNow) { CacheHit = false };
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
			if (experianDL97AccountsRepository.GetAll().FirstOrDefault(x => x.CustomerId == customerId) == null)
			{
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

					AddToCache(regNumber, requestXml, newResponse);

					return new NonLimitedResults(newResponse, DateTime.UtcNow) { CacheHit = false };
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

		private void AddToCache(string refNumber, string request, string response) {
			m_oRetryer.Retry(() => {
				var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

				MP_ExperianDataCache cacheVal =
					repo.GetAll().FirstOrDefault(c => c.CompanyRefNumber == refNumber)
					?? new MP_ExperianDataCache {CompanyRefNumber = refNumber};

				cacheVal.LastUpdateDate = DateTime.UtcNow;
				cacheVal.JsonPacketInput = request;
				cacheVal.JsonPacket = response;
				
				repo.SaveOrUpdate(cacheVal);
			}, "EBusinessService.AddToCache(" + refNumber + ")");
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
		private readonly ConfigurationVariablesRepository configurationVariablesRepository;
		private readonly ExperianDL97AccountsRepository experianDL97AccountsRepository;
		private readonly string eSeriesUrl;

		#endregion properties

		#endregion private
	} // class EBusinessService
} // namespace
