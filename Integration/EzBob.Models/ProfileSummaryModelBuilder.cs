﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Database.Repository;
using ExperianLib;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic;
using EzBob.PayPal;
using EzBob.Web.Areas.Customer.Models;
using log4net;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class ProfileSummaryModelBuilder
    {
        private readonly AlertRepository _alertRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly IDecisionHistoryRepository _decisions;

        public ProfileSummaryModelBuilder(AlertRepository alertRepository, CustomerRepository customerRepository, IDecisionHistoryRepository decisions)
        {
            _alertRepository = alertRepository;
            _customerRepository = customerRepository;
            _decisions = decisions;
        }


        private static readonly ILog _log = LogManager.GetLogger(typeof(ProfileSummaryModelBuilder));

        public ProfileSummaryModel CreateProfile(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            var summary = new ProfileSummaryModel();

            summary.Id = customer.Id;
            var alerts = _alertRepository.GetByCustomerId(customer.Id);
            var marketplacesSeniority = _customerRepository.MarketplacesSeniority(customer.Id, false);


            var paypalInternalId = new PayPalDatabaseMarketPlace().InternalId;

            var marketplacesAll = customer.CustomerMarketPlaces
                .Where(mp => mp.Marketplace.InternalId != paypalInternalId).ToList();

            var marketplaces = marketplacesAll.Where(mp => mp.EliminationPassed).ToList();

            var customerPayPal = customer.GetPayPalAccounts().ToList();

            double? anualTurnOver = 0;

            double? totalPositiveReviews = 0;
            double? totalNegativeReviews = 0;
            double? totalNeutralReviews = 0;
            double? totalReviews = 0;

            var customerSchedule = customer.Loans.Where(x => x.Status == LoanStatus.Late || x.Status == LoanStatus.Live)
                .Select(x => x.Schedule);
            var monthlyRepaymentSum = customerSchedule.Sum(x => x.Sum(y => y.AmountDue) + x.Sum(y => y.RepaymentAmount));
            var count = customerSchedule.Count();
            var repaymentAmount = count != 0 ? monthlyRepaymentSum / count : 0;

            var previousLoans = customer.Loans.Count(x => x.DateClosed != null);
            var currentBalance = customer.Loans.Sum(x => x.Balance);
            var latePayments = customer.Loans.Sum(x => x.PastDues);
            var collection = customer.Loans.Where(x => x.IsDefaulted).Sum(x => x.PastDues);
            var inventory = 0d;

            foreach (var mp in marketplacesAll)
            {
                var analisysFunction = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(mp.Id);
                var av = analisysFunction.Data
                         .FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

                var lastAnualTurnover = av != null ?
                    av.LastOrDefault(
                        x =>
                        x.ParameterName == "Total Sum of Orders" && x.TimePeriod.TimePeriodType <= TimePeriodEnum.Year) : null;
                anualTurnOver += lastAnualTurnover != null ? Double.Parse( lastAnualTurnover.Value.ToString(), CultureInfo.InvariantCulture) :
                    0;

                inventory += av != null ? 
                    av.Where(x => x.ParameterName == "Total Value of Inventory" && x.TimePeriod.TimePeriodType == TimePeriodEnum.Lifetime).Sum(x => Double.Parse(x.Value.ToString(), CultureInfo.InvariantCulture)) :
                    0;

                var isAmazon = mp.Marketplace.Name == "Amazon";
                var amazonFeedback = mp.AmazonFeedback.LastOrDefault();
                var ebayFeedBack = mp.EbayFeedback.LastOrDefault();

                var feedbackByPeriodAmazon = amazonFeedback != null ? amazonFeedback.FeedbackByPeriodItems.FirstOrDefault(x => x.TimePeriod.Id == 4) : null;
                var feedbackByPeriodEbay = ebayFeedBack != null ? ebayFeedBack.FeedbackByPeriodItems.FirstOrDefault(x => x.TimePeriod.Id == 4) : null;

                totalNegativeReviews += isAmazon ?
                    (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Negative : 0) :
                    (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Negative : 0);
                totalPositiveReviews += isAmazon ?
                    (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Positive : 0) :
                    (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Positive : 0);
                totalNeutralReviews += isAmazon ?
                    (feedbackByPeriodAmazon != null ? feedbackByPeriodAmazon.Neutral : 0) :
                    (feedbackByPeriodEbay != null ? feedbackByPeriodEbay.Neutral : 0);
            }

            var minAccountAge = DateTime.Now - marketplacesSeniority;
            var minAccountAgeTotalMonth = minAccountAge != null ? minAccountAge.Value.TotalDays / 30 : 0;

            totalReviews = totalNegativeReviews + totalPositiveReviews + totalNeutralReviews;

            summary.MarketPlaces =
                new MarketPlaces
                {
                    NumberOfStores = String.Format("{0} / {1}", marketplaces.Count, marketplacesAll.Count),
                    AnualTurnOver = anualTurnOver,
                    Inventory = string.Format("{0:0.#}", inventory),
                    Seniority = String.Format("{0:0.#}", minAccountAgeTotalMonth),
                    TotalPositiveReviews = String.Format("{0:0.#} ({1:0.#}%)", totalPositiveReviews, (totalReviews != 0 ? totalPositiveReviews / totalReviews * 100 : 0)),
                    Lighter = new Lighter(ObtainMarketPlacesState(alerts, marketplaces))
                };

            summary.PaymentAccounts =
                new PaymentAccounts
                {
                    NumberOfPayPalAccounts = String.Format("{0}", customerPayPal.Count),
                    Balance = "Not implemented now",
                    NetExpences = String.Format("{0}", customerPayPal.Sum(x => x.TotalNetOutPayments)),
                    NetIncome = customerPayPal.Sum(x => x.TotalNetInPayments),
                    Lighter = new Lighter(ObtainPaymentsAccountsState(customer))
                };
            summary.AffordabilityAnalysis =
                    new AffordabilityAnalysis
                    {
                        CashAvailabilityOrDeficits = "Not implemented now",
                        EzBobMonthlyRepayment = String.Format("{0:0.#}", repaymentAmount)
                    };
            summary.LoanActivity =
                new LoanActivity
                {
                    PreviousLoans = String.Format("{0:0.#}", previousLoans),
                    CurrentBalance = String.Format("{0:0.#}", currentBalance),
                    LatePaymentsSum = String.Format("{0:0.#}", latePayments),
                    Collection = String.Format("{0:0.#}", collection),
                    Lighter = new Lighter(ObtainLoanActivityState(latePayments, collection))
                };
            summary.AmlBwa =
                new AmlBwa
                {
                    Aml = customer.AMLResult,
                    Bwa = customer.BWAResult,
                    Fraud = String.Format("{0}", customer.Fraud ? "Yes" : "No"),
                    Lighter = new Lighter(ObtainAmlState(customer))
                };

            summary.OverallTurnOver = customer.PersonalInfo.OverallTurnOver;
            summary.WebSiteTurnOver = customer.PersonalInfo.WebSiteTurnOver;

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
                                                         customer.PersonalInfo.DateOfBirth, null, loc, "PL", customer.Id, true);

                if (result != null)
                {
                    creditBureau.CreditBureauScore = result.BureauScore;
                    creditBureau.TotalDebt = result.TotalAccountBalances;
                    creditBureau.TotalMonthlyRepayments = result.SumOfRepayements;
                    creditBureau.CreditCardBalances = result.CreditCardBalances;
                    creditBureau.BorrowerType = TypeOfBusinessExtenstions.TypeOfBussinessForWeb(customer.PersonalInfo.TypeOfBusiness);
                    creditBureau.Lighter = new Lighter(ObtainCreditBureauState(result.ExperianResult));
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            summary.CreditBureau = creditBureau;
            summary.Comment = customer.Comment;

            AddDecisionHistory(summary, customer);

            return summary;
        }

        private void AddDecisionHistory(ProfileSummaryModel summary, EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            summary.DecisionHistory = _decisions.ByCustomer(customer).Select(d => DecisionHistoryModel.Create(d)).ToList();
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

        private LightsState ObtainMarketPlacesState(List<MP_Alert> alerts, List<MP_CustomerMarketPlace> marketplaces)
        {
            if (marketplaces.Any(x => (!String.IsNullOrEmpty(x.UpdateError))))
            {
                return LightsState.Error;
            }

            if (marketplaces.Any(x => x.UpdatingStart != null && x.UpdatingEnd == null))
            {
                return LightsState.InProgress;
            }

            if (!alerts.Any()) return LightsState.Reject;

            return alerts.Any(x => x.AlertType.IndexOf("Elimination Orders Total", StringComparison.Ordinal) > -1 && x.AlertSeverity != AlertsSeverity.Passed) ?
                LightsState.Reject : LightsState.Passed;
        }


        private LightsState ObtainAmlState(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            if (customer.Fraud || customer.AMLResult == "Rejected" || customer.BWAResult == "Rejected")
                return LightsState.Reject;
            if (customer.AMLResult == "Warning" || customer.BWAResult == "Warning")
                return LightsState.Warning;

            return LightsState.Passed;
        }

        private LightsState ObtainPaymentsAccountsState(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            if (customer.BWAResult == "Warning")
                return LightsState.Warning;
            if (customer.BWAResult == "Rejected")
                return LightsState.Warning;

            return LightsState.Passed;
        }
    }
}