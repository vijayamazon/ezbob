namespace Ezbob.Backend.Strategies.Alibaba {
	using System;
	using System.Linq;
	using AlibabaLib;
	using ConfigManager;
	using Ezbob.Backend.Models.Alibaba;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Alibaba;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using RestSharp;
	using StructureMap;

	public class DataSharing : AStrategy {

		public DataSharing(int customerID, int finalDecision) {
			this.CustomerID = customerID;
			this.FinalDecision = finalDecision;
			Result = new CustomerDataSharing();
		}

		public override string Name { get { return "Alibaba Customer DataSharing"; } }

		public override void Execute() {

			var aliMemberRep = ObjectFactory.GetInstance<AlibabaBuyerRepository>();
			AlibabaBuyer aliMember = aliMemberRep.ByCustomer(CustomerID);
			
			Log.Info("ali member: {0}", aliMember.AliId);

			if (aliMember == null || aliMember.Customer.Id != CustomerID || aliMember.AliId == 0 ) {
				Log.Info("Alibaba member for customer {0} not exists", CustomerID);
				return;
			}

			Console.WriteLine("******************************************************{0}, {1}", CustomerID, FinalDecision);

			DB.FillFirst(
					Result,
					"AlibabaCustomerDataSharing",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.CustomerID),
					new QueryParameter("FinalDecision", this.@FinalDecision)
				);
			
			Log.Debug("DataSharing strategy, execute customerID: {0}, finalDecision: {1}, Result: {2}", CustomerID, FinalDecision, JsonConvert.SerializeObject(Result)); 

			if (Result == null || Result.aliMemberId == 0 || Result.aId == 0) {
				Log.Info("Relevant data for sharing with Alibaba (001/002) for customer {0}, aliId {1} not found", CustomerID, aliMember.AliId);
				return;
			}

			// send 001/002 
			try {

				AlibabaClient client;

				if (CurrentValues.Instance.AlibabaClientEnvironment != null && CurrentValues.Instance.AlibabaClientEnvironment.Value.Contains("Sandbox")) {
					client = new AlibabaClient(CurrentValues.Instance.AlibabaBaseUrl_Sandbox, CurrentValues.Instance.AlibabaUrlPath_Sandbox, CurrentValues.Instance.AlibabaAppSecret_Sandbox);
				} else {
					client = new AlibabaClient(CurrentValues.Instance.AlibabaBaseUrl, CurrentValues.Instance.AlibabaUrlPath, CurrentValues.Instance.AlibabaAppSecret);
				}

				IRestResponse response = client.SendDecision(JObject.FromObject(this.Result), this.FinalDecision);
				
				//Log.Info("response.Request {0}", JsonConvert.SerializeObject(response.Request.Parameters));
				//Log.Info("response.Content {0}", JsonConvert.SerializeObject(response.Content));
				//Log.Info(response.Request.Parameters.FirstOrDefault(p => p.Name == "_aop_signature").Value.ToString());

				var sentDataRep = ObjectFactory.GetInstance<AlibabaSentDataRepository>();

				AlibabaSentData sent = new AlibabaSentData();
				sent.AlibabaBuyer = aliMember;
				sent.Customer = sent.AlibabaBuyer.Customer;
				sent.Request = JsonConvert.SerializeObject(response.Request.Parameters);
				sent.Response = response.Content;
				var signature = response.Request.Parameters.FirstOrDefault(p => p.Name == "_aop_signature");
				if (signature != null) {
					sent.Signature = signature.Value.ToString();
				}
				sent.BizTypeCode = (FinalDecision == 0) ? BizType.APPLICATION.DescriptionAttr() : BizType.APPLICATION_REVIEW.DescriptionAttr();
				sent.SentDate = DateTime.UtcNow;
				sentDataRep.Save(sent);

			} catch (Exception e) {
				Console.WriteLine(e);
				throw new StrategyAlert(this, string.Format("Failed to transmit 001/002 for customer {0}", CustomerID), e);
			}
		}


		public int CustomerID { get; private set; }

		public int FinalDecision { get; private set; }

		public CustomerDataSharing Result { get; private set; }



	} //DataSharing
}