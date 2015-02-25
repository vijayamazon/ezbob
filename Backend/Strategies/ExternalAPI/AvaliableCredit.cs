
namespace Ezbob.Backend.Strategies.ExternalAPI {
	using System;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Database;

	public class AvaliableCredit : AStrategy {
		public override string Name {
			get { return "CustomerAvailableCredit"; }
		}

		public AvaliableCredit(string email) {
			this.CustomerEmail = email;
			Result = new AvailableCreditResult();
		}

		public override void Execute() {

			// check user exists
			int customerID = DB.ExecuteScalar<int>("GetCustomerIdByEmail", CommandSpecies.StoredProcedure, new QueryParameter("CustomerEmail", CustomerEmail));

			Console.WriteLine("===================================================="+customerID);

			if (customerID <= 0) {
				Log.Debug("Failed to retrieve customerID for email requested: {0}", CustomerEmail);
				return;
			}

			DB.FillFirst(
					Result,
					"GetCustomerAvalableCredit",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerEmail", this.CustomerEmail)
				);

			Result.CustomerEmail = this.CustomerEmail;

			Console.WriteLine(Result.ToString());
		}

		public string CustomerEmail { get; private set; }

		public AvailableCreditResult Result { get; private set; }
	}
}