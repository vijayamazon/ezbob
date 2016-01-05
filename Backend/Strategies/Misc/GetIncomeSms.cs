namespace Ezbob.Backend.Strategies.Misc {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConfigManager;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.Strategies.MailStrategies;
    using Ezbob.Backend.Strategies.SalesForce;
    using Ezbob.Database;
    using EZBob.DatabaseLib.Model.CustomerRelations;
    using LandRegistryLib;
    using SalesForceLib.Models;
    using StructureMap;
    using Twilio;

	public class GetIncomeSms : AStrategy {
			
		public GetIncomeSms(DateTime? date, bool isYesterday) {
			if (date == null && isYesterday) {
				date = DateTime.Today.AddDays(-1);
			}
			this.sentDate = date;

			this.fromNumberUK = CurrentValues.Instance.TwilioSendingNumber;
			this.twilioClient = new TwilioRestClient(CurrentValues.Instance.TwilioAccountSid, CurrentValues.Instance.TwilioAuthToken);

			var crmActionsRepository = ObjectFactory.GetInstance<CRMActionsRepository>();
			var crmStatusesRepository = ObjectFactory.GetInstance<CRMStatusesRepository>();
			this.customerRelationsRepository = ObjectFactory.GetInstance<CustomerRelationsRepository>();
			this.customerRelationStateRepository = ObjectFactory.GetInstance<CustomerRelationStateRepository>();

			this.smsActionItem = crmActionsRepository.GetAll().FirstOrDefault(x => x.Name == "SMS");
			this.noteStatusItem = crmStatusesRepository.GetAll().FirstOrDefault(x => x.Name == "Note for underwriting");
		}

		public override string Name { get { return "Get Income SMS"; } }

        public override void Execute() {
			int page = 0, numOfPages = 0;
			do {
				var result = this.twilioClient.ListSmsMessages(this.fromNumberUK, null, this.sentDate, page, 50);
				numOfPages = result.NumPages;
				if(result.SMSMessages == null) return;
				foreach (var msg in result.SMSMessages) {
					HandleOneSms(msg);
				}
				page++;
			} while (numOfPages > page);
        }//Execute

        private void HandleOneSms(SMSMessage msg) {
	        try {
		        var senderID = FindSender(msg.From);
		        Log.Info("Retrieved income SMS from user: {0} {1} date: {2} body: {3}", senderID, msg.From, msg.DateSent, msg.Body);
		        var message = EzbobSmsMessage.FromMessage(msg);
		        message.UserId = senderID;
		        message.UnderwriterId = 1;
		        SaveToDb(message);
		        AddCrm(message);
		        AddSalesForceActivity(message);
			} catch (Exception ex) {
				msg = msg ?? new SMSMessage();
				Log.Error(ex, "Failed to retrieve sms {0} {1} {2}", msg.Sid, msg.From, msg.Body);
			}
        }//HandleOneSms

		private void AddCrm(EzbobSmsMessage message) {
			if(!message.UserId.HasValue) return;

			
			var newEntry = new CustomerRelations {
				CustomerId = message.UserId.Value,
				UserName = "System",
				Type = "In",
				Action = this.smsActionItem,
				Status = this.noteStatusItem,
				Comment = message.Body,
				Timestamp = message.DateSent,
				IsBroker = false,
				PhoneNumber = message.From
			};

			this.customerRelationsRepository.SaveOrUpdate(newEntry);
			this.customerRelationStateRepository.SaveUpdateState(message.UserId.Value, false, null, newEntry);
		}

		private void AddSalesForceActivity(EzbobSmsMessage message) {
			if (!message.UserId.HasValue) return;

			var customerData = new CustomerData(this, message.UserId.Value, DB);
			customerData.Load();

			var addActivityStrategy = new AddActivity(message.UserId, new ActivityModel {
				Description = string.Format("Received SMS: {0}", message.Body),
				Email = customerData.Mail,
				Origin = customerData.Origin,
				StartDate = DateTime.UtcNow,
				EndDate = DateTime.UtcNow,
				IsOpportunity = false,
				Originator = "System",
				Type = ActivityType.Sms.DescriptionAttr(),
			});

			addActivityStrategy.Execute();
		}

		private int? FindSender(string from) {
			if(string.IsNullOrEmpty(from)) {
				return null;
			}

			var fromWithoutPrefix = from.Replace(UkMobilePrefix, "0");
			var senderID = DB.ExecuteScalar<int?>("GetSmsSender", 
				CommandSpecies.StoredProcedure, 
				new QueryParameter("PhoneWithPrefix", from), 
				new QueryParameter("Phone", fromWithoutPrefix));

			return senderID;
		}//FindSender

		private void SaveToDb(EzbobSmsMessage message) {
			DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure,
								DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));
		}//SaveToDb

		private readonly DateTime? sentDate;
		private readonly TwilioRestClient twilioClient;
		private readonly string fromNumberUK;
		private readonly CustomerRelationsRepository customerRelationsRepository;
		private readonly CustomerRelationStateRepository customerRelationStateRepository;
		private readonly CRMActions smsActionItem;
		private readonly CRMStatuses noteStatusItem;
		private const string UkMobilePrefix = "+44";
	}//class
}//ns
