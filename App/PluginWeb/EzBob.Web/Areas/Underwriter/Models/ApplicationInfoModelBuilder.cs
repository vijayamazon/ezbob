using System;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Code;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class ApplicationInfoModelBuilder
    {
        private readonly IPacNetBalanceRepository _funds;
        private readonly RepaymentCalculator _repaymentCalculator = new RepaymentCalculator();
        private readonly ILoanTypeRepository _loanTypes;
        private readonly IDiscountPlanRepository _discounts;

        public ApplicationInfoModelBuilder(
            IPacNetBalanceRepository funds, 
            IDiscountPlanRepository discounts, 
            ILoanTypeRepository loanTypes)
        {
            _funds = funds;
            _discounts = discounts;
            _loanTypes = loanTypes;
        }

        public void InitApplicationInfo(ApplicationInfoModel model, EZBob.DatabaseLib.Model.Database.Customer customer, CashRequest cr)
        {
            if (customer == null) return;

            model.Id = customer.Id;

            if (cr != null)
            {
                model.InterestRate = cr.InterestRate;
                model.CashRequestId = cr.Id;
                model.UseSetupFee = cr.UseSetupFee;
                model.AllowSendingEmail = !cr.EmailSendingBanned;

                var loanType = cr.LoanType ?? _loanTypes.GetDefault();

                model.LoanType = loanType.Name;
                model.LoanTypeId = loanType.Id;

                cr.OfferStart = cr.OfferStart ?? customer.OfferStart;
                cr.OfferValidUntil = cr.OfferValidUntil ?? customer.OfferValidUntil;

                model.RepaymentPerion = _repaymentCalculator.ReCalculateRepaymentPeriod(cr);
            }

            model.CustomerId = customer.Id;
            model.IsTest = customer.IsTest;
            model.SystemDecision = customer.Status.ToString();

            if (cr.SystemCalculatedSum != null && cr.SystemCalculatedSum.Value != 0)
            {
                model.SystemCalculatedAmount = Convert.ToDecimal(cr.SystemCalculatedSum.Value);
            }


            model.OfferedCreditLine = Convert.ToDecimal(cr.ManagerApprovedSum ?? cr.SystemCalculatedSum);
            model.BorrowedAmount = customer.Loans.Where(x => x.CashRequest != null && x.CashRequest.Id == cr.Id).Sum(x => x.LoanAmount);
            model.AvaliableAmount = customer.CreditSum ?? 0M;
            model.OfferExpired = cr.OfferValidUntil <= DateTime.UtcNow;

            model.StartingFromDate = FormattingUtils.FormatDateToString(cr.OfferStart);
            model.OfferValidateUntil = FormattingUtils.FormatDateToString(cr.OfferValidUntil);

            model.FundsAvaliable = FormattingUtils.FormatPounds(_funds.GetFunds());
            //Status = "Active";
            model.Details = customer.Details;
            var isWaitingOrEscalated = customer.CreditResult == CreditResultStatus.WaitingForDecision ||
                                       customer.CreditResult == CreditResultStatus.Escalated;

            var isEnabled = customer.CollectionStatus.CurrentStatus == CollectionStatusType.Enabled;
            model.Editable = isWaitingOrEscalated && cr != null && isEnabled;

            model.IsModified = !string.IsNullOrEmpty(cr.LoanTemplate);

            model.LoanTypes = _loanTypes.GetAll().Select(t => LoanTypesModel.Create(t)).ToArray();

            model.DiscountPlans = _discounts.GetAll().Select(d => DiscountPlanModel.Create(d)).ToArray();
            var discountPlan = (cr.DiscountPlan ?? _discounts.GetDefault());
            model.DiscountPlan = discountPlan.Name;
            model.DiscountPlanPercents = discountPlan.Discounts.Any(d => d != 0) ? string.Format("({0})", discountPlan.ValuesStr) : "";
            model.DiscountPlanId = discountPlan.Id;

            model.Reason = cr.UnderwriterComment;

            model.IsLoanTypeSelectionAllowed = cr.IsLoanTypeSelectionAllowed;
        }
    }
}