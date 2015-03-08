namespace EzbobAPI {
	using System.ComponentModel;
	using System.Runtime.Serialization;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using EzbobAPI.DataObject;

	[ServiceContract(Namespace = "https://localhost:12498/2015/03")]
	public interface ICustomer {

		/*[OperationContract]
		[WebGet(UriTemplate = "availableCredit/{email}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		[Description("Returns customers' available creadit, right for now. Response includes last check date, credit data (and next available check date???)")]
		AlibabaCompositeType GetCustomerAvailableCredit(string email);*/
		
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "qualify/{aId}/{aliMemberId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		[Description("Activate customer's requalification process (limited to once in 30-day check)")]
		//CompositeType RequalifyCustomer(string aId, string aliMemberId);
		CompositeType RequalifyCustomer(string email);

		[OperationContract]
		[WebInvoke(Method = "GET", UriTemplate = "getdata/{value}")]
		[FaultContract(typeof(Evil666Error))]
		string GetData(string value);

		/*[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "getdatacimptype")]
		CompositeType GetDataUsingDataContract(CompositeType composite);*/

		/** REST client usage:  POST to url: http://localhost:12498/Customer.svc/qualify	
			HEADERS	( MUST ):	 
			Accept: application/json
			Content-Type: application/json; charset=UTF-8
			--------------------
			Request Body: 	{ "Email"  :  "asdfas@com",  "BoolValue": true, "StringValue" :  "adfasfdasf" }			 
		***/
		/*[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "qualify", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		[Description("Activate customer's requalification process (limited to once in 30-day check)")]
		CompositeType QualifyCustomer(CompositeType data);*/

		/*[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetData/{value}")]
		string GetData(string value);

		[OperationContract]
		[WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "test/{value}")]
		string TestData(string value);

	/*	[OperationContract]
		[WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "testComposite/{composite}")]
		CompositeType GetDataUsingDataContract(CompositeType composite);#1#


		/*[OperationContract]
		[WebInvoke(Method = "POST",  UriTemplate = "testComposteArg/{fileName}")]
		void GetDataUsingDataContractCompositeArg(string fileName, Stream fileContents);#1#

		/*[WebGet]
		Stream GetImage(int width, int height);
		 
		[OperationContract]
		[WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "GetProductList/")]
		List<Product> GetProductList();

		[OperationContract]
		[WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "notepad")]
		Stream GetStream();

		// ne budet rabotat' strings only
		[WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "?x={x}&y={y}")]
		//[WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "add/{x}/{y}")]
		int Add(int x, int y);*/
	}

	[DataContract]
	public class Evil666Error {
		[DataMember]
		public string Message { get; set; }
	}


}