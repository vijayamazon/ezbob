namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.AutoDecision.AutoApproval;
	using ConfigManager;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

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
			return this.cashRequestsRepository.GetAll()
				.Count(cr =>
					cr.UnderwriterDecisionDate.HasValue &&
						cr.UnderwriterDecisionDate.Value.Year == Now.Year &&
						cr.UnderwriterDecisionDate.Value.Month == Now.Month &&
						cr.UnderwriterDecisionDate.Value.Day == Now.Day &&
						cr.AutoDecisionID == 1
				);
		} // CalculateTodaysApprovals

		private decimal CalculateTodaysLoans() {
			DateTime today = Now;

			var todayLoans = this.loanRepository.GetAll()
				.Where(l =>
					(l.Date.Year == today.Year) &&
						(l.Date.Month == today.Month) &&
						(l.Date.Day == today.Day) &&
						(l.Date < today)
				);

			decimal todayLoansAmount = 0;

			if (todayLoans.Any())
				todayLoansAmount = todayLoans.Sum(l => l.LoanAmount);

			return todayLoansAmount;
		} // CalculateTodaysLoans

		private void FindLatePayments() {
			int autoApproveMaxAllowedDaysLate = CurrentValues.Instance.AutoApproveMaxAllowedDaysLate;

			List<int> customerLoanIds = this.loanRepository.ByCustomer(this.customerId)
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

		private DateTime GetCustomerIncorporationDate() {
			if (this.customer == null)
				return DateTime.UtcNow;

			bool bIsLimited =
				(this.customer.Company != null) &&
					(this.customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited);

			if (bIsLimited) {
				CustomerAnalytics oAnalytics = this.customerAnalytics.GetAll()
					.FirstOrDefault(ca => ca.Id == this.customer.Id);

				DateTime oIncorporationDate = (oAnalytics != null) ? oAnalytics.IncorporationDate : DateTime.UtcNow;

				if (oIncorporationDate.Year < 1000)
					oIncorporationDate = DateTime.UtcNow;

				return oIncorporationDate;
			} // if ltd

			DateTime? oDate = this.db.ExecuteScalar<DateTime?>(
				"GetNoLtdIncorporationDate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customer.Id)
			);

			return oDate ?? DateTime.UtcNow;
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
