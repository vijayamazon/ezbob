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
using Scorto.Configuration;
using StructureMap;
using log4net;

namespace ExperianLib.Ebusiness
{
    public class EBusinessService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EBusinessService));
        readonly ExperianIntegrationParams _config;

        public EBusinessService()
        {
            _config = ConfigurationRootBob.GetConfiguration().Experian;
        }

        //-----------------------------------------------------------------------------------
        public TargetResults TargetBusiness(string companyName, string postCode, int customerId)
        {
            try
            {
                companyName = HttpUtility.HtmlEncode(companyName);
                string requestXml = GetResource("ExperianLib.Ebusiness.TargetBusiness.xml", companyName, postCode);
                var response = MakeRequest("POST", "application/xml", requestXml);
                Utils.WriteLog(requestXml, response, "ESeriesTargeting", customerId);
                return new TargetResults(response);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------
        public LimitedResults GetLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly = false)
        {
            try
            {
                var response = CheckCache(regNumber);
                if (string.IsNullOrEmpty(response) && !checkInCacheOnly)
                {
                    string requestXml = GetResource("ExperianLib.Ebusiness.LimitedBusinessRequest.xml", regNumber);
                    response = MakeRequest("POST", "application/xml", requestXml);
                    Utils.WriteLog(requestXml, response, "E-SeriesLimitedData", customerId);
                    AddToCache(regNumber, requestXml, response);
                }
                return new LimitedResults(response);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new LimitedResults(e);
            }
        }

        //-----------------------------------------------------------------------------------
        public NonLimitedResults GetNotLimitedBusinessData(string regNumber, int customerId, bool checkInCacheOnly = false)
        {
            try
            {
                var response = CheckCache(regNumber);
                if (string.IsNullOrEmpty(response) && !checkInCacheOnly)
                {
                    string requestXml = GetResource("ExperianLib.Ebusiness.NonLimitedBusinessRequest.xml", regNumber);
                    response = MakeRequest("POST", "application/xml", requestXml);
                    Utils.WriteLog(requestXml, response, "E-SeriesNonLimitedData", customerId);
                    AddToCache(regNumber, requestXml, response);
                }

                return new NonLimitedResults(response);
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new NonLimitedResults(e);
            }
        }

        //-----------------------------------------------------------------------------------
        private string CheckCache(string refNumber)
        {
            var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();
            Log.InfoFormat("Checking cache for refNumber={0}...", refNumber);
            var cacheVal = (from c in repo.GetAll() where c.CompanyRefNumber == refNumber select c).FirstOrDefault();
            if (cacheVal != null)
            {
                if ((DateTime.Now - cacheVal.LastUpdateDate).TotalDays <= _config.UpdateBusinessDataPeriodDays)
                {
                    Log.InfoFormat("Returning data from cache for refNumber={0}", refNumber);
                    return cacheVal.JsonPacket;
                }
            }
            return null;
        }

        //-----------------------------------------------------------------------------------
        private void AddToCache(string refNumber, string request, string response)
        {
            var repo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_ExperianDataCache>>();
            var cacheVal = (from c in repo.GetAll() where c.CompanyRefNumber == refNumber select c).FirstOrDefault() ?? new MP_ExperianDataCache { CompanyRefNumber = refNumber };
            cacheVal.LastUpdateDate = DateTime.Now;
            cacheVal.JsonPacketInput = request;
            cacheVal.JsonPacket = response;
            repo.SaveOrUpdate(cacheVal);
        }
        //-----------------------------------------------------------------------------------
        private string MakeRequest(string method, string contentType, string post)
        {
            Log.DebugFormat("Request URL: {0} with data: {1}", _config.ESeriesUrl, post);
            var request = (HttpWebRequest)WebRequest.Create(_config.ESeriesUrl);
            request.Method = method;
            request.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(contentType)) request.ContentType = contentType;
            request.ContentLength = post.Length;
            var stOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
            stOut.Write(post);
            stOut.Close();
            WebResponse resp = request.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream());
            var respStr = sr.ReadToEnd();
            return respStr;
        }

        //-----------------------------------------------------------------------------------
        private string GetResource(string resName, params object[] p)
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
            {
                if (s == null) return null;
                var data = new byte[s.Length];
                s.Read(data, 0, (int) s.Length);
                return string.Format(Encoding.UTF8.GetString(data), p);
            }
        }
    }
}
