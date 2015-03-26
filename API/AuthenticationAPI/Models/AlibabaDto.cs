namespace Ezbob.API.AuthenticationAPI.Models {
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Utils.Extensions;
	using Newtonsoft.Json;


	public enum AlibabaErrorCode {

		[Description("OK")]
		OK,

		[Description("Incoming request: requestId not found")]
		INCOMING_REQUEST_ID_NOT_FOUND,

		[Description("Incoming request: requestId not valid")]
		INCOMING_REQUEST_ID_NOT_VALID,

		[Description("Incoming request: responseId not valid")]
		INCOMING_RESPONSE_ID_NOT_VALID,

		[Description("Incoming request: aId not valid")]
		INCOMING_CUSTOMER_ID_NOT_VALID,

		[Description("Incoming request: aliMemberId not valid")]
		INCOMING_ALI_MEMBER_ID_NOT_VALID,

		[Description("System: customerID (aId) not found")]
		SYSTEM_CUSTOMER_ID_NOT_FOUND,

		[Description("System: aliMemberId not found")]
		SYSTEM_ALI_MEMBER_ID_NOT_FOUND,

		[Description("System: aId and aliMemberId mismathching")]
		SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH,

		[Description("System: valid credit line for aId not found")]
		SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER ,

		[Description("System: loanId not found")]
		SYSTEM_CUSTOMER_ID_LOAN_NOT_FOUND,

		[Description("Customer's re-qualifycation process started")]
		REQUALIFY_STARTED,

		[Description("Unauthorized")]
		Unauthorized,

		[Description("Internal error ocuured")]
		INTERNAL_ERROR
	}

	public class AlibabaDto {

		[Required]
		public string requestId { get; set; }

		[Required]
		public string responseId { get; set; }

		[Required]
		[Range(1, Int64.MaxValue, ErrorMessage = "AliMemberId is invalid")]
		public decimal aliMemberId { get; set; }

		[Required(AllowEmptyStrings = false)]
		[Range(1, Int32.MaxValue, ErrorMessage = "ezbob customer ID is invalid")]
		public int aId { get; set; }

		[Required]
		[Range(Int32.MinValue, Int32.MaxValue, ErrorMessage = "loanId is invalid")]
		public int loanId { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public AlibabaAvailableCreditResult availableCredit = null;

		[Required]
		public AlibabaErrorCode errCode = AlibabaErrorCode.OK;

		[Required]
		public string errMsg = AlibabaErrorCode.OK.DescriptionAttr();

		public string url { get; set; }
	}
}
