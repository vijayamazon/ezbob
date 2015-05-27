﻿namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models.NewLoan;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class NL_Loader {

		public static AConnection DB { get; private set; }
		public static ASafeLog Log { get; private set; }

		public NL_Loader(NL_Model model) {
			this.model = model;

			DB = Library.Instance.DB;
			Log = Library.Instance.Log;

			Console.WriteLine("DB=============" + DB.GetType());

		} // constructor


		public static void CheckDB() {
			Console.WriteLine("in static DB=============" + DB.GetType());
		}

		// lookup data
		public static List<NL_PacnetTransactionStatuses> PacnetTransactionStatuses() {
			return DB.Fill<NL_PacnetTransactionStatuses>("NL_PacnetTransactionStatusesLoad", CommandSpecies.StoredProcedure);
		}// PacnetTransactionStatuses

		public static IEnumerable<NL_LoanStatuses> LoanStatuses() {
			return DB.Fill<NL_LoanStatuses>("NL_LoanStatusesLoad", CommandSpecies.StoredProcedure);
		} // LoanStatuses

		public static IEnumerable<NL_LoanFeeTypes> LoanFeeTypes() {
			return DB.Fill<NL_LoanFeeTypes>("NL_LoanFeeTypesLoad", CommandSpecies.StoredProcedure);
		} // LoanStatuses

		public static IEnumerable<NL_RepaymentIntervalTypes> RepaymentIntervalTypes() {
			return DB.Fill<NL_RepaymentIntervalTypes>("NL_RepaymentIntervalTypesLoad", CommandSpecies.StoredProcedure);
		} // RepaymentIntervalTypes


		// delete this strategy later, use the method instead 
		//public OfferForLoan OfferForLoan() { 
			
		//	//OfferForLoan data = new OfferForLoan();
		//	if (this.model.CustomerID == 0) {
		//		Log.Debug("OfferForLoan: customerID {0} param is wrong", this.model.CustomerID);
		//	}
		//	Console.WriteLine("==" + this.model.CustomerID);
		//	try {
		//		return DB.FillFirst<OfferForLoan>("NL_OfferForLoan", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", this.model.CustomerID));
		//	} catch (System.NullReferenceException nref) {
		//		Console.WriteLine(nref);
		//	} catch (System.Exception e) {
		//		Log.Debug("NL_OfferForLoan: valid offer for customerID {0} not found", this.model.CustomerID);
		//	}

		//	return null;
		//} // OfferForLoan


		// all those are referenced to the latest history ID

		public List<NL_Model> GetAllSchedules(int loanID) {
			// call SP [dbo].[NL_AllSchedulesLoad]
			return null;
		}

		/// <summary>
		/// loan schedules that paid and not cancelled: marked as "wrong" or "charged back"
		/// </summary>
		/// <param name="loanID"></param>
		/// <returns></returns>
		public List<NL_Model> GetPaidSchedules(int loanID) {
			// call SP [dbo].[NL_PaidSchedulesLoad]
			return null;
		}

		public List<NL_Model> GetNotPaidSchedules(int loanID) {
			// call SP [dbo].[NL_NotPaidSchedulesLoad]
			return null;
		}

		public List<NL_Model> GetCancelledPaymentSchedules(int loanID) {
			// call SP [dbo].[NL_CancelledSchedulesLoad]
			return null;
		}//GetCancelledPaymentSchedules


		

		private NL_Model model;

	} // class NL_Loader
}