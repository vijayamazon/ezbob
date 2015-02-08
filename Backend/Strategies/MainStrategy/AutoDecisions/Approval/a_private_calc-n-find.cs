namespace Ezbob.Backend.Strategies.MainStrategy.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Misc;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EzBob.Models;

	public partial class Approval {
		private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");

		private int CalculateRollovers() {
			return this.loanRepository.ByCustomer(this.customerId)
				.SelectMany(loan => loan.Schedule)
				.Sum(sch => sch.Rollovers.Count());
		} // CalculateRollovers

		private int CalculateSeniority() {
			if (this.customer == null)
				return -1;

			DateTime oMpOriginationDate = this.customer.GetMarketplaceOriginationDate(oIncludeMp: mp =>
				!mp.Marketplace.IsPaymentAccount ||
				mp.Marketplace.InternalId == PayPal ||
				mp.Marketplace.InternalId == Hmrc
			);

			DateTime oIncorporationDate = GetCustomerIncorporationDate();

			DateTime oDate = (oMpOriginationDate < oIncorporationDate) ? oMpOriginationDate : oIncorporationDate;

			return (int)(DateTime.UtcNow - oDate).TotalDays;
		} // CalculateSeniority

		private int CalculateTodaysApprovals() {
			DateTime today = Now;

			return this.cashRequestsRepository.GetAll().Count(cr =>
				cr.CreationDate.HasValue &&
				cr.CreationDate.Value.Year == today.Year &&
				cr.CreationDate.Value.Month == today.Month &&
				cr.CreationDate.Value.Day == today.Day &&
				cr.UnderwriterComment == "Auto Approval"
			);
		} // CalculateTodaysApprovals

		private decimal CalculateTodaysLoans() {
			DateTime today = Now;

			var todayLoans = this.loanRepository.GetAll()
				.Where(l => l.Date.Year == today.Year && l.Date.Month == today.Month && l.Date.Day == today.Day);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			return todayLoansAmount;
		} // CalculateTodaysLoans

		private void FindLatePayments() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = this.loanRepository.ByCustomer(this.customerId).Select(d => d.Id).ToList();

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
						this.m_oTrail.MyInputData.AddLatePayment(new Payment {
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
			MetaData oMeta = this.m_oTrail.MyInputData.MetaData; // just a shortcut

			List<Loan> outstandingLoans = FirstOfMonthStatusStrategyHelper.GetOutstandingLoans(this.customerId);

			oMeta.OpenLoanCount = outstandingLoans.Count;
			oMeta.TakenLoanAmount = 0;
			oMeta.RepaidPrincipal = 0;
			oMeta.SetupFees = 0;

			foreach (var loan in outstandingLoans) {
				oMeta.TakenLoanAmount += loan.LoanAmount;
				oMeta.RepaidPrincipal += loan.LoanAmount - loan.Principal;
				oMeta.SetupFees += loan.SetupFee;
			} // for
		} // FindOutstandingLoans
	} // class Approval
} // namespace
