namespace Ezbob.Backend.Strategies.Alibaba {
	using System;
	using System.Linq;
	using System.Web;
	using AlibabaLib;
	using ConfigManager;
	using DbConstants;
	using Ezbob.Backend.Models.Alibaba;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Alibaba;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using NHibernate.SqlCommand;
	using RestSharp;
	using StructureMap;

	public class DataSharing : AStrategy {

		public JsonSerializerSettings jf = new JsonSerializerSettings { Formatting = Formatting.None, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
	
		public DataSharing(int customerID, AlibabaBusinessType businessType) {
			CustomerID = customerID;
			this.businessType = businessType;
			Result = new CustomerDataSharing();
			this.sentDataRep = ObjectFactory.GetInstance<AlibabaSentDataRepository>();
		}

		public override string Name { get { return "Alibaba Customer DataSharing"; } }

		public override void Execute() {

			// check this customer is Alibaba's member also
			var aliMemberRep = ObjectFactory.GetInstance<AlibabaBuyerRepository>();
			AlibabaBuyer aliMember = aliMemberRep.ByCustomer(CustomerID);
			if( aliMember == null) {
				Log.Alert("Alibaba customer {0} not found in AlibabaBuyer table", CustomerID);
				return;
			}

			Log.Info(string.Format("DATASHARING1*************{0}*******************", aliMember.AliId));

			if (aliMember.Customer.Id != CustomerID || aliMember.AliId == 0) {
				Log.Info("Alibaba member for customer {0} not exists OR customer business Type not supported in Alibaba (Entrepreneur|SoleTrader|None)", CustomerID);
				return;
			}

			// check if 001 exists before 002
			if (this.businessType == AlibabaBusinessType.APPLICATION_REVIEW) {

				var exists001 = this.sentDataRep.GetAll().FirstOrDefault(c => (c.AlibabaBuyer.Id == aliMember.Id && c.Customer.Id == CustomerID && c.BizTypeCode == AlibabaBusinessType.APPLICATION.DescriptionAttr()));
	
				// create 001 fisrt
				if (exists001 == null || exists001.Id == 0) {
					DB.FillFirst(
						Result,
						"AlibabaCustomerDataSharing",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerID", this.CustomerID),
						new QueryParameter("FinalDecision", 0)
					);
					
					SendRequest(aliMember, this.Result, AlibabaBusinessType.APPLICATION);
				}
			}
			Result = new CustomerDataSharing();
			DB.FillFirst(
					Result,
					"AlibabaCustomerDataSharing",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", this.CustomerID),
					new QueryParameter("FinalDecision", (this.businessType == AlibabaBusinessType.APPLICATION)?0:1)
				);

			Log.Debug("**********DATASHARING4 strategy, execute customerID: {0}, finalDecision: {1}, Result: {2}", CustomerID, this.businessType.DescriptionAttr(), JsonConvert.SerializeObject(Result, jf));

			if (Result == null || Result.aliMemberId == 0 || Result.aId == 0) {
				Log.Info("Relevant data for sharing with Alibaba (001/002) for customer {0}, aliId {1} not found", CustomerID, aliMember.AliId);
				return;
			}

			SendRequest(aliMember, this.Result, this.businessType);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="aliMember"></param>
		/// <param name="result"></param>
		/// <param name="bizType"></param>
		private void SendRequest(AlibabaBuyer aliMember, CustomerDataSharing result, AlibabaBusinessType bizType) {

			if (result==null)
				return;
			if(aliMember==null)
				return;
			if (result.aliMemberId == 0)
				return;
			if (result.aId == 0)
				return;

			try {

				AlibabaClient client;

				if (CurrentValues.Instance.AlibabaClientEnvironment != null && CurrentValues.Instance.AlibabaClientEnvironment.Value.Contains("Sandbox")) {
					client = new AlibabaClient(CurrentValues.Instance.AlibabaBaseUrl_Sandbox, CurrentValues.Instance.AlibabaUrlPath_Sandbox, CurrentValues.Instance.AlibabaAppSecret_Sandbox);
				} else {
					client = new AlibabaClient(CurrentValues.Instance.AlibabaBaseUrl, CurrentValues.Instance.AlibabaUrlPath, CurrentValues.Instance.AlibabaAppSecret);
				}
	
				IRestResponse response = client.SendDecision(JObject.FromObject(result), bizType);
	
				AlibabaSentData sent = new AlibabaSentData();
				sent.AlibabaBuyer = aliMember;
				sent.Customer = sent.AlibabaBuyer.Customer;
				sent.Request = JsonConvert.SerializeObject(response.Request.Parameters);
				sent.Response = response.Content;

				var jsonContent = JsonConvert.DeserializeObject(response.Content);
				var objContent = JObject.Parse(jsonContent.ToString());

				var errCode = objContent.Properties().FirstOrDefault(c => c.Name == "errCode");
				if (errCode != null) {
					sent.ErrorCode = errCode.Value.ToString();
				}

				var errMsg = objContent.Properties().FirstOrDefault(c => c.Name == "errMsg");
				if (errMsg != null) {
					sent.ErrorMessage = errMsg.Value.ToString();
				}

				var signature = response.Request.Parameters.FirstOrDefault(p => p.Name == "_aop_signature");
				if (signature != null) {
					sent.Signature = signature.Value.ToString();
				}

				var btype = objContent.Properties().FirstOrDefault(c => c.Name == "bizType");
				if (btype != null) {
					sent.BizTypeCode = btype.Value.ToString();
				}
				
				sent.StatusCode = response.StatusCode.DescriptionAttr();
				sent.SentDate = DateTime.UtcNow;
				this.sentDataRep.Save(sent);

			} catch (HttpException e) {
				throw new StrategyAlert(this, string.Format("HttpException: Failed to transmit for customer {0}, CustomerDataSharing: {1}, alimember:{2}", CustomerID, result.Stringify(), aliMember.AliId), e);
			} catch (Exception ex) {
				throw new StrategyAlert(this, string.Format("Failed to transmit for customer {0}, CustomerDataSharing: {1}, alimember:{2}", CustomerID, result.Stringify(), aliMember.AliId), ex);
			}
		}


		public int CustomerID { get; private set; }

		private AlibabaBusinessType businessType { get; set; }

		public CustomerDataSharing Result { get; private set; }

		private readonly AlibabaSentDataRepository sentDataRep;

	} //DataSharing
}