using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.API;

namespace PaymentServices.PacNet
{
    public class PacnetReturnData
    {
        public string Error { get; set; }
        public bool HasError
        {
            get { return !String.IsNullOrEmpty(Error); }
        }
        public Dictionary<string, string> OutParams { get; set; }
        public string TrackingNumber { get; set; }
        public string Status { get; set; }
        
        //-----------------------------------------------------------------------------------
        public PacnetReturnData()
        {
            
        }

        //-----------------------------------------------------------------------------------
        public PacnetReturnData(RavenResponse response)
        {
            if (response == null)
            {
                Error = "Response is null";
                return;
            }
            var data = response.GetParams();
            OutParams = data;

            if (data.ContainsKey("HTTPStatus") && data["HTTPStatus"] != "OK")
            {
                Error = "Http status was: " + data["HTTPStatus"];
            }

            if(data.ContainsKey("TrackingNumber"))
            {
                TrackingNumber = data["TrackingNumber"];
            }

            if (data.ContainsKey("Status"))
            {
                Status = data["Status"];
            }
        }

    }
}
