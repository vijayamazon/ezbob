namespace EzbobAPI {
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using System.Text;
	using EzbobAPI.DataObject;
	using Newtonsoft.Json;

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class Customer : ICustomer {

	//	[PrincipalPermission(SecurityAction.Demand, Role = "Alibaba")]
	//	[PrincipalPermission(SecurityAction.Demand, Role = "Alibaba")]
		public string GetData(string value) {
			Debug.WriteLine("value {0}, ", value);
			/*if (value == "666") {
				try {
					throw new FaultException<Evil666Error>(new Evil666Error() {
						Message = "Hey, this is 666."
					});
				} catch (FaultException<Evil666Error> faultException) {
					System.Diagnostics.Debug.WriteLine("faultException {0}, {1}", faultException.Data, faultException.Detail);
				}
			}*/
			return string.Format("You entered: {0}", value);
		}

		/*public CompositeType GetDataUsingDataContract(CompositeType composite) {
			if (composite == null) {
				try {
					throw new ArgumentNullException("composite");
				} catch (ArgumentNullException argumentNullException) {}
			}

			System.Diagnostics.Debug.WriteLine("composite.BoolValue {0}", composite);

			if (composite.BoolValue) {
				composite.StringValue += "Suffix";
			}
			return composite;
		}*/

		private string GetAuthHeader(string userName, string password) {
			string userNamePassword = Convert.ToBase64String
				(new UTF8Encoding().GetBytes(string.Format("{0}:{1}", userName, password)));
			return string.Format("Basic {0}", userNamePassword);
		}

		/*/// <exception cref="WebFaultException">Condition. </exception>
		public AlibabaCompositeType GetCustomerAvailableCredit(AlibabaCompositeType input) {

			
			AlibabaCompositeType response = new AlibabaCompositeType {
				requestId = input.requestId,
				responseId = input.responseId
			};


			int customerID = 0;

			// request: customerID not found
			try {
				
				customerID = Convert.ToInt32(input.aId);

				Debug.WriteLine("customerID " + customerID);

				
			/*	if (customerID.IsNull() || customerID == 0) {

					response.err = new ErrorData(AlibabaErrorCode.INCOMING_REQUEST_CUSTOMER_NOT_FOUND, "INCOMING_REQUEST_CUSTOMER_NOT_FOUND");
					//ErrorData errorData = new ErrorData(AlibabaErrorCode.INCOMING_REQUEST_CUSTOMER_NOT_FOUND, "INCOMING_REQUEST_CUSTOMER_NOT_FOUND");
					throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
				}#1#

			} catch (Exception EX_NAME) {

				Debug.WriteLine(EX_NAME);

				response.errCode = AlibabaErrorCode.INCOMING_REQUEST_CUSTOMER_NOT_FOUND;
				response.errMsg = "INCOMING_REQUEST_CUSTOMER_NOT_FOUND";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
		

			}


			int aliMemberId = Convert.ToInt32(input.aliMemberId);

Debug.Write("; aliMemberId " + aliMemberId);

			// request: alibabaID not found
			if (aliMemberId.IsNull() || aliMemberId == 0) {
				ErrorData errorData = new ErrorData(AlibabaErrorCode.INCOMING_REQUEST_ALI_MEMBER_ID_NOT_FOUND, "INCOMING_REQUEST_ALI_MEMBER_ID_NOT_FOUND");
				throw new WebFaultException<ErrorData>(errorData, HttpStatusCode.BadRequest);
			}
			
			ServiceClient client = new ServiceClient();

			var result= client.Instance.CustomerAvaliableCredit(customerID, aliMemberId).Result;
			
Debug.Write("result.Result: {0}", result.ToString());

			// customerID not found in system DB
			if (result.aId.IsNull()) {
				ErrorData errorData = new ErrorData(AlibabaErrorCode.SYSTEM_CUSTOMER_NOT_FOUND, "SYSTEM_CUSTOMER_NOT_FOUND");
				throw new WebFaultException<ErrorData>(errorData, HttpStatusCode.BadRequest);
			}

			// ali memberID not found in system DB
			if (result.aliMemberId.IsNull()) {
				ErrorData errorData = new ErrorData(AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND, "SYSTEM_ALI_MEMBER_ID_NOT_FOUND");
				throw new WebFaultException<ErrorData>(errorData, HttpStatusCode.BadRequest);
			}

			// customerID and aliMemberID doesn't mathc eacj other in in system DB
			if (result.aId.IsNull() && result.aliMemberId.IsNull()) {
				ErrorData errorData = new ErrorData(AlibabaErrorCode.SYSTEM_CUSTOMER_ALI_MEMBER_ID_MISMATCH, "SYSTEM_CUSTOMER_ALI_MEMBER_ID_MISMATCH");
				throw new WebFaultException<ErrorData>(errorData, HttpStatusCode.BadRequest);
			}

			return new AlibabaCompositeType {
				requestId = input.requestId, 
				responseId  = input.responseId, 
				availableCredit = result//,
			//	errCode = AlibabaErrorCode.NO_ERROR,
			//	errMsg = "NO_ERROR"
			//	err = new ErrorData(AlibabaErrorCode.NO_ERROR, "NO_ERROR")
				//apiUrl = this.
			};
		}*/

		public string TestAutorizationCustoManagerWithAllenConway(string param) {
			return "I'm here " + param;
		}

		public CompositeType RequalifyCustomer(string email) {
			CompositeType response = new CompositeType();
			Console.WriteLine(WebOperationContext.Current.IncomingRequest.ContentType.ToString());
			response.StringValue = WebOperationContext.Current.IncomingRequest.Headers["Authorization"].ToString();
			response.Email = email;
			/*ServiceClient client = new ServiceClient();
			var result = client.Instance.RequalifyCustomer(email);			
			response.StringValue = result;*/
			return response;
		}

		public Stream GetStream() {
			try {
				return File.OpenRead(@"c:\windows\notepad.exe");
			} catch (UnauthorizedAccessException unauthorizedAccessException) {
				Console.WriteLine(unauthorizedAccessException.Message);
			}
			return null;
		}

		public void GetDataUsingDataContractCompositeArg(string fileName, Stream fileContents) {
			byte[] buffer = new byte[10000];
			int bytesRead, totalBytesRead = 0;
			do {
				bytesRead = fileContents.Read(buffer, 0, buffer.Length);
				totalBytesRead += bytesRead;
			} while (bytesRead > 0);
			Console.WriteLine("Service: Received file {0} with {1} bytes", fileName, totalBytesRead);
		}

		public object PostData(string data) {
			var results = JsonConvert.DeserializeObject<dynamic>(data);
			var experience = results.Experience;
			var status = results.Status;
			var name = results.Name;
			var uuid = results.Uuid;
			var dynamic_property = results.AnotherProperty;
			return results;
		}

		/*public QualificationResponse QualifyCustomer(string id, QualificationRequest data) {
			try {
				Console.WriteLine("============>" + data.email);
				var response = new QualificationResponse() {
					Decision = String.Format("in the process (manual) for enail: {0}, id: {1}", data.email, id)
				};
				return response;
			} 			
			catch (Exception ex) {
				Console.WriteLine("===" + ex.Message);
				throw new FaultException<string>(ex.Message);
			}
		}*/

		/*public CompositeType QualifyCustomerComposite(CompositeType data) {
			if (data == null) {
				throw new ArgumentNullException("composite");
			}
			if (data.BoolValue) {
				data.StringValue += "Suffix";
			}
			return data;
		}*/

		/*public string GetData(string value) {
			return string.Format("You entered: {0}", value.ToString());
		}*/

		/*public string TestData(string value) {
			return string.Format("You entered: {0}", value.ToString());
		}*/

		/*public CompositeType GetDataUsingDataContract(CompositeType composite) {
			if (composite == null) {
				throw new ArgumentNullException("composite");
			}
			if (composite.BoolValue) {
				composite.StringValue += "Suffix";
			}
			return composite;
		}*/
	}
}
