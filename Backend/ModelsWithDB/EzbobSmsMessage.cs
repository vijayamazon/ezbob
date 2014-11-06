namespace Ezbob.Backend.ModelsWithDB
{
	using Utils;
	using Twilio;
	using System;

	public class EzbobSmsMessage : SMSMessage
	{
		public static EzbobSmsMessage FromSmsMessage(SMSMessage smsMessage) {
			var model = new EzbobSmsMessage();
			if (smsMessage != null) {
				smsMessage.Traverse((inst, propInfo) => propInfo.SetValue((SMSMessage) model, propInfo.GetValue(smsMessage)));

				if (smsMessage.DateSent == default(DateTime) || smsMessage.DateCreated == default(DateTime) || smsMessage.DateUpdated == default(DateTime))
				{
					var now = DateTime.UtcNow;
					model.DateSent = now;
					model.DateCreated = now;
					model.DateUpdated = now;
				}
			} else {
				model.Status = "null";
			}
			return model;
		}

		[NonTraversable]
		public int Id { get; set; }

		/// <summary>
		/// sms sent to CustomerId or BrokerId
		/// </summary>
		public int? UserId { get;set;}
		
		/// <summary>
		/// sms sent from UnderwriterId (if system sms then 1)
		/// </summary>
		public int UnderwriterId { get; set; }

		[NonTraversable]
		public RestException RestException { get; set; }

		[NonTraversable]
		public Uri Uri { get; set; }

		public override string ToString()
		{
			return string.Format("userid {0} uwId {1} dates {2} {3} {4} to {5} from {6} sid {7} status {8} body {9} direction {10} accountsid {11}", UserId, UnderwriterId, DateCreated, DateSent, DateUpdated, To, From, Sid, Status, Body, Direction, AccountSid);
		}
	}
}
