using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database {
	public class RegisteredCustomer {
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual bool IsSuccessfullyRegistered { get; set; }

		public virtual string MPStatus { get; set; }

		public virtual DateTime? GreetingMailSentDate { get; set; }

		public virtual string EbayStatus { get; set; }
		public virtual string AmazonStatus { get; set; }
		public virtual string PayPalStatus { get; set; }
		public virtual string EkmStatus { get; set; }

		public virtual WizardStepType WizardStep { get; set; }
	} // class RegisteredCustomer

	public class RegisteredCustomerMap : ClassMap<RegisteredCustomer> {
		public RegisteredCustomerMap() {
			Table("RegisteredCustomers");
			ReadOnly();
			Cache.ReadOnly().Region("LongTerm").ReadOnly();
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.IsSuccessfullyRegistered);
			Map(x => x.MPStatus);
			Map(x => x.GreetingMailSentDate);
			Map(x => x.EbayStatus);
			Map(x => x.AmazonStatus);
			Map(x => x.PayPalStatus);
			Map(x => x.EkmStatus);
			Map(x => x.WizardStep).CustomType(typeof(WizardStepType));
		} // constructor
	} // class RegisteredCustomerMap
} // namespace
