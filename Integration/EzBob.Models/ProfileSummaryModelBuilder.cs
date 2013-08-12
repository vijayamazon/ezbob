﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExperianLib;
using EzBob.CommonLib.TimePeriodLogic;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter;
using EzBob.Web.Areas.Underwriter.Models;
using log4net;

namespace EzBob.Models
{
    public class ProfileSummaryModelBuilder
    {
        private readonly IDecisionHistoryRepository _decisions;
        private readonly MarketPlacesFacade _mpFacade;

        public ProfileSummaryModelBuilder(IDecisionHistoryRepository decisions, MarketPlacesFacade mpFacade)
        {
            _decisions = decisions;
            _mpFacade = mpFacade;
        }


        private static readonly ILog _log = LogManager.GetLogger(typeof(ProfileSummaryModelBuilder));

        public ProfileSummaryModel CreateProfile(Customer customer)
        {
            var summary = new ProfileSummaryModel {Id = customer.Id};

            BuildMarketplaces(customer, summary);

            BuildCreditBureau(customer, summary);
            
            BuildPaymentAccounts(customer, summary);

            AddDecisionHistory(summary, customer);

            summary.AffordabilityAnalysis =
                    new AffordabilityAnalysis
                    {
                        CashAvailabilityOrDeficits = "Not implemented now",
                        EzBobMonthlyRepayment = Money(GetRepaymentAmount(customer))
                    };

            summary.LoanActivity = CreateLoanActivity(customer);

            summary.AmlBwa =
                new AmlBwa
                {
                    Aml = customer.AMLResult,
                    Bwa = customer.BWAResult,
                    Lighter = new Lighter(ObtainAmlState(customer))
                };

            summary.FraudCheck = new FraudCheck
                {
                    Status = customer.Fraud.ToString(),
                    
                };

            summary.OverallTurnOver = customer.PersonalInfo.OverallTurnOver;
            summary.WebSiteTurnOver = customer.PersonalInfo.WebSiteTurnOver;

            summary.Comment = customer.Comment;

            return summary;
        }

        private static void BuildCreditBureau(Customer customer, ProfileSummaryModel summary)
        {
            var creditBureau = new CreditBureau();
            var consumerSrv = new ConsumerService();

            try
            {
                var loc = new EzBobIntegration.Web_References.Consumer.InputLocationDetailsMultiLineLocation();
                if (customer.AddressInfo.PersonalAddress.Any())
                {
                    var customerMainAddress = customer.AddressInfo.PersonalAddress.First();

                    loc.LocationLine1 = customerMainAddress.Line1;
                    loc.LocationLine2 = customerMainAddress.Line2;
                    loc.LocationLine3 = customerMainAddress.Line3;
                    loc.LocationLine4 = customerMainAddress.Town;
                    loc.LocationLine5 = customerMainAddress.County;
                    loc.LocationLine6 = customerMainAddress.Postcode;
                }
                var result = consumerSrv.GetConsumerInfo(customer.PersonalInfo.FirstName, customer.PersonalInfo.Surname,
                    customer.PersonalInfo.Gender.ToString(), // should be Gender
                    customer.PersonalInfo.DateOfBirth, null, loc, "PL", customer.Id, 0, true);

                if (result != null)
                {
                    creditBureau.CreditBureauScore = result.BureauScore;
                    creditBureau.TotalDebt = result.TotalAccountBalances;
                    creditBureau.TotalMonthlyRepayments = result.SumOfRepayements;
                    creditBureau.CreditCardBalances = result.CreditCardBalances;
                    creditBureau.BorrowerType =
                        TypeOfBusinessExtenstions.TypeOfBussinessForWeb(customer.PersonalInfo.TypeOfBusiness);
                    //creditBureau.Lighter = new Lighter(ObtainCreditBureauState(result.ExperianResult));
                    creditBureau.FinancialAccounts = customer.FinancialAccounts;
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            summary.CreditBureau = creditBureau;
        }

        private decimal GetRepaymentAmount(Customer customer)
        {
            var customerSchedule = customer.Loans
                .Where(x => x.Status == LoanStatus.Late || x.Status == LoanStatus.Live)
                .Select(x => x.Schedule)
                .ToList();

            var monthlyRepaymentSum = customerSchedule.Sum(x => x.Sum(y => y.AmountDue));
            var count = customerSchedule.Count();
            var repaymentAmount = count != 0 ? monthlyRepaymentSum / count : 0;
            return repaymentAmount;
        }

        private void BuildMarketplaces(Customer customer, ProfileSummaryModel summary)
        {
            double? anualTurnOver = 0;

            double? totalPositiveReviews = 0;
            double? totalNegativeReviews = 0;
            double? totalNeutralReviews = 0;
            double? totalReviews = 0;

            var inventory = 0d;

            var marketplacesAll = customer.CustomerMarketPlaces
                .Where(mp => mp.Marketplace.IsPaymentAccount == false).ToList();


            var marketplaces =
                marketplacesAll.Where(mp => mp.Disabled == false && string.IsNullOrEmpty(mp.UpdateError)).ToList();

            foreach (var mp in marketplaces)
            {
                var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
                var av = analisysFunction.Data
                    .FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

                var lastAnualTurnover = av != null
                    ? av.LastOrDefault(
                        x =>
                            x.ParameterName == "Total Sum of Orders" && x.TimePeriod.TimePeriodType <= TimePeriodEnum.Year)
                    : null;
                anualTurnOver += lastAnualTurnover != null
                    ? Double.Parse(lastAnualTurnover.Value.ToString(), CultureInfo.InvariantCulture)
                    : 0;

                inventory += av != null
                    ? av.Where(
                        x =>
                            x.ParameterName == "Total Value of Inventory" &&
                            x.TimePeriod.TimePeriodType == TimePeriodEnum.Lifetime)
                        .Sum(x => Double.Parse(x.Value.ToString(), CultureInfo.InvariantCulture))
                    : 0;

                var isAmazon = mp.Marketplace.Name == "Amazon";
                var amazonFeedback = mp.AmazonFeedback.LastOrDefault();
                var ebayFeedBack = mp.EbayFeedback.LastOrDefault();

                var feedbackByPeriodAmazon = amazonFeedback != null
                    ? amazonFeedback.FeedbackByPeriodItems.FirstOrDefault(x => x.TimePeriod.Id == 4)
                    : null;
                var feedbackByPeriodEbay = ebayFeedBack != null
                    ? ebayFeedBack.FeedbackByPeriodItems.FirstOrDefault(x => x.TimePeriod.Id == 4)
                    : null;

                totalNegativeReviews += isAmazon
                    ? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Negative : 0)
                    : (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Negative : 0);
                totalPositiveReviews += isAmazon
                    ? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Positive : 0)
                    : (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Positive : 0);
                totalNeutralReviews += isAmazon
                    ? (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Neutral : 0)
                    : (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Neutral : 0);
            }

            totalReviews = totalNegativeReviews + totalPositiveReviews + totalNeutralReviews;

            summary.MarketPlaces =
                new MarketPlaces
                {
                    NumberOfStores = String.Format("{0} / {1}", marketplaces.Count, marketplacesAll.Count),
                    AnualTurnOver = anualTurnOver,
                    Inventory = Money(inventory),
                    Seniority = Money(GetSeniority(customer, false)),
                    TotalPositiveReviews =
                        String.Format("{0:0.#} ({1:0.#}%)", totalPositiveReviews,
                            (totalReviews != 0 ? totalPositiveReviews/totalReviews*100 : 0)),
                    Lighter = new Lighter(ObtainMarketPlacesState(marketplaces))
                };
        }

        private void BuildPaymentAccounts(Customer customer, ProfileSummaryModel summary)
        {
            var paymentAccounts = customer.GetPaymentAccounts();
            summary.PaymentAccounts =
                new PaymentAccounts
                {
                    NumberOfPayPalAccounts = String.Format("{0}", paymentAccounts.Count()),
                    Balance = "Not implemented now",
                    NetExpences = String.Format("{0}", paymentAccounts.Sum(x => x.TotalNetOutPayments)),
                    NetIncome = paymentAccounts.Sum(x => x.TotalNetInPayments),
                    Lighter = new Lighter(ObtainPaymentsAccountsState(customer)),
                    Seniority = Money(GetSeniority(customer, true))
                };
        }

        private double GetSeniority(Customer customer, bool isPaymentAccountOnly)
        {
            var marketplacesSeniority = _mpFacade.MarketplacesSeniority(customer, false, isPaymentAccountOnly);
            var minAccountAge = DateTime.UtcNow - marketplacesSeniority;
            var minAccountAgeTotalMonth = minAccountAge.TotalDays/30;
            return minAccountAgeTotalMonth;
        }

        private LoanActivity CreateLoanActivity(Customer customer)
        {

            var previousLoans = customer.Loans.Count(x => x.DateClosed != null);
            var currentBalance = customer.Loans.Sum(x => x.Balance);
            var latePayments = customer.Loans.Sum(x => x.PastDues);
            var interest = customer.Loans.Where(l => l.Status == LoanStatus.Late).Sum(l => l.InterestDue);
            var collection = customer.Loans.Where(x => x.IsDefaulted).Sum(x => x.PastDues);

            return new LoanActivity
                {
                    PreviousLoans = Money(previousLoans),
                    CurrentBalance = Money(currentBalance),
                    LatePaymentsSum = Money(latePayments),
                    Collection = Money(collection),
                    LateInterest = Money(interest ?? 0),
                    Lighter = new Lighter(ObtainLoanActivityState(latePayments, collection))
                };
        }

        private static string Money(decimal amount)
        {
            return String.Format("{0:0.#}", amount);
        }

        private static string Money(double amount)
        {
            return Money((decimal)amount);
        }

        private static string Money(int amount)
        {
            return Money((decimal)amount);
        }

        private void AddDecisionHistory(ProfileSummaryModel summary, Customer customer)
        {
            summary.DecisionHistory = _decisions.ByCustomer(customer).Select(DecisionHistoryModel.Create).OrderBy(x => x.Date).ToList();
        }


        private LightsState ObtainCreditBureauState(string experianResult)
        {
            if (experianResult == null)
                return LightsState.Passed;
            switch (experianResult)
            {
                case "Passed":
                    return LightsState.Passed;
                case "Referred":
                case "Warning":
                    return LightsState.Warning;
                case "Rejected":
                    return LightsState.Reject;
                default :
                    throw new Exception("Unknown Expirian Result");
            }
        }

        private LightsState ObtainLoanActivityState(decimal latePayments, decimal collection)
        {
            if (collection > 0)
                return LightsState.Reject;
            if (latePayments > 0)
                return LightsState.Warning;
            return LightsState.Passed;
        }

        private LightsState ObtainMarketPlacesState(List<MP_CustomerMarketPlace> marketplaces)
        {
            if (marketplaces.Any(x => (!String.IsNullOrEmpty(x.UpdateError))))
            {
                return LightsState.Error;
            }

            if (marketplaces.Any(x => x.UpdatingStart != null && x.UpdatingEnd == null))
            {
                return LightsState.InProgress;
            }

            return LightsState.Passed;
        }


        private LightsState ObtainAmlState(Customer customer)
        {
            if (customer.AMLResult == "Rejected" || customer.BWAResult == "Rejected")
                return LightsState.Reject;
            if (customer.AMLResult == "Warning" || customer.BWAResult == "Warning")
                return LightsState.Warning;
            if (customer.AMLResult == "Not performed" || customer.BWAResult == "Not performed")
                return LightsState.NotPerformed;

            return LightsState.Passed;
        }

        private LightsState ObtainPaymentsAccountsState(Customer customer)
        {
            if (customer.BWAResult == "Warning")
                return LightsState.Warning;
            if (customer.BWAResult == "Rejected")
                return LightsState.Warning;

            return LightsState.Passed;
        }
    }
}