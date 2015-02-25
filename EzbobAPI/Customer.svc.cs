namespace EzbobAPI {
	using System;
	using System.IO;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using System.Text;
	using Newtonsoft.Json;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;

	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class Customer : ICustomer {

		private string GetAuthHeader(string userName, string password) {
			string userNamePassword = Convert.ToBase64String
				(new UTF8Encoding().GetBytes(string.Format("{0}:{1}", userName, password)));
			return string.Format("Basic {0}", userNamePassword);
		}

		public AvailableCreditActionResult GetCustomerAvailableCredit(string email) {
			ServiceClient client = new ServiceClient();
			var response = client.Instance.AvailableCredit(email);

			return response;
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


		public object PostData(string data) {
			var results = JsonConvert.DeserializeObject<dynamic>(data);
			var experience = results.Experience;
			var status = results.Status;
			var name = results.Name;
			var uuid = results.Uuid;
			var dynamic_property = results.AnotherProperty;
			return results;
		}

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