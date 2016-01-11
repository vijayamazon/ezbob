namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;

	internal class RolloverReschedulingMethod : AMethod {

		public RolloverReschedulingMethod(ALoanCalculator calculator)
			: base(calculator, false) {
		} // constructor

		/// <exception cref="OverflowException">Condition. </exception>
		public virtual void Execute() {
			// not accepted rollover
			if (Calculator.acceptedRollover.Rollover == null) {
				Log.Alert("RolloverRescheduling: no accepted rollover");
				return;
			}

			// not accepted rollover
			if (!Calculator.acceptedRollover.Rollover.IsAccepted || !Calculator.acceptedRollover.Rollover.CustomerActionTime.HasValue) {
				Log.Alert("RolloverRescheduling: rollover not accepted. {0}", Calculator.acceptedRollover.Rollover);
				return;
			}

			// rollover proceeseed
			if (Calculator.acceptedRollover.Rollover.CustomerActionTime.Value.Date == Calculator.currentHistory.EventTime.Date || Calculator.acceptedRolloverProcessed) {
				Log.Debug("RolloverRescheduling: History ({0}) for rollover {1:d}, rolloverID {2} already exists", Calculator.currentHistory.LoanHistoryID, Calculator.acceptedRollover.Rollover.CustomerActionTime.Value, Calculator.acceptedRollover.Rollover.LoanRolloverID);
				return;
			}

			var rolloverPayment = Calculator.events.FirstOrDefault(p => p.Payment != null && p.Payment.PaymentTime.Date.Equals(Calculator.acceptedRollover.Rollover.CustomerActionTime.Value.Date) && p.Payment.PaymentDestination.Equals(NLPaymentDestinations.Rollover.ToString()));

			// rollover not paid
			if (rolloverPayment == null) {
				Log.Alert("RolloverRescheduling: rollover payment not found. {0}", Calculator.acceptedRollover.Rollover);
				return;
			}

			DateTime acceptionTime = Calculator.acceptedRollover.Rollover.CustomerActionTime.Value.Date;

			NL_LoanHistory lastHistory = WorkingModel.Loan.LastHistory();

			NL_LoanHistory newHistory = new NL_LoanHistory {
				LoanID = lastHistory.LoanID,
				LoanLegalID = lastHistory.LoanLegalID,
				AgreementModel = lastHistory.AgreementModel,
				Agreements = lastHistory.Agreements,
				InterestRate = lastHistory.InterestRate,
				RepaymentIntervalTypeID = lastHistory.RepaymentIntervalTypeID,
				UserID = WorkingModel.CustomerID,
				Description = "accept rollover",
				Amount = Calculator.currentOpenPrincipal,
				EventTime = acceptionTime
			};

			RepaymentIntervalTypes intervalType = (RepaymentIntervalTypes)lastHistory.RepaymentIntervalTypeID;

			int removedItems = 0;
			newHistory.RepaymentDate = DateTime.MinValue;

			// mark removed schedules + add new schedules
			foreach (NL_LoanSchedules s in Calculator.schedule.Where(s => s.PlannedDate >= acceptionTime)) {

				s.LoanScheduleStatusID = ((s.Principal - s.PrincipalPaid + s.Interest - s.InterestPaid + s.FeesAssigned - s.FeesPaid) == 0) ? (int)NLScheduleStatuses.DeletedOnReschedule : (int)NLScheduleStatuses.ClosedOnReschedule;
				s.ClosedTime = acceptionTime;

				removedItems++;

				DateTime plannedDate = Calculator.AddRepaymentIntervals(1, s.PlannedDate, intervalType);

				if (newHistory.RepaymentDate.Equals(DateTime.MinValue)) {
					newHistory.RepaymentDate = plannedDate;
				}

				// add new schedule instead of removed
				NL_LoanSchedules newSchedule = new NL_LoanSchedules() {
					LoanScheduleID = 0,
					LoanScheduleStatusID = (int)NLScheduleStatuses.StillToPay,
					Position = s.Position,
					PlannedDate = plannedDate,
					Principal = (s.Principal - s.PrincipalPaid),
					InterestRate = s.InterestRate,
					TwoDaysDueMailSent = s.TwoDaysDueMailSent,
					FiveDaysDueMailSent = s.FiveDaysDueMailSent
				};

				Log.Debug("schedule {0} replaced by {1}", s, newSchedule);

				newHistory.Schedule.Add(newSchedule);
			}

			newHistory.RepaymentCount = removedItems;

			WorkingModel.Loan.Histories.Add(newHistory);

			List<NL_LoanFees> replacedDistributedFees = new List<NL_LoanFees>();

			// mark removed distributed fees + add new distributed fees
			foreach (NL_LoanFees fee in Calculator.distributedFeesList.Where(f => f.AssignTime.Date >= acceptionTime)) {
				fee.DisabledTime = acceptionTime;
				fee.Notes = "disabled on rollover";
				fee.DeletedByUserID = 1;

				NL_LoanFees newFee = new NL_LoanFees() {
					LoanFeeID = 0,
					Amount = fee.Amount,
					AssignTime = Calculator.AddRepaymentIntervals(1, fee.AssignTime, intervalType),
					LoanID = WorkingModel.Loan.LoanID,
					LoanFeeTypeID = fee.LoanFeeTypeID,
					AssignedByUserID = fee.AssignedByUserID,
					CreatedTime = acceptionTime,
					Notes = fee.Notes
				};

				Log.Debug("fee {0} replaced by {1}", fee, newFee);

				replacedDistributedFees.Add(newFee);
			}

			Calculator.distributedFeesList.AddRange(replacedDistributedFees);
			WorkingModel.Loan.Fees.AddRange(replacedDistributedFees);

			// reset paid amount for deleted/closed schedules and disabled distributed fees 
			foreach (NL_Payments p in WorkingModel.Loan.Payments) {

				foreach (NL_LoanSchedulePayments sp in p.SchedulePayments) {
					foreach (NL_LoanSchedules s in Calculator.schedule.Where(s => s.LoanScheduleStatusID == (int)NLScheduleStatuses.DeletedOnReschedule || s.LoanScheduleStatusID == (int)NLScheduleStatuses.ClosedOnReschedule)) {
						if (s.LoanScheduleID == sp.LoanScheduleID) {
							sp.PrincipalPaid = 0;
							sp.InterestPaid = 0;
						}
					}
				}

				foreach (NL_LoanFeePayments fp in p.FeePayments) {
					foreach (NL_LoanFees f in Calculator.distributedFeesList.Where(f => f.DisabledTime.Equals(acceptionTime))) {
						if (f.LoanFeeID == fp.LoanFeeID) {
							fp.Amount = 0;
							fp.ResetAmount = 0;
						}
					}
				}
			}

			Calculator.acceptedRolloverProcessed = true;
		}
	} 
} // namespace
