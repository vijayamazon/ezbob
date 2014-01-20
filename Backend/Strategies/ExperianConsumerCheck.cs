namespace EzBob.Backend.Strategies
{
	using System;
	using System.Data;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianConsumerCheck : AStrategy
	{
		public ExperianConsumerCheck(int customerId, int directorId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
			this.directorId = directorId;

			GetAddresses();
			DataTable dt = DB.ExecuteReader("GetPersonalInfoForConsumerCheck", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId), new QueryParameter("DirectorId", directorId));
			var results = new SafeReader(dt.Rows[0]);
			firstName = results["FirstName"];
			surname = results["Surname"];
			gender = results["Gender"];
			birthDate = results["DateOfBirth"];
			timeAtAddress = results["TimeAtAddress"];
		} // constructor

		private void GetAddresses()
		{
			DataTable dt = DB.ExecuteReader("GetCustomerAddresses", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var addressesResults = new SafeReader(dt.Rows[0]);
			currentAddressLine1 = addressesResults["Line1"];
			currentAddressLine2 = addressesResults["Line2"];
			currentAddressLine3 = addressesResults["Line3"];
			currentAddressLine4 = addressesResults["Line4"];
			currentAddressLine5 = addressesResults["Line5"];
			currentAddressLine6 = addressesResults["Line6"];
			prevAddressLine1 = addressesResults["Line1Prev"];
			prevAddressLine2 = addressesResults["Line2Prev"];
			prevAddressLine3 = addressesResults["Line3Prev"];
			prevAddressLine4 = addressesResults["Line4Prev"];
			prevAddressLine5 = addressesResults["Line5Prev"];
			prevAddressLine6 = addressesResults["Line6Prev"];
		} // GetAddresses

		public override string Name
		{
			get { return "Experian consumer check"; }
		} // Name

		public override void Execute()
		{
			Log.Info("Starting consumer check with params: FirstName={0} Surname={1} Gender={2} DateOfBirth={3} Line1={4} Line2={5} Line3={6} Line4={7} Line5={8} Line6={9} PrevLine1={10} PrevLine2={11} PrevLine3={12} PrevLine4={13} PrevLine5={14} PrevLine6={15}",
				firstName, surname, gender, birthDate, currentAddressLine1, currentAddressLine2, currentAddressLine3, currentAddressLine4, currentAddressLine5, currentAddressLine6,
				prevAddressLine1, prevAddressLine2, prevAddressLine3, prevAddressLine4, prevAddressLine5, prevAddressLine6);
			GetConsumerInfoAndSave(currentAddressLine1, currentAddressLine2, currentAddressLine3, currentAddressLine4, currentAddressLine5, currentAddressLine6);

			if (!string.IsNullOrEmpty(error) && CanUsePrevAddress())
			{
				GetConsumerInfoAndSave(prevAddressLine1, prevAddressLine2, prevAddressLine3, prevAddressLine4, prevAddressLine5, prevAddressLine6);
			}
		}

		private bool CanUsePrevAddress()
		{
			return directorId == 0 && timeAtAddress == 1 && !string.IsNullOrEmpty(prevAddressLine6);
		}

		private void GetConsumerInfoAndSave(string line1, string line2, string line3, string line4, string line5, string line6)
		{
			var consumerService = new ConsumerService();

			var location = new InputLocationDetailsMultiLineLocation
				{
					LocationLine1 = line1,
					LocationLine2 = line2,
					LocationLine3 = line3,
					LocationLine4 = line4,
					LocationLine5 = line5,
					LocationLine6 = line6
				};

			ConsumerServiceResult result = consumerService.GetConsumerInfo(firstName, surname, gender, birthDate, null, location, "PL", customerId, directorId);

			if (result.IsError)
			{
				error = result.Error;
			}
			else
			{
				Score = (int) result.BureauScore;
				error = null;
			}

			DB.ExecuteNonQuery(
				"UpdateExperianConsumer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Name", firstName),
				new QueryParameter("Surname", surname),
				new QueryParameter("PostCode", line6),
				new QueryParameter("ExperianError", error),
				new QueryParameter("ExperianScore", Score),
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DirectorId", directorId)
				);
		}

		public int Score { get; private set; }
		private string error;
		private readonly int customerId;
		private readonly string firstName;
		private readonly string surname;
		private readonly string gender;
		private readonly DateTime birthDate;
		private readonly int directorId;
		private string currentAddressLine1;
		private string currentAddressLine2;
		private string currentAddressLine3;
		private string currentAddressLine4;
		private string currentAddressLine5;
		private string currentAddressLine6;
		private string prevAddressLine1;
		private string prevAddressLine2;
		private string prevAddressLine3;
		private string prevAddressLine4;
		private string prevAddressLine5;
		private string prevAddressLine6;
		private readonly int timeAtAddress;
	} // class ExperianConsumerCheck
} // namespace
