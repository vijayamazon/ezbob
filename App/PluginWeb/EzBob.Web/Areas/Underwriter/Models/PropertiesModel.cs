namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;
	using LandRegistryLib;

	public class PropertiesModel
	{
		public PropertiesModel()
		{
			Properties = new List<PropertyModel>();
		}

		public void Init(int numberOfProperties, int numberOfMortgages, int assetsValue, int totalMortgages, int zoopla1YearAverage, DateTime? zooplaUpdateDate)
		{
			NumberOfProperties = numberOfProperties;
			NumberOfMortgages = numberOfMortgages;
			MarketValue = assetsValue;
			SumOfMortgages = totalMortgages;
			NetWorth = MarketValue - SumOfMortgages;
			Ltv = MarketValue == 0 ? 0 : SumOfMortgages * 100 / MarketValue;
			NetWorthPercentages = 100 - Ltv;
			Zoopla1YearAverage = zoopla1YearAverage;
			ZooplaUpdateDate = zooplaUpdateDate;
		}

		// TODO: add data for main address for fields that holds totals
		public int NumberOfProperties { get; set; }
		public int NumberOfMortgages { get; set; }
		public int MarketValue { get; set; }
		public int SumOfMortgages { get; set; }
		public int Ltv { get; set; }
		public int NetWorth { get; set; }
		public int NetWorthPercentages { get; set; }
		public int Zoopla1YearAverage { get; set; }
		public DateTime? ZooplaUpdateDate { get; set; }
		public List<LandRegistryResModel> LandRegistries { get; set; }
		public string Postcode { get; set; }
		public string FormattedAddress { get; set; }
		public List<PropertyModel> Properties { get; set; }
	}

	[Serializable]
	public class PropertyModel
	{
		public int MarketValue { get; set; }
		public string Address { get; set; }
		public int YearOfOwnership { get; set; }
		public int NumberOfOwners { get; set; }
	}
}
