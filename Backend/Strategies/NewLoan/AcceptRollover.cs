namespace Ezbob.Backend.Strategies.NewLoan {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.NewLoan.Exceptions;
	using Ezbob.Database;

	public class AcceptRollover : AStrategy {


		public AcceptRollover(int customerID, long loanID, NL_LoanFees rolloverFee = null) {

			/*if (Context.CustomerID == null || Context.CustomerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			CustomerID = customerID;
			LoanID = loanID;

			this.strategyArgs = new object[] { CustomerID, Context.UserID };*/

			this.strategyArgs = new object[] { customerID, loanID, rolloverFee };

			if (customerID == 0) {
				this.Error = NL_ExceptionCustomerNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			if (loanID == 0) {
				this.Error = NL_ExceptionLoanNotFound.DefaultMessage;
				NL_AddLog(LogType.Error, "Strategy Faild", this.strategyArgs, null, this.Error, null);
				return;
			}

			CustomerID = customerID;
			LoanID = loanID;
			fee = rolloverFee;

			this.strategyArgs = new object[] { customerID, loanID, rolloverFee };

		}

		public override string Name { get { return "AcceptRollover"; } }

		public int CustomerID { get; private set; }
		public long LoanID { get; private set; }

		public string Error;
		
		private readonly object[] strategyArgs;

		private ALoanCalculator calc;
		public NL_LoanFees fee { get; private set; }

		/// <exception cref="NL_ExceptionInputDataInvalid">Condition. </exception>
		public override void Execute() {
            if (!Convert.ToBoolean(CurrentValues.Instance.NewLoanRun.Value))
                return;
			if (!string.IsNullOrEmpty(this.Error)) {
				throw new NL_ExceptionInputDataInvalid(this.Error);
			}

			// TODO EZ-4330
			/* RolloverPayment = open interest till accept day included + rollover fees assigned
			 * 
			1. records NL_Payments (+NL_PaypointTransactions) for rollover () - via AddPayment strategy from outside? 
			2. record rollover fee for into NL_LoanFees
			3. update existing rollover opportunity record: [CustomerActionTime], [IsAccepted] in [dbo].[NL_LoanRollovers] table
			4. 
			5. make rollover using calculator - add to NL model new history, rearrange schedules, statuses
			 *  - run calc.GetState to get outstanding balance
			 *  - create new history
			 *  - mark non relevant schedules as CancelledOnRollover
			 *  - create schedule for new history
			 *  
			 *  
			6. update DB: records new history, new schedule items; update previous schedule with appropriate statuses
			*/

			NL_AddLog(LogType.Info, "Strategy Start", this.strategyArgs, null, this.Error, null);

			if (fee != null) {

				// insert fees
				List<NL_LoanFees> fList = new List<NL_LoanFees>();
				fList.Add(fee);

				//DB.ExecuteNonQuery(pconn, "NL_LoanFeesSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter<NL_LoanFees>("Tbl", fList));
			}

			GetLoanState stateStrategy = new GetLoanState(CustomerID, LoanID, DateTime.UtcNow, CustomerID);
			stateStrategy.Execute();
	
			try {
				this.calc = new LegacyLoanCalculator(stateStrategy.Result);
				this.calc.RolloverRescheduling();
			} catch (NoLoanHistoryException noHistory) {
				this.Error = noHistory.Message;
			} catch (NoInitialDataException noData) {
				this.Error = noData.Message;
			} catch (InvalidInitialAmountException initialAmountException) {
				this.Error = initialAmountException.Message;
			} catch (InvalidInitialInterestRateException interestRateException) {
				this.Error = interestRateException.Message;
			} catch (InvalidInitialRepaymentCountException repaymentCountException) {
				this.Error = repaymentCountException.Message;
				// ReSharper disable once CatchAllClause
			} catch (Exception ex) {
				this.Error = ex.Message;
				return;
			}

			/*
						int deletedItems = 0;
						// mark non relevant schedules as CancelledOnRollover and save
						foreach (NL_LoanSchedules s in model.Loan.LastHistory().Schedule.Where(s => this.nowTime >= s.PlannedDate) ) {
							s.LoanScheduleStatusID = (int)NLScheduleStatuses.DeletedOnReschedule;
							s.ClosedTime = this.nowTime;
							// close future 
							DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure,
										new QueryParameter("LoanScheduleID", s.LoanScheduleID),
										new QueryParameter("LoanScheduleStatusID", s.LoanScheduleStatusID),
										new QueryParameter("ClosedTime", s.ClosedTime)
										);
							deletedItems++;
						}

			
						// get balance
						try {
							this.calc = new LegacyLoanCalculator(model);
							this.calc.GetState();
						} catch (NoLoanHistoryException noHistory) {
							this.Error = noHistory.Message;
						} catch (NoInitialDataException noData) {
							this.Error = noData.Message;
						} catch (InvalidInitialAmountException initialAmountException) {
							this.Error = initialAmountException.Message;
						} catch (InvalidInitialInterestRateException interestRateException) {
							this.Error = interestRateException.Message;
						} catch (InvalidInitialRepaymentCountException repaymentCountException) {
							this.Error = repaymentCountException.Message;
							// ReSharper disable once CatchAllClause
						} catch (Exception ex) {
							this.Error = ex.Message;
							return;
						}

						NL_LoanHistory firstHistory = model.Loan.FirstHistory();

						//	create new history
						model.Loan.Histories.Add(
							new NL_LoanHistory() {
								Amount = this.calc.Balance,
								InterestRate = firstHistory.InterestRate,
								Description = "rollover",
								EventTime = this.nowTime, // TODO check +month?
								LoanID = LoanID,
								LoanLegalID = firstHistory.LoanLegalID,
								AgreementModel = firstHistory.AgreementModel,
								Agreements = firstHistory.Agreements,
								RepaymentCount = (firstHistory.RepaymentCount - deletedItems),
								RepaymentIntervalTypeID = firstHistory.RepaymentIntervalTypeID,
								UserID = 1
							});

						// create schedule for new history
						try {
							this.calc.CreateSchedule();
						} catch (NoInitialDataException noInitialDataException) {
							this.Error = "CreateSchedule: " + noInitialDataException.Message;
						} catch (InvalidInitialRepaymentCountException invalidInitialRepaymentCountException) {
							this.Error = "CreateSchedule: " + invalidInitialRepaymentCountException.Message;
						} catch (InvalidInitialInterestRateException invalidInitialInterestRateException) {
							this.Error = "CreateSchedule: " + invalidInitialInterestRateException.Message;
						} catch (InvalidInitialAmountException invalidInitialAmountException) {
							this.Error = "CreateSchedule: " + invalidInitialAmountException.Message;
						} catch (NoScheduleException noScheduleException) {
							this.Error = "CreateSchedule: " + noScheduleException.Message;
						} catch (Exception ex) {
							this.Error = ex.Message;
							return;
						}*/


			// save new recalculated loan state to DB
			UpdateLoanDBState reloadLoanDBState = new UpdateLoanDBState(CustomerID, LoanID, CustomerID);
			reloadLoanDBState.Execute();
		}

	} // class AcceptRollover
} // ns