namespace Ezbob.Backend.Strategies.Alibaba {
	using System;
	using System.Linq;
	using AlibabaLib;
	using ConfigManager;
	using Ezbob.Backend.Models.Alibaba;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EzBob.eBayServiceLib.com.ebay.developer.soap;
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

			Console.WriteLine("******************************************************{0}, {1}", CustomerID, FinalDecision);

			DB.FillFirst(
					Result,
					"AlibabaCustomerDataSharing",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.CustomerID),
					new QueryParameter("FinalDecision", this.@FinalDecision)
				);

		//	Console.WriteLine(JsonConvert.SerializeObject(Result));
		//	Log.Debug("DataSharing strategy, execute customerID: {0}, finalDecision: {1}, Result: {2}", CustomerID, FinalDecision, JsonConvert.SerializeObject(Result)); 

			// send 001/002 to Alibaba
			if (Result != null && Result.aliMemberId != 0 && Result.aId != 0) {

				try {

					AlibabaClient client;

					if(CurrentValues.Instance.AlibabaClientEnvironment !=null && CurrentValues.Instance.AlibabaClientEnvironment.Value.Contains("Sandbox")){
						client = new AlibabaClient(CurrentValues.Instance.AlibabaBaseUrl_Sandbox, CurrentValues.Instance.AlibabaUrlPath_Sandbox, CurrentValues.Instance.AlibabaAppSecret_Sandbox);
					} else {
						client = new AlibabaClient(CurrentValues.Instance.AlibabaBaseUrl, CurrentValues.Instance.AlibabaUrlPath, CurrentValues.Instance.AlibabaAppSecret);
					}

					IRestResponse response = client.SendDecision(JObject.FromObject(this.Result), this.FinalDecision);

					var aliMemberRep = ObjectFactory.GetInstance<AlibabaBuyerRepository>();
					var sentDataRep = ObjectFactory.GetInstance<AlibabaSentDataRepository>();

					AlibabaSentData sent = new AlibabaSentData();
					
					sent.AlibabaBuyer = aliMemberRep.ByCustomer(Result.aId).FirstOrDefault();
					if (sent.AlibabaBuyer != null)
						sent.Customer = sent.AlibabaBuyer.Customer;
					sent.Request = JsonConvert.SerializeObject(response.Request.Parameters);
					sent.Response = response.Content;
					var signature = response.Request.Parameters.FirstOrDefault(p => p.Name == "signature");
					if (signature != null) {
						sent.Signature = signature.Value.ToString();
					}

					sent.BizTypeCode = (FinalDecision == 0) ? BizType.APPLICATION.DescriptionAttr() : BizType.APPLICATION_REVIEW.DescriptionAttr();
					sent.SentDate = DateTime.UtcNow;
					sentDataRep.Save(sent);

				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}
		

		public int CustomerID { get; private set; }

		public int FinalDecision { get; private set; }

		public CustomerDataSharing Result { get; private set; }



	} //DataSharing
}