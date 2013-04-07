using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Infrastructure;
using NHibernate;
using Newtonsoft.Json;
using Scorto.Web;
using log4net;

namespace EzBob.Web.Controllers
{
    public class PostcodeController : Controller
    {
        private readonly string _datakey;
        private readonly IEzBobConfiguration _config;
        private readonly ISession _session;
        private readonly IEzbobWorkplaceContext _context;

        private static readonly ILog _log = LogManager.GetLogger(typeof(PostcodeController));

        public PostcodeController(IEzBobConfiguration config, ISession session, IEzbobWorkplaceContext context)
        {
            _config = config;
            _session = session;
            _context = context;
            _datakey = _config.PostcodeConnectionKey;
        }

        [Authorize]
        [OutputCache(VaryByParam = "postCode", Duration = 3600 * 24 * 7)]
        [Transactional]
        public JsonNetResult GetAddressFromPostCode(string postCode)
        {
            return
                PostToPostcodeService(
                    "http://www.simplylookupadmin.co.uk/JSONservice/JSONSearchForAddress.aspx?datakey=" + _datakey + "&postcode=" +
                    postCode, 0);
        }
        //-------------------------------------------------------------------
        [Authorize]
        [OutputCache(VaryByParam = "id", Duration = 3600 * 24 * 7)]
        [Transactional]
        public JsonNetResult GetFullAddressFromPostCode(string id)
        {
            return
                PostToPostcodeService(
                    "http://www.simplylookupadmin.co.uk/JSONservice/JSONGetAddressRecord.aspx?datakey=" + _datakey + "&id=" + id, 1);
        }
        //-------------------------------------------------------------------
        
        private JsonNetResult PostToPostcodeService(string url, byte typePost)
        {
            var postcodeServiseRepository = new NHibernateRepositoryBase<PostcodeServiceLog>(_session);
            var postcodeServiceLog = new PostcodeServiceLog
            {
                Customer = _context.Customer,
                InsertDate = DateTime.Now,
                RequestType = typePost == 0 ? "Get list of address by postcode" : "Get address details by postcode id",
                RequestData = url
            };
            try
            {
                var request =
                      (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";
                request.Accept = "application/json";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var stream = response.GetResponseStream();

                    using (var reader = new StreamReader(stream))
                    {
                        var responseData = reader.ReadToEnd();
                        var model = typePost == 0 ? (object)JsonConvert.DeserializeObject<PostcodeSearchListModel>(responseData) :
                                                JsonConvert.DeserializeObject<PostcodeFullAddressModel>(responseData);

                        if (!string.IsNullOrEmpty(((IPostcode)model).Errormessage))
                        {
                            _log.ErrorFormat("Postcode service return error: {0}", ((IPostcode)model).Errormessage);
                        }
                        _log.DebugFormat("Postcode service credit text: {0}", ((IPostcode)model).Credits_display_text);

                        response.Close();

                        postcodeServiceLog.Status = "Successful";
                        postcodeServiceLog.ResponseData = responseData;

                        postcodeServiseRepository.Save(postcodeServiceLog);
                        return ((IPostcode)model).Found == 0
                                    ? this.JsonNet(new { Success = false, Message = "Not found" })
                                    : this.JsonNet(model);
                    }
                }
            }
            catch (Exception e)
            {
                postcodeServiceLog.Status = "Failed";
                postcodeServiceLog.ErrorMessage = e.ToString();
                return this.JsonNet(new { Success = false, Message = "Not found" });
            }
            finally
            {
                postcodeServiseRepository.Save(postcodeServiceLog);
            }
        }

    }
}
