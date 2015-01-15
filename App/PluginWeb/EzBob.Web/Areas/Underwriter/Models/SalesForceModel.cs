namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EzBob.Models;
	using EzBob.Models.Marketplaces;

	public class SalesForceModel {
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
				ExperianScore = customer.ExperianConsumerScore,
			};

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

			Decisions = customer
				.CashRequests
				.SelectMany(x => x.DecisionHistories)
				.Select(DecisionHistoryModel.Create)
				.OrderBy(x => x.Date)
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
		public string FraudStatus { get; set; }
		public string CreditStatus { get; set; }
		public int? ExperianScore { get; set; }
		public int? AmlScore { get; set; }
		public string AmlDescription { get; set; }
	}
	public class SalesForceMarketPlaceModel : SimpleMarketPlaceModel {
		public DateTime? Created { get; set; }
		public DateTime? Updated { get; set; }
		public string UpdateError { get; set; }
	}
}