using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Code.Agreements
{
    public class AgreementsModelBuilder
    {
        private readonly CustomerModelBuilder _customerModelBuilder;
        private readonly APRCalculator _aprCalc;
        private readonly RepaymentCalculator _repaymentCalculator =new RepaymentCalculator();

        public AgreementsModelBuilder(CustomerModelBuilder customerModelBuilder)
        {
            _aprCalc = new APRCalculator();
            _customerModelBuilder = customerModelBuilder;
        }


        /// <summary>
        /// Either customer.LastCashRequest or Loan should be available
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="amount"></param>
        /// <param name="loan"></param>
        /// <returns></returns>
        public virtual AgreementModel Build(Customer customer, decimal amount, Loan loan)
        {
            var now = DateTime.UtcNow;

            if (customer.LastCashRequest == null && loan == null) throw new ArgumentException("LastCashRequest or Loan is required");

            var cashRequest = loan.CashRequest;

            var useSetupFee = cashRequest.UseSetupFee;

            var fee = 0M;

            if (useSetupFee)
            {
                var feeCalculator = new SetupFeeCalculator();
                fee = feeCalculator.Calculate(amount);
            }

            var apr = _aprCalc.Calculate(amount, loan.Schedule, fee, loan.Date);
            return GenerateAgreementModel(customer, loan, useSetupFee, now, apr);
        }

        public virtual AgreementModel ReBuild(Customer customer, Loan loan)
        {
            var useSetupFee = loan.SetupFee != 0;
            return GenerateAgreementModel(customer, loan, useSetupFee, loan.Date, (double)loan.APR);
        }

        private AgreementModel GenerateAgreementModel(Customer customer, Loan loan, bool useSetupFee, DateTime now, double apr)
        {
            var model = new AgreementModel();

            model.Schedule = loan.Schedule.Select(s => LoanScheduleItemModel.FromLoanScheduleItem(s)).ToList();
            model.Customer = _customerModelBuilder.BuildWizardModel(customer);
            model.TypeOfBusinessName = model.Customer.CustomerPersonalInfo.TypeOfBusinessName;

            var businessType = model.Customer.CustomerPersonalInfo.TypeOfBusiness;

            if (businessType.Reduce() == TypeOfBusinessReduced.Limited)
            {
                model.CompanyName = model.Customer.LimitedInfo.LimitedCompanyName;
                model.CompanyNumber = model.Customer.LimitedInfo.LimitedCompanyNumber;
                model.Address = model.Customer.LimitedAddress.FirstOrDefault();
            }
            else if (businessType.Reduce() == TypeOfBusinessReduced.NonLimited)
            {
                model.CompanyName = model.Customer.NonLimitedInfo.NonLimitedCompanyName;
                model.Address = model.Customer.NonLimitedAddress.FirstOrDefault();
            }

            model.CustomerAddress = model.Customer.PersonalAddress.FirstOrDefault();

            model.CompanyAdress = model.Address.GetFormatted();
            model.PersonAddress = model.CustomerAddress.GetFormatted();

            CalculateTotal(useSetupFee, loan.Schedule.ToList(), model);

            model.CurentDate = FormattingUtils.FormatDateTimeToString(now);
            model.CurrentDate = now;

            model.FormattedSchedules = CreateSchedule(loan.LoanAmount,loan.Schedule.ToList(), loan.Date, loan.SetupFee);

            model.InterestRate = loan.InterestRate * 100;
            model.SetupFee = FormattingUtils.NumericFormats(loan.SetupFee);
            model.APR = apr;

            var start = loan.Schedule.First().Date.AddMonths(-1);
            var end = loan.Schedule.Last().Date;
            var days = (end - start).TotalDays;

            model.Term = _repaymentCalculator.ReCalculateRepaymentPeriod(loan.CashRequest);
            
            //model.InterestRatePerDay = (model.Schedule.Count * model.InterestRate) / (decimal)days;
            model.InterestRatePerDay = model.Schedule[1].InterestRate/30; // For first month
            model.InterestRatePerDayFormatted = string.Format("{0:0.00}", model.InterestRatePerDay);
            model.InterestRatePerYearFormatted = string.Format("{0:0.00}", model.InterestRate * 12);

            var loanType = customer.LastCashRequest.LoanType ?? new StandardLoanType();

            model.LoanType = loanType.Type;
            model.TermOnlyInterest = loan.Schedule.Count(s => s.LoanRepayment == 0);
            model.TermOnlyInterestWords = FormattingUtils.ConvertToWord(model.TermOnlyInterest).ToLower();
            model.TermInterestAndPrincipal = loan.Schedule.Count(s => s.LoanRepayment != 0 && s.Interest != 0);
            model.TermInterestAndPrincipalWords = FormattingUtils.ConvertToWord(model.TermInterestAndPrincipal).ToLower();
            model.isHalwayLoan = loanType.IsHalwayLoan;
            model.CountRepayment = _repaymentCalculator.CalculateCountRepayment(loan);

            model.TotalPrincipalWithSetupFee = FormattingUtils.NumericFormats (loan.Schedule.Sum(a => a.LoanRepayment) - loan.SetupFee);
            return model;
        }

        private IList<FormattedSchedule> CreateSchedule(decimal loanAmount, List<LoanScheduleItem> schedule, DateTime loanDate, decimal setupFee)
        {
            return schedule.Select((installment, i) => new FormattedSchedule
            {
                AmountDue = FormattingUtils.NumericFormats(installment.AmountDue),
                Principal = FormattingUtils.NumericFormats(installment.LoanRepayment),
                Interest = FormattingUtils.NumericFormats(installment.Interest),
                Fees = "0",
                Date = FormattingUtils.FormatDateToString(installment.Date),
                StringNumber = FormattingUtils.ConvertingNumberToWords(i + 1),
                InterestRate = string.Format("{0:0.0}", installment.InterestRate * 100),
                Iterration = i + 1,
				//AprMonthRate = string.Format("{0:0.0}", _aprCalc.CalculateMonthly(loanAmount, schedule, i, setupFee, loanDate))
            }).ToList();
        }

        public void CalculateTotal(bool useSetupFee, List<LoanScheduleItem> schedule, AgreementModel model)
        {
            model.TotalAmount = FormattingUtils.NumericFormats(schedule.Sum(a => a.AmountDue));
            model.TotalPrincipal = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
            model.TotalInterest = FormattingUtils.NumericFormats(schedule.Sum(a => a.Interest));
            model.TotalAmoutOfCredit = FormattingUtils.NumericFormats(schedule.Sum(a => a.LoanRepayment));
            model.TotalFees = FormattingUtils.NumericFormats(useSetupFee ? (new SetupFeeCalculator()).Calculate(schedule.Sum(a => a.AmountDue)) : 0);
        }
    }
}