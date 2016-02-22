namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Extensions;
	using EzBob.Models;
	using EzBob.Models.Marketplaces;
	using EZBob.DatabaseLib.Model.Database;

    public class SalesForceModel {
        public SalesForceModel() {
            PersonalModel = new PersonalModel();
            MarketPlaces = new List<SalesForceMarketPlaceModel>();
            CompanyFiles = new List<CompanyFile>();
            Phones = new List<CrmPhoneNumber>();
            Fraud = new FraudDetectionLogModel() {
                FraudDetectionLogRows = new List<FraudDetectionLogRowModel>()
            };

            Messages = new List<MessagesModel>();
            OldCrm = new List<CustomerRelationsModel>();
            Loans = new List<LoanHistoryModel>();
            Decisions = new List<DecisionHistoryModel>();
        }

		public PersonalModel PersonalModel { get; set; }
		
		public List<SalesForceMarketPlaceModel> MarketPlaces { get; set; }
		public List<CompanyFile> CompanyFiles { get; set; }
		public List<CrmPhoneNumber> Phones { get; set; } 
		public FraudDetectionLogModel Fraud { get; set; }
		public List<MessagesModel> Messages { get; set; }
		public List<CustomerRelationsModel> OldCrm { get; set; }

		public List<LoanHistoryModel> Loans { get; set;}
		public List<DecisionHistoryModel> Decisions { get; set; } 

		public void FromCustomer(EZBob.DatabaseLib.Model.Database.Customer customer) {
			PersonalModel = new PersonalModel {
				ID = customer.Id,
				Email = customer.Name,
				Name = customer.PersonalInfo != null ? customer.PersonalInfo.Fullname : string.Empty,
				FraudStatus = customer.FraudStatus.ToString(),
				CreditStatus = customer.CreditResult != null ? customer.CreditResult.ToString() : string.Empty,
				AmlScore = customer.AmlScore,
				AmlDescription = customer.AmlDescription,
                IsPendingDecision = customer.CreditResult.HasValue && 
                                    customer.CreditResult.Value == CreditResultStatus.ApprovedPending &&
                                   (!customer.IsWaitingForSignature.HasValue || (!customer.IsWaitingForSignature.Value)),
				TypeOfBusiness = customer.PersonalInfo != null ? customer.PersonalInfo.TypeOfBusiness.DescriptionAttr() : string.Empty,
				PromoCode = customer.PromoCode
			};

			if (customer.Company != null) {
				
				PersonalModel.CompanyName = string.IsNullOrEmpty(customer.Company.ExperianCompanyName) ? customer.Company.CompanyName : customer.Company.ExperianCompanyName;
				PersonalModel.CompanyRefNumber = string.IsNullOrEmpty(customer.Company.ExperianRefNum) ? customer.Company.CompanyNumber : customer.Company.ExperianRefNum;
			}

            if (customer.IsWaitingForSignature.HasValue && customer.IsWaitingForSignature.Value) {
                PersonalModel.CreditStatus += " (Signatures)";
            }

			MarketPlaces = customer
				.CustomerMarketPlaces
				.Where(x => x.Disabled == false)
				.Select(x => new SalesForceMarketPlaceModel {
					Created = x.Created,
					MpName = x.Marketplace.Name,
					displayName = x.DisplayName,
					UpdateError = x.UpdateError,
					Updated = x.UpdatingEnd
				})
				.ToList();

			Loans = customer
				.Loans
				.Select(LoanHistoryModel.Create)
				.OrderBy(x => x.DateApplied)
				.ToList();
		}
	}

	public class PersonalModel {
		public int ID { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string TypeOfBusiness { get; set; }
		public string CompanyName { get; set; }
		public string CompanyRefNumber { get; set; }
		public string FraudStatus { get; set; }
		public string CreditStatus { get; set; }
		public int? ExperianPersonalScore { get; set; }
		public int? ExperianCII { get; set; }
		public int? ExperianCompanyScore { get; set; }
		public int? AmlScore { get; set; }
		public string AmlDescription { get; set; }
        public bool IsPendingDecision { get; set; }
		public string PromoCode { get; set; }
	}
	public class SalesForceMarketPlaceModel : SimpleMarketPlaceModel {
		public DateTime? Created { get; set; }
		public DateTime? Updated { get; set; }
		public string UpdateError { get; set; }
	}
}