namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using ConfigManager;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public partial class Approval {
		private int CalculateRollovers() {
			return this.loanRepository.ByCustomer(this.trail.CustomerID)
				.SelectMany(loan => loan.Schedule)
				.Sum(sch => sch.Rollovers.Count());
		} // CalculateRollovers

		private int CalculateSeniority() {
			if (this.customer == null) {
				this.log.Debug("CalculateSeniority: 0 because customer is null.");
				return -1;
			} // if

			DateTime oMpOriginationDate = this.db.ExecuteScalar<DateTime?>(
				"LoadCustomerFirstTransactionTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customer.Id)
			) ?? Now;

			this.log.Debug("CalculateSeniority: mp origination date is {0}.", oMpOriginationDate.MomentStr());

			DateTime oIncorporationDate = GetCustomerIncorporationDate();

			this.log.Debug("CalculateSeniority: incorporation date is {0}.", oIncorporationDate.MomentStr());

			DateTime oDate = (oMpOriginationDate < oIncorporationDate) ? oMpOriginationDate : oIncorporationDate;

			this.log.Debug("CalculateSeniority: chosen date is {0}.", oDate.MomentStr());

			this.log.Debug("CalculateSeniority: current date is {0}.", Now.MomentStr());

			int seniority = (int)(Now - oDate).TotalDays;

			this.log.Debug("CalculateSeniority: result is {0} days.", seniority);

			return seniority;
		} // CalculateSeniority

		private int CalculateTodaysApprovals(DateTime now) {
			return this.cashRequestsRepository.GetAll().Count(cr =>
				cr.UnderwriterDecisionDate.HasValue &&
				cr.UnderwriterDecisionDate.Value.Date == now.Date &&
				cr.AutoDecisionID == 1
			);
		} // CalculateTodaysApprovals

		private int CalculateHourlyApprovals() {
			return this.cashRequestsRepository.GetAll().Count(cr =>
				cr.UnderwriterDecisionDate.HasValue &&
				cr.UnderwriterDecisionDate.Value.Date == Now.Date &&
				cr.UnderwriterDecisionDate.Value.Hour == Now.Hour &&
				cr.AutoDecisionID == 1
			);
		} // CalculateHourlyTodaysApprovals

		private int CalculateLastHourApprovals() {
			DateTime anHourAgo = Now.AddHours(-1);

			return this.cashRequestsRepository.GetAll().Count(cr =>
				cr.UnderwriterDecisionDate.HasValue &&
				cr.UnderwriterDecisionDate.Value >= anHourAgo &&
				cr.AutoDecisionID == 1
			);
		} // CalculateLastHourApprovals

		private decimal CalculateTodaysLoans(DateTime now) {
			var todayLoans = this.loanRepository.GetAll().Where(l =>
				(l.Date.Date == now.Date) &&
				(l.Date < Now) &&
				(l.CashRequest != null) &&
				(l.CashRequest.AutoDecisionID == 1)
			);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			return todayLoansAmount;
		} // CalculateTodaysLoans

		private void FindLatePayments() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = this.loanRepository.ByCustomer(this.trail.CustomerID)
				.Select(d => d.Id)
				.ToList();

			foreach (int loanId in customerLoanIds) {
				int innerLoanId = loanId;

				IQueryable<LoanScheduleTransaction> backfilledMapping = this.loanScheduleTransactionRepository.GetAll()
					.Where(x =>
						x.Loan.Id == innerLoanId &&
							x.Schedule.Date.Date < x.Transaction.PostDate.Date &&
							x.Transaction.Status == LoanTransactionStatus.Done
					);

				foreach (var paymentMapping in backfilledMapping) {
					DateTime scheduleDate = paymentMapping.Schedule.Date.Date;

					DateTime transactionDate = paymentMapping.Transaction.PostDate.Date;

					double nTotalLateDays = (transactionDate - scheduleDate).TotalDays;

					if (nTotalLateDays > autoApproveMaxAllowedDaysLate) {
						this.trail.MyInputData.AddLatePayment(new Payment {
							LoanID = innerLoanId,
							ScheduleDate = paymentMapping.Schedule.Date.Date,
							ScheduleID = paymentMapping.Schedule.Id,
							TransactionID = paymentMapping.Transaction.Id,
							TransactionTime = paymentMapping.Transaction.PostDate,
						});
					} // if
				} // for
			} // for
		} // FindLatePayments

		private void FindOutstandingLoans() {
			MetaData oMeta = this.trail.MyInputData.MetaData; // just a shortcut

			List<Loan> outstandingLoans = FirstOfMonthStatusStrategyHelper.GetOutstandingLoans(this.trail.CustomerID);

			oMeta.OpenLoanCount = 0;
			oMeta.TakenLoanAmount = 0;
			oMeta.RepaidPrincipal = 0;
			oMeta.SetupFees = 0;

			foreach (Loan loan in outstandingLoans) {
				if (loan.DateClosed != null)
					continue;

				oMeta.OpenLoanCount++;
				oMeta.TakenLoanAmount += loan.LoanAmount;
				oMeta.RepaidPrincipal += loan.LoanAmount - loan.Principal;
				oMeta.SetupFees += loan.SetupFee;
			} // for
		} // FindOutstandingLoans

		private DateTime GetCustomerIncorporationDate() {
			if (this.incorporationDate.HasValue)
				return this.incorporationDate.Value;

			if (this.customer == null)
				this.incorporationDate = Now;
			else {
				this.incorporationDate = this.db.ExecuteScalar<DateTime?>(
					"GetCustomerIncorporationDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.customer.Id),
					new QueryParameter("Now", DateTime.UtcNow)
				) ?? Now;
			} // if

			return this.incorporationDate.Value;
		} // GetCustomerIncorporationDate

		private IEnumerable<string> FindBadCaisStatuses() {
			return this.experianConsumerData.Cais
				.Where(ca => ca.IsBad(Now))
				.Select(ca => string.Format(
					"ID {0}, updated on {1}, balance {2}, codes {3}",
					ca.Id,
					(ca.LastUpdatedDate ?? DateTime.UtcNow).ToString("d-MMM-yyyy", CultureInfo.InvariantCulture),
					Math.Max(ca.Balance ?? 0, ca.CurrentDefBalance ?? 0),
					ca.AccountStatusCodes
				));
		} // FindBadCaisStatuses

		private int DetectFraudStatusValue() {
			if (this.customer == null)
				return (int)FraudStatus.UnderInvestigation;

			var fraudStatus = new QueryParameter("@FraudStatus") {
				Direction = ParameterDirection.Output,
				Type = DbType.Int32,
			};

			this.db.ExecuteNonQuery(
				"DetectCustomerFraudStatus",
				new QueryParameter("@CustomerID", this.customer.Id),
				new QueryParameter("@Now", Now),
				fraudStatus
			);

			return (int)fraudStatus.ReturnedValue;
		} // DetectFraudStatusValue
	} // class Approval
} // namespace
