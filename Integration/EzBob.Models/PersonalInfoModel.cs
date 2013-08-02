using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using NHibernate;
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
        public string ReferenceSource { get; set; }
        public string ABTesting { get; set; }
        public bool IsMainStratFinished { get; set; }
        public string StrategyError { get; set; }
        public string FraudCheckStatus { get; set; }
		public string FraudHighlightCss { get; set; }
        public PersonalInfoModel()
        {
            IndustryFields = new List<string>();
            StrategyError = "";
        }

        public void InitFromCustomer(EZBob.DatabaseLib.Model.Database.Customer customer, ISession session = null)
        {
            if (customer == null) return;

            Id = customer.Id;
            IsTest = customer.IsTest;
            IsAvoid = customer.IsAvoid;
            ZohoId = customer.ZohoId;
            FraudCheckStatus = customer.FraudStatus.ToString();
			if (customer.FraudStatus == FraudStatus.FraudSuspect)
			{
				FraudHighlightCss = "red_cell";
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

            ReferenceSource = customer.ReferenceSource;
            ABTesting = customer.ABTesting;
            var app = customer.LastStartedMainStrategy;

            IsMainStratFinished = app != null &&
                (app.State == ApplicationStrategyState.SecurityViolation ||
                 app.State == ApplicationStrategyState.StrategyFinishedWithoutErrors ||
                 app.State == ApplicationStrategyState.StrategyFinishedWithErrors ||
                 app.State == ApplicationStrategyState.Error
                ) || app == null;

            if (app != null)
            {
                var sql = string.Format("SELECT top 1 ErrorMsg FROM Application_Application where ApplicationId = {0}",
                                    app.Id);
                StrategyError = session != null ? session.CreateSQLQuery(sql).UniqueResult<string>() : "";
            }
        }

        public string ZohoId { get; set; }
        public bool IsTest { get; set; }
        public bool IsAvoid { get; set; }
    }
}