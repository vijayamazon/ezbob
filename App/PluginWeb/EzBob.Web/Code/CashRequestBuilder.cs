﻿using System;
using System.Linq;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Infrastructure;
using PaymentServices.Calculators;

namespace EzBob.Web.Code
{
    public class CashRequestBuilder
    {
        private readonly ILoanTypeRepository _loanTypes;
        private readonly IDiscountPlanRepository _discounts;
        private readonly IAppCreator _creator;
        private readonly IUsersRepository _users;
        private readonly IEzBobConfiguration _config;

        public CashRequestBuilder(
                                    ILoanTypeRepository loanTypes, 
                                    IDiscountPlanRepository discounts, 
                                    IAppCreator creator,
                                    IUsersRepository users,
                                    IEzBobConfiguration config
            )
        {
            _loanTypes = loanTypes;
            _discounts = discounts;
            _creator = creator;
            _users = users;
            _config = config;
        }

        public CashRequest CreateCashRequest(Customer customer)
        {
            var loanType = _loanTypes.GetDefault();
            var discount = _discounts.GetDefault();

            var cashRequest = new CashRequest
                                  {
                                      CreationDate = DateTime.UtcNow,
                                      Customer = customer,
                                      InterestRate = 0.06M,
                                      LoanType = loanType,
                                      RepaymentPeriod = loanType.RepaymentPeriod,
                                      UseSetupFee = false,
                                      DiscountPlan = discount,
                                      OfferValidUntil = DateTime.UtcNow.AddDays(1),
                                      OfferStart = DateTime.UtcNow,
									  IsLoanTypeSelectionAllowed = 1
                                  };

            customer.CashRequests.Add(cashRequest);

            return cashRequest;
        }

        public void ForceEvaluate(Customer customer, NewCreditLineOption newCreditLineOption, bool isUnderwriterForced)
        {
            if (
                customer.CustomerMarketPlaces.Any(
                    x =>
                    x.UpdatingEnd != null &&
                    (DateTime.UtcNow - x.UpdatingEnd.Value).Days > _config.UpdateOnReapplyLastDays) &&
                (newCreditLineOption == NewCreditLineOption.UpdateEverythingAndApplyAutoRules ||
                 newCreditLineOption == NewCreditLineOption.UpdateEverythingAndGoToManualDecision))
            {
                //UpdateAllMarketplaces не успевает проставить UpdatingEnd = null для того что бы MainStrategy подождала окончание его работы
                foreach (var val in customer.CustomerMarketPlaces)
                {
                    val.UpdatingEnd = null;
                }
                _creator.UpdateAllMarketplaces(customer);
            }
            _creator.Evaluate(_users.Get(customer.Id), newCreditLineOption, Convert.ToInt32(customer.IsAvoid), isUnderwriterForced);
        }
    }
}