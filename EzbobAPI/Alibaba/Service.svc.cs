namespace EzbobAPI.Alibaba {
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.ServiceModel.Web;
	using EzbobAPI.DataObject;
	using ServiceClientProxy;

	public class Service : IService {

		/// <exception cref="WebFaultException">Condition. </exception>
		public AlibabaCompositeType GetCustomerAvailableCredit(AlibabaCompositeType input) {

			AlibabaCompositeType response = new AlibabaCompositeType {
				apiUrl = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.OriginalString,
				responseId = input.responseId,
				requestId = input.requestId
			};

			if (input.requestId == null) {
				response.errCode = AlibabaErrorCode.INCOMING_REQUEST_ID_NOT_VALID;
				response.errMsg = "INCOMING_REQUEST_ID_NOT_VALID";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			if (input.responseId == null) {
				response.errCode = AlibabaErrorCode.INCOMING_RESPONSE_ID_NOT_VALID;
				response.errMsg = "INCOMING_RESPONSE_ID_NOT_VALID";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			var customerID = 0;

			try {
				customerID = Convert.ToInt32(input.aId);
			} catch (Exception ex) {
				response.errCode = AlibabaErrorCode.INCOMING_CUSTOMER_ID_NOT_VALID;
				response.errMsg = "INCOMING_CUSTOMER_ID_NOT_VALID";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			Debug.WriteLine("customerID " + customerID);

			var aliMemberId = 0;

			try {
				aliMemberId = Convert.ToInt32(input.aliMemberId);
			} catch (Exception ex1) {
				response.errCode = AlibabaErrorCode.INCOMING_ALI_MEMBER_ID_NOT_VALID;
				response.errMsg = "INCOMING_ALI_MEMBER_ID_NOT_VALID";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			Debug.Write("\t  aliMemberId " + aliMemberId);

			ServiceClient client = new ServiceClient();
			var result= client.Instance.CustomerAvaliableCredit(customerID, aliMemberId).Result;

			Console.WriteLine(result);

			Debug.Write(result.ToString());

			// customerID and aliMemberID doesn't match each other in in system DB
			if (result.aId == null && result.aliMemberId == null) {

				Debug.Write("SYSTEM_CUSTOMER_ALI_MEMBER_ID_MISMATCH: {0}", result.aliMemberId.ToString());

				response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH;
				response.errMsg = "SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			// customerID not found in system DB
			if (result.aId == null) {

				Debug.Write("SYSTEM_CUSTOMER_ID_NOT_FOUND: {0}", result.aId.ToString());

				response.errCode = AlibabaErrorCode.SYSTEM_CUSTOMER_ID_NOT_FOUND;
				response.errMsg = "SYSTEM_CUSTOMER_ID_NOT_FOUND";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			// ali memberID not found in system DB
			if (result.aliMemberId == null) {

				Debug.Write("SYSTEM_ALI_MEMBER_ID_NOT_FOUND: {0}", result.ToString());

				response.errCode = AlibabaErrorCode.SYSTEM_ALI_MEMBER_ID_NOT_FOUND;
				response.errMsg = "SYSTEM_ALI_MEMBER_ID_NOT_FOUND";
				throw new WebFaultException<AlibabaCompositeType>(response, HttpStatusCode.BadRequest);
			}

			if (result.creditLine == null) {
				response.errCode = AlibabaErrorCode.SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER;
				response.errMsg = "SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER";
			}

			response.availableCredit = result;

			return response;
		}

		public AlibabaCompositeType RequalifyCustomer(AlibabaCompositeType input) {
			/*ServiceClient client = new ServiceClient();
			var result = client.Instance.RequalifyCustomer(email);			
			response.StringValue = result;*/
			return null;
		}
	}
}
