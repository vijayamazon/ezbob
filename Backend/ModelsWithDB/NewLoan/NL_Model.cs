﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.Attributes;

	[DataContract]
	public class NL_Model : AStringable {
		public NL_Model(int customerID) {

			CustomerID = customerID;

			Offer = new NL_Offers();
			Loan = new NL_Loans();
			Agreements = new List<NLAgreementItem>();
			FundTransfer = new NL_FundTransfers();

			//CalculatorImplementation = CurrentValues.Instance.DefaultLoanCalculator.Value;
		} // constructor

		[DataMember]
		public int CustomerID { get; set; }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public NL_FundTransfers FundTransfer { get; set; }

		[DataMember]
		public NL_Loans Loan { get; set; }

		[DataMember]
		[ExcludeFromToString]
		public List<NLAgreementItem> Agreements { get; set; }

		[DataMember]
		public decimal? BrokerComissions { get; set; }

		[DataMember]
		public NL_Offers Offer { get; set; }

		[DataMember]
		public string Error { get; set; }

		[DataMember]
		public decimal? APR { get; set; }

		// generated by Calculator

		private decimal _fees;
		private decimal _interest;
		private decimal _principal;
		private decimal _totalEarlyPayment;
		private decimal _nextEarlyPayment;
		private decimal _rolloverPayment;
		private decimal _nextEarlyPaymentSavedAmount;
		private decimal _totalEarlyPaymentSavedAmount;

		public decimal TotalEarlyPayment {
			get { return this._totalEarlyPayment; }
			set { this._totalEarlyPayment = value; }
		}

		public decimal NextEarlyPayment {
			get { return this._nextEarlyPayment; }
			set { this._nextEarlyPayment = value; }
		}

		public decimal RolloverPayment {
			get { return this._rolloverPayment; }
			set { this._rolloverPayment = value; }
		}

		/// <summary>
		/// for backworkd compatibility with LoanRepaymentScheduleCalculator and "old" Loan model - sum of all AmountDue(s)
		/// </summary>
		public decimal Balance {
			get { return this._interest + this._fees + this._principal; }
			set { }
		}

		/// <summary>
		/// for customer dashboards - amount to be saved for NextEarlyPayment
		/// </summary>
		public decimal NextEarlyPaymentSavedAmount {
			get { return this._nextEarlyPaymentSavedAmount; }
			set { this._nextEarlyPaymentSavedAmount = value; }
		}

		/// <summary>
		/// for customer dashboards - amount to be saved for TotalEarlyPayment
		/// </summary>
		public decimal TotalEarlyPaymentSavedAmount {
			get { return this._totalEarlyPaymentSavedAmount; }
			set { this._totalEarlyPaymentSavedAmount = value; }
		}

		/// <summary>
		/// open fees at t'
		/// </summary>
		public decimal Fees { get { return this._fees; } set { this._fees = value; } }


		/// <summary>
		/// open interest at t'
		/// </summary>
		public decimal Interest { get { return this._interest; } set { this._interest = value; } }

		/// <summary>
		/// open principal at t'
		/// </summary>
		public decimal Principal { get { return this._principal; } set { this._principal = value; } }

		// ### generated by Calculator

		/// <exception cref="InvalidCastException"><paramref /> cannot be cast to the element type of the current <see cref="T:System.Array" />.</exception>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();

			if (Offer.Amount > 0) {
				sb.Append(Environment.NewLine).Append("Offer:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_Offers))).Append(Offer);
			} else
				sb.Append(Environment.NewLine).Append("No offer data");

			if (Loan.LoanID > 0) {
				sb.Append(Environment.NewLine).Append("Loan:").Append(Loan);
			} else
				sb.Append(Environment.NewLine).Append("No Loan data");

			if (FundTransfer.FundTransferStatusID > 0) {
				sb.Append(Environment.NewLine).Append("FundTransfer:").Append(Environment.NewLine).Append(PrintHeadersLine(typeof(NL_FundTransfers))).Append(FundTransfer.ToStringAsTable());
			} else
				sb.Append(Environment.NewLine).Append("No FundTransfer data");

			return sb.ToString();
		}


		// use default from configuration
		//[DataMember]
		//public string CalculatorImplementation { get; private set; } // AloanCalculator LegacyLoanCalculator/BankLikeLoanCalculator


		/*/// <exception cref="Exception">Condition. </exception>
     /*   public ALoanCalculator CalculatorInstance(){
            // set default
            ALoanCalculator calc = new LegacyLoanCalculator(this);
            try            {
                Type t = Type.GetType(CalculatorImplementation);
                if (t != null) {
                    if (t == typeof(BankLikeLoanCalculator))
                        calc = new BankLikeLoanCalculator(this);
                    //default type from configurations
                    //ALoanCalculator calc = (ALoanCalculator)Activator.CreateInstance(t, this);
                    //if (t.GetType() == typeof(BankLikeLoanCalculator))
                    //     calc = (BankLikeLoanCalculator)Activator.CreateInstance(t, this); // new BankLikeLoanCalculator(this);
                    //else if (t.GetType() == typeof(LegacyLoanCalculator))
                    //     calc = (LegacyLoanCalculator)Activator.CreateInstance(t, this); // calc = new LegacyLoanCalculator(this);
                    Console.WriteLine(calc);
                    return calc;
                }
                // ReSharper disable once CatchAllClause
            }
            catch (Exception e) {
                Console.WriteLine(e);
                // ReSharper disable once ThrowingSystemException
                throw new Exception(string.Format("Failed to create calculator instance for {0}", CalculatorImplementation), e);
            }
            return null;
		}*/




	} // class NL_Model
} // namespace