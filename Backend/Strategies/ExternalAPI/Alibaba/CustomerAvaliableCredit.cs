
namespace Ezbob.Backend.Strategies.ExternalAPI.Alibaba {
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Database;

	public class CustomerAvaliableCredit : AStrategy {
		public override string Name {
			get { return "Alibaba CustomerAvaliableCredit"; }
		}

		public CustomerAvaliableCredit(string customerRefNum, long aliMemberID) {
			CustomerRefNum = customerRefNum;
			AliMemberID = aliMemberID;
			Result = new AlibabaAvailableCreditResult();
		}

		public override void Execute() {
			DB.FillFirst(
					Result,
					"AlibabaCustomerAvalableCredit",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerRefNum", this.CustomerRefNum),
					new QueryParameter("AliMemberId", this.AliMemberID)
				);
		}

        public string CustomerRefNum { get; private set; }
		public long AliMemberID { get; private set; }
		public AlibabaAvailableCreditResult Result { get; private set; }
	}
}