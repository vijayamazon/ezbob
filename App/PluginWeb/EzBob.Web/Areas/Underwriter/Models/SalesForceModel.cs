namespace EzBob.Web.Areas.Underwriter.Models {
	using System.Collections.Generic;

	public class SalesForceModel {
		public int ID { get; set; }
		public string Email { get; set; }
		public string Name { get; set; }
		public string FraudStatus { get; set; }
		public string CreditStatus { get; set; }

		public List<CrmPhoneNumber> Phones { get; set; } 
		public FraudDetectionLogModel Fraud { get; set; }
		public List<MessagesModel> Messages { get; set; }
		public List<CustomerRelationsModel> OldCrm { get; set; }

		public void FromCustomer(EZBob.DatabaseLib.Model.Database.Customer customer) {
			ID = customer.Id;
			Email = customer.Name;
			Name = customer.PersonalInfo != null ? customer.PersonalInfo.Fullname : string.Empty;
			FraudStatus = customer.FraudStatus.ToString();
			CreditStatus = customer.CreditResult != null ? customer.CreditResult.ToString() : string.Empty;
		}
	}
}