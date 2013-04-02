using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.Areas.Underwriter.Models.Reports;

namespace EzBob.Web.Code
{
    public class DailyReportBuilder
    {
        private readonly ILoanScheduleRepository _loanScheduleRepository;
        private readonly ILoanTransactionRepository _transactions;
        private readonly ILoanHistoryRepository _historyRepository;

        public DailyReportBuilder(
            ILoanScheduleRepository loanScheduleRepository, 
            ILoanTransactionRepository transactions,
            ILoanHistoryRepository historyRepository)
        {
            _loanScheduleRepository = loanScheduleRepository;
            _transactions = transactions;
            _historyRepository = historyRepository;
        }

        public List<ExpectationReportData> GenerateReport(int year, int month, int day)
        {
            var from = new DateTime(year, month, day);
            var to = from.AddDays(1);
            var installmentsQ = _loanScheduleRepository.GetByDate(from, to);
            var transactionsQ = _transactions.GetByDate(from ,to);

            var installments = installmentsQ.ToList();
            var transactions = transactionsQ.ToList();

            var loans = installments.Select(i => i.Loan).Union(transactions.Select(t => t.Loan)).Distinct();
            var historyItems = loans.Select(l => _historyRepository.FetchHistoryByDay(l, from)).ToList();

            return historyItems.Select(historyItem =>
                {
                    var ri = new ExpectationReportData();

                    ri.OriginationDate = historyItem.Loan.Date;
                    ri.LoanRef = historyItem.Loan.RefNumber;
                    ri.CustomerName = historyItem.Loan.Customer.PersonalInfo.Fullname;

                    ri.Before.Balance = historyItem.Before.Balance;
                    ri.Before.Total = historyItem.Before.Balance;
                    ri.Before.Interest = historyItem.Before.Interest;
                    ri.Before.Principal = historyItem.Before.Principal;
                    ri.Before.Fees = historyItem.Before.Fees;

                    ri.After.Balance = historyItem.After.Balance;
                    ri.After.Total = historyItem.After.Balance;
                    ri.After.Interest = historyItem.After.Interest;
                    ri.After.Principal = historyItem.After.Principal;
                    ri.After.Fees = historyItem.After.Fees;

                    var inst = installments.Where(i => i.Loan.Id == historyItem.Loan.Id);
                    if (inst.Any())
                    {
                        ri.Expected.Principal = inst.Sum(installment => installment.LoanRepayment);
                        ri.Expected.Interest = inst.Sum(installment => installment.Interest);
                        ri.Expected.Fees = inst.Sum(installment => installment.Fees);
                        ri.Expected.Total = inst.Sum(installment => installment.AmountDue);
                    }

                    if (historyItem.Expected != null && historyItem.Expected.ExpectedAmountDue > 0)
                    {
                        ri.Expected.Principal = historyItem.Expected.ExpectedPrincipal;
                        ri.Expected.Interest = historyItem.Expected.ExpectedInterest;
                        ri.Expected.Fees = historyItem.Expected.ExpectedFees;
                        ri.Expected.Total = historyItem.Expected.ExpectedAmountDue;
                    }

                    var done = transactions.Where(t => t.Loan.Id == historyItem.Loan.Id);
                    ri.Paid.Total = done.Sum(d => d.Amount);
                    ri.Paid.Fees = done.Sum(d => d.Fees);
                    ri.Paid.Interest = done.Sum(d => d.Interest);
                    ri.Paid.Principal = done.Sum(d => d.LoanRepayment);

                    ri.Variance = ri.Expected.Total - ri.Paid.Total;

                    return ri;
                }).ToList();
        }
    }
}