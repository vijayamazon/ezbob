namespace Ezbob.Backend.ModelsWithDB
{
	using Utils;
	using Twilio;
	using System;

	public class EzbobSmsMessage : Message
	{
		public static EzbobSmsMessage FromMessage(Message smsMessage) {
			var model = new EzbobSmsMessage();
			if (smsMessage != null) {
				smsMessage.Traverse((inst, propInfo) => propInfo.SetValue((Message) model, propInfo.GetValue(smsMessage)));

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

		public string GetRestException() {
			if (RestException != null) {
				return string.Format("RestException Code:{0}, Status:{3}, Message:{1}, MoreInfo:{2}", RestException.Code, RestException.Message, RestException.MoreInfo, RestException.Status);
			}
			return "";
		}
	}
}
