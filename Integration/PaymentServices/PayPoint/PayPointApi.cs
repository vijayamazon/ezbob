using System;
using System.Globalization;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Configuration;
using PaymentServices.Calculators;
using PaymentServices.Web_References.PayPoint;
using Scorto.Configuration;
using StructureMap;
using log4net;

namespace PaymentServices.PayPoint
{
    public class PayPointApi
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PayPointApi));

        readonly SECVPNService _service = new SECVPNService();
        private readonly PayPointConfiguration _config;
        private readonly LoanChargesRepository _loanChargesRepository;
        private ILoanRepository _loans;
        private ConfigurationVariablesRepository _vars;

        public PayPointApi()
        {
            _loanChargesRepository = ObjectFactory.GetInstance<LoanChargesRepository>();
            _vars = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
            _loans = ObjectFactory.GetInstance<ILoanRepository>();
            _config = ConfigurationRootBob.GetConfiguration().PayPoint;
            _service.Url = _config.ServiceUrl;
        }

        //-----------------------------------------------------------------------------------
        public PayPointReturnData RefundCard(string cardHolder, string cardNumber, decimal amount, DateTime expiryDate, string issueNumber, DateTime startDate, string order, string cv2, bool isTest)
        {
            Log.InfoFormat("RefundCard: cardHolder={0}, cardNumber={1}, amount = {2}, expiryDate = {3}, issueNumber={4}, startDate={5}, order={6}, cv2={7}, isTest = {8}", cardHolder, cardNumber, amount, expiryDate, issueNumber, startDate, order, cv2, isTest);

            try
            {
                string transactionId = "TRAN" + Guid.NewGuid();
                string transactionIdNew = transactionId + "_refund";
                string options = String.Format("dups=false,card_type=Visa,cv2={0}", cv2);
                if (isTest) options += ",test_status=true";
                string startDateStr = startDate.ToString("MMyy");
                string expiryDateStr = expiryDate.ToString("MMyy");

                var str = _service.validateCardFull(_config.Mid, _config.VpnPassword, transactionId,
                                                   "127.0.0.1", cardHolder, cardNumber,
                                                   amount.ToString(CultureInfo.InvariantCulture),
                                                   expiryDateStr, issueNumber, startDateStr, order,
                                                   String.Empty, String.Empty, options);
                Log.Debug("validateCardFull result: " + str);
                var ret = new PayPointReturnData(str);

                if (!ret.HasError)
                {
                    str = _service.refundCardFull(_config.Mid, _config.VpnPassword, transactionId, amount.ToString(CultureInfo.InvariantCulture), _config.RemotePassword, transactionIdNew);
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
        public string GetReport(string reportType, string filterType, string filter, string currency)
        {
            Log.InfoFormat("GetReport: reportType={0}, filterType={1}, filter = {2}, currency = {3}", reportType, filterType, filter, currency);
            try
            {
                var report = _service.getReport(_config.Mid, _config.VpnPassword, _config.RemotePassword, reportType, filterType, filter, currency, String.Empty, false, false);

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
        /// Make automatic payment for givven installment
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
                
                var payPointTransactionId = customer.PayPointTransactionId;

                if (loan.PayPointCard != null && !string.IsNullOrEmpty(loan.PayPointCard.TransactionId))
                {
                    payPointTransactionId = loan.PayPointCard.TransactionId;
                }

                var now = DateTime.UtcNow;

                try
                {
                    payPointReturnData = RepeatTransactionEx(payPointTransactionId, amount);
                }
                catch (PayPointException ex)
                {
                    Log.Error(ex);
                    loan.Transactions.Add(new PaypointTransaction()
                        {
                            Amount = amount,
                            Description = ex.PaypointData.Message,
                            PostDate = now,
                            Status = LoanTransactionStatus.Error,
                            PaypointId = payPointTransactionId,
                            IP = "",
                            Balance = loan.Balance,
                            Principal = loan.Principal,
                            Loan = loan
                        });
                    installments.CommitTransaction();
                    return ex.PaypointData;
                }

                loanPaymentFacade.PayLoan(loan, payPointReturnData.NewTransId, amount, null, now);
                installments.CommitTransaction();
            }
            catch (Exception e)
            {
                Log.Error(e);
                if(payPointReturnData == null)
                    payPointReturnData = new PayPointReturnData{ Error = e.Message };
                installments.Dispose();
            }
            return payPointReturnData;
        }

        //-----------------------------------------------------------------------------------
        public PayPointReturnData RepeatTransaction(string transactionId, decimal amount)
        {
            try
            {
                return RepeatTransactionEx(transactionId, amount);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return new PayPointReturnData { Error = ex.Message };
            }

        }

        public PayPointReturnData RepeatTransactionEx(string transactionId, decimal amount)
        {

            var newTransactionId = transactionId + DateTime.Now.ToString("yyyy-MM-dd_hh:mm:ss");
            string str;
            PayPointReturnData ret;

            Log.InfoFormat("RepeatTransaction: for transactionId='{0}', amount='{1}', newTransactionId='{2}'", transactionId, amount, newTransactionId);

            if (_config.DebugMode)
            {
                var tempErrorStr = "";
                var code = "A";
                var isValid = true;

                if (!_config.IsValidCard)
                {
                    tempErrorStr = "Card is not valid";
                    code = "B";
                    isValid = false;
                }
                else
                {
                    if (_config.EnableCardLimit && amount > _config.CardLimitAmount)
                    {
                        tempErrorStr = "Amount more than card amount";
                        code = "P:A";
                        isValid = false;
                    }
                }

                str = string.Format("?valid={0}&trans_id={1}&code={2}&auth_code=9999", isValid, transactionId,code);

                ret = new PayPointReturnData
                    {
                        AuthCode = "",
                        Code = code,
                        Error = tempErrorStr,
                        Message = "",
                        NewTransId = newTransactionId,
                        OutStr = str
                    };
            }
            else
            {
                str = _service.repeatCardFullAddr(_config.Mid, _config.VpnPassword, transactionId,
                                                  amount.ToString(CultureInfo.InvariantCulture), _config.RemotePassword,
                                                  newTransactionId, null, null, null, null, "repeat=true");
                ret = new PayPointReturnData(str);
            }

            if (ret.HasError)
            {
                Log.Error("RepeatTransaction error: " + str);
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
                                 ChargesType = _vars.Load(loanChargesTypeId),
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
            catch (Exception )
            {
                return 0;
            }
        }
    }
}
