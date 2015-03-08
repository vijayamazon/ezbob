namespace EzbobAPI.DataObject
{
	using System.Runtime.Serialization;
	using Ezbob.Backend.Models.ExternalAPI;
	using Newtonsoft.Json;

	public enum AlibabaErrorCode {

		NO_ERROR,

		INCOMING_REQUEST_ID_NOT_VALID,
		INCOMING_RESPONSE_ID_NOT_VALID,

		INCOMING_CUSTOMER_ID_NOT_VALID,
		INCOMING_ALI_MEMBER_ID_NOT_VALID,

		SYSTEM_CUSTOMER_ID_NOT_FOUND,
		SYSTEM_ALI_MEMBER_ID_NOT_FOUND,
		SYSTEM_CUSTOMER_ID_ALI_MEMBER_ID_MISMATCH,

		SYSTEM_NO_VALID_CREDITLINE_FOR_CUSTOMER,

		SYSTEM_CUSTOMER_ID_LOAN_NOT_FOUND
	}

	[DataContract]
	public class AlibabaCompositeType {

		[DataMember(IsRequired = true)] // For idempotence
		public string requestId { get; set; }

		[DataMember(IsRequired = true)] // For idempotence
		public string responseId { get; set; }

		[DataMember(EmitDefaultValue = false)] // For idempotence
		public string apiUrl { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string aliMemberId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string aId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public string loanId { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public AlibabaAvailableCreditResult availableCredit = null;

		[DataMember(EmitDefaultValue = true, IsRequired = true)]
		public AlibabaErrorCode errCode = AlibabaErrorCode.NO_ERROR;

		[DataMember(EmitDefaultValue = true, IsRequired = true)]
		public string errMsg = "NO_ERROR";
	}	

	public class DataObject {
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

	[DataContract]
	public class CompositeType {
		private bool boolValue = true;
		private string stringValue = "Hello ";

		private string _email;
		private int _customerID;
		private decimal _amount;
		private string _decision;

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		[DataMember]
		public bool BoolValue {
			get { return this.boolValue; }
			set { this.boolValue = value; }
		}

		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string StringValue {
			get { return this.stringValue; }
			set { this.stringValue = value; }
		}

		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Email {
			get { return this._email; }
			set { this._email = value; }
		}

		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public int CustomerID {
			get { return this._customerID; }
			set { this._customerID = value; }
		}

		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public decimal Amount {
			get { return this._amount; }
			set { this._amount = value; }
		}

		[DataMember]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Decision {
			get { return this._decision; }
			set { this._decision = value; }
		}
	}


	[DataContract]
	public class ErrorData {
		public ErrorData(AlibabaErrorCode code, string msg) {
			errCode = code;
			errMsg = msg;
		}

		[DataMember]
		public AlibabaErrorCode errCode { get; set; }

		[DataMember]
		public string errMsg { get; set; }
	}
}
