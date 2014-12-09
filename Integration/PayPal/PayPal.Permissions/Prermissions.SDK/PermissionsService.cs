using System;
using PayPal.Services.Private.Permissions;

namespace PayPal.Platform.SDK
{
     //<summary>
     //Permissions Wrapper class
     //</summary>
    public class Permissions : CallerServices
    {
        public static readonly string Endpoint = "Permissions/";
        private string result = string.Empty;
        private TransactionException lastError = null;
        private object res = string.Empty;

        /// <summary>
        /// Returns "SUCCESS" if API request returns Success response, else returns "FAILURE"
        /// </summary>
        public string isSuccess
        {
            get
            {
                return this.result;
            }
        }

        /// <summary>
        /// LastError
        /// </summary>
        public TransactionException LastError
        {
            get
            {
                return this.lastError;
            }

        }

        /// <summary>
        /// LastResponse
        /// </summary>
        public string LastResponse
        {
            get
            {
                return this.res.ToString();
            }
        }

        /// <summary>
        /// Calls Pay Platform API for the given PayRequest and returns PayResponse 
        /// </summary>
        /// <param name="request">PayRequest</param>
        /// <returns>PayResponse</returns>
        public RequestPermissionsResponse requestPermissions(RequestPermissionsRequest  request)
        {

            RequestPermissionsResponse PResponse = null;
            PayLoad = null;

            try
            {
                APIProfile.EndPointAppend = Endpoint + "RequestPermissions";
                if (APIProfile.RequestDataformat == "SOAP11")
                {
                    PayLoad = SoapEncoder.Encode(request);
                }
                else if (APIProfile.RequestDataformat == "XML")
                {
                    PayLoad = PayPal.Platform.SDK.XMLEncoder.Encode(request);
                }
                else
                {
                    PayLoad = PayPal.Platform.SDK.JSONSerializer.ToJavaScriptObjectNotation(request);
                }
                res = CallAPI();

                if (APIProfile.RequestDataformat == "JSON")
                {
                    object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse));
                    if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse))
                    {
                        PResponse = (PayPal.Services.Private.Permissions.RequestPermissionsResponse)obj;
                    }
                    string name = Enum.GetName(PResponse.responseEnvelope.ack.GetType(), PResponse.responseEnvelope.ack);

                    if (name == "Failure")
                    {
                        this.result = "FAILURE";
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }
                }

                else if (res.ToString().ToUpper().Replace("<ACK>FAILURE</ACK>", "").Length != res.ToString().Length)
                {
                    this.result = "FAILURE";

                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.SOAP11, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.XML, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }

                }
                else
                {
                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.RequestPermissionsResponse)SoapEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse));
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.RequestPermissionsResponse)XMLEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse));
                    }
                    else
                    {
                        object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse));
                        if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse))
                        {
                            PResponse = (PayPal.Services.Private.Permissions.RequestPermissionsResponse)obj;
                        }
                    }
                    this.result = "SUCCESS";

                }

            }
            catch (FATALException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new FATALException("Error occurred in Permissions -> RequestPermissions method.", ex);
            }
            return PResponse;
        }

        public CancelPermissionsResponse cancelPermissions(CancelPermissionsRequest request)
        {

            CancelPermissionsResponse PResponse = null;
            PayLoad = null;

            try
            {
                APIProfile.EndPointAppend = Endpoint + "CancelPermissions";
                if (APIProfile.RequestDataformat == "SOAP11")
                {
                    PayLoad = SoapEncoder.Encode(request);
                }
                else if (APIProfile.RequestDataformat == "XML")
                {
                    PayLoad = PayPal.Platform.SDK.XMLEncoder.Encode(request);
                }
                else
                {
                    PayLoad = PayPal.Platform.SDK.JSONSerializer.ToJavaScriptObjectNotation(request);
                }
                res = CallAPI();

                if (APIProfile.RequestDataformat == "JSON")
                {
                    object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.CancelPermissionsResponse));
                    if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.CancelPermissionsResponse))
                    {
                        PResponse = (PayPal.Services.Private.Permissions.CancelPermissionsResponse)obj;
                    }
                    string name = Enum.GetName(PResponse.responseEnvelope.ack.GetType(), PResponse.responseEnvelope.ack);

                    if (name == "Failure")
                    {
                        this.result = "FAILURE";
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }
                }

                else if (res.ToString().ToUpper().Replace("<ACK>FAILURE</ACK>", "").Length != res.ToString().Length)
                {
                    this.result = "FAILURE";

                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.SOAP11, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.XML, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }

                }
                else
                {
                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.CancelPermissionsResponse)SoapEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.CancelPermissionsResponse));
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.CancelPermissionsResponse)XMLEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.CancelPermissionsResponse));
                    }
                    else
                    {
                        object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.CancelPermissionsResponse));
                        if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.RequestPermissionsResponse))
                        {
                            PResponse = (PayPal.Services.Private.Permissions.CancelPermissionsResponse)obj;
                        }
                    }
                    this.result = "SUCCESS";

                }

            }
            catch (FATALException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new FATALException("Error occurred in Permissions -> CancelPermissions method.", ex);
            }
            return PResponse;
        }

        public GetAccessTokenResponse getAccessToken(GetAccessTokenRequest request)
        {

            GetAccessTokenResponse PResponse = null;
            PayLoad = null;

            try
            {
                APIProfile.EndPointAppend = Endpoint + "GetAccessToken";
                if (APIProfile.RequestDataformat == "SOAP11")
                {
                    PayLoad = SoapEncoder.Encode(request);
                }
                else if (APIProfile.RequestDataformat == "XML")
                {
                    PayLoad = PayPal.Platform.SDK.XMLEncoder.Encode(request);
                }
                else
                {
                    PayLoad = PayPal.Platform.SDK.JSONSerializer.ToJavaScriptObjectNotation(request);
                }
                res = CallAPI();

                if (APIProfile.RequestDataformat == "JSON")
                {
                    object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAccessTokenResponse));
                    if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetAccessTokenResponse))
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetAccessTokenResponse)obj;

                    }
                    string name = Enum.GetName(PResponse.responseEnvelope.ack.GetType(), PResponse.responseEnvelope.ack);

                    if (name == "Failure")
                    {
                        this.result = "FAILURE";
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }
                }

                else if (res.ToString().ToUpper().Replace("<ACK>FAILURE</ACK>", "").Length != res.ToString().Length)
                {
                    this.result = "FAILURE";

                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.SOAP11, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.XML, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }

                }
                else
                {
                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetAccessTokenResponse)SoapEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAccessTokenResponse));
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetAccessTokenResponse)XMLEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAccessTokenResponse));
                    }
                    else
                    {
                        object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAccessTokenResponse));
                        if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetAccessTokenResponse))
                        {
                            PResponse = (PayPal.Services.Private.Permissions.GetAccessTokenResponse)obj;
                        }
                    }
                    this.result = "SUCCESS";

                }

            }
            catch (FATALException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new FATALException("Error occurred in Permissions -> GetAccessToken method.", ex);
            }
            return PResponse;
        }

        public GetPermissionsResponse getPermissions(GetPermissionsRequest request)
        {

            GetPermissionsResponse PResponse = null;
            PayLoad = null;

            try
            {
                APIProfile.EndPointAppend = Endpoint + "GetPermissions";
                if (APIProfile.RequestDataformat == "SOAP11")
                {
                    PayLoad = SoapEncoder.Encode(request);
                }
                else if (APIProfile.RequestDataformat == "XML")
                {
                    PayLoad = PayPal.Platform.SDK.XMLEncoder.Encode(request);
                }
                else
                {
                    PayLoad = PayPal.Platform.SDK.JSONSerializer.ToJavaScriptObjectNotation(request);
                }
                res = CallAPI();

                if (APIProfile.RequestDataformat == "JSON")
                {
                    object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetPermissionsResponse));
                    if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetPermissionsResponse))
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetPermissionsResponse)obj;
                    }
                    string name = Enum.GetName(PResponse.responseEnvelope.ack.GetType(), PResponse.responseEnvelope.ack);

                    if (name == "Failure")
                    {
                        this.result = "FAILURE";
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }
                }

                else if (res.ToString().ToUpper().Replace("<ACK>FAILURE</ACK>", "").Length != res.ToString().Length)
                {
                    this.result = "FAILURE";

                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.SOAP11, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.XML, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }

                }
                else
                {
                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetPermissionsResponse)SoapEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetPermissionsResponse));
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetPermissionsResponse)XMLEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetPermissionsResponse));
                    }
                    else
                    {
                        object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetPermissionsResponse));
                        if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetPermissionsResponse))
                        {
                            PResponse = (PayPal.Services.Private.Permissions.GetPermissionsResponse)obj;
                        }
                    }
                    this.result = "SUCCESS";

                }

            }
            catch (FATALException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new FATALException("Error occurred in Permissions -> GetPermissions method.", ex);
            }
            return PResponse;
        }

        public GetBasicPersonalDataResponse getBasicPersonalData(GetBasicPersonalDataRequest request)
        {

            GetBasicPersonalDataResponse PResponse = null;
            PayLoad = null;

            try
            {
                APIProfile.EndPointAppend = Endpoint + "GetBasicPersonalData";
                if (APIProfile.RequestDataformat == "SOAP11")
                {
                    PayLoad = SoapEncoder.Encode(request);
                }
                else if (APIProfile.RequestDataformat == "XML")
                {
                    PayLoad = PayPal.Platform.SDK.XMLEncoder.Encode(request);
                }
                else
                {
                    PayLoad = PayPal.Platform.SDK.JSONSerializer.ToJavaScriptObjectNotation(request);
                }
                res = CallAPI();

                if (APIProfile.RequestDataformat == "JSON")
                {
                    object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse));
                    if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse))
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse)obj;
                    }
                    string name = Enum.GetName(PResponse.responseEnvelope.ack.GetType(), PResponse.responseEnvelope.ack);

                    if (name == "Failure")
                    {
                        this.result = "FAILURE";
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }
                }

                else if (res.ToString().ToUpper().Replace("<ACK>FAILURE</ACK>", "").Length != res.ToString().Length)
                {
                    this.result = "FAILURE";

                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.SOAP11, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.XML, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }

                }
                else
                {
                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse)SoapEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse));
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse)XMLEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse));
                    }
                    else
                    {
                        object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse));
                        if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse))
                        {
                            PResponse = (PayPal.Services.Private.Permissions.GetBasicPersonalDataResponse)obj;
                        }
                    }
                    this.result = "SUCCESS";

                }

            }
            catch (FATALException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new FATALException("Error occurred in Permissions ->  GetBasicPersonalData method.", ex);
            }
            return PResponse;
        }

        public GetAdvancedPersonalDataResponse getAdvancedPersonalData(GetAdvancedPersonalDataRequest request)
        {

            GetAdvancedPersonalDataResponse PResponse = null;
            PayLoad = null;

            try
            {
                APIProfile.EndPointAppend = Endpoint + "GetAdvancedPersonalData";
                if (APIProfile.RequestDataformat == "SOAP11")
                {
                    PayLoad = SoapEncoder.Encode(request);
                }
                else if (APIProfile.RequestDataformat == "XML")
                {
                    PayLoad = PayPal.Platform.SDK.XMLEncoder.Encode(request);
                }
                else
                {
                    PayLoad = PayPal.Platform.SDK.JSONSerializer.ToJavaScriptObjectNotation(request);
                }
                res = CallAPI();

                if (APIProfile.RequestDataformat == "JSON")
                {
                    object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse));
                    if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse))
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse)obj;
                    }
                    string name = Enum.GetName(PResponse.responseEnvelope.ack.GetType(), PResponse.responseEnvelope.ack);

                    if (name == "Failure")
                    {
                        this.result = "FAILURE";
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }
                }

                else if (res.ToString().ToUpper().Replace("<ACK>FAILURE</ACK>", "").Length != res.ToString().Length)
                {
                    this.result = "FAILURE";

                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.SOAP11, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.XML, res.ToString());
                        this.lastError = tranactionEx;
                    }
                    else
                    {
                        TransactionException tranactionEx = new TransactionException(PayLoadFromat.JSON, res.ToString());
                        this.lastError = tranactionEx;
                    }

                }
                else
                {
                    if (APIProfile.RequestDataformat == "SOAP11")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse)SoapEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse));
                    }
                    else if (APIProfile.RequestDataformat == "XML")
                    {
                        PResponse = (PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse)XMLEncoder.Decode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse));
                    }
                    else
                    {
                        object obj = JSONSerializer.JsonDecode(res.ToString(), typeof(PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse));
                        if (obj.GetType() == typeof(PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse))
                        {
                            PResponse = (PayPal.Services.Private.Permissions.GetAdvancedPersonalDataResponse)obj;
                        }
                    }
                    this.result = "SUCCESS";

                }

            }
            catch (FATALException)
            {
                throw;
            }
            catch (Exception ex)
            {

                throw new FATALException("Error occurred in Permissions ->  GetAdvancedPersonalData method.", ex);
            }
            return PResponse;
        }
    }
}
