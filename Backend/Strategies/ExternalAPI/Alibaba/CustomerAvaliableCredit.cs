
namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba {
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Database;

	public class CustomerAvaliableCredit : AStrategy {
		public override string Name {
			get { return "Alibaba CustomerAvaliableCredit"; }
		}

		public CustomerAvaliableCredit(int customerID, long aliMemberID) {
			CustomerID = customerID;
			AliMemberID = aliMemberID;
			Result = new AlibabaAvailableCreditResult();
		}

		public override void Execute() {
			DB.FillFirst(
					Result,
					"AlibabaCustomerAvalableCredit",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.CustomerID),
					new QueryParameter("AliMemberId", this.AliMemberID)
				);
		}

		public int CustomerID { get; private set; }
		public long AliMemberID { get; private set; }
		public AlibabaAvailableCreditResult Result { get; private set; }
	}
}