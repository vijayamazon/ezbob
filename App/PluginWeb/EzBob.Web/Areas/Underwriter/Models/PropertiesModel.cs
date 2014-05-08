namespace EzBob.Web.Areas.Underwriter.Models
{
	public class PropertiesModel
	{
		public PropertiesModel(int numberOfProperties, int numberOfMortgages, int assetsValue, int totalMortgages)
		{
			NumberOfProperties = numberOfProperties;
			NumberOfMortgages = numberOfMortgages;
			MarketValue = assetsValue;
			SumOfMortgages = totalMortgages;
			NetWorth = MarketValue - SumOfMortgages;
			Ltv = MarketValue == 0 ? 0 : SumOfMortgages*100/MarketValue;
			NetWorthPercentages = 100 - Ltv;
			MarketValue /= 1000;
			SumOfMortgages /= 1000;
		}

		public int NumberOfProperties { get; set; }
		public int NumberOfMortgages { get; set; }
		public int MarketValue { get; set; }
		public int SumOfMortgages { get; set; }
		public int Ltv { get; set; }
		public int NetWorth { get; set; }
		public int NetWorthPercentages { get; set; }
	}
}
