using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using StructureMap;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class PersonalInfoModel
    {
        public int Id { get; set; }
        public string RefNumber { get; set; }
        public string Name { get; set; }
        public string Medal { get; set; }
        public string Email { get; set; }
        public string EmailState { get; set; }
        public string MobilePhone { get; set; }
        public string DaytimePhone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<string> IndustryFields { get; set; }
        public string UserStatus { get; set; }
        public string CreditResult { get; set; }
        public double CreditScore { get; set; }
        public int Disabled { get; set; }
        public bool Editable { get; set; }
        public List<string> TopCategories { get; set; }
        public decimal? WebSiteTurnOver { get; set; }
        public decimal? OverallTurnOver { get; set; }

        public PersonalInfoModel()
        {
            IndustryFields = new List<string>();
        }

        public void InitFromCustomer(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            if (customer == null) return;

            Id = customer.Id;
            IsTest = customer.IsTest;
            ZohoId = customer.ZohoId;

            var ebayAmazonCategoryRepository = ObjectFactory.GetInstance<EbayAmazonCategoryRepository>();
            TopCategories = new List<string>();
            foreach (var mp in customer.CustomerMarketPlaces)
            {
                var category = ebayAmazonCategoryRepository.GetTopCategories(mp);
                if (!string.IsNullOrEmpty(category))
                {
                    TopCategories.Add(category);
                }
            }

            if (customer.PersonalInfo != null)
            {
                Name = customer.PersonalInfo.Fullname;
                MobilePhone = customer.PersonalInfo.MobilePhone;
                DaytimePhone = customer.PersonalInfo.DaytimePhone;

            }

            Medal = customer.Medal.HasValue ? customer.Medal.ToString() : "";
            Email = customer.Name;
            EmailState = customer.EmailState.ToString();
            RefNumber = customer.RefNumber;
            
            if (customer.GreetingMailSentDate != null) RegistrationDate = customer.GreetingMailSentDate.Value;
            IndustryFields.Add(string.Empty);
            UserStatus = customer.Status.ToString();
            CreditResult = customer.CreditResult.ToString();
            CreditScore = customer.ScoringResults.Any() ? customer.ScoringResults.Last().ScoreResult : 0.00;
            Disabled = (int)customer.CollectionStatus.CurrentStatus;
            Editable = customer.CreditResult == CreditResultStatus.WaitingForDecision && (customer.CollectionStatus.CurrentStatus == 0);

            if (customer.PersonalInfo != null)
            {
                OverallTurnOver = customer.PersonalInfo.OverallTurnOver;
                WebSiteTurnOver = customer.PersonalInfo.WebSiteTurnOver;
            }
        }

        public string ZohoId { get; set; }
        public bool IsTest { get; set; }
    }
}