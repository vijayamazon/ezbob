using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.PayPal
{
    public class PayPalSecurityInfo
    {
        public string TokenSecret { get; set; }
        public string VerificationCode { get; set; }
        public string RequestToken { get; set; }
        public string AccessToken { get; set; }
        public string UserId { get; set; }
    }
}
