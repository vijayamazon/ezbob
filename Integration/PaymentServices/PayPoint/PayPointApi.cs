﻿namespace PaymentServices.PayPoint
{
	using ConfigManager;
	using EZBob.DatabaseLib;
	using global::PayPoint;
	using System;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib.Model;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Calculators;
	using StructureMap;
	using log4net;

	public class PayPointApi
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PayPointApi));
		private readonly SECVPNService _service = new SECVPNService();
		private readonly ILoanRepository _loans;

		public PayPointApi()
		{
			_loans = ObjectFactory.GetInstance<ILoanRepository>();
		}

		public void PayPointPayPal(PayPointAccount account, string notificationUrl, string returnUrl, string cancelUrl, decimal amount, string currency = "GBP", bool isTest = false)
		{
			try
			{
				string transactionId = "TRAN" + Guid.NewGuid();
				string options = string.Format("notificationurl={0},returnurl={1},cancelurl={2}", notificationUrl, returnUrl, cancelUrl);
				if (isTest) options += ",test_status=true";
				_service.Url = account.ServiceUrl;

				var str = _service.
					performTransactionViaAlternatePaymentMethod(account.Mid, account.VpnPassword, "PayPal",
																			   "ExpressCheckout", "Initialise", "Transaction",
																			   transactionId,
																			   amount.ToString(CultureInfo.InvariantCulture),
																			   currency, options);
				Log.Debug(str);
				var ret = new PayPointReturnData(str);

				if (!ret.HasError)
				{

				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
		//-----------------------------------------------------------------------------------
		public PayPointReturnData RefundCard(PayPointAccount account, string cardHolder, string cardNumber, decimal amount, DateTime expiryDate, string issueNumber, DateTime startDate, string order, string cv2, bool isTest)
		{
			Log.InfoFormat("RefundCard: cardHolder={0}, cardNumber={1}, amount = {2}, expiryDate = {3}, issueNumber={4}, startDate={5}, order={6}, cv2={7}, isTest = {8}", cardHolder, cardNumber, amount, expiryDate, issueNumber, startDate, order, cv2, isTest);
			_service.Url = account.ServiceUrl;
			try
			{
				string transactionId = "TRAN" + Guid.NewGuid();
				string transactionIdNew = transactionId + "_refund";
				string options = String.Format("dups=false,card_type=Visa,cv2={0}", cv2);
				if (isTest) options += ",test_status=true";
				string startDateStr = startDate.ToString("MMyy");
				string expiryDateStr = expiryDate.ToString("MMyy");

				var str = _service.validateCardFull(account.Mid, account.VpnPassword, transactionId,
												   "127.0.0.1", cardHolder, cardNumber,
												   amount.ToString(CultureInfo.InvariantCulture),
												   expiryDateStr, issueNumber, startDateStr, order,
												   String.Empty, String.Empty, options);
				Log.Debug("validateCardFull result: " + str);
				var ret = new PayPointReturnData(str);

				if (!ret.HasError)
				{
					str = _service.refundCardFull(account.Mid, account.VpnPassword, transactionId, amount.ToString(CultureInfo.InvariantCulture), account.RemotePassword, transactionIdNew);
					ret = new PayPointReturnData(str);
					Log.Debug("refundCardFull result: " + str);
				}
				else Log.InfoFormat("RefundCard completed successfully");
				return ret;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return new PayPointReturnData { Error = ex.Message };
			}
		}

		//-----------------------------------------------------------------------------------
		public string GetReport(PayPointAccount account, string reportType, string filterType, string filter, string currency) {
			_service.Url = account.ServiceUrl;
			Log.InfoFormat("GetReport: reportType={0}, filterType={1}, filter = {2}, currency = {3}", reportType, filterType, filter, currency);
			try
			{
				var report = _service.getReport(account.Mid, account.VpnPassword, account.RemotePassword, reportType, filterType, filter, currency, String.Empty, false, false);

				if (report.Length > 1000) Log.Debug("GetReport result (first 1000 symbols): " + report.Substring(0, 1000));
				else Log.Debug("GetReport result: " + report);
				Log.InfoFormat("GetReport completed successfully");

				return report;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return null;
			}

		}

		//-----------------------------------------------------------------------------------
		/// <summary>
		/// Make automatic payment for given installment
		/// </summary>
		/// <param name="loanScheduleId">Installment Id</param>
		/// <param name="amount">Amount to pay</param>
		/// <returns>PayPointReturnData as a result of call to paypoint API</returns>
		public PayPointReturnData MakeAutomaticPayment(int loanScheduleId, decimal amount)
		{
			var installments = ObjectFactory.GetInstance<ILoanScheduleRepository>();
			var loanPaymentFacade = ObjectFactory.GetInstance<LoanPaymentFacade>();

			PayPointReturnData payPointReturnData = null;

			installments.BeginTransaction();

			try
			{
				var installment = installments.Get(loanScheduleId);
				var loan = installment.Loan;
				var customer = loan.Customer;

				Log.InfoFormat("Making automatic repayment for customer {0}(#{1}) for amount {2} for loan# {3}({4})",
							   customer.PersonalInfo.Fullname, customer.RefNumber, amount, loan.RefNumber, loan.Id);

				PayPointCard defaultCard = customer.PayPointCards.FirstOrDefault(x => x.IsDefaultCard);
				if (defaultCard == null && customer.PayPointCards.Any()) {
					defaultCard = customer.PayPointCards.First();
				}
				if (defaultCard == null) {
					throw new Exception("Debit card not found");
				}

				var payPointTransactionId = defaultCard.TransactionId;
				var now = DateTime.UtcNow;

				try
				{
					payPointReturnData = RepeatTransactionEx(defaultCard.PayPointAccount, payPointTransactionId, amount);
				}
				catch (PayPointException ex)
				{
					loan.Transactions.Add(new PaypointTransaction {
						Amount = amount,
						Description = ex.PaypointData.Message,
						PostDate = now,
						Status = LoanTransactionStatus.Error,
						PaypointId = payPointTransactionId,
						IP = "",
						Balance = loan.Balance,
						Principal = loan.Principal,
						Loan = loan,
						LoanTransactionMethod = ObjectFactory
							.GetInstance<DatabaseDataHelper>()
							.LoanTransactionMethodRepository
							.FindOrDefault("Auto"),
					});
					installments.CommitTransaction();
					return ex.PaypointData;
				}

				loanPaymentFacade.PayLoan(loan, payPointReturnData.NewTransId, amount, null, now);
				installments.CommitTransaction();
			}
			catch (Exception e)
			{
				if (!(e is PayPointException))
				{
					Log.Error(e);
				}
				if (payPointReturnData == null)
					payPointReturnData = new PayPointReturnData { Error = e.Message };
				installments.Dispose();
			}
			return payPointReturnData;
		}

		public PayPointReturnData RepeatTransactionEx(PayPointAccount account, string transactionId, decimal amount)
		{

			var newTransactionId = transactionId + DateTime.Now.ToString("yyyy-MM-dd_hh:mm:ss");
			string str;
			PayPointReturnData ret;

			Log.InfoFormat("RepeatTransaction: for transactionId='{0}', amount='{1}', newTransactionId='{2}'", transactionId, amount, newTransactionId);

			if (account.DebugModeEnabled)
			{
				var code = "A";
				var isValid = true;
				var message = "debug mode";
				var respCode = 0;

				if (!account.DebugModeIsValidCard)
				{
					message = "Card is not valid debug mode";
					code = "B";
					isValid = false;
				}
				else
				{
					if (account.EnableCardLimit && amount > account.CardLimitAmount)
					{
						message = "Amount more than card amount debug mode";
						code = "P:A";
						isValid = false;
					}
				}

				Random r = new Random();
				var rand = r.Next(10);
				if (account.DebugModeErrorCodeNEnabled && rand>6)
				{
					isValid = false;
					code = "N";
					message = "INSUFF FUNDS";
					respCode = 5;
				}

				str = string.Format("?valid={0}&trans_id={1}&code={2}&auth_code=9999&message={3}&resp_code={4}", isValid, transactionId, code, message, respCode);
				ret = new PayPointReturnData(str);
			}
			else
			{
				str = _service.repeatCardFullAddr(account.Mid, account.VpnPassword, transactionId,
												  amount.ToString(CultureInfo.InvariantCulture),account.RemotePassword,
												  newTransactionId, null, null, null, null, "repeat=true");
				ret = new PayPointReturnData(str);
			}

			if (ret.HasError)
			{
				if (ret.Code == "N")
				{
					Log.WarnFormat("RepeatTransaction error: {0} error {1} message {2} respCode {3}", str, ret.Error,ret.Message, (ResponseCode)ret.RespCode);
				}
				else
				{
					Log.ErrorFormat("RepeatTransaction error: {0} error {1} message {2} respCode {3}", str, ret.Error, ret.Message, (ResponseCode)ret.RespCode);
				}
				throw new PayPointException(str, ret);
			}
			Log.DebugFormat("RepeatTransaction successful: " + str);
			return ret;
		}

		public bool ApplyLateCharge(decimal amount, int loanId, int loanChargesTypeId)
		{
			var loan = _loans.Get(loanId);

			var date = DateTime.UtcNow;

			return ApplyLateCharge(loan, amount, loanChargesTypeId, date);
		}

		public bool ApplyLateCharge(Loan loan, decimal amount, int loanChargesTypeId, DateTime date)
		{
			var charge = new LoanCharge
							 {
								 Amount = amount,
								 ChargesType = new ConfigurationVariable(CurrentValues.Instance.GetByID(loanChargesTypeId)),
								 Date = date,
								 Loan = loan
							 };

			var res = loan.TryAddCharge(charge);

			var facade = new LoanPaymentFacade();
			facade.Recalculate(loan, date);

			_loans.Update(loan);

			return res;
		}

		public decimal GetAmountToPay(int installmentId)
		{
			try
			{
				Log.InfoFormat("Calculating payment for installment {0}", installmentId);

				var installments = ObjectFactory.GetInstance<ILoanScheduleRepository>();
				var facade = ObjectFactory.GetInstance<LoanPaymentFacade>();

				var installment = installments.Get(installmentId);
				var loan = installment.Loan;

				var state = facade.GetStateAt(loan, DateTime.Now);

				_loans.Update(loan);

				Log.InfoFormat("Amount to charge is: {0}", state.AmountDue);
				return state.AmountDue;
			}
			catch (Exception ex)
			{
                Log.Error(ex);
				return 0;
			}
		}
	}
}
