using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Configuration;
using StructureMap;
using log4net;

namespace ExperianLib.Ebusiness {
	public class EBusinessService {
		public EBusinessService() {
			_config = ConfigurationRootBob.GetConfiguration().Experian;
		} // constructor

		public TargetResults TargetBusiness(string companyName, string postCode, int customerId, TargetResults.LegalStatus nFilter, string regNum = "") {
			try {
				companyName = HttpUtility.HtmlEncode(companyName);
				string requestXml = GetResource("ExperianLib.Ebusiness.TargetBusiness.xml", companyName, postCode, regNum);
				var response = MakeRequest("POST", "application/xml", requestXml);
				Utils.WriteLog(requestXml, response, "ESeriesTargeting", customerId);
				return new TargetResults(response, nFilter);
			}
			catch (Exception e) {
				Log.Error(e);
				throw;
			} // try
		} // TargetBusiness

		public LimitedResults GetLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly = false) {
			LimitedResults oRes = GetOneLimitedBusinessData(regNumber, customerId, checkInCacheOnly);

			foreach (string sOwnerRegNum in oRes.Owners)
				GetOneLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly);

			return oRes;
		} // GetLimitedBusinessData

		private LimitedResults GetOneLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly) {
			try {
				string response = CheckCache(regNumber);

				if (string.IsNullOrEmpty(response) && !checkInCacheOnly) {
					string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);

					response = MakeRequest("POST", "application/xml", requestXml);

					Utils.WriteLog(requestXml, response, "E-SeriesLimitedData", customerId);

					AddToCache(regNumber, requestXml, response);
				} // if

				return new LimitedResults(response);
			}
			catch (Exception e) {
				Log.Error(e);
				return new LimitedResults(e);
			} // try
		} // GetOneLimitedBusinessData

		public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly = false) {
			NonLimitedResults oRes = GetOneNotLimitedBusinessData(regNumber, customerId, checkInCacheOnly);

			foreach (string sOwnerRegNum in oRes.Owners)
				GetOneNotLimitedBusinessData(sOwnerRegNum, customerId, checkInCacheOnly);

			return oRes;
		} // GetNotLimitedBusinessData

		private NonLimitedResults GetOneNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly) {
			try {
				var response = CheckCache(regNumber);

				if (string.IsNullOrEmpty(response) && !checkInCacheOnly) {
					string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", regNumber);

					response = MakeRequest("POST", "application/xml", requestXml);

					Utils.WriteLog(requestXml, response, "E-SeriesNonLimitedData", customerId);

					AddToCache(regNumber, requestXml, response);
				} // if

				return new NonLimitedResults(response);
			}
			catch (Exception e) {
				Log.Error(e);
				return new NonLimitedResults(e);
			} // try
		} // GetOneNotLimitedBusinessData

		private string CheckCache(string refNumber) {
			var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

			Log.InfoFormat("Checking cache for refNumber={0}...", refNumber);

			var cacheVal = (from c in repo.GetAll() where c.CompanyRefNumber == refNumber select c).FirstOrDefault();

			if (cacheVal != null) {
				if ((DateTime.Now - cacheVal.LastUpdateDate).TotalDays <= _config.UpdateBusinessDataPeriodDays) {
					Log.InfoFormat("Returning data from cache for refNumber={0}", refNumber);
					return cacheVal.JsonPacket;
				} // if
			} // if

			return null;
		} // CheckCache

		private void AddToCache(string refNumber, string request, string response) {
			var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();

			var cacheVal =
				(from c in repo.GetAll() where c.CompanyRefNumber == refNumber select c).FirstOrDefault()
				?? new MP_ExperianDataCache { CompanyRefNumber = refNumber };

			cacheVal.LastUpdateDate = DateTime.UtcNow;
			cacheVal.JsonPacketInput = request;
			cacheVal.JsonPacket = response;

			repo.SaveOrUpdate(cacheVal);
		} // AddToCache

		private string MakeRequest(string method, string contentType, string post) {
			Log.DebugFormat("Request URL: {0} with data: {1}", _config.ESeriesUrl, post);

			var request = (HttpWebRequest)WebRequest.Create(_config.ESeriesUrl);
			request.Method = method;
			request.AllowAutoRedirect = false;

			if (!string.IsNullOrEmpty(contentType))
				request.ContentType = contentType;

			request.ContentLength = post.Length;

			var stOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
			stOut.Write(post);
			stOut.Close();

			WebResponse resp = request.GetResponse();

			var sr = new StreamReader(resp.GetResponseStream());

			var respStr = sr.ReadToEnd();

			return respStr;
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

		private static readonly ILog Log = LogManager.GetLogger(typeof(EBusinessService));
		private readonly ExperianIntegrationParams _config;
	} // class EBusinessService
} // namespace
