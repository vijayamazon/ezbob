namespace Ezbob.Backend.Strategies.MailStrategies.Broker {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.UserManagement.EmailConfirmation;

	public class BrokerCommissionInvoice : ABrokerMailToo {
		public BrokerCommissionInvoice(BrokerInvoiceCommissionModel model) : base(model.BrokerID, true) {
			this.model = model;
		} // constructor

		public override string Name { get { return "Broker invoice"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Broker Invoice";

			var ecg = new EmailConfirmationGenerate(BrokerData.UserID);
			ecg.Execute();

			Variables = new Dictionary<string, string> {
				{ "BrokerCompanyName", BrokerData.FirmName },
				{ "BrokerContactName", BrokerData.FullName },
				{ "Phone", BrokerData.MobilePhone },
				{ "Email", BrokerData.Email},
				{ "Date", this.model.CommissionTime.ToString("dd/MM/yyyy") },
				{ "CustomerName", this.model.CustomerName },
				{ "CommissionAmount", FormattingUtils.NumericFormats(this.model.CommissionAmount) },
				{ "BankAccount", this.model.BankAccount },
				{ "Sortcode", this.model.SortCode },
				{ "Invoice", string.Format("EZBobLTD{0:yyyy}/{0:MM}/{0:dd}/{1}", this.model.CommissionTime, this.model.InvoiceID ) }
			};
		} // SetTemplateAndVariables

		protected virtual int BrokerID {
			get { return CustomerId; } // get
			set { CustomerId = value; } // set
		} // BrokerID

		protected virtual BrokerData BrokerData {
			get { return (BrokerData)CustomerData; } // get
			set { CustomerData = value; } // set
		} // BrokerData

		protected override void LoadRecipientData() {
			Log.Debug("Loading broker data...");

			BrokerData = new BrokerData(this, BrokerID, DB);
			BrokerData.Load();

			Log.Debug("Loading broker data complete.");
		} // LoadRecipientData

		protected override Addressee[] GetRecipients() {
			var aryAddresses = new List<Addressee>();

			if (!string.IsNullOrWhiteSpace(BrokerData.Email)) {
				aryAddresses.Add(new Addressee(BrokerData.Email, userID: BrokerData.BrokerID, isBroker: true, origin: BrokerData.Origin, addSalesforceActivity: false));
			}

			if (!string.IsNullOrWhiteSpace(ConfigManager.CurrentValues.Instance.AccountingBrokersEmail)) {
				aryAddresses.Add(new Addressee(ConfigManager.CurrentValues.Instance.AccountingBrokersEmail, userID: BrokerData.BrokerID, isBroker: true, origin: BrokerData.Origin, addSalesforceActivity: false));
			}

			return aryAddresses.ToArray();
		} // GetRecipients

		private readonly BrokerInvoiceCommissionModel model;
	} // class BrokerCommissionInvoice
} // namespace
