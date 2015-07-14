namespace Ezbob.Backend.Strategies.NewLoan
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using DbConstants;
    using Ezbob.Backend.Models.NewLoan;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;
    using MailApi;
    using PaymentServices.Calculators;

    public class AddLoan : AStrategy
    {

        public AddLoan(NL_Model nlModel)
        {
            NLModel = nlModel;
            Loader = new NL_Loader(NLModel);

            this.emailToAddress = "elinar@ezbob.com";
            this.emailFromAddress = "elinar@ezbob.com";
            this.emailFromName = "ezbob-system";
        }//constructor

        public override string Name { get { return "AddLoan"; } }
        public NL_Model Result; // output

        /// <exception cref="Exception">Add loan failed: {0}</exception>
        public override void Execute()
        {

            Log.Debug("------------------ customer {0}------------------------", NLModel.CustomerID);

            string message;

            if (NLModel.CustomerID == 0)
            {
                this.Result.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
                return;
            }

            OfferForLoan dataForLoan = DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", NLModel.CustomerID), new QueryParameter("@Now", DateTime.UtcNow));

            if (dataForLoan == null)
            {
                this.Result.Error = NL_ExceptionOfferNotValid.DefaultMessage;
                return;
            }

            Log.Debug(dataForLoan.ToString());

            // check if "creadit available" is enough for this loan amount
            // loan for the offer exists
            /*SafeReader sr = DB.GetFirst(string.Format("SELECT LoanID FROM NL_Loans WHERE OfferID={0}", dataForLoan.OfferID));
            if (sr["LoanID"] > 0) {
                  message = string.Format("Loan for customer {0} and offer {1} exists", NLModel.CustomerID, dataForLoan.OfferID);
                  Log.Alert(message);
                  throw new NL_ExceptionLoanExists(message);
            }*/

            // input validation
            if (NLModel.Loan == null)
            {
                message = string.Format("Expected input data not found (NL_Model initialized by: Loan.OldLoanID, Loan.InitialLoanAmount, Loan.Refnum). Customer {0}", NLModel.CustomerID);
                this.Result.Error = message;
                return;
            }

            if (NLModel.FundTransfer == null)
            {
                message = string.Format("Expected input data not found (NL_Model initialized by: FundTransfer). Customer {0}", NLModel.CustomerID);
                this.Result.Error = message;
                return;
            }

            if (NLModel.LoanAgreements == null)
            {
                message = string.Format("Expected input data not found (NL_Model initialized by: LoanAgreements list). Customer {0}", NLModel.CustomerID);
                this.Result.Error = message;
                return;
            }

            // complete other validations here

            /*** 
            //CHECK "Enough Funds" (uncomment WHEN old BE REMOVED from \App\PluginWeb\EzBob.Web\Code\LoanCreator.cs, method CreateLoan method)                    
            VerifyEnoughAvailableFunds enoughAvailableFunds = new VerifyEnoughAvailableFunds(NLModel.Loan.InitialLoanAmount);
            enoughAvailableFunds.Execute();
            if (!enoughAvailableFunds.HasEnoughFunds) {
                  Log.Alert("No enough funds for loan: customer {0}; offer {1}", NLModel.CustomerID, dataForLoan.Offer.OfferID);
            }
            ****/

            // TODO check usage of DefaultRepaymentPeriod from LoanSource
            // TODO additional check to be replicated here in master, commits 
            // adding safe check before creating loan that the interest rate is correct 4b6d139658203863dc57cc13748ea49da498ff27
            // add server side validation of repayment period when CreateLoan  b6926a4428919dd0343dd4f8c7ceb0d5d6b1b03d

            /**
            - loan
            - fees
            - history
            - schedules
            - broker comissions ? - update NLLoanID
            - fund transfer
            - pacnet transaction
            */

            int fundTransferID = 0;
            List<NL_LoanSchedules> scheduleItems = new List<NL_LoanSchedules>();
            List<NL_LoanFees> fees = new List<NL_LoanFees>();
            NL_LoanHistory history = null;

            ConnectionWrapper pconn = DB.GetPersistent();

            try
            {

                // order: 1.method argument 2. from LoanLegals 3.from the offer
                if (NLModel.Loan.InitialLoanAmount != dataForLoan.LoanLegalAmount)
                {
                    Log.Alert("Mismatch InitialLoanAmount param amount {0}, LoanLegalAmount {1}, loan: {2}", NLModel.Loan.InitialLoanAmount, dataForLoan.LoanLegalAmount, NLModel.Loan.Refnum);
                } // by default, use method argument amount value

                // 1. set required data for Schedule and Fees calculation
                NLModel.Loan.IssuedTime = DateTime.UtcNow;

                // 2. Generate Schedule and Fees
                CalculateLoanSchedule sCalcScheduleAndFee = new CalculateLoanSchedule(NLModel);
                sCalcScheduleAndFee.Context.UserID = this.NLModel.UserID;
                sCalcScheduleAndFee.Execute();
                if (sCalcScheduleAndFee.Result.Schedule.Count == 0 || sCalcScheduleAndFee.Result.Fees.Count == 0)
                {
                    message = "Failed to get Schedule|Fees. " + sCalcScheduleAndFee.Result.Error;
                    this.Result.Error = message;
                    return;
                }

                // 3. prepare NL_Loans object
                NLModel.Loan.CreationTime = DateTime.UtcNow;
                NLModel.Loan.LoanStatusID = (int)NLLoanStatuses.Live; //liveLoanStatusID;

                // from offer
                NLModel.Loan.RepaymentCount = dataForLoan.LoanLegalRepaymentPeriod;
                NLModel.Loan.OfferID = dataForLoan.OfferID;
                NLModel.Loan.LoanTypeID = dataForLoan.LoanTypeID;
                NLModel.Loan.RepaymentIntervalTypeID = dataForLoan.RepaymentIntervalTypeID;
                NLModel.Loan.LoanSourceID = dataForLoan.LoanSourceID;
                NLModel.Loan.InterestRate = dataForLoan.MonthlyInterestRate;
                NLModel.Loan.InterestOnlyRepaymentCount = dataForLoan.InterestOnlyRepaymentCount;
                NLModel.Loan.Position = dataForLoan.LoansCount;

                //Log.Debug(NLModel.Loan.ToString());

                pconn.BeginTransaction();

                // 4. save loan
                this.LoanID = DB.ExecuteScalar<int>(pconn, "NL_LoansSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.Loan));

                Log.Debug("NL_LoansSave: LoanID: {0}", this.LoanID);

                // 5. save setup fees
                foreach (NLFeeItem f in sCalcScheduleAndFee.Result.Fees)
                {
                    if (f.Fee.LoanFeeTypeID == (int)FeeTypes.SetupFee)
                    {
                        f.Fee.LoanID = this.LoanID;
                        fees.Add(f.Fee);
                    }
                }

                DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", fees));

                // 6. broker comissions
                // done in controller. When old loan removed: check if this is the broker's customer, calc broker fees, insert into LoanBrokerCommission
                var feeCalculator = new SetupFeeCalculator(dataForLoan.SetupFeePercent, dataForLoan.BrokerSetupFeePercent); // moved to CalculateLoanSchedule
                var brokerComissions = feeCalculator.CalculateBrokerFee(NLModel.Loan.InitialLoanAmount);
                if (brokerComissions > 0)
                {
                    DB.ExecuteNonQuery(string.Format("UPDATE dbo.LoanBrokerCommission SET NLLoanID = {0} WHERE LoanID = {1}", this.LoanID, NLModel.Loan.OldLoanID));
                }

                // 7. history
                history = new NL_LoanHistory()
                {
                    LoanID = this.LoanID,
                    UserID = NLModel.UserID,
                    LoanLegalID = dataForLoan.LoanLegalID,
                    Amount = NLModel.Loan.InitialLoanAmount,
                    RepaymentCount = NLModel.Loan.RepaymentCount,
                    InterestRate = NLModel.Loan.InterestRate,
                    EventTime = DateTime.UtcNow,
                    Description = "add loan ID " + this.LoanID,
                    AgreementModel = NLModel.AgreementModel
                };

                Log.Debug(history.ToString());

                int historyID = DB.ExecuteScalar<int>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", history));

                Log.Debug("NL_LoanHistorySave: LoanID: {0}, LoanHistoryID: {1}", this.LoanID, historyID);

                // 4. loan agreements
                foreach (NL_LoanAgreements agreement in NLModel.LoanAgreements)
                {
                    agreement.LoanHistoryID = historyID;
                    Log.Debug(agreement.ToString());
                }
                DB.ExecuteNonQuery(pconn, "NL_LoanAgreementsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanAgreements>("Tbl", NLModel.LoanAgreements));

                // 5. schedules 
                foreach (NLScheduleItem s in sCalcScheduleAndFee.Result.Schedule)
                {
                    s.ScheduleItem.LoanHistoryID = historyID;
                    scheduleItems.Add(s.ScheduleItem);
                }

                DB.ExecuteNonQuery(pconn, "NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", scheduleItems));

                // 5. Fund Transfer 
                NLModel.FundTransfer.LoanID = this.LoanID;
                fundTransferID = DB.ExecuteScalar<int>(pconn, "NL_FundTransfersSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.FundTransfer));

                Log.Debug("NL_FundTransfersSave: LoanID: {0}, fundTransferID: {1}", this.LoanID, fundTransferID);

                pconn.Commit();

                // ReSharper disable once CatchAllClause
            }
            catch (Exception ex)
            {

                message = string.Format("Failed to write NL_Loan for customer {0}, oldLoanID {1}, err: {2}", NLModel.CustomerID, NLModel.Loan.OldLoanID, ex);
                this.LoanID = 0;
                pconn.Rollback();

                Log.Error(message);
                SendErrorMail(message, fees, scheduleItems, history);

                // ReSharper disable once ThrowingSystemException
                throw new Exception("Add NL loan failed: {0}", ex);
            }


            // 7. Pacnet transaction
            try
            {
                if (NLModel.PacnetTransaction != null && fundTransferID > 0)
                {
                    NLModel.PacnetTransaction.FundTransferID = fundTransferID;

                    List<NL_PacnetTransactionStatuses> pacnetStatuses = NL_Loader.PacnetTransactionStatuses();
                    var pacnetTransactionStatus = pacnetStatuses.Find(s => s.TransactionStatus.Equals(NLModel.PacnetTransactionStatus)) ?? pacnetStatuses.Find(s => s.TransactionStatus.Equals("Unknown"));
                    NLModel.PacnetTransaction.PacnetTransactionStatusID = pacnetTransactionStatus.PacnetTransactionStatusID;

                    NLModel.PacnetTransaction.PacnetTransactionStatusID = DB.ExecuteScalar<int>("NL_PacnetTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", NLModel.PacnetTransaction));

                    Log.Debug("NL_PacnetTransactionsSave: LoanID: {0}, pacnetTransactionID: {1}", this.LoanID, NLModel.PacnetTransaction.PacnetTransactionStatusID);
                }
            }
            catch (Exception e1)
            {
                message = string.Format("Failed to write NL PacnetTransaction: Customer {0}, oldLoanID {1}, err: {2}", NLModel.CustomerID, NLModel.Loan.OldLoanID, e1);
                SendErrorMail(message, fees, scheduleItems, history);
                Log.Error(message);
            }

        }//Execute


        private void SendErrorMail(string sMsg, List<NL_LoanFees> setupFee = null, List<NL_LoanSchedules> scheduleItems = null, NL_LoanHistory history = null)
        {
            var message = string.Format(
                  "<h3>CustomerID: {0}; UserID: {1}</h3><p>"
                  + "<h3>Input data</h3>: {2} <br/>"
                  + "<h3>NL_Loan</h3>: {3} <br/>"
                  + "<h3>NL_LoanHistory</h3>: {4} <br/>"
                  + "<h3>NL_LoanFees</h3>: {5} <br/>"
                  + "<h3>NL_LoanSchedules</h3>: {6} <br/>"
                  + "<h3>NL_LoanAgreements</h3>: {7} <br/>"
                  + "<h3>NL_FundTransfer</h3>: {8} <br/>"
                  + "<h3>NL_PacnetTransactionr</h3>: {9} <br/></p>",

                  NLModel.CustomerID
                  , NLModel.UserID
                  , HttpUtility.HtmlEncode(NLModel.ToString())
                  , HttpUtility.HtmlEncode(NLModel.Loan == null ? "no Loan specified" : NLModel.Loan.ToString())
                  , HttpUtility.HtmlEncode(history == null ? "no LoanHistory specified" : history.ToString())
                  , HttpUtility.HtmlEncode(setupFee == null ? "no LoanFees specified" : setupFee.ToString())
                  , HttpUtility.HtmlEncode(scheduleItems == null ? "no scheduleItems specified" : scheduleItems.ToString()) // NL_LoanSchedules
                  , HttpUtility.HtmlEncode(NLModel.LoanAgreements == null ? "no LoanAgreements specified" : NLModel.LoanAgreements.ToString()) // NL_LoanAgreements
                  , HttpUtility.HtmlEncode(NLModel.FundTransfer == null ? "no FundTransfer specified" : NLModel.FundTransfer.ToString())
                  , HttpUtility.HtmlEncode(NLModel.PacnetTransaction == null ? "no PacnetTransaction specified" : NLModel.PacnetTransaction.ToString())
            );

            new Mail().Send(
                  this.emailToAddress,
                  null, // message text
                  message, //html
                  this.emailFromAddress, // fromEmail
                  this.emailFromName, // fromName
                  sMsg //"#NL_Loan failed oldLoanID: " + (int)NLModel.Loan.OldLoanID + " for customer " + NLModel.CustomerID // subject
            );

        } // SendErrorMail

        public int LoanID;
        public NL_Loader Loader { get; private set; }
        public NL_Model NLModel { get; private set; }

        private readonly string emailToAddress;
        private readonly string emailFromAddress;
        private readonly string emailFromName;

    }//class AddLoan
}//ns
