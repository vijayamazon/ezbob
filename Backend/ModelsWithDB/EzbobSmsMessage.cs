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
	}
}
