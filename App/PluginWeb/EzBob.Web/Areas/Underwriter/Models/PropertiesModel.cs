namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using EZBob.DatabaseLib.Model.Database;
	using LandRegistryLib;

	public class PropertiesModel {
		public PropertiesModel() {
			Properties = new List<PropertyModel>();
		}

		public void Init(int numberOfProperties, int numberOfMortgages, int assetsValue, int totalMortgages, int zooplaAverage) {
			NumberOfProperties = numberOfProperties;
			NumberOfMortgages = numberOfMortgages;
			MarketValue = assetsValue;
			Zoopla1YearAverage = zooplaAverage;
			SumOfMortgages = totalMortgages;
			NetWorth = MarketValue - SumOfMortgages;
			Ltv = MarketValue == 0 ? 0 : SumOfMortgages * 100 / MarketValue;
			NetWorthPercentages = 100 - Ltv;
		}

		public int NumberOfProperties { get; set; }
		public int NumberOfMortgages { get; set; }
		public int MarketValue { get; set; }
		public int SumOfMortgages { get; set; }
		public int Ltv { get; set; }
		public int NetWorth { get; set; }
		public int NetWorthPercentages { get; set; }
		public List<PropertyModel> Properties { get; set; }
		public int Zoopla1YearAverage { get; set; }
	}

	[Serializable]
	public enum PropertyVerifyStatus {
		[Description("Owned")]
		VerifiedOwned,
		[Description("Not Owned")]
		VerifiedNotOwned,
		[Description("Not verified")]
		NotVerified
	}

	[Serializable]
	public class PropertyModel {
		public PropertyVerifyStatus VerifyStatus { get; set; }
		public string Status { get { return VerifyStatus.DescriptionAttr(); } }
		public string StatusAlert {
			get {
				switch (VerifyStatus) {
					case PropertyVerifyStatus.VerifiedOwned:
						return "success";
					case PropertyVerifyStatus.VerifiedNotOwned:
						return "danger";
					case PropertyVerifyStatus.NotVerified:
						return "warning";
					default:
						return "empty";
				}
			}
		}
		public int AddressId { get; set; }
		public int MarketValue { get; set; }
		public string Address { get; set; }
		public DateTime? YearOfOwnership { get; set; }
		public int NumberOfOwners { get; set; }
		public string Postcode { get; set; }
		public string FormattedAddress { get; set; }
		public List<LandRegistryResModel> LandRegistries { get; set; }
		public List<LandRegistryEnquiryModel> LandRegistryEnquiries { get; set; }
		public Zoopla Zoopla { get; set; }
		public string AddressType { get; set; }
	}
}
