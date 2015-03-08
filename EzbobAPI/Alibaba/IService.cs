namespace EzbobAPI.Alibaba {
	using System.ComponentModel;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using EzbobAPI.DataObject;

	[ServiceContract]
	public interface IService {

		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "availableCredit", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		[Description("Returns customers' available creadit, right for now. Response includes last check date, credit data (and next available check date???)")]
		AlibabaCompositeType GetCustomerAvailableCredit(AlibabaCompositeType input);


		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "qualify", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		[Description("Activate customer's requalification process (limited to once in 30-day check)")]
		AlibabaCompositeType RequalifyCustomer(AlibabaCompositeType input);

	}
}