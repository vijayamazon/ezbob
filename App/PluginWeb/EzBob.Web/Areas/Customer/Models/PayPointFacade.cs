namespace EzBob.Web.Areas.Customer.Models
{
	using System;
	using System.Globalization;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	public interface IPayPointFacade
    {
        bool CheckHash(string hash, Uri url);
        string GeneratePaymentUrl(bool bIsOffline, decimal amount, string callback, DateTime? dateOfBirth, string surname, string postcode, string accountNumber, string sortCode, bool deferred = false);
    }

    public class PayPointFacade : IPayPointFacade
	{
		private readonly string remotePassword;
		private readonly string mid;
		private readonly string templateUrl;
		private readonly string paypointOptions;

        public PayPointFacade()
		{
			var configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
			remotePassword = configurationVariablesRepository.GetByName("PayPointRemotePassword");
			mid = configurationVariablesRepository.GetByName("PayPointMid");
			templateUrl = configurationVariablesRepository.GetByName("PayPointTemplateUrl");
			paypointOptions = configurationVariablesRepository.GetByName("PayPointOptions");
        }

        public virtual bool CheckHash(string hash, Uri url)
        {
            var request = url.PathAndQuery;
            request = request.Replace("hash=" + hash, remotePassword);
            var digest = CalculateMD5Hash(request);
            return digest == hash;
        }

        protected string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLowerInvariant();
        }

        public string GeneratePaymentUrl(bool bIsOffline, decimal amount, string callback, DateTime? dateOfBirth, string surname, string postcode, string accountNumber, string sortCode, bool deferred = false)
        {
            var transactionId = Guid.NewGuid().ToString();
            var merchantId = mid;
            string amountStr = amount.ToString(CultureInfo.InvariantCulture);
            var digest = CalculateMD5Hash(transactionId + amountStr + remotePassword);

	        var dateOfBirthFormatted = dateOfBirth.HasValue? dateOfBirth.Value.ToString("yyyyMMdd") : "";
	        var surnameFormatted = surname.Substring(0, 6);
	        var accountNumberFormatted = accountNumber.Length >= 10
		                                     ? accountNumber.Substring(0, 10)
		                                     : string.Format("{0}{1}", accountNumber, sortCode).Substring(0, 10);
	        var postCodeFormatted = postcode.Split(' ')[0];
			var options = string.Format("{0};fin_serv_birth_date={1};fin_serv_surname={2};fin_serv_postcode={3};fin_serv_account={4}", paypointOptions, dateOfBirthFormatted, surnameFormatted, postCodeFormatted, accountNumberFormatted);
            if (deferred)
            {
                if (!string.IsNullOrEmpty(options) && options[options.Length-1] != ';')
                {
                    options += ";";
                }
                options += "deferred=true;";
            }
            var url = string.Format("https://www.secpay.com/java-bin/ValCard?amount={0}&merchant={1}&trans_id={2}&callback={3}&digest={4}&template={5}&options={6}&segmenttype={7}",
                amountStr,
                merchantId,
                transactionId,
                HttpUtility.UrlEncode(callback),
                digest,
                HttpUtility.UrlEncode(templateUrl),
                HttpUtility.UrlEncode(options),
				bIsOffline ? "offline" : "online"
			);
            return url;
        }
    }
}