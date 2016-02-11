namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using PaymentServices.Calculators;

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

			// 1. create new history
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

			// 2. mark removed schedules + add new schedules
			foreach (NL_LoanSchedules s in Calculator.schedule.Where(s => s.PlannedDate >= acceptionTime)) {

				s.SetStatusOnRescheduling();
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
					Principal = s.Principal, // (s.Principal - s.PrincipalPaid),
					InterestRate = s.InterestRate,
					TwoDaysDueMailSent = false, //s.TwoDaysDueMailSent,
					FiveDaysDueMailSent = false, //s.FiveDaysDueMailSent
				};

				Log.Debug("schedule {0} replaced by {1}", s, newSchedule);

				newHistory.Schedule.Add(newSchedule);
			}

			newHistory.RepaymentCount = removedItems;

			WorkingModel.Loan.Histories.Add(newHistory);

			//List<NL_LoanFees> replacedDistributedFees = new List<NL_LoanFees>();

			bool distributedFees = false;

			// 3. mark removed distributed fees + add new distributed fees
			foreach (NL_LoanFees fee in Calculator.distributedFeesList.Where(f => f.AssignTime.Date >= acceptionTime)) {
				fee.DisabledTime = acceptionTime;
				fee.Notes = "disabled on rollover";
				fee.DeletedByUserID = WorkingModel.UserID ?? 1;

				distributedFees = true;

				//NL_LoanFees newFee = new NL_LoanFees() {
				//	LoanFeeID = 0,
				//	Amount = fee.Amount,
				//	AssignTime = Calculator.AddRepaymentIntervals(1, fee.AssignTime, intervalType),
				//	LoanID = WorkingModel.Loan.LoanID,
				//	LoanFeeTypeID = fee.LoanFeeTypeID,
				//	AssignedByUserID = fee.AssignedByUserID,
				//	CreatedTime = acceptionTime,
				//	Notes = fee.Notes
				//};

				//Log.Debug("fee {0} replaced by {1}", fee, newFee);

				//replacedDistributedFees.Add(newFee);
			}

			//Calculator.distributedFeesList.AddRange(replacedDistributedFees);
			//WorkingModel.Loan.Fees.AddRange(replacedDistributedFees);

			if (distributedFees) {

				// offer-fees
				NL_OfferFees offerFees = WorkingModel.Offer.OfferFees.FirstOrDefault();

				if (offerFees != null && offerFees.DistributedPartPercent != null && (decimal)offerFees.DistributedPartPercent == 1) {

					var feeCalculator = new SetupFeeCalculator(offerFees.Percent, null);
					decimal servicingFeeAmount = feeCalculator.Calculate(newHistory.Amount);
					decimal servicingFeePaidAmount = WorkingModel.Loan.Fees.Where(f => f.LoanFeeTypeID == (int)NLFeeTypes.ServicingFee).Sum(f => f.PaidAmount);

					Log.Debug("servicingFeeAmount: {0}, servicingFeePaidAmount: {1}", servicingFeeAmount, servicingFeePaidAmount); // new "spreaded" amount

					Calculator.AttachDistributedFeesToLoanBySchedule(WorkingModel, (servicingFeeAmount - servicingFeePaidAmount), acceptionTime);
				}
			}

			// TODO could be reseted at all??????????????
			// reset paid amount for deleted/closed schedules and disabled distributed fees 
			foreach (NL_Payments p in WorkingModel.Loan.Payments) {

				foreach (NL_LoanSchedulePayments sp in p.SchedulePayments) {
					foreach (NL_LoanSchedules s in Calculator.schedule.Where(s => s.IsDeleted())) {
						if (s.LoanScheduleID == sp.LoanScheduleID) {
							sp.ResetInterestPaid = sp.PrincipalPaid;
							sp.ResetPrincipalPaid = sp.PrincipalPaid;

							sp.PrincipalPaid = 0;
							sp.InterestPaid = 0;
						}
					}
				}

				foreach (NL_LoanFeePayments fp in p.FeePayments) {
					foreach (NL_LoanFees f in Calculator.distributedFeesList.Where(f => f.DisabledTime.Equals(acceptionTime))) {
						if (f.LoanFeeID == fp.LoanFeeID) {
							fp.ResetAmount = fp.Amount;
							fp.Amount = 0;

							f.PaidAmount -= (decimal)fp.ResetAmount;
						}
					}
				}
			}

			Calculator.acceptedRolloverProcessed = true;
		}
	} 
} // namespace
