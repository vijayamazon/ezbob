namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;
	using KellermanSoftware.CompareNetObjects;
	using NHibernate.Linq;

	/// <summary>
	/// triggers for this strategy: payment cancellation (done); payment adding (done); rollover; re-scheduling;
	/// </summary>
	public class UpdateLoanDBState : AStrategy {

		public UpdateLoanDBState(int customerID, long loanID, int? userID = null) {

			this.strategyArgs = new object[] { customerID, loanID, userID };

			if (customerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, this.Error, null);
				return;
			}

			if (loanID == 0) {
				this.Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, this.Error, this.Error, null);
				return;
			}

			Model = new NL_Model(customerID);
			Model.Loan.LoanID = loanID;
			Context.UserID = userID ?? Context.UserID;
			Model.UserID = Context.UserID;

			this.strategyArgs = new object[] { customerID, loanID, Context.UserID };
		} // ctor

		public override string Name { get { return "UpdateLoanDBState"; } }

		public string Error;

		public NL_Model Model { get; private set; }

		private readonly object[] strategyArgs;

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		/// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
		public override void Execute() {

			if (!string.IsNullOrEmpty(this.Error)) {
				throw new NL_ExceptionInputDataInvalid(this.Error);
			}

			// get raw DB state of the loan - without calc
			GetLoanState getLoanState = new GetLoanState(Model.CustomerID, Model.Loan.LoanID, DateTime.UtcNow, Context.UserID, false);
			getLoanState.Execute();

			// failed to load loan from DB
			if (!string.IsNullOrEmpty(getLoanState.Error)) {
				this.Error = getLoanState.Error;
				NL_AddLog(LogType.Error, "Loan get state failed", this.strategyArgs, getLoanState.Error, this.Error, null);
				return;
			}

			//Model = getLoanState.Result;

		//	Log.Debug("NOTRECALUCLATED===================={0}", Model);

			Model = getLoanState.Result;

			// get loan state updated by calculator
			try {
				ALoanCalculator calc = new LegacyLoanCalculator(Model);
				calc.GetState();
			} catch (NoInitialDataException noInitialDataException) {
				this.Error = noInitialDataException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Model, this.Error, null);
			} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
				this.Error = invalidInitialInterestRateException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Model, this.Error, null);
			} catch (NoLoanHistoryException noLoanHistoryException) {
				this.Error = noLoanHistoryException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Model, this.Error, null);
			} catch (InvalidInitialAmountException invalidInitialAmountException) {
				this.Error = invalidInitialAmountException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Model, this.Error, null);
			} catch (OverflowException overflowException) {
				this.Error = overflowException.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Model, this.Error, null);

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Error = ex.Message;
				NL_AddLog(LogType.Error, "Calculator exception", this.strategyArgs, Model, this.Error, null);
			}


			//Log.Debug("RECALUCLATED===================={0}", updatedModel);

			/*List<Type> classTypesToInclude = new List<Type>();
			classTypesToInclude.Add(typeof(NL_Loans));
			classTypesToInclude.Add(typeof(NL_LoanHistory));
			classTypesToInclude.Add(typeof(NL_Payments));
			classTypesToInclude.Add(typeof(NL_LoanSchedules));
			classTypesToInclude.Add(typeof(NL_LoanSchedulePayments));
			classTypesToInclude.Add(typeof(NL_LoanFeePayments));

			List<string> membersToInclude = new List<string>();
			membersToInclude.Add("Loan");
			membersToInclude.Add("Payments");
			membersToInclude.Add("SchedulePayments");
			membersToInclude.Add("FeePayments");

			ComparisonConfig compareConf = new ComparisonConfig() {
				//ClassTypesToInclude = classTypesToInclude, 
				//IgnoreCollectionOrder = true, 
				//MaxStructDepth = 6,
				MaxDifferences = 5000,
				ShowBreadcrumb = true,
				MembersToInclude = membersToInclude
			}; 

			//This is the comparison class
			CompareLogic compareLogic = new CompareLogic(compareConf);
	
			ComparisonResult result = compareLogic.Compare(Model, updatedModel);
			//if (!result.AreEqual) {
			Log.Debug("updatedModel==============================={0}", result.DifferencesString);
			Log.Debug(result.ExceededDifferences);
			result.Differences.ForEach(d => Log.Debug(d));
			Log.Debug(result.Config);
			//}*/
			
			/*int propertyCount = typeof(NL_Loans).GetProperties().Length;
			propertyCount += typeof(NL_Payments).GetProperties().Length;
			propertyCount += typeof(NL_LoanSchedules).GetProperties().Length;
			propertyCount += typeof(NL_LoanHistory).GetProperties().Length;
			propertyCount += typeof(NL_LoanSchedulePayments).GetProperties().Length;
			propertyCount += typeof(NL_LoanFeePayments).GetProperties().Length;

			CompareLogic advanedComparison = new CompareLogic() {
				Config = new ComparisonConfig() {
					MaxDifferences = propertyCount
				}
			};
			List<Difference> diffs = advanedComparison.Compare(Model.Loan, updatedModel.Loan).Differences;
			foreach (Difference diff in diffs) {
				Log.Debug("Property name:" + diff.PropertyName);
				Log.Debug("1 value:" + diff.Object1Value);
				Log.Debug("2 value:" + diff.Object2Value + "\n");
			}
			
			return;*/

			int loanstatusBefore = Model.Loan.LoanStatusID;

			NL_AddLog(LogType.Info, "Loan state", this.strategyArgs, Model, this.Error, null);

			List<NL_LoanSchedules> schedules = new List<NL_LoanSchedules>();
			List<NL_LoanSchedulePayments> schedulePayments = new List<NL_LoanSchedulePayments>();
			List<NL_LoanFeePayments> feePayments = new List<NL_LoanFeePayments>();
			List<BigintList> resetPayments = new List<BigintList>();

			ConnectionWrapper pconn = DB.GetPersistent();

			try {
				pconn.BeginTransaction();

				// save new history - on rescheduling/rollover
				foreach (NL_LoanHistory h in Model.Loan.Histories.Where(h => h.LoanHistoryID == 0)) {
					h.LoanHistoryID = DB.ExecuteScalar<long>(pconn, "NL_LoanHistorySave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("Tbl", h));
					h.Schedule.ForEach(s => s.LoanHistoryID = h.LoanHistoryID);
				}

				Model.Loan.Histories.ForEach(h => h.Schedule.ForEach(s => schedules.Add(s)));

				// add new schedules - on rescheduling/rollover
				DB.ExecuteNonQuery("NL_LoanSchedulesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedules>("Tbl", schedules.Where(s => s.LoanScheduleID == 0)));

				// update existing schedules - closed time and statuses
				foreach (NL_LoanSchedules s in schedules.Where(s => s.LoanScheduleID > 0)) {
					DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
							new QueryParameter("LoanScheduleID", s.LoanScheduleID),
							new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
							new QueryParameter("ClosedTime", s.ClosedTime)
							);
				}

				// assign payment to loan
				foreach (NL_Payments p in Model.Loan.Payments) {
					p.SchedulePayments
						.Where(sp => sp.LoanSchedulePaymentID == 0)
						.ForEach(sp => schedulePayments.Add(sp));
					p.FeePayments
						.Where(fp => fp.LoanFeePaymentID == 0)
						.ForEach(fp => feePayments.Add(fp));

					p.FeePayments.Where(fp => fp.LoanFeePaymentID > 0 && fp.Amount == 0).ForEach(fp => resetPayments.Add(new BigintList() { Item = fp.PaymentID }));
					p.SchedulePayments.Where(sp => sp.LoanSchedulePaymentID > 0 && sp.InterestPaid == 0 && sp.PrincipalPaid == 0).ForEach(fp => resetPayments.Add(new BigintList() { Item = fp.PaymentID }));
				}

				// reset after reordered/cancelled payments - their amounts
				if (resetPayments.Count > 0) {
					DB.ExecuteNonQuery("NL_PaidAmountsReset", CommandSpecies.StoredProcedure, DB.CreateTableParameter<BigintList>("PaymentIds", resetPayments));
				}

				// save new schedule payment
				if (schedulePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanSchedulePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanSchedulePayments>("Tbl", schedulePayments));
				}

				// save new fee payments
				if (feePayments.Count > 0) {
					DB.ExecuteNonQuery("NL_LoanFeePaymentsSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFeePayments>("Tbl", feePayments));
				}

				// update loan status
				if (loanstatusBefore != Model.Loan.LoanStatusID) {
					DB.ExecuteNonQuery("NL_LoanUpdate", CommandSpecies.StoredProcedure,
						new QueryParameter("LoanID", Model.Loan.LoanID),
						new QueryParameter("LoanStatusID", Model.Loan.LoanStatusID),
						new QueryParameter("DateClosed", Model.Loan.DateClosed)
						);
				}

				pconn.Commit();

				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {

				pconn.Rollback();

				this.Error = ex.Message;
				Log.Error("Failed to update loan DB dbState. err: {0}", this.Error);

				NL_AddLog(LogType.Error, "Faild - Rollback", this.strategyArgs, this.Error, ex.ToString(), ex.StackTrace);
			}
		}
	}

	public class BigintList {
		public long Item { get; set; }
	}


}