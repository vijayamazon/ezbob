﻿namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Methods {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	internal class CalculatePlanMethod : AMethod {
		public CalculatePlanMethod(ALoanCalculator calculator, bool writeToLog) : base(calculator, writeToLog) {
		} // constructor

		public virtual List<Repayment> Execute() {
			WorkingModel.ValidateSchedule();

			DateTime firstInterestDay = WorkingModel.LoanIssueTime.Date.AddDays(1);

			DateTime lastInterestDay = WorkingModel.LastScheduledDate;

			var days = new DailyLoanStatus();

			for (DateTime d = firstInterestDay; d <= lastInterestDay; d = d.AddDays(1))
				days.Add(new OneDayLoanStatus(d, WorkingModel.LoanAmount, days.LastDailyLoanStatus));

			DateTime prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < WorkingModel.Schedule.Count; i++) {
				ScheduledItem sp = WorkingModel.Schedule[i];

				foreach (OneDayLoanStatus cls in days.Where(dd => dd.Date > sp.Date))
					cls.OpenPrincipal -= sp.Principal;

				DateTime preScheduleEnd = prevTime; // This assignment is to prevent "access to modified closure" warning.

				foreach (OneDayLoanStatus cls in days.Where(cls => preScheduleEnd < cls.Date && cls.Date <= sp.Date)) {
					cls.DailyInterestRate = Calculator.GetDailyInterestRate(
						cls.Date,
						sp.InterestRate,
						false,
						preScheduleEnd,
						sp.Date
					);
				} // for each

				prevTime = sp.Date;
			} // for each scheduled payment

			var result = new List<Repayment>(
				WorkingModel.Schedule.Select(sp => new Repayment(sp.Date, sp.Principal, 0, 0))
			);

			prevTime = WorkingModel.LoanIssueTime;

			for (int i = 0; i < result.Count; i++) {
				Repayment r = result[i];

				DateTime dt = prevTime; // This assignment is to prevent "access to modified closure" warning.

				r.Interest = days.Where(cls => dt < cls.Date && cls.Date <= r.Time).Sum(cls => cls.DailyInterest);

				prevTime = r.Time;
			} // for

			if (WriteToLog) {
				days.AddScheduleNotes(WorkingModel);

				Library.Instance.Log.Debug(
					"\n\n{3}.CreatePlan - begin:" +
					"\n\nLoan calculator model:\n{0}" +
					"\n\nLoan plan:\n\t\t{1}" +
					"\n\nDaily data:\n{2}" +
					"\n\n{3}.CreatePlan - end." +
					"\n\n",
					WorkingModel,
					string.Join("\n\t\t", result),
					days.ToFormattedString("\t\t"),
					Calculator.Name
				);
			} // if

			return result;
		} // Execute
	} // class CalculatePlanMethod
} // namespace
