
namespace ExperianLib.Dictionaries
{
	using System.Collections.Generic;

	public class AccountTypeDictionary
	{
		protected static Dictionary<string, string> AccountTypes;

		protected static void Initialize()
		{
			if (AccountTypes == null)
				AccountTypes = new Dictionary<string, string>();
			AccountTypes.Clear();

			//EZ-1895 begin
			AccountTypes.Add("01", "Hire purchase (including lease purchase)");
			AccountTypes.Add("02", "Loan");
			AccountTypes.Add("03", "Mortgage (including second)");
			AccountTypes.Add("04", "Budget Account / Revolving Account");
			AccountTypes.Add("05", "Credit card / Store card");
			AccountTypes.Add("06", "Charge card");
			AccountTypes.Add("07", "Rental");
			AccountTypes.Add("08", "Mail Order");
			AccountTypes.Add("15", "Current accounts");
			AccountTypes.Add("19", "Primary Lease");
			AccountTypes.Add("20", "Secondary Lease");
			AccountTypes.Add("21", "Dealer buy back");
			AccountTypes.Add("22", "Balloon Rental");
			//EZ-1895 end

			AccountTypes.Add("00", "Bank");
			AccountTypes.Add("12", "CML member");
			AccountTypes.Add("13", "CML member");
			AccountTypes.Add("14", "CML member");
			AccountTypes.Add("16", "Second mortgage (secured loan)");
			AccountTypes.Add("17", "Credit sale fixed term");
			AccountTypes.Add("18", "Communications");
			AccountTypes.Add("23", "Operating lease");
			AccountTypes.Add("24", "Unpresentable cheques");
			AccountTypes.Add("25", "Flexible Mortgages");
			AccountTypes.Add("26", "Consolidated Debt");
			AccountTypes.Add("27", "Combined Credit Account");
			AccountTypes.Add("28", "Pay Day Loans");
			AccountTypes.Add("29", "Balloon HP");
			AccountTypes.Add("30", "Residential Mortgage");
			AccountTypes.Add("31", "Buy To Let Mortgage");
			AccountTypes.Add("32", "100+% LTV Mortgage");
			AccountTypes.Add("33", "Current Account Off Set Mortgage");
			AccountTypes.Add("34", "Investment Off Set Mortgage");
			AccountTypes.Add("35", "Shared Ownership Mortgage");
			AccountTypes.Add("36", "Contingent Liability");
			AccountTypes.Add("37", "Store Card");
			AccountTypes.Add("38", "Multi Function Card");
			AccountTypes.Add("39", "Water");
			AccountTypes.Add("40", "Gas");
			AccountTypes.Add("41", "Electricity");
			AccountTypes.Add("42", "Oil");
			AccountTypes.Add("43", "Duel Fuel");
			AccountTypes.Add("44", "Fuel Card (not Motor fuel)");
			AccountTypes.Add("45", "House Insurance");
			AccountTypes.Add("46", "Car Insurance");
			AccountTypes.Add("47", "Life Insurance");
			AccountTypes.Add("48", "Health Insurance");
			AccountTypes.Add("49", "Card Protection");
			AccountTypes.Add("50", "Mortgage Protection");
			AccountTypes.Add("51", "Payment Protection");
			AccountTypes.Add("52", "Not Available in CAIS");
			AccountTypes.Add("53", "Mobile");
			AccountTypes.Add("54", "Fixed Line");
			AccountTypes.Add("55", "Cable");
			AccountTypes.Add("56", "Satellite");
			AccountTypes.Add("57", "Business Line");
			AccountTypes.Add("58", "Broadband");
			AccountTypes.Add("59", "Multi Communications");
			AccountTypes.Add("60", "Student Loan");
			AccountTypes.Add("61", "Home Credit");
			AccountTypes.Add("62", "Education");
			AccountTypes.Add("63", "Property Rental");
			AccountTypes.Add("64", "Other Rental");
			AccountTypes.Add("65", "Not Available in CAIS");
			AccountTypes.Add("66", "Not Available in CAIS");
			AccountTypes.Add("67", "Not Available in CAIS");
			AccountTypes.Add("68", "Not Available in CAIS");
			AccountTypes.Add("69", "Mortgage and Unsecured Loan");
			AccountTypes.Add("70", "Gambling");

		}

		public static string GetAccountType(string accountTypeCode)
		{
			if (AccountTypes == null)
			{
				AccountTypes = new Dictionary<string, string>();
				Initialize();
			}
			return AccountTypes.ContainsKey(accountTypeCode)
					? AccountTypes[accountTypeCode]
					: (accountTypeCode ?? string.Empty);
		}
	}
}
