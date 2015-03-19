namespace Ezbob.API.AuthenticationAPI.Models {
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using Ezbob.Backend.Models.ExternalAPI;
	using Ezbob.Utils.Extensions;

	public enum AlibabaErrorCode {

		[Description("OK")]
		OK,

		[Description("INCOMING_REQUEST_ID_NOT_FOUND")]
		INCOMING_REQUEST_ID_NOT_FOUND,

		[Description("INCOMING_REQUEST_ID_NOT_VALID")]
		INCOMING_REQUEST_ID_NOT_VALID,

		[Description("INCOMING_RESPONSE_ID_NOT_VALID")]
		INCOMING_RESPONSE_ID_NOT_VALID,

		[Description("INCOMING_CUSTOMER_ID_NOT_VALID")]
		INCOMING_CUSTOMER_ID_NOT_VALID,

		[Description("INCOMING_ALI_MEMBER_ID_NOT_VALID")]
		INCOMING_ALI_MEMBER_ID_NOT_VALID,

		[Description("SYSTEM_CUSTOMER_ID_NOT_FOUND")]
		SYSTEM_CUSTOMER_ID_NOT_FOUND,

		[Description("SYSTEM_ALI_MEMBER_ID_NOT_FOUND")]
		SYSTEM_ALI_MEMBER_ID_NOT_FOUND,

		[Description("SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH")]
		SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH,

		[Description("SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER")]
		SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER,

		[Description("SYSTEM_CUSTOMER_ID_LOAN_NOT_FOUND")]
		SYSTEM_CUSTOMER_ID_LOAN_NOT_FOUND,

		[Description("REQUALIFY_CUSTOMER_PROCESS_STARTED")]
		REQUALIFY_STARTED,

		[Description("Unauthorized")]
		Unauthorized
	}

	public class AlibabaDto {

		[Required]
		public string requestId { get; set; }

		[Required]
		public string responseId { get; set; }

		[Required]
		[Range(1, Int32.MaxValue)]
		public int aliMemberId { get; set; }

		[Required(AllowEmptyStrings = false)]
		[Range(1, Int32.MaxValue)]
		public int aId { get; set; }

		[Required]
		[Range(Int32.MinValue, Int32.MaxValue)]
		public int loanId { get; set; }

		public AlibabaAvailableCreditResult availableCredit = null;

		[Required]
		public AlibabaErrorCode errCode = AlibabaErrorCode.OK;

		[Required]
		public string errMsg = AlibabaErrorCode.OK.DescriptionAttr();
	}
}
