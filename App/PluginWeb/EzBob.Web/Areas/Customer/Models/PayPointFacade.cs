namespace EzBob.Web.Areas.Customer.Models
{
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using ConfigManager;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public interface IPayPointFacade
	{
		bool CheckHash(string hash, Uri url);
		string GeneratePaymentUrl(bool bIsOffline, decimal amount, string callback, DateTime? dateOfBirth, string surname, string postcode, string accountNumber, bool deferred = false);
		string GeneratePaymentUrl(Customer customer, decimal amount, string callback, bool deferred = false);
	}

	public class PayPointFacade : IPayPointFacade
	{
		private readonly string remotePassword;
		private readonly string mid;
		private readonly string templateUrl;
		private readonly string paypointOptions;

		public PayPointFacade(bool isDefault = true) {
			var payPointAccountRepository = ObjectFactory.GetInstance<PayPointAccountRepository>();
			PayPointAccount payPointAccount;
			if (isDefault) {
				payPointAccount = payPointAccountRepository.GetDefaultAccount();
			} else {
				payPointAccount = payPointAccountRepository.GetOldAccount();
			}
			remotePassword = payPointAccount.RemotePassword;
			mid = payPointAccount.Mid;
			templateUrl = payPointAccount.TemplateUrl;
			paypointOptions = payPointAccount.Options;
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

		public string GeneratePaymentUrl(Customer customer, decimal amount, string callback, bool deferred = false)
		{
			var isOffline = customer.IsOffline.HasValue && customer.IsOffline.Value;
			var address = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			var postCode = address != null ? address.Postcode : "";
			var accountNumber = customer.BankAccount != null && customer.BankAccount.AccountNumber != null ? customer.BankAccount.AccountNumber : "";
			DateTime? dateOfBirth = customer.PersonalInfo != null ? customer.PersonalInfo.DateOfBirth : null;
			var surname = customer.PersonalInfo != null ? customer.PersonalInfo.Surname : "";
			return GeneratePaymentUrl(isOffline, amount, callback, dateOfBirth, surname, postCode, accountNumber, deferred);
		}

		public string GeneratePaymentUrl(bool bIsOffline, decimal amount, string callback, DateTime? dateOfBirth, string surname, string postcode, string accountNumber, bool deferred = false)
		{
			var transactionId = Guid.NewGuid().ToString();
			var merchantId = mid;
			string amountStr = amount.ToString(CultureInfo.InvariantCulture);
			var digest = CalculateMD5Hash(transactionId + amountStr + remotePassword);

			var dateOfBirthFormatted = dateOfBirth.HasValue ? dateOfBirth.Value.ToString("yyyyMMdd") : "";
			surname = Regex.Replace(surname, "[^a-zA-Z]", String.Empty);
			var surnameFormatted = surname.Length > 6 ? surname.Substring(0, 6) : surname;
			var accountNumberFormatted = accountNumber.Length >= 10 ? accountNumber.Substring(0, 10) : accountNumber;
			var postCodeFormatted = "";
			postCodeFormatted = !postcode.Contains(" ") ? postcode.Substring(0, postcode.Length - 3) : postcode.Split(' ')[0];
			postCodeFormatted = postCodeFormatted.Length > 6 ? postCodeFormatted.Substring(0, 6) : postCodeFormatted;

			var options = string.Format("{0};fin_serv_birth_date={1};fin_serv_surname={2};fin_serv_postcode={3};fin_serv_account={4}", 
				paypointOptions, 
				dateOfBirthFormatted, 
				surnameFormatted, 
				postCodeFormatted, 
				accountNumberFormatted);
			if (deferred)
			{
				if (!string.IsNullOrEmpty(options) && options[options.Length - 1] != ';')
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