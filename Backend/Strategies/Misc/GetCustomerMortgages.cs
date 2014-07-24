namespace EzBob.Backend.Strategies.Misc 
{
	using System.Data;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class GetCustomerMortgages : AStrategy
	{
		private readonly int customerId;
		private string firstName;
		private string surname;
		private string gender;
		private DateTime? dateOfBirth;
		private string line1;
		private string line2;
		private string line3;
		private string town;
		private string county;
		private string postcode;

		public GetCustomerMortgages(AConnection oDb, ASafeLog oLog, int customerId)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		}

		public override string Name {
			get { return "GetCustomerMortgages"; }
		}

		public bool HasMortgages { get; set; }

		public int MortgagesSum { get; set; }

		public override void Execute()
		{
			HasMortgages = false;
			MortgagesSum = 0;
			try
			{

				GatherData();
				var loc = new InputLocationDetailsMultiLineLocation
					{
						LocationLine1 = line1,
						LocationLine2 = line2,
						LocationLine3 = line3,
						LocationLine4 = town,
						LocationLine5 = county,
						LocationLine6 = postcode
					};

				var consumerSrv = new ConsumerService();
				ConsumerServiceResult eInfo = consumerSrv.GetConsumerInfo(firstName, surname, gender, dateOfBirth, null, loc, "PL",
				                                                          customerId, 0, true, false, false);
				if (eInfo.Output.Output.FullConsumerData.ConsumerData.CAIS != null)
				{
					foreach (var caisData in eInfo.Output.Output.FullConsumerData.ConsumerData.CAIS)
					{
						foreach (var caisDetails in caisData.CAISDetails)
						{
							var accStatus = caisDetails.AccountStatus;
							string MortgageAccounts = "03,16,25,30,31,32,33,34,35,69";
							if ((accStatus == "D" || accStatus == "A") &&
							    MortgageAccounts.IndexOf(caisDetails.AccountType, StringComparison.Ordinal) >= 0) // it is mortgage account
							{
								double balance = 0;
								bool isMainApplicantAccount = caisDetails.MatchDetails != null && caisDetails.MatchDetails.MatchTo == "1";
								if (isMainApplicantAccount)
								{
									HasMortgages = true;
									if (caisDetails.Balance != null && caisDetails.Balance.Amount != null)
									{
										string balanceStr = caisDetails.Balance.Amount.Replace("£", "");
										double.TryParse(balanceStr, out balance);
									}
								}
								MortgagesSum += (int) balance;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Log.Error("Exception while getting mortgages info: {0}", e);
			}
		}

		private void GatherData()
		{
			DataTable dt = DB.ExecuteReader("GetPersonalDataForMortgages", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));

			if (dt.Rows.Count != 1)
			{
				throw new Exception("Couldn't gather required data");
			}

			var sr = new SafeReader(dt.Rows[0]);

			firstName = sr["FirstName"];
			surname = sr["Surname"];
			gender = sr["Gender"];
			dateOfBirth = sr["DateOfBirth"];
			line1 = sr["Line1"];
			line2 = sr["Line2"];
			line3 = sr["Line3"];
			town = sr["Town"];
			county = sr["County"];
			postcode = sr["Postcode"];
		}
	}
}
