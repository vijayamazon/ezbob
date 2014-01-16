namespace EzBob.Backend.Strategies
{
	using System;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianConsumerCheck : AStrategy
	{
		public ExperianConsumerCheck(int customerId, string firstName, string surname, string gender, DateTime birthDate, int directorId, string line1, string line2, string line3, string line4, string line5, string line6, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;

			this.firstName = firstName;
			this.surname = surname;
			this.gender = gender;
			this.birthDate = birthDate;
			this.directorId = directorId;
			this.line1 = line1;
			this.line2 = line2;
			this.line3 = line3;
			this.line4 = line4;
			this.line5 = line5;
			this.line6 = line6;
		} // constructor

		public override string Name
		{
			get { return "Experian consumer check"; }
		} // Name

		public override void Execute()
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
				Error = result.Error;
			}
			else
			{
				Score = (int)result.BureauScore;
				Error = null;
			}
			
			DB.ExecuteNonQuery(
				"UpdateExperianConsumer",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Name", firstName),
				new QueryParameter("Surname", surname),
				new QueryParameter("PostCode", line6),
				new QueryParameter("ExperianError", Error),
				new QueryParameter("ExperianScore", Score),
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("DirectorId", directorId)
			);
		} // Execute

		public string Error { get; private set; }
		public int Score { get; private set; }
		private readonly int customerId;
		private readonly string firstName;
		private readonly string surname;
		private readonly string gender;
		private readonly DateTime birthDate;
		private readonly int directorId;
		private readonly string line1;
		private readonly string line2;
		private readonly string line3;
		private readonly string line4;
		private readonly string line5;
		private readonly string line6;
	} // class ExperianConsumerCheck
} // namespace
