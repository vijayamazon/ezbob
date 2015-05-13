namespace Ezbob.Backend.Strategies.NewLoan {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.NewLoan;
    using Ezbob.Database;

    public class AddPayPointPayment : AStrategy {
        public AddPayPointPayment(NL_PaypointTransactions paypointTransaction, IEnumerable<NL_LoanSchedulePayments> schedulePayments, IEnumerable<NL_LoanFeePayments> feePayments) {
            this.paypointTransaction = paypointTransaction;
            this.schedulePayments = schedulePayments;
            this.feePayments = feePayments;
        }//constructor

        public override string Name { get { return "AddPayPointPayment"; } }

        public override void Execute() {
            int? paymentID = null;
            if (this.paypointTransaction.PaypointTransactionStatusID.HasValue || this.paypointTransaction.PaypointTransactionStatusID.Value != 0 /*todo check if not error*/) {
                paymentID = DB.ExecuteScalar<int>("NL_PaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_Payments>("Tbl",
                    new NL_Payments {
                        CreationTime = this.paypointTransaction.TransactionTime,
                        IsActive = true,
                        PaymentTime = this.paypointTransaction.TransactionTime,
                        CreatedByUserID = 1, //TODO
                        PaymentMethodID = 1, //TODO !!!!
                        PaymentStatusID = 1 //TODO !!!!
                    }));
            }//if

            this.paypointTransaction.PaymentID = paymentID;
            DB.ExecuteNonQuery("NL_PaypointTransactionsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_PaypointTransactions>("Tbl", this.paypointTransaction));

            if (paymentID.HasValue) {
                foreach (var feePayment in this.feePayments) {
                    feePayment.PaymentID = paymentID.Value;
                    DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayment));
                }//foreach

                foreach (var schedulePayment in this.schedulePayments) {
                    schedulePayment.PaymentID = paymentID.Value;
                    DB.ExecuteNonQuery("NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayment));
                }//foreach
            }//if

        }//Execute

        private readonly NL_PaypointTransactions paypointTransaction;
        private readonly IEnumerable<NL_LoanFeePayments> feePayments;
        private readonly IEnumerable<NL_LoanSchedulePayments> schedulePayments;
    }//class AddPayPointPayment
}//ns
