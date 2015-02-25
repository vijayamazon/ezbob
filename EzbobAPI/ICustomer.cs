
namespace EzbobAPI {
	using System.ComponentModel;
	using System.IO;
	using System.Runtime.Serialization;
	using System.ServiceModel;
	using System.ServiceModel.Web;
	using Newtonsoft.Json;

	[ServiceContract(Namespace = "https://localhost:12498/2015/02")]
	public interface ICustomer {

		

		[OperationContract]
		//[Authorization() ]
		[WebGet(UriTemplate = "availableCredit/{email}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		[Description("Returns customers' available creadit, right for now. Response includes last check date, credit data (and next available check date???)")]
		ServiceClientProxy.EzServiceReference.AvailableCreditActionResult GetCustomerAvailableCredit(string email);


		/** REST client usage:  POST to url: http://localhost:12498/Customer.svc/qualify	
			HEADERS	( MUST ):	 
			Accept: application/json
			Content-Type: application/json; charset=UTF-8
			--------------------
			Request Body: 	{ "Email"  :  "asdfas@com",  "BoolValue": true, "StringValue" :  "adfasfdasf" }			 
		***/
		[OperationContract]
		[WebInvoke(Method = "POST", UriTemplate = "qualify", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
		[Description("Activate customer's requalification process (limited to once in 30-day check)")]
		CompositeType RequalifyCustomer(string email);

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
	public class CompositeType {
		bool boolValue = true;
		string stringValue = "Hello ";

		string _email;
		private int _customerID;
		private decimal _amount;
		private string _decision;
		//private AvailableCreditActionResult _availableCreditResult;

		[DataMember]
		public bool BoolValue {
			get { return boolValue; }
			set { boolValue = value; }
		}

		[DataMember]
		public string StringValue {
			get { return stringValue; }
			set { stringValue = value; }
		}

		[DataMember]
		public string Email {
			get { return this._email; }
			set { this._email = value; }
		}

		[DataMember]
		public int CustomerID {
			get { return this._customerID; }
			set { this._customerID = value; }
		}

		[DataMember]
		public decimal Amount {
			get { return this._amount; }
			set { this._amount = value; }
		}

		[DataMember]
		public string Decision {
			get { return this._decision; }
			set { this._decision = value; }
		}
	}

	public class MyModel {
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int experience { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool status { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string name { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string uuid { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public object property_1 { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public object property_2 { get; set; }
	}

}
