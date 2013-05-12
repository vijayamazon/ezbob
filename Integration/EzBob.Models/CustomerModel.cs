using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;

namespace EzBob.Web.Areas.Customer.Models
{
    [Serializable]
    public class CustomerModel
    {
        public int Id { get; set; }
        public bool loggedIn { get; set; }
        public bool bankAccountAdded { get; set; }

        public string userName { get; set; }
        public string accountType { get; set; }

        public PersonalInfo CustomerPersonalInfo { get; set; }

        public string BusinessTypeReduced { get; set; }

        public IEnumerable<SimpleMarketPlaceModel> ebayMarketPlaces { get; set; }
        public IEnumerable<SimpleMarketPlaceModel> amazonMarketPlaces { get; set; }
        public IEnumerable<SimpleMarketPlaceModel> ekmShops { get; set; }
        public IEnumerable<SimpleMarketPlaceModel> volusionShops { get; set; }
        public IEnumerable<SimpleMarketPlaceModel> playShops { get; set; }
        public IEnumerable<SimpleMarketPlaceModel> payPointAccounts { get; set; }        
        public IEnumerable<SimpleMarketPlaceModel> paypalAccounts { get; set; }

        public decimal? CreditSum { get; set; }
        public string CreditResult { get; set; }
        public DateTime? OfferStart { get; set; }
        public DateTime? OfferValidUntil { get; set; }
        public string Status { get; set; }

        public IEnumerable<LoanModel> Loans { get; set; }

        public AccountSettingsModel AccountSettings { get; set; }

        public decimal TotalBalance { get; set; }
        public decimal PrincipalBalance { get; set; }

        public decimal NextPayment { get; set; }
        public DateTime? NextPaymentDate { get; set; }

        public DateTime? GreetingMailSentDate { get; set; }

        public decimal LastPaymentTotal { get; set; }
        public decimal LastPaymentPrincipal { get; set; }
        public decimal LastPaymentInterest { get; set; }
        public decimal LastPaymentFees { get; set; }

        public string BankAccountNumber { get; set; }
        public string SortCode { get; set; }

        public PayPointCardModel[] PayPointCards { get; set; }


        public string Medal { get; set; }

        public LimitedInfoMap LimitedInfo { get; set; }

        public NonLimitedInfoMap NonLimitedInfo { get; set; }

        public CustomerAddress[] PersonalAddress { get; set; }

        public CustomerAddress[] LimitedAddress { get; set; }

        public CustomerAddress[] NonLimitedAddress { get; set; }


        public int ApplyCount { get; set; }

        public string CreditCardNo { get; set; }

        public string Email { get; set; }

        public decimal TotalEarlyPayment { get; set; }

        public decimal TotalPayEarlySavings { get; set; }

        public string EmailState { get; set; }

        public IEnumerable<PaymentRollover> ActiveRollovers { get; set; }

        public CollectionStatus CollectionStatus { get; set; }

        public bool HasRollovers { get; set; }

        public decimal TotalLatePayment { get; set; }

        public bool IsLoanDetailsFixed { get; set; }
    }

    public class LimitedInfoMap
    {
        public string LimitedCompanyNumber { get; set; }
        public string LimitedCompanyName { get; set; }
        public int? LimitedTimeAtAddress { get; set; }
        public string LimitedBusinessPhone { get; set; }

        public virtual DirectorModel[] Directors { get; set; }

        public static LimitedInfoMap FromLimitedInfo(LimitedInfo info)
        {
            return new LimitedInfoMap()
                       {
                           LimitedCompanyNumber = info.LimitedCompanyNumber,
                           LimitedCompanyName = info.LimitedCompanyName,
                           LimitedTimeAtAddress = info.LimitedTimeAtAddress,
                           LimitedBusinessPhone = info.LimitedBusinessPhone,
                           Directors = info.Directors.Select(d => DirectorModel.FromDirector(d)).ToArray()
                       };
        }
    }

    public class NonLimitedInfoMap
    {
        public string NonLimitedCompanyName { get; set; }
        public string NonLimitedTimeInBusiness { get; set; }
        public int? NonLimitedTimeAtAddress { get; set; }
        public bool? NonLimitedConsentToSearch { get; set; }
        public string NonLimitedBusinessPhone { get; set; }

        public virtual DirectorModel[] Directors { get; set; }


        public static NonLimitedInfoMap FromLimitedInfo(NonLimitedInfo info)
        {
            return new NonLimitedInfoMap()
            {
                NonLimitedCompanyName = info.NonLimitedCompanyName,
                NonLimitedTimeInBusiness = info.NonLimitedTimeInBusiness,
                NonLimitedTimeAtAddress = info.NonLimitedTimeAtAddress,
                NonLimitedConsentToSearch = info.NonLimitedConsentToSearch,
                NonLimitedBusinessPhone = info.NonLimitedBusinessPhone,
                Directors =info.Directors.Select(d => DirectorModel.FromDirector(d)).ToArray()
            };
        }
    }
}