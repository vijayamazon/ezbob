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
		public GetIncomeSms(DateTime? date) {
			this.date = date;
			this.fromNumberUK = CurrentValues.Instance.TwilioSendingNumber;
			this.twilioClient = new TwilioRestClient(CurrentValues.Instance.TwilioAccountSid, CurrentValues.Instance.TwilioAuthToken);

			this.crmActionsRepository = ObjectFactory.GetInstance<CRMActionsRepository>();
			this.crmStatusesRepository = ObjectFactory.GetInstance<CRMStatusesRepository>();
			this.customerRelationsRepository = ObjectFactory.GetInstance<CustomerRelationsRepository>();
			this.customerRelationStateRepository = ObjectFactory.GetInstance<CustomerRelationStateRepository>();
		}

		public override string Name { get { return "Get Income SMS"; } }

        public override void Execute() {
			int page = 0, numOfPages = 0;
			do {
				var result = this.twilioClient.ListSmsMessages(this.fromNumberUK, null, this.date, page, 50);
				numOfPages = result.NumPages;

				foreach (var msg in result.SMSMessages) {
					HandleOneSms(msg);
				}
				page++;
			} while (numOfPages > page);
        }//Execute

        private void HandleOneSms(SMSMessage msg) {
			var senderID = FindSender(msg.From);

			var message = EzbobSmsMessage.FromMessage(msg);
			message.UserId = senderID;

			SaveToDb(message);
			AddCrm(message);
			AddSalesForceActivity(message);
        }//HandleOneSms

		private void AddCrm(EzbobSmsMessage message) {
			if(!message.UserId.HasValue) return;

			var actionItem = this.crmActionsRepository.GetAll().FirstOrDefault(x => x.Name == "SMS");
			var statusItem = this.crmStatusesRepository.GetAll().FirstOrDefault(x => x.Name == "Note for underwriting");

			var newEntry = new CustomerRelations {
				CustomerId = message.UserId.Value,
				UserName = "System",
				Type = "In",
				Action = actionItem,
				Status = statusItem,
				Comment = message.Body,
				Timestamp = message.DateSent,
				IsBroker = false,
				PhoneNumber = message.To
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

		private readonly DateTime? date;
		private readonly TwilioRestClient twilioClient;
		private readonly string fromNumberUK;
		private readonly CRMActionsRepository crmActionsRepository;
		private readonly CRMStatusesRepository crmStatusesRepository;
		private readonly CustomerRelationsRepository customerRelationsRepository;
		private readonly CustomerRelationStateRepository customerRelationStateRepository;
		private const string UkMobilePrefix = "+44";
	}//class
}//ns
