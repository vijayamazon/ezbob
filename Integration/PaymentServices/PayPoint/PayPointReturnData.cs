using System;
using System.Collections.Generic;
using System.Linq;

namespace PaymentServices.PayPoint
{
	public enum ResponseCode
	{
		Success = 0,
		Referral = 2,
		Referral2 = 83,
		NotAuthorised = 5,
		NotAuthorized2 = 54,
		GeneralError = 30,
	}

	public class PayPointReturnData
    {
        public string Error { get; set; }
        public bool HasError
        {
            get { return !String.IsNullOrEmpty(Error); }
        }
        public string OutStr { get; set; }
        public string AuthCode { get; set; }
        public string NewTransId { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
		public int RespCode { get; set; }
        
        //-----------------------------------------------------------------------------------
        public PayPointReturnData()
        {
            
        }

        //-----------------------------------------------------------------------------------
        public PayPointReturnData(string outStr)
        {
            OutStr = outStr;
	        Message = string.Empty;
            if (String.IsNullOrEmpty(outStr))
            {
                Error = "Response is null";
                return;
            }
            outStr = outStr.TrimStart('?');
            var dict = ParseParams(outStr);

            if (dict.ContainsKey("code"))
            {
				Message = string.Concat(PayPointStatusTranslator.TranslateStatusCode(dict["code"]));
                Code = dict["code"];
                if (Code != "A") Error = Message;
            }

            if (dict.ContainsKey("auth_code")) AuthCode = dict["auth_code"];
            if (dict.ContainsKey("trans_id")) NewTransId = dict["trans_id"];

            if ((dict.ContainsKey("valid") && dict["valid"] == "false"))
            {
                Error = dict["message"] ?? "OutStr string contain valid=false, please versify OutStr for additional details";
            }

			if (dict.ContainsKey("resp_code"))
			{
				int respCode = 0;
				int.TryParse(dict["resp_code"], out respCode);
				RespCode = respCode;
			}
        }

        private Dictionary<string, string> ParseParams(string input)
        {
            return input.Split('&').Select(str => str.Split('=')).Where(parts => parts.Length == 2).ToDictionary(parts => parts[0], parts => parts[1]);
        }

    }
}
