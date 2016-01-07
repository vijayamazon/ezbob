using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.PayPalService.Soap
{
    public class PayPalSoapConfig
    {
        public string Mode { get; set; }
        public string ApiUsername { get; set; }
        public string ApiPassword { get; set; }
        public string ApiSignature { get; set; }

        public string ApplicationId { get; set; }

        public string ConnectionTimeout { get; set; }
        public string RequestRetries { get; set; }

        public string RedirectUrl { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                {"mode", Mode},
                {"requestRetries", RequestRetries},
                {"connectionTimeout", ConnectionTimeout},
                {"account1.apiUsername", ApiUsername},
                {"account1.apiPassword", ApiPassword},
                {"account1.apiSignature", ApiUsername},
                {"account1.applicationId", ApplicationId}
            };
        }
    }
}
