namespace Ezbob.Backend.ModelsWithDB
{
	using Utils;
	using Twilio;
	using System;

	public class EzbobSmsMessage : SMSMessage
	{
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
