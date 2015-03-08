
namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba {
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Database;

	public class CustomerAvaliableCredit : AStrategy {
		public override string Name {
			get { return "Alibaba CustomerAvaliableCredit"; }
		}

		public CustomerAvaliableCredit(int customerID, int aliMemberID) {
			this.CustomerID = customerID;
			this.AliMemberID = aliMemberID;
			Result = new AlibabaAvailableCreditResult();
		}

		public override void Execute() {

			//Console.WriteLine(this.CustomerID + "===" + this.AliMemberID);

			DB.FillFirst(
					Result,
					"AlibabaCustomerAvalableCredit",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.CustomerID),
					new QueryParameter("AliMemberId", this.AliMemberID)
				);

			//Console.WriteLine( JsonConvert.SerializeObject(Result));
		}

		public int CustomerID { get; private set; }
		public int AliMemberID { get; private set; }

		public AlibabaAvailableCreditResult Result { get; private set; }

	}
}