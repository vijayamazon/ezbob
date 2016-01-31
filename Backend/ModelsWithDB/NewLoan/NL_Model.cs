﻿namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using ConfigManager;
	using DbConstants;
	using EZBob.DatabaseLib.Model;

	[DataContract]
	public class NL_Model : AStringable {
		public NL_Model(int customerID) {
			CustomerID = customerID;
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
		private decimal _balance;
		private decimal _totalEarlyPayment;
		private decimal _nextEarlyPayment;
		private decimal _rolloverPayment;
		private decimal _nextEarlyPaymentSavedAmount;
		private decimal _totalEarlyPaymentSavedAmount;


		[DataMember]
		public decimal TotalEarlyPayment {
			get { return this._totalEarlyPayment; }
			set { this._totalEarlyPayment = value; }
		}

		[DataMember]
		public decimal NextEarlyPayment {
			get { return this._nextEarlyPayment; }
			set { this._nextEarlyPayment = value; }
		}

		[DataMember]
		public decimal RolloverPayment {
			get { return this._rolloverPayment; }
			set { this._rolloverPayment = value; }
		}

		/// <summary>
		/// for backworkd compatibility with LoanRepaymentScheduleCalculator and "old" Loan model - sum of all AmountDue(s)
		/// </summary>
		[DataMember]
		public decimal Balance {
			get { return this._balance; }
			set { this._balance = value; }
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
		[DataMember]
		public decimal TotalEarlyPaymentSavedAmount {
			get { return this._totalEarlyPaymentSavedAmount; }
			set { this._totalEarlyPaymentSavedAmount = value; }
		}

		/// <summary>
		/// open fees at t'
		/// </summary>
		[DataMember]
		public decimal Fees {
			get { return this._fees; }
			set { this._fees = value; }
		}

		/// <summary>
		/// open interest at t'
		/// </summary>
		[DataMember]
		public decimal Interest {
			get { return this._interest; }
			set { this._interest = value; }
		}

		/// <summary>
		/// open principal at t'
		/// </summary>
		[DataMember]
		public decimal Principal {
			get { return this._principal; }
			set { this._principal = value; }
		}

		// ### generated by Calculator

		public static void CalculateFee(int daysBetween, decimal interest, out int feeAmount, out NLFeeTypes feeType) {
			feeAmount = 0;
			feeType = NLFeeTypes.None;
			if (daysBetween >= CurrentValues.Instance.CollectionPeriod1 && daysBetween < CurrentValues.Instance.CollectionPeriod2) {
				feeAmount = CurrentValues.Instance.LatePaymentCharge;
				feeType = NLFeeTypes.LatePaymentFee;
			} else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest > 0) {
				feeAmount = CurrentValues.Instance.AdministrationCharge;
				feeType = NLFeeTypes.AdminFee;
			} else if (daysBetween >= CurrentValues.Instance.CollectionPeriod2 && daysBetween < CurrentValues.Instance.CollectionPeriod3 && interest <= 0) {
				feeAmount = CurrentValues.Instance.PartialPaymentCharge;
				feeType = NLFeeTypes.PartialPaymentFee;
			} //if
		} //CalculateFee

		// could be updated by system users from UW GUI
		public static NLFeeTypes[] NLFeeTypesEditable = { NLFeeTypes.OtherCharge };


		public static VariableValue NLFeeTypeToConfVar(NLFeeTypes fType) {

			VariableValue result = null;

			switch (fType) {

			case NLFeeTypes.AdminFee:
				result = CurrentValues.Instance.AdministrationCharge;
				break;

			case NLFeeTypes.ServicingFee:
			case NLFeeTypes.ArrangementFee:
				result = CurrentValues.Instance.SpreadSetupFeeCharge;
				break;

			case NLFeeTypes.LatePaymentFee:
				result = CurrentValues.Instance.LatePaymentCharge;
				break;

			case NLFeeTypes.PartialPaymentFee:
				result = CurrentValues.Instance.PartialPaymentCharge;
				break;

			case NLFeeTypes.SetupFee:
				//result = CurrentValues.Instance.set
				break;

			case NLFeeTypes.RolloverFee:
				result = CurrentValues.Instance.RolloverCharge;
				break;

			case NLFeeTypes.OtherCharge:
				result = CurrentValues.Instance.OtherCharge;
				break;
			}

			return result;
		}

		public static NLFeeTypes ConfVarToNLFeeType(ConfigurationVariable confVar) {

			if( confVar.Name == CurrentValues.Instance.AdministrationCharge)
				return NLFeeTypes.AdminFee;
			
			if (confVar.Name == CurrentValues.Instance.SpreadSetupFeeCharge)
				return NLFeeTypes.ServicingFee;
			
			if (confVar.Name == CurrentValues.Instance.LatePaymentCharge)
				return NLFeeTypes.LatePaymentFee;

			if (confVar.Name == CurrentValues.Instance.PartialPaymentCharge)
				return NLFeeTypes.PartialPaymentFee;

			if (confVar.Name == CurrentValues.Instance.RolloverCharge)
				return NLFeeTypes.RolloverFee;

			if (confVar.Name == CurrentValues.Instance.OtherCharge)
				return NLFeeTypes.OtherCharge;

			return NLFeeTypes.OtherCharge;
		}

	} // class NL_Model
} // namespace