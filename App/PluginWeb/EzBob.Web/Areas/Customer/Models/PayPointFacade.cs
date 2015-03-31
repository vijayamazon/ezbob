namespace EzBob.Web.Areas.Customer.Models {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database;
	using StructureMap;

	public interface IPayPointFacade {
		bool CheckHash(string hash, Uri url);

		string GeneratePaymentUrl(
			bool bIsOffline,
			decimal amount,
			string callback,
			DateTime? dateOfBirth,
			string surname,
			string postcode,
			string accountNumber,
			bool deferred = false
		);

		string GeneratePaymentUrl(Customer customer, decimal amount, string callback, bool deferred = false);
	} // interface IPayPointFacade

	public class PayPointFacade : IPayPointFacade {
		public PayPointAccount PayPointAccount { get; private set; }

		public PayPointFacade(DateTime? firstOpenLoanDate = null) {
			var payPointAccountRepository = ObjectFactory.GetInstance<PayPointAccountRepository>();
			PayPointAccount = payPointAccountRepository.GetAccount(firstOpenLoanDate);

			this.remotePassword = PayPointAccount.RemotePassword;
			this.mid = PayPointAccount.Mid;
			this.templateUrl = PayPointAccount.TemplateUrl;
			this.paypointOptions = PayPointAccount.Options;
		} // constructor

		public virtual bool CheckHash(string hash, Uri url) {
			log.Debug("CheckHash(\n\thash: '{0}',\n\turl: '{1}'\n) started...)", hash, url);

			string request = url.PathAndQuery;

			log.Debug("CheckHash: request (before replace) = '{0}'", request);

			request = request.Replace("hash=" + hash, this.remotePassword);

			log.Debug("CheckHash: request (after replace) = '{0}'", request);

			var digest = CalculateMD5Hash(request);

			log.Debug("CheckHash: digest = '{0}'", request);

			bool result = digest == hash;

			log.Debug("CheckHash(\n\thash: '{0}',\n\turl: '{1}'\n) = {2}.", hash, url, result);

			return result;
		} // CheckHash

		public string GeneratePaymentUrl(Customer customer, decimal amount, string callback, bool deferred = false) {
			bool isOffline = customer.IsOffline.HasValue && customer.IsOffline.Value;
			CustomerAddress address = customer.AddressInfo.PersonalAddress.FirstOrDefault();
			string postCode = address != null ? address.Postcode : "";
			string accountNumber = customer.BankAccount != null && customer.BankAccount.AccountNumber != null
				? customer.BankAccount.AccountNumber
				: "";
			DateTime? dateOfBirth = customer.PersonalInfo != null ? customer.PersonalInfo.DateOfBirth : null;
			string surname = customer.PersonalInfo != null ? customer.PersonalInfo.Surname : "";
			return GeneratePaymentUrl(isOffline, amount, callback, dateOfBirth, surname, postCode, accountNumber, deferred);
		} // GeneratePaymentUrl

		public string GeneratePaymentUrl(
			bool bIsOffline,
			decimal amount,
			string callback,
			DateTime? dateOfBirth,
			string surname,
			string postcode,
			string accountNumber,
			bool deferred = false
		) {
			var transactionId = Guid.NewGuid().ToString();
			var merchantId = this.mid;
			string amountStr = amount.ToString(CultureInfo.InvariantCulture);
			var digest = CalculateMD5Hash(transactionId + amountStr + this.remotePassword);

			var dateOfBirthFormatted = dateOfBirth.HasValue ? dateOfBirth.Value.ToString("yyyyMMdd") : "";
			surname = Regex.Replace(surname, "[^a-zA-Z]", String.Empty);
			var surnameFormatted = surname.Length > 6 ? surname.Substring(0, 6) : surname;
			var accountNumberFormatted = accountNumber.Length >= 10 ? accountNumber.Substring(0, 10) : accountNumber;

			string postCodeFormatted = !postcode.Contains(" ")
				? postcode.Substring(0, postcode.Length - 3)
				: postcode.Split(' ')[0];

			postCodeFormatted = postCodeFormatted.Length > 6 ? postCodeFormatted.Substring(0, 6) : postCodeFormatted;

			var options = string.Format(
				"{0};fin_serv_birth_date={1};fin_serv_surname={2};fin_serv_postcode={3};fin_serv_account={4}",
				this.paypointOptions,
				dateOfBirthFormatted,
				surnameFormatted,
				postCodeFormatted,
				accountNumberFormatted
			);

			if (deferred) {
				if (!string.IsNullOrEmpty(options) && options[options.Length - 1] != ';')
					options += ";";

				options += "deferred=true;";
			} // if

			var url = string.Format(
				"https://www.secpay.com/java-bin/ValCard" +
				"?amount={0}&merchant={1}&trans_id={2}&callback={3}&digest={4}&template={5}&options={6}&segmenttype={7}",
				amountStr,
				merchantId,
				transactionId,
				HttpUtility.UrlEncode(callback),
				digest,
				HttpUtility.UrlEncode(this.templateUrl),
				HttpUtility.UrlEncode(options),
				bIsOffline ? "offline" : "online"
			);

			return url;
		} // GeneratePaymentUrl

		protected string CalculateMD5Hash(string input) {
			// step 1, calculate MD5 hash from input
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < hash.Length; i++)
				sb.Append(hash[i].ToString("X2"));

			return sb.ToString().ToLowerInvariant();
		} // CalculateMD5Hash

		private readonly string remotePassword;
		private readonly string mid;
		private readonly string templateUrl;
		private readonly string paypointOptions;

		private static readonly ASafeLog log = new SafeILog(typeof(PayPointFacade));
	} // class PayPointFacade
} // namespace