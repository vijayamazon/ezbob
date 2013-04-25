using System;
using System.IO;
using System.Net;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Infrastructure;
using NHibernate;
using Newtonsoft.Json;
using log4net;

namespace EzBob.Web.Code.PostCode
{
    public class SimplyPostCodeFacade : IPostCodeFacade
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(SimplyPostCodeFacade));
        private readonly string _datakey;
        private readonly NHibernateRepositoryBase<PostcodeServiceLog> _postcodeServiseRepository;

        public SimplyPostCodeFacade(ISession session, IEzBobConfiguration config)
        {
            _postcodeServiseRepository = new NHibernateRepositoryBase<PostcodeServiceLog>(session);
            _datakey = config.PostcodeConnectionKey;
        }


        public IPostCodeResponse GetAddressFromPostCode(Customer customer, string postCode)
        {
            var url = "http://www.simplylookupadmin.co.uk/JSONservice/JSONSearchForAddress.aspx?datakey=" + _datakey + "&postcode=" + postCode;
            return PostToPostcodeService(customer, url, 0);
        }

        public IPostCodeResponse GetFullAddressFromPostCode(Customer customer, string id)
        {
            var url = "http://www.simplylookupadmin.co.uk/JSONservice/JSONGetAddressRecord.aspx?datakey=" + _datakey + "&id=" + id;
            return PostToPostcodeService(customer, url, 1);
        }

        private IPostCodeResponse PostToPostcodeService(Customer customer, string url, byte typePost)
        {
            var postcodeServiceLog = new PostcodeServiceLog
                                         {
                                             Customer = customer,
                                             InsertDate = DateTime.Now,
                                             RequestType = typePost == 0 ? "Get list of address by postcode" : "Get address details by postcode id",
                                             RequestData = url
                                         };
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "GET";
                request.Accept = "application/json";

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var stream = response.GetResponseStream();

                    using (var reader = new StreamReader(stream))
                    {
                        var responseData = reader.ReadToEnd();
                        var model = typePost == 0 ? (IPostCodeResponse)JsonConvert.DeserializeObject<PostCodeResponseSearchListModel>(responseData) :
                                        JsonConvert.DeserializeObject<PostCodeResponseFullAddressModel>(responseData);

                        if (!string.IsNullOrEmpty(model.Errormessage))
                        {
                            _log.ErrorFormat("Postcode service return error: {0}", (model).Errormessage);
                        }
                        _log.DebugFormat("Postcode service credit text: {0}", (model).Credits_display_text);

                        response.Close();

                        postcodeServiceLog.Status = "Successful";
                        postcodeServiceLog.ResponseData = responseData;

                        _postcodeServiseRepository.Save(postcodeServiceLog);

                        if (model.Found == 0)
                        {
                            model.Success = false;
                            model.Message = "Not found";
                        }
                        else
                        {
                            model.Success = true;
                            model.Message = "";
                        }

                        return model;
                    }
                }
            }
            catch (Exception e)
            {
                postcodeServiceLog.Status = "Failed";
                postcodeServiceLog.ErrorMessage = e.ToString();
                return new PostCodeResponseSearchListModel {Success = false, Message = "Not found" };
            }
            finally
            {
                _postcodeServiseRepository.Save(postcodeServiceLog);
            }
        }
    }
}