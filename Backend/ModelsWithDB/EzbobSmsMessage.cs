namespace Ezbob.Backend.ModelsWithDB
{
	using Twilio;
	using System;

	public class EzbobSmsMessage
	{
		public static EzbobSmsMessage FromMessage(Message smsMessage) {
			DateTime now = DateTime.UtcNow;
			if (smsMessage == null) {
				
				return new EzbobSmsMessage {
					Status = "null",
					DateCreated = now,
					DateSent = now,
					DateUpdated = now
				};
			}

			var model = new EzbobSmsMessage{
				AccountSid = smsMessage.AccountSid,
				Sid = smsMessage.Sid,
				DateCreated = smsMessage.DateCreated == default(DateTime) ? now : smsMessage.DateCreated,
				DateSent = smsMessage.DateSent == default(DateTime) ? now : smsMessage.DateSent,
				DateUpdated = smsMessage.DateUpdated  == default(DateTime) ? now : smsMessage.DateUpdated,
				From = smsMessage.From,
				To = smsMessage.To,
				Body = smsMessage.Body == null ? "" : (smsMessage.Body.Length > 255 ? smsMessage.Body.Substring(0, 255) : smsMessage.Body),
				Status = smsMessage.Status,
				Direction = smsMessage.Direction,
				Price = smsMessage.Price,
				ApiVersion = smsMessage.ApiVersion,
			};
			
			return model;
		}

		/// <summary>
		/// sms sent to CustomerId or BrokerId
		/// </summary>
		public int? UserId { get;set;}
		
		/// <summary>
		/// sms sent from UnderwriterId (if system sms then 1)
		/// </summary>
		public int UnderwriterId { get; set; }

		/// <summary>
		/// A 34 character string that uniquely identifies this resource.
		/// 
		/// </summary>
		public string Sid { get; set; }

		/// <summary>
		/// The date that this resource was created
		/// 
		/// </summary>
		public DateTime DateCreated { get; set; }

		/// <summary>
		/// The date that this resource was last updated
		/// 
		/// </summary>
		public DateTime DateUpdated { get; set; }

		/// <summary>
		/// The date that the Message was sent
		/// 
		/// </summary>
		public DateTime DateSent { get; set; }

		/// <summary>
		/// The unique id of the Account that sent this Message.
		/// 
		/// </summary>
		public string AccountSid { get; set; }

		/// <summary>
		/// The phone number that initiated the message in E.164 format. For
		///             incoming messages, this will be the remote phone. For outgoing messages,
		///             this will be one of your Twilio phone numbers.
		/// 
		/// </summary>
		public string From { get; set; }

		/// <summary>
		/// The phone number that received the message in E.164 format. For
		///             incoming messages, this will be one of your Twilio phone numbers.
		///             For outgoing messages, this will be the remote phone.
		/// 
		/// </summary>
		public string To { get; set; }

		/// <summary>
		/// The text body of the Message.
		/// 
		/// </summary>
		public string Body { get; set; }

		

		/// <summary>
		/// The status of this Message. Either queued, sending, sent, or failed.
		/// 
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// The direction of this Message. incoming for incoming messages,
		///             outbound-api for messages initiated via the REST API, outbound-call
		///             for messages initiated during a call or outbound-reply for messages
		///             initiated in response to an incoming Message.
		/// 
		/// </summary>
		public string Direction { get; set; }

		/// <summary>
		/// The amount billed for the Message.
		/// 
		/// </summary>
		public Decimal Price { get; set; }

		/// <summary>
		/// The version of the Twilio API used to process the Message.
		/// 
		/// </summary>
		public string ApiVersion { get; set; }
		
		public override string ToString()
		{
			return string.Format("userid {0} uwId {1} dates {2} {3} {4} to {5} from {6} sid {7} status {8} body {9} direction {10} accountsid {11}",
				UserId, UnderwriterId, DateCreated, DateSent, DateUpdated, To, From, Sid, Status, Body, Direction, AccountSid);
		}
	}
}
